using UnityEditor;
using UnityEngine;

namespace PuzzlePals.EditorTools
{
    /// <summary>
    /// 개발 중 PlayerPrefs / 진행도 초기화용 디버그 메뉴.
    /// 출시 직전 이 파일을 그대로 두어도 메뉴 자체가 Editor 빌드만 노출 → 안전.
    /// </summary>
    public static class DebugTools
    {
        // ─────────────────────────────────────────────────────
        // 진행도 초기화
        // ─────────────────────────────────────────────────────

        [MenuItem("PuzzlePals/Debug/Reset Save Data (All)")]
        public static void ResetSaveDataAll()
        {
            bool confirm = EditorUtility.DisplayDialog(
                "Reset Save Data",
                "PlayerPrefs 의 모든 데이터를 삭제합니다.\n\n" +
                "포함:\n" +
                "  • 레벨 클리어 진행도\n" +
                "  • 힌트 카운트 + 일일 리셋 날짜\n" +
                "  • 사운드 / 언어 / 기타 설정\n\n" +
                "되돌릴 수 없습니다. 진행할까요?",
                "Delete All",
                "Cancel");

            if (!confirm) return;

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            Debug.Log("[DebugTools] PlayerPrefs.DeleteAll() 실행 — 모든 진행도/설정 초기화됨.");
            EditorUtility.DisplayDialog("Reset 완료",
                "PlayerPrefs 가 비워졌습니다.\n다음 Play 부터 새 사용자처럼 시작합니다.",
                "OK");
        }

        [MenuItem("PuzzlePals/Debug/Reset Level Progress Only")]
        public static void ResetLevelProgressOnly()
        {
            bool confirm = EditorUtility.DisplayDialog(
                "Reset Level Progress",
                "레벨 클리어 진행도만 삭제합니다.\n" +
                "(힌트 카운트, 사운드 / 언어 설정 은 유지)\n\n진행할까요?",
                "Delete",
                "Cancel");

            if (!confirm) return;

            // SaveManager.LevelKeyPrefix = "Level_Complete_" — 모든 인덱스 삭제
            // PlayerPrefs 는 키 enumerate 가 안 되므로 충분히 큰 범위로 삭제 (성인 피벗 20레벨 + 여유)
            int deleted = 0;
            for (int i = 0; i < 200; i++)
            {
                string key = $"Level_Complete_{i}";
                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                    deleted++;
                }
            }
            PlayerPrefs.Save();

            Debug.Log($"[DebugTools] 레벨 진행도 {deleted}개 삭제됨.");
            EditorUtility.DisplayDialog("Reset 완료",
                $"레벨 진행도 {deleted}개 삭제됨.\n힌트/설정은 유지됩니다.",
                "OK");
        }

        [MenuItem("PuzzlePals/Debug/Reset Hint Data Only")]
        public static void ResetHintDataOnly()
        {
            PlayerPrefs.DeleteKey("Hint_Count");
            PlayerPrefs.DeleteKey("Hint_LastResetDate");
            PlayerPrefs.Save();

            Debug.Log("[DebugTools] 힌트 카운트 + 일일 리셋 날짜 삭제됨.");
            EditorUtility.DisplayDialog("Reset 완료",
                "힌트 데이터 초기화됨.\n다음 Play 진입 시 자동으로 일일 무료 +2 지급.",
                "OK");
        }

        // ─────────────────────────────────────────────────────
        // 진행도 set (테스트 편의)
        // ─────────────────────────────────────────────────────

        [MenuItem("PuzzlePals/Debug/Mark First 5 Levels Cleared")]
        public static void MarkFirstFiveCleared()
        {
            for (int i = 0; i < 5; i++)
                PlayerPrefs.SetInt($"Level_Complete_{i}", 1);
            PlayerPrefs.Save();
            Debug.Log("[DebugTools] L1~L5 클리어 마킹됨. L6 가 활성 카드.");
        }

        [MenuItem("PuzzlePals/Debug/Add 10 Hints")]
        public static void AddTenHints()
        {
            int cur = PlayerPrefs.GetInt("Hint_Count", 0);
            PlayerPrefs.SetInt("Hint_Count", cur + 10);
            PlayerPrefs.Save();
            Debug.Log($"[DebugTools] 힌트 +10 (현재 {cur + 10}개)");
        }
    }
}
