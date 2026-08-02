using System;
using UnityEngine;
using UnityEngine.UI;
using SpotDiff.Core;
using SpotDiff.Data;
using SpotDiff.Systems;

namespace SpotDiff.UI
{
    /// <summary>
    /// LevelSelect 씬 — MyGames(Puzzle Pals) LevelSelectController 의 HomeSequential 패턴을 그대로 재사용.
    /// Header(타이틀) → Section Label → 3열 SequentialTile 그리드 (PaletteUI.BuildSequentialTile 공용 컴포넌트).
    /// </summary>
    [DisallowMultipleComponent]
    public class LevelSelectController : MonoBehaviour
    {
        private const string LevelsResourcePath = "Levels";
        private const float RefW = 1080f;
        private const float RefH = 1920f;

        private const float HeaderH       = 210f;
        private const float SectionLabelH = 96f;
        private const float SectionLabelHPad = 48f;
        private const float SectionLabelVTop = 24f;
        private const float SectionLabelVBot = 36f;

        private const float GridHPad = 48f;
        private const float GridGap  = 30f;
        private const int   GridCols = 3;

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

            int total      = levels.Length;
            int currentIdx = SaveManager.GetCurrentLevelIndex(total);

            float topY = 0f;
            float headerH = BuildHeader(_canvas.transform, topY);
            topY += headerH;

            BuildSectionLabelWrap(_canvas.transform, topY);
            topY += SectionLabelH;

            BuildGrid(_canvas.transform, topY, levels, currentIdx);
            BuildNav(_canvas.transform);
        }

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

        private const float SettingsBtnSize = 108f;

        /// <summary>Header 를 짓고 최종 높이(HeaderH + SafeArea top inset)를 반환 — 호출 측이 topY 진행에 사용.</summary>
        private float BuildHeader(Transform parent, float topY)
        {
            var p = PaletteTokens.Active;
            var header = BuildSection(parent, "Header", topY, HeaderH, p.Surface);
            BuildBottomBorder(header.transform, p.Line);

            // 노치/상태바 영역까지 배경(p.Surface) 을 늘리고, 실제 컨텐츠는 SafeArea 안쪽으로 push.
            var ext = header.AddComponent<SafeAreaTopExtender>();

            var inner = new GameObject("Inner", typeof(RectTransform));
            inner.transform.SetParent(header.transform, worldPositionStays: false);
            var innerRt = (RectTransform)inner.transform;
            innerRt.anchorMin = Vector2.zero; innerRt.anchorMax = Vector2.one;
            innerRt.offsetMin = Vector2.zero; innerRt.offsetMax = Vector2.zero;
            ext.innerContent = innerRt;
            ext.Refresh();

            var title = DesignTokens.BuildLocalizedText(innerRt, "Title",
                "level_select_title", "틀린그림찾기",
                PaletteTokens.FontDisplay, FontStyle.Bold, p.Text, TextAnchor.UpperLeft);
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            var tRt = (RectTransform)title.transform;
            tRt.anchorMin = new Vector2(0f, 0f); tRt.anchorMax = new Vector2(1f, 1f);
            tRt.offsetMin = new Vector2(SectionLabelHPad, 0f);
            tRt.offsetMax = new Vector2(-(SettingsBtnSize + SectionLabelHPad), -60f);

            var settingsBtn = PaletteUI.BuildIconBtn(innerRt, "Settings",
                "settings", SettingsBtnSize, highlight: false);
            var sRt = (RectTransform)settingsBtn.transform;
            sRt.anchorMin = new Vector2(1f, 0.5f);
            sRt.anchorMax = new Vector2(1f, 0.5f);
            sRt.pivot     = new Vector2(1f, 0.5f);
            sRt.sizeDelta = new Vector2(SettingsBtnSize, SettingsBtnSize);
            sRt.anchoredPosition = new Vector2(-SectionLabelHPad, 0f);
            settingsBtn.onClick.AddListener(OnSettings);

            return ((RectTransform)header.transform).sizeDelta.y;
        }

