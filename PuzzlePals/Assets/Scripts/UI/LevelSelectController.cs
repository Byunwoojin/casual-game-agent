using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using PuzzlePals.Core;
using PuzzlePals.Data;
using PuzzlePals.Systems;

namespace PuzzlePals.UI
{
    /// <summary>
    /// LevelSelect 씬 컨트롤러 — 디자인 v0.6 (HomeSequential / Pastel Puzzle).
    ///
    /// 시안: document/design_handoff_pastel_puzzle/more-screens.jsx 의 HomeSequential.
    /// 구조: Header → Progress → 지금 도전 카드 → "전체 작품" 섹션 라벨 → 3-col 그리드 (16 레벨) → BannerAd → BottomNav.
    ///
    /// Layout 정책: nested LayoutGroup 회피. 모든 sticky 섹션은 명시적 anchor + 고정 height,
    /// 섹션 내부도 anchor 로 직접 배치. LayoutGroup 은 GridLayoutGroup (그리드) + HorizontalLayoutGroup (BottomNav 4탭) 만 사용.
    /// </summary>
    [DisallowMultipleComponent]
    public class LevelSelectController : MonoBehaviour
    {
        private const string LevelsResourcePath = "Levels";

        // ── Reference resolution (1080×1920) ──
        private const float RefW = 1080f;
        private const float RefH = 1920f;

        // ── 섹션 고정 height ──
        private const float HeaderH       = 210f;
        private const float ProgressH     = 180f;
        private const float CurrentH      = 330f;
        private const float SectionLabelH = 96f;
        private const float BannerH       = PaletteUI.BannerAdHeight; // 168
        private const float NavH          = PaletteUI.NavHeight; // 162 (시안 padding 8/10 + content 36 ×3) — Gallery 와 통일

        // ── 시안 px × 3 (각 섹션 패딩) ──
        private const float HeaderHPad    = 60f;  // 20 × 3
        private const float HeaderVPadTop = 42f;  // 14 × 3

        private const float ProgressHPad  = 60f;
        private const float ProgressVPad  = 42f;

        private const float CurrentOuterHPad = 48f; // 16 × 3
        private const float CurrentOuterVTop = 42f; // 14 × 3
        private const float CurrentOuterVBot = 24f; // 8 × 3
        private const float CurrentCardPad   = 36f; // 12 × 3
        private const float CurrentThumbSize = 192f;// 64 × 3
        private const int   CurrentThumbRadius = 30; // 10 × 3
        private const int   CurrentCardRadius  = 48; // 16 × 3
        private const float CurrentCtaHeight   = 96f;  // 32 × 3 (시안 padding 10/10 + text ~12)

        private const float SectionLabelHPad = 48f;
        private const float SectionLabelVTop = 24f;
        private const float SectionLabelVBot = 36f;

        private const float GridHPad = 48f;
        private const float GridGap  = 30f;
        private const int   GridCols = 3;

        // ── 버킷 헤더 (E/M/H/VH) ──
        private const float BucketHeaderH    = 72f; // 24 × 3
        private const float BucketHeaderVTop = 24f;
        private const float BucketGapAfterGrid = 36f; // 12 × 3 — 다음 버킷 헤더 전 여백

        // 16레벨 기준 마일스톤
        private static readonly int[] Milestones16 = { 5, 10, 15 };

        private GameObject _toastGO;
        private Canvas     _canvas;

        // SafeArea top inset (ref 좌표계). Build() 시 한 번 측정.
        private float _topInsetRef;

        private void Start() => Build();

        public void Build()
        {
            // 1) 자식 정리 (Setup 도구 재실행 호환)
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
                else                       DestroyImmediate(transform.GetChild(i).gameObject);
            }

            // 2) Canvas
            _canvas = BuildCanvas();

            // 3) Background — 단색
            BuildFullStretch(_canvas.transform, "Background", PaletteTokens.Active.Bg);

            // 4) 레벨 데이터 로드
            var levels = Resources.LoadAll<LevelDataSO>(LevelsResourcePath);
            if (levels == null || levels.Length == 0)
            {
                Debug.LogWarning($"[LevelSelect] Resources/{LevelsResourcePath}/ 에 LevelDataSO 없음");
                return;
            }
            Array.Sort(levels, (a, b) => a.levelNumber.CompareTo(b.levelNumber));

            int total       = levels.Length;
            int cleared     = SaveManager.GetClearedCount(total);
            int currentIdx  = SaveManager.GetCurrentLevelIndex(total);
            bool allCleared = currentIdx >= total;

            // SafeArea top inset 측정 (status bar / 노치 만큼). Header 와 모든 sticky 섹션을 push.
            _topInsetRef = ComputeTopInsetRef();

            // 5) Sticky 섹션들 (anchor + 고정 height)
            float topY = 0f;

            BuildHeader        (_canvas.transform, topY);                 topY += HeaderH + _topInsetRef;
            BuildProgress      (_canvas.transform, topY, cleared, total); topY += ProgressH;
            BuildCurrentChallenge(_canvas.transform, topY,
                levels[Mathf.Clamp(currentIdx, 0, total - 1)],
                Mathf.Clamp(currentIdx, 0, total - 1), allCleared);       topY += CurrentH;
            BuildSectionLabelWrap(_canvas.transform, topY);               topY += SectionLabelH;

