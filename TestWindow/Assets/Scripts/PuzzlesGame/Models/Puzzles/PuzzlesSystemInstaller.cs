using UnityMVVM.DI;
using Zenject;

namespace PuzzlesGame.Models.Puzzles
{
    public class PuzzlesSystemInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.FastBind<IPuzzlesModel, PuzzlesMockModel>();
        }
    }
}