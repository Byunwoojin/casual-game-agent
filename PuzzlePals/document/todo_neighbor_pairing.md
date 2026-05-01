# 인접 짝맞춤 (Neighbor Pairing) 기능 구현

**작성일**: 2026-04-27
**상태**: Phase 1~3 완료 (2026-05-01). Phase 4 (디바이스 검증) 만 남음
**예상 소요**: 4-8시간 (Phase 1~3 한 세션, Phase 4 별도)

---

## 목적

진짜 직소 게임 경험 — 두 조각이 정답 자리(SnapZone)에 들어가지 않아도, 서로 인접 짝(요철 일치)으로 만나면 자동으로 align + 합체. 합체된 조각들은 한 그룹으로 같이 움직임. 그룹의 한 조각이 자기 SnapZone 가까이 가면 그룹 전체가 정답 자리로 흡수.

현재 구현은 **개별 조각 → SnapZone 정답 위치만** 스냅. 인접 짝맞춤 X.

## 사용자 결정 사항 (2026-04-27)

- 시각적 합체 + 그룹 드래그 + 효과음 모두 구현 (옵션 a)
- 큰 작업이라 별도 대화로 분리

## 핵심 설계

### 신규 클래스: `PieceGroup`
- 합체된 조각들 list
- 그룹 ID (또는 reference 자체로 비교)
- 그룹 단위 이동, 회전 X (직소는 회전 없음)
- 그룹이 SnapZone 흡수 시 모든 멤버 IsSnapped = true

### `PuzzlePiece` 변경
- `public PieceGroup Group` 필드
- 드래그 시 `Group` 이 있으면 그룹 전체 이동, 없으면 자기만 이동
- TrySnap 후 인접 조각 검사 → 짝맞춤 조건 만족 시 합체

### 짝맞춤 검출 로직
조각 A 가 OnPointerUp 또는 OnDrag 종료 시점에:
1. 인접 조각 (Row±1, Col±1, 대각선 제외) 의 PieceGroup 또는 isolated piece 들 가져옴
2. 인접 조각 B 가 있다면, B 의 현재 위치 + (A.Row - B.Row, A.Col - B.Col) × cellSize = A 의 "예상 위치"
3. A 의 현재 위치와 예상 위치 거리 < snapTolerance (예: cellW × 0.3) 면 합체
4. A 가 B 의 그룹에 합류 (또는 A/B 둘 다 새 그룹 생성)

### SnapZone 흡수
- 그룹의 한 조각이 자기 SnapZone 정답 위치 가까이 가면
- 그룹 전체 offset 계산 (한 조각 → 정답 위치 delta)
- 그룹 모든 조각에 그 delta 적용 → 정확 align
- 모든 조각 IsSnapped = true, AssignedSnapZone.IsOccupied = true

## 영향받는 파일

### 신규
- `Assets/Scripts/Puzzle/PieceGroup.cs`

### 수정
- `Assets/Scripts/Puzzle/PuzzlePiece.cs` — Group 필드, 드래그 로직, 짝맞춤 검출
- `Assets/Scripts/Puzzle/PuzzleBoard.cs` — cellW/cellH public 노출 (짝맞춤 거리 계산용)
- `Assets/Scripts/Puzzle/PuzzleCompleteChecker.cs` — 그룹 단위 체크 (선택, 기존도 동작 가능)
- `Assets/Scripts/Systems/AudioManager.cs` — `PlayPair()` 같은 새 효과음 메서드 (또는 기존 PlayDing 재사용)

## 단계별 작업 plan

### Phase 1: PieceGroup 인프라 + 그룹 드래그 (1-2h) ✅ 완료
- [x] PieceGroup.cs 작성 — list, AddPiece/RemovePiece, MergeFrom, MoveBy, RealignToLeader, SetCellSize
- [x] PuzzlePiece 에 `public PieceGroup Group` 필드 추가 (NonSerialized)
- [x] OnPointerDown / OnDrag / OnPointerUp 에서 Group 있으면 그룹 전체 이동
- [x] 드래그 시 sortingOrder 그룹 전체 일괄 변경
- [x] **검증**: PieceGroupTests TC-PG-001~006

### Phase 2: 인접 조각 거리 검출 + 합체 (1-2h) ✅ 완료
- [x] PuzzleBoard.CellW / CellH public 노출 + (row,col) dictionary
- [x] PuzzlePiece.OnPointerUp 끝에 PuzzleBoard.TryNeighborPair 호출
- [x] 4-방향 인접 (Row±1 또는 Col±1, 대각선 제외) 검색
- [x] 짝맞춤 조건 (cellW × 0.3 tolerance) 만족 시 그룹 합체
- [x] 합체 시 A 위치를 B 기준으로 정확히 align (NeighborPairing.AlignAndMerge)
- [x] **검증**: NeighborPairingTests TC-NP-001~006

