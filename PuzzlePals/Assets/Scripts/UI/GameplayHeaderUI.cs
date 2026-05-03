using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PuzzlePals.Core;
using PuzzlePals.Gameplay;
using PuzzlePals.Puzzle;
using PuzzlePals.Systems;

namespace PuzzlePals.UI
{
    /// <summary>
    /// Gameplay 씬 상단 툴바 — 디자인 v0.6 (Pastel Puzzle / PlayClassic).
    ///
    /// 시안: document/design_handoff_pastel_puzzle/screens.jsx 의 PlayClassic.
    /// 구조:
    ///   ┌────────────────────────────────────────────────────────┐
    ///   │ [Back]      [Counter pill]      [Pause][Original][Hint]│
    ///   │────────────────────────────────────────────────────────│  ← 1px line
    ///   │                Progress bar (4dp)                       │
    ///   └────────────────────────────────────────────────────────┘
    ///
    /// 중앙 Counter pill: "{snapped} / {total}" — accentSoft bg + primaryDeep text.
    /// Progress bar: primary→gold gradient fill, snapped/total 비율.
    ///
    /// height 220 유지 — PuzzleBoard.Build() 의 viewport 스케일링 코드와 호환.
    /// HomeButton property 는 Back 별칭 (SceneTransitionTests TC_ST_003 호환).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class GameplayHeaderUI : MonoBehaviour
    {
        // ── 사이즈 토큰 (1080×1920 ref) ──
        // Progress bar 제거 — 사용자 피드백. ToolbarH = HeaderH 전체.
        private const float HeaderH       = 220f;
        private const float ToolbarH      = HeaderH;
        private const float SidePad       = 42f;  // 시안 14 × 3
        private const float ButtonGap     = 18f;  // 6 × 3
        private const float IconBtnSize   = 108f; // 36 × 3

        private Button _backButton;
        private Button _pauseButton;
        private Button _hintButton;
        private Button _originalButton;

        private GameObject _previewOverlay;
        private Image      _previewImage;

        // 힌트 버튼 우상단 배지 — hintCount > 0 면 숫자, 0 이면 광고 시청 아이콘
        private GameObject _hintBadgeNumber;
        private Text       _hintBadgeNumberTxt;
        private GameObject _hintBadgeAdIcon;
        private bool       _hintEventSubscribed;

        // Counter 갱신 캐시
        private GameplayLoader _loader;
        private Text           _counterTxt;
        private int            _lastSnapped = -1;
        private int            _lastTotal   = -1;

        /// <summary>Back 버튼 별칭 (구 API 호환 — 기존 테스트가 HomeButton 이름으로 GameObject 탐색)</summary>
        public Button HomeButton => _backButton;

        private void Awake() => EnsureBuilt();

        public Button EnsureBuilt()
        {
            if (_backButton == null) BuildUI();
            return _backButton;
        }

        private void OnEnable()
        {
            var hm = HintManager.Instance;
            if (hm != null && !_hintEventSubscribed)
            {
                hm.OnHintCountChanged += RefreshHintBadge;
                _hintEventSubscribed = true;
                RefreshHintBadge(hm.hintCount);
            }
            // 카운터 갱신용 loader 캐시
            _loader = FindAnyObjectByType<GameplayLoader>(FindObjectsInactive.Include);
        }

        private void OnDisable()
        {
            if (_hintEventSubscribed)
            {
                var hm = HintManager.Instance;
                if (hm != null) hm.OnHintCountChanged -= RefreshHintBadge;
                _hintEventSubscribed = false;
            }
        }

        private void Start()
        {
            if (!_hintEventSubscribed)
            {
                var hm = HintManager.Instance;
                if (hm != null)
                {
                    hm.OnHintCountChanged += RefreshHintBadge;
                    _hintEventSubscribed = true;
                    RefreshHintBadge(hm.hintCount);
                }
            }
            if (_loader == null)
                _loader = FindAnyObjectByType<GameplayLoader>(FindObjectsInactive.Include);
        }

        private void Update()
        {
            // Counter + Progress bar 갱신 — 값이 바뀌었을 때만 setter 호출
            if (_loader == null || _loader.SpawnedPieces == null) return;
            var pieces = _loader.SpawnedPieces;
            if (pieces.Length == 0) return;

            int snapped = 0;
            for (int i = 0; i < pieces.Length; i++)
                if (pieces[i] != null && pieces[i].IsSnapped) snapped++;

            int total = pieces.Length;
            if (snapped == _lastSnapped && total == _lastTotal) return;
            _lastSnapped = snapped;
            _lastTotal   = total;

            if (_counterTxt != null)
                _counterTxt.text = $"{snapped} / {total}";
        }

        // ─────────────────────────────────────────────────────
        // UI 빌드
        // ─────────────────────────────────────────────────────

        private void BuildUI()
        {
            var rt = transform as RectTransform;
            if (rt == null) return;

            // 기존 자식 모두 제거
            var children = new List<GameObject>();
            foreach (Transform c in transform) children.Add(c.gameObject);
            foreach (var go in children)
            {
                if (Application.isPlaying) Destroy(go);
                else                       DestroyImmediate(go);
            }

            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, HeaderH);
            rt.anchoredPosition = Vector2.zero;

            // Surface bg (시안 — 흰색 surface). 노치 영역까지 흰색 stretch (SafeAreaTopExtender 가 height 확장).
            var bg = gameObject.GetComponent<Image>();
            if (bg == null) bg = gameObject.AddComponent<Image>();
            bg.color         = PaletteTokens.Active.Surface;
            bg.raycastTarget = true;

            // 기존 SafeAreaFitter 제거 (이전 패턴) — SafeAreaTopExtender 로 교체
            var oldFitter = gameObject.GetComponent<SafeAreaFitter>();
            if (oldFitter != null)
            {
                if (Application.isPlaying) Destroy(oldFitter);
                else                       DestroyImmediate(oldFitter);
            }

            // ── Toolbar 섹션 (헤더 안 SafeArea inset) — SafeAreaTopExtender 가 offsetMax.y 조정 ──
            var toolbar = new GameObject("Toolbar", typeof(RectTransform));
            toolbar.transform.SetParent(transform, worldPositionStays: false);
            var tbrt = (RectTransform)toolbar.transform;
            tbrt.anchorMin = new Vector2(0f, 0f);
            tbrt.anchorMax = new Vector2(1f, 1f);
            tbrt.offsetMin = Vector2.zero;
            tbrt.offsetMax = Vector2.zero;

            // SafeAreaTopExtender — 헤더 height 확장 + Toolbar 를 SafeArea 안으로 inset.
            // OnEnable 시점엔 innerContent 가 null 이므로, set 후 Refresh() 로 명시 재적용 필요.
            var ext = gameObject.GetComponent<SafeAreaTopExtender>();
            if (ext == null) ext = gameObject.AddComponent<SafeAreaTopExtender>();
            ext.innerContent = tbrt;
            ext.Refresh();

            // Back — 좌측
            _backButton = BuildIconButton(toolbar.transform, "HomeButton", "arrow_back", "btn_back", "뒤로");
            var backRt = (RectTransform)_backButton.transform;
            backRt.anchorMin = new Vector2(0f, 0.5f);
            backRt.anchorMax = new Vector2(0f, 0.5f);
            backRt.pivot     = new Vector2(0f, 0.5f);
            backRt.anchoredPosition = new Vector2(SidePad, 0f);

            // 우측 그룹 (Pause / Original / Hint) — 우측 끝부터 좌측으로
            // 순서: Hint (가장 우측), Original, Pause
            _hintButton = BuildIconButton(toolbar.transform, "HintButton", "lightbulb", "btn_hint", "힌트", highlight: true);
            var hintRt = (RectTransform)_hintButton.transform;
            hintRt.anchorMin = new Vector2(1f, 0.5f);
            hintRt.anchorMax = new Vector2(1f, 0.5f);
            hintRt.pivot     = new Vector2(1f, 0.5f);
            hintRt.anchoredPosition = new Vector2(-SidePad, 0f);
            BuildHintBadges(_hintButton.transform);

            _originalButton = BuildIconButton(toolbar.transform, "OriginalButton", "visibility", "btn_preview", "원본");
            var origRt = (RectTransform)_originalButton.transform;
            origRt.anchorMin = new Vector2(1f, 0.5f);
            origRt.anchorMax = new Vector2(1f, 0.5f);
            origRt.pivot     = new Vector2(1f, 0.5f);
            origRt.anchoredPosition = new Vector2(-(SidePad + IconBtnSize + ButtonGap), 0f);

            // Restart 버튼 (이전 Pause 자리) — refresh 아이콘. 사용자 피드백: 일시정지보다 reset 이 유용
            _pauseButton = BuildIconButton(toolbar.transform, "RestartButton", "refresh", "btn_restart", "초기화");
            var pauseRt = (RectTransform)_pauseButton.transform;
            pauseRt.anchorMin = new Vector2(1f, 0.5f);
            pauseRt.anchorMax = new Vector2(1f, 0.5f);
            pauseRt.pivot     = new Vector2(1f, 0.5f);
            pauseRt.anchoredPosition = new Vector2(-(SidePad + IconBtnSize * 2f + ButtonGap * 2f), 0f);

            // Counter pill — 가운데
            BuildCounterPill(toolbar.transform);

            WireButtons();
        }

