// TC-LC-001 ~ TC-LC-004 : 레벨 클리어 판정
using NUnit.Framework;
using UnityEngine;
using PuzzlePals.Core;
using PuzzlePals.Puzzle;
using PuzzlePals.Systems;

namespace PuzzlePals.Tests.EditMode
{
    public class LevelCompleteTests
    {
        private GameObject _checkerGO;
        private PuzzleCompleteChecker _checker;
        private PuzzlePiece[] _pieces;
        private GameObject[] _pieceGOs;

        [SetUp]
        public void SetUp()
        {
            _checkerGO = new GameObject("Checker");
            _checker = _checkerGO.AddComponent<PuzzleCompleteChecker>();

            // 3조각 생성
            _pieceGOs = new GameObject[3];
            _pieces = new PuzzlePiece[3];
            for (int i = 0; i < 3; i++)
            {
                _pieceGOs[i] = new GameObject($"Piece_{i}");
                _pieces[i] = _pieceGOs[i].AddComponent<PuzzlePiece>();
            }
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_checkerGO);
            foreach (var go in _pieceGOs)
                Object.DestroyImmediate(go);

            // 싱글톤 정리
            if (GameManager.Instance != null)
                Object.DestroyImmediate(GameManager.Instance.gameObject);
        }

        // TC-LC-001: 모든 조각이 스냅되면 OnPuzzleComplete 호출됨
        [Test]
        public void TC_LC_001_AllPiecesSnapped_OnPuzzleCompleteInvoked()
        {
            foreach (var p in _pieces)
                p.SetSnappedForTest(true);

            bool completeCalled = false;
            _checker.OnPuzzleComplete += () => completeCalled = true;

            _checker.Check(_pieces);

            Assert.IsTrue(completeCalled, "OnPuzzleComplete should be invoked when all pieces are snapped");
        }

        // TC-LC-002: 조각 1개라도 스냅 안 되면 OnPuzzleComplete 호출되지 않음
        [Test]
        public void TC_LC_002_OnePieceNotSnapped_OnPuzzleCompleteNotInvoked()
        {
            _pieces[0].SetSnappedForTest(true);
            _pieces[1].SetSnappedForTest(true);
            _pieces[2].SetSnappedForTest(false); // 미스냅

            bool completeCalled = false;
            _checker.OnPuzzleComplete += () => completeCalled = true;

            _checker.Check(_pieces);

            Assert.IsFalse(completeCalled, "OnPuzzleComplete should NOT be invoked when any piece is unsnapped");
        }

        // TC-LC-003: 클리어 시 GameManager.State == GameState.Complete
        [Test]
        public void TC_LC_003_OnPuzzleComplete_GameStateChangesToComplete()
        {
            // GameManager 생성 및 초기 Playing 상태 설정
            var gmGO = new GameObject("GameManager");
            var gm = gmGO.AddComponent<GameManager>();
            gm.State = GameState.Playing;

            // checker와 GameManager를 연결
            _checker.OnPuzzleComplete += gm.OnPuzzleComplete;

            foreach (var p in _pieces)
                p.SetSnappedForTest(true);

            _checker.Check(_pieces);

            Assert.AreEqual(GameState.Complete, gm.State,
                "GameManager state should be Complete after puzzle complete");
        }

        // TC-LC-004: 클리어 시 AdManager.OnLevelComplete()가 호출됨
        [Test]
        public void TC_LC_004_OnPuzzleComplete_AdManagerOnLevelCompleteInvoked()
        {
            var adGO = new GameObject("AdManager");
            var adManager = adGO.AddComponent<AdManager>();
            adManager.RoundsSinceAd = 4; // 다음 클리어 시 광고 트리거 조건

            bool adOnLevelCompleteCalled = false;
            adManager.OnLevelCompleteCallback = () => adOnLevelCompleteCalled = true;

            _checker.OnPuzzleComplete += adManager.OnLevelComplete;

            foreach (var p in _pieces)
                p.SetSnappedForTest(true);

            _checker.Check(_pieces);

            Assert.IsTrue(adOnLevelCompleteCalled,
                "AdManager.OnLevelComplete() should be called when puzzle is complete");

            Object.DestroyImmediate(adGO);
        }
    }
}
