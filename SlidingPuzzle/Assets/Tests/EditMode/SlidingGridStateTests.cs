using System.Collections.Generic;
using NUnit.Framework;
using SlidingPuzzle.Gameplay;

namespace SlidingPuzzle.Tests.EditMode
{
    /// <summary>SlidingGridState — 셔플/인접판정/완료판정/solvability 순수 로직 테스트.</summary>
    public class SlidingGridStateTests
    {
        // TC-SG-001: 초기 상태(3×3)는 이미 정렬된 상태
        [Test]
        public void TC_SG_001_InitialState_IsSolved()
        {
            var s = new SlidingGridState(3);
            Assert.IsTrue(s.IsSolved());
            Assert.AreEqual(8, s.MovableCount);
            Assert.AreEqual(8, s.BlankSlot); // 마지막 칸(9칸 중 인덱스 8)이 빈 칸
        }

        // TC-SG-002: 인접 판정 — 3×3 에서 빈 칸(8)의 인접 슬롯은 5(위), 7(왼쪽) 뿐
        [Test]
        public void TC_SG_002_IsAdjacentToBlank_CorrectForNeighbors()
        {
            var s = new SlidingGridState(3);
            Assert.IsTrue(s.IsAdjacentToBlank(5));
            Assert.IsTrue(s.IsAdjacentToBlank(7));
            Assert.IsFalse(s.IsAdjacentToBlank(4));  // 대각선 방향 — 인접 아님
            Assert.IsFalse(s.IsAdjacentToBlank(0));
        }

        // TC-SG-003: 빈 칸과 스왑하면 조각/슬롯 매핑이 갱신되고, 정위치 카운트가 그만큼 줄어듦
        [Test]
        public void TC_SG_003_SwapBlankWith_UpdatesMappings()
        {
            var s = new SlidingGridState(3);
            Assert.AreEqual(8, s.CountCorrect());

            s.SwapBlankWith(7); // piece 7 이 있던 자리 <-> 빈 칸
            Assert.AreEqual(7, s.BlankSlot);
            Assert.AreEqual(8, s.SlotOfPiece(7));
            Assert.AreEqual(-1, s.PieceAtSlot(7)); // 이제 7번 슬롯이 빈 칸
            Assert.AreEqual(7, s.CountCorrect());  // piece 7 만 정위치를 벗어남
            Assert.IsFalse(s.IsSolved());
        }

        // TC-SG-004: 셔플 후에도 모든 조각(0..MovableCount-1)이 정확히 한 번씩만 존재 (유효한 순열)
        [Test]
        public void TC_SG_004_Shuffle_ProducesValidPermutation()
        {
            var s = new SlidingGridState(4);
            s.Shuffle(new System.Random(42));

            var seen = new HashSet<int>();
            for (int piece = 0; piece < s.MovableCount; piece++)
            {
                int slot = s.SlotOfPiece(piece);
                Assert.AreEqual(piece, s.PieceAtSlot(slot), "SlotOfPiece/PieceAtSlot 역함수 관계가 깨짐");
                Assert.IsTrue(seen.Add(slot), $"슬롯 {slot} 이 두 조각에 중복 배정됨");
            }
            Assert.AreEqual(s.MovableCount, seen.Count);
        }

        // TC-SG-005: 셔플 결과는 항상 풀 수 있는 상태 (여러 그리드 크기 × 여러 시드)
        [Test]
        public void TC_SG_005_Shuffle_ResultIsAlwaysSolvable(
            [Values(3, 4, 5)] int gridSize,
            [Values(1, 2, 3, 4, 5)] int seed)
        {
            var s = new SlidingGridState(gridSize);
            s.Shuffle(new System.Random(seed));
            Assert.IsTrue(s.IsSolvable(),
                $"gridSize={gridSize}, seed={seed} 셔플 결과가 이론상 풀 수 없는 상태로 나옴");
        }

        // TC-SG-006: 충분한 이동 횟수로 셔플하면 실제로 흐트러짐 (제자리 그대로가 아님)
        [Test]
        public void TC_SG_006_Shuffle_ActuallyChangesState()
        {
            var s = new SlidingGridState(3);
            s.Shuffle(new System.Random(1));
            Assert.Less(s.CountCorrect(), s.MovableCount, "셔플 후에도 정렬 상태 그대로임");
        }

        // TC-SG-007: solvability 공식 자체 검증 — 정렬 상태에서 두 조각만 맞바꾼(단일 전치) 배치는
        // 수학적으로 항상 풀 수 없음. 셔플 메커니즘과 무관하게 IsSolvable() 공식이 맞는지 확인.
        [Test]
        public void TC_SG_007_IsSolvable_DetectsSingleTransposition_AsUnsolvable()
        {
            var solved = new SlidingGridState(3, new[] { 0, 1, 2, 3, 4, 5, 6, 7, -1 });
            Assert.IsTrue(solved.IsSolvable());

            var oneSwap = new SlidingGridState(3, new[] { 1, 0, 2, 3, 4, 5, 6, 7, -1 });
            Assert.IsFalse(oneSwap.IsSolvable(), "단일 전치는 항상 풀 수 없는 상태여야 함");
        }

        // TC-SG-008: 힌트 후보 탐색 로직이 의존하는 CountCorrect — 정위치 판정 자체의 기본 케이스
        [Test]
        public void TC_SG_008_CountCorrect_ZeroWhenAllDisplaced()
        {
            // 3×3 에서 0번 자리에 1, 1번 자리에 2 ... 순환 이동 + 빈 칸은 그대로 8
            var s = new SlidingGridState(3, new[] { 7, 0, 1, 2, 3, 4, 5, 6, -1 });
            Assert.AreEqual(0, s.CountCorrect());
        }
    }
}
