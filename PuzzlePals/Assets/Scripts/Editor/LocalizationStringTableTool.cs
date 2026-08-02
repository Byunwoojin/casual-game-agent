using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace PuzzlePals.EditorTools
{
    /// <summary>
    /// String Table 생성 + 100 캐릭터 + UI 키 4개 언어 번역 일괄 입력.
    /// 메뉴: PuzzlePals > Populate String Tables
    /// 선행: PuzzlePals > Setup Localization (Locales + Settings)
    /// idempotent — 재실행 시 기존 값 덮어씀.
    /// </summary>
    public static class LocalizationStringTableTool
    {
        private const string TableFolder       = "Assets/Localization/Tables";
        private const string CharCollectionName = "Characters";
        private const string UICollectionName   = "UI";

        // 열 순서: key, ko, en, ja, zh-Hans
        private static readonly string[][] CHAR_ROWS =
        {
            new[] { "char_apple",          "사과",           "Apple",            "りんご",                 "苹果" },
            new[] { "char_ball",           "공",             "Ball",             "ボール",                 "球" },
            new[] { "char_sun",            "태양",           "Sun",              "たいよう",               "太阳" },
            new[] { "char_heart",          "하트",           "Heart",            "ハート",                 "心" },
            new[] { "char_star",           "별",             "Star",             "ほし",                   "星星" },
            new[] { "char_cup",            "컵",             "Cup",              "コップ",                 "杯子" },
            new[] { "char_balloon",        "풍선",           "Balloon",          "ふうせん",               "气球" },
            new[] { "char_flower",         "꽃",             "Flower",           "はな",                   "花" },
            new[] { "char_house",          "집",             "House",            "いえ",                   "房子" },
            new[] { "char_cake",           "케이크",         "Cake",             "ケーキ",                 "蛋糕" },
            new[] { "char_car",            "자동차",         "Car",              "くるま",                 "汽车" },
            new[] { "char_train",          "기차",           "Train",            "でんしゃ",               "火车" },
            new[] { "char_boat",           "배",             "Boat",             "ふね",                   "船" },
            new[] { "char_airplane",       "비행기",         "Airplane",         "ひこうき",               "飞机" },
            new[] { "char_bicycle",        "자전거",         "Bicycle",          "じてんしゃ",             "自行车" },
            new[] { "char_umbrella",       "우산",           "Umbrella",         "かさ",                   "雨伞" },
            new[] { "char_rainbow",        "무지개",         "Rainbow",          "にじ",                   "彩虹" },
            new[] { "char_ice_cream",      "아이스크림",     "Ice Cream",        "アイスクリーム",         "冰淇淋" },
            new[] { "char_pencil",         "연필",           "Pencil",           "えんぴつ",               "铅笔" },
            new[] { "char_book",           "책",             "Book",             "ほん",                   "书" },
            new[] { "char_tree",           "나무",           "Tree",             "き",                     "树" },
            new[] { "char_kite",           "연",             "Kite",             "たこ",                   "风筝" },
            new[] { "char_lollipop",       "막대사탕",       "Lollipop",         "ぺろぺろキャンディ",     "棒棒糖" },
            new[] { "char_piano",          "피아노",         "Piano",            "ピアノ",                 "钢琴" },
            new[] { "char_rocket",         "로켓",           "Rocket",           "ロケット",               "火箭" },
            new[] { "char_camera",         "카메라",         "Camera",           "カメラ",                 "相机" },
            new[] { "char_guitar",         "기타",           "Guitar",           "ギター",                 "吉他" },
            new[] { "char_lighthouse",     "등대",           "Lighthouse",       "とうだい",               "灯塔" },
            new[] { "char_clock",          "시계",           "Clock",            "とけい",                 "时钟" },
            new[] { "char_castle",         "성",             "Castle",           "しろ",                   "城堡" },
            new[] { "char_submarine",      "잠수함",         "Submarine",        "せんすいかん",           "潜水艇" },
            new[] { "char_telescope",      "망원경",         "Telescope",        "ぼうえんきょう",         "望远镜" },
            new[] { "char_lantern",        "등불",           "Lantern",          "ちょうちん",             "灯笼" },
            new[] { "char_tractor",        "트랙터",         "Tractor",          "トラクター",             "拖拉机" },
            new[] { "char_spaceship",      "우주선",         "Spaceship",        "うちゅうせん",           "宇宙飞船" },
            new[] { "char_bridge",         "다리",           "Bridge",           "はし",                   "桥" },
            new[] { "char_windmill",       "풍차",           "Windmill",         "ふうしゃ",               "风车" },
            new[] { "char_ferris_wheel",   "관람차",         "Ferris Wheel",     "かんらんしゃ",           "摩天轮" },
            new[] { "char_hot_air_balloon","열기구",         "Hot Air Balloon",  "ねつききゅう",           "热气球" },
            new[] { "char_crown",          "왕관",           "Crown",            "おうかん",               "王冠" },
            new[] { "char_diamond",        "다이아몬드",     "Diamond",          "ダイヤモンド",           "钻石" },
            new[] { "char_snowflake",      "눈꽃",           "Snowflake",        "ゆきのけっしょう",       "雪花" },
            new[] { "char_treasure_chest", "보물상자",       "Treasure Chest",   "たからばこ",             "宝箱" },
            new[] { "char_cactus",         "선인장",         "Cactus",           "サボテン",               "仙人掌" },
            new[] { "char_pirate_ship",    "해적선",         "Pirate Ship",      "かいぞくせん",           "海盗船" },
            new[] { "char_robot",          "로봇",           "Robot",            "ロボット",               "机器人" },
            new[] { "char_fire_truck",     "소방차",         "Fire Truck",       "しょうぼうしゃ",         "消防车" },
            new[] { "char_space_station",  "우주정거장",     "Space Station",    "うちゅうステーション",   "太空站" },
            new[] { "char_dragon_ball",    "드래곤볼",       "Dragon Ball",      "ドラゴンボール",         "龙珠" },
            new[] { "char_magic_wand",     "마법 지팡이",    "Magic Wand",       "まほうのつえ",           "魔杖" },
            new[] { "char_cat",            "고양이",         "Cat",              "ねこ",                   "猫" },
            new[] { "char_dog",            "강아지",         "Dog",              "いぬ",                   "狗" },
            new[] { "char_duck",           "오리",           "Duck",             "アヒル",                 "鸭子" },
            new[] { "char_fish",           "물고기",         "Fish",             "さかな",                 "鱼" },
            new[] { "char_chick",          "병아리",         "Chick",            "ひよこ",                 "小鸡" },
            new[] { "char_rabbit",         "토끼",           "Rabbit",           "うさぎ",                 "兔子" },
            new[] { "char_bear",           "곰",             "Bear",             "くま",                   "熊" },
            new[] { "char_penguin",        "펭귄",           "Penguin",          "ペンギン",               "企鹅" },
            new[] { "char_pig",            "돼지",           "Pig",              "ぶた",                   "猪" },
            new[] { "char_frog",           "개구리",         "Frog",             "カエル",                 "青蛙" },
            new[] { "char_elephant",       "코끼리",         "Elephant",         "ぞう",                   "大象" },
            new[] { "char_giraffe",        "기린",           "Giraffe",          "キリン",                 "长颈鹿" },
            new[] { "char_lion",           "사자",           "Lion",             "ライオン",               "狮子" },
            new[] { "char_monkey",         "원숭이",         "Monkey",           "さる",                   "猴子" },
            new[] { "char_cow",            "젖소",           "Cow",              "うし",                   "奶牛" },
            new[] { "char_horse",          "말",             "Horse",            "うま",                   "马" },
            new[] { "char_sheep",          "양",             "Sheep",            "ひつじ",                 "羊" },
            new[] { "char_panda",          "판다",           "Panda",            "パンダ",                 "熊猫" },
            new[] { "char_koala",          "코알라",         "Koala",            "コアラ",                 "考拉" },
            new[] { "char_turtle",         "거북이",         "Turtle",           "カメ",                   "乌龟" },
            new[] { "char_butterfly",      "나비",           "Butterfly",        "ちょう",                 "蝴蝶" },
            new[] { "char_owl",            "부엉이",         "Owl",              "ふくろう",               "猫头鹰" },
            new[] { "char_peacock",        "공작새",         "Peacock",          "クジャク",               "孔雀" },
            new[] { "char_crab",           "게",             "Crab",             "カニ",                   "螃蟹" },
            new[] { "char_octopus",        "문어",           "Octopus",          "たこ",                   "章鱼" },
            new[] { "char_flamingo",       "플라밍고",       "Flamingo",         "フラミンゴ",             "火烈鸟" },
            new[] { "char_hedgehog",       "고슴도치",       "Hedgehog",         "ハリネズミ",             "刺猬" },
            new[] { "char_fox",            "여우",           "Fox",              "キツネ",                 "狐狸" },
            new[] { "char_deer",           "사슴",           "Deer",             "シカ",                   "鹿" },
            new[] { "char_parrot",         "앵무새",         "Parrot",           "オウム",                 "鹦鹉" },
            new[] { "char_hippo",          "하마",           "Hippo",            "カバ",                   "河马" },
            new[] { "char_crocodile",      "악어",           "Crocodile",        "ワニ",                   "鳄鱼" },
            new[] { "char_polar_bear",     "북극곰",         "Polar Bear",       "しろくま",               "北极熊" },
            new[] { "char_zebra",          "얼룩말",         "Zebra",            "シマウマ",               "斑马" },
            new[] { "char_gorilla",        "고릴라",         "Gorilla",          "ゴリラ",                 "大猩猩" },
            new[] { "char_kangaroo",       "캥거루",         "Kangaroo",         "カンガルー",             "袋鼠" },
            new[] { "char_narwhal",        "일각고래",       "Narwhal",          "イッカク",               "独角鲸" },
            new[] { "char_axolotl",        "아홀로틀",       "Axolotl",          "ウーパールーパー",       "美西螈" },
            new[] { "char_sloth",          "나무늘보",       "Sloth",            "ナマケモノ",             "树懒" },
            new[] { "char_chameleon",      "카멜레온",       "Chameleon",        "カメレオン",             "变色龙" },
            new[] { "char_dragon",         "용",             "Dragon",           "ドラゴン",               "龙" },
            new[] { "char_phoenix",        "불사조",         "Phoenix",          "フェニックス",           "凤凰" },
            new[] { "char_unicorn",        "유니콘",         "Unicorn",          "ユニコーン",             "独角兽" },
            new[] { "char_mermaid_cat",    "인어고양이",     "Mermaid Cat",      "にんぎょねこ",           "美人鱼猫" },
            new[] { "char_dinosaur",       "공룡",           "Dinosaur",         "きょうりゅう",           "恐龙" },
            new[] { "char_shark",          "상어",           "Shark",            "サメ",                   "鲨鱼" },
            new[] { "char_toucan",         "투칸",           "Toucan",           "オオハシ",               "巨嘴鸟" },
            new[] { "char_snow_leopard",   "설표",           "Snow Leopard",     "ユキヒョウ",             "雪豹" },
            new[] { "char_giant_squid",    "대왕오징어",     "Giant Squid",      "ダイオウイカ",           "大王乌贼" },
            new[] { "char_rainbow_dragon", "무지개 용",      "Rainbow Dragon",   "にじのドラゴン",         "彩虹龙" },
        };

        private static readonly string[][] UI_ROWS =
        {
            new[] { "app_title",            "퍼즐 팔스",              "Puzzle Pals",              "パズルパル",                     "拼图伙伴" },
            new[] { "btn_play",             "플레이",                 "Play",                     "プレイ",                         "开始" },
            new[] { "btn_home",             "홈",                     "Home",                     "ホーム",                         "首页" },
            new[] { "btn_next",             "다음",                   "Next",                     "つぎへ",                         "下一个" },
            new[] { "btn_settings",         "설정",                   "Settings",                 "設定",                           "设置" },
            new[] { "btn_hint",             "힌트",                   "Hint",                     "ヒント",                         "提示" },
            new[] { "btn_close",            "닫기",                   "Close",                    "閉じる",                         "关闭" },
            new[] { "btn_buy",              "구매",                   "Buy",                      "購入",                           "购买" },
            new[] { "btn_watch_ad",         "광고 보기",              "Watch Ad",                 "広告を見る",                     "观看广告" },
            new[] { "level_select_title",   "레벨 선택",              "Select Level",             "レベル選択",                     "选择关卡" },
            new[] { "level_locked",         "잠김",                   "Locked",                   "ロック",                         "未解锁" },
            new[] { "complete_title",       "완성!",                  "Complete!",                "かんせい!",                      "完成!" },
            new[] { "complete_subtitle",    "{0} 완성!",              "{0} Complete!",            "{0}かんせい!",                   "{0}完成!" },
            new[] { "hint_popup_title",     "힌트가 부족해요",         "Need More Hints",          "ヒントが足りない",               "提示不足" },
            new[] { "hint_popup_description","광고를 보거나 구매해서\n힌트를 얻을 수 있어요", "Watch an ad or buy\nto get more hints", "広告を見るか購入で\nヒントを獲得", "观看广告或购买\n获取更多提示" },
            new[] { "settings_language",    "언어",                   "Language",                 "言語",                           "语言" },
            new[] { "settings_sfx",         "효과음",                 "Sound Effects",            "効果音",                         "音效" },
            new[] { "settings_bgm",         "배경음",                 "Music",                    "BGM",                            "背景音乐" },

            // ─────────────────────────────────────────────
            // 성인 피벗 v0.5 — 신규 노출 텍스트 (2026-04-27 추가)
            // ─────────────────────────────────────────────
            new[] { "settings_title",       "설정",                   "Settings",                 "設定",                           "设置" },

            // Gameplay 툴바 4 버튼
            new[] { "btn_back",             "뒤로",                   "Back",                     "戻る",                           "返回" },
            new[] { "btn_restart",          "초기화",                 "Restart",                   "リセット",                       "重置" },
            new[] { "btn_preview",          "원본",                   "Preview",                   "プレビュー",                     "预览" },

            // 원본 오버레이
            new[] { "tap_to_close",         "탭해서 닫기",            "Tap to close",              "タップで閉じる",                 "点击关闭" },

            // 힌트 / 광고 토스트
            new[] { "hint_ad_unavailable",  "광고 시스템 준비 안 됨",  "Ads unavailable",           "広告は利用できません",           "广告不可用" },
            new[] { "hint_ad_loading",      "광고를 불러오는 중...",   "Loading ad...",             "広告を読み込み中...",            "加载广告中..." },
            new[] { "hint_ad_not_ready",    "광고가 준비되지 않았어요. 잠시 후 다시 시도해주세요.", "Ad not ready. Please try again shortly.", "広告の準備ができていません。少々お待ちください。", "广告未准备好，请稍后再试。" },
            new[] { "hint_ad_rewarded",     "광고 시청 완료! 힌트 +1",  "Ad complete! +1 hint",      "視聴完了! ヒント+1",             "观看完成! 提示+1" },

            // LevelSelect 잠긴 카드 토스트
            new[] { "level_locked_toast",   "이전 퍼즐을 완료하세요",  "Complete the previous puzzle first", "前のパズルを完成させてください", "请先完成上一关" },

            // Complete 씬
            new[] { "complete_level_done",  "LEVEL {0} 완성!",        "Level {0} Complete!",       "レベル{0}クリア!",               "第{0}关完成!" },
            new[] { "btn_next_level",       "다음 레벨",              "Next Level",                "次のレベル",                     "下一关" },

            // 중간 축하 토스트
            new[] { "progress_50",          "50% 완성!",              "50% Complete!",             "50%達成!",                       "50%完成!" },
            new[] { "progress_75",          "75% 완성!",              "75% Complete!",             "75%達成!",                       "75%完成!" },
            new[] { "progress_edges",       "가장자리 완성!",          "Edges Complete!",           "外枠完成!",                       "边缘完成!" },

            // ─────────────────────────────────────────────
            // v0.6 디자인 — 신규 노출 텍스트 (2026-05-03 추가)
            // ─────────────────────────────────────────────

            // LevelSelect — Progress 영역
            new[] { "level_progress_stat",     "{0}점 / {1}점 완성",    "{0} pts / {1} pts done",     "{0}点 / {1}点達成",                "{0}分 / 共{1}分" },
            new[] { "level_milestone_next",    "다음 마일스톤까지 +{0}점", "Next milestone +{0} pts",  "次のマイルストーンまで +{0}点",       "距下个里程碑 +{0}分" },
            new[] { "level_milestone_last",    "마지막 도전",           "Final challenge",            "最後の挑戦",                       "最后挑战" },

            // LevelSelect — Section label / Current Challenge / 카드
            new[] { "level_section_all",       "전체 작품",             "All Works",                  "すべての作品",                       "全部作品" },
            new[] { "level_section_subtitle",  "순서대로 열려요",        "Unlocks in order",           "順番に解放",                         "按顺序解锁" },

            // LevelSelect — 난이도 버킷 헤더 (E/M/H/VH, 2026-08 추가)
            new[] { "level_bucket_veryeasy",   "아주 쉬움",             "Very Easy",                  "とても簡単",                         "非常简单" },
            new[] { "level_bucket_easy",       "쉬움",                  "Easy",                       "簡単",                              "简单" },
            new[] { "level_bucket_medium",     "보통",                  "Medium",                     "普通",                              "普通" },
            new[] { "level_bucket_hard",       "어려움",                "Hard",                       "難しい",                            "困难" },
            new[] { "level_bucket_veryhard",   "매우 어려움",            "Very Hard",                  "とても難しい",                       "非常困难" },
            new[] { "level_badge_now",         "지금 도전",             "Up Next",                    "今すぐ挑戦",                         "即将挑战" },
            new[] { "level_badge_all_done",    "모두 완성!",            "All Complete!",              "すべてクリア!",                      "全部完成!" },
            new[] { "level_card_replay_title", "다시 한 번 즐겨보세요",   "Enjoy it once more",         "もう一度遊んでみて",                  "再玩一次" },
            // level_card_title_format ("{0}. {1}") 구 키 — 작품명 표시용. 2026-05-03 사용자 결정으로 레벨 번호만 노출 (level_card_title_levelnum) 로 교체. 데이터 호환성 위해 보존
            new[] { "level_card_title_format", "{0}. {1}",             "{0}. {1}",                   "{0}. {1}",                          "{0}. {1}" },
            // "Level" 영문 그대로 노출 (디자인 의도 — 사용자 결정 2026-05-03)
            new[] { "level_card_title_levelnum", "Level {0}",          "Level {0}",                  "Level {0}",                         "Level {0}" },
            new[] { "level_card_meta",         "{0}조각 · 약 {1}분",    "{0} pieces · ~{1} min",      "{0}ピース · 約{1}分",                 "{0}片 · 约{1}分钟" },
            new[] { "level_label_default",     "제목",                  "Untitled",                   "タイトル",                           "标题" },
            new[] { "level_continue_btn",      "계속하기",              "Continue",                   "続ける",                             "继续" },
            new[] { "level_replay_btn",        "다시 풀기",             "Replay",                     "もう一度",                           "重玩" },
            new[] { "level_locked_v6",         "이전 작품을 먼저 완성해 주세요", "Complete the previous work first", "前の作品を先にクリアしてください", "请先完成上一幅作品" },
            new[] { "common_coming_soon",      "준비 중",               "Coming soon",                "準備中",                             "敬请期待" },

            // LevelSelect / Gallery — 공통 조각수 뱃지
            new[] { "tile_pieces_format",      "{0}조각",               "{0} pieces",                 "{0}ピース",                         "{0}片" },

            // Gallery — 헤더 / Progress / 빈 상태 / Floor
            new[] { "gallery_subtitle",        "나의 갤러리",           "My Gallery",                 "マイギャラリー",                     "我的画廊" },
            new[] { "gallery_title",           "그림 수집관",           "Painting Hall",              "絵画コレクション",                   "画作收藏馆" },
            new[] { "gallery_count_format",    "{0} / {1}",            "{0} / {1}",                  "{0} / {1}",                         "{0} / {1}" },
            new[] { "gallery_progress_until",  "다음 작품까지",         "Until next work",            "次の作品まで",                       "距下一幅作品" },
            new[] { "gallery_progress_now",    "레벨 {0} 진행중",       "Level {0} in progress",      "レベル{0}進行中",                   "第{0}关进行中" },
            new[] { "gallery_empty",           "아직 완성한 작품이 없어요", "No works completed yet",   "まだ完成した作品がありません",        "尚未完成任何作品" },
            new[] { "gallery_empty_hint",      "그림을 완성하면 갤러리에 모여요", "Completed works appear here", "完成した絵がここに集まります", "完成画作后将收藏在此" },
            new[] { "gallery_floor_first",     "FIRST FLOOR",          "FIRST FLOOR",                "FIRST FLOOR",                       "FIRST FLOOR" },
            new[] { "gallery_floor_second",    "SECOND FLOOR",         "SECOND FLOOR",               "SECOND FLOOR",                      "SECOND FLOOR" },

            // Complete — pill / CTA / 캡션
            new[] { "complete_done_title",     "완성하셨어요!",         "Beautifully done!",          "完成しました!",                      "完成了!" },
            new[] { "complete_subtitle_v6",    "나만의 갤러리에 그림 한 점이 더해졌어요", "Another work added to your gallery", "あなたのギャラリーに一作品追加されました", "您的画廊新增了一幅作品" },
            new[] { "complete_pill_clear",     "레벨 {0} 클리어",       "Level {0} Clear",            "レベル{0}クリア",                   "第{0}关通关" },
            new[] { "complete_caption_saved",  "나만의 갤러리에 저장되었어요", "Saved to your gallery", "ギャラリーに保存されました",          "已保存到您的画廊" },
            new[] { "complete_ad_notice",      "다음 그림 시작 시 짧은 광고가 나올 수 있어요", "A short ad may play before the next painting", "次の絵を始める時に短い広告が表示される場合があります", "开始下一幅画时可能会显示短广告" },
            new[] { "complete_btn_next",       "다음 그림 시작하기",     "Start Next Painting",        "次の絵を始める",                     "开始下一幅画" },
            new[] { "complete_btn_gallery",    "갤러리 보기",           "View Gallery",               "ギャラリーを見る",                   "查看画廊" },

            // Settings — v0.6 라벨
            new[] { "settings_lang_subhead",   "언어 · Language",       "Language",                   "言語 · Language",                   "语言 · Language" },
            new[] { "settings_bgm_v6",         "배경음악",              "Music",                      "BGM",                               "背景音乐" },
            new[] { "settings_version_format", "v{0} · 따뜻한 직소",    "v{0} · A warm jigsaw",       "v{0} · あたたかいジグソー",           "v{0} · 温暖拼图" },

            // BottomNav 4탭 (사용자 시안 명세 — home/daily/collection/shop). 현 v0.6 노출은 home/collection 2탭만.
            new[] { "nav_home",                "메인",                  "Main",                       "メイン",                             "主页" },
            new[] { "nav_daily",               "일일",                  "Daily",                      "デイリー",                           "每日" },
            new[] { "nav_collection",          "갤러리",                "Gallery",                    "ギャラリー",                         "画廊" },
            new[] { "nav_shop",                "상점",                  "Shop",                       "ショップ",                           "商店" },

            // LevelSelect Header — greeting + title (v0.6 누락 키 보충 2026-05-03)
            new[] { "home_greeting",           "오늘도 반가워요",         "Welcome back",               "今日もようこそ",                     "今天也欢迎您" },
            new[] { "home_title",              "그림 한 점, 시작해 볼까요", "Shall we start a painting?", "一枚、始めてみませんか",            "开始一幅画吧" },
        };

        [MenuItem("PuzzlePals/Populate String Tables")]
        public static void PopulateStringTables()
        {
            EnsureFolder(TableFolder);

            var charCol = GetOrCreateCollection(CharCollectionName);
            int charCount = PopulateCollection(charCol, CHAR_ROWS);

            var uiCol = GetOrCreateCollection(UICollectionName);
            int uiCount = PopulateCollection(uiCol, UI_ROWS);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[LocalizationStringTableTool] Populated — Characters={charCount}, UI={uiCount}");
            EditorUtility.DisplayDialog("PuzzlePals",
                "String Table 입력 완료\n\n" +
                $"  • Characters: {charCount} keys × 4 locales\n" +
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
