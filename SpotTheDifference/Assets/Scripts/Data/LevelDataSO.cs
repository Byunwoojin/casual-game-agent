using UnityEngine;

namespace SpotDiff.Data
{
    [CreateAssetMenu(fileName = "Level_XX", menuName = "SpotDiff/LevelData")]
    public class LevelDataSO : ScriptableObject
    {
        public enum Difficulty { Easy, Medium, Hard }

        [Header("기본 정보")]
        public int levelNumber;
        public Difficulty difficulty;

        [Header("이미지")]
        public Sprite originalSprite;      // 정답(원본) 이미지 — Complete 화면 등에서 참고용
        public Sprite transformedSprite;   // 플레이 중 보여주는 변형 이미지 — 여기서 diff 를 찾음
        public int sourceImageWidth;       // diff_result.json 계산 당시 원본 픽셀 폭 (좌표 스케일링 기준)
        public int sourceImageHeight;      // diff_result.json 계산 당시 원본 픽셀 높이

        [Header("정답 diff 영역 — diff_result.json 의 confirmed:true 만 반입됨")]
        public DiffRegion[] diffRegions;

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

    /// <summary>diff_result.json 한 개 항목 — 좌상단 기준 픽셀 rect + 판정용 중심점.</summary>
    [System.Serializable]
    public struct DiffRegion
    {
        public int id;
        public float x, y, width, height;
        public float centerX, centerY;

        /// <summary>
        /// (px, py) 탭 좌표가 이 영역의 정답 판정 범위 안인지 — 순수 함수 (테스트 가능).
        /// tolerancePx 만큼 영역의 절반 폭/높이에 여유를 더해 축 정렬 사각형으로 판정.
        /// </summary>
        public readonly bool IsHit(float px, float py, float tolerancePx)
        {
            float halfW = width  * 0.5f + tolerancePx;
            float halfH = height * 0.5f + tolerancePx;
            return Mathf.Abs(px - centerX) <= halfW && Mathf.Abs(py - centerY) <= halfH;
        }
    }
}
