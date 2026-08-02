namespace MemoryMatch.Data
{
    public static class LevelDataValidator
    {
        public static bool ValidateLevelNumber(int levelNumber) => levelNumber >= 1;

        public static bool ValidateCardSprites(UnityEngine.Sprite[] sprites, int pairCount)
            => sprites != null && sprites.Length >= pairCount && pairCount >= 2;
    }
}
