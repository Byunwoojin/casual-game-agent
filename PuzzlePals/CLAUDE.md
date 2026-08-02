# Puzzle Pals — Claude Code Project Guide

## Project Overview

Unity 6.4 (6000.4.2f1) 2D URP 성인 여성 타겟 퍼즐 게임 (성인 피벗 v0.5).
정사각 PNG 16장을 직소 조각으로 풀어 완성하는 캐주얼 퍼즐.
16레벨 (E3/M6/H4/VH3, 조각 36=6×6 또는 64=8×8 — 정사각 셀 보장 위해 제곱수만), 광고 기반 (IAP 폐기).
※ 구 100레벨 유아용 명세는 stale — 신규 기준은 `document/puzzle_adult.md` + 이 파일.

- **GDD**: `document/puzzle_pals_gdd.md`
- **테스트 명세**: `document/puzzle_pals_test_spec.md`
- **진행 현황**: `document/progress_report.md`

## Architecture

```
Assets/Scripts/
  Core/         GameManager, LevelManager, SceneLoader
  Data/         LevelDataSO (ScriptableObject), LevelDataValidator
  Puzzle/       PuzzleBoard, PuzzlePiece, SnapZone, PuzzleCompleteChecker, PieceData
  Gameplay/     GameplaySceneController, GameplayLoader, CharacterCompleteAnimation
  Systems/      HintManager, SaveManager, AdManager, AudioManager, VFXManager, IAPManager
  Interfaces/   IAdProvider, IUIManager
  UI/           UIManager, LevelSelectController, GameplayHeaderUI, SceneFader,
                SettingsPopupUI, ShopPopupUI, SafeAreaFitter, LetterboxFitter
  Editor/       SceneSetupTool, LevelAssetGenerator, 기타 에디터 도구들

Assets/Tests/
  EditMode/     LevelDataTests, PieceSplitTests, SnapLogicTests,
                LevelCompleteTests, HintManagerTests, SaveManagerTests
  PlayMode/     DragSnapFlowTests, SceneTransitionTests, AdTriggerTests

Assets/Scenes/  MainMenu, LevelSelect, Gameplay, Complete
```

## Key Design Patterns

- **싱글톤**: GameManager, LevelManager, AdManager, AudioManager, VFXManager (DontDestroyOnLoad)
- **의존성 주입**: HintManager에 IUIManager 주입, AdManager에 IAdProvider 주입
- **시간 주입**: HintManager.SetTimeProvider(Func<DateTime>) — EditMode 테스트에서 시각 고정
- **SceneLoader**: SceneFader 경유 페이드 전환 (UseFader=false로 테스트 시 즉시 전환)
- **PuzzlePiece.AssignedSnapZone**: public 필드 (Unity 직렬화). property → public field로 변경해야 씬 직렬화됨

## Test Framework

43개 테스트 (EditMode 32 + PlayMode 11) — 전부 통과 상태.

- EditMode: NUnit, `new GameObject().AddComponent<T>()`로 MonoBehaviour 테스트
- PlayMode: `[UnityTest]`, `SceneManager.LoadSceneAsync()`로 씬 전환 테스트
- SceneTransitionTests는 `SceneLoader.UseFader = false`로 즉시 로드
- DragSnapFlowTests는 `SimulatePointerDown/Up/Drag` 헬퍼 사용
- AdTriggerTests는 MockAdProvider 인라인 Mock 사용

## Packages

DOTween (DG.Tweening), Unity Localization, Unity Purchasing (IAP),
LevelPlay (IronSource 광고), Unity Input System, 2D URP

## Build & Run

- Build Settings에 4개 씬 등록 필요 (MainMenu:0, LevelSelect:1, Gameplay:2, Complete:3)
- `PuzzlePals > Setup All Scenes` 메뉴로 씬 자동 세팅
- Test Runner: Window > General > Test Runner → EditMode/PlayMode 탭
- Active Input Handling: "Both" (Player Settings)

## Important Lessons (from development)

