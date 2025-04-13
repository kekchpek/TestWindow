using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace AssetsSystem
{
    
    /// <summary>
    /// The entry point for assets management API.
    /// </summary>
    public interface IAssetsModel
    {

        /// <summary>
        /// Fetching inforation about remote assets state. No new remote assets will be downloaded.
        /// </summary>
        /// <returns></returns>
        UniTask FetchRemoteAssetsData();
        
        /// <summary>
        /// Downloads the least asset versions.
        /// </summary>
        /// <param name="paths">The paths of assets to download.</param>
        /// <param name="progress">Progress(count of downloaded paths)</param>
        /// <returns>Returns a download task.</returns>
        Task DownloadAsset(string path, CancellationToken ct);
        
        /// <summary>
        /// Loads the asset of specified type.
        /// </summary>
        /// <param name="path">The path to the asset.</param>
        /// <param name="cache">True if asset should be cached.</param>
        /// <typeparam name="T">The type of the asset.</typeparam>
        /// <returns>Returns loading async operation handle with.</returns>
        Task<T> LoadAsset<T>(string path, bool cache = true);
        
        /// <summary>
        /// Caches specified asset.
        /// </summary>
        /// <param name="path">The pass of the asset.</param>
        /// <returns>Returns caching async operation handle.</returns>
        Task CacheAsset<T>(string path);

        /// <summary>
        /// Gets cached assets. Throws an exception if asset is not cached.
        /// </summary>
        /// <param name="path">The path to the asset.</param>
        /// <typeparam name="T">The type of the asset.</typeparam>
        /// <returns>Returns cached asset.</returns>
        T GetCachedAsset<T>(string path);

        /// <summary>
        /// Tries get cached assets.
        /// </summary>
        /// <param name="path">The path of the asset.</param>
        /// <param name="asset">The asset, got from cache.</param>
        /// <typeparam name="T">The type of the asset.</typeparam>
        /// <returns>True if asset of specified type is found in cache. False - otherwise.</returns>
        bool TryGetCachedAsset<T>(string path, out T asset);
        
        /// <summary>
        /// Clears entire cache.
        /// </summary>
        void ReleaseAllLoadedAssets();
        
        /// <summary>
        /// Clears specified cached assets asynchronously 
        /// </summary>
        /// <param name="pathPattern">The pattern of assets path to be cleared from cache</param>
        /// <returns>Clearing async operation handle.</returns>
        void ReleaseLoadedAssets(string pathPattern);

        /// <summary>
        /// Clears all released assets asynchronously.
        /// </summary>
        /// <returns>Clearing async operation handle.</returns>
        Task ClearReleasedAssets();

        /// <summary>
        /// Checks if the asset at the specified path is already downloaded.
        /// </summary>
        /// <param name="path">The path to the asset.</param>
        /// <returns>True if the asset is downloaded, false otherwise.</returns>
        Task<bool> IsAssetDownloaded(string path);

        /// <summary>
        /// Loads the asset with reference counting.
        /// Each call to this method increments the reference counter.
        /// The asset will be released only when the reference counter reaches zero.
        /// </summary>
        /// <param name="path">The path to the asset.</param>
        /// <typeparam name="T">The type of the asset.</typeparam>
        /// <returns>Returns the loaded asset.</returns>
        Task<T> LoadAssetWithReference<T>(string path);

        /// <summary>
        /// Releases the asset with reference counting.
        /// Decrements the reference counter and releases the asset if the counter reaches zero.
        /// </summary>
        /// <param name="path">The path of the asset to release.</param>
        void ReleaseAssetWithReference(string path);
    }
}