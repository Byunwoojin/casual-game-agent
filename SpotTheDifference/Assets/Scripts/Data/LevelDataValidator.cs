namespace SpotDiff.Data
{
    public static class LevelDataValidator
    {
        public static bool ValidateLevelNumber(int levelNumber) => levelNumber >= 1;

        public static bool ValidateSprites(UnityEngine.Sprite original, UnityEngine.Sprite transformed)
            => original != null && transformed != null;

        public static bool ValidateDiffRegions(DiffRegion[] regions)
            => regions != null && regions.Length > 0;
    }
}
