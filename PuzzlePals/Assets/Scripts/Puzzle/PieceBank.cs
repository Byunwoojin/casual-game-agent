using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using PuzzlePals.UI;

namespace PuzzlePals.Puzzle
{
    /// <summary>
    /// 하단 조각 뱅크 — 초기 상태의 조각들이 가로로 놓이는 흰색 스트립.
    /// 유저가 조각을 드래그해서 끌어내면 Remove 되고, 남은 조각들은 자동 재배치.
    /// 조각이 안 보이게 많으면 빈 영역을 스와이프해서 스크롤 가능.
    /// </summary>
    public class PieceBank : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [SerializeField] private Color _bankColor      = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private float _verticalMargin = 0.3f; // 보드와 뱅크 사이 간격

        private Camera     _cam;
        private List<PuzzlePiece> _pieces = new List<PuzzlePiece>();
        private float _scrollOffset;
        private Vector2 _lastPointerScreen;

        // Bank 영역 (world 좌표)
        private float _centerY;
        private float _height;
        private float _left, _right;
        private float _pieceSpacing;

        private GameObject _bgGO;

        public int Count => _pieces.Count;

        /// <param name="cam">카메라 (viewport 계산용)</param>
        /// <param name="boardBottomY">보드 하단 world Y (뱅크는 그 아래에 배치)</param>
        /// <param name="cellSize">조각 한 변 world 크기 (spacing 기준)</param>
        /// <param name="bankHeight">뱅크 스트립 높이 (보통 cellSize * 1.3 정도)</param>
        public void Setup(Camera cam, float boardBottomY, float cellSize, float bankHeight)
        {
            _cam           = cam;
            _height        = bankHeight;
            // 직소 조각의 탭(요철) 이 셀 경계 ±25% 까지 돌출 → 실 폭 cellSize × 1.5 (worst case).
            // 1.85 = 탭 보호 (1.5) + 시각적 breathing (0.35).
            _pieceSpacing  = cellSize * 1.85f;
            _centerY       = boardBottomY - bankHeight * 0.5f - _verticalMargin;

            float camHalfW = cam.orthographicSize * cam.aspect;
            _left  = -camHalfW;
            _right =  camHalfW;

            CreateBackground();
        }

        private void CreateBackground()
        {
            if (_bgGO != null)
            {
                if (Application.isPlaying) Destroy(_bgGO);
                else                       DestroyImmediate(_bgGO);
            }

            _bgGO = new GameObject("BankBackground");
            _bgGO.transform.SetParent(transform, worldPositionStays: false);
            _bgGO.transform.localPosition = new Vector3(0f, _centerY, 0f);
            _bgGO.transform.localScale    = new Vector3(_right - _left, _height, 1f);

            var sr = _bgGO.AddComponent<SpriteRenderer>();
            var tex = Texture2D.whiteTexture;
            sr.sprite       = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                            new Vector2(0.5f, 0.5f), tex.width);
            // SerializeField 값 우선, 미설정 이면 DesignTokens.SurfaceCard 폴백
            sr.color        = _bankColor.a > 0f ? _bankColor : DesignTokens.SurfaceCard;
            sr.sortingOrder = -5;

            // BoxCollider2D + 포인터 이벤트 forwarder
            var box = _bgGO.AddComponent<BoxCollider2D>();
            box.size = Vector2.one;
            var fwd = _bgGO.AddComponent<BankPointerForwarder>();
            fwd.Bank = this;
        }

        public void Add(PuzzlePiece piece)
        {
            if (piece == null || _pieces.Contains(piece)) return;
            _pieces.Add(piece);
            piece.BankRef  = this;
            piece.IsInBank = true;
            UpdateLayout();
        }

        public void Remove(PuzzlePiece piece)
        {
            if (piece == null || !_pieces.Remove(piece)) return;
            piece.IsInBank = false;
            // BankRef 는 null 로 안 바꿈 — 조각이 나중에 드롭으로 뱅크 복귀할 때 참조 필요.
            UpdateLayout();
        }

        /// <summary>월드 좌표 기준 X 위치에 해당하는 인덱스로 끼워넣기. 기존 조각들은 자동 재배치.</summary>
        public void InsertAt(PuzzlePiece piece, float worldX)
        {
            if (piece == null) return;
            if (_pieces.Contains(piece)) return;

            // 현재 조각들의 X 위치를 훑어 worldX 보다 오른쪽에 있는 첫 인덱스를 찾음
            int index = _pieces.Count;
            for (int i = 0; i < _pieces.Count; i++)
            {
                if (_pieces[i] == null) continue;
                if (_pieces[i].transform.position.x > worldX)
                {
                    index = i;
                    break;
                }
            }

            _pieces.Insert(index, piece);
            piece.BankRef  = this;
            piece.IsInBank = true;
            UpdateLayout();
        }

        /// <summary>월드 좌표가 뱅크 스트립 영역(Y 범위) 안인지.</summary>
        public bool IsWithinBankArea(Vector3 worldPos)
        {
            float top    = _centerY + _height * 0.5f;
            float bottom = _centerY - _height * 0.5f;
            return worldPos.y >= bottom && worldPos.y <= top;
        }

        private void UpdateLayout()
        {
            float bankWidth  = _right - _left;
            float totalWidth = _pieces.Count * _pieceSpacing;

            float startX;
            if (totalWidth <= bankWidth)
            {
                // 전부 들어가면 중앙 정렬, 스크롤 리셋
                _scrollOffset = 0f;
                startX = -totalWidth * 0.5f + _pieceSpacing * 0.5f;
            }
            else
            {
                // 오버플로 → 좌측 정렬 + 스크롤 오프셋
                startX = _left + _pieceSpacing * 0.5f + _scrollOffset;
            }

            for (int i = 0; i < _pieces.Count; i++)
            {
                var p = _pieces[i];
                if (p == null) continue;
                p.transform.position = new Vector3(startX + i * _pieceSpacing, _centerY, 0f);
                p.OriginalPosition   = p.transform.position;
            }
        }

        /// <summary>가로 세계 단위 만큼 뱅크 스크롤 (양수=오른쪽으로 shift)</summary>
        public void Scroll(float worldDeltaX)
        {
            float bankWidth  = _right - _left;
            float totalWidth = _pieces.Count * _pieceSpacing;
            if (totalWidth <= bankWidth) return;

            _scrollOffset += worldDeltaX;
            float minOffset = -(totalWidth - bankWidth);
            _scrollOffset = Mathf.Clamp(_scrollOffset, minOffset, 0f);
            UpdateLayout();
        }

        // ── Pointer events (빈 뱅크 영역 스와이프 시) ──
        public void OnPointerDown(PointerEventData e)
        {
            _lastPointerScreen = e.position;
        }

        public void OnDrag(PointerEventData e)
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;

            float worldPerScreen = (_cam.orthographicSize * 2f * _cam.aspect) / Screen.width;
            float worldDeltaX    = (e.position.x - _lastPointerScreen.x) * worldPerScreen;
            Scroll(worldDeltaX);
            _lastPointerScreen = e.position;
        }
    }

    /// <summary>뱅크 배경 GameObject 에서 발생한 포인터 이벤트를 PieceBank 로 전달.</summary>
    public class BankPointerForwarder : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public PieceBank Bank;
        public void OnPointerDown(PointerEventData e) => Bank?.OnPointerDown(e);
        public void OnDrag(PointerEventData e)        => Bank?.OnDrag(e);
    }
}
