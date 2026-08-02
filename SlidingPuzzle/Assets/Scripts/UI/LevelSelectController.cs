using System;
using UnityEngine;
using UnityEngine.UI;
using SlidingPuzzle.Core;
using SlidingPuzzle.Data;
using SlidingPuzzle.Systems;

namespace SlidingPuzzle.UI
{
    /// <summary>
    /// LevelSelect 씬 컨트롤러 — MyGames(Puzzle Pals) LevelSelectController(HomeSequential) 를 그대로 이식.
    /// 16레벨 규모가 직소와 동일해서 "레벨 3개라 축소" 버전이 아니라 원본 그대로 사용.
    ///
    /// 구조: Header(인사말+타이틀+설정) → Progress → 지금 도전 카드 → "전체 작품" 섹션 라벨
    ///       → 난이도별(쉬움/보통/어려움) 헤더 + 3-col 그리드 → BottomNav.
    ///
    /// 직소의 imageBucket(5단계: 아주쉬움~매우어려움) 대신, 이 프로젝트의 Difficulty(3단계)로 그룹핑.
    /// 범위 1~5/6~11/12~16 은 SlidingPuzzleImporter.DifficultyForLevel 매핑과 정확히 일치해야 함.
    /// </summary>
    [DisallowMultipleComponent]
    public class LevelSelectController : MonoBehaviour
    {
        private const string LevelsResourcePath = "Levels";

        private const float RefW = 1080f;
        private const float RefH = 1920f;

        private const float HeaderH       = 210f;
        private const float ProgressH     = 180f;
        private const float CurrentH      = 330f;
        private const float SectionLabelH = 96f;
        private const float NavH          = PaletteUI.NavHeight;

        private const float HeaderHPad    = 60f;
        private const float HeaderVPadTop = 42f;

        private const float ProgressHPad = 60f;
        private const float ProgressVPad = 42f;

        private const float CurrentOuterHPad = 48f;
        private const float CurrentOuterVTop = 42f;
        private const float CurrentOuterVBot = 24f;
        private const float CurrentCardPad   = 36f;
        private const float CurrentThumbSize = 192f;
        private const int   CurrentThumbRadius = 30;
        private const int   CurrentCardRadius  = 48;
        private const float CurrentCtaHeight   = 96f;

        private const float SectionLabelHPad = 48f;
        private const float SectionLabelVTop = 24f;
        private const float SectionLabelVBot = 36f;

        private const float GridHPad = 48f;
        private const float GridGap  = 30f;
        private const int   GridCols = 3;

        private const float BucketHeaderH      = 72f;
        private const float BucketHeaderVTop   = 24f;
        private const float BucketGapAfterGrid = 36f;

        // 16레벨 기준 마일스톤
        private static readonly int[] Milestones16 = { 5, 10, 15 };

        private GameObject _toastGO;
        private Canvas     _canvas;
        private float      _topInsetRef;

        private void Start() => Build();

        public void Build()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
                else                       DestroyImmediate(transform.GetChild(i).gameObject);
            }

            _canvas = BuildCanvas();
            BuildFullStretch(_canvas.transform, "Background", PaletteTokens.Active.Bg);

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

            _topInsetRef = ComputeTopInsetRef();

            float topY = 0f;
            BuildHeader(_canvas.transform, topY);                        topY += HeaderH + _topInsetRef;
            BuildProgress(_canvas.transform, topY, cleared, total);       topY += ProgressH;
            BuildCurrentChallenge(_canvas.transform, topY,
                levels[Mathf.Clamp(currentIdx, 0, total - 1)],
                Mathf.Clamp(currentIdx, 0, total - 1), allCleared);       topY += CurrentH;
            BuildSectionLabelWrap(_canvas.transform, topY);               topY += SectionLabelH;

