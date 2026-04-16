using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using PuzzlePals.Data;
using PuzzlePals.Gameplay;
using PuzzlePals.Puzzle;

namespace PuzzlePals.EditorTools
{
    /// <summary>
    /// 100레벨 실제 플로우를 한 번에 연결하는 파이프라인 도구.
    ///   1) 플레이스홀더 캐릭터 스프라이트 100개 생성 + LevelDataSO 자동 할당
    ///   2) PuzzlePiece / SnapZone 프리팹 생성
    ///   3) Gameplay 씬을 GameplayLoader-driven 구조로 마이그레이션
    ///   4) 콤보 메뉴: 위 1~3 + FX Setup 까지 한 번에
    ///
    /// 모든 단계 idempotent — 재실행해도 안전.
    /// </summary>
    public static class RealLevelsPipelineTool
    {
        private const string SpriteFolder           = "Assets/Sprites/Characters";
        private const string PrefabFolder           = "Assets/Prefabs";
        private const string LevelsResourcePath     = "Assets/Resources/Levels";
        private const string PuzzlePiecePrefabPath  = "Assets/Prefabs/PuzzlePiece.prefab";
        private const string SnapZonePrefabPath     = "Assets/Prefabs/SnapZone.prefab";
        private const string GameplayScenePath      = "Assets/Scenes/Gameplay.unity";

        // ─────────────────────────────────────────────
        // 1. 플레이스홀더 캐릭터 스프라이트 100개
        // ─────────────────────────────────────────────

        [MenuItem("PuzzlePals/Generate Placeholder Character Sprites")]
        public static void GeneratePlaceholderSprites() => GeneratePlaceholderSpritesImpl(forceRegenerate: false);

        [MenuItem("PuzzlePals/Regenerate Placeholder Character Sprites (Force)")]
        public static void RegeneratePlaceholderSpritesForce() => GeneratePlaceholderSpritesImpl(forceRegenerate: true);

