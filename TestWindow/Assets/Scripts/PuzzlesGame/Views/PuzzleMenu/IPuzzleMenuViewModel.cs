using System.Collections.Generic;
using AsyncReactAwait.Bindable;
using UnityMVVM.ViewModelCore;

namespace PuzzlesGame.Views.PuzzleMenu
{
    public interface IPuzzleMenuViewModel : IViewModel
    {
        
        IBindable<IReadOnlyList<string>> PuzzleIds { get; }
                
    }
}