using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SpotDiff.Core;
using SpotDiff.Data;
using SpotDiff.Systems;

namespace SpotDiff.UI
{
    /// <summary>
    /// Gallery 씬 — MyGames(Puzzle Pals) GalleryMuseumController 의 축소판.
    /// 레벨이 3개뿐이라 1F/2F 층 나누기 없이 단일 그리드. 골드 액자 컨셉은 그대로 재사용.
    /// 완료(cleared)한 레벨의 원본 이미지만 노출.
    /// </summary>
    [DisallowMultipleComponent]
    public class GalleryController : MonoBehaviour
    {
        private const string LevelsResourcePath = "Levels";
        private const float RefW = 1080f;
        private const float RefH = 1920f;

        private const float HeaderH = 210f;
        private const float GridHPad = 48f;
        private const float GridGap  = 30f;
        private const int   GridCols = 3;
        private const float CaptionH = 60f;
        private const float CaptionTopGap = 12f;

        private static readonly PaletteUI.NavItem[] NavItems2 =
        {
            new PaletteUI.NavItem { Key = "home",       Label = "메인",  IconKey = "home",  EntryKey = "nav_home" },
            new PaletteUI.NavItem { Key = "collection", Label = "갤러리", IconKey = "image", EntryKey = "nav_collection" },
        };

        private Canvas _canvas;

        private void Start() => Build();

        public void Build()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
                else                       DestroyImmediate(transform.GetChild(i).gameObject);
            }

            _canvas = BuildCanvas();
            var p = PaletteTokens.Active;
            BuildFullStretch(_canvas.transform, "Background", p.Bg);

            var levels = Resources.LoadAll<LevelDataSO>(LevelsResourcePath);
            Array.Sort(levels, (a, b) => a.levelNumber.CompareTo(b.levelNumber));

            var clearedList = new List<(LevelDataSO lv, int idx)>();
            for (int i = 0; i < levels.Length; i++)
                if (SaveManager.IsLevelComplete(i))
                    clearedList.Add((levels[i], i));

            float headerH = BuildHeader(_canvas.transform, clearedList.Count, levels.Length);

            if (clearedList.Count == 0)
                BuildEmptyState(_canvas.transform);
            else
                BuildGrid(_canvas.transform, clearedList, headerH);

