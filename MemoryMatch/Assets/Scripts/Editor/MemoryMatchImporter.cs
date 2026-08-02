using System.IO;
using UnityEditor;
using UnityEngine;
using MemoryMatch.Data;

namespace MemoryMatch.EditorTools
{
    /// <summary>
    /// jigsaw_source/ (직소 퍼즐 프로젝트에서 재사용하는 정사각 PNG 16장)를 스캔해서
    /// Assets/Sprites/MemoryMatch/ 로 복사+임포트하고, Assets/Resources/Levels/ 에
    /// LevelDataSO 에셋을 생성/갱신한다.
    ///
    /// 난이도 분배(순서대로 난이도 상승, LevelSelect 의 순차 언락과 맞물림 — 다른 두 게임과 동일 관례):
    ///   1~5   Easy   (4쌍)
    ///   6~11  Medium (6쌍)
    ///   12~16 Hard   (8쌍)
    ///
    /// 16장 이미지 풀에서 레벨별로 회전 오프셋을 둬서 pairCount 장씩 뽑아 쓴다 (레벨마다 다른 조합,
    /// 같은 레벨 안에서는 절대 중복 없음 — pairCount ≤ 16 이므로 mod 연산으로 보장됨).
    ///
    /// 메뉴: MemoryMatch > Import Memory Match Levels
    /// idempotent — 재실행 시 기존 에셋 값 덮어씀.
    /// </summary>
    public static class MemoryMatchImporter
    {
        private const string SourceFolder = "jigsaw_source";
        private const string SpriteFolder = "Assets/Sprites/MemoryMatch";
        private const string LevelsFolder = "Assets/Resources/Levels";

        [MenuItem("MemoryMatch/Import Memory Match Levels")]
        public static void ImportLevels()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string sourcePath  = Path.Combine(projectRoot, SourceFolder);

            if (!Directory.Exists(sourcePath))
            {
                EditorUtility.DisplayDialog("MemoryMatch — Import",
                    $"소스 폴더를 찾을 수 없습니다: {sourcePath}", "OK");
                return;
            }

            EnsureFolder(SpriteFolder);
            EnsureFolder(LevelsFolder);

            var files = Directory.GetFiles(sourcePath, "*.png");
            System.Array.Sort(files, System.StringComparer.OrdinalIgnoreCase);

            // 1) 전체 풀 임포트 (Sprite 로 강제 재설정 — 필수 스텝)
            var pool = new Sprite[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                string destPath = $"{SpriteFolder}/Level_{(i + 1):D2}.png";
                if (!File.Exists(destPath))
                    File.Copy(files[i], destPath, overwrite: true);

                AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceSynchronousImport);
                ConfigureAsSprite(destPath);

                pool[i] = AssetDatabase.LoadAssetAtPath<Sprite>(destPath);
                if (pool[i] == null)
                    Debug.LogWarning($"[MemoryMatchImporter] 스프라이트 로드 실패: {destPath}");
            }

            // 2) 레벨별 LevelDataSO 생성/갱신
            int imported = 0;
            int levelCount = files.Length; // 16
            for (int levelNumber = 1; levelNumber <= levelCount; levelNumber++)
            {
                var difficulty = DifficultyForLevel(levelNumber);
                int pairCount  = LevelDataSO.PairCountFor(difficulty);
                var cardSprites = SelectCardSprites(pool, levelNumber, pairCount);

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
                level.pairCount    = pairCount;
                level.cardSprites  = cardSprites;

                EditorUtility.SetDirty(level);
                imported++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[MemoryMatchImporter] {imported}개 레벨 임포트 완료 ({SpriteFolder}, {LevelsFolder})");
            EditorUtility.DisplayDialog("MemoryMatch — Import",
                $"{imported}개 레벨 임포트 완료\n\n소스: {sourcePath}\n출력: {SpriteFolder}, {LevelsFolder}", "OK");
        }

        // 풀에서 pairCount 장을 회전 오프셋으로 뽑음 — 레벨마다 다른 조합, 레벨 내부는 절대 중복 없음.
        private static Sprite[] SelectCardSprites(Sprite[] pool, int levelNumber, int pairCount)
        {
            var result = new Sprite[pairCount];
            int start = ((levelNumber - 1) * pairCount) % pool.Length;
            for (int i = 0; i < pairCount; i++)
                result[i] = pool[(start + i) % pool.Length];
            return result;
        }

        private static LevelDataSO.Difficulty DifficultyForLevel(int levelNumber)
        {
            if (levelNumber <= 5)  return LevelDataSO.Difficulty.Easy;
            if (levelNumber <= 11) return LevelDataSO.Difficulty.Medium;
            return LevelDataSO.Difficulty.Hard;
        }

        private static void ConfigureAsSprite(string assetPath)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (importer == null) return;
            if (importer.textureType == TextureImporterType.Sprite) return; // 이미 설정됨 — idempotent

            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled       = false;
            importer.filterMode          = FilterMode.Bilinear;
            importer.maxTextureSize      = 2048;
            importer.textureCompression  = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
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
