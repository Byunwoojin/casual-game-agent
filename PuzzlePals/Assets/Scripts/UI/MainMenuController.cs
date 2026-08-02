using UnityEngine;
using UnityEngine.UI;
using PuzzlePals.Core;

namespace PuzzlePals.UI
{
    /// <summary>
    /// MainMenu 씬 컨트롤러 — v0.6 Pastel Puzzle 톤 적용.
    /// 디자인 handoff(document/design_handoff_pastel_puzzle) 8개 화면 중 MainMenu 전용 시안은 없음 —
    /// LevelSelect Header 와 동일한 팔레트/타이포 토큰만 재사용해 톤만 통일.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainMenuController : MonoBehaviour
    {
        private const float RefW = 1080f;
        private const float RefH = 1920f;

        private Canvas _canvas;
        private float  _topInsetRef;

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

            _topInsetRef = ComputeTopInsetRef();

            BuildSettingsButton(_canvas.transform);
            BuildCenter(_canvas.transform);
        }

        private Canvas BuildCanvas()
        {
            var go = new GameObject("MainMenuCanvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, worldPositionStays: false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode    = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder  = 0;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(RefW, RefH);
            scaler.matchWidthOrHeight  = 0f; // Width 고정 — 메모리 규칙

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

        private void BuildSettingsButton(Transform parent)
        {
            const float iconBtnSize = 108f;
            const float pad = 48f;
            var settingsBtn = PaletteUI.BuildIconBtn(parent, "Settings", "settings", iconBtnSize, highlight: false);
            var rt = (RectTransform)settingsBtn.transform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(iconBtnSize, iconBtnSize);
            rt.anchoredPosition = new Vector2(-pad, -(pad + _topInsetRef));
            settingsBtn.onClick.AddListener(OnSettings);
        }

        private void BuildCenter(Transform parent)
        {
            var p = PaletteTokens.Active;

            // 타이틀 — 화면 중앙보다 약간 위
            var title = DesignTokens.BuildLocalizedText(parent, "Title",
                "app_title", "퍼즐 팔스",
                PaletteTokens.FontDisplay + 12, FontStyle.Bold, p.Text, TextAnchor.MiddleCenter);
            var tRt = (RectTransform)title.transform;
            tRt.anchorMin = new Vector2(0.5f, 0.5f);
            tRt.anchorMax = new Vector2(0.5f, 0.5f);
            tRt.pivot     = new Vector2(0.5f, 0.5f);
            tRt.sizeDelta = new Vector2(RefW - 120f, PaletteTokens.FontDisplay + 60f);
            tRt.anchoredPosition = new Vector2(0f, 160f);

            // PLAY 버튼 — pill, primary color, 중앙 하단. ButtonPulse 로 GDD §Game Feel 어필 루프.
            const float btnW = 420f, btnH = 132f;
            var playGO = PaletteUI.BuildSurface(parent, "PlayButton", p.Primary, (int)(btnH / 2f));
            var pRt = (RectTransform)playGO.transform;
            pRt.anchorMin = new Vector2(0.5f, 0.5f);
            pRt.anchorMax = new Vector2(0.5f, 0.5f);
            pRt.pivot     = new Vector2(0.5f, 0.5f);
            pRt.sizeDelta = new Vector2(btnW, btnH);
            pRt.anchoredPosition = new Vector2(0f, -40f);

            var playBtn = playGO.AddComponent<Button>();
            playBtn.targetGraphic = playGO.GetComponent<Image>();
            playBtn.onClick.AddListener(OnPlay);
            playGO.AddComponent<ButtonPulse>();

            var lbl = DesignTokens.BuildLocalizedText(playGO.transform, "Lbl",
                "btn_play", "플레이",
                PaletteTokens.FontSubhead, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            var lRt = (RectTransform)lbl.transform;
            lRt.anchorMin = Vector2.zero; lRt.anchorMax = Vector2.one;
            lRt.offsetMin = Vector2.zero; lRt.offsetMax = Vector2.zero;
        }

        private void OnPlay() => SceneLoader.LoadScene(SceneLoader.LevelSelect);

        private void OnSettings()
        {
            var popup = FindAnyObjectByType<SettingsPopupUI>(FindObjectsInactive.Include);
            if (popup != null) popup.Show();
            else Debug.LogWarning("[MainMenu] SettingsPopup 미배치");
        }
    }
}