        // ─────────────────────────────────────────────────────
        // 공용 IconButton — 시안 톤 (surfaceAlt bg, radius height/2.4, 라벨 없음)
        // ─────────────────────────────────────────────────────

        private Button BuildIconButton(Transform parent, string name, string iconKey,
                                       string labelEntryKey, string labelFallback,
                                       bool highlight = false)
        {
            // PaletteUI.BuildIconBtn 재사용 — 시안 IconBtn 과 동일
            var btn = PaletteUI.BuildIconBtn(parent, name, iconKey, IconBtnSize, highlight);
            var go = btn.gameObject;
            // sizeDelta 는 BuildIconBtn 에서 set 됨
            return btn;
        }

        // ─────────────────────────────────────────────────────
        // Counter pill — accentSoft bg + primaryDeep text + sparkle 아이콘
        // 시안: padding 6/12 × 3 = 18/36, fontSize 12 → 36, weight 700
        // ─────────────────────────────────────────────────────

        private void BuildCounterPill(Transform parent)
        {
            var p = PaletteTokens.Active;
            const float pillH = 78f;     // 26 × 3 (padding 6+6 + content ~14)
            const float pillW = 264f;    // 88 × 3 — "36 / 72" + sparkle + padding 충분
            int radius = (int)(pillH / 2f);

            var pill = PaletteUI.BuildSurface(parent, "Counter", p.AccentSoft, radius);
            var rt = (RectTransform)pill.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(pillW, pillH);
            rt.anchoredPosition = Vector2.zero;

            // 퍼즐 아이콘 (좌측) — 진행 상황 표시
            var icon = DesignTokens.BuildIcon(pill.transform, "Puzzle", "puzzle", 36, p.PrimaryDeep);
            var iRt = (RectTransform)icon.transform;
            iRt.anchorMin = new Vector2(0f, 0.5f);
            iRt.anchorMax = new Vector2(0f, 0.5f);
            iRt.pivot     = new Vector2(0f, 0.5f);
            iRt.sizeDelta = new Vector2(36f, 36f);
            iRt.anchoredPosition = new Vector2(36f, 0f);

            // 카운터 텍스트 — sparkle 우측, 가운데 정렬
            _counterTxt = DesignTokens.BuildText(pill.transform, "CounterTxt", "0 / 0",
                PaletteTokens.FontCaption, FontStyle.Bold, p.PrimaryDeep,
                TextAnchor.MiddleCenter);
            _counterTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            _counterTxt.verticalOverflow   = VerticalWrapMode.Overflow;
            var ctRt = (RectTransform)_counterTxt.transform;
            ctRt.anchorMin = new Vector2(0f, 0f);
            ctRt.anchorMax = new Vector2(1f, 1f);
            ctRt.offsetMin = new Vector2(72f, 0f); // sparkle 자리 비움
            ctRt.offsetMax = new Vector2(-24f, 0f);
        }

