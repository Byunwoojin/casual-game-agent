# Puzzle Pals — Test Specification
> TDD 기반 개발을 위한 테스트 명세서
> Unity Test Runner (EditMode + PlayMode)

---

## 테스트 구조

```
Assets/
  Tests/
    EditMode/
      LevelDataTests.cs         # 레벨 데이터 로드/검증
      PieceSplitTests.cs        # 조각 자동 분할 계산
      SnapLogicTests.cs         # 스냅 판정 로직
      HintManagerTests.cs       # 힌트 충전/사용 로직
      SaveManagerTests.cs       # 진행도 저장/불러오기
      LevelCompleteTests.cs     # 레벨 클리어 판정
    PlayMode/
      DragSnapFlowTests.cs      # 드래그 → 스냅 전체 흐름
      SceneTransitionTests.cs   # 씬 전환
      AdTriggerTests.cs         # 광고 호출 타이밍
```

---

## EditMode Tests

---

### 1. LevelDataTests.cs
> LevelDataSO 데이터 로드 및 유효성 검증

```
TC-LD-001 레벨 번호가 1~100 범위 안에 있어야 한다
  Given: LevelDataSO 파일
  When: levelNumber 값 확인
  Then: 1 <= levelNumber <= 100

TC-LD-002 characterSprite가 null이 아니어야 한다
  Given: LevelDataSO 파일
  When: characterSprite 값 확인
  Then: characterSprite != null

TC-LD-003 pieceCount가 난이도 커브 범위 안에 있어야 한다
  Given: LevelDataSO 파일
  When: levelNumber에 따른 pieceCount 확인
  Then:
    레벨 1~10   → pieceCount: 2~3
    레벨 11~25  → pieceCount: 3~4
    레벨 26~45  → pieceCount: 4~5
    레벨 46~60  → pieceCount: 5~6
    레벨 61~80  → pieceCount: 6~7
    레벨 81~100 → pieceCount: 7~8

TC-LD-004 snapThreshold가 난이도 커브 값과 일치해야 한다
  Given: LevelDataSO 파일
  When: levelNumber에 따른 snapThreshold 확인
  Then:
    레벨 1~10   → 1.8f
    레벨 11~25  → 1.4f
    레벨 26~45  → 1.2f
    레벨 46~60  → 1.0f
    레벨 61~80  → 0.9f
    레벨 81~100 → 0.8f

TC-LD-005 characterNameKey가 비어있지 않아야 한다
  Given: LevelDataSO 파일
  When: characterNameKey 값 확인
  Then: characterNameKey != ""
```

---

### 2. PieceSplitTests.cs
> 원본 이미지 → 조각 자동 분할 계산 로직

```
TC-PS-001 2조각 분할 시 조각이 정확히 2개 생성된다
  Given: pieceCount = 2, 512x512 스프라이트
  When: PuzzleBoard.SplitSprite() 호출
  Then: 반환된 조각 배열 길이 == 2

TC-PS-002 4조각 분할 시 2x2 그리드로 분할된다
  Given: pieceCount = 4, 512x512 스프라이트
  When: PuzzleBoard.SplitSprite() 호출
  Then:
    조각 개수 == 4
    각 조각 크기 == 256x256

TC-PS-003 분할된 조각들의 위치 합이 원본 이미지를 커버한다
  Given: pieceCount = 4, 512x512 스프라이트
  When: PuzzleBoard.SplitSprite() 호출
  Then: 조각 4개의 Rect 합 == 원본 Rect (512x512, 겹침/빈공간 없음)

TC-PS-004 스냅존 위치가 원본 이미지 기준 좌표와 일치한다
  Given: pieceCount = 2, 분할된 조각
  When: PuzzleBoard.GenerateSnapZones() 호출
  Then: snapZone[0].position == 조각[0]의 원본 위치
        snapZone[1].position == 조각[1]의 원본 위치

TC-PS-005 조각 초기 배치가 화면 하단 1/3 안에 있다
  Given: 분할된 조각들
  When: PuzzleBoard.PlacePiecesAtBottom() 호출
  Then: 모든 조각의 y좌표 < 화면 높이의 1/3 지점

TC-PS-006 조각 초기 배치 시 서로 겹치지 않는다
  Given: pieceCount = 8, 분할된 조각들
  When: PuzzleBoard.PlacePiecesAtBottom() 호출
  Then: 어떤 두 조각의 Rect도 Overlap되지 않음
```

---

### 3. SnapLogicTests.cs
> 조각 스냅 판정 로직

