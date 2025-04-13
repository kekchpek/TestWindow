using UnityMVVM.ViewModelCore;
using UnityEngine;
using AsyncReactAwait.Bindable;
using System;

namespace PuzzlesGame.Views.PuzzlePreview
{
    public interface IPuzzlePreviewViewModel : IViewModel
    {
        IBindable<bool> IsLoading { get; }
        IBindable<Sprite> PreviewSprite { get; }
        IBindable<bool> IsClickable { get; }
        IBindable<bool> IsError { get; }
        void SetPuzzleId(string puzzleId);
        void ResetPuzzleId();
        void OpenPuzzle();
    }
}