using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SlidingPuzzle.Core;
using SlidingPuzzle.Data;
using SlidingPuzzle.Systems;

namespace SlidingPuzzle.UI
{
    /// <summary>
    /// Gallery 씬 컨트롤러 — MyGames(Puzzle Pals) GalleryMuseumController 를 그대로 이식.
    /// 16레벨 규모가 직소와 동일해서 "레벨 3개라 층 구분 생략" 축소판이 아니라 원본 그대로 사용.
    ///
    /// 구조:
    ///   - Header (back + "나의 갤러리" subtitle + "그림 수집관" 타이틀 + "{cleared}/{total}" pill)
    ///   - Progress 카드 (다음 작품까지 + 레벨 N 진행중 + 그라디언트 bar)
    ///   - ScrollRect 안에 Floor 카드 (FIRST FLOOR / SECOND FLOOR, 9개씩)
    ///   - BottomNav (갤러리 활성)
    ///
    /// 액자: 골드 frame + 드롭섀도우 + 우상단 조각수 뱃지 + 작품명 캡션(고정 16개 이름 — 직소와 동일 이미지 재사용).
    /// 잠긴 레벨 미노출. 클릭 시 다시 풀기 → Gameplay 씬.
    /// </summary>
    [DisallowMultipleComponent]
    public class GalleryController : MonoBehaviour
    {
        private const string LevelsResourcePath = "Levels";

        private const float RefW = 1080f;
        private const float RefH = 1920f;

        private const float HeaderH   = 300f;
        private const float ProgressH = 210f;
        private const float NavH      = PaletteUI.NavHeight;

        private const float HeaderHPad    = 60f;
        private const float HeaderVPadTop = 36f;

        private const float ProgressHPad = 48f;
        private const float ProgressVPad = 24f;

        private const float ContentTopPad    = 36f;
        private const float ContentSidePad   = 48f;
        private const float ContentBottomPad = 60f;

        private const float FloorCardPad    = 36f;
        private const float FloorCardRadius = 24;
        private const float FloorLabelH     = 78f;
        private const float FloorGapH       = 48f;
        private const float CaptionH        = 60f;
        private const float CaptionTopGap   = 12f;

        private const float GridGap        = 30f;
        private const int   GridCols       = 3;
        private const int   PiecesPerFloor = 9;

        private float _topInsetRef;
        private GameObject _toastGO;
        private Canvas _canvas;

        private static readonly PaletteUI.NavItem[] NavItems2 =
        {
            new PaletteUI.NavItem { Key = "home",       Label = "메인",  IconKey = "home",  EntryKey = "nav_home" },
            new PaletteUI.NavItem { Key = "collection", Label = "갤러리", IconKey = "image", EntryKey = "nav_collection" },
        };

        private void Start() => Build();

        public void Build()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
                else                       DestroyImmediate(transform.GetChild(i).gameObject);
            }

            _topInsetRef = ComputeTopInsetRef();
            _canvas = BuildCanvas();
            BuildBackground();

            var levels = Resources.LoadAll<LevelDataSO>(LevelsResourcePath);
            if (levels == null || levels.Length == 0)
            {
                Debug.LogWarning($"[Gallery] Resources/{LevelsResourcePath}/ 비어있음");
                return;
            }
            Array.Sort(levels, (a, b) => a.levelNumber.CompareTo(b.levelNumber));

            int total      = levels.Length;
            int cleared    = SaveManager.GetClearedCount(total);
            int currentIdx = SaveManager.GetCurrentLevelIndex(total);

            float topY = 0f;
            BuildHeader(_canvas.transform, topY, cleared, total); topY += HeaderH + _topInsetRef;
            BuildProgressCard(_canvas.transform, topY, cleared, total, currentIdx); topY += ProgressH;
            BuildScrollContent(_canvas.transform, topY, levels, currentIdx);

