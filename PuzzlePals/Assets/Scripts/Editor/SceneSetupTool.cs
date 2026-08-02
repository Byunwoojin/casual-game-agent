using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PuzzlePals.Core;
using PuzzlePals.Gameplay;
using PuzzlePals.Puzzle;
using PuzzlePals.UI;

namespace PuzzlePals.EditorTools
{
    /// <summary>
    /// 메뉴 한 번 클릭으로 4개 씬(MainMenu/LevelSelect/Gameplay/Complete)을
    /// PlayMode 테스트가 통과할 수 있도록 자동 세팅한다.
    ///
    /// 사용법: Unity 상단 메뉴 → [PuzzlePals] → [Setup All Scenes]
    /// </summary>
    public static class SceneSetupTool
    {
        // ─────────────────────────────────────────────────────
        // 메뉴 엔트리
        // ─────────────────────────────────────────────────────

        [MenuItem("PuzzlePals/Setup All Scenes")]
        public static void SetupAllScenes()
        {
            if (!GuardEditMode()) return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            SetupMainMenu();
            SetupLevelSelectFull();   // 실제 게임용 100버튼 그리드 (placeholder 대체)
            SetupGameplay();
            SetupComplete();
            SetupGallery();           // 갤러리 (v0.6) — Build Settings 에 Gallery 씬 추가 필요

            Debug.Log("[SceneSetupTool] All 4 scenes setup complete.");
            EditorUtility.DisplayDialog("Setup Complete",
                "4개 씬(MainMenu / LevelSelect / Gameplay / Complete) 세팅 완료!\n\n" +
                "Test Runner에서 PlayMode 테스트를 실행해 보세요.",
                "OK");
        }

        [MenuItem("PuzzlePals/Setup MainMenu Only")]
        public static void SetupMainMenuOnly() => SetupMainMenu();

        [MenuItem("PuzzlePals/Setup LevelSelect Only")]
        public static void SetupLevelSelectOnly() => SetupLevelSelect();

        [MenuItem("PuzzlePals/Setup LevelSelect (Full Grid)")]
        public static void SetupLevelSelectFullOnly() => SetupLevelSelectFull();

        [MenuItem("PuzzlePals/Setup Gameplay Only")]
        public static void SetupGameplayOnly() => SetupGameplay();

        [MenuItem("PuzzlePals/Setup Complete Only")]
        public static void SetupCompleteOnly() => SetupComplete();

        [MenuItem("PuzzlePals/Setup Gallery Only")]
        public static void SetupGalleryOnly() => SetupGallery();

        // ─────────────────────────────────────────────────────
        // 씬별 세팅
        // ─────────────────────────────────────────────────────

        private static void SetupMainMenu()
        {
            if (!GuardEditMode()) return;

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

            // 옛 UI 정리 (v0.6 — MainMenuController 가 모든 UI 를 런타임 빌드)
            RemoveByName("PlayButton");
            RemoveByName("SettingsButton");
            RemoveByName("SettingsPopup");
            RemoveByName("MainMenuController");
            RemoveByName("MainMenuCanvas");
            RemoveByName("UIRoot");
            RemoveByName("Canvas");
            RemoveByName("_GameSystems");

            EnsureEventSystem();

            CreateGameSystems();

            // SettingsPopup — controller 가 FindAnyObjectByType 으로 찾음
            var popupGo = new GameObject("SettingsPopup", typeof(RectTransform));
            popupGo.AddComponent<SettingsPopupUI>();
            popupGo.SetActive(false); // 초기 숨김

            // 컨트롤러 GO 1개 — Start 에서 Canvas/배경/타이틀/PLAY/Settings 전부 빌드
            var ctrlGO = new GameObject("MainMenuController");
            ctrlGO.AddComponent<MainMenuController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[SceneSetupTool] MainMenu (v0.6 Pastel Puzzle) 세팅 완료.");
        }

        private static void SetupLevelSelect()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/LevelSelect.unity");

            RemoveByName("LevelButton_0");
            RemoveByName("_GameSystems");

            var canvas = EnsureCanvas();
            EnsureEventSystem();
            var uiRoot = EnsureUIRoot(canvas);

            var sceneLoader = CreateGameSystems();
            // LevelSelect에는 LevelManager도 필요 (현재 레벨 기억용, 싱글톤)
            if (Object.FindAnyObjectByType<LevelManager>() == null)
                sceneLoader.gameObject.AddComponent<LevelManager>();

