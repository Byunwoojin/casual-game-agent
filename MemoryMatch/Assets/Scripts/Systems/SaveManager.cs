using System;
using System.Globalization;
using UnityEngine;

namespace MemoryMatch.Systems
{
    /// <summary>
    /// PlayerPrefs 기반 진행도 저장소 — 모든 메서드가 static이므로
    /// 테스트에서 PlayerPrefs.DeleteAll() 후 바로 격리 가능
    /// </summary>
    public static class SaveManager
    {
        private const string LevelKeyPrefix      = "Level_Complete_";
        private const string LevelStarsKeyPrefix = "Level_Stars_";
        private const string HintCountKey        = "Hint_Count";
        private const string HintLastResetKey    = "Hint_LastResetDate";
        private const string DateFormat          = "yyyy-MM-dd";

        // ── 레벨 진행도 ──────────────────────────────────────

        public static void MarkLevelComplete(int levelIndex)
        {
            PlayerPrefs.SetInt(LevelKeyPrefix + levelIndex, 1);
            PlayerPrefs.Save();
        }

        public static bool IsLevelComplete(int levelIndex)
            => PlayerPrefs.GetInt(LevelKeyPrefix + levelIndex, 0) == 1;

        // ── 별점 (디자인 v0.6 — HomeSequential 우상단 ★1~3) ──
        // 0 = 미지급/별 없음, 1~3 = 별 개수. 클리어한 레벨만 의미 있음.
        // 별점 지급 로직 (시간 기반 등) 은 PuzzleCompleteChecker 측에서 결정 후 SetLevelStars 호출.

        public static void SetLevelStars(int levelIndex, int stars)
        {
            int clamped = Mathf.Clamp(stars, 0, 3);
            PlayerPrefs.SetInt(LevelStarsKeyPrefix + levelIndex, clamped);
            PlayerPrefs.Save();
        }

        public static int GetLevelStars(int levelIndex)
            => PlayerPrefs.GetInt(LevelStarsKeyPrefix + levelIndex, 0);

        /// <summary>
        /// 다음 도전 레벨 인덱스 — 첫 미클리어 레벨.
        /// 모두 클리어 시 levelCount 반환 (= "끝났음" 의미).
        /// HomeSequential 의 currentLevel 산출에 사용.
        /// </summary>
        public static int GetCurrentLevelIndex(int levelCount)
        {
            for (int i = 0; i < levelCount; i++)
                if (!IsLevelComplete(i)) return i;
            return levelCount;
        }

        /// <summary>클리어한 레벨 수 — Progress bar 의 fill 비율 산출에 사용.</summary>
        public static int GetClearedCount(int levelCount)
        {
            int n = 0;
            for (int i = 0; i < levelCount; i++)
                if (IsLevelComplete(i)) n++;
            return n;
        }

        // ── 힌트 데이터 ──────────────────────────────────────

        public static void SaveHintCount(int count)
        {
            PlayerPrefs.SetInt(HintCountKey, count);
            PlayerPrefs.Save();
        }

        public static int LoadHintCount()
            => PlayerPrefs.GetInt(HintCountKey, 0);

        /// <summary>일일 무료 힌트 마지막 지급일 (YYYY-MM-DD). 없으면 DateTime.MinValue.</summary>
        public static void SaveHintLastResetDate(DateTime date)
        {
            PlayerPrefs.SetString(HintLastResetKey, date.ToString(DateFormat, CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
        }

        public static DateTime LoadHintLastResetDate()
        {
            var s = PlayerPrefs.GetString(HintLastResetKey, "");
            if (string.IsNullOrEmpty(s)) return DateTime.MinValue;
            if (DateTime.TryParseExact(s, DateFormat, CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var d))
                return d;
            return DateTime.MinValue;
        }
    }
}
