using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SlidingPuzzle.Interfaces;
using SlidingPuzzle.UI;

namespace SlidingPuzzle.Systems
{
    /// <summary>
    /// 인터스티셜 / 리워디드 광고 트리거.
    /// 성인 피벗 v0.4: L1 무광고, L2+ 시작/완료 시점에 인터스티셜 표시 + 무제한 rewarded.
    /// Editor 에선 시각적 MockEditorAdProvider 가 광고 UX 흐름을 시뮬레이트.
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        private const int    AdsEveryNRounds  = 5;
        private const string InterstitialId   = "Interstitial_Android";

        // 레벨 간 진행도(RoundsSinceAd) 보존을 위해 싱글톤 + DontDestroyOnLoad
        private static AdManager _instance;

        public static AdManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                if (!Application.isPlaying) return null;

                var existing = FindAnyObjectByType<AdManager>(FindObjectsInactive.Include);
                if (existing != null) { _instance = existing; return _instance; }

                var go = new GameObject(nameof(AdManager));
                _instance = go.AddComponent<AdManager>();
                return _instance;
            }
        }

        public int RoundsSinceAd { get; set; } = 0;

        /// <summary>실제 광고 SDK 대신 주입 가능한 프로바이더 (기본: 로그 출력)</summary>
        public IAdProvider AdProvider { get; set; } = new LogAdProvider();

        /// <summary>TC-LC-004 테스트용 — OnLevelComplete 호출 여부를 확인할 콜백</summary>
        public Action OnLevelCompleteCallback { get; set; }

        [Header("LevelPlay (IronSource) 설정")]
        [Tooltip("platform.ironsrc.com 에서 앱 등록 후 발급받은 App Key")]
        [SerializeField] private string levelPlayAppKey = "2622ec855";

        [Tooltip("개발 중 true — 테스트 광고 표시. 스토어 출시 직전 false 로 변경")]
        [SerializeField] private bool testMode = true;

        [Tooltip("Banner Ad Unit ID — ironSource 대시보드 > Ad Units > Banner 에서 발급")]
        [SerializeField] private string bannerAdUnitId = "qosis68wi51gr1wc";

        [Tooltip("Editor 에서도 LevelPlay 초기화 시도 (보통 false — 에디터에선 Mock 사용)")]
        [SerializeField] private bool useLevelPlayInEditor = false;

        [Tooltip("Editor 에서 시각적 Mock 광고 오버레이 표시 (LogAdProvider 대신). false 면 콘솔 로그만.")]
        [SerializeField] private bool useMockAdsInEditor = true;

        [Header("참고: Unity Ads Game ID (IronSource Dashboard 에서 Unity Ads 네트워크 설정 시 사용)")]
        [SerializeField] private string androidGameId = "6092073";
        [SerializeField] private string iosGameId     = "6092072";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // 빌드: LevelPlay (실 광고). Editor: 옵션에 따라 LevelPlay / Mock / Log
#if !UNITY_EDITOR
            AdProvider = new LevelPlayAdProvider(levelPlayAppKey, testMode, bannerAdUnitId);
#else
            if (useLevelPlayInEditor)
                AdProvider = new LevelPlayAdProvider(levelPlayAppKey, testMode, bannerAdUnitId);
            else if (useMockAdsInEditor)
                AdProvider = new MockEditorAdProvider(this);
            // else: LogAdProvider (필드 기본값) 유지
