using System.Collections.Generic;
using NUnit.Framework;
using MemoryMatch.Gameplay;

namespace MemoryMatch.Tests.EditMode
{
    /// <summary>MemoryMatchState — 셔플/뒤집기/매치판정/힌트 순수 로직 테스트.</summary>
    public class MemoryMatchStateTests
    {
        // TC-MM-001: 초기 상태 — 매치 0, 미완료, 카드 수 = pairCount*2
        [Test]
        public void TC_MM_001_InitialState_NotComplete()
        {
            var s = new MemoryMatchState(4, new System.Random(1));
            Assert.AreEqual(0, s.MatchedPairs);
            Assert.IsFalse(s.IsComplete);
            Assert.AreEqual(8, s.CardCount);
        }

        // TC-MM-002: 셔플 후에도 각 pairId 가 정확히 두 번씩만 등장 (유효한 카드 세트)
        [Test]
        public void TC_MM_002_Shuffle_EachPairIdAppearsExactlyTwice()
        {
            var s = new MemoryMatchState(6, new System.Random(42));
            var counts = new Dictionary<int, int>();
            for (int i = 0; i < s.CardCount; i++)
            {
                int id = s.PairIdAt(i);
                counts[id] = counts.GetValueOrDefault(id) + 1;
            }
            Assert.AreEqual(6, counts.Count, "pairId 종류는 pairCount 만큼이어야 함");
            foreach (var kv in counts)
                Assert.AreEqual(2, kv.Value, $"pairId {kv.Key} 가 정확히 2번 등장해야 함");
        }

        // TC-MM-003: 첫 카드를 뒤집으면 FirstFlip 반환 + 앞면 상태가 됨
        [Test]
        public void TC_MM_003_FlipCard_First_ReturnsFirstFlip()
        {
            var s = new MemoryMatchState(4, new System.Random(1));
            var result = s.FlipCard(0);
            Assert.AreEqual(MemoryMatchState.FlipResult.FirstFlip, result);
            Assert.IsTrue(s.IsFaceUp(0));
        }

        // TC-MM-004: 두 번째 카드가 같은 짝이면 Match + 즉시 확정(정위치 유지)
        [Test]
        public void TC_MM_004_FlipCard_SecondMatchingCard_ReturnsMatch()
        {
            var s = new MemoryMatchState(4, new System.Random(7));
            s.FlipCard(0);
            int pairId = s.PairIdAt(0);
            int partner = FindOtherIndexWithPairId(s, pairId, exclude: 0);

            var result = s.FlipCard(partner);

            Assert.AreEqual(MemoryMatchState.FlipResult.Match, result);
            Assert.AreEqual(1, s.MatchedPairs);
            Assert.IsTrue(s.IsMatched(0));
            Assert.IsTrue(s.IsMatched(partner));
        }

        // TC-MM-005: 두 번째 카드가 다른 짝이면 Mismatch — 둘 다 잠깐 앞면 유지, pending 기록됨
        [Test]
        public void TC_MM_005_FlipCard_SecondDifferentCard_ReturnsMismatch()
        {
            var s = new MemoryMatchState(4, new System.Random(7));
            s.FlipCard(0);
            int pairId = s.PairIdAt(0);
            int other = FindIndexWithDifferentPairId(s, pairId);

            var result = s.FlipCard(other);

            Assert.AreEqual(MemoryMatchState.FlipResult.Mismatch, result);
            Assert.IsTrue(s.IsFaceUp(0));
            Assert.IsTrue(s.IsFaceUp(other));
            Assert.AreEqual(0, s.MatchedPairs);
        }

        // TC-MM-006: ResolveMismatch 호출하면 대기 중이던 두 카드가 다시 가려짐
        [Test]
        public void TC_MM_006_ResolveMismatch_HidesBothPendingCards()
        {
            var s = new MemoryMatchState(4, new System.Random(7));
            s.FlipCard(0);
            int other = FindIndexWithDifferentPairId(s, s.PairIdAt(0));
            s.FlipCard(other);

            s.ResolveMismatch();

            Assert.IsFalse(s.IsFaceUp(0));
            Assert.IsFalse(s.IsFaceUp(other));
            Assert.AreEqual(-1, s.PendingFirst);
            Assert.AreEqual(-1, s.PendingSecond);
        }

