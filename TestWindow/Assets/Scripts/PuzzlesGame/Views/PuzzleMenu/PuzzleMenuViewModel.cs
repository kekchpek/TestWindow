using System.Collections.Generic;
using AsyncReactAwait.Bindable;
using PuzzlesGame.Models.Puzzles;
using UnityEngine;
using UnityMVVM.ViewModelCore;

namespace PuzzlesGame.Views.PuzzleMenu
{
    public class PuzzleMenuViewModel : ViewModel, IPuzzleMenuViewModel
    {
        private readonly IPuzzlesModel _puzzlesModel;

        private readonly Mutable<IReadOnlyList<string>> _puzzleIds = new();

        public IBindable<IReadOnlyList<string>> PuzzleIds => _puzzleIds;

        public PuzzleMenuViewModel(IPuzzlesModel puzzlesModel)
        {
            _puzzlesModel = puzzlesModel;
        }

        protected override void OnSetupInternal()
        {
            base.OnSetupInternal();
            _puzzleIds.Value = _puzzlesModel.GetAllPuzzles();
        }

    }
}