            var btn = CreateUIButton(uiRoot, "LevelButton_0", "LEVEL 1", Vector2.zero);
            // 무인자 래퍼 사용 (int 인자 PersistentListener 직렬화 회피)
            WireOnClick(btn, sceneLoader, nameof(SceneLoader.LoadLevel0));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void SetupGameplay()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");

            // 기존 placeholder + 누적된 PuzzlePiece/SnapZone 모두 삭제
            RemoveByName("HomeButton");
            RemoveByName("CompleteUI");
            RemoveByName("PuzzleBoard");
            RemoveByName("_GameSystems");
            RemoveByName("GameplaySceneController");
            RemoveByName("Canvas");
            RemoveByName("EventSystem");
            RemoveAllPuzzlePieces();   // 누적된 PuzzlePiece_*  전부 제거
            RemoveAllSnapZones();      // 누적된 SnapZone_*    전부 제거

            // Main Camera에 Physics2DRaycaster 부착 (PuzzlePiece의 Pointer 이벤트 수신용)
            var mainCam = Camera.main;
            if (mainCam != null && mainCam.GetComponent<Physics2DRaycaster>() == null)
                mainCam.gameObject.AddComponent<Physics2DRaycaster>();

            var canvas = EnsureCanvas();
            EnsureEventSystem();
            var uiRoot = EnsureUIRoot(canvas);

            // SafeArea (a) 패턴 v0.6 — uiRoot 의 SafeAreaFitter 제거 (헤더가 화면 위 끝까지 stretch 해야 노치 영역까지 흰색).
            // 자식 헤더 안의 SafeAreaTopExtender 가 자식 컨테이너만 SafeArea inset 처리.
            var oldRootFitter = uiRoot.GetComponent<UI.SafeAreaFitter>();
            if (oldRootFitter != null) Object.DestroyImmediate(oldRootFitter);

            var sceneLoader = CreateGameSystems();

            // 헤더 — GameplayHeaderUI 가 자체적으로 Material 아이콘 4버튼 + 내부 onClick wiring 처리.
            RemoveByName("GameplayHeader");
            var headerGo = new GameObject("GameplayHeader", typeof(RectTransform));
            headerGo.transform.SetParent(uiRoot, false);
            var header = headerGo.AddComponent<GameplayHeaderUI>();
            header.EnsureBuilt(); // Editor 모드 Awake 미실행 회피

            // BannerAd placeholder — 화면 하단 (PaletteUI placeholder 회색 박스).
            //   PuzzleBoard 의 _bannerReserveH 가 이미 하단 영역 reserve 함 — 보드 자동 위로.
            RemoveByName("BannerAd");
            var bannerGo = PaletteUI.BuildBannerAdPlaceholder(uiRoot, "BannerAd");
            var brt = (RectTransform)bannerGo.transform;
            brt.anchorMin = new Vector2(0f, 0f);
            brt.anchorMax = new Vector2(1f, 0f);
            brt.pivot     = new Vector2(0.5f, 0f);
            brt.sizeDelta = new Vector2(0f, PaletteUI.BannerAdHeight);
            brt.anchoredPosition = Vector2.zero;

            // CompleteUI — 초기 비활성
            var completeUI = new GameObject("CompleteUI", typeof(RectTransform));
            completeUI.transform.SetParent(uiRoot, false);
            var cuRt = completeUI.GetComponent<RectTransform>();
            cuRt.anchorMin = Vector2.zero;
            cuRt.anchorMax = Vector2.one;
            cuRt.offsetMin = Vector2.zero;
            cuRt.offsetMax = Vector2.zero;
            var cuImg = completeUI.AddComponent<Image>();
            cuImg.color = new Color(0f, 0f, 0f, 0.6f);
            completeUI.SetActive(false);

            // PuzzleBoard GO + PuzzleCompleteChecker + GameplayLoader (PlayMode 진입 시 Awake 가 LevelDataSO
            //   로드해 board.Build() 호출 → 36/64 진짜 조각 동적 생성).
            var boardGO = new GameObject("PuzzleBoard");
            var board   = boardGO.AddComponent<PuzzleBoard>();
            var checker = boardGO.AddComponent<PuzzleCompleteChecker>();
            var loader  = boardGO.AddComponent<GameplayLoader>();

