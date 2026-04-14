using System.IO;
using UnityEditor;
using UnityEngine;
using PuzzlePals.Data;

namespace PuzzlePals.EditorTools
{
    /// <summary>
    /// GDD의 100개 캐릭터 표를 기반으로 Level_001.asset ~ Level_100.asset 를 일괄 생성한다.
    /// characterSprite 는 비워 두며, 디자이너가 추후 수동으로 할당한다.
    ///
    /// 사용법: Unity 상단 메뉴 → [PuzzlePals] → [Generate All Level Assets]
    /// </summary>
    public static class LevelAssetGenerator
    {
        private const string LevelFolder = "Assets/Resources/Levels";

        // GDD `puzzle_pals_gdd.md` 캐릭터 표(1~100). (영문 이름, 조각 수).
        private static readonly (string name, int pieces)[] Characters = new (string, int)[]
        {
            ("Apple", 2),       ("Ball", 2),        ("Sun", 2),          ("Heart", 2),       ("Star", 3),
            ("Cup", 3),         ("Balloon", 3),     ("Flower", 3),       ("House", 3),       ("Cake", 3),
            ("Car", 4),         ("Train", 4),       ("Boat", 4),         ("Airplane", 4),    ("Bicycle", 4),
            ("Umbrella", 4),    ("Rainbow", 4),     ("Ice Cream", 4),    ("Pencil", 4),      ("Book", 4),
            ("Tree", 5),        ("Kite", 5),        ("Lollipop", 5),     ("Piano", 5),       ("Rocket", 5),
            ("Camera", 5),      ("Guitar", 5),      ("Lighthouse", 5),   ("Clock", 5),       ("Castle", 6),
            ("Submarine", 6),   ("Telescope", 6),   ("Lantern", 6),      ("Tractor", 6),     ("Spaceship", 6),
            ("Bridge", 6),      ("Windmill", 6),    ("Ferris Wheel", 7), ("Hot Air Balloon", 7), ("Crown", 7),
            ("Diamond", 7),     ("Snowflake", 7),   ("Treasure Chest", 7), ("Cactus", 7),    ("Pirate Ship", 8),
            ("Robot", 8),       ("Fire Truck", 8),  ("Space Station", 8), ("Dragon Ball", 8), ("Magic Wand", 8),
            ("Cat", 2),         ("Dog", 2),         ("Duck", 2),         ("Fish", 3),        ("Chick", 3),
            ("Rabbit", 3),      ("Bear", 3),        ("Penguin", 3),      ("Pig", 3),         ("Frog", 3),
            ("Elephant", 4),    ("Giraffe", 4),     ("Lion", 4),         ("Monkey", 4),      ("Cow", 4),
            ("Horse", 4),       ("Sheep", 4),       ("Panda", 4),        ("Koala", 4),       ("Turtle", 4),
            ("Butterfly", 5),   ("Owl", 5),         ("Peacock", 5),      ("Crab", 5),        ("Octopus", 5),
            ("Flamingo", 5),    ("Hedgehog", 5),    ("Fox", 5),          ("Deer", 5),        ("Parrot", 5),
            ("Hippo", 6),       ("Crocodile", 6),   ("Polar Bear", 6),   ("Zebra", 6),       ("Gorilla", 6),
            ("Kangaroo", 6),    ("Narwhal", 6),     ("Axolotl", 6),      ("Sloth", 7),       ("Chameleon", 7),
            ("Dragon", 7),      ("Phoenix", 7),     ("Unicorn", 7),      ("Mermaid Cat", 7), ("Dinosaur", 8),
            ("Shark", 8),       ("Toucan", 8),      ("Snow Leopard", 8), ("Giant Squid", 8), ("Rainbow Dragon", 8),
        };

        [MenuItem("PuzzlePals/Generate All Level Assets")]
        public static void GenerateAll()
        {
            EnsureFolder(LevelFolder);

            int created = 0;
            int updated = 0;

            try
            {
                AssetDatabase.StartAssetEditing();

                for (int i = 0; i < Characters.Length; i++)
                {
                    int levelNumber = i + 1;
                    var (englishName, pieces) = Characters[i];
                    string assetPath = $"{LevelFolder}/Level_{levelNumber:D3}.asset";

                    var data = AssetDatabase.LoadAssetAtPath<LevelDataSO>(assetPath);
                    bool isNew = data == null;
                    if (isNew)
                    {
                        data = ScriptableObject.CreateInstance<LevelDataSO>();
                    }

                    data.levelNumber      = levelNumber;
                    data.characterNameKey = "char_" + ToSnakeCase(englishName);
                    data.characterSprite  = null;
                    data.pieceCount       = pieces;
                    data.snapThreshold    = ResolveSnapThreshold(levelNumber);
                    data.enablePieceDrift = levelNumber >= 81;
                    data.backgroundColor  = ResolvePastelColor(levelNumber);

                    if (isNew)
                    {
                        AssetDatabase.CreateAsset(data, assetPath);
                        created++;
                    }
                    else
                    {
                        EditorUtility.SetDirty(data);
                        updated++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log($"[PuzzlePals] Level 에셋 생성 완료. 신규={created}, 갱신={updated}, 경로={LevelFolder}");
            EditorUtility.DisplayDialog(
                "PuzzlePals",
                $"Level 에셋 생성 완료\n신규: {created}개\n갱신: {updated}개\n경로: {LevelFolder}",
                "OK");
        }

        // GDD Difficulty Curve 표 기준 스냅 민감도
        private static float ResolveSnapThreshold(int level)
        {
            if (level <= 10) return 1.8f;
            if (level <= 25) return 1.4f;
            if (level <= 45) return 1.2f;
            if (level <= 60) return 1.0f;
            if (level <= 80) return 0.9f;
            return 0.8f;
        }

        // 파스텔 단색 — 낮은 채도, 높은 명도. 황금각(137.5°)으로 인접 레벨 색이 충분히 분리되도록.
        private static Color ResolvePastelColor(int level)
        {
            float hue = ((level - 1) * 137.5f % 360f) / 360f;
            return Color.HSVToRGB(hue, 0.22f, 1.0f);
        }

        private static string ToSnakeCase(string englishName)
        {
            return englishName.Trim().ToLowerInvariant().Replace(' ', '_');
        }

        private static void EnsureFolder(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder)) return;

            string parent = Path.GetDirectoryName(assetFolder).Replace('\\', '/');
            string leaf   = Path.GetFileName(assetFolder);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
