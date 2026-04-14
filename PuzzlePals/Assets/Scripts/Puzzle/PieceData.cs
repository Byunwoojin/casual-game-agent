using UnityEngine;

namespace PuzzlePals.Puzzle
{
    /// <summary>조각 하나의 순수 데이터 — MonoBehaviour 없이 테스트 가능</summary>
    public struct PieceData
    {
        /// <summary>원본 스프라이트 내 픽셀 Rect (텍스처 좌하단 기준)</summary>
        public Rect PixelRect;

        /// <summary>이 조각이 맞춰져야 할 세계 좌표 (스냅존 위치)</summary>
        public Vector2 SnapPosition;

        /// <summary>현재 배치 세계 좌표 (PlacePiecesAtBottom 후 채워짐)</summary>
        public Vector2 CurrentPosition;

        /// <summary>스프라이트 Pixels Per Unit — WorldRect 계산에 사용</summary>
        public float PixelsPerUnit;

        /// <summary>세계 좌표 기준 조각 Rect (겹침 판정용)</summary>
        public Rect WorldRect => new Rect(
            CurrentPosition.x - PixelRect.width  / PixelsPerUnit / 2f,
            CurrentPosition.y - PixelRect.height / PixelsPerUnit / 2f,
            PixelRect.width  / PixelsPerUnit,
            PixelRect.height / PixelsPerUnit
        );
    }

    /// <summary>스냅존 위치 데이터</summary>
    public struct SnapZoneData
    {
        public Vector2 Position;
        public bool    IsOccupied;
    }
}
