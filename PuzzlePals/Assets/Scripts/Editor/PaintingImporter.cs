using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using PuzzlePals.Data;

namespace PuzzlePals.EditorTools
{
    /// <summary>
    /// 성인 퍼즐 피벗 — 16 레벨 정사각 이미지 임포트 + LevelDataSO 생성.
    ///
    /// 사용 절차:
    ///  1) Assets/Sprites/Paintings/_Source/{easy|medium|hard|very_hard}/ 에 정사각 PNG 16장.
    ///  2) 메뉴 `PuzzlePals/Adult — Import Paintings (16 Levels)` 실행.
    ///  3) 자동 처리:
    ///     - Assets/Sprites/Paintings/Level_01.png ~ Level_16.png 로 정규 이름 복사 + Sprite 설정
    ///     - Assets/Resources/Levels/Level_001.asset ~ Level_016.asset 생성/갱신
    ///     - 구버전 Level_017.asset ~ Level_100.asset 삭제 (16레벨 체계)
    ///
    /// Idempotent — 재실행 안전. 기존 asset 있으면 갱신, 없으면 생성.
    /// 메타 (Artist/Title/Year/TitleKey) 는 빈값 — UI 에서 사용 안 함.
    /// </summary>
    public static class PaintingImporter
    {
        private const string SourceFolder       = "Assets/Sprites/Paintings/_Source";
        private const string TargetFolder       = "Assets/Sprites/Paintings";
        private const string LevelsResourcePath = "Assets/Resources/Levels";

        // 퍼즐 이미지 월드 폭 (유닛). 카메라·보드 크기와 연동해 조정.
        private const float TargetWorldWidth = 10f;

        // 모바일 안전 최대 텍스처 크기
        private const int MaxTextureSize = 2048;

        // ─────────────────────────────────────────────
        // 16 레벨 매핑 (정사각 PNG)
        // ─────────────────────────────────────────────

        private struct Painting
        {
            public int    Level;
            public LevelDataSO.ImageBucket Bucket;
            public int    Pieces;
            public string SourceRelative; // "{bucket}/N.png"
        }

        private static readonly Painting[] Paintings = new[]
        {
            // Easy: 36 조각 (6×6 정사각 셀)
            P( 1, LevelDataSO.ImageBucket.Easy,     36, "easy/1.png"),
            P( 2, LevelDataSO.ImageBucket.Easy,     36, "easy/2.png"),
            P( 3, LevelDataSO.ImageBucket.Easy,     36, "easy/3.png"),

            // Medium: 36 ↔ 64 교차 (6×6 / 8×8 정사각 셀)
            P( 4, LevelDataSO.ImageBucket.Medium,   36, "medium/4.png"),
            P( 5, LevelDataSO.ImageBucket.Medium,   64, "medium/5.png"),
            P( 6, LevelDataSO.ImageBucket.Medium,   36, "medium/6.png"),
            P( 7, LevelDataSO.ImageBucket.Medium,   64, "medium/7.png"),
            P( 8, LevelDataSO.ImageBucket.Medium,   36, "medium/8.png"),
            P( 9, LevelDataSO.ImageBucket.Medium,   64, "medium/9.png"),

            // Hard: 64 조각 (8×8 정사각 셀)
            P(10, LevelDataSO.ImageBucket.Hard,     64, "hard/10.png"),
            P(11, LevelDataSO.ImageBucket.Hard,     64, "hard/11.png"),
            P(12, LevelDataSO.ImageBucket.Hard,     64, "hard/12.png"),
            P(13, LevelDataSO.ImageBucket.Hard,     64, "hard/13.png"),

            // Very Hard: 36 조각 (6×6 — 이미지 복잡도로 난이도 표현)
            P(14, LevelDataSO.ImageBucket.VeryHard, 36, "very_hard/14.png"),
            P(15, LevelDataSO.ImageBucket.VeryHard, 36, "very_hard/15.png"),
            P(16, LevelDataSO.ImageBucket.VeryHard, 36, "very_hard/16.png"),
        };

        private static Painting P(int level, LevelDataSO.ImageBucket bucket, int pieces, string src)
            => new Painting { Level = level, Bucket = bucket, Pieces = pieces, SourceRelative = src };