            float bottomInsetRef = ComputeBottomInsetRef();
            float scrollBottom = NavH + bottomInsetRef;
            BuildScrollGrid(_canvas.transform, topY, scrollBottom, levels, currentIdx);

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
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(RefW, RefH);
            scaler.matchWidthOrHeight  = 0f;
            return canvas;
        }

        private GameObject BuildFullStretch(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, worldPositionStays: false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private GameObject BuildSection(Transform parent, string name, float topY, float height, Color bg)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
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
            var border = new GameObject("BottomBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            border.transform.SetParent(parent, worldPositionStays: false);
            var rt = (RectTransform)border.transform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot     = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, 3f);
            border.GetComponent<Image>().color = color;
            border.GetComponent<Image>().raycastTarget = false;
        }

        // ─────────────────────────────────────────────────────
        // Header — greeting + title + Settings
        // ─────────────────────────────────────────────────────

        private float ComputeTopInsetRef()
        {
            if (Screen.height <= 0) return 0f;
            float topInsetDevice = Screen.height - (Screen.safeArea.y + Screen.safeArea.height);
            float minInset = Mathf.Max(75f, Screen.height * 0.04f);
            float maxInset = Screen.height * 0.10f;
            if (topInsetDevice < minInset || topInsetDevice > maxInset)
                topInsetDevice = minInset;

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
            float bottomInsetDevice = Screen.safeArea.y;
            if (bottomInsetDevice < 0f) bottomInsetDevice = 0f;
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
            var header = BuildSection(parent, "Header", topY, HeaderH + _topInsetRef, p.Surface);
            BuildBottomBorder(header.transform, p.Line);

            var inner = new GameObject("Inner", typeof(RectTransform));
            inner.transform.SetParent(header.transform, worldPositionStays: false);
            var innerRt = (RectTransform)inner.transform;
            innerRt.anchorMin = new Vector2(0f, 0f);
            innerRt.anchorMax = new Vector2(1f, 1f);
            innerRt.offsetMin = Vector2.zero;
            innerRt.offsetMax = new Vector2(0f, -_topInsetRef);
            BuildHeaderInner(inner.transform);
        }

        private void BuildHeaderInner(Transform inner)
        {
            var p = PaletteTokens.Active;

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

            float titleY = HeaderVPadTop + (PaletteTokens.FontCaptionSm + 9) + 6;
            var title = DesignTokens.BuildLocalizedText(inner, "Title",
                "home_title", "퍼즐 한 판, 시작해 볼까요",
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

            const float iconBtnSize = 108f;
            var settingsBtn = PaletteUI.BuildIconBtn(inner, "Settings", "settings", iconBtnSize, highlight: false);
            var sRt = (RectTransform)settingsBtn.transform;
            sRt.anchorMin = new Vector2(1f, 0.5f);
            sRt.anchorMax = new Vector2(1f, 0.5f);
            sRt.pivot     = new Vector2(1f, 0.5f);
            sRt.sizeDelta = new Vector2(iconBtnSize, iconBtnSize);
            sRt.anchoredPosition = new Vector2(-HeaderHPad, 0f);
            settingsBtn.onClick.AddListener(OnSettings);
        }

        // ─────────────────────────────────────────────────────
        // Progress
        // ─────────────────────────────────────────────────────

        private void BuildProgress(Transform parent, float topY, int cleared, int total)
        {
            var p = PaletteTokens.Active;
            var progress = BuildSection(parent, "Progress", topY, ProgressH, p.Surface);
            BuildBottomBorder(progress.transform, p.Line);

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

            float barWidth = RefW - ProgressHPad * 2f;
            float ratio    = total > 0 ? (float)cleared / total : 0f;
            float barY     = ProgressVPad + (PaletteTokens.FontBody + 18) + 24f;
            var bar = PaletteUI.BuildProgressBar(progress.transform, "Bar", ratio, total, Milestones16, barWidth);
            var bRt = (RectTransform)bar.transform;
            bRt.anchorMin = new Vector2(0f, 1f);
            bRt.anchorMax = new Vector2(0f, 1f);
            bRt.pivot     = new Vector2(0f, 1f);
            bRt.anchoredPosition = new Vector2(ProgressHPad, -barY);
        }

        private static int NextMilestone(int cleared, int total)
        {
            foreach (var m in Milestones16) if (m > cleared && m < total) return m;
            return -1;
        }

        // ─────────────────────────────────────────────────────
        // Current Challenge
        // ─────────────────────────────────────────────────────

        private void BuildCurrentChallenge(Transform parent, float topY, LevelDataSO level, int index, bool allCleared)
        {
            var p = PaletteTokens.Active;

            var wrap = BuildSection(parent, "CurrentWrap", topY, CurrentH, new Color(0, 0, 0, 0));

            var card = PaletteUI.BuildSurface(wrap.transform, "Card", p.Surface, CurrentCardRadius);
            var cRt = (RectTransform)card.transform;
            cRt.anchorMin = new Vector2(0f, 0f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.offsetMin = new Vector2(CurrentOuterHPad, CurrentOuterVBot);
            cRt.offsetMax = new Vector2(-CurrentOuterHPad, -CurrentOuterVTop);
            var cardBtn = card.AddComponent<Button>();
            cardBtn.targetGraphic = card.GetComponent<Image>();
            cardBtn.onClick.AddListener(() => OnLevelClicked(index));

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
                Sprite = level.sourceSprite,
                GradA  = p.StarterA,
                GradB  = p.StarterB,
                Label  = "",
            };
            PaletteUI.BuildPuzzleImagePlaceholder(thumb.transform, "Img", slot);

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

            float cardWidth = RefW - CurrentOuterHPad * 2f;
            float contentX  = CurrentCardPad + CurrentThumbSize + CurrentCardPad;
            float contentW  = cardWidth - contentX - (CurrentCardPad + ctaW + CurrentCardPad);

            const float colSpacing = 6f;
            float badgeH = PaletteTokens.FontLabel + 18;
            float titleH = PaletteTokens.FontSubhead + 18;
            float metaH  = PaletteTokens.FontCaptionSm + 12;
            float colH   = badgeH + colSpacing + titleH + colSpacing + metaH;

            var col = new GameObject("Content", typeof(RectTransform));
            col.transform.SetParent(card.transform, worldPositionStays: false);
            var colRt = (RectTransform)col.transform;
            colRt.anchorMin = new Vector2(0f, 0.5f);
            colRt.anchorMax = new Vector2(0f, 0.5f);
            colRt.pivot     = new Vector2(0f, 0.5f);
            colRt.sizeDelta = new Vector2(contentW, colH);
            colRt.anchoredPosition = new Vector2(contentX, 0f);

            string badgeText = allCleared ? "모두 완성!" : "지금 도전";
            string badgeKey  = allCleared ? "level_badge_all_done" : "level_badge_now";
            var badge = BuildPrimaryBadge(col.transform, badgeText, badgeKey);
            var bRt = (RectTransform)badge.transform;
            bRt.anchorMin = new Vector2(0f, 1f);
            bRt.anchorMax = new Vector2(0f, 1f);
            bRt.pivot     = new Vector2(0f, 1f);
            bRt.anchoredPosition = new Vector2(0f, 0f);

            string levelNum = (index + 1).ToString("D2");
            string title = allCleared ? "다시 한 번 즐겨보세요" : $"Level {levelNum}";
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
                    "level_card_title_levelnum", title, new object[] { levelNum },
                    PaletteTokens.FontSubhead, FontStyle.Bold, p.Text, TextAnchor.MiddleLeft);
            }
            titleTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            titleTxt.verticalOverflow   = VerticalWrapMode.Overflow;
            var ttRt = (RectTransform)titleTxt.transform;
            ttRt.anchorMin = new Vector2(0f, 1f);
            ttRt.anchorMax = new Vector2(1f, 1f);
            ttRt.pivot     = new Vector2(0f, 1f);
            ttRt.sizeDelta = new Vector2(0f, titleH);
            ttRt.anchoredPosition = new Vector2(0f, -(badgeH + colSpacing));

            int pieces = level.gridSize * level.gridSize - 1;
            int minutes = EstimatedMinutes(pieces);
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
            float h = PaletteTokens.FontLabel + 18;
            int radius = Mathf.Min(12, (int)(h / 2f));

            var badge = PaletteUI.BuildSurface(parent, "Badge", p.Primary, radius);
            var rt = (RectTransform)badge.transform;

            var lbl = string.IsNullOrEmpty(entryKey)
                ? DesignTokens.BuildText(badge.transform, "Lbl", text,
                    PaletteTokens.FontLabel, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter)
                : DesignTokens.BuildLocalizedText(badge.transform, "Lbl", entryKey, text,
                    PaletteTokens.FontLabel, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            float textW = Mathf.Max(60f, lbl.preferredWidth);
            float w     = textW + 42f;
            rt.sizeDelta = new Vector2(w, h);

            var lRt = (RectTransform)lbl.transform;
            lRt.anchorMin = Vector2.zero; lRt.anchorMax = Vector2.one;
            lRt.offsetMin = Vector2.zero; lRt.offsetMax = Vector2.zero;

            return badge;
        }

        private Button BuildContinueButton(Transform parent, string label, string entryKey = null)
        {
            var p = PaletteTokens.Active;
            int radius = (int)(CurrentCtaHeight / 2f);

            var cta = PaletteUI.BuildSurface(parent, "CTA", p.Text, radius);
            var btn = cta.AddComponent<Button>();
            btn.targetGraphic = cta.GetComponent<Image>();

            var lbl = string.IsNullOrEmpty(entryKey)
                ? DesignTokens.BuildText(cta.transform, "Lbl", label,
                    PaletteTokens.FontBody, FontStyle.Bold, p.Surface, TextAnchor.MiddleCenter)
                : DesignTokens.BuildLocalizedText(cta.transform, "Lbl", entryKey, label,
                    PaletteTokens.FontBody, FontStyle.Bold, p.Surface, TextAnchor.MiddleCenter);
            float textW = lbl.preferredWidth;

            const float padL = 36f, padR = 30f, gap = 12f, chevSize = 36f;
            float ctaW = padL + textW + gap + chevSize + padR;
            ((RectTransform)cta.transform).sizeDelta = new Vector2(ctaW, CurrentCtaHeight);

            var lRt = (RectTransform)lbl.transform;
            lRt.anchorMin = new Vector2(0f, 0f);
            lRt.anchorMax = new Vector2(1f, 1f);
            lRt.offsetMin = new Vector2(padL, 0f);
            lRt.offsetMax = new Vector2(-(gap + chevSize + padR), 0f);

            var icon = DesignTokens.BuildIcon(cta.transform, "Chev", "chevron_right", (int)chevSize, p.Surface);
            var iRt = (RectTransform)icon.transform;
            iRt.anchorMin = new Vector2(1f, 0.5f);
            iRt.anchorMax = new Vector2(1f, 0.5f);
            iRt.pivot     = new Vector2(1f, 0.5f);
            iRt.sizeDelta = new Vector2(chevSize, chevSize);
            iRt.anchoredPosition = new Vector2(-padR, 0f);

            return btn;
        }

        private static int EstimatedMinutes(int pieceCount) => Mathf.Max(3, pieceCount / 8);

        // ─────────────────────────────────────────────────────
        // Section Label
        // ─────────────────────────────────────────────────────

        private void BuildSectionLabelWrap(Transform parent, float topY)
        {
            var wrap = BuildSection(parent, "SectionLabelWrap", topY, SectionLabelH, new Color(0, 0, 0, 0));

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
        // ScrollGrid — 난이도(쉬움/보통/어려움)별 헤더 + 3-col SequentialTile 그리드
        // ─────────────────────────────────────────────────────

        private void BuildScrollGrid(Transform parent, float topY, float bottomInset,
                                     LevelDataSO[] levels, int currentIdx)
        {
            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            scrollGo.transform.SetParent(parent, worldPositionStays: false);
            var srt = (RectTransform)scrollGo.transform;
            srt.anchorMin = new Vector2(0f, 0f);
            srt.anchorMax = new Vector2(1f, 1f);
            srt.pivot     = new Vector2(0.5f, 0.5f);
            srt.offsetMin = new Vector2(0f, bottomInset);
            srt.offsetMax = new Vector2(0f, -topY);

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical   = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f;

            var vpGo = new GameObject("Viewport",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            vpGo.transform.SetParent(scrollGo.transform, worldPositionStays: false);
            var vpRt = (RectTransform)vpGo.transform;
            vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero; vpRt.offsetMax = Vector2.zero;
            var vpImg = vpGo.GetComponent<Image>();
            vpImg.color = new Color(1, 1, 1, 0.01f);
            vpImg.raycastTarget = true;
            vpGo.GetComponent<Mask>().showMaskGraphic = false;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(vpGo.transform, worldPositionStays: false);
            var crt = (RectTransform)contentGo.transform;
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot     = new Vector2(0.5f, 1f);

            float cellW = (RefW - GridHPad * 2f - GridGap * (GridCols - 1)) / GridCols;
            float cellH = cellW + PaletteUI.TileLabelHeight;

            var p = PaletteTokens.Active;
            float cy = 0f;
            int i = 0;
            while (i < levels.Length)
            {
                var difficulty = levels[i].difficulty;
                int start = i;
                while (i < levels.Length && levels[i].difficulty == difficulty) i++;

                cy += BuildBucketHeader(contentGo.transform, cy, difficulty);
                cy += BuildBucketGrid(contentGo.transform, cy, levels, start, i, currentIdx, cellW, cellH, p);
                cy += BucketGapAfterGrid;
            }

            crt.sizeDelta = new Vector2(0f, cy);

            scroll.viewport = vpRt;
            scroll.content  = crt;
        }

        private float BuildBucketHeader(Transform parent, float topY, LevelDataSO.Difficulty difficulty)
        {
            string entryKey = LevelDataSO.DifficultyEntryKey(difficulty);
            string fallback = LevelDataSO.DifficultyFallback(difficulty);
            string rangeText = BucketRangeFor(difficulty);

            float labelH = BucketHeaderH - BucketHeaderVTop;
            float labelW = RefW - SectionLabelHPad * 2f;
            var label = PaletteUI.BuildSectionLabel(parent, $"Bucket_{difficulty}",
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
                int stars = state == PaletteUI.TileState.Cleared ? SaveManager.GetLevelStars(k) : 0;

                var slot = new PaletteUI.TileSlot
                {
                    Level  = k + 1,
                    Stars  = stars,
                    Pieces = lv.gridSize * lv.gridSize - 1,
                    Label  = LevelDataSO.GetDefaultArtworkName(lv.levelNumber),
                    Sprite = lv.sourceSprite,
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

        // 16레벨 고정 매핑 (SlidingPuzzleImporter.DifficultyForLevel 과 일치해야 함 — 1~5/6~11/12~16).
        private static string BucketRangeFor(LevelDataSO.Difficulty difficulty) => difficulty switch
        {
            LevelDataSO.Difficulty.Easy   => "1~5",
            LevelDataSO.Difficulty.Medium => "6~11",
            LevelDataSO.Difficulty.Hard   => "12~16",
            _ => "",
        };

        // ─────────────────────────────────────────────────────
        // BottomNav
        // ─────────────────────────────────────────────────────

        private static readonly PaletteUI.NavItem[] NavItems2 =
        {
            new PaletteUI.NavItem { Key = "home",       Label = "메인",  IconKey = "home",  EntryKey = "nav_home" },
            new PaletteUI.NavItem { Key = "collection", Label = "갤러리", IconKey = "image", EntryKey = "nav_collection" },
        };

        private void BuildNav(Transform parent)
        {
            var p = PaletteTokens.Active;
            float bottomInset = ComputeBottomInsetRef();

            if (bottomInset > 1f)
            {
                var spacer = new GameObject("NavSpacer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
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

        private void OnLockedTileClicked() => ShowToast("이전 작품을 먼저 완성해 주세요", "level_locked_v6");

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
