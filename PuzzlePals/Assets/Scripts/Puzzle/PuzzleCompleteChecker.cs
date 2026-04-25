using System;
using UnityEngine;

namespace PuzzlePals.Puzzle
{
    /// <summary>모든 조각이 스냅됐는지 확인하고 완료 이벤트를 발행</summary>
    public class PuzzleCompleteChecker : MonoBehaviour
    {
        /// <summary>모든 조각이 스냅되는 순간 한 번 발행</summary>
        public event Action OnPuzzleComplete;

        /// <param name="pieces">현재 레벨의 모든 퍼즐 조각</param>
        /// <returns>전부 스냅됐으면 true</returns>
        public bool Check(PuzzlePiece[] pieces)
        {
            if (pieces == null || pieces.Length == 0) return false;
            foreach (var piece in pieces)
            {
                // Restart 직후 등에 stale reference (destroyed Unity Object) 들어올 수 있음
                if (piece == null) return false;
                if (!piece.IsSnapped) return false;
            }

            OnPuzzleComplete?.Invoke();
            return true;
        }
    }
}
