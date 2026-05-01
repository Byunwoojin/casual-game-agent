// TC-PG-001 ~ TC-PG-005 : 인접 짝맞춤 그룹 인프라 (Phase 1)
using NUnit.Framework;
using UnityEngine;
using PuzzlePals.Puzzle;

namespace PuzzlePals.Tests.EditMode
{
    public class PieceGroupTests
    {
        private static PuzzlePiece NewPiece(string name, Vector3 pos)
        {
            var go = new GameObject(name);
            var p = go.AddComponent<PuzzlePiece>();
            p.transform.position = pos;
            return p;
        }

        // TC-PG-001: AddPiece 시 piece.Group 이 그룹을 참조한다
        [Test]
        public void TC_PG_001_AddPiece_AssignsGroupReference()
        {
            var p = NewPiece("A", Vector3.zero);
            var g = new PieceGroup();

            g.AddPiece(p);

            Assert.AreSame(g, p.Group);
            Assert.AreEqual(1, g.Count);

            Object.DestroyImmediate(p.gameObject);
        }

        // TC-PG-002: 동일 piece 중복 Add 는 무시
        [Test]
        public void TC_PG_002_AddPiece_DoesNotDuplicate()
        {
            var p = NewPiece("A", Vector3.zero);
            var g = new PieceGroup();

            g.AddPiece(p);
            g.AddPiece(p);

            Assert.AreEqual(1, g.Count);

            Object.DestroyImmediate(p.gameObject);
        }

        // TC-PG-003: MoveBy 는 모든 멤버를 동일 delta 로 이동 — 상대 위치 보존
        [Test]
        public void TC_PG_003_MoveBy_MovesAllMembersUniformly()
        {
            var a = NewPiece("A", new Vector3(0f, 0f, 0f));
            var b = NewPiece("B", new Vector3(1f, 0f, 0f));
            var g = new PieceGroup();
            g.AddPiece(a);
            g.AddPiece(b);

            g.MoveBy(new Vector3(2f, 3f, 0f));

            Assert.AreEqual(new Vector3(2f, 3f, 0f), a.transform.position);
            Assert.AreEqual(new Vector3(3f, 3f, 0f), b.transform.position);

            Object.DestroyImmediate(a.gameObject);
            Object.DestroyImmediate(b.gameObject);
        }

        // TC-PG-004: MergeFrom 은 다른 그룹의 멤버를 흡수하고 원본을 비운다
        [Test]
        public void TC_PG_004_MergeFrom_AbsorbsMembersAndClearsSource()
        {
            var a = NewPiece("A", Vector3.zero);
            var b = NewPiece("B", Vector3.zero);
            var ga = new PieceGroup();
            var gb = new PieceGroup();
            ga.AddPiece(a);
            gb.AddPiece(b);

            ga.MergeFrom(gb);

            Assert.AreEqual(2, ga.Count);
            Assert.AreEqual(0, gb.Count);
            Assert.AreSame(ga, a.Group);
            Assert.AreSame(ga, b.Group);

            Object.DestroyImmediate(a.gameObject);
            Object.DestroyImmediate(b.gameObject);
        }

        // TC-PG-005: RemovePiece 는 멤버를 빼고 piece.Group 을 null 로 만든다
        [Test]
        public void TC_PG_005_RemovePiece_ClearsReference()
        {
            var p = NewPiece("A", Vector3.zero);
            var g = new PieceGroup();
            g.AddPiece(p);

            g.RemovePiece(p);

            Assert.IsNull(p.Group);
            Assert.AreEqual(0, g.Count);

            Object.DestroyImmediate(p.gameObject);
        }

        // TC-PG-006: RealignToLeader — 멤버가 어긋나 있어도 leader 기준 grid 절대 위치로 정정
        [Test]
        public void TC_PG_006_RealignToLeader_CorrectsDriftedMembers()
        {
            var leader = NewPiece("L", new Vector3(5f, 7f, 0f));
            leader.Row = 0; leader.Col = 1;
            var member = NewPiece("M", new Vector3(6.05f, 7.03f, 0f)); // 누적 오차로 살짝 어긋남
            member.Row = 0; member.Col = 2;

            var g = new PieceGroup();
            g.AddPiece(leader);
            g.AddPiece(member);

            g.RealignToLeader(leader, cellW: 1f, cellH: 1f);

            // leader 는 그대로, member 는 leader + (cellW, 0) = (6, 7) 정확
            Assert.AreEqual(5f, leader.transform.position.x, 0.0001f);
            Assert.AreEqual(7f, leader.transform.position.y, 0.0001f);
            Assert.AreEqual(6f, member.transform.position.x, 0.0001f);
            Assert.AreEqual(7f, member.transform.position.y, 0.0001f);

            Object.DestroyImmediate(leader.gameObject);
            Object.DestroyImmediate(member.gameObject);
        }
    }
}