        // ─────────────────────────────────────────────────────
        // 힌트 배지 (우상단) — Hint 버튼의 자식
        // ─────────────────────────────────────────────────────

        private void BuildHintBadges(Transform hintButtonTransform)
        {
            // 1) 숫자 배지 — 흰 원
            _hintBadgeNumber = new GameObject("Badge_Number",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            _hintBadgeNumber.transform.SetParent(hintButtonTransform, worldPositionStays: false);
            var nrt = (RectTransform)_hintBadgeNumber.transform;
            nrt.anchorMin = new Vector2(1f, 1f);
            nrt.anchorMax = new Vector2(1f, 1f);
            nrt.pivot     = new Vector2(0.5f, 0.5f);
            nrt.sizeDelta = new Vector2(46f, 46f);
            nrt.anchoredPosition = new Vector2(-12f, -12f);

            var nImg = _hintBadgeNumber.GetComponent<Image>();
            nImg.sprite        = DesignTokens.CircleSprite;
            nImg.color         = PaletteTokens.Active.Surface;
            nImg.raycastTarget = false;

            _hintBadgeNumberTxt = DesignTokens.BuildText(_hintBadgeNumber.transform, "Num", "0",
                26, FontStyle.Bold, PaletteTokens.Active.PrimaryDeep);
            var ntrt = (RectTransform)_hintBadgeNumberTxt.transform;
            ntrt.anchorMin = Vector2.zero;
            ntrt.anchorMax = Vector2.one;
            ntrt.offsetMin = Vector2.zero;
            ntrt.offsetMax = Vector2.zero;

            // 2) 광고 아이콘 배지 — 어두운 원 + ▶
            _hintBadgeAdIcon = new GameObject("Badge_Ad",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            _hintBadgeAdIcon.transform.SetParent(hintButtonTransform, worldPositionStays: false);
            var art = (RectTransform)_hintBadgeAdIcon.transform;
            art.anchorMin = new Vector2(1f, 1f);
            art.anchorMax = new Vector2(1f, 1f);
            art.pivot     = new Vector2(0.5f, 0.5f);
            art.sizeDelta = new Vector2(46f, 46f);
            art.anchoredPosition = new Vector2(-12f, -12f);

            var aImg = _hintBadgeAdIcon.GetComponent<Image>();
            aImg.sprite        = DesignTokens.CircleSprite;
            aImg.color         = PaletteTokens.Active.Text;
            aImg.raycastTarget = false;

            var adGlyph = DesignTokens.BuildIcon(_hintBadgeAdIcon.transform, "Icon", "play_arrow",
                28, Color.white);
            var grt = (RectTransform)adGlyph.transform;
            grt.anchorMin = Vector2.zero;
            grt.anchorMax = Vector2.one;
            grt.offsetMin = Vector2.zero;
            grt.offsetMax = Vector2.zero;

            _hintBadgeNumber.SetActive(false);
            _hintBadgeAdIcon.SetActive(false);
        }

        private void RefreshHintBadge(int count)
        {
            if (_hintBadgeNumber == null || _hintBadgeAdIcon == null) return;

            if (count > 0)
            {
                _hintBadgeNumber.SetActive(true);
                _hintBadgeAdIcon.SetActive(false);
                if (_hintBadgeNumberTxt != null)
                    _hintBadgeNumberTxt.text = count > 99 ? "99+" : count.ToString();
            }
            else
            {
                _hintBadgeNumber.SetActive(false);
                _hintBadgeAdIcon.SetActive(true);
            }
        }

        private void WireButtons()
        {
            if (_backButton != null)
            {
                _backButton.onClick.RemoveAllListeners();
                _backButton.onClick.AddListener(OnBack);
            }
            if (_pauseButton != null)
            {
                _pauseButton.onClick.RemoveAllListeners();
                _pauseButton.onClick.AddListener(OnRestart);
            }
            if (_hintButton != null)
            {
                _hintButton.onClick.RemoveAllListeners();
                _hintButton.onClick.AddListener(OnHint);
            }
            if (_originalButton != null)
            {
                _originalButton.onClick.RemoveAllListeners();
                _originalButton.onClick.AddListener(ShowOriginalImage);
            }
        }

        // ─────────────────────────────────────────────────────
        // 버튼 핸들러
        // ─────────────────────────────────────────────────────

        private void OnBack() => SceneLoader.LoadScene(SceneLoader.LevelSelect);

        // Restart — 보드 다시 빌드 (조각 위치 / 진행도 초기화).
        private void OnRestart()
        {
            var loader = _loader != null
                ? _loader
                : FindAnyObjectByType<GameplayLoader>(FindObjectsInactive.Include);
            if (loader != null) loader.Build();
            else Debug.LogWarning("[Toolbar] GameplayLoader 못 찾음 — Restart 스킵");
        }

        private void OnHint()
        {
            var hm = HintManager.Instance;
            if (hm == null)
            {
                Debug.LogWarning("[Toolbar] HintManager 없음 — 힌트 사용 불가");
                return;
            }

            if (hm.hintCount > 0)
            {
                hm.UseHint();
                return;
            }

            var ad = AdManager.Instance?.AdProvider;
            if (ad == null)
            {
                ShowHintFeedback("hint_ad_unavailable", "광고 시스템 준비 안 됨");
                Debug.LogWarning("[Toolbar] AdProvider 없음 — 힌트 획득 경로 막힘");
                return;
            }

            Debug.Log("[Toolbar] 힌트 0개 — 리워디드 광고 표시 시도");
            ShowHintFeedback("hint_ad_loading", "광고를 불러오는 중...");

            bool rewardReceived = false;
            bool fallbackTriggered = false;
            ad.ShowRewarded("Rewarded_Android",
                onRewarded: () =>
                {
                    rewardReceived = true;
                    hm.OnRewardedHintEarned();
                    ShowHintFeedback("hint_ad_rewarded", "광고 시청 완료! 힌트 +1");
                },
                onFailed: () =>
                {
                    if (fallbackTriggered) return;
                    fallbackTriggered = true;
                    rewardReceived = true; // 타임아웃 코루틴 중복 진입 방지
                    RunFallbackHouseAd(hm);
                });

            // 8초 안에 SDK 콜백 (보상/실패) 미수신이면 — SDK 가 응답 없는 것으로 판단 → fallback 진입.
            StartCoroutine(WatchRewardOutcome(() => rewardReceived, () =>
            {
                if (fallbackTriggered) return;
                fallbackTriggered = true;
                rewardReceived = true;
                Debug.LogWarning("[Toolbar] SDK 응답 시간초과 — fallback house ad 진입");
                RunFallbackHouseAd(hm);
            }));
        }

        // 광고 fill 실패 / SDK 응답 없을 때 fallback — 자체 풀스크린 5초 후 보상 지급.
        private void RunFallbackHouseAd(HintManager hm)
        {
            var adMgr = AdManager.Instance;
            if (adMgr != null)
            {
                adMgr.ShowMockAd(AdManager.HouseAdFallbackPlacement, rewarded: true, () =>
                {
                    hm.OnRewardedHintEarned();
                    ShowHintFeedback("hint_ad_rewarded", "광고 시청 완료! 힌트 +1");
                });
            }
            else
            {
                // AdManager 없는 극단 케이스 — 즉시 보상 지급 (UX 보장)
                hm.OnRewardedHintEarned();
                ShowHintFeedback("hint_ad_rewarded", "광고 시청 완료! 힌트 +1");
            }
        }

        private System.Collections.IEnumerator WatchRewardOutcome(System.Func<bool> rewardedCheck, System.Action onTimeout)
        {
            // SDK 응답 대기 — 8초로 단축. 8초 안에 onRewarded/onFailed 콜백 미수신이면 fallback 진입.
            // 정상 광고는 SDK 가 이 시간 안에 onAvailable + showRewardedVideo() 까지 진행함.
            const float MaxAdDurationSeconds = 8f;
            yield return new UnityEngine.WaitForSecondsRealtime(MaxAdDurationSeconds);
            if (!rewardedCheck())
            {
                onTimeout?.Invoke();
            }
        }

        private void ShowHintFeedback(string entryKey, string fallback)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var existing = canvas.transform.Find("HintFeedbackToast");
            if (existing != null) Destroy(existing.gameObject);

            var toast = PaletteUI.BuildToast(canvas.transform, fallback, 1.8f, entryKey);
            toast.name = "HintFeedbackToast";
            // 헤더 바로 아래로 위치 조정
            var rt = (RectTransform)toast.transform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -(HeaderH + 36f));
        }

