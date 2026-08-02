using UnityEngine;
using UnityEngine.UI;
using SpotDiff.Core;
using SpotDiff.Data;
using SpotDiff.Systems;
using SpotDiff.UI;

namespace SpotDiff.Gameplay
{
    /// <summary>Complete 씬 — 원본(정답) 이미지 공개 + 다음 레벨 CTA.</summary>
    [DisallowMultipleComponent]
    public class CompleteSceneController : MonoBehaviour
    {
        private const float RefW = 1080f;
        private const float RefH = 1920f;

        // 테스트 호환용 이름 (NextButton/HomeButton GameObject 이름 고정)
        public Button NextButton { get; private set; }
        public Button HomeButton { get; private set; }

        private void Start() => Build();

        public void Build()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
                else                       DestroyImmediate(transform.GetChild(i).gameObject);
            }

            var canvas = BuildCanvas();
            var p = PaletteTokens.Active;
            BuildFullStretch(canvas.transform, "Background", p.Bg);

            int idx = LevelManager.Instance != null ? LevelManager.Instance.currentLevelIndex : 0;
            var level = Resources.Load<LevelDataSO>($"Levels/Level_{(idx + 1):D3}");

            var title = DesignTokens.BuildLocalizedText(canvas.transform, "Title",
                "complete_title", "완성하셨어요!",
                PaletteTokens.FontDisplay, FontStyle.Bold, p.Text, TextAnchor.MiddleCenter);
            var tRt = (RectTransform)title.transform;
            tRt.anchorMin = tRt.anchorMax = new Vector2(0.5f, 1f);
            tRt.pivot = new Vector2(0.5f, 1f);
            tRt.sizeDelta = new Vector2(RefW - 120f, PaletteTokens.FontDisplay + 40f);
            tRt.anchoredPosition = new Vector2(0f, -220f);

            if (level != null && level.originalSprite != null)
            {
                var frame = PaletteUI.BuildSurface(canvas.transform, "Frame", p.Surface, 32);
                var fRt = (RectTransform)frame.transform;
                fRt.anchorMin = fRt.anchorMax = new Vector2(0.5f, 0.5f);
                fRt.pivot = new Vector2(0.5f, 0.5f);
                fRt.sizeDelta = new Vector2(RefW - 160f, RefW - 160f);
                fRt.anchoredPosition = new Vector2(0f, 40f);

                var img = new GameObject("Img", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                img.transform.SetParent(frame.transform, worldPositionStays: false);
                var iRt = (RectTransform)img.transform;
                iRt.anchorMin = new Vector2(0.05f, 0.05f); iRt.anchorMax = new Vector2(0.95f, 0.95f);
                iRt.offsetMin = Vector2.zero; iRt.offsetMax = Vector2.zero;
                var iImg = img.GetComponent<Image>();
                iImg.sprite = level.originalSprite;
                iImg.preserveAspect = true;
            }

            const float ctaW = 420f, ctaH = 132f;
            var cta = PaletteUI.BuildSurface(canvas.transform, "NextButton", p.Primary, (int)(ctaH / 2f));
            var cRt = (RectTransform)cta.transform;
            cRt.anchorMin = cRt.anchorMax = new Vector2(0.5f, 0f);
            cRt.pivot = new Vector2(0.5f, 0f);
            cRt.sizeDelta = new Vector2(ctaW, ctaH);
            cRt.anchoredPosition = new Vector2(0f, 140f);

            NextButton = cta.AddComponent<Button>();
            NextButton.targetGraphic = cta.GetComponent<Image>();
            NextButton.onClick.AddListener(OnNext);

            var ctaLbl = DesignTokens.BuildLocalizedText(cta.transform, "Lbl", "complete_next_btn", "다음 레벨",
                PaletteTokens.FontSubhead, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            var clRt = (RectTransform)ctaLbl.transform;
            clRt.anchorMin = Vector2.zero; clRt.anchorMax = Vector2.one;
            clRt.offsetMin = Vector2.zero; clRt.offsetMax = Vector2.zero;

            var homeGo = new GameObject("HomeButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            homeGo.transform.SetParent(canvas.transform, worldPositionStays: false);
            var hRt = (RectTransform)homeGo.transform;
            hRt.anchorMin = hRt.anchorMax = new Vector2(0f, 1f);
            hRt.pivot = new Vector2(0f, 1f);
            hRt.sizeDelta = new Vector2(96f, 96f);
            hRt.anchoredPosition = new Vector2(36f, -120f);
            homeGo.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            HomeButton = homeGo.AddComponent<Button>();
            HomeButton.targetGraphic = homeGo.GetComponent<Image>();
            var homeIcon = DesignTokens.BuildIcon(homeGo.transform, "Icon", "home", 56, p.Text);
            var hiRt = (RectTransform)homeIcon.transform;
            hiRt.anchorMin = Vector2.zero; hiRt.anchorMax = Vector2.one;
            hiRt.offsetMin = Vector2.zero; hiRt.offsetMax = Vector2.zero;
            HomeButton.onClick.AddListener(() => SceneLoader.LoadScene(SceneLoader.LevelSelect));

            AudioManager.Instance?.PlayCompleteScene();
        }

        /// <summary>다음 레벨 에셋이 있으면 이어서 진행, 없으면(마지막 레벨) LevelSelect 로 복귀.</summary>
        private void OnNext()
        {
            int idx  = LevelManager.Instance != null ? LevelManager.Instance.currentLevelIndex : 0;
            int next = idx + 1;

            var probe = Resources.Load<LevelDataSO>($"Levels/Level_{(next + 1):D3}");
            if (probe == null)
            {
                SceneLoader.LoadScene(SceneLoader.LevelSelect);
                return;
            }

            if (LevelManager.Instance != null) LevelManager.Instance.currentLevelIndex = next;
            SceneLoader.LoadScene(SceneLoader.Gameplay);
        }

        private Canvas BuildCanvas()
        {
            var go = new GameObject("CompleteCanvas",
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
