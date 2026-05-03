using UnityEngine;
using UnityEngine.SceneManagement;
using PuzzlePals.UI;

namespace PuzzlePals.Core
{
    /// <summary>
    /// 디바이스 백 키 (Android 시스템 백 = KeyCode.Escape) 처리.
    ///
    /// 씬별 매핑 (A안 — 단순 매핑):
    ///   MainMenu     → 두 번 누르면 앱 종료 (2초 윈도우)
    ///   LevelSelect  → MainMenu
    ///   Gameplay     → LevelSelect
    ///   Gallery      → MainMenu
    ///   Complete     → LevelSelect
    ///
    /// 팝업이 열려있으면 씬 이동 대신 팝업부터 닫는다.
    /// 새 Input System + 레거시 "Both" 모드라 Input.GetKeyDown 으로 통일.
    /// </summary>
    public class BackButtonHandler : MonoBehaviour
    {
        private const float DoubleTapWindowSec = 2f;

        private float _lastBackPressedAt = -999f;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                HandleBack();
        }

        private void HandleBack()
        {
            if (TryCloseTopPopup()) return;

            string scene = SceneManager.GetActiveScene().name;
            switch (scene)
            {
                case SceneLoader.MainMenu:
                    HandleMainMenuBack();
                    break;
                case SceneLoader.LevelSelect:
                case SceneLoader.Gallery:
                    SceneLoader.LoadScene(SceneLoader.MainMenu);
                    break;
                case SceneLoader.Gameplay:
                case SceneLoader.Complete:
                    SceneLoader.LoadScene(SceneLoader.LevelSelect);
                    break;
                default:
                    Debug.LogWarning($"[BackButtonHandler] 매핑 없는 씬: {scene}");
                    break;
            }
        }

        private bool TryCloseTopPopup()
        {
            var settings = FindAnyObjectByType<SettingsPopupUI>(FindObjectsInactive.Exclude);
            if (settings != null && settings.gameObject.activeInHierarchy)
            {
                settings.Hide();
                return true;
            }
            return false;
        }

        private void HandleMainMenuBack()
        {
            float now = Time.unscaledTime;
            if (now - _lastBackPressedAt < DoubleTapWindowSec)
            {
                Debug.Log("[BackButtonHandler] 종료");
                Application.Quit();
                return;
            }
            _lastBackPressedAt = now;
            Debug.Log("[BackButtonHandler] 한 번 더 누르면 종료됩니다");
        }
    }
}
