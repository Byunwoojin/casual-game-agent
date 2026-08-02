using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace SpotDiff.EditorTools
{
    /// <summary>
    /// UI String Table 생성 + 4개 언어 번역 일괄 입력.
    /// 메뉴: SpotDiff > Populate String Tables
    /// 선행: SpotDiff > Setup Localization (Locales + Settings)
    /// idempotent — 재실행 시 기존 값 덮어씀.
    /// </summary>
    public static class LocalizationStringTableTool
    {
        private const string TableFolder     = "Assets/Localization/Tables";
        private const string UICollectionName = "UI";

        // 열 순서: key, ko, en, ja, zh-Hans
        private static readonly string[][] UI_ROWS =
        {
            // MainMenu (최초 진입 화면)
            new[] { "app_title",              "틀린그림찾기",             "Spot the Difference",         "まちがいさがし",                 "大家来找茬" },
            new[] { "btn_play",                "플레이",                   "Play",                        "プレイ",                         "开始" },

            // LevelSelect (허브 화면)
            new[] { "level_select_title",      "틀린그림찾기",             "Spot the Difference",         "まちがいさがし",                 "大家来找茬" },
            new[] { "level_section_all",       "전체 레벨",                "All Levels",                  "すべてのレベル",                 "全部关卡" },
            new[] { "level_section_subtitle",  "순서대로 열려요",           "Unlocks in order",            "順番に解放",                     "按顺序解锁" },
            new[] { "level_locked_toast",      "이전 레벨을 먼저 완성해 주세요", "Complete the previous level first", "前のレベルを先にクリアしてください", "请先完成上一关" },
            new[] { "tile_pieces_format",      "{0}개",                    "{0} spots",                   "{0}か所",                        "{0}处" },

            // 난이도 라벨 (LevelSelect 타일 / Gallery 캡션 공용)
            new[] { "difficulty_easy",         "쉬움",                     "Easy",                        "簡単",                           "简单" },
            new[] { "difficulty_medium",       "보통",                     "Medium",                      "普通",                           "普通" },
            new[] { "difficulty_hard",         "어려움",                   "Hard",                        "難しい",                         "困难" },

            // BottomNav (LevelSelect / Gallery 공용, 2탭)
            new[] { "nav_home",                "메인",                     "Main",                        "メイン",                         "主页" },
            new[] { "nav_collection",          "갤러리",                   "Gallery",                     "ギャラリー",                     "画廊" },

            // Gallery
            new[] { "gallery_title",           "갤러리",                   "Gallery",                     "ギャラリー",                     "画廊" },
            new[] { "gallery_count_format",    "{0}/{1}",                  "{0}/{1}",                     "{0}/{1}",                        "{0}/{1}" },
            new[] { "gallery_empty",           "아직 완성한 그림이 없어요", "No pictures completed yet",   "まだ完成した絵がありません",     "尚未完成任何画作" },

            // Gameplay / Complete
            new[] { "complete_title",          "완성하셨어요!",             "Well done!",                  "完成しました!",                  "完成了!" },
            new[] { "complete_next_btn",       "다음 레벨",                 "Next Level",                  "次のレベル",                     "下一关" },

            // Settings
            new[] { "settings_title",          "설정",                     "Settings",                    "設定",                           "设置" },
            new[] { "settings_lang_subhead",   "언어 · Language",          "Language",                    "言語 · Language",               "语言 · Language" },
            new[] { "settings_sfx",            "효과음",                   "Sound Effects",               "効果音",                         "音效" },
            new[] { "settings_bgm_v6",         "배경음악",                 "Music",                       "BGM",                            "背景音乐" },
            new[] { "settings_version_format", "v{0} · 틀린그림찾기",      "v{0} · Spot the Difference",  "v{0} · まちがいさがし",          "v{0} · 大家来找茬" },
        };

        [MenuItem("SpotDiff/Populate String Tables")]
        public static void PopulateStringTables()
        {
            EnsureFolder(TableFolder);

            var uiCol = GetOrCreateCollection(UICollectionName);
            int uiCount = PopulateCollection(uiCol, UI_ROWS);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[LocalizationStringTableTool] Populated — UI={uiCount}");
            EditorUtility.DisplayDialog("SpotDiff",
                "String Table 입력 완료\n\n" +
                $"  • UI: {uiCount} keys × 4 locales\n\n" +
                "경로: " + TableFolder,
                "OK");
        }

        private static StringTableCollection GetOrCreateCollection(string name)
        {
            var existing = LocalizationEditorSettings.GetStringTableCollection(name);
            if (existing != null) return existing;

            return LocalizationEditorSettings.CreateStringTableCollection(name, TableFolder);
        }

        // rows[i] 열 순서: key, ko, en, ja, zh-Hans
        private static int PopulateCollection(StringTableCollection col, string[][] rows)
        {
            var localeCodes = new[] { "ko", "en", "ja", "zh-Hans" };

            foreach (var row in rows)
            {
                string key = row[0];
                var shared = col.SharedData.GetEntry(key) ?? col.SharedData.AddKey(key);

                for (int li = 0; li < localeCodes.Length; li++)
                {
                    var table = col.GetTable(new LocaleIdentifier(localeCodes[li])) as StringTable;
                    if (table == null) continue;

                    string value = row[li + 1];
                    var entry = table.GetEntry(shared.Id);
                    if (entry == null) table.AddEntry(shared.Id, value);
                    else entry.Value = value;

                    EditorUtility.SetDirty(table);
                }
            }

            EditorUtility.SetDirty(col.SharedData);
            EditorUtility.SetDirty(col);
            return rows.Length;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
            string leaf   = System.IO.Path.GetFileName(folder);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
