using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SlidingPuzzle.Core;
using SlidingPuzzle.Gameplay;
using SlidingPuzzle.UI;

namespace SlidingPuzzle.EditorTools
{
    /// <summary>
    /// 5개 씬(MainMenu/LevelSelect/Gameplay/Complete/Gallery) 자동 세팅.
    /// Puzzle Pals 의 SceneSetupTool 패턴 재사용 — idempotent, 메뉴 한 번 클릭.
    /// </summary>
    public static class SceneSetupTool
    {
        private const string ScenesFolder = "Assets/Scenes";

        [MenuItem("SlidingPuzzle/Setup All Scenes")]
        public static void SetupAllScenes()
        {
            if (!GuardEditMode()) return;
            SetupMainMenu();
            SetupLevelSelect();
            SetupGameplay();
            SetupComplete();
            SetupGallery();
            RegisterBuildScenes();
            Debug.Log("[SlidingPuzzle] 5개 씬 세팅 + Build Settings 등록 완료.");
        }

        [MenuItem("SlidingPuzzle/Setup MainMenu Only")]
        public static void SetupMainMenuOnly() { if (GuardEditMode()) SetupMainMenu(); }

        [MenuItem("SlidingPuzzle/Setup LevelSelect Only")]
        public static void SetupLevelSelectOnly() { if (GuardEditMode()) SetupLevelSelect(); }

        [MenuItem("SlidingPuzzle/Setup Gameplay Only")]
        public static void SetupGameplayOnly() { if (GuardEditMode()) SetupGameplay(); }

        [MenuItem("SlidingPuzzle/Setup Complete Only")]
        public static void SetupCompleteOnly() { if (GuardEditMode()) SetupComplete(); }

        [MenuItem("SlidingPuzzle/Setup Gallery Only")]
        public static void SetupGalleryOnly() { if (GuardEditMode()) SetupGallery(); }

        // ─────────────────────────────────────────────────────

        private static void SetupMainMenu()
        {
            string path = $"{ScenesFolder}/MainMenu.unity";
            var scene = OpenOrCreateScene(path);

            RemoveByName("MainMenuController");
            RemoveByName("_GameSystems");
            RemoveByName("EventSystem");
            RemoveByName("SettingsPopup");

            EnsureEventSystem();
            CreateGameSystems();

            var popupGo = new GameObject("SettingsPopup", typeof(RectTransform));
            popupGo.AddComponent<SettingsPopupUI>();
            popupGo.SetActive(false);

            var go = new GameObject("MainMenuController");
            go.AddComponent<MainMenuController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void SetupLevelSelect()
        {
            string path = $"{ScenesFolder}/LevelSelect.unity";
            var scene = OpenOrCreateScene(path);

            RemoveByName("LevelSelectController");
            RemoveByName("_GameSystems");
            RemoveByName("EventSystem");
            RemoveByName("SettingsPopup");

            EnsureEventSystem();
            var sceneLoader = CreateGameSystems();
            if (Object.FindAnyObjectByType<LevelManager>() == null)
                sceneLoader.gameObject.AddComponent<LevelManager>();

            var popupGo = new GameObject("SettingsPopup", typeof(RectTransform));
            popupGo.AddComponent<SettingsPopupUI>();
            popupGo.SetActive(false);

            var go = new GameObject("LevelSelectController");
            go.AddComponent<LevelSelectController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void SetupGameplay()
        {
            string path = $"{ScenesFolder}/Gameplay.unity";
            var scene = OpenOrCreateScene(path);

            RemoveByName("Canvas");
            RemoveByName("GameplaySceneController");
            RemoveByName("_GameSystems");
            RemoveByName("EventSystem");
            RemoveByName("Main Camera");

            EnsureEventSystem();
            CreateGameSystems();
            EnsureCamera();

            var canvasGo = new GameObject("Canvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0f;

            const float headerH = 168f;
            const float bottomMargin = 168f; // 배너 광고 영역 (PaletteUI.BannerAdHeight) 만큼 비워둠

            var headerGo = new GameObject("GameplayHeader", typeof(RectTransform));
            headerGo.transform.SetParent(canvasGo.transform, false);
            headerGo.AddComponent<GameplayHeaderUI>();

            var boardGo = new GameObject("SlidingBoard", typeof(RectTransform));
            boardGo.transform.SetParent(canvasGo.transform, false);
            var boardRt = (RectTransform)boardGo.transform;
            boardRt.anchorMin = new Vector2(0f, 0f);
            boardRt.anchorMax = new Vector2(1f, 1f);
            boardRt.offsetMin = new Vector2(0f, bottomMargin);
            boardRt.offsetMax = new Vector2(0f, -headerH);
            boardGo.AddComponent<SlidingBoard>();

            var ctrlGo = new GameObject("GameplaySceneController");
            ctrlGo.AddComponent<GameplaySceneController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void SetupComplete()
        {
            string path = $"{ScenesFolder}/Complete.unity";
            var scene = OpenOrCreateScene(path);

            RemoveByName("CompleteSceneController");
            RemoveByName("_GameSystems");
            RemoveByName("EventSystem");

            EnsureEventSystem();
            CreateGameSystems();

            var go = new GameObject("CompleteSceneController");
            go.AddComponent<CompleteSceneController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void SetupGallery()
        {
            string path = $"{ScenesFolder}/Gallery.unity";
            var scene = OpenOrCreateScene(path);

            RemoveByName("GalleryController");
            RemoveByName("_GameSystems");
            RemoveByName("EventSystem");

            EnsureEventSystem();
            var sceneLoader = CreateGameSystems();
            if (Object.FindAnyObjectByType<LevelManager>() == null)
                sceneLoader.gameObject.AddComponent<LevelManager>();

            var go = new GameObject("GalleryController");
            go.AddComponent<GalleryController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void RegisterBuildScenes()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene($"{ScenesFolder}/MainMenu.unity", true),
                new EditorBuildSettingsScene($"{ScenesFolder}/LevelSelect.unity", true),
                new EditorBuildSettingsScene($"{ScenesFolder}/Gameplay.unity", true),
                new EditorBuildSettingsScene($"{ScenesFolder}/Complete.unity", true),
                new EditorBuildSettingsScene($"{ScenesFolder}/Gallery.unity", true),
            };
            EditorBuildSettings.scenes = scenes;
        }

        // ─────────────────────────────────────────────────────
        // 헬퍼
        // ─────────────────────────────────────────────────────

        private static bool GuardEditMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Play Mode 종료 필요",
                    "Setup 도구는 Edit Mode 에서만 동작합니다.", "OK");
                return false;
            }
            return true;
        }

        private static Scene OpenOrCreateScene(string path)
        {
            if (File.Exists(path))
                return EditorSceneManager.OpenScene(path);

            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, path);
            return scene;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            string leaf    = Path.GetFileName(folder);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static void RemoveByName(string name)
        {
            var all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var go in all)
                if (go != null && go.name == name) Object.DestroyImmediate(go);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include) != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static void EnsureCamera()
        {
            if (Camera.main != null) return;
            var go = new GameObject("Main Camera", typeof(Camera));
            go.tag = "MainCamera";
            var cam = go.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = PaletteTokens.Active.Bg;
        }

        private static SceneLoader CreateGameSystems()
        {
            var go = new GameObject("_GameSystems");
            var loader = go.AddComponent<SceneLoader>();
            go.AddComponent<BackButtonHandler>();
            return loader;
        }
    }
}
