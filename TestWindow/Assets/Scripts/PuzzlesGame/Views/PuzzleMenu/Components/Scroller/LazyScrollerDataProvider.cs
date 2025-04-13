using System.Collections.Generic;

namespace PuzzlesGame.Views.PuzzleMenu.Components.Scroller
{
    public class LazyScrollerDataProvider<T> : ILazyScrollerDataProvider
    {

        private readonly IReadOnlyList<T> _data;

        public LazyScrollerDataProvider(IReadOnlyList<T> data)
        {
            _data = data;
        }

        public int GetDataSize()
        {
            return _data?.Count ?? 0;
        }

        public void FillElement(ILazyScrollerElement element, int index)
        {
            element.SetData(_data[index]);
        }
        
    }
}