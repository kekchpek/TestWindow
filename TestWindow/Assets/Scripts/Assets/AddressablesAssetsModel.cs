using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Diagnostics.Time;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AI;

namespace AssetsSystem
{
    public class AddressablesAssetsModel : IAssetsModel
    {

        private const int CacheSize = 1000;
        private const int MaxConcurrentDownloads = 1;
        
        private readonly Dictionary<string, object> _cache = new(CacheSize);
        private readonly SemaphoreSlim _downloadSemaphore = new(MaxConcurrentDownloads, MaxConcurrentDownloads);
        private readonly Task _initTask;
        private readonly Dictionary<string, object> _refLoaded = new();
        private readonly Dictionary<string, (object asset, AsyncOperationHandle handle, int referenceCount)> _referenceCountedAssets = new();

        private readonly Dictionary<string, AsyncOperationHandle> _downloadingBundlesHandle = new();
        private readonly Dictionary<string, int> _downloadingBundlesCount = new();
        private readonly Dictionary<string, Task> _downloadingBundlesTasks = new();

        public AddressablesAssetsModel()
        {
            _initTask = Application.isPlaying 
                ? Initialize() 
                : Task.CompletedTask;
        }

        private async Task Initialize()
        {
            using (TimeDebug.StartMeasure("Addressables initialization"))
            {
                await Addressables.InitializeAsync().Task;
            }
        }

        public async UniTask FetchRemoteAssetsData()
        {
            using (TimeDebug.StartMeasure("Addressables fetch await init task"))
            {
                await _initTask;
            }

            using (TimeDebug.StartMeasure("Addressables refreshing catalogs"))
            {
                await RefreshCatalogs();
            }
        }

        public async Task DownloadAsset(string path, CancellationToken ct)
        {
            await _initTask;
            var bundleName = await GetBundle(path);
            if (ct.IsCancellationRequested)
            {
                return;
            }
            if (bundleName == null) 
            {
                throw new InvalidOperationException($"Can not locate bundle for {path}");
            }

            
            ct.RegisterWithoutCaptureExecutionContext(() => 
            {
                if (_downloadingBundlesCount.ContainsKey(bundleName))
                {
                    if (--_downloadingBundlesCount[bundleName] == 0) 
                    {
                        _downloadingBundlesCount.Remove(bundleName);
                        _downloadingBundlesTasks.Remove(bundleName);
                        if (_downloadingBundlesHandle.Remove(bundleName, out var handle))
                        {
                            Addressables.Release(handle);
                        }
                    }
                }
            });

            if (_downloadingBundlesTasks.TryGetValue(bundleName, out var downloadingTask)) {
                _downloadingBundlesCount[bundleName]++;
                await downloadingTask;
            }
            else 
            {
                var task = DownloadBundleForAsset(path, bundleName, ct);
                _downloadingBundlesTasks.Add(bundleName, task);
                _downloadingBundlesCount.Add(bundleName, 0);
                await task;
                _downloadingBundlesCount.Remove(bundleName);
                _downloadingBundlesTasks.Remove(bundleName);
                _downloadingBundlesHandle.Remove(bundleName);
            }
        }

        private async Task DownloadBundleForAsset(string path, string bundleName, CancellationToken ct) 
        {
            Debug.Log($"Downloading bundle for asset {path}");

            long bundleSize;
            using (TimeDebug.StartMeasure($"Get download size {path}"))
            {
                bundleSize = await Addressables.GetDownloadSizeAsync(path).Task;
            }
            
            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (bundleSize >= 0)
            {
                Debug.Log($"Download bundle size = {bundleSize} bytes");
                var op = Addressables.DownloadDependenciesAsync(path);
                _downloadingBundlesHandle.Add(bundleName, op);
                try
                {
                    await op.Task;
                    if (op.OperationException != null)
                        throw op.OperationException;
                    Debug.Log($"Group {path} downloaded!");
                }
                finally
                {
                    Addressables.Release(op);
                }
            }
            else
            {
                Debug.Log($"Group {path} already downloaded. No need to download.");
            }
        }

        private async UniTask<string> GetBundle(string path) {
            var location =  await Addressables.LoadResourceLocationsAsync(path).Task;
            if (location.Count < 1)
            {
                Debug.LogError($"No locations for {path}");
                return null;
            }
            if (location[0].Dependencies.Count != 1) {
                Debug.LogError($"No dependencies for {path}");
                return null;
            }
            return location[0].Dependencies[0].InternalId;
        }