            // 주의: 여기서 placeholder PuzzlePiece/SnapZone 만들지 않음.
            //   이전 코드는 분홍 색 placeholder piece 2개를 만들었는데 (Color(1, 0.6, 0.6)),
            //   PlayMode 시 GameplayLoader 가 그것들을 destroy 하지 않아 잔존 →
            //   GameplaySceneController 의 fallback FindObjectsByType<PuzzlePiece> 가 호출되면
            //   placeholder 2개 포함된 38개 배열로 잡혀 Check() = false (= Complete 안 넘어감).
            //   해결: placeholder 만들지 말고, _loader 도 SerializeField 로 wiring → fallback 안 거침.

            // GameplaySceneController 부착
            var controllerGO = new GameObject("GameplaySceneController");
            var controller   = controllerGO.AddComponent<GameplaySceneController>();

            // SerializeField wiring — _loader 도 명시 wiring (fallback 회피)
            var so = new SerializedObject(controller);
            so.FindProperty("_checker").objectReferenceValue     = checker;
            so.FindProperty("_completeUI").objectReferenceValue  = completeUI;
            var loaderProp = so.FindProperty("_loader");
            if (loaderProp != null) loaderProp.objectReferenceValue = loader;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        /// <summary>
        /// Resources/Levels 의 LevelDataSO 100개를 ScrollView+GridLayoutGroup 으로 표시하는
        /// 실사용 LevelSelect 씬 세팅. PlayMode 테스트용 placeholder 경로(SetupLevelSelect)와는 분리.
        /// </summary>
        private static void SetupLevelSelectFull()
        {
            if (!GuardEditMode()) return;

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/LevelSelect.unity");

            // 옛 UI 정리 (성인 피벗 v0.4 — controller 가 모든 UI 를 런타임 빌드)
            RemoveByName("LevelButton_0");
            RemoveByName("_GameSystems");
            RemoveByName("LevelSelectScroll");
            RemoveByName("LevelSelectController");
            RemoveByName("LevelSelectCanvas");
            RemoveByName("UIRoot");
            RemoveByName("Canvas");
            RemoveByName("HomeButton");
            RemoveByName("SettingsButton");
            RemoveByName("SettingsPopup");

            EnsureEventSystem();

            var sceneLoader = CreateGameSystems();
            if (Object.FindAnyObjectByType<LevelManager>() == null)
                sceneLoader.gameObject.AddComponent<LevelManager>();

            // SettingsPopup — controller 가 FindAnyObjectByType 으로 찾음
            var popupGo = new GameObject("SettingsPopup", typeof(RectTransform));
            popupGo.AddComponent<SettingsPopupUI>();
            popupGo.SetActive(false); // 초기 숨김

            // 컨트롤러 GO 1개 — Start 에서 Canvas/배경/헤더/스크롤/카드 전부 빌드
            var ctrlGO = new GameObject("LevelSelectController");
            ctrlGO.AddComponent<LevelSelectController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[SceneSetupTool] LevelSelect (Adult v0.4) 세팅 완료. " +
                      "DesignTokens 기반 카드 격자 — Play 모드에서 Resources/Levels 자동 로드.");
        }

