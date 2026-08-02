using System.IO;
using UnityEditor;
using UnityEngine;
using SlidingPuzzle.Data;

namespace SlidingPuzzle.EditorTools
{
    /// <summary>
    /// jigsaw_source/ (직소 퍼즐 프로젝트에서 재사용하는 정사각 PNG 16장)를 스캔해서
    /// Assets/Sprites/SlidingPuzzle/ 로 복사+임포트하고, Assets/Resources/Levels/ 에
    /// LevelDataSO 에셋을 생성/갱신한다.
    ///
    /// 난이도 분배(순서대로 난이도 상승, LevelSelect 의 순차 언락과 맞물림):
    ///   1~5   Easy   (3×3)
    ///   6~11  Medium (4×4)
    ///   12~16 Hard   (5×5)
    ///
    /// 메뉴: SlidingPuzzle > Import Sliding Puzzle Levels
    /// idempotent — 재실행 시 기존 에셋 값 덮어씀.
    /// </summary>
    public static class SlidingPuzzleImporter
    {
        private const string SourceFolder = "jigsaw_source";
        private const string SpriteFolder = "Assets/Sprites/SlidingPuzzle";
        private const string LevelsFolder = "Assets/Resources/Levels";

        [MenuItem("SlidingPuzzle/Import Sliding Puzzle Levels")]
        public static void ImportLevels()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string sourcePath  = Path.Combine(projectRoot, SourceFolder);

            if (!Directory.Exists(sourcePath))
            {
                EditorUtility.DisplayDialog("SlidingPuzzle — Import",
                    $"소스 폴더를 찾을 수 없습니다: {sourcePath}", "OK");
                return;
            }

            EnsureFolder(SpriteFolder);
            EnsureFolder(LevelsFolder);

            var files = Directory.GetFiles(sourcePath, "*.png");
            System.Array.Sort(files, System.StringComparer.OrdinalIgnoreCase);

            int imported = 0;
            for (int i = 0; i < files.Length; i++)
            {
                int levelNumber = i + 1;
                string destPath = $"{SpriteFolder}/Level_{levelNumber:D2}.png";

                if (!File.Exists(destPath))
                    File.Copy(files[i], destPath, overwrite: true);

                AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceSynchronousImport);
                ConfigureAsSprite(destPath);

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(destPath);
                if (sprite == null)
                {
                    Debug.LogWarning($"[SlidingPuzzleImporter] 스프라이트 로드 실패: {destPath}");
                    continue;
                }

                var difficulty = DifficultyForLevel(levelNumber);
                string assetPath = $"{LevelsFolder}/Level_{levelNumber:D3}.asset";

                var level = AssetDatabase.LoadAssetAtPath<LevelDataSO>(assetPath);
                bool isNew = level == null;
                if (isNew)
                {
                    level = ScriptableObject.CreateInstance<LevelDataSO>();
                    AssetDatabase.CreateAsset(level, assetPath);
                }

                level.levelNumber  = levelNumber;
                level.difficulty   = difficulty;
                level.sourceSprite = sprite;
                level.gridSize     = LevelDataSO.GridSizeFor(difficulty);

                EditorUtility.SetDirty(level);
                imported++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SlidingPuzzleImporter] {imported}개 레벨 임포트 완료 ({SpriteFolder}, {LevelsFolder})");
            EditorUtility.DisplayDialog("SlidingPuzzle — Import",
                $"{imported}개 레벨 임포트 완료\n\n소스: {sourcePath}\n출력: {SpriteFolder}, {LevelsFolder}", "OK");
        }

        private static void ConfigureAsSprite(string assetPath)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (importer == null) return;
            if (importer.textureType == TextureImporterType.Sprite) return; // 이미 설정됨 — idempotent

            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled       = false; // 타일 크롭용 — 밉맵 불필요
            importer.filterMode          = FilterMode.Bilinear;
            importer.maxTextureSize      = 2048;
            importer.textureCompression  = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static LevelDataSO.Difficulty DifficultyForLevel(int levelNumber)
        {
            if (levelNumber <= 5)  return LevelDataSO.Difficulty.Easy;
            if (levelNumber <= 11) return LevelDataSO.Difficulty.Medium;
            return LevelDataSO.Difficulty.Hard;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            string leaf   = Path.GetFileName(folder);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
