using UnityEngine;

namespace SlidingPuzzle.Data
{
    [CreateAssetMenu(fileName = "Level_XX", menuName = "SlidingPuzzle/LevelData")]
    public class LevelDataSO : ScriptableObject
    {
        public enum Difficulty { Easy, Medium, Hard }

        [Header("기본 정보")]
        public int levelNumber;
        public Difficulty difficulty;

        [Header("이미지")]
        public Sprite sourceSprite; // 정사각 원본 — gridSize × gridSize 로 분할해서 사용

        [Header("그리드 — N×N 분할, 마지막 한 칸은 항상 빈 칸")]
        public int gridSize;

        public static int GridSizeFor(Difficulty d) => d switch
        {
            Difficulty.Easy   => 3,
            Difficulty.Medium => 4,
            Difficulty.Hard   => 5,
            _ => 3,
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

        // 작품명 — 직소 프로젝트의 동일 16장 이미지를 재사용하므로 이름도 그대로 대응 (LevelSelect/Gallery 공용).
        private static readonly string[] DefaultArtworkNames =
        {
            "사과 정물", "장미 꽃다발", "도자기와 과일",
            "푸른 발레리나", "분홍 발레리나", "강가의 다리",
            "별이 빛나는 밤", "수련의 정원", "황금의 키스",
            "진주 귀걸이", "철학자의 마지막", "아침의 정원",
            "튤립밭", "프로방스 라벤더", "벚꽃길", "해바라기",
        };

        public static string GetDefaultArtworkName(int levelNumber)
        {
            int idx = Mathf.Clamp(levelNumber - 1, 0, DefaultArtworkNames.Length - 1);
            return DefaultArtworkNames[idx];
        }
    }
}