            // 6) ScrollGrid — 중간 영역 (sticky 섹션 위/아래 사이). anchor stretch + offset inset 으로
            //   디바이스 height 무관 자동 fit (RefH hardcode 안 씀 — 1080×2340 같은 긴 디바이스 대응).
            float bottomInsetRef = ComputeBottomInsetRef();
            float scrollTop    = topY;
            float scrollBottom = NavH + bottomInsetRef;
            BuildScrollGrid(_canvas.transform, scrollTop, scrollBottom, levels, currentIdx);

            // 7) BottomNav 만 — Gameplay 외 씬은 배너 미표시 (사용자 결정).
            BuildNav(_canvas.transform);
        }

        // ─────────────────────────────────────────────────────
        // 기본 구조
        // ─────────────────────────────────────────────────────

        private Canvas BuildCanvas()
        {
            var go = new GameObject("LevelSelectCanvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, worldPositionStays: false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(RefW, RefH);
            scaler.matchWidthOrHeight  = 0f; // Width 고정 — 메모리 규칙

            return canvas;
        }

        /// <summary>parent 에 풀 스트레치 (anchor stretch, full inset 0) 단색 Image 생성.</summary>
        private GameObject BuildFullStretch(Transform parent, string name, Color color)
        {
            var go = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, worldPositionStays: false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        /// <summary>Canvas 좌상단 기준 anchor (top-stretch) 으로 섹션 컨테이너 생성. topY 만큼 아래로, height 만큼 차지.</summary>
        private GameObject BuildSection(Transform parent, string name, float topY, float height, Color bg)
        {
            var go = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, worldPositionStays: false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, height);
            rt.anchoredPosition = new Vector2(0f, -topY);
            go.GetComponent<Image>().color = bg;
            return go;
        }

        private void BuildBottomBorder(Transform parent, Color color)
        {
            var border = new GameObject("BottomBorder",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            border.transform.SetParent(parent, worldPositionStays: false);
            var rt = (RectTransform)border.transform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot     = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, 3f); // 1 × 3
            rt.anchoredPosition = Vector2.zero;
            border.GetComponent<Image>().color = color;
            border.GetComponent<Image>().raycastTarget = false;
        }

        // ─────────────────────────────────────────────────────
        // Header — greeting + title (좌), CoinPill + Settings (우)
        // ─────────────────────────────────────────────────────

        private float ComputeTopInsetRef()
        {
            if (Screen.height <= 0) return 0f;
            // SafeArea reported 가 비정상 (0 또는 화면의 10% 초과) 이면 표준 status bar 추정값 (4%) fallback.
            float topInsetDevice = Screen.height - (Screen.safeArea.y + Screen.safeArea.height);
            float minInset = Mathf.Max(75f, Screen.height * 0.04f);
            float maxInset = Screen.height * 0.10f;
            if (topInsetDevice < minInset || topInsetDevice > maxInset)
                topInsetDevice = minInset;

            // Canvas logical height 기준 변환 (ScaleWithScreenSize 정확)
            float canvasH = RefH;
            if (_canvas != null)
            {
                var canvasRt = _canvas.transform as RectTransform;
                if (canvasRt != null) canvasH = canvasRt.rect.height;
            }
            return topInsetDevice * (canvasH / Screen.height);
        }

        private float ComputeBottomInsetRef()
        {
            if (Screen.height <= 0) return 0f;
            float bottomInsetDevice = Screen.safeArea.y;  // home indicator 영역
            if (bottomInsetDevice < 0f) bottomInsetDevice = 0f;
            // 비정상 (화면의 8% 초과) 이면 0 으로 clamp
            if (bottomInsetDevice > Screen.height * 0.08f) bottomInsetDevice = 0f;
            float canvasH = RefH;
            if (_canvas != null)
            {
                var canvasRt = _canvas.transform as RectTransform;
                if (canvasRt != null) canvasH = canvasRt.rect.height;
            }
            return bottomInsetDevice * (canvasH / Screen.height);
        }

        private void BuildHeader(Transform parent, float topY)
        {
            var p = PaletteTokens.Active;
            // Header 자체는 화면 끝까지 (sizeDelta = HeaderH + _topInsetRef) — Surface 색이 노치 영역까지 stretch
            var header = BuildSection(parent, "Header", topY, HeaderH + _topInsetRef, p.Surface);
            BuildBottomBorder(header.transform, p.Line);

            // Inner — SafeArea 안 컨테이너. 자식들 (greeting/title/Settings) 의 parent.
            var inner = new GameObject("Inner", typeof(RectTransform));
            inner.transform.SetParent(header.transform, worldPositionStays: false);
            var innerRt = (RectTransform)inner.transform;
            innerRt.anchorMin = new Vector2(0f, 0f);
            innerRt.anchorMax = new Vector2(1f, 1f);
            innerRt.offsetMin = Vector2.zero;
            innerRt.offsetMax = new Vector2(0f, -_topInsetRef); // top 만큼 push
            // 이후 자식들은 inner.transform 에 부착
            var headerInner = inner.transform;
            BuildHeaderInner(headerInner);
        }

        private void BuildHeaderInner(Transform inner)
        {
            var p = PaletteTokens.Active;

            // Greeting — top-left, anchor top
            var greeting = DesignTokens.BuildLocalizedText(inner, "Greeting",
                "home_greeting", "오늘도 반가워요",
                PaletteTokens.FontCaptionSm, FontStyle.Normal, p.TextSoft, TextAnchor.UpperLeft);
            greeting.horizontalOverflow = HorizontalWrapMode.Overflow;
            greeting.verticalOverflow   = VerticalWrapMode.Overflow;
            var gRt = (RectTransform)greeting.transform;
            gRt.anchorMin = new Vector2(0f, 1f);
            gRt.anchorMax = new Vector2(0.6f, 1f);
            gRt.pivot     = new Vector2(0f, 1f);
            gRt.sizeDelta = new Vector2(0f, PaletteTokens.FontCaptionSm + 9);
            gRt.offsetMin = new Vector2(HeaderHPad, gRt.offsetMin.y);
            gRt.offsetMax = new Vector2(0f, gRt.offsetMax.y);
            gRt.anchoredPosition = new Vector2(gRt.anchoredPosition.x, -HeaderVPadTop);

            // Title
            float titleY = HeaderVPadTop + (PaletteTokens.FontCaptionSm + 9) + 6;
            var title = DesignTokens.BuildLocalizedText(inner, "Title",
                "home_title", "그림 한 점, 시작해 볼까요",
                PaletteTokens.FontDisplay, FontStyle.Bold, p.Text, TextAnchor.UpperLeft);
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            title.verticalOverflow   = VerticalWrapMode.Overflow;
            var tRt = (RectTransform)title.transform;
            tRt.anchorMin = new Vector2(0f, 1f);
            tRt.anchorMax = new Vector2(0.7f, 1f);
            tRt.pivot     = new Vector2(0f, 1f);
            tRt.sizeDelta = new Vector2(0f, PaletteTokens.FontDisplay + 18);
            tRt.offsetMin = new Vector2(HeaderHPad, tRt.offsetMin.y);
            tRt.offsetMax = new Vector2(0f, tRt.offsetMax.y);
            tRt.anchoredPosition = new Vector2(tRt.anchoredPosition.x, -titleY);

            // Settings — middle-right (CoinPill 제거: 결제/포인트 시스템 안 씀)
            const float iconBtnSize = 108f; // 36 × 3
            var settingsBtn = PaletteUI.BuildIconBtn(inner, "Settings",
                "settings", iconBtnSize, highlight: false);
            var sRt = (RectTransform)settingsBtn.transform;
            sRt.anchorMin = new Vector2(1f, 0.5f);
            sRt.anchorMax = new Vector2(1f, 0.5f);
            sRt.pivot     = new Vector2(1f, 0.5f);
            sRt.sizeDelta = new Vector2(iconBtnSize, iconBtnSize);
            sRt.anchoredPosition = new Vector2(-HeaderHPad, 0f);
            settingsBtn.onClick.AddListener(OnSettings);
        }

        // ─────────────────────────────────────────────────────
        // Progress — "X점 / N점 완성" + 마일스톤 안내 + ProgressBar
        // ─────────────────────────────────────────────────────

        private void BuildProgress(Transform parent, float topY, int cleared, int total)
        {
            var p = PaletteTokens.Active;
            var progress = BuildSection(parent, "Progress", topY, ProgressH, p.Surface);
            BuildBottomBorder(progress.transform, p.Line);

            // Stat (left) — "{cleared}점 / total점 완성" — rich text 로 부분 색상 처리
            // 다국어 진입점은 단순 format 키 (color span 은 fallback 만 적용 — Locale 변경 시 색강조 사라짐).
            //   현 시점 비주얼 우선순위 낮아 trade-off 수용.
            string statFallback =
                $"<color=#{ColorUtility.ToHtmlStringRGB(p.PrimaryDeep)}><b>{cleared}점</b></color>" +
                $" <color=#{ColorUtility.ToHtmlStringRGB(p.TextSoft)}>/ {total}점 완성</color>";
            var stat = DesignTokens.BuildLocalizedTextFormat(progress.transform, "Stat",
                "level_progress_stat", statFallback, new object[] { cleared, total },
                PaletteTokens.FontBody, FontStyle.Bold, p.Text, TextAnchor.UpperLeft);
            stat.supportRichText = true;
            var sRt = (RectTransform)stat.transform;
            sRt.anchorMin = new Vector2(0f, 1f);
            sRt.anchorMax = new Vector2(0.6f, 1f);
            sRt.pivot     = new Vector2(0f, 1f);
            sRt.sizeDelta = new Vector2(0f, PaletteTokens.FontBody + 18);
            sRt.offsetMin = new Vector2(ProgressHPad, sRt.offsetMin.y);
            sRt.offsetMax = new Vector2(0f, sRt.offsetMax.y);
            sRt.anchoredPosition = new Vector2(sRt.anchoredPosition.x, -ProgressVPad);

            // Milestone info (right)
            int nextMilestone = NextMilestone(cleared, total);
            int milestoneRemain = nextMilestone > 0 ? (nextMilestone - cleared) : 0;
            string milestoneFallback = nextMilestone > 0
                ? $"다음 마일스톤까지 +{milestoneRemain}점"
                : "마지막 도전";
            Text info;
            if (nextMilestone > 0)
            {
                info = DesignTokens.BuildLocalizedTextFormat(progress.transform, "MilestoneInfo",
                    "level_milestone_next", milestoneFallback, new object[] { milestoneRemain },
                    PaletteTokens.FontCaptionSm, FontStyle.Normal, p.TextSoft, TextAnchor.UpperRight);
            }
            else
            {
                info = DesignTokens.BuildLocalizedText(progress.transform, "MilestoneInfo",
                    "level_milestone_last", milestoneFallback,
                    PaletteTokens.FontCaptionSm, FontStyle.Normal, p.TextSoft, TextAnchor.UpperRight);
            }
            var iRt = (RectTransform)info.transform;
            iRt.anchorMin = new Vector2(0.4f, 1f);
            iRt.anchorMax = new Vector2(1f, 1f);
            iRt.pivot     = new Vector2(1f, 1f);
            iRt.sizeDelta = new Vector2(0f, PaletteTokens.FontCaptionSm + 9);
            iRt.offsetMin = new Vector2(0f, iRt.offsetMin.y);
            iRt.offsetMax = new Vector2(-ProgressHPad, iRt.offsetMax.y);
            iRt.anchoredPosition = new Vector2(iRt.anchoredPosition.x, -ProgressVPad);

            // ProgressBar — anchor top stretch, 좌우 padding
            float barWidth = RefW - ProgressHPad * 2f;
            float ratio    = total > 0 ? (float)cleared / total : 0f;
            float barY     = ProgressVPad + (PaletteTokens.FontBody + 18) + 24f; // stat height + gap
            var bar = PaletteUI.BuildProgressBar(progress.transform, "Bar",
                ratio, total, Milestones16, barWidth);
            var bRt = (RectTransform)bar.transform;
            bRt.anchorMin = new Vector2(0f, 1f);
            bRt.anchorMax = new Vector2(0f, 1f);
            bRt.pivot     = new Vector2(0f, 1f);
            // sizeDelta 는 BuildProgressBar 에서 set (width, 18)
            bRt.anchoredPosition = new Vector2(ProgressHPad, -barY);
        }

        private static int NextMilestone(int cleared, int total)
        {
            foreach (var m in Milestones16) if (m > cleared && m < total) return m;
            return -1;
        }

        // ─────────────────────────────────────────────────────
        // Current Challenge — 카드 안에 thumbnail + 텍스트 + CTA
        // ─────────────────────────────────────────────────────

        private void BuildCurrentChallenge(Transform parent, float topY,
                                           LevelDataSO level, int index, bool allCleared)
        {
            var p = PaletteTokens.Active;

            // Wrap — 외부 padding
            var wrap = BuildSection(parent, "CurrentWrap", topY, CurrentH,
                new Color(0, 0, 0, 0)); // transparent — 이 섹션은 배경 안 그림 (Bg 가 보임)

            // Card — 안쪽 inset
            var card = PaletteUI.BuildSurface(wrap.transform, "Card", p.Surface, CurrentCardRadius);
            var cRt = (RectTransform)card.transform;
            cRt.anchorMin = new Vector2(0f, 0f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.offsetMin = new Vector2(CurrentOuterHPad, CurrentOuterVBot);
            cRt.offsetMax = new Vector2(-CurrentOuterHPad, -CurrentOuterVTop);
            // 카드 클릭 = 시작
            var cardBtn = card.AddComponent<Button>();
            cardBtn.targetGraphic = card.GetComponent<Image>();
            cardBtn.onClick.AddListener(() => OnLevelClicked(index));

            // Thumbnail — top-left
            var thumb = PaletteUI.BuildSurface(card.transform, "Thumb", p.SurfaceAlt, CurrentThumbRadius);
            var tRt = (RectTransform)thumb.transform;
            tRt.anchorMin = new Vector2(0f, 0.5f);
            tRt.anchorMax = new Vector2(0f, 0.5f);
            tRt.pivot     = new Vector2(0f, 0.5f);
            tRt.sizeDelta = new Vector2(CurrentThumbSize, CurrentThumbSize);
            tRt.anchoredPosition = new Vector2(CurrentCardPad, 0f);
            thumb.AddComponent<RectMask2D>();

            var slot = new PaletteUI.TileSlot
            {
                Level  = index + 1,
                Sprite = level.characterSprite,
                GradA  = p.StarterA,
                GradB  = p.StarterB,
                Label  = "",
            };
            PaletteUI.BuildPuzzleImagePlaceholder(thumb.transform, "Img", slot);

            // CTA — 우측 중앙. width 는 텍스트 + chevron + padding 으로 측정
            string ctaLabel = allCleared ? "다시 풀기" : "계속하기";
            string ctaKey   = allCleared ? "level_replay_btn" : "level_continue_btn";
            var cta = BuildContinueButton(card.transform, ctaLabel, ctaKey);
            var ctaRt = (RectTransform)cta.transform;
            float ctaW = ctaRt.sizeDelta.x;
            ctaRt.anchorMin = new Vector2(1f, 0.5f);
            ctaRt.anchorMax = new Vector2(1f, 0.5f);
            ctaRt.pivot     = new Vector2(1f, 0.5f);
            ctaRt.anchoredPosition = new Vector2(-CurrentCardPad, 0f);
            cta.GetComponent<Button>().onClick.AddListener(() => OnLevelClicked(index));

            // 컨텐츠 영역 — 썸네일 우측, CTA 좌측 사이. 명시적 sizeDelta + anchor.
            float cardWidth = RefW - CurrentOuterHPad * 2f;          // 984
            float contentX  = CurrentCardPad + CurrentThumbSize + CurrentCardPad; // 264
            float contentW  = cardWidth - contentX - (CurrentCardPad + ctaW + CurrentCardPad);

            // contentColumn — anchor middle-left, pivot left-middle, height = badge+title+meta+spacing
            const float colSpacing = 6f;
            float badgeH = PaletteTokens.FontLabel + 18;     // 45
            float titleH = PaletteTokens.FontSubhead + 18;   // 63
            float metaH  = PaletteTokens.FontCaptionSm + 12; // 45
            float colH   = badgeH + colSpacing + titleH + colSpacing + metaH;

            var col = new GameObject("Content", typeof(RectTransform));
            col.transform.SetParent(card.transform, worldPositionStays: false);
            var colRt = (RectTransform)col.transform;
            colRt.anchorMin = new Vector2(0f, 0.5f);
            colRt.anchorMax = new Vector2(0f, 0.5f);
            colRt.pivot     = new Vector2(0f, 0.5f);
            colRt.sizeDelta = new Vector2(contentW, colH);
            colRt.anchoredPosition = new Vector2(contentX, 0f);

            // Badge "지금 도전" — col 안 top
            string badgeText = allCleared ? "모두 완성!" : "지금 도전";
            string badgeKey  = allCleared ? "level_badge_all_done" : "level_badge_now";
            var badge = BuildPrimaryBadge(col.transform, badgeText, badgeKey);
            var bRt = (RectTransform)badge.transform;
            bRt.anchorMin = new Vector2(0f, 1f);
            bRt.anchorMax = new Vector2(0f, 1f);
            bRt.pivot     = new Vector2(0f, 1f);
            bRt.anchoredPosition = new Vector2(0f, 0f);

            // Title "Level NN" — badge 아래. 작품명 대신 레벨 번호만 (사용자 결정 2026-05-03)
            string levelNum = (index + 1).ToString("D2");
            string title = allCleared
                ? "다시 한 번 즐겨보세요"
                : $"Level {levelNum}";
            Text titleTxt;
            if (allCleared)
            {
                titleTxt = DesignTokens.BuildLocalizedText(col.transform, "Title",
                    "level_card_replay_title", title,
                    PaletteTokens.FontSubhead, FontStyle.Bold, p.Text, TextAnchor.MiddleLeft);
            }
            else
            {
                titleTxt = DesignTokens.BuildLocalizedTextFormat(col.transform, "Title",
                    "level_card_title_levelnum", title,
                    new object[] { levelNum },
                    PaletteTokens.FontSubhead, FontStyle.Bold, p.Text, TextAnchor.MiddleLeft);
            }
            // Overflow 로 박스 사이즈 빡빡해도 텍스트 자체는 그려짐. 길이 제한은 후속 ellipsis 헬퍼로.
            titleTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            titleTxt.verticalOverflow   = VerticalWrapMode.Overflow;
            var ttRt = (RectTransform)titleTxt.transform;
            ttRt.anchorMin = new Vector2(0f, 1f);
            ttRt.anchorMax = new Vector2(1f, 1f);
            ttRt.pivot     = new Vector2(0f, 1f);
            ttRt.sizeDelta = new Vector2(0f, titleH);
            ttRt.anchoredPosition = new Vector2(0f, -(badgeH + colSpacing));

            // Meta "P조각 · 약 X분"
            int pieces = level.pieceCount;
            int minutes = EstimatedMinutes(level.pieceCount);
            string meta = $"{pieces}조각 · 약 {minutes}분";
            var metaTxt = DesignTokens.BuildLocalizedTextFormat(col.transform, "Meta",
                "level_card_meta", meta, new object[] { pieces, minutes },
                PaletteTokens.FontCaptionSm, FontStyle.Normal, p.TextSoft, TextAnchor.MiddleLeft);
            metaTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            metaTxt.verticalOverflow   = VerticalWrapMode.Overflow;
            var mtRt = (RectTransform)metaTxt.transform;
            mtRt.anchorMin = new Vector2(0f, 1f);
            mtRt.anchorMax = new Vector2(1f, 1f);
            mtRt.pivot     = new Vector2(0f, 1f);
            mtRt.sizeDelta = new Vector2(0f, metaH);
            mtRt.anchoredPosition = new Vector2(0f, -(badgeH + colSpacing + titleH + colSpacing));
        }

        private GameObject BuildPrimaryBadge(Transform parent, string text, string entryKey = null)
        {
            var p = PaletteTokens.Active;
            float h = PaletteTokens.FontLabel + 18;       // 9 × 3 + 6 (vertical padding)
            int radius = Mathf.Min(12, (int)(h / 2f));    // 작은 사각형 코너 (시안 4 × 3 = 12)

            var badge = PaletteUI.BuildSurface(parent, "Badge", p.Primary, radius);
            var rt = (RectTransform)badge.transform;

            // 텍스트 먼저 만들고 preferredWidth 측정 → badge sizeDelta 설정
            var lbl = string.IsNullOrEmpty(entryKey)
                ? DesignTokens.BuildText(badge.transform, "Lbl", text,
                    PaletteTokens.FontLabel, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter)
                : DesignTokens.BuildLocalizedText(badge.transform, "Lbl", entryKey, text,
                    PaletteTokens.FontLabel, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            float textW = Mathf.Max(60f, lbl.preferredWidth);
            float w     = textW + 42f; // 7 × 3 좌우 패딩
            rt.sizeDelta = new Vector2(w, h);

            // 텍스트 stretch — badge 안에서 정확히 채움
            var lRt = (RectTransform)lbl.transform;
            lRt.anchorMin = Vector2.zero; lRt.anchorMax = Vector2.one;
            lRt.offsetMin = Vector2.zero; lRt.offsetMax = Vector2.zero;

            return badge;
        }

        private Button BuildContinueButton(Transform parent, string label, string entryKey = null)
        {
            var p = PaletteTokens.Active;
            int radius = (int)(CurrentCtaHeight / 2f); // pill — height/2

            var cta = PaletteUI.BuildSurface(parent, "CTA", p.Text, radius);
            var btn = cta.AddComponent<Button>();
            btn.targetGraphic = cta.GetComponent<Image>();

            // 텍스트 — 임시 만들어서 preferredWidth 측정
            var lbl = string.IsNullOrEmpty(entryKey)
                ? DesignTokens.BuildText(cta.transform, "Lbl", label,
                    PaletteTokens.FontBody, FontStyle.Bold, p.Surface, TextAnchor.MiddleCenter)
                : DesignTokens.BuildLocalizedText(cta.transform, "Lbl", entryKey, label,
                    PaletteTokens.FontBody, FontStyle.Bold, p.Surface, TextAnchor.MiddleCenter);
            float textW = lbl.preferredWidth;

            // CTA width = padding-left 36 + text + gap 12 + chevron 36 + padding-right 30
            const float padL = 36f, padR = 30f, gap = 12f, chevSize = 36f;
            float ctaW = padL + textW + gap + chevSize + padR;
            ((RectTransform)cta.transform).sizeDelta = new Vector2(ctaW, CurrentCtaHeight);

            // 텍스트 anchor 좌측, 우측엔 chevron 자리
            var lRt = (RectTransform)lbl.transform;
            lRt.anchorMin = new Vector2(0f, 0f);
            lRt.anchorMax = new Vector2(1f, 1f);
            lRt.offsetMin = new Vector2(padL, 0f);
            lRt.offsetMax = new Vector2(-(gap + chevSize + padR), 0f);

            // chevron — 우측 중앙
            var icon = DesignTokens.BuildIcon(cta.transform, "Chev", "chevron_right",
                (int)chevSize, p.Surface);
            var iRt = (RectTransform)icon.transform;
            iRt.anchorMin = new Vector2(1f, 0.5f);
            iRt.anchorMax = new Vector2(1f, 0.5f);
            iRt.pivot     = new Vector2(1f, 0.5f);
            iRt.sizeDelta = new Vector2(chevSize, chevSize);
            iRt.anchoredPosition = new Vector2(-padR, 0f);

            return btn;
        }

        private static int EstimatedMinutes(int pieceCount)
            => Mathf.Max(3, pieceCount / 8);

        // characterNameKey 가 진짜 작품명 (사용자 입력) — 다국어 변환 X. 비어있을 때만 fallback "제목".
        // fallback 자체는 짧은 문자열이라 다국어 적용 시점은 호출처(level_label_default).
        private static string GetLevelLabel(LevelDataSO level)
            => string.IsNullOrEmpty(level.characterNameKey) ? "제목" : level.characterNameKey;

        // ─────────────────────────────────────────────────────
        // Section Label — "전체 작품 ─── 순서대로 열려요"
        // ─────────────────────────────────────────────────────

        private void BuildSectionLabelWrap(Transform parent, float topY)
        {
            var p = PaletteTokens.Active;
            var wrap = BuildSection(parent, "SectionLabelWrap", topY, SectionLabelH,
                new Color(0, 0, 0, 0));

            float labelH = SectionLabelH - SectionLabelVTop - SectionLabelVBot;
            float labelW = RefW - SectionLabelHPad * 2f;
            var label = PaletteUI.BuildSectionLabel(wrap.transform, "Label",
                "전체 작품", "순서대로 열려요",
                new Vector2(labelW, labelH),
                leftEntryKey: "level_section_all", rightEntryKey: "level_section_subtitle");
            var lRt = (RectTransform)label.transform;
            lRt.anchorMin = new Vector2(0f, 1f);
            lRt.anchorMax = new Vector2(0f, 1f);
            lRt.pivot     = new Vector2(0f, 1f);
            lRt.anchoredPosition = new Vector2(SectionLabelHPad, -SectionLabelVTop);
        }

        // ─────────────────────────────────────────────────────
        // ScrollGrid — 버킷(E/M/H/VH)별 헤더 + 3-col SequentialTile 그리드
        // ─────────────────────────────────────────────────────

        private void BuildScrollGrid(Transform parent, float topY, float bottomInset,
                                     LevelDataSO[] levels, int currentIdx)
        {
            // ScrollRect — anchor stretch full + offset inset. 디바이스 height 무관 자동 fit.
            var scrollGo = new GameObject("Scroll",
                typeof(RectTransform), typeof(ScrollRect));
            scrollGo.transform.SetParent(parent, worldPositionStays: false);
            var srt = (RectTransform)scrollGo.transform;
            srt.anchorMin = new Vector2(0f, 0f);
            srt.anchorMax = new Vector2(1f, 1f);
            srt.pivot     = new Vector2(0.5f, 0.5f);
            srt.offsetMin = new Vector2(0f, bottomInset);   // bottom inset (Banner+Nav 만큼 위)
            srt.offsetMax = new Vector2(0f, -topY);         // top inset (sticky 섹션 만큼 아래)

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical   = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f; // default 1 — 마우스 휠/터치 제스처 감도 ↑

            // Viewport — Mask + Image (raycast 받아 ScrollRect drag 트리거)
            var vpGo = new GameObject("Viewport",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            vpGo.transform.SetParent(scrollGo.transform, worldPositionStays: false);
            var vpRt = (RectTransform)vpGo.transform;
            vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero; vpRt.offsetMax = Vector2.zero;
            var vpImg = vpGo.GetComponent<Image>();
            vpImg.color = new Color(1, 1, 1, 0.01f);
            vpImg.raycastTarget = true; // 명시 — drag 이벤트 받음
            vpGo.GetComponent<Mask>().showMaskGraphic = false;

            // Content — 버킷 헤더 삽입을 위해 수동 배치 (GridLayoutGroup 은 버킷별 하위 그리드에만 사용).
            // 높이는 버킷 순회하며 직접 누적 계산 (ContentSizeFitter 불필요 — 산수로 검산 가능).
            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(vpGo.transform, worldPositionStays: false);
            var crt = (RectTransform)contentGo.transform;
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot     = new Vector2(0.5f, 1f);

            float cellW = (RefW - GridHPad * 2f - GridGap * (GridCols - 1)) / GridCols;
            float cellH = cellW + PaletteUI.TileLabelHeight;

            var p = PaletteTokens.Active;
            float cy = 0f; // Content 내부 누적 y (아래로 증가)
            int i = 0;
            while (i < levels.Length)
            {
                var bucket = levels[i].imageBucket;
                int start = i;
                while (i < levels.Length && levels[i].imageBucket == bucket) i++;
                int count = i - start;

                cy += BuildBucketHeader(contentGo.transform, cy, bucket, count);
                cy += BuildBucketGrid(contentGo.transform, cy, levels, start, i, currentIdx,
                    cellW, cellH, p);
                cy += BucketGapAfterGrid;
            }

            crt.sizeDelta = new Vector2(0f, cy);

            scroll.viewport = vpRt;
            scroll.content  = crt;
        }

        /// <summary>버킷(난이도) 헤더 한 줄 빌드. 반환값 = 소비한 높이 (BucketHeaderH).</summary>
        private float BuildBucketHeader(Transform parent, float topY, LevelDataSO.ImageBucket bucket, int count)
        {
            var (entryKey, fallback) = BucketLabelFor(bucket);
            string rangeText = BucketRangeFor(bucket);

            float labelH = BucketHeaderH - BucketHeaderVTop;
            float labelW = RefW - SectionLabelHPad * 2f;
            var label = PaletteUI.BuildSectionLabel(parent, $"Bucket_{bucket}",
                fallback, rangeText,
                new Vector2(labelW, labelH),
                leftEntryKey: entryKey, rightEntryKey: null);
            var lRt = (RectTransform)label.transform;
            lRt.anchorMin = new Vector2(0f, 1f);
            lRt.anchorMax = new Vector2(0f, 1f);
            lRt.pivot     = new Vector2(0f, 1f);
            lRt.anchoredPosition = new Vector2(SectionLabelHPad, -(topY + BucketHeaderVTop));

            return BucketHeaderH;
        }

        /// <summary>버킷 하나의 SequentialTile 그리드. 반환값 = 소비한 높이 (rows × cellH + gaps).</summary>
        private float BuildBucketGrid(Transform parent, float topY, LevelDataSO[] levels,
                                      int start, int end, int currentIdx,
                                      float cellW, float cellH, PaletteTokens.Palette p)
        {
            int count = end - start;
            int rows  = Mathf.CeilToInt(count / (float)GridCols);
            float gridH = rows * cellH + Mathf.Max(0, rows - 1) * GridGap;

            var gridGo = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridGo.transform.SetParent(parent, worldPositionStays: false);
            var grt = (RectTransform)gridGo.transform;
            grt.anchorMin = new Vector2(0f, 1f);
            grt.anchorMax = new Vector2(1f, 1f);
            grt.pivot     = new Vector2(0.5f, 1f);
            grt.sizeDelta = new Vector2(0f, gridH);
            grt.anchoredPosition = new Vector2(0f, -topY);

            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(cellW, cellH);
            grid.spacing  = new Vector2(GridGap, GridGap);
            grid.padding  = new RectOffset((int)GridHPad, (int)GridHPad, 0, 0);
            grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = GridCols;
            grid.childAlignment  = TextAnchor.UpperCenter;

            for (int k = start; k < end; k++)
            {
                var lv = levels[k];
                PaletteUI.TileState state =
                    SaveManager.IsLevelComplete(k) ? PaletteUI.TileState.Cleared :
                    k == currentIdx                ? PaletteUI.TileState.Current :
                                                     PaletteUI.TileState.Locked;
                int stars = state == PaletteUI.TileState.Cleared
                          ? SaveManager.GetLevelStars(k) : 0;

                var slot = new PaletteUI.TileSlot
                {
                    Level  = k + 1,
                    Stars  = stars,
                    Pieces = lv.pieceCount,
                    Label  = GetLevelLabel(lv),
                    Sprite = lv.characterSprite, // locked 도 이미지 표시 + dim 처리
                    GradA  = p.StarterA,
                    GradB  = p.StarterB,
                    State  = state,
                };

                int captured = k;
                Action onClick = state == PaletteUI.TileState.Locked
                    ? () => OnLockedTileClicked()
                    : (Action)(() => OnLevelClicked(captured));

                PaletteUI.BuildSequentialTile(gridGo.transform, slot, onClick);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(grt);
            return gridH;
        }

        // 버킷별 라벨 (좌측 — 다국어 키) / 범위 (우측 — 숫자라 다국어 미적용, level_bucket_* 키 참고)
        private static (string entryKey, string fallback) BucketLabelFor(LevelDataSO.ImageBucket bucket) => bucket switch
        {
            LevelDataSO.ImageBucket.VeryEasy => ("level_bucket_veryeasy", "아주 쉬움"),
            LevelDataSO.ImageBucket.Easy     => ("level_bucket_easy", "쉬움"),
            LevelDataSO.ImageBucket.Medium   => ("level_bucket_medium", "보통"),
            LevelDataSO.ImageBucket.Hard     => ("level_bucket_hard", "어려움"),
            LevelDataSO.ImageBucket.VeryHard => ("level_bucket_veryhard", "매우 어려움"),
            _                                 => ("level_bucket_easy", "쉬움"),
        };

        // 16레벨 고정 매핑 (E3/M6/H4/VH3 — 결정된 디자인, 변경 금지). 범위 표기는 순수 숫자라 로케일 무관.
        private static string BucketRangeFor(LevelDataSO.ImageBucket bucket) => bucket switch
        {
            LevelDataSO.ImageBucket.Easy     => "1~3",
            LevelDataSO.ImageBucket.Medium   => "4~9",
            LevelDataSO.ImageBucket.Hard     => "10~13",
            LevelDataSO.ImageBucket.VeryHard => "14~16",
            _                                 => "",
        };

        // ─────────────────────────────────────────────────────
        // BannerAd — anchor bottom. Banner 는 Nav + bottomInset 위에 위치.
        // ─────────────────────────────────────────────────────

        // ─────────────────────────────────────────────────────
        // BottomNav — NavSpacer (home indicator surface) + Nav 본체 분리 패턴
        // ─────────────────────────────────────────────────────

        // 2탭 — 결제 안 함 (shop 제거), daily 도 이번 v0.6 범위 밖
        private static readonly PaletteUI.NavItem[] NavItems2 = new[]
        {
            new PaletteUI.NavItem { Key = "home",       Label = "메인", IconKey = "home",  EntryKey = "nav_home" },
            new PaletteUI.NavItem { Key = "collection", Label = "갤러리", IconKey = "image", EntryKey = "nav_collection" },
        };

        private void BuildNav(Transform parent)
        {
            var p = PaletteTokens.Active;
            float bottomInset = ComputeBottomInsetRef();

            // SafeArea bottom (home indicator) 영역 — surface 색 spacer 만 stretch (자식 X)
            if (bottomInset > 1f)
            {
                var spacer = new GameObject("NavSpacer",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                spacer.transform.SetParent(parent, worldPositionStays: false);
                var spRt = (RectTransform)spacer.transform;
                spRt.anchorMin = new Vector2(0f, 0f);
                spRt.anchorMax = new Vector2(1f, 0f);
                spRt.pivot     = new Vector2(0.5f, 0f);
                spRt.sizeDelta = new Vector2(0f, bottomInset);
                spRt.anchoredPosition = Vector2.zero;
                spacer.GetComponent<Image>().color = p.Surface;
                spacer.GetComponent<Image>().raycastTarget = false;
            }

            // Nav 본체 — NavH 만, spacer 위 (= SafeArea 안쪽 가장 하단)
            var nav = PaletteUI.BuildBottomNav(parent, "BottomNav", "home", OnNavTabClicked, NavItems2);
            var rt = (RectTransform)nav.transform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot     = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, NavH);
            rt.anchoredPosition = new Vector2(0f, bottomInset);
        }

        // ─────────────────────────────────────────────────────
        // 액션
        // ─────────────────────────────────────────────────────

        private void OnLevelClicked(int levelIndex)
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.currentLevelIndex = levelIndex;
            SceneLoader.LoadScene(SceneLoader.Gameplay);
        }

        private void OnLockedTileClicked()
            => ShowToast("이전 작품을 먼저 완성해 주세요", "level_locked_v6");

        private void OnNavTabClicked(string key)
        {
            if (key == "home") return;
            if (key == "collection")
            {
                SceneLoader.LoadScene(SceneLoader.Gallery);
                return;
            }
            ShowToast("준비 중", "common_coming_soon");
        }

        private void OnSettings()
        {
            var popup = FindAnyObjectByType<SettingsPopupUI>(FindObjectsInactive.Include);
            if (popup != null) popup.Show();
            else Debug.LogWarning("[LevelSelect] SettingsPopup 미배치");
        }

        private void ShowToast(string fallback, string entryKey = null)
        {
            if (_toastGO != null) Destroy(_toastGO);
            if (_canvas == null) return;
            _toastGO = PaletteUI.BuildToast(_canvas.transform, fallback, 1.5f, entryKey);
        }
    }
}
