using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using SpotDiff.Data;

namespace SpotDiff.EditorTools
{
    /// <summary>
    /// diff_game_source/ (프로젝트 루트, Assets 밖) 에 있는 원본/변형 이미지 쌍 + diff_result.json 을
    /// LevelDataSO 에셋으로 임포트.
    ///
    /// 소스 구조 (사용자가 준비):
    ///   diff_game_source/
    ///     easy_1_diff_result.json / medium_1_diff_result.json / hard_1_diff_result.json
    ///     easy/easy_1_origin.png, easy_1_transform.png
    ///     medium/medium_1_origin.png, medium_1_transfrom.png   ← 원본 파일명 오타 그대로 (transfrom)
    ///     hard/hard_1_origin.png, hard_1_transform.png
    ///
    /// diff_regions 중 confirmed:true 인 항목만 정답 좌표로 반입 (false 는 노이즈로 확인된 것 — 무시).
    ///
    /// Idempotent — 재실행 안전 (기존 에셋 있으면 갱신).
    /// </summary>
    public static class DiffGameImporter
    {
        private const string SourceFolderName    = "diff_game_source"; // 프로젝트 루트 기준
        private const string TargetSpriteFolder  = "Assets/Sprites/DiffGame";
        private const string LevelsResourcePath  = "Assets/Resources/Levels";

        private struct LevelSource
        {
            public int Level;
            public LevelDataSO.Difficulty Difficulty;
            public string OriginFile;
            public string TransformFile;
            public string JsonFile;
        }

        // 지금은 난이도별 1장씩 — 나중에 늘어나면 이 배열에 항목만 추가.
        private static readonly LevelSource[] Sources =
        {
            new LevelSource
            {
                Level = 1, Difficulty = LevelDataSO.Difficulty.Easy,
                OriginFile = "easy/easy_1_origin.png", TransformFile = "easy/easy_1_transform.png",
                JsonFile = "easy_1_diff_result.json",
            },
            new LevelSource
            {
                Level = 2, Difficulty = LevelDataSO.Difficulty.Medium,
                OriginFile = "medium/medium_1_origin.png", TransformFile = "medium/medium_1_transfrom.png",
                JsonFile = "medium_1_diff_result.json",
            },
            new LevelSource
            {
                Level = 3, Difficulty = LevelDataSO.Difficulty.Hard,
                OriginFile = "hard/hard_1_origin.png", TransformFile = "hard/hard_1_transform.png",
                JsonFile = "hard_1_diff_result.json",
            },
        };

        [MenuItem("SpotDiff/Import Diff Game Levels")]
        public static void Import()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string sourceRoot  = Path.Combine(projectRoot, SourceFolderName);

            if (!Directory.Exists(sourceRoot))
            {
                EditorUtility.DisplayDialog("SpotDiff Importer",
                    $"소스 폴더가 없습니다:\n{sourceRoot}", "OK");
                return;
            }

            EnsureFolder(TargetSpriteFolder);
            EnsureFolder(LevelsResourcePath);

            var missing = new List<string>();
            int importedPairs = 0;

