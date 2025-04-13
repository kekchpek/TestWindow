using System.Linq;
using AssetsSystem;
using PuzzlesGame.Models.Puzzles;
using PuzzlesGame.Views;
using Startup;
using UnityEngine;
using UnityMVVM.DI;
using UnityMVVM.ViewModelCore.PrefabsProvider;
using Zenject;

namespace DI
{
    public class CoreInstaller : MonoInstaller
    {

        [SerializeField] 
        private Transform[] _viewLayers;
        
        public override void InstallBindings()
        {
            Container.UseAsMvvmContainer(_viewLayers.Select(x => (x.name, x)).ToArray());
            Container.Bind<IStartupService>().To<StartupService>().AsSingle().WhenInjectedInto<StartupBehaviour>();
            Container.FastBind<IViewsPrefabsProvider, AssetsViewsPrefabsProvider>();

            Container.FastBind<IAssetsModel, AddressablesAssetsModel>();
            
            // Game Installers
            Container.Install<ViewsInstaller>();
            Container.Install<PuzzlesSystemInstaller>();
        }
        
    }
}