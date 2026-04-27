using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using PuzzlePals.Puzzle;
using PuzzlePals.Systems;
using PuzzlePals.UI;

namespace PuzzlePals.Gameplay
{
    /// <summary>
    /// 성인 피벗 v0.4 Step 4-B — 중간 진행 축하.
    /// L2+ 에서 50% / 75% / 가장자리(edge) 전체 스냅 시점에 토스트 + 파티클 + 효과음.
    /// L1 은 첫 인상 보호 차원에서 모든 트리거 스킵 (광고도 동일 정책).
    /// 각 트리거는 한 레벨 당 한 번만 발사. Init 호출 시 리셋되므로 idempotent.
    /// </summary>
    public class ProgressCelebrationController : MonoBehaviour
    {
        private PuzzleBoard _board;
        private int  _levelNumber;
        private bool _enabled;

        private bool _fired50;
        private bool _fired75;
        private bool _firedEdges;

        private Canvas _toastCanvas;

        /// <summary>GameplayLoader.Build 끝부분에서 호출. 레벨 바뀔 때마다 트리거 리셋.</summary>
        public void Init(PuzzleBoard board, int levelNumber)
        {
            _board       = board;
            _levelNumber = levelNumber;
            _enabled     = (board != null && levelNumber >= 2);

            _fired50 = _fired75 = _firedEdges = false;
        }

        private void Update()
        {
            if (!_enabled || _board == null) return;
            int total = _board.Pieces.Count;
            if (total == 0) return;

            int rows = _board.Rows;
            int cols = _board.Cols;

            int snapped     = 0;
            int edgeTotal   = 0;
            int edgeSnapped = 0;

            for (int i = 0; i < total; i++)
            {
                var p = _board.Pieces[i];
                if (p == null) continue;
                bool isEdge = (p.Row == 0 || p.Row == rows - 1 || p.Col == 0 || p.Col == cols - 1);
                if (isEdge) edgeTotal++;
                if (p.IsSnapped)
                {
                    snapped++;
                    if (isEdge) edgeSnapped++;
                }
            }

            // 100% 완성은 GameplaySceneController 가 별도로 처리 — 여기선 중간 단계만
            if (snapped >= total) return;

            float ratio = (float)snapped / total;

            if (!_fired50 && ratio >= 0.5f)
            {
                _fired50 = true;
                Celebrate("progress_50", "50% 완성!");
            }
            else if (!_fired75 && ratio >= 0.75f)
            {
                _fired75 = true;
                Celebrate("progress_75", "75% 완성!");
            }

            if (!_firedEdges && edgeTotal > 0 && edgeSnapped == edgeTotal)
            {
                _firedEdges = true;
                Celebrate("progress_edges", "가장자리 완성!");
            }
        }

        private void Celebrate(string entryKey, string fallback)
        {
            ShowToast(entryKey, fallback);
            VFXManager.Instance?.PlayCompleteBurst(Vector3.zero);
            AudioManager.Instance?.PlayDing();
        }

        // ── 토스트 UI ────────────────────────────────────────

        private void ShowToast(string entryKey, string fallback)
        {
            EnsureToastCanvas();
            if (_toastCanvas == null) return;

            var toastGo = new GameObject("Toast",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            toastGo.transform.SetParent(_toastCanvas.transform, worldPositionStays: false);

            var rt = (RectTransform)toastGo.transform;
            rt.anchorMin = new Vector2(0.5f, 0.62f);
            rt.anchorMax = new Vector2(0.5f, 0.62f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(660f, 160f);
            rt.anchoredPosition = Vector2.zero;

            // DesignTokens 톤 통일: 흰 surface + 어두운 텍스트 + 옅은 그림자 보더
            var img = toastGo.GetComponent<Image>();
            img.color         = DesignTokens.SurfaceCard;
            img.raycastTarget = false;

            // 외곽 옅은 그림자 (다른 화면의 캡슐 패턴과 동일)
            var shadow = new GameObject("Shadow",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            shadow.transform.SetParent(toastGo.transform, worldPositionStays: false);
            shadow.transform.SetAsFirstSibling();
            var srt = (RectTransform)shadow.transform;
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = Vector2.one;
            srt.offsetMin = new Vector2(-6f, -6f);
            srt.offsetMax = new Vector2( 6f,  6f);
            var shImg = shadow.GetComponent<Image>();
            shImg.color         = new Color(0f, 0f, 0f, 0.10f);
            shImg.raycastTarget = false;

            // 라벨 — DesignTokens 폰트/색상, Localized
            var lbl = DesignTokens.BuildLocalizedText(toastGo.transform, "Label", entryKey, fallback,
                52, FontStyle.Bold, DesignTokens.TextPrimary);
            var lrt = (RectTransform)lbl.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            // scale 0.5 → 1 (punch) + fade in → 1.0s 유지 → fade out → destroy
            // (DOTween UI 모듈 미사용 → CanvasGroup.DOFade 대신 DOTween.To + 수동 setter)
            var cg = toastGo.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            toastGo.transform.localScale = Vector3.one * 0.5f;

            var seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => cg.alpha, v => cg.alpha = v, 1f, 0.2f));
            seq.Join(toastGo.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
            seq.AppendInterval(1.0f);
            seq.Append(DOTween.To(() => cg.alpha, v => cg.alpha = v, 0f, 0.35f));
            seq.OnComplete(() => { if (toastGo != null) Destroy(toastGo); });
        }

        private void EnsureToastCanvas()
        {
            if (_toastCanvas != null) return;

            var canvasGo = new GameObject("ToastCanvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200; // 툴바/CompleteUI 위에 렌더

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0f;

            _toastCanvas = canvas;
        }
    }
}
