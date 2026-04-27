using UnityEngine;

namespace PuzzlePals.Data
{
    [CreateAssetMenu(fileName = "Level_XX", menuName = "PuzzlePals/LevelData")]
    public class LevelDataSO : ScriptableObject
    {
        public enum ImageBucket { VeryEasy, Easy, Medium, Hard, VeryHard }

        [Header("기본 정보")]
        public int    levelNumber;        // 레벨 번호 (1~20, 성인 피벗 v0.4)
        public string characterNameKey;   // Localization Key (성인 피벗 후: 작품명, 예: "art_the_great_wave")
        public Sprite characterSprite;    // 퍼즐 이미지 (명화 원본)
        public int    pieceCount;         // 조각 수 (36=6×6 또는 64=8×8 — 정사각 셀 보장 위해 제곱수만)

        [Header("명화 메타 (성인 피벗 v0.4)")]
        public string      artistName;    // "Winslow Homer" 등 (로컬라이즈 없음 — 고유 명사)
        public int         year;          // 제작 연도 (0이면 unknown)
        public ImageBucket imageBucket;   // UX 카테고리 (VE/E/M/H/VH) — 조각 수 분류와 독립

        [Header("[Legacy] 구버전 필드 — 리팩터 중 점진 제거")]
        public float snapThreshold;       // 레거시: 기존 유아용 스냅 민감도
        public bool  enablePieceDrift;    // 레거시: 81+ 드리프트
        public Color backgroundColor;     // 레거시: 파스텔 배경
    }
}
