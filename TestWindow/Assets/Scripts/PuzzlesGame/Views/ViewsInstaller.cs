using Core;
using PuzzlesGame.Views.PuzzleMenu;
using PuzzlesGame.Views.PuzzlePreview;
using UnityMVVM.DI;
using Zenject;

namespace PuzzlesGame.Views
{
    public class ViewsInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.InstallView<PuzzleMenuView, IPuzzleMenuViewModel  , PuzzleMenuViewModel>(ViewNames.PuzzleMenuView);
            Container.InstallView<PuzzlePreviewView, IPuzzlePreviewViewModel, PuzzlePreviewViewModel>();
        }
    }
}