        [MenuItem("PuzzlePals/Adult — Import Paintings (16 Levels)")]
        public static void ImportPaintings()
        {
            if (!AssetDatabase.IsValidFolder(SourceFolder))
            {
                EditorUtility.DisplayDialog("PuzzlePals",
                    $"소스 폴더가 없습니다:\n{SourceFolder}\n\n" +
                    "정사각 PNG 16장이 아래 구조로 있어야 합니다:\n" +
                    "  _Source/easy/1.png ~ 3.png      (3장)\n" +
                    "  _Source/medium/4.png ~ 9.png    (6장)\n" +
                    "  _Source/hard/10.png ~ 13.png    (4장)\n" +
                    "  _Source/very_hard/14.png ~ 16.png (3장)",
                    "OK");
                return;
            }

            EnsureFolder(TargetFolder);
            EnsureFolder(LevelsResourcePath);

            var missing = new List<string>();
            int importedImages = 0, createdSO = 0, updatedSO = 0, deletedLegacy = 0;

            // ── Phase 1: 20장 이미지 복사 + Sprite 설정 ──
            // StartAssetEditing 블록 내부에서 LoadAssetAtPath<Sprite> 가 신규 import된 서브에셋을
            // 반환하지 못하는 이슈 때문에 phase 를 나눠 배치 간 reimport 를 확정시킴.
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var p in Paintings)
                {
                    string srcPath = $"{SourceFolder}/{p.SourceRelative}";
                    if (!File.Exists(srcPath))
                    {
                        missing.Add(p.SourceRelative);
                        continue;
                    }
                    string ext        = Path.GetExtension(srcPath).ToLowerInvariant();
                    string targetPath = $"{TargetFolder}/Level_{p.Level:D2}{ext}";

                    File.Copy(srcPath, targetPath, overwrite: true);
                    AssetDatabase.ImportAsset(targetPath);
                    ConfigureAsSprite(targetPath);

                    importedImages++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            // ── Phase 2: LevelDataSO 생성/갱신 + 구 레벨 삭제 ──
            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (var p in Paintings)
                {
                    string assetPath  = $"{LevelsResourcePath}/Level_{p.Level:D3}.asset";
                    string ext        = Path.GetExtension($"{SourceFolder}/{p.SourceRelative}").ToLowerInvariant();
                    string spritePath = $"{TargetFolder}/Level_{p.Level:D2}{ext}";

                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

                    var data  = AssetDatabase.LoadAssetAtPath<LevelDataSO>(assetPath);
                    bool isNew = data == null;
                    if (isNew)
                    {
                        data = ScriptableObject.CreateInstance<LevelDataSO>();
                    }

                    data.levelNumber      = p.Level;
                    data.characterNameKey = "";       // 메타 미사용 (성인 피벗 v0.5)
                    data.characterSprite  = sprite;
                    data.pieceCount       = p.Pieces;
                    data.artistName       = "";       // 메타 미사용
                    data.year             = 0;        // 메타 미사용
                    data.imageBucket      = p.Bucket;
                    // 레거시 필드(snapThreshold/enablePieceDrift/backgroundColor)는 건드리지 않음 — 기존 값 보존

                    if (isNew)
                    {
                        AssetDatabase.CreateAsset(data, assetPath);
                        createdSO++;
                    }
                    else
                    {
                        EditorUtility.SetDirty(data);
                        updatedSO++;
                    }
                }

                for (int i = 17; i <= 100; i++)
                {
                    string legacyPath = $"{LevelsResourcePath}/Level_{i:D3}.asset";
                    if (AssetDatabase.LoadAssetAtPath<LevelDataSO>(legacyPath) != null)
                    {
                        AssetDatabase.DeleteAsset(legacyPath);
                        deletedLegacy++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            string report = $"임포트 완료\n" +
                            $"  • 이미지 복사: {importedImages}/16 장\n" +
                            $"  • LevelDataSO 신규: {createdSO} / 갱신: {updatedSO}\n" +
                            $"  • 구 레벨 삭제 (17~100): {deletedLegacy} 개";

            if (missing.Count > 0)
            {
                report += $"\n\n⚠ 누락 원본 {missing.Count} 개:\n  - " + string.Join("\n  - ", missing);
            }

            Debug.Log($"[PuzzlePals] {report}");
            EditorUtility.DisplayDialog("PuzzlePals — Adult Paintings", report, "OK");
        }

        // ─────────────────────────────────────────────
        // 헬퍼
        // ─────────────────────────────────────────────

        private static void ConfigureAsSprite(string assetPath)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (importer == null) return;

            int pxWidth = ReadImageWidth(assetPath);
            float ppu   = pxWidth > 0 ? pxWidth / TargetWorldWidth : 100f;

            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled       = true;
            importer.filterMode          = FilterMode.Bilinear;
            importer.maxTextureSize      = MaxTextureSize;
            importer.textureCompression  = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        // PNG / JPEG width 추출 (간이). 실패 시 0.
        private static int ReadImageWidth(string path)
        {
            try
            {
                using (var fs = File.OpenRead(path))
                {
                    int b0 = fs.ReadByte();
                    int b1 = fs.ReadByte();
                    if (b0 < 0 || b1 < 0) return 0;

                    // PNG: 89 50 ...  → width @ offset 16 (big-endian)
                    if (b0 == 0x89 && b1 == 0x50)
                    {
                        fs.Position = 16;
                        var buf = new byte[4];
                        if (fs.Read(buf, 0, 4) < 4) return 0;
                        return (buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3];
                    }

                    // JPEG: FFD8 → SOF 마커 스캔
                    if (b0 == 0xFF && b1 == 0xD8)
                    {
                        return ReadJpegWidth(fs);
                    }

                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private static int ReadJpegWidth(FileStream fs)
        {
            while (fs.Position < fs.Length - 1)
            {
                int m1 = fs.ReadByte();
                if (m1 != 0xFF) continue;
                int m2 = fs.ReadByte();

                // SOF0-3, SOF5-7, SOF9-11, SOF13-15 제외 (C4=DHT, C8=JPG ext, CC=DAC)
                bool isSOF = (m2 >= 0xC0 && m2 <= 0xCF) && m2 != 0xC4 && m2 != 0xC8 && m2 != 0xCC;
                if (isSOF)
                {
                    fs.Position += 3; // segment length(2) + sample precision(1) 스킵
                    int hHi = fs.ReadByte();
                    int hLo = fs.ReadByte();
                    int wHi = fs.ReadByte();
                    int wLo = fs.ReadByte();
                    if (wHi < 0 || wLo < 0) return 0;
                    return (wHi << 8) | wLo;
                }

                if (m2 >= 0xD0 && m2 <= 0xD9) continue; // RST/SOI/EOI — 길이 없음

                int lenHi = fs.ReadByte();
                int lenLo = fs.ReadByte();
                if (lenHi < 0 || lenLo < 0) return 0;
                int segLen = ((lenHi << 8) | lenLo) - 2;
                fs.Position += segLen;
            }
            return 0;
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
