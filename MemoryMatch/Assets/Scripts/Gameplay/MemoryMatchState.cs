using System.Collections.Generic;

namespace MemoryMatch.Gameplay
{
    /// <summary>
    /// 메모리 매칭 그리드 상태 — 순수 로직만 (Unity GameObject/UI 의존 없음, 유닛 테스트 가능).
    /// MatchBoard(MonoBehaviour) 가 이 클래스를 감싸서 렌더링/입력/타이밍(뒤집기 딜레이)만 담당한다.
    /// </summary>
    public class MemoryMatchState
    {
        public enum FlipResult { Invalid, FirstFlip, Match, Mismatch }

        public readonly int PairCount;
        public readonly int CardCount;
        public int MatchedPairs { get; private set; }
        public bool IsComplete => MatchedPairs >= PairCount;

        private readonly int[]  _pairIdOfCard; // card index -> pairId (0..PairCount-1)
        private readonly bool[] _faceUp;
        private readonly bool[] _matched;

        // 미스매치로 잠깐 뒤집혀서 대기 중인 두 카드 (ResolveMismatch 호출 전까지 유지)
        public int PendingFirst  { get; private set; } = -1;
        public int PendingSecond { get; private set; } = -1;

        public MemoryMatchState(int pairCount, System.Random rng)
        {
            PairCount = pairCount;
            CardCount = pairCount * 2;

            _pairIdOfCard = new int[CardCount];
            for (int i = 0; i < CardCount; i++) _pairIdOfCard[i] = i % pairCount;

            // Fisher-Yates 셔플
            for (int i = CardCount - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (_pairIdOfCard[i], _pairIdOfCard[j]) = (_pairIdOfCard[j], _pairIdOfCard[i]);
            }

            _faceUp  = new bool[CardCount];
            _matched = new bool[CardCount];
        }

        public int PairIdAt(int index) => _pairIdOfCard[index];
        public bool IsFaceUp(int index) => _faceUp[index];
        public bool IsMatched(int index) => _matched[index];

        /// <summary>
        /// 카드를 탭했을 때 호출. 첫 장이면 뒤집고 FirstFlip, 두 번째 장이면 즉시 비교해서
        /// Match(그대로 앞면 유지) 또는 Mismatch(호출측이 딜레이 후 ResolveMismatch 호출해서 닫아야 함) 반환.
        /// 이미 매치됐거나 뒤집혀 있거나, 미스매치 대기 중(두 장 다 보여주는 동안)이면 Invalid.
        /// </summary>
        public FlipResult FlipCard(int index)
        {
            if (index < 0 || index >= CardCount) return FlipResult.Invalid;
            if (_matched[index] || _faceUp[index]) return FlipResult.Invalid;
            if (PendingFirst != -1 && PendingSecond != -1) return FlipResult.Invalid; // 미스매치 판정 대기 중

            _faceUp[index] = true;

            if (PendingFirst == -1)
            {
                PendingFirst = index;
                return FlipResult.FirstFlip;
            }

            PendingSecond = index;
            if (_pairIdOfCard[PendingFirst] == _pairIdOfCard[PendingSecond])
            {
                _matched[PendingFirst]  = true;
                _matched[PendingSecond] = true;
                MatchedPairs++;
                PendingFirst  = -1;
                PendingSecond = -1;
                return FlipResult.Match;
            }

            return FlipResult.Mismatch;
        }

        /// <summary>Mismatch 이후 호출 — 대기 중인 두 카드를 다시 뒤집어 가림.</summary>
        public void ResolveMismatch()
        {
            if (PendingFirst != -1)  _faceUp[PendingFirst]  = false;
            if (PendingSecond != -1) _faceUp[PendingSecond] = false;
            PendingFirst  = -1;
            PendingSecond = -1;
        }

        /// <summary>
        /// 힌트 — 아직 못 맞춘 짝 하나의 카드 두 장(인덱스)을 반환. 자동 매치 아님 —
        /// 호출측이 잠깐 두 장 다 보여줬다 가리기만 하고, 실제 매치는 플레이어가 직접 탭해야 함.
        /// 없으면(모든 짝이 매치됨) null.
        /// </summary>
        public (int first, int second)? FindHintPair()
        {
            for (int pairId = 0; pairId < PairCount; pairId++)
            {
                int first = -1, second = -1;
                for (int i = 0; i < CardCount; i++)
                {
                    if (_pairIdOfCard[i] != pairId) continue;
                    if (_matched[i]) { first = -1; second = -1; break; } // 이미 매치된 짝 — 스킵
                    if (first == -1) first = i;
                    else second = i;
                }
                if (first != -1 && second != -1) return (first, second);
            }
            return null;
        }
    }
}