            BuildNav(_canvas.transform);
        }

        private Canvas BuildCanvas()
        {
            var go = new GameObject("GalleryCanvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, worldPositionStays: false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(RefW, RefH);
            scaler.matchWidthOrHeight = 0f;
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

        /// <summary>Header 를 짓고 최종 높이(HeaderH + SafeArea top inset)를 반환.</summary>
        private float BuildHeader(Transform parent, int cleared, int total)
        {
            var p = PaletteTokens.Active;
            var header = new GameObject("Header", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            header.transform.SetParent(parent, worldPositionStays: false);
            var rt = (RectTransform)header.transform;
            rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, HeaderH);
            header.GetComponent<Image>().color = p.Surface;

            // 노치/상태바 영역까지 배경을 늘리고, 실제 컨텐츠는 SafeArea 안쪽으로 push.
            var ext = header.AddComponent<SafeAreaTopExtender>();
            var inner = new GameObject("Inner", typeof(RectTransform));
            inner.transform.SetParent(header.transform, worldPositionStays: false);
            var innerRt = (RectTransform)inner.transform;
            innerRt.anchorMin = Vector2.zero; innerRt.anchorMax = Vector2.one;
            innerRt.offsetMin = Vector2.zero; innerRt.offsetMax = Vector2.zero;
            ext.innerContent = innerRt;
            ext.Refresh();

            var title = DesignTokens.BuildLocalizedText(innerRt, "Title",
                "gallery_title", "갤러리", PaletteTokens.FontDisplay, FontStyle.Bold, p.Text, TextAnchor.MiddleLeft);
            var tRt = (RectTransform)title.transform;
            tRt.anchorMin = new Vector2(0f, 0f); tRt.anchorMax = new Vector2(0.6f, 1f);
            tRt.offsetMin = new Vector2(GridHPad, 0f); tRt.offsetMax = Vector2.zero;

            const float pillH = 66f;
            var pill = PaletteUI.BuildSurface(innerRt, "CountPill", p.SurfaceAlt, (int)(pillH / 2f));
            var pRt = (RectTransform)pill.transform;
            pRt.anchorMin = pRt.anchorMax = new Vector2(1f, 0.5f);
            pRt.pivot = new Vector2(1f, 0.5f);
            pRt.sizeDelta = new Vector2(120f, pillH);
            pRt.anchoredPosition = new Vector2(-GridHPad, 0f);
            var lbl = DesignTokens.BuildLocalizedTextFormat(pill.transform, "Lbl", "gallery_count_format",
                $"{cleared}/{total}", new object[] { cleared, total },
                PaletteTokens.FontBody, FontStyle.Bold, p.Text, TextAnchor.MiddleCenter);
            var lRt = (RectTransform)lbl.transform;
            lRt.anchorMin = Vector2.zero; lRt.anchorMax = Vector2.one;
            lRt.offsetMin = Vector2.zero; lRt.offsetMax = Vector2.zero;

            return rt.sizeDelta.y;
        }

        private void BuildEmptyState(Transform parent)
        {
            var p = PaletteTokens.Active;
            var msg = DesignTokens.BuildLocalizedText(parent, "EmptyState",
                "gallery_empty", "아직 완성한 그림이 없어요",
                PaletteTokens.FontBody, FontStyle.Bold, p.TextSoft, TextAnchor.MiddleCenter);
            var rt = (RectTransform)msg.transform;
            rt.anchorMin = new Vector2(0f, 0.4f); rt.anchorMax = new Vector2(1f, 0.6f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private void BuildGrid(Transform parent, List<(LevelDataSO lv, int idx)> cleared, float headerH)
        {
            var gridGo = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridGo.transform.SetParent(parent, worldPositionStays: false);
            var grt = (RectTransform)gridGo.transform;
            grt.anchorMin = new Vector2(0f, 1f); grt.anchorMax = new Vector2(1f, 1f);
            grt.pivot = new Vector2(0.5f, 1f);
            grt.anchoredPosition = new Vector2(0f, -(headerH + 36f));

            float cellW = (RefW - GridHPad * 2f - GridGap * (GridCols - 1)) / GridCols;
            float frameH = cellW;
            float cellH = frameH + CaptionTopGap + CaptionH;
            int rows = Mathf.CeilToInt(cleared.Count / (float)GridCols);
            grt.sizeDelta = new Vector2(0f, rows * cellH + Mathf.Max(0, rows - 1) * GridGap);

            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(cellW, cellH);
            grid.spacing  = new Vector2(GridGap, GridGap);
            grid.padding  = new RectOffset((int)GridHPad, (int)GridHPad, 0, 0);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = GridCols;
            grid.childAlignment = TextAnchor.UpperLeft;

            foreach (var (lv, idx) in cleared)
                BuildFrameCell(gridGo.transform, lv, idx, frameH);
        }

        private void BuildFrameCell(Transform parent, LevelDataSO lv, int levelIdx, float frameH)
        {
            var p = PaletteTokens.Active;

            var cell = new GameObject($"Cell_{levelIdx:D2}", typeof(RectTransform));
            cell.transform.SetParent(parent, worldPositionStays: false);

            var frame = PaletteUI.BuildSurface(cell.transform, "Frame", Color.white, 18);
            var frameImg = frame.GetComponent<Image>();
            frameImg.sprite = PaletteUI.GoldFrameGradient();
            frameImg.type   = Image.Type.Simple;
            frameImg.color  = Color.white;
            var fRt = (RectTransform)frame.transform;
            fRt.anchorMin = new Vector2(0f, 1f); fRt.anchorMax = new Vector2(1f, 1f);
            fRt.pivot = new Vector2(0.5f, 1f);
            fRt.sizeDelta = new Vector2(0f, frameH);

            var btn = frame.AddComponent<Button>();
            btn.targetGraphic = frameImg;
            int captured = levelIdx;
            btn.onClick.AddListener(() => OnFrameClicked(captured));

            const float matPad = 18f;
            var mat = new GameObject("Mat", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
            mat.transform.SetParent(frame.transform, worldPositionStays: false);
            var mRt = (RectTransform)mat.transform;
            mRt.anchorMin = Vector2.zero; mRt.anchorMax = Vector2.one;
            mRt.offsetMin = new Vector2(matPad, matPad); mRt.offsetMax = new Vector2(-matPad, -matPad);
            mat.GetComponent<Image>().color = DesignTokens.Hex("#FFFEFA");

            if (lv != null && lv.originalSprite != null)
            {
                const float imgInset = 18f;
                var img = new GameObject("Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                img.transform.SetParent(mat.transform, worldPositionStays: false);
                var iRt = (RectTransform)img.transform;
                iRt.anchorMin = Vector2.zero; iRt.anchorMax = Vector2.one;
                iRt.offsetMin = new Vector2(imgInset, imgInset); iRt.offsetMax = new Vector2(-imgInset, -imgInset);
                var iImg = img.GetComponent<Image>();
                iImg.sprite = lv.originalSprite;
                iImg.preserveAspect = true;
            }

            string diffEntryKey = lv != null ? LevelDataSO.DifficultyEntryKey(lv.difficulty) : "difficulty_easy";
            string diffFallback = lv != null ? LevelDataSO.DifficultyFallback(lv.difficulty) : "";
            var caption = DesignTokens.BuildLocalizedText(cell.transform, "Caption", diffEntryKey, diffFallback,
                PaletteTokens.FontLabel, FontStyle.Bold, p.Text, TextAnchor.MiddleCenter);
            var capRt = (RectTransform)caption.transform;
            capRt.anchorMin = new Vector2(0f, 0f); capRt.anchorMax = new Vector2(1f, 0f);
            capRt.pivot = new Vector2(0.5f, 0f);
            capRt.sizeDelta = new Vector2(0f, CaptionH);
        }

        private void BuildNav(Transform parent)
        {
            var nav = PaletteUI.BuildBottomNav(parent, "BottomNav", "collection", OnNavTabClicked, NavItems2);
            var rt = (RectTransform)nav.transform;
            rt.anchorMin = new Vector2(0f, 0f); rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = Vector2.zero;
        }

        private void OnFrameClicked(int levelIdx)
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.currentLevelIndex = levelIdx;
            SceneLoader.LoadScene(SceneLoader.Gameplay);
        }

        private void OnNavTabClicked(string key)
        {
            if (key == "collection") return;
            // "메인" 탭 = 앱의 허브인 LevelSelect 로 복귀. MainMenu(타이틀/PLAY)는 최초 진입 때만 노출.
            if (key == "home") SceneLoader.LoadScene(SceneLoader.LevelSelect);
        }
    }
}
