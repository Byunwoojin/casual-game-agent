using System.Collections;
using DG.Tweening;
using UnityEngine;
using PuzzlePals.Puzzle;

namespace PuzzlePals.Gameplay
{
    /// <summary>
    /// 퍼즐 완성 시 모든 조각이 함께 점프 2회 + 흔들기 애니메이션.
    /// GDD §Game Feel "눈 깜빡 → 점프 2회 → 손 흔들기" 중 sprite 분할 가능한 진짜 아트가 도착하기 전까지의 대체 효과.
    /// (눈/손 별도 sprite는 캐릭터 아트 작업에 포함되어야 구현 가능)
    /// </summary>
    public class CharacterCompleteAnimation : MonoBehaviour
    {
        public static CharacterCompleteAnimation Play(PuzzlePiece[] pieces, System.Action onComplete = null)
        {
            var go = new GameObject("CharacterCompleteAnimation");
            var anim = go.AddComponent<CharacterCompleteAnimation>();
            anim.StartCoroutine(anim.Routine(pieces, onComplete));
            return anim;
        }

        private IEnumerator Routine(PuzzlePiece[] pieces, System.Action onComplete)
        {
            if (pieces == null || pieces.Length == 0)
            {
                onComplete?.Invoke();
                Destroy(gameObject);
                yield break;
            }

            // 0.2s 대기 (스냅 직후 잠시 정지)
            yield return new WaitForSeconds(0.2f);

            // 점프 2회 — 모든 조각 동시에
            foreach (var p in pieces)
            {
                if (p == null) continue;
                var t = p.transform;
                t.DOKill();
                Vector3 home = t.position;

                var seq = DOTween.Sequence();
                seq.Append(t.DOJump(home, jumpPower: 0.5f, numJumps: 2, duration: 0.8f).SetEase(Ease.OutQuad));
                // 점프 후 손 흔들기 대체 — PunchScale + 좌우 회전
                seq.Append(t.DOPunchScale(Vector3.one * 0.2f, 0.4f, vibrato: 4, elasticity: 0.6f));
                seq.Join(t.DOPunchRotation(new Vector3(0f, 0f, 12f), 0.4f, vibrato: 6, elasticity: 0.5f));
            }

            // 점프 0.8s + 흔들기 0.4s = 1.2s 대기 + 여유
            yield return new WaitForSeconds(1.4f);

            onComplete?.Invoke();
            Destroy(gameObject);
        }
    }
}