        private static void GeneratePlaceholderSpritesImpl(bool forceRegenerate)
        {
            EnsureFolder(SpriteFolder);

            var guids = AssetDatabase.FindAssets("t:LevelDataSO", new[] { LevelsResourcePath });
            int created = 0, regenerated = 0, assigned = 0, skipped = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<LevelDataSO>(path);
                if (data == null) continue;

                string spritePath = $"{SpriteFolder}/Char_{data.levelNumber:D3}.png";
                bool existed = File.Exists(spritePath);

                if (!existed || forceRegenerate)
                {
                    WritePlaceholderPng(spritePath, data.levelNumber, data.characterNameKey);
                    AssetDatabase.ImportAsset(spritePath);
                    ConfigureAsSprite(spritePath);
                    if (existed) regenerated++; else created++;
                }

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

                // 보수적 정책: 이미 다른 스프라이트가 할당돼 있으면 건드리지 않음 (디자이너 작업물 보호)
                if (sprite != null && data.characterSprite == null)
                {
                    data.characterSprite = sprite;
                    EditorUtility.SetDirty(data);
                    assigned++;
                }
                else if (data.characterSprite != null && data.characterSprite != sprite)
                {
                    skipped++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[RealLevelsPipelineTool] Placeholder sprites — created={created}, regenerated={regenerated}, assigned={assigned}, skipped={skipped}");
            EditorUtility.DisplayDialog("PuzzlePals",
                $"플레이스홀더 스프라이트\n" +
                $"  • 신규: {created}\n" +
                $"  • 재생성: {regenerated}\n" +
                $"  • LevelDataSO 자동 할당: {assigned}\n" +
                $"  • 건너뜀(이미 다른 sprite): {skipped}\n" +
                $"  • 경로: {SpriteFolder}",
                "OK");
        }

        // ─────────────────────────────────────────────
        // 도형 라이브러리 — 캐릭터 이름 기반 10종 배정
        // (실제 캐릭터 아트 도착 전 임시 다양성 확보)
        // ─────────────────────────────────────────────

        private enum PlaceholderShape { Circle, Square, Triangle, Diamond, Star, Heart, Flower, Cross, Cloud, Moon }

        // GDD 캐릭터 이름 키워드 매칭 + 해시 폴백
        private static PlaceholderShape ShapeFor(string characterNameKey)
        {
            string n = (characterNameKey ?? "").ToLowerInvariant();

            // 의미 매칭 — 이름 자체가 도형이면 그 도형
            if (n.Contains("star"))      return PlaceholderShape.Star;
            if (n.Contains("heart"))     return PlaceholderShape.Heart;
            if (n.Contains("diamond"))   return PlaceholderShape.Diamond;
            if (n.Contains("flower"))    return PlaceholderShape.Flower;
            if (n.Contains("cloud"))     return PlaceholderShape.Cloud;
            if (n.Contains("moon"))      return PlaceholderShape.Moon;
            if (n.Contains("snowflake")) return PlaceholderShape.Cross;
            if (n.Contains("ball") || n.Contains("apple") || n.Contains("sun") || n.Contains("balloon"))
                return PlaceholderShape.Circle;
            if (n.Contains("book") || n.Contains("box") || n.Contains("chest") || n.Contains("station"))
                return PlaceholderShape.Square;
            if (n.Contains("kite") || n.Contains("crown"))
                return PlaceholderShape.Triangle;

            // 해시 폴백 — 같은 이름은 항상 같은 도형
            int h = 0;
            foreach (var c in n) h = h * 31 + c;
            int idx = (h % 10 + 10) % 10;
            return (PlaceholderShape)idx;
        }

        private static void WritePlaceholderPng(string path, int levelNumber, string characterNameKey)
        {
            // 1순위: CharacterArtGenerator (25개 캐릭터 직접 그리기, 384x384)
            int size = CharacterArtGenerator.Size;
            var pixels = new Color32[size * size];
            // 모두 투명으로 초기화
            var clear = new Color32(0, 0, 0, 0);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            bool drewSpecific = CharacterArtGenerator.DrawCharacter(characterNameKey, pixels);

            // 2순위 폴백: 도형 라이브러리 (10종) — 기존 동작
            if (!drewSpecific)
            {
                size = 256; // 폴백은 작은 사이즈
                pixels = new Color32[size * size];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

                float hue = ((levelNumber - 1) * 137.5f % 360f) / 360f;
                var fill  = (Color32)Color.HSVToRGB(hue, 0.7f, 0.95f);
                var shape = ShapeFor(characterNameKey);

                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        pixels[y * size + x] = IsInsideShape(x, y, size, shape) ? fill : clear;
            }

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.SetPixels32(pixels);
            tex.Apply();

            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        // 도형별 픽셀 내부/외부 판정 — 모두 256x256 중앙 정렬, 패딩 12px
        private static bool IsInsideShape(int x, int y, int size, PlaceholderShape shape)
        {
            float cx = size * 0.5f, cy = size * 0.5f;
            float r  = size * 0.5f - 12f;
            float dx = x - cx, dy = y - cy;

            switch (shape)
            {
                case PlaceholderShape.Circle:
                    return dx * dx + dy * dy <= r * r;

                case PlaceholderShape.Square:
                    return Mathf.Abs(dx) <= r && Mathf.Abs(dy) <= r;

                case PlaceholderShape.Triangle:
                {
                    if (dy < -r || dy > r) return false;
                    float t = (r - dy) / (2f * r);   // 0=top, 1=bottom
                    float halfW = r * t;
                    return Mathf.Abs(dx) <= halfW;
                }

                case PlaceholderShape.Diamond:
                    return Mathf.Abs(dx) + Mathf.Abs(dy) <= r;

                case PlaceholderShape.Star:
                {
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx) - Mathf.PI * 0.5f;  // 위쪽 꼭지점
                    float wave  = Mathf.Cos(5f * angle);                  // -1..1, 5 spike
                    float s     = (wave + 1f) * 0.5f;                     // 0..1
                    float rAt   = Mathf.Lerp(r * 0.4f, r, s);
                    return dist <= rAt;
                }

                case PlaceholderShape.Heart:
                {
                    // 정규화된 좌표에서 하트 음함수: (x²+y²-1)³ - x²y³ ≤ 0
                    float nx = dx / r;
                    float ny = -dy / r;  // y 뒤집어서 끝부분이 아래쪽
                    float a  = nx * nx + ny * ny - 1f;
                    return (a * a * a - nx * nx * ny * ny * ny) <= 0f;
                }

                case PlaceholderShape.Flower:
                {
                    float petalR  = r * 0.4f;
                    float centerR = r * 0.32f;
                    if (dx * dx + dy * dy <= centerR * centerR) return true;
                    for (int i = 0; i < 5; i++)
                    {
                        float ang = i * (Mathf.PI * 2f / 5f) - Mathf.PI * 0.5f;
                        float px  = Mathf.Cos(ang) * (r - petalR);
                        float py  = Mathf.Sin(ang) * (r - petalR);
                        float fdx = dx - px, fdy = dy - py;
                        if (fdx * fdx + fdy * fdy <= petalR * petalR) return true;
                    }
                    return false;
                }

                case PlaceholderShape.Cross:
                {
                    float armW = r * 0.32f;
                    return (Mathf.Abs(dx) <= armW && Mathf.Abs(dy) <= r)
                        || (Mathf.Abs(dy) <= armW && Mathf.Abs(dx) <= r);
                }

                case PlaceholderShape.Cloud:
                {
                    // 하단 큰 원 3개 + 상단 작은 원 2개
                    float baseY = -r * 0.25f;
                    float topY  =  r * 0.25f;
                    float cr    =  r * 0.5f;
                    float ct    =  r * 0.4f;
                    if (InCircle(dx + r * 0.5f, dy - baseY, cr)) return true;
                    if (InCircle(dx,            dy - baseY, cr)) return true;
                    if (InCircle(dx - r * 0.5f, dy - baseY, cr)) return true;
                    if (InCircle(dx + r * 0.25f, dy - topY, ct)) return true;
                    if (InCircle(dx - r * 0.25f, dy - topY, ct)) return true;
                    return false;
                }

                case PlaceholderShape.Moon:
                {
                    // 큰 원에서 오프셋된 작은 원을 빼는 초승달
                    bool inOuter   = dx * dx + dy * dy <= r * r;
                    float odx      = dx - r * 0.4f;
                    bool inOffset  = odx * odx + dy * dy <= (r * 0.85f) * (r * 0.85f);
                    return inOuter && !inOffset;
                }

                default:
                    return dx * dx + dy * dy <= r * r;
            }
        }

        private static bool InCircle(float dx, float dy, float r) => dx * dx + dy * dy <= r * r;

        // 모든 캐릭터 스프라이트의 world 폭을 일정하게 유지 (4 unit) — PNG 사이즈와 무관
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

        // PNG signature 8바이트 + IHDR chunk 시작 — width 는 16~19바이트 (big-endian)
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

        // ─────────────────────────────────────────────
        // 2. PuzzlePiece / SnapZone 프리팹
        // ─────────────────────────────────────────────

        [MenuItem("PuzzlePals/Generate Puzzle Prefabs")]
        public static void GeneratePuzzlePrefabs()
        {
            EnsureFolder(PrefabFolder);
            var piece = LoadOrCreatePiecePrefab();
            var zone  = LoadOrCreateZonePrefab();

            Debug.Log($"[RealLevelsPipelineTool] Prefabs ready: {piece.name}, {zone.name}");
            EditorUtility.DisplayDialog("PuzzlePals",
                $"퍼즐 프리팹 준비 완료\n  • {PuzzlePiecePrefabPath}\n  • {SnapZonePrefabPath}",
                "OK");
        }

        private static GameObject LoadOrCreatePiecePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PuzzlePiecePrefabPath);
            if (existing != null) return existing;