        // ─────────────────────────────────────────────────────
        // Original 이미지 풀스크린 오버레이 (탭해서 닫기)
        // ─────────────────────────────────────────────────────

        private void ShowOriginalImage()
        {
            if (_previewOverlay == null) BuildPreviewOverlay();

            var sprite = _loader != null && _loader.Level != null ? _loader.Level.characterSprite : null;
            if (sprite != null && _previewImage != null)
            {
                _previewImage.sprite         = sprite;
                _previewImage.preserveAspect = true;
            }
            _previewOverlay.SetActive(true);
        }

        private void BuildPreviewOverlay()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[Toolbar] 부모 Canvas 없음 — Preview 오버레이 생성 불가");
                return;
            }

            var overlay = new GameObject("PreviewOverlay",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            overlay.transform.SetParent(canvas.transform, worldPositionStays: false);

            var ort = (RectTransform)overlay.transform;
            ort.anchorMin = Vector2.zero;
            ort.anchorMax = Vector2.one;
            ort.offsetMin = Vector2.zero;
            ort.offsetMax = Vector2.zero;
            ort.SetAsLastSibling();

            overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);
            overlay.GetComponent<Button>().onClick.AddListener(() => overlay.SetActive(false));

            // 중앙 이미지
            var imgGo = new GameObject("OriginalImage",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imgGo.transform.SetParent(overlay.transform, worldPositionStays: false);
            var irt = (RectTransform)imgGo.transform;
            irt.anchorMin = new Vector2(0.05f, 0.1f);
            irt.anchorMax = new Vector2(0.95f, 0.9f);
            irt.offsetMin = Vector2.zero;
            irt.offsetMax = Vector2.zero;
            _previewImage = imgGo.GetComponent<Image>();
            _previewImage.raycastTarget = false;

            // 안내 텍스트
            var hintGo = new GameObject("TapToClose",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            hintGo.transform.SetParent(overlay.transform, worldPositionStays: false);
            var hrt = (RectTransform)hintGo.transform;
            hrt.anchorMin = new Vector2(0f, 0f);
            hrt.anchorMax = new Vector2(1f, 0f);
            hrt.pivot     = new Vector2(0.5f, 0f);
            hrt.sizeDelta = new Vector2(0f, 80f);
            hrt.anchoredPosition = new Vector2(0f, 20f);

            var hintTxt = hintGo.GetComponent<Text>();
            hintTxt.text          = "탭해서 닫기";
            hintTxt.alignment     = TextAnchor.MiddleCenter;
            hintTxt.color         = new Color(1f, 1f, 1f, 0.7f);
            hintTxt.fontSize      = 32;
            hintTxt.font          = DesignTokens.Regular;
            hintTxt.raycastTarget = false;

            var binder = hintGo.AddComponent<LocalizedTextBinder>();
            binder.EntryKey = "tap_to_close";

            _previewOverlay = overlay;
            _previewOverlay.SetActive(false);
        }
    }
}
