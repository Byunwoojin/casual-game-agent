// TC-NP-001 ~ TC-NP-006 : 인접 짝맞춤 검출 + align + 합체 (Phase 2)
using NUnit.Framework;
using UnityEngine;
using PuzzlePals.Puzzle;

namespace PuzzlePals.Tests.EditMode
{
    public class NeighborPairingTests
    {
        private static PuzzlePiece NewPiece(int row, int col, Vector3 pos)
        {
            var go = new GameObject($"Piece_{row}_{col}");
            var p = go.AddComponent<PuzzlePiece>();
            p.Row = row;
            p.Col = col;
            p.transform.position = pos;
            return p;
        }

        // TC-NP-001: 4-방향 인접 (상하좌우) 만 true, 대각선/2칸/동일은 false
        [Test]
        public void TC_NP_001_AreCardinalNeighbors_OrthogonalOnly()
        {
            Assert.IsTrue (NeighborPairing.AreCardinalNeighbors(0, 0, 0, 1)); // 우
            Assert.IsTrue (NeighborPairing.AreCardinalNeighbors(0, 1, 0, 0)); // 좌
            Assert.IsTrue (NeighborPairing.AreCardinalNeighbors(0, 0, 1, 0)); // 하
            Assert.IsTrue (NeighborPairing.AreCardinalNeighbors(2, 3, 2, 4));
            Assert.IsFalse(NeighborPairing.AreCardinalNeighbors(0, 0, 1, 1)); // 대각선
            Assert.IsFalse(NeighborPairing.AreCardinalNeighbors(0, 0, 0, 2)); // 2칸
            Assert.IsFalse(NeighborPairing.AreCardinalNeighbors(0, 0, 0, 0)); // 동일 좌표
        }

        // TC-NP-002: 정확 거리 + 작은 오차 (tol 안) 인접은 ShouldPair=true
        [Test]
        public void TC_NP_002_ShouldPair_NearNeighbor_ReturnsTrue()
        {
            float cellW = 1f, cellH = 1f, tol = 0.3f;
            var a = NewPiece(0, 1, new Vector3(1.0f, 0f, 0f));
            var b = NewPiece(0, 0, new Vector3(0.0f, 0f, 0f));

            // 정확 위치 (delta=0)
            Assert.IsTrue(NeighborPairing.ShouldPair(a, b, cellW, cellH, tol));

            // 0.2 오차 (tol 0.3 안)
            a.transform.position = new Vector3(1.2f, 0.1f, 0f);
            Assert.IsTrue(NeighborPairing.ShouldPair(a, b, cellW, cellH, tol));

            Object.DestroyImmediate(a.gameObject);
            Object.DestroyImmediate(b.gameObject);
        }

        // TC-NP-003: tolerance 밖이면 ShouldPair=false
        [Test]
        public void TC_NP_003_ShouldPair_OutsideTolerance_ReturnsFalse()
        {
            float cellW = 1f, cellH = 1f, tol = 0.3f;
            var a = NewPiece(0, 1, new Vector3(1.5f, 0f, 0f)); // 0.5 오차
            var b = NewPiece(0, 0, new Vector3(0.0f, 0f, 0f));

            Assert.IsFalse(NeighborPairing.ShouldPair(a, b, cellW, cellH, tol));

            Object.DestroyImmediate(a.gameObject);
            Object.DestroyImmediate(b.gameObject);
        }

        // TC-NP-004: 대각선 인접은 가까이 있어도 ShouldPair=false
        [Test]
        public void TC_NP_004_ShouldPair_Diagonal_ReturnsFalse()
        {
            float cellW = 1f, cellH = 1f, tol = 0.3f;
            var a = NewPiece(1, 1, new Vector3(1f, 1f, 0f));
            var b = NewPiece(0, 0, new Vector3(0f, 0f, 0f));

            Assert.IsFalse(NeighborPairing.ShouldPair(a, b, cellW, cellH, tol));

            Object.DestroyImmediate(a.gameObject);
            Object.DestroyImmediate(b.gameObject);
        }

        // TC-NP-005: AlignAndMerge — A 가 정확 위치로 align + 두 조각이 같은 그룹
        [Test]
        public void TC_NP_005_AlignAndMerge_SinglePieces_MergesAndAligns()
        {
            float cellW = 1f, cellH = 1f;
            var a = NewPiece(0, 1, new Vector3(1.15f, 0.10f, 0f)); // 약간 어긋남
            var b = NewPiece(0, 0, new Vector3(0.0f, 0f, 0f));

            var result = NeighborPairing.AlignAndMerge(a, b, cellW, cellH);

            // A 가 정확히 (1, 0) 으로 align
            Assert.AreEqual(1f, a.transform.position.x, 0.001f);
            Assert.AreEqual(0f, a.transform.position.y, 0.001f);
            // 둘 다 같은 그룹
            Assert.IsNotNull(a.Group);
            Assert.AreSame(a.Group, b.Group);
            Assert.AreEqual(2, result.Count);

            Object.DestroyImmediate(a.gameObject);
            Object.DestroyImmediate(b.gameObject);
        }

        // TC-NP-006: A 가 그룹 멤버일 때 그룹 통째로 align (상대 위치 보존)
        [Test]
        public void TC_NP_006_AlignAndMerge_GroupA_MovesEntireGroup()
        {
            float cellW = 1f, cellH = 1f;
            // 그룹 A: a (row 0, col 1) + a2 (row 0, col 2) — 둘은 이미 정확 거리 1
            var a  = NewPiece(0, 1, new Vector3(1.15f, 0.10f, 0f));
            var a2 = NewPiece(0, 2, new Vector3(2.15f, 0.10f, 0f));
            var groupA = new PieceGroup();
            groupA.AddPiece(a);
            groupA.AddPiece(a2);

            var b = NewPiece(0, 0, new Vector3(0f, 0f, 0f));

            NeighborPairing.AlignAndMerge(a, b, cellW, cellH);

            // a → (1,0), a2 → (2,0) — delta (-0.15, -0.10) 동일 적용
            Assert.AreEqual(1f, a.transform.position.x, 0.001f);
            Assert.AreEqual(0f, a.transform.position.y, 0.001f);
            Assert.AreEqual(2f, a2.transform.position.x, 0.001f);
            Assert.AreEqual(0f, a2.transform.position.y, 0.001f);

            // 셋 모두 같은 그룹
            Assert.IsNotNull(a.Group);
            Assert.AreSame(a.Group, b.Group);
            Assert.AreSame(a.Group, a2.Group);
            Assert.AreEqual(3, a.Group.Count);

            Object.DestroyImmediate(a.gameObject);
            Object.DestroyImmediate(a2.gameObject);
            Object.DestroyImmediate(b.gameObject);
        }
    }
}
