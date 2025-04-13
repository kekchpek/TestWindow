using UnityMVVM.ViewModelCore;
using UnityEngine;
using AsyncReactAwait.Bindable;
using System;
using Cysharp.Threading.Tasks;
using PuzzlesGame.Models.Puzzles;
using System.Threading.Tasks;
using AssetsSystem;
using System.Threading;

namespace PuzzlesGame.Views.PuzzlePreview
{
    public class PuzzlePreviewViewModel : ViewModel, IPuzzlePreviewViewModel
    {
        private readonly IAssetsModel _assetsModel;
        private readonly IPuzzlesModel _puzzleModel;
        private string _puzzleId;
        private readonly Mutable<bool> _isLoading = new(false);
        private readonly Mutable<Sprite> _previewSprite = new(null);
        private readonly Mutable<bool> _isClickable = new(false);
        private readonly Mutable<bool> _isError = new(false);
        private CancellationTokenSource _cancelDownload;
        private Task<Sprite> _previewHandle;
        private Task _previewReleaseHandle;
        private uint _puzzleLock = 0;

        public PuzzlePreviewViewModel(IAssetsModel assetsModel, IPuzzlesModel puzzleModel)
        {
            _assetsModel = assetsModel;
            _puzzleModel = puzzleModel;
        }

        public IBindable<bool> IsLoading => _isLoading;
        public IBindable<Sprite> PreviewSprite => _previewSprite;
        public IBindable<bool> IsClickable => _isClickable;
        public IBindable<bool> IsError => _isError;

        public void SetPuzzleId(string puzzleId)
        {
            if (_puzzleId == puzzleId) return;
            _puzzleId = puzzleId;
            _puzzleLock++;
            
            _isLoading.Value = true;
            _isClickable.Value = false;
            _previewSprite.Value = null;
            _isError.Value = false;
            
            LoadPuzzlePreview().Forget();
        }

        public void ResetPuzzleId()
        {
            if (string.IsNullOrEmpty(_puzzleId)) return;
            var puzzleId = _puzzleId;
            
            // Cancel any ongoing download
            _cancelDownload?.Cancel();
            _cancelDownload = null;

            _puzzleId = null;
            _puzzleLock++;
            _isLoading.Value = false;
            _isClickable.Value = false;
            _previewSprite.Value = null;
            _isError.Value = false;

            if (_previewReleaseHandle != null)
                return;
            _previewReleaseHandle = ReleasePreview(puzzleId);
        }

        private async Task ReleasePreview(string puzzleId) 
        {
            var puzzleLock = _puzzleLock;
            // Release the preview sprite if it exists
            if (_previewHandle != null)
            {
                await _previewHandle;
                var preview = _puzzleModel.GetPuzzlePreviewPath(puzzleId);
                _assetsModel.ReleaseAssetWithReference(preview);
                _previewHandle = null;
            }
        }

        private async UniTask LoadPuzzlePreview()
        {
            if (string.IsNullOrEmpty(_puzzleId))
            {
                _isLoading.Value = false;
                return;
            }

            var puzzleLock = _puzzleLock;

            try
            {
                // Get preview path from puzzle model
                var previewPath = _puzzleModel.GetPuzzlePreviewPath(_puzzleId);
                if (string.IsNullOrEmpty(previewPath))
                {
                    Debug.LogError($"Preview path not found for puzzle {_puzzleId}");
                    _isError.Value = true;
                    _isLoading.Value = false;
                    return;
                }

                // Check if puzzle is downloaded
                var isDownloaded = await _assetsModel.IsAssetDownloaded(previewPath);
                
                // Double check puzzle ID is still valid after download
                if (puzzleLock != _puzzleLock)
                {
                    return;
                }

                if (!isDownloaded)
                {
                    _cancelDownload = new CancellationTokenSource();
                    var task = _assetsModel.DownloadAsset(previewPath, _cancelDownload.Token);
                    await task;
                    if (puzzleLock != _puzzleLock)
                    {
                        return;
                    }
                    _cancelDownload = null;
                }

                if (_previewReleaseHandle != null) {
                    await _previewReleaseHandle;
                    if (puzzleLock != _puzzleLock)
                    {
                        return;
                    }
                    _previewReleaseHandle = null;
                }
            
                // Load preview sprite
                _previewHandle = _assetsModel.LoadAssetWithReference<Sprite>(previewPath);
                var sprite = await _previewHandle;
                if (sprite == null) 
                {
                    _previewHandle = null;
                }

                if (puzzleLock != _puzzleLock)
                {
                    return;
                }

                _previewSprite.Value = sprite;
                _isClickable.Value = true;
                _isLoading.Value = false;
            }
            catch (Exception e)
            {
                if (puzzleLock != _puzzleLock)
                {
                    return;
                }
                Debug.LogWarning($"Failed to load puzzle preview {_puzzleId}: {e.Message}");
                _isError.Value = true;
                _isLoading.Value = false;
            }
            finally
            {
                _cancelDownload = null;
            }
        }

        public void OpenPuzzle()
        {
            if (!_isClickable.Value || string.IsNullOrEmpty(_puzzleId)) return;
            Debug.Log($"Opening puzzle {_puzzleId}");
        }

        public void SetData<T>(T data)
        {
            if (data is string puzzleId)
            {
                SetPuzzleId(puzzleId);
            }
        }
    }
}