using UnityEngine;

namespace PuzzlePals.Puzzle
{
    /// <summary>
    /// 인접 짝맞춤 (Phase 2) — 4-방향 인접 (Row±1 또는 Col±1, 대각선 제외) 검출 + 정확 align + 그룹 합체.
    /// 호출 진입점: PuzzleBoard.TryNeighborPair → 4-방향 후보 piece 마다 ShouldPair → AlignAndMerge.
    /// 효과음/VFX 는 Phase 3 에서 추가.
    /// </summary>
    public static class NeighborPairing
    {
        /// <summary>대각선 제외, 정확히 한 칸 떨어진 4-방향 인접만 true.</summary>
        public static bool AreCardinalNeighbors(int rA, int cA, int rB, int cB)
        {
            int dr = Mathf.Abs(rA - rB);
            int dc = Mathf.Abs(cA - cB);
            return (dr == 1 && dc == 0) || (dr == 0 && dc == 1);
        }

        /// <summary>B 기준으로 A 가 있어야 할 정확 위치 — grid 차이 × 셀 크기.</summary>
        public static Vector3 ComputeExpectedPosition(PuzzlePiece a, PuzzlePiece b, float cellW, float cellH)
        {
            return b.transform.position + new Vector3(
                (a.Col - b.Col) * cellW,
                (a.Row - b.Row) * cellH,
                0f);
        }

        /// <summary>
        /// 4-방향 인접 + A 가 expected 위치 tolerance 안 + 둘 다 보드 위 (스냅/뱅크 X) + 동일 그룹 X
        /// 일 때만 합체 가능.
        /// </summary>
        public static bool ShouldPair(PuzzlePiece a, PuzzlePiece b, float cellW, float cellH, float tolerance)
        {
            if (a == null || b == null || a == b) return false;
            // 정답 SnapZone 안착 piece 와의 합체는 Phase 3 (그룹의 SnapZone 흡수) 영역
            if (a.IsSnapped || b.IsSnapped) return false;
            // 뱅크 안 piece 와는 합체 X — 사용자가 보드 위에 둘 다 꺼낸 상태에서만
            if (a.IsInBank || b.IsInBank) return false;
            // 이미 같은 그룹이면 합체 불필요
            if (a.Group != null && a.Group == b.Group) return false;
            if (!AreCardinalNeighbors(a.Row, a.Col, b.Row, b.Col)) return false;
            Vector3 expected = ComputeExpectedPosition(a, b, cellW, cellH);
            return Vector3.Distance(a.transform.position, expected) <= tolerance;
        }

        /// <summary>
        /// A 의 그룹(없으면 새로 생성)을 expected 위치로 align 한 다음 B 의 그룹과 합체.
        /// 큰 그룹이 작은 그룹을 흡수. 반환: 결과 그룹.
        /// </summary>
        public static PieceGroup AlignAndMerge(PuzzlePiece a, PuzzlePiece b, float cellW, float cellH)
        {
            var groupA = a.Group;
            if (groupA == null)
            {
                groupA = new PieceGroup();
                groupA.AddPiece(a);
            }
            var groupB = b.Group;
            if (groupB == null)
            {
                groupB = new PieceGroup();
                groupB.AddPiece(b);
            }

            // A 그룹을 정확 위치로 이동 — A 가 expected 위치에 가게 (그룹 멤버는 상대 위치 보존)
            Vector3 expected   = ComputeExpectedPosition(a, b, cellW, cellH);
            Vector3 alignDelta = expected - a.transform.position;
            if (alignDelta.sqrMagnitude > 0f) groupA.MoveBy(alignDelta);

            // 큰 그룹이 작은 그룹 흡수 (동률이면 B 가 흡수자)
            PieceGroup result;
            if (groupB.Count >= groupA.Count)
            {
                groupB.MergeFrom(groupA);
                result = groupB;
            }
            else
            {
                groupA.MergeFrom(groupB);
                result = groupA;
            }

            // 결과 그룹에 cellSize 주입 — drag/스냅 시 Board reference 없이도 RealignToLeader 가능
            result.SetCellSize(cellW, cellH);
            // 합체 직후 한번 더 grid 절대 정렬 — 작은 누적/반올림 오차 제거
            result.RealignToLeader(b, cellW, cellH);
            return result;
        }
    }
}
