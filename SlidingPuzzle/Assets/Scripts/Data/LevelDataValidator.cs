namespace SlidingPuzzle.Data
{
    public static class LevelDataValidator
    {
        public static bool ValidateLevelNumber(int levelNumber) => levelNumber >= 1;

        public static bool ValidateSprite(UnityEngine.Sprite sprite) => sprite != null;

        public static bool ValidateGridSize(int gridSize) => gridSize >= 2;
    }
}
