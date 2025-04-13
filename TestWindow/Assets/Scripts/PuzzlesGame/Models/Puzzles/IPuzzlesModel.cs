using System.Collections.Generic;

namespace PuzzlesGame.Models.Puzzles
{
    public interface IPuzzlesModel
    {
        string GetPuzzlePreviewPath(string puzzleId);
        IReadOnlyList<string> GetAllPuzzles();
    }
}