        private void OnSettings()
        {
            var popup = FindAnyObjectByType<SettingsPopupUI>(FindObjectsInactive.Include);
            if (popup != null) popup.Show();
            else Debug.LogWarning("[LevelSelect] SettingsPopup 미배치");
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
            rt.anchorMin = new Vector2(0f, 0f); rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, 3f);
            border.GetComponent<Image>().color = color;
            border.GetComponent<Image>().raycastTarget = false;
        }

        private void BuildSectionLabelWrap(Transform parent, float topY)
        {
            var wrap = BuildSection(parent, "SectionLabelWrap", topY, SectionLabelH, new Color(0, 0, 0, 0));

            float labelH = SectionLabelH - SectionLabelVTop - SectionLabelVBot;
            float labelW = RefW - SectionLabelHPad * 2f;
            var label = PaletteUI.BuildSectionLabel(wrap.transform, "Label",
                "전체 레벨", "순서대로 열려요",
                new Vector2(labelW, labelH),
                leftEntryKey: "level_section_all", rightEntryKey: "level_section_subtitle");
            var lRt = (RectTransform)label.transform;
            lRt.anchorMin = new Vector2(0f, 1f); lRt.anchorMax = new Vector2(0f, 1f);
            lRt.pivot = new Vector2(0f, 1f);
            lRt.anchoredPosition = new Vector2(SectionLabelHPad, -SectionLabelVTop);
        }

        private void BuildGrid(Transform parent, float topY, LevelDataSO[] levels, int currentIdx)
        {
            var p = PaletteTokens.Active;

            var gridGo = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridGo.transform.SetParent(parent, worldPositionStays: false);
            var grt = (RectTransform)gridGo.transform;
            grt.anchorMin = new Vector2(0f, 1f);
            grt.anchorMax = new Vector2(1f, 1f);
            grt.pivot     = new Vector2(0.5f, 1f);
            grt.anchoredPosition = new Vector2(0f, -topY);

            float cellW = (RefW - GridHPad * 2f - GridGap * (GridCols - 1)) / GridCols;
            float cellH = cellW + PaletteUI.TileLabelHeight;
            int rows = Mathf.CeilToInt(levels.Length / (float)GridCols);
            grt.sizeDelta = new Vector2(0f, rows * cellH + Mathf.Max(0, rows - 1) * GridGap + 48f);

            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(cellW, cellH);
            grid.spacing  = new Vector2(GridGap, GridGap);
            grid.padding  = new RectOffset((int)GridHPad, (int)GridHPad, 0, 48);
            grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = GridCols;
            grid.childAlignment  = TextAnchor.UpperCenter;

            for (int i = 0; i < levels.Length; i++)
            {
                var lv = levels[i];
                PaletteUI.TileState state =
                    SaveManager.IsLevelComplete(i) ? PaletteUI.TileState.Cleared :
                    i == currentIdx                ? PaletteUI.TileState.Current :
                                                     PaletteUI.TileState.Locked;
                int stars = state == PaletteUI.TileState.Cleared ? SaveManager.GetLevelStars(i) : 0;

                var slot = new PaletteUI.TileSlot
                {
                    Level      = i + 1,
                    Stars      = stars,
                    Pieces     = lv.diffRegions?.Length ?? 0,
                    PiecesUnit = "개",
                    Label      = DifficultyLabel(lv.difficulty),
                    Sprite     = lv.transformedSprite,
                    GradA      = p.StarterA,
                    GradB      = p.StarterB,
                    State      = state,
                };

                int captured = i;
                Action onClick = state == PaletteUI.TileState.Locked
                    ? (Action)(() => ShowLockedToast())
                    : () => OnLevelClicked(captured);

                PaletteUI.BuildSequentialTile(gridGo.transform, slot, onClick);
            }
        }

        private static string DifficultyLabel(LevelDataSO.Difficulty d) => LevelDataSO.DifficultyFallback(d);

        private GameObject _toastGO;

        private void ShowLockedToast()
        {
            if (_toastGO != null) Destroy(_toastGO);
            if (_canvas == null) return;
            _toastGO = PaletteUI.BuildToast(_canvas.transform, "이전 레벨을 먼저 완성해 주세요", 1.5f, "level_locked_toast");
        }

        private void OnLevelClicked(int levelIndex)
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.currentLevelIndex = levelIndex;
            SceneLoader.LoadScene(SceneLoader.Gameplay);
        }

        // MyGames(Puzzle Pals) 하단 네비 — 2탭 (메인/갤러리)
        private static readonly PaletteUI.NavItem[] NavItems2 =
        {
            new PaletteUI.NavItem { Key = "home",       Label = "메인",  IconKey = "home",  EntryKey = "nav_home" },
            new PaletteUI.NavItem { Key = "collection", Label = "갤러리", IconKey = "image", EntryKey = "nav_collection" },
        };

        private void BuildNav(Transform parent)
        {
            var nav = PaletteUI.BuildBottomNav(parent, "BottomNav", "home", OnNavTabClicked, NavItems2);
            var rt = (RectTransform)nav.transform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot     = new Vector2(0.5f, 0f);
            rt.anchoredPosition = Vector2.zero;
        }

        private void OnNavTabClicked(string key)
        {
            if (key == "home") return; // 이미 LevelSelect ("메인" 역할 겸함)
            if (key == "collection") SceneLoader.LoadScene(SceneLoader.Gallery);
        }
    }
}
