using UnityEngine;
using UnityEngine.SceneManagement;
using SlidingPuzzle.UI;

namespace SlidingPuzzle.Core
{
    /// <summary>씬 전환 유틸리티 — UI 버튼 OnClick에서 직접 연결 가능</summary>
    public class SceneLoader : MonoBehaviour
    {
        // ── 씬 이름 상수 ─────────────────────────────────────
        public const string MainMenu    = "MainMenu";
        public const string LevelSelect = "LevelSelect";
        public const string Gameplay    = "Gameplay";
        public const string Complete    = "Complete";
        public const string Gallery     = "Gallery";

        // ── 버튼 OnClick 대상 메서드 ─────────────────────────

        public void GoToMainMenu()    => LoadScene(MainMenu);
        public void GoToLevelSelect() => LoadScene(LevelSelect);
        public void GoToComplete()    => LoadScene(Complete);
        public void GoToGallery()     => LoadScene(Gallery);

        /// <summary>레벨 선택 씬에서 레벨 번호로 호출</summary>
        public void LoadLevel(int levelIndex)
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.currentLevelIndex = levelIndex;
            LoadScene(Gameplay);
        }

        // ── Button OnClick에서 직접 연결 가능한 무인자 래퍼들 ────
        // (Button.onClick은 int 인자 PersistentListener 직렬화가 불안정한 케이스가 있어 래퍼 사용)
        public void LoadLevel0() { Debug.Log("[SceneLoader] LoadLevel0 invoked"); LoadLevel(0); }
        public void LoadLevel1() => LoadLevel(1);
        public void LoadLevel2() => LoadLevel(2);
        public void LoadLevel3() => LoadLevel(3);
        public void LoadLevel4() => LoadLevel(4);

        /// <summary>Complete 씬 → 다음 레벨 Gameplay. 마지막 레벨이면 LevelSelect 로 복귀.</summary>
        public void LoadNextLevel()
        {
            if (LevelManager.Instance == null)
            {
                LoadScene(Gameplay);
                return;
            }

            int next = LevelManager.Instance.currentLevelIndex + 1;

            // 다음 레벨 에셋 존재 여부로 끝 판정 (하드코딩 100 회피 — 레벨 추가 시 자동 대응)
            var probe = Resources.Load<SlidingPuzzle.Data.LevelDataSO>($"Levels/Level_{(next + 1):D3}");
            if (probe == null)
            {
                LoadScene(LevelSelect);
                return;
            }

            LevelManager.Instance.currentLevelIndex = next;
            LoadScene(Gameplay);
        }

        // ── 정적 헬퍼 ────────────────────────────────────────

        // 테스트 환경 / 페이더 미사용 시 false 로 두면 즉시 로드 (디버그/안정성)
        public static bool UseFader { get; set; } = true;

        public static void LoadScene(string sceneName)
        {
            Debug.Log($"[SceneLoader] LoadScene called: {sceneName}, useFader={UseFader}");

            // 플레이 모드 + 페이더 사용 모드에서만 SceneFader 경유. 그 외엔 즉시 로드.
            if (Application.isPlaying && UseFader)
            {
                try
                {
                    var fader = SceneFader.Instance;
                    if (fader != null && fader.IsReady)
                    {
                        fader.FadeAndLoad(sceneName);
                        return;
                    }
                    Debug.LogWarning($"[SceneLoader] Fader 미준비 — 즉시 로드 폴백: {sceneName}");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SceneLoader] Fader 실패, 즉시 로드 폴백: {e.Message}");
                }
            }
            SceneManager.LoadScene(sceneName);
        }
    }
}
