using System.Collections.Generic;
using UnityEngine;

namespace PuzzlePals.Puzzle
{
    /// <summary>
    /// 인접 짝맞춤으로 합체된 조각들의 묶음. 그룹 단위로 같이 이동/스냅된다.
    /// 회전 X (직소는 회전 없음). MonoBehaviour 가 아니라 단순 데이터 컨테이너 — 멤버 PuzzlePiece 의 lifetime 에 매여 산다.
    ///
    /// Phase 1 (현재): 인프라 + 그룹 드래그.
    /// Phase 2: PuzzlePiece.OnPointerUp 에서 인접 조각 검출 → MergeFrom 호출.
    /// Phase 3: SnapZone 흡수 시 그룹 모든 멤버 IsSnapped.
    /// </summary>
    public class PieceGroup
    {
        private readonly List<PuzzlePiece> _members = new List<PuzzlePiece>();

        public IReadOnlyList<PuzzlePiece> Members => _members;
        public int Count => _members.Count;

        // 그룹 내 멤버 간 grid 정렬용. NeighborPairing.AlignAndMerge 시점에 PuzzleBoard 가
        // 결정한 cellW/cellH 가 주입된다. drag/스냅 시 Board reference 없이 직접 사용.
        public float CellW { get; private set; }
        public float CellH { get; private set; }

        public void SetCellSize(float w, float h)
        {
            CellW = w;
            CellH = h;
        }

        public void AddPiece(PuzzlePiece p)
        {
            if (p == null || _members.Contains(p)) return;
            _members.Add(p);
            p.Group = this;
        }

        public void RemovePiece(PuzzlePiece p)
        {
            if (p == null) return;
            _members.Remove(p);
            if (p.Group == this) p.Group = null;
        }

        /// <summary>other 의 모든 멤버를 this 로 흡수하고 other 를 비운다. Phase 2 합체 진입점.</summary>
        public void MergeFrom(PieceGroup other)
        {
            if (other == null || other == this) return;
            var snapshot = new List<PuzzlePiece>(other._members);
            other._members.Clear();
            foreach (var p in snapshot)
            {
                if (p == null) continue;
                if (!_members.Contains(p)) _members.Add(p);
                p.Group = this;
            }
        }

        /// <summary>모든 멤버를 delta 만큼 동일 이동 (translation).</summary>
        public void MoveBy(Vector3 delta)
        {
            for (int i = 0; i < _members.Count; i++)
            {
                var m = _members[i];
                if (m == null) continue;
                m.transform.position += delta;
            }
        }

        /// <summary>
        /// leader 의 현재 위치 기준으로 모든 멤버를 grid 절대 위치로 재정렬 — 누적 오차 제거.
        /// drag 중 매 frame 호출하면 멤버 사이 빈 틈이 누적되지 않는다.
        /// </summary>
        public void RealignToLeader(PuzzlePiece leader, float cellW, float cellH)
        {
            if (leader == null) return;
            Vector3 leaderPos = leader.transform.position;
            int leaderRow = leader.Row;
            int leaderCol = leader.Col;
            for (int i = 0; i < _members.Count; i++)
            {
                var m = _members[i];
                if (m == null || m == leader) continue;
                m.transform.position = leaderPos + new Vector3(
                    (m.Col - leaderCol) * cellW,
                    (m.Row - leaderRow) * cellH,
                    0f);
            }
        }

        /// <summary>모든 멤버의 sortingOrder 일괄 설정 — 드래그 중 다른 조각 위로 띄울 때 사용.</summary>
        public void SetSortingOrder(int order)
        {
            for (int i = 0; i < _members.Count; i++)
            {
                var m = _members[i];
                if (m == null) continue;
                m.SetSortingOrder(order);
            }
        }

    }
}