- Unity 6.4에서 FindFirstObjectByType → FindAnyObjectByType (deprecated)
- FindObjectsByType(FindObjectsSortMode) → FindObjectsByType(FindObjectsInactive) (deprecated)
- Assert.AreNotEqual에는 delta 오버로드 없음 → Assert.That(Is.Not.EqualTo.Within) 사용
- [field: SerializeField] auto-property는 Unity 씬 직렬화 dirty 마킹 문제 있음 → public 필드 사용
- Button.onClick AddIntPersistentListener는 불안정 → 무인자 래퍼 메서드(LoadLevel0) 사용
- SceneSetupTool 반복 실행 시 GameObject 누적 방지 → RemoveAllPuzzlePieces/SnapZones 타입 기반 정리
- EditorSceneManager API는 Play Mode에서 호출 불가 → GuardEditMode() 가드 필수
- GameObject.Find()는 비활성 오브젝트 못 찾음 → FindObjectsByType(FindObjectsInactive.Include) + 이름 비교
- StandaloneInputModule은 새 Input System과 충돌 → Player Settings "Both"로 해결
- 씬 변경 팝업(Save/Don't Save)에서 Don't Save 선택해야 Setup된 씬 유지
- CanvasScaler.matchWidthOrHeight = 0f (Width 고정) 통일 — 1080×1920 ref 기준. 0.5f (평균) 는 길쭉한 폰(1080×2340 등)에서 가상 가로폭이 1080보다 좁아져 그리드/카드가 viewport 밖으로 잘림. 풀스크린 그리드 짤 때 cellSize × cols + spacing × (cols-1) 합이 가용폭에 들어가는지 산수로 검산
- 직소 pieceCount 는 **제곱수만** (36=6×6, 64=8×8) — JigsawGrid.DetermineGrid 가 cols×rows=pieceCount 정확 분해 + 종횡비 근사라서 32 같은 비제곱수는 정사각 이미지에서도 4×8 직사각 셀이 나옴 → 조각 모양 불일치 + 탭 크기 비대칭 (tabHeight = edgeLen × 0.25 라 변별로 다름)
- 사용자 노출 UI 텍스트는 **반드시 다국어 키** (`UI` StringTable). 코드에선 `DesignTokens.BuildLocalizedText(parent, name, entryKey, fallback, ...)` 또는 `LocalizedTextBinder` 명시 부착. format 텍스트(`"Level {0} 완성!"` 등)는 `new LocalizedString("UI", key) { Arguments = ... }` + `StringChanged` 이벤트로 처리. 새 키 추가 시 `Editor/LocalizationStringTableTool.cs` 의 `UI_ROWS` 배열에 ko/en/ja/zh-Hans 4개 언어 입력 → 메뉴 `PuzzlePals → Populate String Tables` 실행 → StringTable 갱신. Locale 4개 (`ko`/`en`/`ja`/`zh-Hans`) 셋업은 `Setup Localization` 메뉴로 완료됨
- **프로젝트를 몇 달 방치했다가 다시 열 때** — Gameplay 씬에서 퍼즐 조각이 (Scene 뷰엔 정상 출력되는데) Game 뷰에서만 안 보이는 등 stale 참조 증상이 나타날 수 있음. 카메라/Canvas 설정 자체는 멀쩡한데도 발생 — 원인 파고들기 전에 `PuzzlePals → Wire Real Levels Pipeline` 부터 재실행해서 재현되는지 먼저 확인 (2026-08-02 확인: 재실행으로 해결됨)

## Commands

```bash
# 테스트 결과 확인 (Unity 밖에서)
grep -c "m_Name: PuzzlePiece" Assets/Scenes/Gameplay.unity   # PuzzlePiece 개수 확인
grep "AssignedSnapZone" Assets/Scenes/Gameplay.unity          # 스냅존 할당 확인
grep "m_MethodName" Assets/Scenes/LevelSelect.unity           # OnClick 메서드 확인
```

## What to NOT do

- MonoBehaviour의 직렬화 필드를 C# property로 만들지 않기 (씬 저장 안 됨)
- SceneSetupTool에서 RemoveByName만 쓰지 않기 (이름 변경 시 누적됨, 타입 기반 제거 병행)
- EditMode 테스트에서 using System + using UnityEngine 동시 사용 시 Object 모호성 → UnityEngine.Object 명시
- PlayMode에서 씬 변경 테스트 후 씬 저장하지 않기 (Don't Save)
- CanvasScaler.matchWidthOrHeight = 0.5f 쓰지 않기 (세로 모바일은 항상 0f / Width 고정)
- 사용자 노출 UI 에 `DesignTokens.BuildText` 직접 호출 금지 (다국어 미지원) → `BuildLocalizedText` 또는 `LocalizedTextBinder` 사용. 새 텍스트 추가 시 `UI_ROWS` 에도 4개 언어 등록 누락 금지