            BuildNav(_canvas.transform);
        }

        // ─────────────────────────────────────────────────────
        // 기본
        // ─────────────────────────────────────────────────────

        private float ComputeTopInsetRef()
        {
            if (Screen.height <= 0) return 0f;
            float topInsetDevice = Screen.height - (Screen.safeArea.y + Screen.safeArea.height);
            float minInset = Mathf.Max(75f, Screen.height * 0.04f);
            float maxInset = Screen.height * 0.10f;
            if (topInsetDevice < minInset || topInsetDevice > maxInset)
                topInsetDevice = minInset;
            return topInsetDevice * (RefH / Screen.height);
        }

        private float ComputeBottomInsetRef()
        {
            if (Screen.height <= 0) return 0f;
            float bottomInsetDevice = Screen.safeArea.y;
            if (bottomInsetDevice < 0f) bottomInsetDevice = 0f;
            if (bottomInsetDevice > Screen.height * 0.08f) bottomInsetDevice = 0f;
            return bottomInsetDevice * (RefH / Screen.height);
        }

        private Canvas BuildCanvas()
        {
            var go = new GameObject("GalleryCanvas",
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

        private void BuildBackground()
        {
            var p = PaletteTokens.Active;
            var bg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bg.transform.SetParent(_canvas.transform, worldPositionStays: false);
            var rt = (RectTransform)bg.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var bImg = bg.GetComponent<Image>();
            bImg.sprite = PaletteUI.GradientVertical(p.Bg, p.SurfaceAlt);
            bImg.color  = Color.white;
            bImg.raycastTarget = false;
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

        // ─────────────────────────────────────────────────────
        // Header — back + "나의 갤러리" + "그림 수집관" + "{cleared}/{total}" pill
        // ─────────────────────────────────────────────────────

        private void BuildHeader(Transform parent, float topY, int cleared, int total)
        {
            var p = PaletteTokens.Active;
            var header = BuildSection(parent, "Header", topY, HeaderH + _topInsetRef, new Color(0, 0, 0, 0));

            var inner = new GameObject("Inner", typeof(RectTransform));
            inner.transform.SetParent(header.transform, worldPositionStays: false);
            var iRt = (RectTransform)inner.transform;
            iRt.anchorMin = new Vector2(0f, 0f);
            iRt.anchorMax = new Vector2(1f, 1f);
            iRt.offsetMin = Vector2.zero;
            iRt.offsetMax = new Vector2(0f, -_topInsetRef);

            const float iconBtnSize = 96f;
            var back = PaletteUI.BuildIconBtn(inner.transform, "BackButton", "arrow_back", iconBtnSize, false);
            var bRt = (RectTransform)back.transform;
            bRt.anchorMin = new Vector2(0f, 1f);
            bRt.anchorMax = new Vector2(0f, 1f);
            bRt.pivot     = new Vector2(0f, 1f);
            bRt.anchoredPosition = new Vector2(HeaderHPad, -HeaderVPadTop);
            back.onClick.AddListener(() => SceneLoader.LoadScene(SceneLoader.LevelSelect));

            float titleX = HeaderHPad + iconBtnSize + 24f;
            const float subtitleH = 48f;
            var subtitle = DesignTokens.BuildLocalizedText(inner.transform, "Subtitle",
                "gallery_subtitle", "나의 갤러리",
                PaletteTokens.FontCaptionSm, FontStyle.Normal, p.TextSoft, TextAnchor.UpperLeft);
            subtitle.horizontalOverflow = HorizontalWrapMode.Overflow;
            subtitle.verticalOverflow   = VerticalWrapMode.Overflow;
            var stRt = (RectTransform)subtitle.transform;
            stRt.anchorMin = new Vector2(0f, 1f);
            stRt.anchorMax = new Vector2(0f, 1f);
            stRt.pivot     = new Vector2(0f, 1f);
            stRt.sizeDelta = new Vector2(500f, subtitleH);
            stRt.anchoredPosition = new Vector2(titleX, -HeaderVPadTop);

            const float titleH = 96f;
            var title = DesignTokens.BuildLocalizedText(inner.transform, "Title",
                "gallery_title", "그림 수집관",
                PaletteTokens.FontDisplay, FontStyle.Bold, p.Text, TextAnchor.UpperLeft);
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            title.verticalOverflow   = VerticalWrapMode.Overflow;
            var tRt = (RectTransform)title.transform;
            tRt.anchorMin = new Vector2(0f, 1f);
            tRt.anchorMax = new Vector2(0f, 1f);
            tRt.pivot     = new Vector2(0f, 1f);
            tRt.sizeDelta = new Vector2(700f, titleH);
            tRt.anchoredPosition = new Vector2(titleX, -HeaderVPadTop - subtitleH);

            BuildCountPill(inner.transform, cleared, total);
        }

        private void BuildCountPill(Transform parent, int cleared, int total)
        {
            var p = PaletteTokens.Active;
            const float pillH = 78f;
            int radius = (int)(pillH / 2f);

            var pill = PaletteUI.BuildSurface(parent, "CountPill", p.Surface, radius);
            var rt = (RectTransform)pill.transform;
            rt.anchorMin = new Vector2(1f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot     = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(-HeaderHPad, 0f);

            string fallback =
                $"<color=#{ColorUtility.ToHtmlStringRGB(p.PrimaryDeep)}><b>{cleared}</b></color>" +
                $" <color=#{ColorUtility.ToHtmlStringRGB(p.TextSoft)}>/ {total}</color>";
            var lbl = DesignTokens.BuildLocalizedTextFormat(pill.transform, "Lbl",
                "gallery_count_format", fallback, new object[] { cleared, total },
                PaletteTokens.FontCaption, FontStyle.Bold, p.Text, TextAnchor.MiddleCenter);
            lbl.supportRichText = true;
            float textW = lbl.preferredWidth;
            rt.sizeDelta = new Vector2(textW + 90f, pillH);

            var lRt = (RectTransform)lbl.transform;
            lRt.anchorMin = Vector2.zero;
            lRt.anchorMax = Vector2.one;
            lRt.offsetMin = Vector2.zero;
            lRt.offsetMax = Vector2.zero;
        }

        // ─────────────────────────────────────────────────────
        // Progress 카드
        // ─────────────────────────────────────────────────────

        private void BuildProgressCard(Transform parent, float topY, int cleared, int total, int currentIdx)
        {
            var p = PaletteTokens.Active;

            var wrap = BuildSection(parent, "ProgressWrap", topY, ProgressH, new Color(0, 0, 0, 0));

            var card = PaletteUI.BuildSurface(wrap.transform, "Card", p.Surface, 24);
            var cRt = (RectTransform)card.transform;
            cRt.anchorMin = new Vector2(0f, 0f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.offsetMin = new Vector2(ProgressHPad, ProgressVPad);
            cRt.offsetMax = new Vector2(-ProgressHPad, -ProgressVPad);

            const float rowH = 48f;
            var leftLbl = DesignTokens.BuildLocalizedText(card.transform, "LeftLbl",
                "gallery_progress_until", "다음 작품까지",
                PaletteTokens.FontCaptionSm, FontStyle.Bold, p.TextSoft, TextAnchor.MiddleLeft);
            leftLbl.horizontalOverflow = HorizontalWrapMode.Overflow;
            leftLbl.verticalOverflow   = VerticalWrapMode.Overflow;
            var lRt = (RectTransform)leftLbl.transform;
            lRt.anchorMin = new Vector2(0f, 1f);
            lRt.anchorMax = new Vector2(0.5f, 1f);
            lRt.pivot     = new Vector2(0f, 1f);
            lRt.sizeDelta = new Vector2(0f, rowH);
            lRt.offsetMin = new Vector2(30f, lRt.offsetMin.y);
            lRt.offsetMax = new Vector2(0f, lRt.offsetMax.y);
            lRt.anchoredPosition = new Vector2(lRt.anchoredPosition.x, -24f);

            string nextLevelText = currentIdx < total ? $"레벨 {currentIdx + 1} 진행중" : "모두 완성!";
            Text rightLbl;
            if (currentIdx < total)
            {
                rightLbl = DesignTokens.BuildLocalizedTextFormat(card.transform, "RightLbl",
                    "gallery_progress_now", nextLevelText, new object[] { currentIdx + 1 },
                    PaletteTokens.FontCaptionSm, FontStyle.Bold, p.PrimaryDeep, TextAnchor.MiddleRight);
            }
            else
            {
                rightLbl = DesignTokens.BuildLocalizedText(card.transform, "RightLbl",
                    "level_badge_all_done", nextLevelText,
                    PaletteTokens.FontCaptionSm, FontStyle.Bold, p.PrimaryDeep, TextAnchor.MiddleRight);
            }
            rightLbl.horizontalOverflow = HorizontalWrapMode.Overflow;
            rightLbl.verticalOverflow   = VerticalWrapMode.Overflow;
            var rRt = (RectTransform)rightLbl.transform;
            rRt.anchorMin = new Vector2(0.5f, 1f);
            rRt.anchorMax = new Vector2(1f, 1f);
            rRt.pivot     = new Vector2(1f, 1f);
            rRt.sizeDelta = new Vector2(0f, rowH);
            rRt.offsetMin = new Vector2(0f, rRt.offsetMin.y);
            rRt.offsetMax = new Vector2(-30f, rRt.offsetMax.y);
            rRt.anchoredPosition = new Vector2(rRt.anchoredPosition.x, -24f);

            float barWidth = RefW - ProgressHPad * 2f - 60f;
            float ratio = total > 0 ? (float)cleared / total : 0f;
            var bar = PaletteUI.BuildProgressBar(card.transform, "Bar", ratio, total, null, barWidth);
            var barRt = (RectTransform)bar.transform;
            barRt.anchorMin = new Vector2(0f, 0f);
            barRt.anchorMax = new Vector2(0f, 0f);
            barRt.pivot     = new Vector2(0f, 0f);
            barRt.anchoredPosition = new Vector2(30f, 30f);
        }

        // ─────────────────────────────────────────────────────
        // ScrollContent — Floor1 / Floor2 카드
        // ─────────────────────────────────────────────────────

        private void BuildScrollContent(Transform parent, float topY, LevelDataSO[] levels, int currentIdx)
        {
            var clearedList = new List<(LevelDataSO lv, int origIdx)>();
            for (int i = 0; i < levels.Length; i++)
                if (SaveManager.IsLevelComplete(i))
                    clearedList.Add((levels[i], i));

            float bottomInset = NavH + ComputeBottomInsetRef();
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

            if (clearedList.Count == 0)
            {
                BuildEmptyState(vpGo.transform);
                scroll.viewport = vpRt;
                return;
            }

            int floor1Count = Mathf.Min(PiecesPerFloor, clearedList.Count);
            int floor2Count = Mathf.Max(0, clearedList.Count - PiecesPerFloor);

            float cardInnerW = RefW - ContentSidePad * 2f - FloorCardPad * 2f;
            float cellW = (cardInnerW - GridGap * (GridCols - 1)) / GridCols;
            float frameH = cellW;
            float cellH = frameH + CaptionTopGap + CaptionH;

            int floor1Rows = Mathf.CeilToInt((float)floor1Count / GridCols);
            int floor2Rows = Mathf.CeilToInt((float)floor2Count / GridCols);
            float floor1GridH = floor1Rows * cellH + Mathf.Max(0, floor1Rows - 1) * GridGap;
            float floor2GridH = floor2Rows * cellH + Mathf.Max(0, floor2Rows - 1) * GridGap;

            float floor1CardH = FloorCardPad + FloorLabelH + 24f + floor1GridH + FloorCardPad;
            float floor2CardH = floor2Count > 0
                ? FloorCardPad + FloorLabelH + 24f + floor2GridH + FloorCardPad
                : 0f;

            float contentH = ContentTopPad + floor1CardH
                           + (floor2Count > 0 ? FloorGapH + floor2CardH : 0f)
                           + ContentBottomPad;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(vpGo.transform, worldPositionStays: false);
            var crt = (RectTransform)contentGo.transform;
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot     = new Vector2(0.5f, 1f);
            crt.sizeDelta = new Vector2(0f, contentH);
            crt.anchoredPosition = Vector2.zero;

            scroll.viewport = vpRt;
            scroll.content  = crt;

            float yCursor = ContentTopPad;

            BuildFloorCard(contentGo.transform, "Floor1Card", "FIRST FLOOR", "gallery_floor_first",
                clearedList, 0, floor1Count, yCursor, floor1CardH,
                cellW, frameH, cellH, floor1GridH);
            yCursor += floor1CardH + FloorGapH;

            if (floor2Count > 0)
            {
                BuildFloorCard(contentGo.transform, "Floor2Card", "SECOND FLOOR", "gallery_floor_second",
                    clearedList, PiecesPerFloor, floor2Count, yCursor, floor2CardH,
                    cellW, frameH, cellH, floor2GridH);
            }
        }

        private void BuildEmptyState(Transform parent)
        {
            var p = PaletteTokens.Active;
            var msg = DesignTokens.BuildLocalizedText(parent, "EmptyState",
                "gallery_empty", "아직 완성한 퍼즐이 없어요",
                PaletteTokens.FontBody, FontStyle.Bold, p.TextSoft, TextAnchor.MiddleCenter);
            msg.horizontalOverflow = HorizontalWrapMode.Overflow;
            var rt = (RectTransform)msg.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var hint = DesignTokens.BuildLocalizedText(parent, "EmptyHint",
                "gallery_empty_hint", "퍼즐을 완성하면 갤러리에 모여요",
                PaletteTokens.FontCaptionSm, FontStyle.Normal, p.TextMuted, TextAnchor.MiddleCenter);
            hint.horizontalOverflow = HorizontalWrapMode.Overflow;
            var hRt = (RectTransform)hint.transform;
            hRt.anchorMin = new Vector2(0f, 0.5f);
            hRt.anchorMax = new Vector2(1f, 0.5f);
            hRt.pivot     = new Vector2(0.5f, 1f);
            hRt.sizeDelta = new Vector2(0f, PaletteTokens.FontCaptionSm + 12);
            hRt.anchoredPosition = new Vector2(0f, -36f);
        }

        // ─────────────────────────────────────────────────────
        // Floor 카드
        // ─────────────────────────────────────────────────────

        private void BuildFloorCard(Transform parent, string name, string floorLabelText, string floorLabelKey,
                                    List<(LevelDataSO lv, int origIdx)> cleared,
                                    int startIdx, int count, float topY, float cardH,
                                    float cellW, float frameH, float cellH, float gridH)
        {
            var p = PaletteTokens.Active;

            var wrap = new GameObject(name + "_Wrap", typeof(RectTransform));
            wrap.transform.SetParent(parent, worldPositionStays: false);
            var wRt = (RectTransform)wrap.transform;
            wRt.anchorMin = new Vector2(0f, 1f);
            wRt.anchorMax = new Vector2(1f, 1f);
            wRt.pivot     = new Vector2(0.5f, 1f);
            wRt.sizeDelta = new Vector2(0f, cardH);
            wRt.anchoredPosition = new Vector2(0f, -topY);

            var card = PaletteUI.BuildSurface(wrap.transform, name, p.Surface, (int)FloorCardRadius);
            var cRt = (RectTransform)card.transform;
            cRt.anchorMin = new Vector2(0f, 0f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.offsetMin = new Vector2(ContentSidePad, 0f);
            cRt.offsetMax = new Vector2(-ContentSidePad, 0f);

            BuildFloorLabel(card.transform, "FloorLabel", floorLabelText, floorLabelKey, FloorCardPad);

            float gridTopY = FloorCardPad + FloorLabelH + 24f;
            var grid = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            grid.transform.SetParent(card.transform, worldPositionStays: false);
            var gRt = (RectTransform)grid.transform;
            gRt.anchorMin = new Vector2(0f, 1f);
            gRt.anchorMax = new Vector2(1f, 1f);
            gRt.pivot     = new Vector2(0.5f, 1f);
            gRt.sizeDelta = new Vector2(0f, gridH);
            gRt.offsetMin = new Vector2(FloorCardPad, gRt.offsetMin.y);
            gRt.offsetMax = new Vector2(-FloorCardPad, gRt.offsetMax.y);
            gRt.anchoredPosition = new Vector2(gRt.anchoredPosition.x, -gridTopY);

            var glg = grid.GetComponent<GridLayoutGroup>();
            glg.cellSize        = new Vector2(cellW, cellH);
            glg.spacing         = new Vector2(GridGap, GridGap);
            glg.padding         = new RectOffset(0, 0, 0, 0);
            glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = GridCols;
            glg.childAlignment  = TextAnchor.UpperLeft;

            for (int i = 0; i < count; i++)
            {
                int slotIdx = startIdx + i;
                if (slotIdx >= cleared.Count) break;
                var (lv, origIdx) = cleared[slotIdx];
                BuildGalleryCell(grid.transform, lv, origIdx, cellW, frameH);
            }
        }

        private void BuildFloorLabel(Transform parent, string name, string text, string entryKey, float topY)
        {
            var p = PaletteTokens.Active;
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, worldPositionStays: false);
            var rt = (RectTransform)row.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, FloorLabelH);
            rt.offsetMin = new Vector2(FloorCardPad, rt.offsetMin.y);
            rt.offsetMax = new Vector2(-FloorCardPad, rt.offsetMax.y);
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -topY);

            var lbl = string.IsNullOrEmpty(entryKey)
                ? DesignTokens.BuildText(row.transform, "Lbl", text,
                    PaletteTokens.FontCaption, FontStyle.Bold, p.TextSoft, TextAnchor.MiddleCenter)
                : DesignTokens.BuildLocalizedText(row.transform, "Lbl", entryKey, text,
                    PaletteTokens.FontCaption, FontStyle.Bold, p.TextSoft, TextAnchor.MiddleCenter);
            lbl.horizontalOverflow = HorizontalWrapMode.Overflow;
            float textW = lbl.preferredWidth + 60f;
            var lRt = (RectTransform)lbl.transform;
            lRt.anchorMin = new Vector2(0.5f, 0f);
            lRt.anchorMax = new Vector2(0.5f, 1f);
            lRt.pivot     = new Vector2(0.5f, 0.5f);
            lRt.sizeDelta = new Vector2(textW, 0f);
            lRt.anchoredPosition = Vector2.zero;

            var leftDash = new GameObject("LeftDash", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            leftDash.transform.SetParent(row.transform, worldPositionStays: false);
            var ldRt = (RectTransform)leftDash.transform;
            ldRt.anchorMin = new Vector2(0f, 0.5f);
            ldRt.anchorMax = new Vector2(0.5f, 0.5f);
            ldRt.pivot     = new Vector2(0f, 0.5f);
            ldRt.sizeDelta = new Vector2(0f, 3f);
            ldRt.offsetMax = new Vector2(-(textW * 0.5f + 24f), ldRt.offsetMax.y);
            ldRt.offsetMin = new Vector2(0f, ldRt.offsetMin.y);
            leftDash.GetComponent<Image>().color = p.Line;
            leftDash.GetComponent<Image>().raycastTarget = false;

            var rightDash = new GameObject("RightDash", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            rightDash.transform.SetParent(row.transform, worldPositionStays: false);
            var rdRt = (RectTransform)rightDash.transform;
            rdRt.anchorMin = new Vector2(0.5f, 0.5f);
            rdRt.anchorMax = new Vector2(1f, 0.5f);
            rdRt.pivot     = new Vector2(1f, 0.5f);
            rdRt.sizeDelta = new Vector2(0f, 3f);
            rdRt.offsetMin = new Vector2(textW * 0.5f + 24f, rdRt.offsetMin.y);
            rdRt.offsetMax = new Vector2(0f, rdRt.offsetMax.y);
            rightDash.GetComponent<Image>().color = p.Line;
            rightDash.GetComponent<Image>().raycastTarget = false;
        }

        // ─────────────────────────────────────────────────────
        // 갤러리 셀 — 골드 액자 + 작품 + 우상단 조각수 + 작품명 캡션
        // ─────────────────────────────────────────────────────

        private void BuildGalleryCell(Transform parent, LevelDataSO lv, int levelIdx, float cellW, float frameH)
        {
            var p = PaletteTokens.Active;

            var cell = new GameObject($"Cell_{levelIdx:D2}", typeof(RectTransform));
            cell.transform.SetParent(parent, worldPositionStays: false);

            int radius = 18;
            var frame = PaletteUI.BuildSurface(cell.transform, "Frame", Color.white, radius);
            var frameImg = frame.GetComponent<Image>();
            frameImg.sprite = PaletteUI.GoldFrameGradient();
            frameImg.type   = Image.Type.Simple;
            frameImg.color  = Color.white;
            var fRt = (RectTransform)frame.transform;
            fRt.anchorMin = new Vector2(0f, 1f);
            fRt.anchorMax = new Vector2(1f, 1f);
            fRt.pivot     = new Vector2(0.5f, 1f);
            fRt.sizeDelta = new Vector2(0f, frameH);
            fRt.anchoredPosition = Vector2.zero;

            var btn = frame.AddComponent<Button>();
            btn.targetGraphic = frame.GetComponent<Image>();
            int captured = levelIdx;
            btn.onClick.AddListener(() => OnFrameClicked(captured));

            var shadow = new GameObject("FrameShadow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            shadow.transform.SetParent(cell.transform, worldPositionStays: false);
            shadow.transform.SetAsFirstSibling();
            var shRt = (RectTransform)shadow.transform;
            shRt.anchorMin = new Vector2(0f, 1f);
            shRt.anchorMax = new Vector2(1f, 1f);
            shRt.pivot     = new Vector2(0.5f, 1f);
            shRt.sizeDelta = new Vector2(12f, frameH + 6f);
            shRt.anchoredPosition = new Vector2(0f, -18f);
            var shImg = shadow.GetComponent<Image>();
            shImg.sprite = PaletteUI.RoundedRect(radius);
            shImg.type   = Image.Type.Sliced;
            shImg.pixelsPerUnitMultiplier = 1f;
            shImg.color  = new Color(60f / 255f, 40f / 255f, 20f / 255f, 0.22f);
            shImg.raycastTarget = false;

            const float framePadding = 18f;
            var coverWrap = new GameObject("CoverWrap",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
            coverWrap.transform.SetParent(frame.transform, worldPositionStays: false);
            var cwRt = (RectTransform)coverWrap.transform;
            cwRt.anchorMin = Vector2.zero;
            cwRt.anchorMax = Vector2.one;
            cwRt.offsetMin = new Vector2(framePadding, framePadding);
            cwRt.offsetMax = new Vector2(-framePadding, -framePadding);
            coverWrap.GetComponent<Image>().color = DesignTokens.Hex("#FFFEFA");
            coverWrap.GetComponent<Image>().raycastTarget = false;

            if (lv != null && lv.sourceSprite != null)
            {
                const float matteInset = 18f;
                var img = new GameObject("Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                img.transform.SetParent(coverWrap.transform, worldPositionStays: false);
                var iRt = (RectTransform)img.transform;
                iRt.anchorMin = Vector2.zero; iRt.anchorMax = Vector2.one;
                iRt.offsetMin = new Vector2(matteInset, matteInset);
                iRt.offsetMax = new Vector2(-matteInset, -matteInset);
                var iImg = img.GetComponent<Image>();
                iImg.sprite = lv.sourceSprite;
                iImg.color = Color.white;
                iImg.raycastTarget = false;
            }

            int pieceCount = lv != null ? lv.gridSize * lv.gridSize - 1 : 0;
            PaletteUI.BuildPiecesBadge(frame.transform, pieceCount, bgColor: PaletteTokens.Active.Gold);
            var badgeT = frame.transform.Find("PiecesBadge");
            if (badgeT != null)
            {
                var bgRt = (RectTransform)badgeT;
                bgRt.anchoredPosition = new Vector2(12f, 18f);
                bgRt.SetAsLastSibling();
            }

            string name = LevelDataSO.GetDefaultArtworkName(lv != null ? lv.levelNumber : levelIdx + 1);
            var caption = DesignTokens.BuildText(cell.transform, "Caption", name,
                PaletteTokens.FontLabel, FontStyle.Bold, p.Text, TextAnchor.MiddleCenter);
            caption.horizontalOverflow = HorizontalWrapMode.Wrap;
            caption.verticalOverflow   = VerticalWrapMode.Truncate;
            var capRt = (RectTransform)caption.transform;
            capRt.anchorMin = new Vector2(0f, 0f);
            capRt.anchorMax = new Vector2(1f, 0f);
            capRt.pivot     = new Vector2(0.5f, 0f);
            capRt.sizeDelta = new Vector2(0f, CaptionH);
            capRt.anchoredPosition = Vector2.zero;
        }

        // ─────────────────────────────────────────────────────
        // Nav
        // ─────────────────────────────────────────────────────

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

            var nav = PaletteUI.BuildBottomNav(parent, "BottomNav", "collection", OnNavTabClicked, NavItems2);
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


        private void OnFrameClicked(int levelIdx)
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.currentLevelIndex = levelIdx;
            SceneLoader.LoadScene(SceneLoader.Gameplay);
        }

        private void OnNavTabClicked(string key)
        {
            if (key == "collection") return;
            if (key == "home")
            {
                SceneLoader.LoadScene(SceneLoader.LevelSelect);
                return;
            }
            ShowToast("준비 중", "common_coming_soon");
        }

        private void ShowToast(string fallback, string entryKey = null)
        {
            if (_toastGO != null) Destroy(_toastGO);
            if (_canvas == null) return;
            _toastGO = PaletteUI.BuildToast(_canvas.transform, fallback, 1.5f, entryKey);
        }
    }
}
