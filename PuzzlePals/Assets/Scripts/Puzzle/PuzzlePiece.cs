using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using PuzzlePals.Systems;

namespace PuzzlePals.Puzzle
{
    /// <summary>
    /// 직소 퍼즐 조각 — MeshRenderer 로 렌더, PolygonCollider2D 로 히트 테스트, 드래그/스냅.
    /// 피봇은 조각의 base cell 중심. transform.position = 조각이 "살고 있는 지점" (snap 위치).
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(PolygonCollider2D))]
    public class PuzzlePiece : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        // ── 공개 상태 ────────────────────────────────────────
        public bool    IsSnapped        { get; private set; }
        public Vector3 OriginalPosition { get; set; }

        // Unity 직렬화 (씬 저장 호환 위해 public field)
        public SnapZone AssignedSnapZone;
        public float    SnapThreshold = 0.4f; // jigsaw 는 정확 스냅 필요 — 옛 1.8 대비 타이트

        // ── 뱅크 상태 (런타임만, 직렬화 X) ──────────────────
        [System.NonSerialized] public bool      IsInBank;
        [System.NonSerialized] public PieceBank BankRef;

        // ── 그리드 좌표 (런타임만, 직렬화 X) ─────────────────
        // ProgressCelebrationController 의 가장자리 감지에 사용 (Step 4-B)
        [System.NonSerialized] public int Row = -1;
        [System.NonSerialized] public int Col = -1;

        // ── 인접 짝맞춤 그룹 (Phase 1) ──────────────────────
        // null = isolated. Phase 2 에서 합체 시점에 PieceGroup 으로 묶인다.
        [System.NonSerialized] public PieceGroup Group;

        // ── 보드 reference (Phase 2 인접 짝맞춤 호출용) ─────
        [System.NonSerialized] public PuzzleBoard Board;

        // ── 내부 참조 ────────────────────────────────────────
        private MeshFilter        _mf;
        private MeshRenderer      _mr;
        private PolygonCollider2D _col;
        private Camera            _cam;
        private bool              _dragging;
        private Vector3           _dragOffset;       // 포인터와 조각 중심의 상대 거리 (집은 순간 점프 방지)
        private Vector2           _pointerDownScreen; // 뱅크에서 빠져나올 때 필요한 최소 드래그 거리 판정
        private bool              _draggedOutOfBank;  // 현재 드래그에서 뱅크 이탈 처리 완료 여부

        // 드래그 중에 다른 조각 위로 올라가도록 sortingOrder 동적 조정
        private const int IdleSortingOrder = 10;
        private const int DragSortingOrder = 100;

        /// <summary>스냅 성공 시점에 발사 — Undo 스택 등 외부 리스너용 (Step 6 Undo).</summary>
        public static event Action<PuzzlePiece> OnAnyPieceSnapped;

        private void Awake()
        {
            _cam = Camera.main;
            _mf  = GetComponent<MeshFilter>();
            _mr  = GetComponent<MeshRenderer>();
            _col = GetComponent<PolygonCollider2D>();
            if (_mr != null) _mr.sortingOrder = IdleSortingOrder;
            OriginalPosition = transform.position;
        }

        /// <summary>PuzzleBoard 에서 호출. mesh / material / collider outline 일괄 세팅.</summary>
        public void Initialize(Mesh mesh, Material sharedMaterial,
                               IReadOnlyList<Vector2> outlineForCollider, SnapZone zone)
        {
            if (_mf == null)  _mf  = GetComponent<MeshFilter>();
            if (_mr == null)  _mr  = GetComponent<MeshRenderer>();
            if (_col == null) _col = GetComponent<PolygonCollider2D>();

            _mf.mesh           = mesh;
            _mr.sharedMaterial = sharedMaterial;
            _mr.sortingOrder   = IdleSortingOrder;

            // PolygonCollider2D 는 Vector2[] 를 경로로 받음. SetPath(0, ...) 은 첫 subpath.
            var pts = new Vector2[outlineForCollider.Count];
            for (int i = 0; i < pts.Length; i++) pts[i] = outlineForCollider[i];
            _col.pathCount = 1;
            _col.SetPath(0, pts);

            AssignedSnapZone = zone;
        }

        // ─────────────────────────────────────────────────────
        // 스냅 로직 (EditMode 테스트에서 직접 호출 가능)
        // ─────────────────────────────────────────────────────

        public bool TrySnap(SnapZone zone, float snapThreshold)
        {
            if (zone == null || zone.IsOccupied) return false;

            float dist = Vector2.Distance(transform.position, zone.transform.position);
            if (dist <= snapThreshold)
            {
                transform.position = zone.transform.position;
                IsSnapped          = true;
                zone.IsOccupied    = true;
                if (_mr != null) _mr.sortingOrder = IdleSortingOrder;

                PlaySnapJuice();
                OnAnyPieceSnapped?.Invoke(this);

                // Phase 3: 그룹이면 모든 멤버를 leader (this) 의 새 위치 기준 grid 정렬 + 자기 SnapZone 흡수
                if (Group != null && Group.CellW > 0f)
                {
                    Group.RealignToLeader(this, Group.CellW, Group.CellH);
                    var members = new List<PuzzlePiece>(Group.Members);
                    foreach (var m in members)
                    {
                        if (m == null || m == this) continue;
                        m.MarkSnappedAtAssignedZoneSilent();
                    }
                }
                return true;
            }

            // 스냅 실패 → 놓은 자리에 그대로 머무름 (piece bank 복귀 X, 성인 피벗 v0.4)
            if (_mr != null) _mr.sortingOrder = IdleSortingOrder;
            return false;
        }

        /// <summary>
        /// Phase 3 그룹 흡수용 — 효과음/VFX 없이 자기 AssignedSnapZone 에 안착.
        /// (효과음/Undo 이벤트는 leader 한 번만 트리거 — 그룹 멤버 우두두두 효과음 방지)
        /// </summary>
        internal void MarkSnappedAtAssignedZoneSilent()
        {
            if (IsSnapped) return;
            if (AssignedSnapZone == null || AssignedSnapZone.IsOccupied) return;
            transform.position           = AssignedSnapZone.transform.position;
            IsSnapped                    = true;
            AssignedSnapZone.IsOccupied  = true;
            if (_mr != null) _mr.sortingOrder = IdleSortingOrder;
            // OnAnyPieceSnapped 미발사 — Undo 스택은 leader 만 추적
        }

        // 안착 시 효과음 + VFX. scale punch 효과는 제거됨 — 그룹 멤버 mismatch 로 hairline 발생.
        // 시각 피드백은 VFXManager.PlaySnapBurst (파티클) 로 충분.
        // 효과음은 PlayPair (합체 시와 동일 — 사용자 결정 2026-05-01).
        private void PlaySnapJuice()
        {
            if (!Application.isPlaying) return;
            VFXManager.Instance?.PlaySnapBurst(transform.position);
            AudioManager.Instance?.PlayPair();
        }

        // ─────────────────────────────────────────────────────
        // 드래그 입력
        // ─────────────────────────────────────────────────────

        /// <summary>PieceGroup 이 멤버 sortingOrder 를 일괄 변경할 때 호출.</summary>
        public void SetSortingOrder(int order)
        {
            if (_mr != null) _mr.sortingOrder = order;
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (IsSnapped) return;
            _dragging = true;
            OriginalPosition = transform.position;
            if (Group != null) Group.SetSortingOrder(DragSortingOrder);
            else if (_mr != null) _mr.sortingOrder = DragSortingOrder;

            if (_cam == null) _cam = Camera.main;
            Vector3 ptr = _cam.ScreenToWorldPoint(new Vector3(e.position.x, e.position.y,
                Mathf.Abs(_cam.transform.position.z)));
            ptr.z = 0f;
            _dragOffset = transform.position - ptr;

            _pointerDownScreen  = e.position;
            _draggedOutOfBank   = false;

            // 들어올림 scale 효과 제거 — leader/group mismatch 로 양각 polyline hairline 발생.
            // 시각 피드백은 sortingOrder 변경 (다른 piece 위로 올라옴) 으로 대체.
            // 조각 집을 때 효과음은 의도적으로 제거 (사용자 결정 2026-04-27 — 너무 빈번)
        }

        // 뱅크 이탈 판정 임계값 (스크린 픽셀). 이 거리 이하 드래그는 뱅크 내 스크롤 의도로 간주 가능하나,
        // 현 구현은 "조각 누르면 바로 내 것" — 단, 점프 방지를 위해 최소 10px 움직인 뒤에만 따라옴.
        private const float BankExitThresholdPx = 10f;

        public void OnDrag(PointerEventData e)
        {
            if (IsSnapped || !_dragging) return;
            if (_cam == null) _cam = Camera.main;

            // 뱅크에서 끌어내기 — 최소 거리 드래그 후 Remove 처리
            if (IsInBank && !_draggedOutOfBank)
            {
                if (Vector2.Distance(e.position, _pointerDownScreen) < BankExitThresholdPx) return;
                _draggedOutOfBank = true;
                BankRef?.Remove(this);
                _dragOffset = Vector3.zero; // 포인터 바로 아래로 매끄럽게
            }

            Vector3 ptr = _cam.ScreenToWorldPoint(new Vector3(e.position.x, e.position.y,
                Mathf.Abs(_cam.transform.position.z)));
            ptr.z = 0f;
            Vector3 newPos = ptr + _dragOffset;

            if (Group != null && Group.CellW > 0f)
            {
                // leader (this) 위치 갱신 후 멤버를 grid 절대 위치로 재정렬 — 매 frame delta 누적 오차 방지.
                // cellSize 는 그룹 자체에 저장되어 있어 Board reference 가 null 이어도 안전.
                transform.position = newPos;
                Group.RealignToLeader(this, Group.CellW, Group.CellH);
            }
            else if (Group != null)
            {
                // 그룹은 있는데 cellSize 미주입 (이론상 안 옴 — NeighborPairing.AlignAndMerge 가 항상 SetCellSize)
                Vector3 delta = newPos - transform.position;
                Group.MoveBy(delta);
            }
            else
            {
                transform.position = newPos;
            }
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (IsSnapped) return;
            _dragging = false;

            bool snapped = false;
            if (AssignedSnapZone != null)
                snapped = TrySnap(AssignedSnapZone, SnapThreshold);

            // 스냅 실패 + 뱅크 영역 위에 놓였으면 뱅크에 재삽입 (끼워넣기)
            if (!snapped && BankRef != null && !IsInBank && BankRef.IsWithinBankArea(transform.position))
            {
                BankRef.InsertAt(this, transform.position.x);
            }

            // 영역 밖 (헤더 위 또는 배너 아래) 으로 놓였으면 가용 영역 안 가장 가까운 위치로 clamp
            // (OriginalPosition 으로 돌아가지 않음 — 사용자 의도: 드래그 한 위치 근처 유지)
            if (!snapped && !IsInBank && Board != null)
            {
                float topY    = Board.DraggableTopY;
                float bottomY = Board.DraggableBottomY;
                Vector3 pos = transform.position;
                if (pos.y > topY || pos.y < bottomY)
                {
                    pos.y = Mathf.Clamp(pos.y, bottomY, topY);
                    transform.DOKill();
                    transform.DOMove(pos, 0.25f).SetEase(Ease.OutCubic);
                }
            }

            // 그룹 멤버 sortingOrder 복귀
            if (Group != null) Group.SetSortingOrder(IdleSortingOrder);

            // Phase 2: 정답 SnapZone 안 맞췄을 때만 인접 짝맞춤 검사
            if (!snapped) Board?.TryNeighborPair(this);
        }

        // EditMode 테스트 전용
        public void SetSnappedForTest(bool v) => IsSnapped = v;

        // ─────────────────────────────────────────────────────
        // 힌트 — 자동 스냅 (Step 5)
        // ─────────────────────────────────────────────────────

        /// <summary>힌트로 호출 — 조각을 AssignedSnapZone 위치로 부드럽게 이동시키고 스냅 처리.</summary>
        public void AutoSnap()
        {
            if (IsSnapped || AssignedSnapZone == null || AssignedSnapZone.IsOccupied) return;

            // 뱅크에서 제거 (있다면)
            if (IsInBank && BankRef != null) BankRef.Remove(this);
 
            Vector3 target = AssignedSnapZone.transform.position;
            if (_mr != null) _mr.sortingOrder = DragSortingOrder; // 이동 동안 다른 조각 위에

            transform.DOKill();
            transform.localScale = Vector3.one;
            transform.DOMove(target, 0.6f).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                IsSnapped                 = true;
                AssignedSnapZone.IsOccupied = true;
                if (_mr != null) _mr.sortingOrder = IdleSortingOrder;
                PlaySnapJuice();
                OnAnyPieceSnapped?.Invoke(this);
            });
        }

        /// <summary>Undo 호출 — 스냅 해제 후 뱅크에 복귀시킬 수 있는 상태로 만든다.</summary>
        public void Unsnap()
        {
            if (!IsSnapped) return;
            IsSnapped = false;
            if (AssignedSnapZone != null) AssignedSnapZone.IsOccupied = false;
            transform.DOKill();
            transform.localScale = Vector3.one;
            if (_mr != null) _mr.sortingOrder = IdleSortingOrder;
        }
    }
}