### Phase 3: SnapZone 그룹 흡수 + 효과음/VFX (30분) ✅ 완료
- [x] PuzzlePiece.TrySnap 의 그룹 분기 — leader 안착 시 RealignToLeader + 멤버 MarkSnappedAtAssignedZoneSilent
- [x] 합체 시점 효과음 (PlayPair 사용 — 절차적 합성 fallback 추가)
- [x] 합체 시 VFXManager.PlaySnapBurst 호출 (작은 파티클)

### Phase 3.5: 시각 hairline 해결 (예상 외 작업, 2026-05-01)
- [x] 정지 상태 hairline → JigsawMeshBuilder 에서 `mesh.colors = Color.white` 명시 (vertex color GPU default 회피)
- [x] drag 중 양각 hairline → PuzzlePiece 의 DOScale(1.05) 들어올림 효과 + DOPunchScale 안착 punch 제거. leader 만 scale 변경 시 양각 polyline 이 인접 멤버 음각과 mismatch 되어 hairline 발생. scale 효과 자체 제거가 root cause fix
- [x] 합체 효과음 PlayDing → PlayPair 변경 (정답 안착과 효과음 분리)

### Phase 4: 디바이스 검증 + 미세 조정 (1h+, 별도 세션 가능) — 보류
- [ ] snapTolerance 값 조정 (cellW × 0.3 → 너무 빡빡하면 0.4, 너무 헐거우면 0.25)
- [ ] 그룹 드래그 시 시각적 alignment (offset 정확한지)
- [ ] 멀티-조각 그룹 (3개 이상) 동작 확인
- [ ] 그룹 vs 그룹 합체 동작
- [ ] 16레벨 풀-사이클 플레이 테스트

## 새 대화에서 시작하는 법

새 대화 (Claude 새 채팅) 에서 첫 메시지로 다음 중 하나 보내면 됩니다:

### 옵션 A — 짧게
```
인접 짝맞춤 기능 구현 시작. document/todo_neighbor_pairing.md 부터 봐줘.
```

### 옵션 B — Phase 명시
```
인접 짝맞춤 기능 Phase 1+2+3 까지 한 세션에 진행하려고 해.
document/todo_neighbor_pairing.md 부터 보고 시작해줘.
```

### 옵션 C — 안전하게 단계별
```
인접 짝맞춤 기능 구현. document/todo_neighbor_pairing.md 보고
일단 Phase 1 (PieceGroup 인프라 + 그룹 드래그) 만 진행해줘.
완료 후 검증하고 다음 phase 결정할게.
```

→ Claude 가 이 파일 + 메모리 (project_adult_pivot.md, feedback_*) 자동으로 읽고 cold-start 가능. CLAUDE.md 에 박힌 규칙들 (CanvasScaler 0f, pieceCount 제곱수, BuildLocalizedText 등) 도 자동 적용됨.

## 진행 시 주의사항

- **drop-stay 정책 보존**: 스냅 실패 시 놓은 자리 유지 (피스뱅크 복귀 X). 이 결정 유지.
- **cellW/cellH 단위 일관성**: 짝맞춤 거리 계산 시 world unit 사용. 픽셀 unit 섞이지 않게 주의.
- **테스트 영향**: SnapLogicTests, DragSnapFlowTests 가 영향받을 수 있음. Phase 별로 테스트 통과 확인.
- **성능**: 64조각 게임에서 매 OnDrag 마다 인접 조각 검사 시 O(n²) 우려. Row/Col 인덱스 dictionary 로 O(1) 조회.
- **메모리 갱신**: 완료 후 `project_adult_pivot.md` 의 "결정된 디자인" 섹션에 인접 짝맞춤 활성화 한 줄 추가.

## 참고 — 현재 코드 진입점

- 짝맞춤 검출 호출 위치: `PuzzlePiece.OnPointerUp` (현재 파일 line 174 부근)
- 현재 TrySnap: `PuzzlePiece.TrySnap` (line 89)
- PlaySnapJuice: `PuzzlePiece.PlaySnapJuice` (line 112) — 효과음 + VFX
- Row/Col 필드: `PuzzlePiece.Row`, `PuzzlePiece.Col` (NonSerialized, PuzzleBoard.Build 에서 세팅)
