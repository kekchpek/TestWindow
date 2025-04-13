using UnityEngine;

namespace PuzzlesGame.Views.PuzzleMenu.Components.Scroller
{
    public interface ILazyScrollerElement
    {
        void SetData<T>(T data);
        RectTransform GetRectTransform();
        void OnTakenFromPool();
        void OnReturnedToPool();
    }
}