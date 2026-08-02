using System.Collections.Generic;
using UnityEngine;

namespace SlidingPuzzle.Gameplay
{
    /// <summary>
    /// N-퍼즐 그리드 상태 — 순수 로직만 (Unity GameObject/UI 의존 없음, 유닛 테스트 가능).
    /// SlidingBoard(MonoBehaviour) 가 이 클래스를 감싸서 렌더링/입력만 담당한다.
    /// </summary>
    public class SlidingGridState
    {
        public readonly int GridSize;
        public readonly int TotalCells;
        public readonly int MovableCount; // TotalCells - 1 (빈 칸 제외)

        private readonly int[] _pieceAtSlot; // slot -> pieceIndex (-1 = 빈 칸)
        private readonly int[] _slotOfPiece; // pieceIndex -> slot

        public int BlankSlot { get; private set; }

        public SlidingGridState(int gridSize)
        {
            GridSize     = Mathf.Max(2, gridSize);
            TotalCells   = GridSize * GridSize;
            MovableCount = TotalCells - 1;
            _pieceAtSlot = new int[TotalCells];
            _slotOfPiece = new int[MovableCount];
            ResetSolved();
        }

        /// <summary>
        /// 테스트/디버그 전용 — 임의의 배치를 직접 주입해서 구성 (일반 게임 흐름에서는 쓰지 않음).
        /// IsSolvable() 의 parity 공식을 셔플 메커니즘과 독립적으로 검증하기 위해 필요.
        /// </summary>
        public SlidingGridState(int gridSize, int[] rawPieceAtSlot)
        {
            GridSize     = Mathf.Max(2, gridSize);
            TotalCells   = GridSize * GridSize;
            MovableCount = TotalCells - 1;
            _pieceAtSlot = new int[TotalCells];
            _slotOfPiece = new int[MovableCount];
            System.Array.Copy(rawPieceAtSlot, _pieceAtSlot, TotalCells);
            for (int slot = 0; slot < TotalCells; slot++)
            {
                if (_pieceAtSlot[slot] == -1) BlankSlot = slot;
                else _slotOfPiece[_pieceAtSlot[slot]] = slot;
            }
        }

        public void ResetSolved()
        {
            for (int i = 0; i < MovableCount; i++) { _pieceAtSlot[i] = i; _slotOfPiece[i] = i; }
            _pieceAtSlot[TotalCells - 1] = -1;
            BlankSlot = TotalCells - 1;
        }

        public int SlotOfPiece(int piece) => _slotOfPiece[piece];
        public int PieceAtSlot(int slot)  => _pieceAtSlot[slot];

        public bool IsAdjacentToBlank(int slot)
        {
            int rowDiff = Mathf.Abs(slot / GridSize - BlankSlot / GridSize);
            int colDiff = Mathf.Abs(slot % GridSize - BlankSlot % GridSize);
            return (rowDiff + colDiff) == 1;
        }

        public void GetAdjacentSlots(int slot, List<int> result)
        {
            result.Clear();
            int row = slot / GridSize, col = slot % GridSize;
            if (row > 0)              result.Add(slot - GridSize);
            if (row < GridSize - 1)   result.Add(slot + GridSize);
            if (col > 0)              result.Add(slot - 1);
            if (col < GridSize - 1)   result.Add(slot + 1);
        }

        /// <summary>빈 칸과 slot 을 스왑 — slot 은 반드시 빈 칸과 인접해야 함(호출 측 보장 — IsAdjacentToBlank 로 사전 확인).</summary>
        public void SwapBlankWith(int slot)
        {
            int piece = _pieceAtSlot[slot];
            _pieceAtSlot[BlankSlot] = piece;
            _pieceAtSlot[slot]      = -1;
            _slotOfPiece[piece]     = BlankSlot;
            BlankSlot = slot;
        }

        public int CountCorrect()
        {
            int c = 0;
            for (int piece = 0; piece < MovableCount; piece++)
                if (_slotOfPiece[piece] == piece) c++;
            return c;
        }

        public bool IsSolved() => CountCorrect() == MovableCount;

        /// <summary>
        /// 정렬 상태에서 유효한 이동(빈 칸 인접 스왑)을 랜덤하게 역산 적용해서 셔플 생성.
        /// 매 스텝이 가역적인 유효 이동이므로, 결과는 항상 풀 수 있는 상태만 나온다
        /// — parity 계산으로 사후 검증할 필요가 없다 (IsSolvable() 은 그 사실을 확인하는 테스트용).
        /// </summary>
        public void Shuffle(System.Random rng, int moves = -1)
        {
            ResetSolved();
            if (moves < 0) moves = TotalCells * 30;

            int prevBlank = -1;
            var buf = new List<int>(4);
            for (int i = 0; i < moves; i++)
            {
                GetAdjacentSlots(BlankSlot, buf);
                int pick = buf[rng.Next(buf.Count)];
                // 직전 위치로 즉시 되돌리는 이동은 가능하면 회피 (셔플 품질 — 필수 조건은 아님)
                if (buf.Count > 1 && pick == prevBlank)
                {
                    int alt = buf[rng.Next(buf.Count)];
                    if (alt != prevBlank) pick = alt;
                }
                prevBlank = BlankSlot;
                SwapBlankWith(pick);
            }
        }

        /// <summary>
        /// 클래식 N-퍼즐 solvability 정리 — 순열 parity(+짝수 그리드는 빈 칸 행 거리)로 판정.
        /// Shuffle() 은 항상 valid-move 역산으로만 상태를 만들기 때문에 이 값은 항상 true 여야 한다
        /// — 그 사실 자체를 테스트로 확인하는 용도 (사전 보장 설계이지 사후 검증 게이트가 아님).
        /// </summary>
        public bool IsSolvable()
        {
            var seq = new int[TotalCells];
            for (int slot = 0; slot < TotalCells; slot++)
                seq[slot] = _pieceAtSlot[slot] == -1 ? MovableCount : _pieceAtSlot[slot];

            int inversions = 0;
            for (int i = 0; i < TotalCells; i++)
                for (int j = i + 1; j < TotalCells; j++)
                    if (seq[i] != MovableCount && seq[j] != MovableCount && seq[i] > seq[j])
                        inversions++;

            if (GridSize % 2 == 1)
                return inversions % 2 == 0;

            int blankRowFromTop        = BlankSlot / GridSize;
            int blankRowFromBottom1Based = GridSize - blankRowFromTop;
            return (inversions + blankRowFromBottom1Based) % 2 == 1;
        }
    }
}