        /// <summary>ScrollRect + Viewport(Mask) + Content(GridLayoutGroup 5열) 생성. Content RectTransform 반환.</summary>
        private static RectTransform BuildLevelSelectScrollView(Transform canvasParent)
        {
            // Root (ScrollRect)
            var rootGO = new GameObject(
                "LevelSelectScroll",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
            rootGO.transform.SetParent(canvasParent, false);

            // 캔버스 크기에 맞춰 stretch (위/아래 마진 200, 좌/우 40) — 모든 해상도 대응
            var rootRt = rootGO.GetComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(0f, 0f);
            rootRt.anchorMax = new Vector2(1f, 1f);
            rootRt.pivot     = new Vector2(0.5f, 0.5f);
            rootRt.offsetMin = new Vector2(40f, 200f);
            rootRt.offsetMax = new Vector2(-40f, -200f);

            // iOS 노치 / Android 네비바 회피
            rootGO.AddComponent<SafeAreaFitter>();

            rootGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.05f);

            // Viewport — RectMask2D 사용 (Mask+Image 조합보다 안정적, UI 표준)
            var vpGO = new GameObject(
                "Viewport",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
            vpGO.transform.SetParent(rootGO.transform, false);

            var vpRt = vpGO.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = Vector2.zero;
            vpRt.pivot     = new Vector2(0.5f, 0.5f);

            // 드래그 입력 수신용 투명 Image (raycastTarget=true 필수)
            var vpImg = vpGO.GetComponent<Image>();
            vpImg.color         = new Color(1f, 1f, 1f, 0.001f);
            vpImg.raycastTarget = true;

            // Content (GridLayoutGroup + ContentSizeFitter)
            var contentGO = new GameObject(
                "Content",
                typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            contentGO.transform.SetParent(vpGO.transform, false);

            var contentRt = contentGO.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot     = new Vector2(0.5f, 1f);
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            // GDD 사양: 5열 그리드 — 1080 기준 폭에 5*180 + 4*20 + 2*20 = 1020 으로 안정 수렴
            var grid = contentGO.GetComponent<GridLayoutGroup>();
            grid.cellSize        = new Vector2(180f, 180f);
            grid.spacing         = new Vector2(20f, 20f);
            grid.padding         = new RectOffset(20, 20, 20, 20);
            grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.childAlignment  = TextAnchor.UpperCenter;

            var fitter = contentGO.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

            // 캔버스 폭에 따라 3~7열 자동 조정 (태블릿에서 더 많은 컬럼 표시)
            contentGO.AddComponent<ResponsiveGridColumns>();

            // ScrollRect 연결 — 세로 스크롤만
            var scroll = rootGO.GetComponent<ScrollRect>();
            scroll.viewport     = vpRt;
            scroll.content      = contentRt;
            scroll.horizontal   = false;
            scroll.vertical     = true;
            scroll.movementType = ScrollRect.MovementType.Elastic; // 탄성 스크롤 (모바일 표준)
            scroll.scrollSensitivity = 20f;

            // 초기 레이아웃 강제 계산 — 첫 프레임 ContentSizeFitter 지연 방지
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(contentRt);

            return contentRt;
        }

        private static void SetupComplete()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Complete.unity");

            // 옛 UI 정리 (성인 피벗 v0.4 — 모든 UI 를 CompleteSceneController 가 런타임 빌드)
            RemoveByName("NextButton");
            RemoveByName("HomeButton");
            RemoveByName("UIRoot");
            RemoveByName("Canvas");
            RemoveByName("CompleteCanvas");
            RemoveByName("CompleteSceneController");
            RemoveByName("_GameSystems");

            EnsureEventSystem();
            CreateGameSystems(); // SceneLoader 싱글톤 (DontDestroyOnLoad 가 보장하지 못하는 부트스트랩 케이스용)

            // 컨트롤러 GO 하나만 — Start 에서 Canvas/배경/텍스트/버튼 모두 빌드
            var go = new GameObject("CompleteSceneController");
            go.AddComponent<PuzzlePals.Gameplay.CompleteSceneController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ─────────────────────────────────────────────────────
        // SetupGallery — Gallery.unity 신규 씬 (없으면 새로 생성)
        // ─────────────────────────────────────────────────────
        private static void SetupGallery()
        {
            if (!GuardEditMode()) return;

            const string path = "Assets/Scenes/Gallery.unity";
            UnityEngine.SceneManagement.Scene scene;
            if (System.IO.File.Exists(path))
            {
                scene = EditorSceneManager.OpenScene(path);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                Debug.Log("[SceneSetupTool] Gallery.unity 새로 생성. Build Settings 에 추가하세요.");
            }

            // 기존 정리
            RemoveByName("Canvas");
            RemoveByName("UIRoot");
            RemoveByName("GalleryMuseumController");
            RemoveByName("_GameSystems");

            EnsureEventSystem();
            CreateGameSystems();

            // 컨트롤러 GO 1개 — Start 에서 모든 UI 빌드
            var go = new GameObject("GalleryMuseumController");
            go.AddComponent<GalleryMuseumController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);

            // Build Settings 에 자동 등록 (없으면 추가) — SceneLoader.LoadScene("Gallery") 가 동작하도록
            var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!buildScenes.Exists(s => s.path == path))
            {
                buildScenes.Add(new EditorBuildSettingsScene(path, enabled: true));
                EditorBuildSettings.scenes = buildScenes.ToArray();
                Debug.Log("[SceneSetupTool] Build Settings 에 Gallery 씬 자동 추가됨.");
            }

            Debug.Log("[SceneSetupTool] Gallery 셋업 완료 — " + path);
        }

