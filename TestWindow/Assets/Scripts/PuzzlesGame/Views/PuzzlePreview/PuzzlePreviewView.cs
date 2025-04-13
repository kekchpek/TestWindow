using UnityMVVM;
using UnityEngine;
using UnityEngine.UI;
using PuzzlesGame.Views.PuzzleMenu.Components.Scroller;

namespace PuzzlesGame.Views.PuzzlePreview
{
    public class PuzzlePreviewView : ViewBehaviour<IPuzzlePreviewViewModel>, ILazyScrollerElement
    {
        [SerializeField] private GameObject _loadingLayout;
        [SerializeField] private GameObject _errorLayout;
        [SerializeField] private Image _previewImage;
        [SerializeField] private Button _openButton;

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            
            _openButton.onClick.AddListener(() => ViewModel!.OpenPuzzle());
            
            SmartBind(ViewModel!.IsLoading, isLoading => _loadingLayout.SetActive(isLoading));
            SmartBind(ViewModel.PreviewSprite, sprite =>
            {
                _previewImage.sprite = sprite;
                _previewImage.gameObject.SetActive(sprite != null);
            });
            SmartBind(ViewModel.IsClickable, isClickable => _openButton.interactable = isClickable);
            SmartBind(ViewModel.IsError, isError => _errorLayout.SetActive(isError));
        }

        protected override void OnDestroy()
        {
            _openButton.onClick.RemoveAllListeners();
            base.OnDestroy();
        }

        public void SetData<T>(T data)
        {
            if (data is string puzzleId)
            {
                ViewModel!.SetPuzzleId(puzzleId);
            }
        }

        public RectTransform GetRectTransform()
        {
            return (RectTransform)transform;
        }

        public void OnTakenFromPool()
        {
            gameObject.SetActive(true);
        }

        public void OnReturnedToPool()
        {
            ViewModel?.ResetPuzzleId();
            gameObject.SetActive(false);
        }
    }
}