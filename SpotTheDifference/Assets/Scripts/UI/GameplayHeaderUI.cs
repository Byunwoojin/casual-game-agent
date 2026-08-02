using UnityEngine;
using UnityEngine.UI;
using SpotDiff.Core;
using SpotDiff.Gameplay;
using SpotDiff.Systems;

namespace SpotDiff.UI
{
    /// <summary>
    /// Gameplay 상단 툴바 — 뒤로 / 진행률 pill / 힌트(카운트 배지).
    /// PuzzlePals GameplayHeaderUI 의 최소 버전 — 4버튼(뒤로/다시/힌트/원본) 대신
    /// 이 장르에 필요한 것만: 뒤로, 힌트. "다시"는 DiffGameBoard.Build() 재호출로 추후 추가 가능.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameplayHeaderUI : MonoBehaviour
    {
        private const float HeaderH  = 168f; // 56 × 3
        private const float SidePad  = 36f;
        private const float BtnSize  = 108f;

        private DiffGameBoard _board;
        private Text _progressLabel;
        private Text _hintBadge;

        public void EnsureBuilt()
        {
            if (transform.Find("Toolbar") != null) return;
            Build();
        }

        private void Awake() => EnsureBuilt();

        public void Build()
        {
            var p = PaletteTokens.Active;

            var rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, HeaderH);
            rt.anchoredPosition = Vector2.zero;

            var bg = gameObject.AddComponent<Image>();
            bg.color = p.Surface;

            var ext = gameObject.GetComponent<SafeAreaTopExtender>();
            if (ext == null) ext = gameObject.AddComponent<SafeAreaTopExtender>();

            var toolbar = new GameObject("Toolbar", typeof(RectTransform));
            toolbar.transform.SetParent(transform, false);
            var tbRt = (RectTransform)toolbar.transform;
            tbRt.anchorMin = Vector2.zero;
            tbRt.anchorMax = Vector2.one;
            tbRt.offsetMin = Vector2.zero;
            tbRt.offsetMax = Vector2.zero;
            ext.innerContent = tbRt;

            // 뒤로
            var backBtn = PaletteUI.BuildIconBtn(toolbar.transform, "Back", "arrow_back", BtnSize, highlight: false);
            var backRt = (RectTransform)backBtn.transform;
            backRt.anchorMin = backRt.anchorMax = new Vector2(0f, 0.5f);
            backRt.pivot = new Vector2(0f, 0.5f);
            backRt.anchoredPosition = new Vector2(SidePad, 0f);
            backBtn.onClick.AddListener(OnBack);

            // 진행률 pill (중앙)
            var pill = PaletteUI.BuildSurface(toolbar.transform, "ProgressPill", p.SurfaceAlt, 999);
            var pillRt = (RectTransform)pill.transform;
            pillRt.anchorMin = pillRt.anchorMax = new Vector2(0.5f, 0.5f);
            pillRt.pivot = new Vector2(0.5f, 0.5f);
            pillRt.sizeDelta = new Vector2(160f, 66f);
            pillRt.anchoredPosition = Vector2.zero;
            _progressLabel = DesignTokens.BuildText(pill.transform, "Lbl", "0 / 0",
                PaletteTokens.FontBody, FontStyle.Bold, p.Text, TextAnchor.MiddleCenter);
            var plRt = (RectTransform)_progressLabel.transform;
            plRt.anchorMin = Vector2.zero; plRt.anchorMax = Vector2.one;
            plRt.offsetMin = Vector2.zero; plRt.offsetMax = Vector2.zero;

            // 힌트 (우측) — BuildIconBtn 의 badge 파라미터로 카운트 배지 자동 생성
            int initialHint = HintManager.Instance != null ? HintManager.Instance.hintCount : 0;
            var hintBtn = PaletteUI.BuildIconBtn(toolbar.transform, "Hint", "lightbulb", BtnSize,
                highlight: true, badge: initialHint);
            var hintRt = (RectTransform)hintBtn.transform;
            hintRt.anchorMin = hintRt.anchorMax = new Vector2(1f, 0.5f);
            hintRt.pivot = new Vector2(1f, 0.5f);
            hintRt.anchoredPosition = new Vector2(-SidePad, 0f);
            hintBtn.onClick.AddListener(OnHint);

            _hintBadge = hintBtn.transform.Find("Badge/Num").GetComponent<Text>();

            _board = FindAnyObjectByType<DiffGameBoard>(FindObjectsInactive.Include);
            if (_board != null) _board.OnProgressChanged += UpdateProgressLabel;

            var hintMgr = HintManager.Instance;
            if (hintMgr != null)
            {
                hintMgr.OnHintCountChanged += UpdateHintBadge;
                UpdateHintBadge(hintMgr.hintCount);
            }
        }

        private void UpdateProgressLabel(int found, int total)
        {
            if (_progressLabel != null) _progressLabel.text = $"{found} / {total}";
        }

        private void UpdateHintBadge(int count)
        {
            if (_hintBadge != null) _hintBadge.text = count.ToString();
        }

        private void OnBack() => SceneLoader.LoadScene(SceneLoader.LevelSelect);

        private void OnHint()
        {
            var hintMgr = HintManager.Instance;
            if (hintMgr == null) return;

            if (hintMgr.hintCount > 0)
            {
                hintMgr.UseHint();
                return;
            }

            // 0개 상태 — 리워드 광고 시청 후 +1 지급하고 바로 사용
            AdManager.Instance?.AdProvider?.ShowRewarded("Rewarded_Android", () =>
            {
                hintMgr.OnRewardedHintEarned();
                hintMgr.UseHint();
            });
        }

        private void OnDestroy()
        {
            if (_board != null) _board.OnProgressChanged -= UpdateProgressLabel;
            var hintMgr = HintManager.Instance;
            if (hintMgr != null) hintMgr.OnHintCountChanged -= UpdateHintBadge;
        }
    }
}
