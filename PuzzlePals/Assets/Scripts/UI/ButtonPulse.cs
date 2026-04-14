using DG.Tweening;
using UnityEngine;

namespace PuzzlePals.UI
{
    /// <summary>
    /// 일정 주기로 PunchScale 어필 — MainMenu Play 버튼 같은 호출용 컴포넌트.
    /// GDD §Game Feel: `DOPunchScale(Vector3.one * 0.15f, 0.3f)` (641줄).
    /// </summary>
    [DisallowMultipleComponent]
    public class ButtonPulse : MonoBehaviour
    {
        [SerializeField] private float interval   = 2.5f;
        [SerializeField] private float punchSize  = 0.15f;
        [SerializeField] private float punchTime  = 0.3f;
        [SerializeField] private int   vibrato    = 4;
        [SerializeField] private float elasticity = 0.6f;

        private Sequence _loop;

        private void OnEnable()
        {
            _loop = DOTween.Sequence();
            _loop.AppendInterval(interval);
            _loop.Append(transform.DOPunchScale(
                Vector3.one * punchSize, punchTime, vibrato, elasticity));
            _loop.SetLoops(-1);
            _loop.SetTarget(transform);
        }

        private void OnDisable()
        {
            _loop?.Kill();
            _loop = null;
            transform.DOKill();
        }
    }
}
