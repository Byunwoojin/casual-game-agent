using DG.Tweening;
using UnityEngine;

namespace PuzzlePals.Puzzle
{
    /// <summary>조각이 맞춰져야 할 목표 위치를 표시하는 스냅존</summary>
    public class SnapZone : MonoBehaviour
    {
        public bool IsOccupied { get; set; } = false;

        private SpriteRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            SetSilhouetteVisual();
        }

        public void SetSilhouetteVisual()
        {
            if (_renderer == null) return;
            _renderer.color = new Color(1f, 1f, 1f, 0.25f); // 반투명 실루엣
        }

        public void SetOccupied(bool occupied)
        {
            IsOccupied = occupied;
            if (_renderer != null)
                _renderer.color = occupied
                    ? new Color(1f, 1f, 1f, 0f)    // 완전 투명 (조각으로 덮임)
                    : new Color(1f, 1f, 1f, 0.25f);
        }

        // 힌트 사용 시 해당 SnapZone 위치에 노란 glow ring 표시 (성인 피벗 v0.4)
        // SnapZone GO 자체엔 SpriteRenderer 가 없을 수 있으므로 임시 child glow GO 를 생성·트윈·제거
        public void Flash()
        {
            if (!Application.isPlaying || IsOccupied) return;

            var glowGO = new GameObject("HintGlow");
            glowGO.transform.SetParent(transform, worldPositionStays: false);
            glowGO.transform.localPosition = Vector3.zero;
            glowGO.transform.localScale    = Vector3.one * 0.6f;

            var sr = glowGO.AddComponent<SpriteRenderer>();
            var tex = Texture2D.whiteTexture;
            sr.sprite       = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                            new Vector2(0.5f, 0.5f), tex.width);
            sr.color        = new Color(1f, 0.85f, 0.25f, 0.7f); // 노란 글로우
            sr.sortingOrder = 50; // 보드 배경(-10) + 조각(10) 위에

            // 2회 yoyo: scale 0.6→1.4 + alpha 0.7→0 (DOTween UI 모듈 회피 — manual setter)
            var seq = DOTween.Sequence();
            seq.Append(glowGO.transform.DOScale(1.4f, 0.45f).SetEase(Ease.OutQuad));
            seq.Join(DOTween.To(() => sr.color.a,
                                v => { var c = sr.color; c.a = v; sr.color = c; },
                                0f, 0.45f));
            seq.OnComplete(() => { if (glowGO != null) Destroy(glowGO); });
        }
    }
}
