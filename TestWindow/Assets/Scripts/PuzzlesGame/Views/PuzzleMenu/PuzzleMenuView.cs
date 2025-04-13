using UnityMVVM;
using UnityEngine;
using PuzzlesGame.Views.PuzzleMenu.Components.Scroller;

namespace PuzzlesGame.Views.PuzzleMenu
{
    public class PuzzleMenuView : ViewBehaviour<IPuzzleMenuViewModel>
    {
        [SerializeField] private LazyScroller _scroller;
        
        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            SmartBind(ViewModel!.PuzzleIds, ids => _scroller.SetData(ids));
        }
    }
}