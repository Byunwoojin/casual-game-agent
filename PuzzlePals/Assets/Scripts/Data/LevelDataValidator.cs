using UnityEngine;

namespace PuzzlePals.Data
{
    /// <summary>
    /// LevelDataSO 유효성 검증 — 성인 피벗 v0.5.
    /// 1~16 레벨, 조각 수 32 또는 64만 허용.
    /// </summary>
    public static class LevelDataValidator
    {
        public static bool ValidateLevelNumber(int levelNumber)
            => levelNumber >= 1 && levelNumber <= 16;

        public static bool ValidateCharacterSprite(Sprite sprite)
            => sprite != null;

        public static bool ValidateCharacterNameKey(string key)
            => !string.IsNullOrEmpty(key);

        /// <summary>v0.5: 조각 수는 36 (6×6) 또는 64 (8×8) 만 허용 — 정사각 셀 보장 위해 제곱수만 사용.</summary>
        public static bool ValidatePieceCount(int pieceCount)
            => pieceCount == 36 || pieceCount == 64;
    }
}
