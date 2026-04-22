// TC-SL-001 ~ TC-SL-005 : 조각 스냅 판정 로직
using NUnit.Framework;
using UnityEngine;
using PuzzlePals.Puzzle;

namespace PuzzlePals.Tests.EditMode
{
    public class SnapLogicTests
    {
        private GameObject _pieceGO;
        private GameObject _zoneGO;
        private PuzzlePiece _piece;
        private SnapZone _zone;

        [SetUp]
        public void SetUp()
        {
            _pieceGO = new GameObject("PuzzlePiece");
            _piece = _pieceGO.AddComponent<PuzzlePiece>();

            _zoneGO = new GameObject("SnapZone");
            _zone = _zoneGO.AddComponent<SnapZone>();
            _zone.transform.position = Vector3.zero;
            _zone.IsOccupied = false;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_pieceGO);
            Object.DestroyImmediate(_zoneGO);
        }

        // TC-SL-001: 임계값 안(1.5f < 1.8f)이면 스냅 성공
        [Test]
        public void TC_SL_001_PieceWithinThreshold_SnapsSuccessfully()
        {
            _piece.transform.position = new Vector3(1.5f, 0f, 0f);
            _piece.OriginalPosition = _piece.transform.position;

            bool snapped = _piece.TrySnap(_zone, snapThreshold: 1.8f);

            Assert.IsTrue(snapped, "Should snap when distance (1.5f) < threshold (1.8f)");
            Assert.IsTrue(_piece.IsSnapped);
        }

        // TC-SL-002: 임계값 밖(2.0f > 1.8f)이면 스냅 실패
        [Test]
        public void TC_SL_002_PieceOutsideThreshold_DoesNotSnap()
        {
            _piece.transform.position = new Vector3(2.0f, 0f, 0f);
            _piece.OriginalPosition = _piece.transform.position;

            bool snapped = _piece.TrySnap(_zone, snapThreshold: 1.8f);

            Assert.IsFalse(snapped, "Should not snap when distance (2.0f) > threshold (1.8f)");
            Assert.IsFalse(_piece.IsSnapped);
        }

        // TC-SL-003: 스냅 실패 시 조각은 놓은 자리에 그대로 머무른다 (성인 피벗 v0.4 — 기존 "원위치 복귀" 폐기)
        [Test]
        public void TC_SL_003_SnapFail_PieceStaysWhereDropped()
        {
            var originalPos = new Vector3(5f, 5f, 0f);
            var droppedPos  = new Vector3(2.5f, 0f, 0f); // threshold 밖
            _piece.OriginalPosition = originalPos;
            _piece.transform.position = droppedPos;

            _piece.TrySnap(_zone, snapThreshold: 1.8f);

            Assert.AreEqual(droppedPos.x, _piece.transform.position.x, 0.001f,
                "snap 실패 시 조각은 놓은 자리에 그대로 머물러야 함");
            Assert.AreEqual(droppedPos.y, _piece.transform.position.y, 0.001f);
        }

        // TC-SL-004: 이미 점유된 스냅존에는 스냅되지 않는다
        [Test]
        public void TC_SL_004_OccupiedSnapZone_DoesNotSnap()
        {
            _zone.IsOccupied = true;
            _piece.transform.position = new Vector3(0.5f, 0f, 0f); // 충분히 가까움
            _piece.OriginalPosition = _piece.transform.position;

            bool snapped = _piece.TrySnap(_zone, snapThreshold: 1.8f);

            Assert.IsFalse(snapped, "Should not snap to already occupied snap zone");
        }

        // TC-SL-005: 스냅 성공 시 조각 position == snapZone position (오차 0.01f 이내)
        [Test]
        public void TC_SL_005_SnapSuccess_PiecePositionMatchesZonePosition()
        {
            _zone.transform.position = new Vector3(3f, 2f, 0f);
            _piece.transform.position = new Vector3(3f + 1.0f, 2f, 0f); // 1.0f 거리
            _piece.OriginalPosition = _piece.transform.position;

            _piece.TrySnap(_zone, snapThreshold: 1.8f);

            Assert.AreEqual(_zone.transform.position.x, _piece.transform.position.x, 0.01f);
            Assert.AreEqual(_zone.transform.position.y, _piece.transform.position.y, 0.01f);
        }
    }
}
