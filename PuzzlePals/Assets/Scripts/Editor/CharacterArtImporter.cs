using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using PuzzlePals.Data;

namespace PuzzlePals.EditorTools
{
    /// <summary>
    /// 외부 캐릭터 아트(Flaticon, game-icons.net, 자체 디자이너 등) 일괄 임포트.
    ///
    /// 사용 절차:
    ///  1) Assets/Sprites/Characters/_Source/ 폴더에 PNG 100장 복사
    ///  2) 메뉴 `PuzzlePals > Import Character Art (Auto Name Match)` 또는 `(Sequential Order)` 실행
    ///  3) 자동으로 Char_001.png ~ Char_100.png 로 복사 + Sprite 임포트 + LevelDataSO 할당
    /// </summary>
    public static class CharacterArtImporter
    {
        private const string SourceFolder       = "Assets/Sprites/Characters/_Source";
        private const string TargetFolder       = "Assets/Sprites/Characters";
        private const string LevelsResourcePath = "Assets/Resources/Levels";

        // ─────────────────────────────────────────────
        // 메뉴 1 — 파일명으로 캐릭터 매칭 (권장)
        //   예: apple.png → Char_001 (Apple), cat.png → Char_051 (Cat)
        // ─────────────────────────────────────────────

        [MenuItem("PuzzlePals/Import Character Art (Auto Name Match)")]
        public static void ImportByName()
        {
            if (!EnsureSourceFolder()) return;

            var sourceFiles = GetSourceFiles();
            if (sourceFiles.Length == 0) return;

            var levels = LoadAllLevels();
            var byKey  = levels.ToDictionary(lvl => Normalize(lvl.characterNameKey), lvl => lvl);

            int matched = 0, assigned = 0;
            var unmatched = new List<string>();

            foreach (var srcPath in sourceFiles)
            {
                string fileName  = Path.GetFileNameWithoutExtension(srcPath);
                string fileKey   = Normalize(fileName);

                LevelDataSO level = null;

                // 1) 정확 매치
                if (byKey.TryGetValue(fileKey, out level)) { /* found */ }
                // 2) substring 매치 (파일명이 캐릭터 키를 포함, 또는 그 반대)
                else
                {
                    foreach (var pair in byKey)
                    {
                        if (fileKey.Contains(pair.Key) || pair.Key.Contains(fileKey))
                        {
                            level = pair.Value;
                            break;
                        }
                    }
                }

                if (level == null)
                {
                    unmatched.Add(fileName);
                    continue;
                }

                ImportToLevel(srcPath, level);
                matched++;
                if (level.characterSprite != null) assigned++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ReportResult(matched, assigned, unmatched);
        }

        // ─────────────────────────────────────────────
        // 메뉴 2 — 알파벳 순서대로 1~100 순차 할당
        //   파일명이 generic (icon-01.png 등) 일 때 사용
        // ─────────────────────────────────────────────

        [MenuItem("PuzzlePals/Import Character Art (Sequential Order)")]
        public static void ImportSequential()
        {
            if (!EnsureSourceFolder()) return;

            var sourceFiles = GetSourceFiles();
            if (sourceFiles.Length == 0) return;

            // 파일명 알파벳 정렬 — 사용자가 파일명 prefix 로 순서 제어 가능
            System.Array.Sort(sourceFiles);

            var levels = LoadAllLevels();
            int count  = Mathf.Min(sourceFiles.Length, levels.Length);
            int assigned = 0;

            for (int i = 0; i < count; i++)
            {
                ImportToLevel(sourceFiles[i], levels[i]);
                if (levels[i].characterSprite != null) assigned++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("PuzzlePals",
                $"순차 할당 완료\n" +
                $"  • 임포트: {count} 개\n" +
                $"  • LevelDataSO 할당: {assigned} 개\n" +
                $"  • 원본 파일: {sourceFiles.Length} 개 (남은 {sourceFiles.Length - count}개는 사용 안 함)",
                "OK");
        }

        // ─────────────────────────────────────────────
        // 헬퍼
        // ─────────────────────────────────────────────

        private static void ImportToLevel(string srcPath, LevelDataSO level)
        {
            string targetPath = $"{TargetFolder}/Char_{level.levelNumber:D3}.png";

            // 같은 경로면 그냥 재임포트만
            File.Copy(srcPath, targetPath, overwrite: true);
            AssetDatabase.ImportAsset(targetPath);
            ConfigureAsSprite(targetPath);

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(targetPath);
            if (sprite != null && level.characterSprite != sprite)
            {
                level.characterSprite = sprite;
                EditorUtility.SetDirty(level);
            }
        }

        // 모든 캐릭터 스프라이트 world 폭 = 4 unit 로 통일 (PNG 사이즈 무관)
        private const float TargetWorldWidth = 4f;

        private static void ConfigureAsSprite(string assetPath)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (importer == null) return;

            int pxWidth = ReadPngWidth(assetPath);
            float ppu   = pxWidth > 0 ? pxWidth / TargetWorldWidth : 64f;

            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled       = false;
            importer.filterMode          = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }

        // PNG signature(8B) + IHDR chunk — width @ 16~19 (big-endian)
        private static int ReadPngWidth(string path)
        {
            try
            {
                using (var fs = File.OpenRead(path))
                {
                    var header = new byte[24];
                    if (fs.Read(header, 0, 24) < 24) return 0;
                    return (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
                }
            }
            catch
            {
                return 0;
            }
        }

        private static LevelDataSO[] LoadAllLevels()
        {
            return AssetDatabase.FindAssets("t:LevelDataSO", new[] { LevelsResourcePath })
                .Select(g => AssetDatabase.LoadAssetAtPath<LevelDataSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(l => l != null)
                .OrderBy(l => l.levelNumber)
                .ToArray();
        }

        private static string[] GetSourceFiles()
        {
            var files = Directory.GetFiles(SourceFolder, "*.png", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                EditorUtility.DisplayDialog("PuzzlePals",
                    $"원본 폴더에 PNG 파일이 없습니다:\n{SourceFolder}\n\n" +
                    "Flaticon 등에서 받은 파일을 이 폴더에 복사한 후 다시 실행하세요.",
                    "OK");
                return new string[0];
            }
            return files;
        }

        private static bool EnsureSourceFolder()
        {
            if (AssetDatabase.IsValidFolder(SourceFolder)) return true;

            EnsureFolder(SourceFolder);
            EditorUtility.DisplayDialog("PuzzlePals",
                $"원본 폴더 생성:\n{SourceFolder}\n\n" +
                "이 폴더에 PNG 100장을 복사한 후 메뉴를 다시 실행하세요.\n\n" +
                "권장 파일명 (Auto Name Match 시):\n" +
                "  apple.png, cat.png, dog.png, ...\n" +
                "  또는 char_apple.png, char_cat.png, ...\n\n" +
                "파일명 매칭 안 되면 'Sequential Order' 메뉴로 알파벳 순서 할당 가능.",
                "OK");
            return false;
        }

        // "char_apple" → "apple", "Ice Cream.png" → "icecream", "char_ice_cream" → "icecream"
        private static string Normalize(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            string s = raw.ToLowerInvariant();
            if (s.StartsWith("char_")) s = s.Substring(5);
            s = Regex.Replace(s, @"[^a-z0-9]", "");
            return s;
        }

        private static void ReportResult(int matched, int assigned, List<string> unmatched)
        {
            string msg = $"임포트 완료\n" +
                         $"  • 매칭/할당: {matched}/{assigned} 개\n";
            if (unmatched.Count > 0)
            {
                msg += $"  • 매칭 안 된 파일: {unmatched.Count} 개\n\n";
                msg += "이름 매칭 실패한 파일들 (10개까지 표시):\n";
                msg += string.Join("\n", unmatched.Take(10).Select(n => $"  - {n}"));
                if (unmatched.Count > 10) msg += $"\n  ... 외 {unmatched.Count - 10}개";
                msg += "\n\n매칭 안 된 파일을 사용하려면 파일명을 GDD 캐릭터 영문명으로 변경 후 재실행하거나, " +
                       "'Sequential Order' 메뉴 사용.";
            }

            EditorUtility.DisplayDialog("PuzzlePals", msg, "OK");
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
