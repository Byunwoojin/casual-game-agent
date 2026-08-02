using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace MemoryMatch.EditorTools
{
    /// <summary>
    /// UI String Table 생성 + 4개 언어 번역 일괄 입력.
    /// 메뉴: MemoryMatch > Populate String Tables
    /// 선행: MemoryMatch > Setup Localization (Locales + Settings)
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
            new[] { "app_title",              "메모리 매칭",               "Memory Match",                "メモリーマッチ",                 "记忆配对" },
            new[] { "btn_play",                "플레이",                   "Play",                        "プレイ",                         "开始" },

            // LevelSelect (허브 화면) — Header 인사말/타이틀
            new[] { "home_greeting",           "오늘도 반가워요",           "Welcome back",                "今日もようこそ",                 "今天也欢迎您" },
            new[] { "home_title",              "카드 짝을 맞춰볼까요",      "Shall we match some cards?",  "カードのペアを揃えてみませんか", "来配对卡片吧" },
            new[] { "level_section_all",       "전체 작품",                "All Works",                   "すべての作品",                   "全部作品" },
            new[] { "level_section_subtitle",  "순서대로 열려요",           "Unlocks in order",            "順番に解放",                     "按顺序解锁" },
            new[] { "tile_pieces_format",      "{0}장",                    "{0} cards",                   "{0}枚",                          "{0}张" },

            // LevelSelect — Progress 영역
            new[] { "level_progress_stat",     "{0}점 / {1}점 완성",       "{0} pts / {1} pts done",      "{0}点 / {1}点達成",              "{0}分 / 共{1}分" },
            new[] { "level_milestone_next",    "다음 마일스톤까지 +{0}점",  "Next milestone +{0} pts",     "次のマイルストーンまで +{0}点",   "距下个里程碑 +{0}分" },
            new[] { "level_milestone_last",    "마지막 도전",              "Final challenge",             "最後の挑戦",                      "最后挑战" },

            // LevelSelect — Current Challenge 카드
            new[] { "level_badge_now",         "지금 도전",                "Up Next",                     "今すぐ挑戦",                      "即将挑战" },
            new[] { "level_card_replay_title", "다시 한 번 즐겨보세요",     "Enjoy it once more",          "もう一度遊んでみて",              "再玩一次" },
            new[] { "level_card_title_levelnum","Level {0}",               "Level {0}",                   "Level {0}",                       "Level {0}" },
            new[] { "level_card_meta",         "{0}장 · 약 {1}분",         "{0} cards · ~{1} min",        "{0}枚 · 約{1}分",                 "{0}张 · 约{1}分钟" },
            new[] { "level_continue_btn",      "계속하기",                 "Continue",                    "続ける",                          "继续" },
            new[] { "level_replay_btn",        "다시 풀기",                "Replay",                      "もう一度",                        "重玩" },
            new[] { "level_locked_v6",         "이전 작품을 먼저 완성해 주세요", "Complete the previous work first", "前の作品を先にクリアしてください", "请先完成上一幅作品" },

            // 난이도 라벨 (LevelSelect 타일 / Gallery 캡션 공용)
            new[] { "difficulty_easy",         "쉬움",                     "Easy",                        "簡単",                           "简单" },
            new[] { "difficulty_medium",       "보통",                     "Medium",                      "普通",                           "普通" },
            new[] { "difficulty_hard",         "어려움",                   "Hard",                        "難しい",                         "困难" },

            // BottomNav (LevelSelect / Gallery 공용, 2탭)
            new[] { "nav_home",                "메인",                     "Main",                        "メイン",                         "主页" },
            new[] { "nav_collection",          "갤러리",                   "Gallery",                     "ギャラリー",                     "画廊" },

            // Gallery
            new[] { "gallery_subtitle",        "나의 갤러리",               "My Gallery",                  "マイギャラリー",                 "我的画廊" },
            new[] { "gallery_title",           "카드 수집관",               "Card Hall",                   "カードコレクション",             "卡片收藏馆" },
            new[] { "gallery_count_format",    "{0}/{1}",                  "{0}/{1}",                     "{0}/{1}",                        "{0}/{1}" },
            new[] { "gallery_progress_until",  "다음 작품까지",             "Until next work",             "次の作品まで",                    "距下一幅作品" },
            new[] { "gallery_progress_now",    "레벨 {0} 진행중",           "Level {0} in progress",       "レベル{0}進行中",                 "第{0}关进行中" },
            new[] { "level_badge_all_done",    "모두 완성!",               "All Complete!",               "すべてクリア!",                   "全部完成!" },
            new[] { "gallery_floor_first",     "FIRST FLOOR",              "FIRST FLOOR",                 "FIRST FLOOR",                    "FIRST FLOOR" },
            new[] { "gallery_floor_second",    "SECOND FLOOR",             "SECOND FLOOR",                "SECOND FLOOR",                   "SECOND FLOOR" },
            new[] { "gallery_empty",           "아직 완성한 카드가 없어요", "No sets completed yet",       "まだ完成したセットがありません", "尚未完成任何卡组" },
            new[] { "gallery_empty_hint",      "짝을 다 맞추면 갤러리에 모여요", "Completed sets appear here", "ペアを揃えるとここに集まります", "完成配对后将收藏在此" },
            new[] { "common_coming_soon",      "준비 중",                  "Coming soon",                 "準備中",                          "敬请期待" },

            // Gameplay / Complete
            new[] { "complete_title",          "완성하셨어요!",             "Well done!",                  "完成しました!",                  "完成了!" },
            new[] { "complete_next_btn",       "다음 레벨",                 "Next Level",                  "次のレベル",                     "下一关" },

            // Settings
            new[] { "settings_title",          "설정",                     "Settings",                    "設定",                           "设置" },
            new[] { "settings_lang_subhead",   "언어 · Language",          "Language",                    "言語 · Language",               "语言 · Language" },
            new[] { "settings_sfx",            "효과음",                   "Sound Effects",               "効果音",                         "音效" },
            new[] { "settings_bgm_v6",         "배경음악",                 "Music",                       "BGM",                            "背景音乐" },
            new[] { "settings_version_format", "v{0} · 메모리 매칭",       "v{0} · Memory Match",         "v{0} · メモリーマッチ",           "v{0} · 记忆配对" },
        };

        [MenuItem("MemoryMatch/Populate String Tables")]
        public static void PopulateStringTables()
        {
            EnsureFolder(TableFolder);

            var uiCol = GetOrCreateCollection(UICollectionName);
            int uiCount = PopulateCollection(uiCol, UI_ROWS);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[LocalizationStringTableTool] Populated — UI={uiCount}");
            EditorUtility.DisplayDialog("MemoryMatch",
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