```
TC-SL-001 조각이 스냅존 임계값 안에 있으면 스냅된다
  Given: snapThreshold = 1.8f, 조각 위치 = 스냅존으로부터 1.5f 거리
  When: PuzzlePiece.TrySnap() 호출
  Then: isSnapped == true

TC-SL-002 조각이 스냅존 임계값 밖에 있으면 스냅되지 않는다
  Given: snapThreshold = 1.8f, 조각 위치 = 스냅존으로부터 2.0f 거리
  When: PuzzlePiece.TrySnap() 호출
  Then: isSnapped == false

TC-SL-003 스냅 실패 시 조각이 원래 위치로 돌아간다
  Given: 조각이 스냅존 밖에 놓임
  When: PuzzlePiece.TrySnap() 호출 후 실패
  Then: 조각 position == 드래그 시작 전 position

TC-SL-004 이미 점유된 스냅존에는 스냅되지 않는다
  Given: snapZone.isOccupied == true
  When: 다른 조각으로 TrySnap() 호출
  Then: isSnapped == false

TC-SL-005 스냅 성공 시 조각 위치가 스냅존 위치와 정확히 일치한다
  Given: 조각이 스냅존 임계값 안에 있음
  When: PuzzlePiece.TrySnap() 호출 성공
  Then: 조각 position == snapZone.position (오차 0.01f 이내)
```

---

### 4. HintManagerTests.cs
> 힌트 충전/사용/광고 로직

```
TC-HM-001 힌트가 있을 때 사용하면 hintCount가 1 감소한다
  Given: hintCount = 2
  When: HintManager.UseHint() 호출
  Then: hintCount == 1

TC-HM-002 힌트가 0개일 때 UseHint()를 호출하면 팝업이 표시된다
  Given: hintCount = 0
  When: HintManager.UseHint() 호출
  Then: UIManager.ShowHintPopup() 호출됨

TC-HM-003 30분 경과 시 힌트가 1개 자동 충전된다
  Given: hintCount = 0, lastRechargeTime = 31분 전
  When: HintManager.Update() 호출
  Then: hintCount == 1

TC-HM-004 자동 충전은 최대 3개를 초과하지 않는다
  Given: hintCount = 3, lastRechargeTime = 60분 전
  When: HintManager.Update() 호출
  Then: hintCount == 3 (초과 충전 없음)

TC-HM-005 오프라인 90분 경과 시 힌트 3개가 한번에 충전된다
  Given: hintCount = 0, lastRechargeTime = 90분 전
  When: HintManager.Update() 호출
  Then: hintCount == 3

TC-HM-006 하루 광고 3회 이하일 때 OnAdWatched() 호출 시 힌트가 증가한다
  Given: adWatchedToday = 2, hintCount = 0
  When: HintManager.OnAdWatched() 호출
  Then: hintCount == 1, adWatchedToday == 3

TC-HM-007 하루 광고 3회 초과 시 CanWatchAd()가 false를 반환한다
  Given: adWatchedToday = 3
  When: HintManager.CanWatchAd() 호출
  Then: false 반환

TC-HM-008 자정이 지나면 adWatchedToday가 0으로 리셋된다
  Given: adWatchedToday = 3, lastAdDate = 어제 날짜
  When: HintManager.Update() 호출
  Then: adWatchedToday == 0
```

---

### 5. SaveManagerTests.cs
> 진행도 저장 및 불러오기

```
TC-SM-001 레벨 클리어 후 SaveManager에 저장된다
  Given: levelIndex = 0
  When: SaveManager.MarkLevelComplete(0) 호출
  Then: SaveManager.IsLevelComplete(0) == true

TC-SM-002 저장 후 앱 재시작 시 진행도가 복구된다
  Given: 레벨 5 클리어 후 PlayerPrefs 저장
  When: SaveManager 재초기화
  Then: IsLevelComplete(4) == true

TC-SM-003 클리어하지 않은 레벨은 IsLevelComplete()가 false를 반환한다
  Given: 초기 상태
  When: SaveManager.IsLevelComplete(0) 호출
  Then: false 반환

TC-SM-004 힌트 데이터가 앱 재시작 후 복구된다
  Given: hintCount = 2 저장
  When: HintManager 재초기화
  Then: hintCount == 2
```

---

### 6. LevelCompleteTests.cs
> 레벨 클리어 판정

```
TC-LC-001 모든 조각이 스냅되면 레벨 클리어로 판정된다
  Given: PuzzlePiece 3개, 전부 isSnapped = true
  When: PuzzleCompleteChecker.Check() 호출
  Then: OnPuzzleComplete() 호출됨

TC-LC-002 조각 1개라도 스냅 안 되면 클리어로 판정되지 않는다
  Given: PuzzlePiece 3개 중 1개 isSnapped = false
  When: PuzzleCompleteChecker.Check() 호출
  Then: OnPuzzleComplete() 호출되지 않음

TC-LC-003 레벨 클리어 시 GameState가 Complete로 변경된다
  Given: GameState = Playing
  When: OnPuzzleComplete() 호출
  Then: GameManager.State == GameState.Complete

TC-LC-004 레벨 클리어 시 AdManager.OnLevelComplete()가 호출된다
  Given: roundsSinceAd = 4
  When: OnPuzzleComplete() 호출
  Then: AdManager.OnLevelComplete() 호출됨
```