        private async Task RefreshCatalogs()
        {
            Debug.Log("Check catalogs for update");
            var catalogUpdates = await Addressables.CheckForCatalogUpdates().Task;
            
            if (catalogUpdates is not {Count: > 0})
            {
                Debug.Log("No catalogs to update");
                return;
            }
            foreach (var catalogUpdate in catalogUpdates)
            {
                Debug.Log($"Catalog to update: {catalogUpdate}");
            }
            await Addressables.UpdateCatalogs(true).Task
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        Debug.LogWarning($"Failed to update catalogs. Error = {t.Exception.Message}");
                    }

                    if (t.Result is { Count: > 0 })
                    {
                        Debug.Log("Catalogs updated successfully");
                        foreach (var locator in t.Result)
                        {
                            Debug.Log($"Updated locator id: {locator.LocatorId}");
                        }
                    }
                    else
                    {
                        Debug.Log("No updated locators");
                    }
                });
        }

        public async Task<T> LoadAsset<T>(string path, bool cache = true)
        {
            object asset;
            lock (_cache)
            {
                if (_cache.TryGetValue(path, out asset))
                {
                    if (asset is T castAsset)
                    {
                        return castAsset;
                    }

                    throw new InvalidCastException(
                        $"Can not cast loaded asset to specified type. Path = {path}. Expected type = {typeof(T).Name}. Actual type = {asset.GetType().Name}");

                }
            }
            var loadOp = Addressables.LoadAssetAsync<T>(path);
            asset = await loadOp.Task;
            if (loadOp.OperationException != null)
            {
                throw loadOp.OperationException;
            }
            if (asset == null)
            {
                throw new InvalidOperationException($"Asset at path {path} is not loaded.");
            }
            if (cache)
            {
                lock (_cache)
                {
                    if (!_cache.ContainsKey(path))
                        _cache.Add(path, asset);
                }
            }

            return (T)asset;
        }

        public Task CacheAsset<T>(string path)
        {
            return LoadAsset<T>(path);
        }

        public T GetCachedAsset<T>(string path)
        {
            if (_cache.TryGetValue(path, out var asset))
            {
                if (asset is T castAsset)
                {
                    return castAsset;
                }

                throw new InvalidCastException("Can not cast loaded asset to specified type.");
                
            }

            throw new InvalidOperationException($"No cached asset at path {path}");
        }

        public bool TryGetCachedAsset<T>(string path, out T asset)
        {
            if (_cache.TryGetValue(path, out var a))
            {
                if (a is T castAsset)
                {
                    asset = castAsset;
                    return true;
                }
            }

            asset = default;
            return false;
        }

        public void ReleaseAllLoadedAssets()
        {
            var keys = _cache.Keys.ToArray();
            foreach (var key in keys)
            {
                Addressables.Release(_cache[key]);
            }
            _cache.Clear();
        }

        public void ReleaseLoadedAssets(string pathPattern)
        {
            if (_cache.ContainsKey(pathPattern))
            {
                Addressables.Release(_cache[pathPattern]);
                _cache.Remove(pathPattern);
            }

            var keys = _cache.Keys.ToArray();
            foreach (var key in keys)
            {
                if (Regex.IsMatch(key, pathPattern))
                {
                    Addressables.Release(_cache[key]);
                    _cache.Remove(key);
                }
            }
        }
        
        public async Task ClearReleasedAssets()
        {
            await Resources.UnloadUnusedAssets();
        }

        public async Task<bool> IsAssetDownloaded(string path)
        {
            await _initTask;
            
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"{GetType().Name}: Path is null or empty.");
                return false;
            }

            var downloadSize = await Addressables.GetDownloadSizeAsync(path).Task;
            return downloadSize <= 0;
        }

        public async Task<T> LoadAssetWithReference<T>(string path)
        {
            await _initTask;
            
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"{GetType().Name}: Path is null or empty.");
                return default;
            }

            var handle = Addressables.LoadAssetAsync<T>(path);
            var asset = await handle.Task;
            
            if (handle.OperationException != null)
            {
                Debug.LogError($"Error loading asset {path}: {handle.OperationException.Message}");
                Addressables.Release(asset);
                return default;
            }

            if (asset == null)
            {
                Debug.LogError($"Asset at path {path} is not loaded.");
                Addressables.Release(asset);
                return default;
            }

            _refLoaded[path] = asset;
            return asset;
        }

        public void ReleaseAssetWithReference(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"{GetType().Name}: Path is null or empty.");
                return;
            }

            if (_refLoaded.TryGetValue(path, out var obj))
            {   
                Addressables.Release(obj);
                return;
            }

            Debug.LogError($"Asset released before it is loaded at least once. {path}");
        }
    }
}