        // TC-MM-007: 이미 매치된 카드는 다시 뒤집을 수 없음(Invalid)
        [Test]
        public void TC_MM_007_FlipCard_AlreadyMatched_IsInvalid()
        {
            var s = new MemoryMatchState(4, new System.Random(7));
            s.FlipCard(0);
            int partner = FindOtherIndexWithPairId(s, s.PairIdAt(0), exclude: 0);
            s.FlipCard(partner);

            var result = s.FlipCard(0);
            Assert.AreEqual(MemoryMatchState.FlipResult.Invalid, result);
        }

        // TC-MM-008: 미스매치 두 장이 판정 대기 중일 때는 세 번째 카드를 뒤집을 수 없음(Invalid)
        [Test]
        public void TC_MM_008_FlipCard_WhilePendingMismatch_IsInvalid()
        {
            var s = new MemoryMatchState(4, new System.Random(7));
            s.FlipCard(0);
            int other = FindIndexWithDifferentPairId(s, s.PairIdAt(0));
            s.FlipCard(other);

            int third = 0;
            for (int i = 0; i < s.CardCount; i++) { if (i != 0 && i != other) { third = i; break; } }

            var result = s.FlipCard(third);
            Assert.AreEqual(MemoryMatchState.FlipResult.Invalid, result);
        }

        // TC-MM-009: 모든 쌍을 맞추면 IsComplete = true
        [Test]
        public void TC_MM_009_IsComplete_TrueWhenAllPairsMatched()
        {
            var s = new MemoryMatchState(2, new System.Random(3)); // 2쌍 = 4장
            while (!s.IsComplete)
            {
                int a = FirstUnmatchedUnflipped(s);
                s.FlipCard(a);
                int b = FindOtherIndexWithPairId(s, s.PairIdAt(a), exclude: a);
                var result = s.FlipCard(b);
                if (result == MemoryMatchState.FlipResult.Mismatch) s.ResolveMismatch();
            }
            Assert.IsTrue(s.IsComplete);
            Assert.AreEqual(2, s.MatchedPairs);
        }

        // TC-MM-010: 힌트 — 미매치 짝의 두 카드 인덱스를 반환(같은 pairId), 다 맞추면 null
        [Test]
        public void TC_MM_010_FindHintPair_ReturnsMatchingPairThenNull()
        {
            var s = new MemoryMatchState(2, new System.Random(3));
            var hint = s.FindHintPair();
            Assert.IsNotNull(hint);
            Assert.AreEqual(s.PairIdAt(hint.Value.first), s.PairIdAt(hint.Value.second));
            Assert.AreNotEqual(hint.Value.first, hint.Value.second);
            Assert.IsFalse(s.IsMatched(hint.Value.first));

            while (!s.IsComplete)
            {
                int a = FirstUnmatchedUnflipped(s);
                s.FlipCard(a);
                int b = FindOtherIndexWithPairId(s, s.PairIdAt(a), exclude: a);
                var result = s.FlipCard(b);
                if (result == MemoryMatchState.FlipResult.Mismatch) s.ResolveMismatch();
            }

            Assert.IsNull(s.FindHintPair());
        }

        // ── 헬퍼 ──────────────────────────────────────────────

        private static int FindOtherIndexWithPairId(MemoryMatchState s, int pairId, int exclude)
        {
            for (int i = 0; i < s.CardCount; i++)
                if (i != exclude && s.PairIdAt(i) == pairId) return i;
            throw new System.Exception("짝 카드 못 찾음");
        }

        private static int FindIndexWithDifferentPairId(MemoryMatchState s, int pairId)
        {
            for (int i = 0; i < s.CardCount; i++)
                if (s.PairIdAt(i) != pairId && !s.IsFaceUp(i) && !s.IsMatched(i)) return i;
            throw new System.Exception("다른 짝 카드 못 찾음");
        }

        private static int FirstUnmatchedUnflipped(MemoryMatchState s)
        {
            for (int i = 0; i < s.CardCount; i++)
                if (!s.IsMatched(i) && !s.IsFaceUp(i)) return i;
            throw new System.Exception("남은 카드 없음");
        }
    }
}