            var go = new GameObject("PuzzlePiece");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetDefaultSquareSprite();
            sr.color  = Color.white;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = Vector2.one;

            go.AddComponent<PuzzlePiece>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PuzzlePiecePrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject LoadOrCreateZonePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(SnapZonePrefabPath);
            if (existing != null) return existing;

            var go = new GameObject("SnapZone");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = GetDefaultSquareSprite();
            sr.color        = new Color(1f, 1f, 1f, 0.25f);
            sr.sortingOrder = -1;

            go.AddComponent<SnapZone>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, SnapZonePrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ─────────────────────────────────────────────
        // 3. Gameplay 씬 → Loader-driven 구조 마이그레이션
        // ─────────────────────────────────────────────

        [MenuItem("PuzzlePals/Setup Gameplay (Loader-driven)")]
        public static void SetupGameplayLoaderDriven()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Play Mode 종료 필요", "Edit Mode 에서만 동작합니다.", "OK");
                return;
            }

            // 의존성 보장
            EnsureFolder(PrefabFolder);
            var piecePrefab = LoadOrCreatePiecePrefab();
            var zonePrefab  = LoadOrCreateZonePrefab();

            // 1) 기본 Gameplay 씬 구조(Camera/Raycaster/Canvas/HomeButton/CompleteUI/Controller/_GameSystems) 재생성
            //    — SceneSetupTool 의 placeholder pass 를 그대로 활용
            SceneSetupTool.SetupGameplayOnly();

