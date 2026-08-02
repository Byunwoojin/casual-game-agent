using UnityEngine;

namespace MemoryMatch.Data
{
    [CreateAssetMenu(fileName = "Level_XX", menuName = "MemoryMatch/LevelData")]
    public class LevelDataSO : ScriptableObject
    {
        public enum Difficulty { Easy, Medium, Hard }

        [Header("기본 정보")]
        public int levelNumber;
        public Difficulty difficulty;

        [Header("카드 이미지 — pairCount 장, 각 장이 카드 2장(쌍)에 쓰임")]
        public Sprite[] cardSprites;
        public int pairCount;

        // 난이도별 체감 차이를 키우기 위해 4/6/8 대신 4/8/12 로 조정 (사용자 피드백 2026-08-03).
        public static int PairCountFor(Difficulty d) => d switch
        {
            Difficulty.Easy   => 4,
            Difficulty.Medium => 8,
            Difficulty.Hard   => 12,
            _ => 4,
        };

        public static string DifficultyEntryKey(Difficulty d) => d switch
        {
            Difficulty.Easy   => "difficulty_easy",
            Difficulty.Medium => "difficulty_medium",
            Difficulty.Hard   => "difficulty_hard",
            _ => "difficulty_easy",
        };

        public static string DifficultyFallback(Difficulty d) => d switch
        {
            Difficulty.Easy   => "쉬움",
            Difficulty.Medium => "보통",
            Difficulty.Hard   => "어려움",
            _ => "",
        };
    }
}