#endif
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        /// <summary>
        /// [Legacy] 매 5레벨마다 인터스티셜 — AdTriggerTests TC_AD_006 호환.
        /// 신규 호출처(GameplaySceneController 등)는 int 오버로드 (성인 피벗 v0.4) 사용.
        /// </summary>
        public void OnLevelComplete()
        {
            OnLevelCompleteCallback?.Invoke();

            RoundsSinceAd++;
            if (RoundsSinceAd >= AdsEveryNRounds)
            {
                AdProvider.Show(InterstitialId);
                RoundsSinceAd = 0;
            }
        }

        // ─────────────────────────────────────────────────────
        // 성인 피벗 v0.4 — L1 무광고, L2+ 시작/완료 인터스티셜
        // ─────────────────────────────────────────────────────

        /// <summary>
        /// 레벨 시작 시 호출 — 광고 정책 v0.5.1 (2026-04-27): 시작 광고 제거됨. 완료 시점에만 광고 표시.
        /// 호출 자체는 GameplayLoader 에 남아있어 분석/이벤트 로깅 용도로 활용 가능 (현재 no-op).
        /// 필요 시 아래 주석 두 줄 풀면 즉시 복구.
        /// </summary>
        public void OnLevelStart(int levelNumber)
        {
            // if (levelNumber <= 1) return;
            // AdProvider.Show(InterstitialId);
        }

        /// <summary>Gameplay 씬 진입 시 호출 — 화면 하단 배너 표시 (모든 레벨, L1 포함).</summary>
        public void ShowBanner()
        {
            AdProvider?.ShowBanner();
        }

        /// <summary>Gameplay 씬 이탈 시 호출 — 배너 제거 (LevelSelect/Complete 등 다른 씬에선 미표시).</summary>
        public void HideBanner()
        {
            AdProvider?.HideBanner();
        }

        /// <summary>레벨 완료 시 호출 — L2 부터 인터스티셜.</summary>
        public void OnLevelComplete(int levelNumber)
        {
            OnLevelCompleteCallback?.Invoke();
            if (levelNumber <= 1) return;
            AdProvider.Show(InterstitialId);
        }

        // ─────────────────────────────────────────────────────
        // Mock 광고 — Editor 시뮬레이터에서 시각적 검증용 + 빌드 fallback house ad
        // placementId == HouseAdFallbackPlacement 일 때 빌드 환경에서 fallback 으로 동작
        // ─────────────────────────────────────────────────────

        /// <summary>광고 fill 실패 시 fallback 으로 사용하는 placement ID.</summary>
        public const string HouseAdFallbackPlacement = "House_Hint_Fallback";

        /// <summary>MockEditorAdProvider 또는 fallback 호출 — 풀스크린 오버레이 + 카운트다운.</summary>
        public void ShowMockAd(string placementId, bool rewarded, Action onRewarded)
        {
            StartCoroutine(MockAdRoutine(placementId, rewarded, onRewarded));
        }

        private IEnumerator MockAdRoutine(string placementId, bool rewarded, Action onRewarded)
        {
            // 1) 풀스크린 Canvas (sortingOrder 32000 — 모든 UI 위)
            var canvasGo = new GameObject("MockAdOverlay",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode      = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder    = 32000;
            canvas.overrideSorting = true;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0f;

            // 2) 풀스크린 BG (불투명 검정)
            var bg = new GameObject("Bg",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bg.transform.SetParent(canvas.transform, worldPositionStays: false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin        = Vector2.zero;
            bgRt.anchorMax        = Vector2.one;
            bgRt.pivot            = new Vector2(0.5f, 0.5f);
            bgRt.sizeDelta        = Vector2.zero;
            bgRt.anchoredPosition = Vector2.zero;
            bgRt.offsetMin        = Vector2.zero;
            bgRt.offsetMax        = Vector2.zero;
            var bgImg = bg.GetComponent<Image>();
            bgImg.color         = new Color(0.05f, 0.05f, 0.05f, 1f); // 완전 불투명
            bgImg.raycastTarget = true; // 게임 클릭 차단

            // 3) 광고 종류 + placement
            bool isHouseFallback = placementId == HouseAdFallbackPlacement;
            string typeLabel = isHouseFallback
                ? "광고 시청 보상"
                : (rewarded ? "리워디드 광고 (Mock)" : "인터스티셜 광고 (Mock)");
            var title = DesignTokens.BuildText(canvas.transform, "TypeLabel", typeLabel,
                DesignTokens.FontHeading, FontStyle.Bold, DesignTokens.TextOnDark);
            var trt = (RectTransform)title.transform;
            trt.anchorMin = new Vector2(0.5f, 0.5f);
            trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.pivot     = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(900f, 100f);
            trt.anchoredPosition = new Vector2(0f, 380f);

            string subText = isHouseFallback
                ? "광고 인벤토리 부족 — 보상은 정상 지급됩니다"
                : $"placement: {placementId}";
            var sub = DesignTokens.BuildText(canvas.transform, "Placement",
                subText, DesignTokens.FontBody, FontStyle.Normal,
                new Color(1f, 1f, 1f, 0.6f));
            var srt = (RectTransform)sub.transform;
            srt.anchorMin = new Vector2(0.5f, 0.5f);
            srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.pivot     = new Vector2(0.5f, 0.5f);
            srt.sizeDelta = new Vector2(900f, 60f);
            srt.anchoredPosition = new Vector2(0f, 290f);

            // 4) 카운트다운 큰 텍스트 (144pt — 200pt 는 box 크기 이슈)
            float duration = rewarded ? 5f : 3f;
            var countdown = DesignTokens.BuildText(canvas.transform, "Countdown",
                $"{Mathf.CeilToInt(duration)}", 144, FontStyle.Bold, DesignTokens.CtaPrimary);
            var crt = (RectTransform)countdown.transform;
            crt.anchorMin = new Vector2(0.5f, 0.5f);
            crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.pivot     = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(500f, 220f);
            crt.anchoredPosition = Vector2.zero;

            // 5) 안내 텍스트
            var hint = DesignTokens.BuildText(canvas.transform, "Hint",
                rewarded ? "끝까지 시청하면 보상 지급" : "곧 자동 닫힘",
                DesignTokens.FontBody, FontStyle.Normal, new Color(1f, 1f, 1f, 0.7f));
            var hrt = (RectTransform)hint.transform;
            hrt.anchorMin = new Vector2(0.5f, 0.5f);
            hrt.anchorMax = new Vector2(0.5f, 0.5f);
            hrt.pivot     = new Vector2(0.5f, 0.5f);
            hrt.sizeDelta = new Vector2(900f, 60f);
            hrt.anchoredPosition = new Vector2(0f, -200f);

            // 6) 메타 표시 (좌하단 작게) — Mock 또는 fallback 안내
            string metaText = isHouseFallback
                ? "Puzzle Pals — 광고 시청 후 힌트 +1"
                : "Mock — 시뮬레이터 검증용 (실 디바이스에선 IronSource 광고)";
            var meta = DesignTokens.BuildText(canvas.transform, "Meta",
                metaText,
                DesignTokens.FontCaption, FontStyle.Italic, new Color(1f, 1f, 1f, 0.4f));
            var mrt = (RectTransform)meta.transform;
            mrt.anchorMin = new Vector2(0f, 0f);
            mrt.anchorMax = new Vector2(1f, 0f);
            mrt.pivot     = new Vector2(0.5f, 0f);
            mrt.sizeDelta = new Vector2(0f, 40f);
            mrt.anchoredPosition = new Vector2(0f, 60f);

            Debug.Log($"[MockAd] {typeLabel} 표시 시작 — {duration}s, sortingOrder={canvas.sortingOrder}");

            // 7) 카운트다운
            float remaining = duration;
            while (remaining > 0f)
            {
                remaining -= Time.unscaledDeltaTime; // timeScale 영향 안 받음
                if (countdown != null)
                    countdown.text = $"{Mathf.CeilToInt(remaining)}";
                yield return null;
            }

            // 8) 종료 — 보상 콜백 (rewarded only) 후 destroy
            Debug.Log($"[MockAd] {typeLabel} 종료 — {(rewarded ? "보상 지급" : "닫기")}");
            if (canvasGo != null) Destroy(canvasGo);
            if (rewarded) onRewarded?.Invoke();
        }

        // ─────────────────────────────────────────────────────
        // AdProvider 구현체들
        // ─────────────────────────────────────────────────────

        /// <summary>로그만 출력 (가장 단순한 fallback).</summary>
        private class LogAdProvider : IAdProvider
        {
            public void Show(string placementId)
                => Debug.Log($"[AdManager] Show ad: {placementId}");

            public void ShowRewarded(string placementId, Action onRewarded)
            {
                Debug.Log($"[AdManager] Show rewarded: {placementId} (Editor — 즉시 보상 지급)");
                onRewarded?.Invoke();
            }

            public void ShowRewarded(string placementId, Action onRewarded, Action onFailed)
                => ShowRewarded(placementId, onRewarded);

            public void ShowBanner() => Debug.Log("[AdManager] ShowBanner (log-only)");
            public void HideBanner() => Debug.Log("[AdManager] HideBanner (log-only)");
        }

        /// <summary>Editor 전용 시각적 Mock — AdManager 의 코루틴으로 풀스크린 오버레이 표시.</summary>
        private class MockEditorAdProvider : IAdProvider
        {
            private readonly AdManager _ad;
            public MockEditorAdProvider(AdManager ad) { _ad = ad; }

            public void Show(string placementId)
                => _ad.ShowMockAd(placementId, rewarded: false, onRewarded: null);

            public void ShowRewarded(string placementId, Action onRewarded)
                => _ad.ShowMockAd(placementId, rewarded: true, onRewarded: onRewarded);

            public void ShowRewarded(string placementId, Action onRewarded, Action onFailed)
                => ShowRewarded(placementId, onRewarded);

            // Editor Mock 에선 배너 시뮬레이션 생략 (필요 시 회색 띠 오버레이 추가 가능)
            public void ShowBanner() => Debug.Log("[AdManager] (Mock) ShowBanner");
            public void HideBanner() => Debug.Log("[AdManager] (Mock) HideBanner");
        }
    }
}