---

## PlayMode Tests

---

### 7. DragSnapFlowTests.cs
> 드래그 → 스냅 전체 흐름

```
TC-DS-001 조각을 드래그하면 손가락을 따라 이동한다
  Given: Gameplay 씬 로드, 조각 존재
  When: 조각 위치에서 PointerDown → PointerMove
  Then: 조각 position이 터치 위치를 따라감

TC-DS-002 스냅존 근처에 드래그 후 손을 떼면 스냅된다
  Given: snapThreshold = 1.8f
  When: 조각을 스냅존 1.5f 거리에서 PointerUp
  Then: 조각 position == snapZone.position

TC-DS-003 스냅존 밖에서 손을 떼면 조각이 원래 위치로 돌아간다
  Given: snapThreshold = 1.8f
  When: 조각을 스냅존 2.5f 거리에서 PointerUp
  Then: 조각 position == 드래그 시작 전 position

TC-DS-004 마지막 조각 스냅 후 클리어 UI가 표시된다
  Given: 조각 2개, 1개 이미 스냅됨
  When: 나머지 1개 스냅 완료
  Then: CompleteUI 활성화됨 (1.5초 딜레이 후)
```

---

### 8. SceneTransitionTests.cs
> 씬 전환

```
TC-ST-001 MainMenu에서 Play 버튼 클릭 시 LevelSelect 씬으로 이동한다
  Given: MainMenu 씬 로드
  When: Play 버튼 클릭
  Then: 현재 씬 == "LevelSelect"

TC-ST-002 LevelSelect에서 레벨 버튼 클릭 시 Gameplay 씬으로 이동한다
  Given: LevelSelect 씬, 레벨 1 버튼
  When: 레벨 1 버튼 클릭
  Then: 현재 씬 == "Gameplay", LevelManager.currentLevelIndex == 0

TC-ST-003 Gameplay에서 홈 버튼 클릭 시 MainMenu로 이동한다
  Given: Gameplay 씬 로드
  When: 홈 버튼 클릭
  Then: 현재 씬 == "MainMenu"

TC-ST-004 Complete 씬에서 다음 버튼 클릭 시 다음 레벨 Gameplay로 이동한다
  Given: Complete 씬, currentLevelIndex = 0
  When: 다음 버튼 클릭
  Then: 현재 씬 == "Gameplay", LevelManager.currentLevelIndex == 1
```

---

### 9. AdTriggerTests.cs
> 광고 호출 타이밍

```
TC-AD-001 5번째 레벨 클리어 시 인터스티셜 광고가 호출된다
  Given: roundsSinceAd = 4
  When: AdManager.OnLevelComplete() 호출
  Then: Advertisement.Show("Interstitial_Android") 호출됨

TC-AD-002 5번 미만 클리어 시 광고가 호출되지 않는다
  Given: roundsSinceAd = 3
  When: AdManager.OnLevelComplete() 호출
  Then: Advertisement.Show() 호출되지 않음

TC-AD-003 광고 표시 후 roundsSinceAd가 0으로 리셋된다
  Given: roundsSinceAd = 5
  When: AdManager.OnLevelComplete() → 광고 표시
  Then: roundsSinceAd == 0
```

---

## 테스트 우선순위

```
Phase 1 — 핵심 로직 (개발 시작 전 작성)
  TC-PS-001 ~ TC-PS-006  조각 분할
  TC-SL-001 ~ TC-SL-005  스냅 판정
  TC-LC-001 ~ TC-LC-004  레벨 클리어

Phase 2 — 시스템 로직 (힌트/저장 구현 전 작성)
  TC-HM-001 ~ TC-HM-008  힌트 매니저
  TC-SM-001 ~ TC-SM-004  저장 매니저

Phase 3 — 통합 테스트 (씬 구성 후 작성)
  TC-DS-001 ~ TC-DS-004  드래그/스냅 흐름
  TC-ST-001 ~ TC-ST-004  씬 전환
  TC-AD-001 ~ TC-AD-003  광고 타이밍
```

---

## 총 테스트 케이스 수

| 파일 | 케이스 수 |
|------|---------|
| LevelDataTests | 5 |
| PieceSplitTests | 6 |
| SnapLogicTests | 5 |
| HintManagerTests | 8 |
| SaveManagerTests | 4 |
| LevelCompleteTests | 4 |
| DragSnapFlowTests | 4 |
| SceneTransitionTests | 4 |
| AdTriggerTests | 3 |
| **합계** | **43** |