            // 2) 씬 다시 열어서 placeholder 조각/스냅존/PuzzleBoard 제거 후 GameplayLoader 부착
            var scene = EditorSceneManager.OpenScene(GameplayScenePath);

            RemoveAllOfType<PuzzlePiece>();
            RemoveAllOfType<SnapZone>();
            RemoveByName("PuzzleBoard");
            RemoveByName("GameplayLoader");

            var loaderGO = new GameObject("GameplayLoader");
            var loader   = loaderGO.AddComponent<GameplayLoader>();
            loaderGO.AddComponent<PuzzleBoard>();              // GameplayLoader.Awake 가 GetComponent 로 재사용
            var checker  = loaderGO.AddComponent<PuzzleCompleteChecker>();

            // Loader 에 프리팹 와이어링 (private SerializeField → SerializedObject 경유)
            var lso = new SerializedObject(loader);
            lso.FindProperty("piecePrefab").objectReferenceValue    = piecePrefab.GetComponent<PuzzlePiece>();
            lso.FindProperty("snapZonePrefab").objectReferenceValue = zonePrefab.GetComponent<SnapZone>();
            lso.ApplyModifiedProperties();
            EditorUtility.SetDirty(loader);

            // GameplaySceneController 의 _checker(=새 checker) / _loader 재연결
            var controller = Object.FindAnyObjectByType<GameplaySceneController>();
            if (controller != null)
            {
                var cso = new SerializedObject(controller);
                cso.FindProperty("_checker").objectReferenceValue = checker;
                cso.FindProperty("_loader").objectReferenceValue  = loader;
                cso.ApplyModifiedProperties();
                EditorUtility.SetDirty(controller);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[RealLevelsPipelineTool] Gameplay 씬 Loader-driven 마이그레이션 완료");
            EditorUtility.DisplayDialog("PuzzlePals",
                "Gameplay 씬 변환 완료\n\n" +
                "런타임에 GameplayLoader 가 LevelManager.currentLevelIndex 로 LevelDataSO 를 로드해\n" +
                "PuzzlePiece / SnapZone 을 동적 스폰합니다.",
                "OK");
        }

        // ─────────────────────────────────────────────
        // 4. 콤보 — 1 → 2 → FX → 3 한 번에
        // ─────────────────────────────────────────────

        [MenuItem("PuzzlePals/Wire Real Levels Pipeline")]
        public static void WireFullPipeline()
        {
            if (!EditorUtility.DisplayDialog("PuzzlePals",
                "Real Levels Pipeline 4단계를 순차 실행합니다:\n\n" +
                "  1) 플레이스홀더 캐릭터 스프라이트 100개\n" +
                "  2) PuzzlePiece / SnapZone 프리팹\n" +
                "  3) FX (AudioManager / VFXManager / 파티클)\n" +
                "  4) Gameplay 씬을 Loader-driven 구조로\n\n" +
                "기존 placeholder Gameplay 씬은 덮어써집니다.",
                "진행", "취소")) return;

            GeneratePlaceholderSprites();
            GeneratePuzzlePrefabs();
            FXSetupTool.SetupFX();
            SetupGameplayLoaderDriven();

            EditorUtility.DisplayDialog("PuzzlePals",
                "Real Levels Pipeline 완료\n\n" +
                "Play → LevelSelect → 아무 레벨 클릭 → 분할 조각 + 스냅존 자동 스폰",
                "OK");
        }

        // ─────────────────────────────────────────────
        // 헬퍼
        // ─────────────────────────────────────────────

        private static void RemoveAllOfType<T>() where T : Component
        {
            var arr = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in arr)
                if (c != null) Object.DestroyImmediate(c.gameObject);
        }

        private static void RemoveByName(string name)
        {
            var all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var go in all)
                if (go != null && go.name == name)
                    Object.DestroyImmediate(go);
        }

        private static Sprite GetDefaultSquareSprite()
        {
            var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            if (sprite != null) return sprite;
            var tex = Texture2D.whiteTexture;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 100f);
        }

        private static void EnsureFolder(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder)) return;
            string parent = Path.GetDirectoryName(assetFolder).Replace('\\', '/');
            string leaf   = Path.GetFileName(assetFolder);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
