# casual-game-agent 스크립트

`document/casual-game-agent-design.md` 4번 문서(에이전트 오케스트레이션)의 기계적인 절차를 스크립트로 옮긴 것.
장르 전용 코드 작성(LevelDataSO/게임보드/Importer)은 여전히 Claude Code 가 직접 해야 함 — 이 스크립트들은 그 앞뒤의 반복 작업만 대신한다.

## bootstrap_project.sh

가장 최근에 완성된 프로젝트를 새 장르 프로젝트로 복제 + 네임스페이스/asmdef/productName 일괄 치환.

```bash
./bootstrap_project.sh <source_project_dir> <dest_project_dir> <OldNamespace> <NewNamespace>

# 예
./bootstrap_project.sh ~/casual-game-agent/SlidingPuzzle ~/casual-game-agent/MatchThree SlidingPuzzle MatchThree
```

끝나면 장르 전용 작업 목록(이전 게임보드/Importer/씬/에셋 삭제, 신규 코드 3종 작성)을 안내 메시지로 출력한다.

## verify_project.sh

컴파일 → (선택) Importer/SceneSetupTool/LocalizationSetupTool/LocalizationStringTableTool 실행 → EditMode 테스트 → **PlayMode 테스트**까지 배치모드로 자동 검증.

```bash
./verify_project.sh <project_dir> [executeMethod ...]

# 예
./verify_project.sh ~/casual-game-agent/MatchThree \
  MatchThree.EditorTools.MatchThreeImporter.ImportLevels \
  MatchThree.EditorTools.SceneSetupTool.SetupAllScenes \
  MatchThree.EditorTools.LocalizationSetupTool.SetupLocalization \
  MatchThree.EditorTools.LocalizationStringTableTool.PopulateStringTables
```

**주의**: 대상 프로젝트를 Unity 에디터로 열어둔 채로 실행하면 프로젝트 락 충돌로 실패한다 — 반드시 Unity 를 먼저 닫을 것.

### PlayMode E2E 테스트 (디스플레이 없이도 동작)

`Assets/Tests/PlayMode/`에 `SceneTransitionTests.cs`(5씬 전환 4종)와 `GameplayFlowTests.cs`(실제 탭→판정 흐름)를 둔다. 핵심은 `target.GetComponent<IPointerClickHandler>().OnPointerClick(...)`을 GameObject에 **직접** 호출하는 것 — EventSystem 레이캐스팅을 거치지 않으므로 실제 화면 렌더링이 없어도(`-nographics`) 동작한다. 신규 장르 게임보드에는 다음 패턴으로 `Debug*` 테스트 지원 메서드를 추가해야 이 테스트들이 성립한다:

- 셔플/랜덤 생성이 있는 장르 → "지금 상태 기준 유효한 탭 대상"을 질의하는 메서드 (예: `SlidingBoard.DebugGetAdjacentPieceIndex()`, `MatchBoard.DebugFindMatchingPair()`) — 특정 시드를 강제하려 하지 말 것, 매번 달라지는 상태에서도 성립하는 질의를 만들 것
- 좌표 기반 판정 장르 → 정답 좌표를 화면 좌표로 역산하는 메서드 (예: `DiffGameBoard.DebugGetScreenPositionForRegion(index)`)
- `SceneLoader.UseFader = false`를 `[UnitySetUp]`/`[UnityTearDown]`으로 켰다 끄기 — 페이드 애니메이션 대기 없이 즉시 씬 전환

이 패턴은 원래 jigsaw(`MyGames`)의 `DragSnapFlowTests.cs`에서 가져온 것.

## 이 스크립트로 커버 안 되는 것 (사람 확인 필수 — 5번 문서 참고)

- 화면 레이아웃 (노치 클리핑, 텍스트 잘림 등)
- 오디오 재생 여부(실제 소리가 나는지 — PlayMode 테스트는 `AudioManager` 호출 여부까지만 확인, 청취 확인은 못 함)
- 허브 화면(LevelSelect/Gallery) UI 완성도가 레벨 수 규모에 맞는지
- 애니메이션/트윈의 시각적 완성도(속도감, 이징 느낌) — PlayMode 테스트는 "상태가 바뀌었는가"만 확인하고 "예쁘게 바뀌는가"는 확인 못 함

이건 배치모드가 아니라 Unity Hub 로 직접 열어서 플레이해봐야 확인 가능.