        // 버튼의 Label(Text) 자식에 LocalizedTextBinder 부착 (entryKey = UI 테이블의 키)
        private static void BindLocalizedLabel(Button btn, string entryKey)
        {
            var labelTr = btn.transform.Find("Label");
            if (labelTr == null) return;

            var binder = labelTr.gameObject.GetComponent<LocalizedTextBinder>();
            if (binder == null) binder = labelTr.gameObject.AddComponent<LocalizedTextBinder>();

            // private SerializeField 직렬화 — SerializedObject 경유
            var so = new SerializedObject(binder);
            so.FindProperty("tableName").stringValue = "UI";
            so.FindProperty("entryKey").stringValue  = entryKey;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(binder);
        }

        // ─────────────────────────────────────────────────────
        // 헬퍼
        // ─────────────────────────────────────────────────────

        /// <summary>Play Mode 가드 — Editor 전용 API는 Play Mode에서 호출 불가</summary>
        private static bool GuardEditMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    "Play Mode 종료 필요",
                    "Setup 도구는 Edit Mode에서만 동작합니다.\n\n상단 ▶ Play 버튼을 다시 눌러 Play Mode를 종료한 뒤 다시 실행하세요.",
                    "OK");
                return false;
            }
            return true;
        }

        private static void RemoveByName(string name)
        {
            // 활성/비활성 모두 포함해 검색
            var all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            foreach (var go in all)
            {
                if (go != null && go.name == name)
                    Object.DestroyImmediate(go);
            }
        }

        private static void RemoveAllPuzzlePieces()
        {
            var pieces = Object.FindObjectsByType<PuzzlePiece>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var p in pieces)
                if (p != null) Object.DestroyImmediate(p.gameObject);
        }

        private static void RemoveAllSnapZones()
        {
            var zones = Object.FindObjectsByType<SnapZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var z in zones)
                if (z != null) Object.DestroyImmediate(z.gameObject);
        }

        private static Canvas EnsureCanvas()
        {
            var existing = Object.FindAnyObjectByType<Canvas>();
            if (existing == null)
            {
                var go = new GameObject("Canvas");
                go.layer = LayerMask.NameToLayer("UI");
                existing = go.AddComponent<Canvas>();
                existing.renderMode = RenderMode.ScreenSpaceOverlay;
                go.AddComponent<CanvasScaler>();
                go.AddComponent<GraphicRaycaster>();
            }

            // 모바일 세로 1080x1920 디자인 기준. 0.5 매칭 — letterbox 가 비율 유지하므로 안전.
            var scaler = existing.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight  = 0f;
            }
            return existing;
        }

        /// <summary>
        /// SafeArea (iOS 노치 / Android 네비바) 회피용 풀스크린 UI 컨테이너.
        /// 모든 UI 는 이 Transform 의 자식으로 만들어야 노치 영역 침범 안 함.
        /// 코너 버튼들은 이 컨테이너의 anchorMin/Max 코너에 anchor.
        /// </summary>
        private static RectTransform EnsureUIRoot(Canvas canvas)
        {
            // 옛 letterbox 잔존물 제거 (이전 버전에서 만들었을 수 있음)
            var oldBg = canvas.transform.Find("LetterboxBackground");
            if (oldBg != null) Object.DestroyImmediate(oldBg.gameObject);
            var oldDesign = canvas.transform.Find("DesignRoot");
            if (oldDesign != null)
            {
                // DesignRoot 자식들을 Canvas 직속으로 옮긴 후 DesignRoot 제거
                while (oldDesign.childCount > 0)
                    oldDesign.GetChild(0).SetParent(canvas.transform, false);
                Object.DestroyImmediate(oldDesign.gameObject);
            }
            var oldBgCam = GameObject.Find("BackgroundCamera");
            if (oldBgCam != null) Object.DestroyImmediate(oldBgCam);
            var mainCamLB = Camera.main != null ? Camera.main.GetComponent<UI.LetterboxCamera>() : null;
            if (mainCamLB != null) Object.DestroyImmediate(mainCamLB);
            if (Camera.main != null) Camera.main.rect = new Rect(0f, 0f, 1f, 1f);

            // 이미 존재하면 재사용
            var existing = canvas.transform.Find("UIRoot");
            if (existing != null) return (RectTransform)existing;

            // SafeArea 컨테이너 — Canvas 풀스크린 stretch + SafeAreaFitter
            var rootGo = new GameObject("UIRoot",
                typeof(RectTransform), typeof(UI.SafeAreaFitter));
            rootGo.transform.SetParent(canvas.transform, false);
            var rootRt = (RectTransform)rootGo.transform;
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            return rootRt;
        }

        /// <summary>버튼을 컨테이너의 좌상단 코너에 고정 (어떤 비율에서도 항상 보임)</summary>
        private static void AnchorTopLeft(RectTransform rt, Vector2 margin)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot     = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(margin.x, -margin.y);
        }

        /// <summary>버튼을 컨테이너의 우상단 코너에 고정</summary>
        private static void AnchorTopRight(RectTransform rt, Vector2 margin)
        {
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-margin.x, -margin.y);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static SceneLoader CreateGameSystems()
        {
            var go = new GameObject("_GameSystems");
            var loader = go.AddComponent<SceneLoader>();
            go.AddComponent<BackButtonHandler>(); // 디바이스 백키 (Escape) 처리
            return loader;
        }

        /// <summary>Canvas 하위에 Image+Button+Text 구조의 UI 버튼 생성</summary>
        private static Button CreateUIButton(Transform parent, string name, string label, Vector2 anchoredPos)
        {
            var btnGO = new GameObject(name, typeof(RectTransform));
            btnGO.transform.SetParent(parent, false);
            var rt = btnGO.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = new Vector2(240f, 80f);

            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 1f, 1f);

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;

            // Label (Legacy Text — 빌트인 폰트 사용)
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(btnGO.transform, false);
            var labelRt = labelGO.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            var txt = labelGO.AddComponent<Text>();
            txt.text      = label;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color     = Color.white;
            txt.fontSize  = 28;
            txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return btn;
        }

        /// <summary>Button.onClick에 target.methodName() 퍼시스턴트 리스너 등록 (인자 없음)</summary>
        private static void WireOnClick(Button button, Object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName);
            var action = System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), target, method)
                as UnityEngine.Events.UnityAction;
            UnityEventTools.AddPersistentListener(button.onClick, action);
        }

        /// <summary>Button.onClick에 target.methodName(int arg) 퍼시스턴트 리스너 등록</summary>
        private static void WireOnClickInt(Button button, Object target, string methodName, int arg)
        {
            // UnityEvent(non-generic) → int 인자는 AddIntPersistentListener 사용
            var method = target.GetType().GetMethod(methodName, new[] { typeof(int) });
            var action = System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction<int>), target, method)
                as UnityEngine.Events.UnityAction<int>;
            UnityEventTools.AddIntPersistentListener(button.onClick, action, arg);
        }

        /// <summary>public 필드 직접 할당 + SerializedObject 통해 dirty 마킹</summary>
        private static void AssignSnapZone(PuzzlePiece piece, SnapZone zone)
        {
            piece.AssignedSnapZone = zone;

            // SerializedObject로 명시적 dirty 마킹 (씬 저장 시 확실히 직렬화)
            var so   = new SerializedObject(piece);
            var prop = so.FindProperty(nameof(PuzzlePiece.AssignedSnapZone));
            if (prop != null)
            {
                prop.objectReferenceValue = zone;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorUtility.SetDirty(piece);
        }

        private static PuzzlePiece CreatePuzzlePiece(string name, Vector3 worldPos)
        {
            var go = new GameObject(name);
            go.transform.position = worldPos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetDefaultSquareSprite();
            sr.color  = new Color(1f, 0.6f, 0.6f, 1f);

            go.AddComponent<BoxCollider2D>();
            var piece = go.AddComponent<PuzzlePiece>();
            return piece;
        }

        private static SnapZone CreateSnapZone(string name, Vector3 worldPos)
        {
            var go = new GameObject(name);
            go.transform.position = worldPos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetDefaultSquareSprite();
            sr.color  = new Color(1f, 1f, 1f, 0.25f);
            sr.sortingOrder = -1;

            return go.AddComponent<SnapZone>();
        }

        /// <summary>Unity 내장 UI Sprite (정사각형) — 없으면 1x1 흰색 텍스처로 대체</summary>
        private static Sprite GetDefaultSquareSprite()
        {
            var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            if (sprite != null) return sprite;

            var tex = Texture2D.whiteTexture;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
