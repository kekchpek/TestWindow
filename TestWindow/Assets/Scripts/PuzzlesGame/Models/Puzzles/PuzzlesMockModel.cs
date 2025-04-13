using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzlesGame.Models.Puzzles
{
    public class PuzzlesMockModel : IPuzzlesModel
    {
        
        public string GetPuzzlePreviewPath(string puzzleId)
        {
            if (Convert.ToInt32(puzzleId.Split("_")[1]) <= 300)
                return $"Assets/Content/Puzzles/Previews/{puzzleId}.jpg";
            return "PuzzlePreviews/Placeholder";
        }

        public IReadOnlyList<string> GetAllPuzzles()
        {
            return Enumerable.Range(1, 1000).Select(x => string.Intern($"puzzle_{x}")).ToArray();
        }
        
    }
}