            // ── Phase 1: 이미지 복사 + Sprite 설정 ──
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var s in Sources)
                {
                    string originSrc    = Path.Combine(sourceRoot, s.OriginFile);
                    string transformSrc = Path.Combine(sourceRoot, s.TransformFile);
                    string jsonSrc      = Path.Combine(sourceRoot, s.JsonFile);

                    if (!File.Exists(originSrc) || !File.Exists(transformSrc) || !File.Exists(jsonSrc))
                    {
                        missing.Add($"Level {s.Level} ({s.Difficulty})");
                        continue;
                    }

                    string originTarget    = $"{TargetSpriteFolder}/Level_{s.Level:D2}_origin.png";
                    string transformTarget = $"{TargetSpriteFolder}/Level_{s.Level:D2}_transform.png";

                    File.Copy(originSrc, originTarget, overwrite: true);
                    File.Copy(transformSrc, transformTarget, overwrite: true);
                    AssetDatabase.ImportAsset(originTarget, ImportAssetOptions.ForceSynchronousImport);
                    AssetDatabase.ImportAsset(transformTarget, ImportAssetOptions.ForceSynchronousImport);
                    ConfigureAsSprite(originTarget);
                    ConfigureAsSprite(transformTarget);

                    importedPairs++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            // ── Phase 2: JSON 파싱 + LevelDataSO 생성/갱신 ──
            int created = 0, updated = 0;
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var s in Sources)
                {
                    string jsonSrc = Path.Combine(sourceRoot, s.JsonFile);
                    if (!File.Exists(jsonSrc)) continue;

                    var parsed = JsonUtility.FromJson<DiffResultJson>(File.ReadAllText(jsonSrc));
                    if (parsed == null)
                    {
                        Debug.LogError($"[SpotDiff] JSON 파싱 실패: {jsonSrc}");
                        continue;
                    }

                    var confirmed = new List<DiffRegion>();
                    if (parsed.diff_regions != null)
                    {
                        foreach (var r in parsed.diff_regions)
                        {
                            if (!r.confirmed) continue; // 노이즈로 확인된 항목 제외
                            confirmed.Add(new DiffRegion
                            {
                                id = r.id,
                                x = r.x, y = r.y, width = r.width, height = r.height,
                                centerX = r.center_x, centerY = r.center_y,
                            });
                        }
                    }

                    string originTarget    = $"{TargetSpriteFolder}/Level_{s.Level:D2}_origin.png";
                    string transformTarget = $"{TargetSpriteFolder}/Level_{s.Level:D2}_transform.png";
                    var originSprite    = AssetDatabase.LoadAssetAtPath<Sprite>(originTarget);
                    var transformSprite = AssetDatabase.LoadAssetAtPath<Sprite>(transformTarget);

                    string assetPath = $"{LevelsResourcePath}/Level_{s.Level:D3}.asset";
                    var data   = AssetDatabase.LoadAssetAtPath<LevelDataSO>(assetPath);
                    bool isNew = data == null;
                    if (isNew) data = ScriptableObject.CreateInstance<LevelDataSO>();

                    data.levelNumber       = s.Level;
                    data.difficulty        = s.Difficulty;
                    data.originalSprite    = originSprite;
                    data.transformedSprite = transformSprite;
                    data.sourceImageWidth  = parsed.image_width;
                    data.sourceImageHeight = parsed.image_height;
                    data.diffRegions       = confirmed.ToArray();

                    if (isNew) { AssetDatabase.CreateAsset(data, assetPath); created++; }
                    else       { EditorUtility.SetDirty(data); updated++; }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            string report = $"임포트 완료 — 이미지 {importedPairs}쌍, LevelDataSO 신규 {created} / 갱신 {updated}";
            if (missing.Count > 0)
                report += $"\n\n누락:\n  - " + string.Join("\n  - ", missing);

            Debug.Log($"[SpotDiff] {report}");
            EditorUtility.DisplayDialog("SpotDiff Importer", report, "OK");
        }

        private static void ConfigureAsSprite(string assetPath)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (importer == null) return;

            importer.textureType        = TextureImporterType.Sprite;
            importer.spriteImportMode   = SpriteImportMode.Single;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled      = false; // UI Image 표시용 — 밉맵 불필요
            importer.filterMode         = FilterMode.Bilinear;
            importer.maxTextureSize     = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            string leaf    = Path.GetFileName(folder);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        // ── diff_result.json DTO (JsonUtility 매핑용 — 필드명이 곧 JSON 키) ──

        [System.Serializable]
        private class DiffResultJson
        {
            public string difficulty;
            public int image_width;
            public int image_height;
            public DiffRegionJson[] diff_regions;
            public int confirmed_count;
        }

        [System.Serializable]
        private class DiffRegionJson
        {
            public int id;
            public float x, y, width, height;
            public float center_x, center_y;
            public bool confirmed;
        }
    }
}
