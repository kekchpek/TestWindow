using AssetsSystem;
using AssetsSystem.Assets.Scripts.Core;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityMVVM.ViewManager;

namespace Startup
{
    public class StartupService : IStartupService
    {
        private readonly IAssetsModel _assetsModel;
        private readonly IViewManager _viewManager;

        public StartupService(
            IAssetsModel assetsModel,
            IViewManager viewManager) {
            _assetsModel = assetsModel;
            _viewManager = viewManager;
        }

        public async UniTask Startup()
        {
            await _assetsModel.CacheAsset<GameObject>(ViewNames.PuzzleMenuView);
            await _viewManager.Open(LayerNames.Screen, ViewNames.PuzzleMenuView);
        }
    }
}
