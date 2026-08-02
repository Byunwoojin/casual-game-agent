using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MemoryMatch.UI
{
    /// <summary>
    /// 풀스크린 BgPage(베이지) 오버레이로 씬 전환 페이드. DontDestroyOnLoad 싱글톤.
    /// SceneLoader.LoadScene 이 자동으로 이 인스턴스를 경유한다.
    /// 검정 대신 BgPage 사용 — 모든 씬 배경이 BgPage 라 fade 가 배경과 자연스럽게 이어짐 (게임 종료 같은 느낌 X).
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneFader : MonoBehaviour
    {
        private static SceneFader _instance;

        public static SceneFader Instance
        {
            get
            {
                if (_instance != null) return _instance;
                if (!Application.isPlaying) return null;

                var existing = FindAnyObjectByType<SceneFader>(FindObjectsInactive.Include);
                if (existing != null) { _instance = existing; return _instance; }

                var go = new GameObject(nameof(SceneFader));
                _instance = go.AddComponent<SceneFader>();
                return _instance;
            }
        }

        // GDD §Game Feel: 씬 전환 = 검정 오버레이 0.3s in/out
        // 0.2s 로 약간 단축 — 기존 PlayMode 테스트의 0.5s 대기 마진 확보
        [SerializeField] private float fadeDuration = 0.2f;

        private Image _overlay;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            BuildOverlay();
        }

        public bool IsReady => _overlay != null;

        private void BuildOverlay()
        {
            var canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 9999;
            }
            if (gameObject.GetComponent<CanvasScaler>() == null)
                gameObject.AddComponent<CanvasScaler>();

            var imgGo = new GameObject("FadeOverlay",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imgGo.transform.SetParent(transform, false);

            var rt = imgGo.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            _overlay = imgGo.GetComponent<Image>();
            if (_overlay != null)
            {
                // BgPage (크림) — 모든 씬 배경과 동일 색이라 fade 시 검은 화면 같은 종료 느낌 X
                var c = DesignTokens.BgPage;
                c.a = 0f;
                _overlay.color         = c;
                _overlay.raycastTarget = false;
            }
        }

        public void FadeAndLoad(string sceneName)
        {
            // 오버레이 빌드 실패했으면 즉시 로드 (페이드 스킵 — 절대 멈추면 안 됨)
            if (_overlay == null)
            {
                Debug.LogWarning("[SceneFader] _overlay null — 페이드 스킵하고 즉시 씬 로드");
                SceneManager.LoadScene(sceneName);
                return;
            }
            StartCoroutine(FadeAndLoadRoutine(sceneName));
        }

        private IEnumerator FadeAndLoadRoutine(string sceneName)
        {
            if (_overlay != null) _overlay.raycastTarget = true;

            TweenAlpha(1f, fadeDuration, Ease.OutQuad);
            yield return new WaitForSeconds(fadeDuration);

            SceneManager.LoadScene(sceneName);

            TweenAlpha(0f, fadeDuration, Ease.InQuad);
            yield return new WaitForSeconds(fadeDuration);

            if (_overlay != null) _overlay.raycastTarget = false;
        }

        // DOTween UI 모듈(Image.DOFade) 의존 회피 — DOTween.To 로 색상 alpha 만 직접 트윈
        private void TweenAlpha(float target, float duration, Ease ease)
        {
            if (_overlay == null) return;

            DOTween.Kill(_overlay);
            DOTween.To(
                    () => _overlay.color.a,
                    a =>
                    {
                        if (_overlay == null) return;
                        var c = _overlay.color;
                        c.a = a;
                        _overlay.color = c;
                    },
                    target,
                    duration)
                .SetEase(ease)
                .SetTarget(_overlay);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
