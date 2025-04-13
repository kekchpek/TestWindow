namespace PuzzlesGame.Views.PuzzleMenu.Components.Scroller
{
    public interface ILazyScrollerDataProvider
    {
        int GetDataSize();
        void FillElement(ILazyScrollerElement element, int index);
    }
}