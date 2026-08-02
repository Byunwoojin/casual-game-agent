using UnityEngine;
using UnityEngine.UI;
using SpotDiff.Core;

namespace SpotDiff.UI
{
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
            var canvas = _canvas;
            var p = PaletteTokens.Active;
            BuildFullStretch(canvas.transform, "Background", p.Bg);

            _topInsetRef = ComputeTopInsetRef();
            BuildSettingsButton(canvas.transform);

            var title = DesignTokens.BuildLocalizedText(canvas.transform, "Title",
                "app_title", "틀린그림찾기",
                PaletteTokens.FontDisplay + 12, FontStyle.Bold, p.Text, TextAnchor.MiddleCenter);
            var tRt = (RectTransform)title.transform;
            tRt.anchorMin = tRt.anchorMax = new Vector2(0.5f, 0.5f);
            tRt.pivot = new Vector2(0.5f, 0.5f);
            tRt.sizeDelta = new Vector2(RefW - 120f, PaletteTokens.FontDisplay + 60f);
            tRt.anchoredPosition = new Vector2(0f, 160f);

            const float btnW = 420f, btnH = 132f;
            var playGO = PaletteUI.BuildSurface(canvas.transform, "PlayButton", p.Primary, (int)(btnH / 2f));
            var pRt = (RectTransform)playGO.transform;
            pRt.anchorMin = pRt.anchorMax = new Vector2(0.5f, 0.5f);
            pRt.pivot = new Vector2(0.5f, 0.5f);
            pRt.sizeDelta = new Vector2(btnW, btnH);
            pRt.anchoredPosition = new Vector2(0f, -40f);

            var playBtn = playGO.AddComponent<Button>();
            playBtn.targetGraphic = playGO.GetComponent<Image>();
            playBtn.onClick.AddListener(() => SceneLoader.LoadScene(SceneLoader.LevelSelect));
            playGO.AddComponent<ButtonPulse>();

            var lbl = DesignTokens.BuildLocalizedText(playGO.transform, "Lbl", "btn_play", "플레이",
                PaletteTokens.FontSubhead, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            var lRt = (RectTransform)lbl.transform;
            lRt.anchorMin = Vector2.zero; lRt.anchorMax = Vector2.one;
            lRt.offsetMin = Vector2.zero; lRt.offsetMax = Vector2.zero;
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

        private void OnSettings()
        {
            var popup = FindAnyObjectByType<SettingsPopupUI>(FindObjectsInactive.Include);
            if (popup != null) popup.Show();
            else Debug.LogWarning("[MainMenu] SettingsPopup 미배치");
        }

        private Canvas BuildCanvas()
        {
            var go = new GameObject("MainMenuCanvas",
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
    }
}
