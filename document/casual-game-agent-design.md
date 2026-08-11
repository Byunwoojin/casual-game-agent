# 캐주얼 게임 에이전트 설계 문서

## 배경 및 목적

직소 퍼즐 게임을 먼저 구현했고, 이제 두 번째 게임(틀린그림찾기 또는 슬라이딩 퍼즐)을 만들려고 한다. 최종 목표는 게임을 하나씩 개별로 만드는 게 아니라, 여러 캐주얼 퍼즐 게임(직소, 틀린그림찾기, 슬라이딩 퍼즐 등)을 공통 구조 위에서 생성할 수 있는 "게임 에이전트"를 만드는 것이다.

바로 에이전트화하지 않고, 두 번째 게임을 만드는 과정에서 직소 퍼즐 코드와 공유 가능한 구조가 무엇인지 검증한 뒤, 그 결과를 바탕으로 공통 스펙 문서를 확정한다.

## 진행 순서

1. **(완료)** 직소 퍼즐 구현
2. **(완료)** 두 번째 게임 구현 — **틀린그림찾기(Spot the Difference), 2026-08-02 완성**. `~/casual-game-agent/SpotTheDifference/` 별도 프로젝트. 게임플레이/힌트/광고/완료 흐름/LevelSelect/Gallery/Settings/다국어/EditMode 테스트/오디오까지 전부 구현 완료. 직소 퍼즐 코드에서 그대로 재사용 가능한 부분과 새로 만들어야 하는 부분을 아래에 실제 구현 기준으로 기록함
3. **(완료)** Core Spec / Game Rule Spec 확정, 이어서 **세 번째 게임(슬라이딩 퍼즐) 구현으로 가설 검증 완료** (2026-08-02). 3~5번 문서(에셋 파이프라인/오케스트레이션/검증 체크리스트)도 3개 게임 실사례 기반으로 작성 완료
4. **(완료)** 네 번째 게임(메모리 매칭) 구현 + 스크립트 자동화(`bootstrap_project.sh`/`verify_project.sh`) 실전 검증, 이어서 실제 플레이 피드백 기반 마무리 다듬기까지 완료 (2026-08-03) — 완료 화면 카드 진열, 힌트 짝-동시-공개, 난이도 간격 확대, 레벨 썸네일 스포일러 제거
5. **(다음 단계)** 다섯 번째 게임 후보 선정 또는 이 문서(1~5번)를 "확정 스펙"으로 동결하고 실제 배포/서비스 단계로 전환 — 4개 장르(드래그/탭+히트테스트/탭+그리드스왑/탭+짝비교)로 인터랙션 레이어 비추상화 원칙이 반복 검증됐고, Core Spec 공용 레이어 재사용도 4회 연속 확인됨

**순서 원칙**: 인터페이스를 먼저 설계하지 않는다 — 구현체가 1개뿐일 때 설계한 인터페이스는 그 구현체에 과적합되기 쉽다. 두 번째 게임을 (거의) 독립적으로 만들어서 실제 겹치는 지점을 데이터로 확보한 뒤에만 공통 구조로 승격한다.

**현재 상태**: 1~5번 문서 전부 4개 게임(직소/틀린그림찾기/슬라이딩 퍼즐/메모리 매칭) 실사례로 채워짐. 매 게임마다 "구현 → 배치모드 검증 → 실제 플레이 피드백 → 스펙 갱신" 루프를 반복하며 문서를 갱신하는 방식이 정착됨 — 새 장르 착수 시에도 이 루프를 그대로 따르면 됨.

---

## 1. Core Spec (게임 공통 아키텍처 문서)

모든 게임이 공유해야 하는 기술 표준. 게임 타입이 늘어나도 이 레이어는 흔들리지 않아야 함.

- **렌더링 레이어**: 어떤 방식으로 그리는지 (Canvas / DOM+CSS / WebGL 등), 좌표계 정의
- **상태 관리 모델**: 정답 상태(target state) vs 현재 상태(current state)를 어떻게 표현하는지 — 그리드, 좌표 배열, 매칭 테이블 등 공통 데이터 구조
- **인터랙션 이벤트 모델**: 드래그/스왑/탭/클릭 등 입력을 게임 로직과 분리해서 처리하는 공통 인터페이스
- **에셋 규격**: 입력 이미지 해상도, 포맷, 비율 제약
- **난이도 파라미터 인터페이스**: 조각 수, 시간 제한, 힌트 횟수 등 게임 종류가 달라도 공통으로 노출할 파라미터 형태

> Claude Code 작업 지침: 두 번째 게임을 만들면서 직소 퍼즐과 겹치는 부분이 나오면 이 섹션에 실제 구현 기준으로 기록할 것. 아직 확정 아님 — 초안 상태로 계속 갱신.

### 두 게임 완성 후 확정된 Core Spec findings (2026-08-02)

- **완료 판정 로직 — 확정**: `progress (foundCount/total) + onComplete 이벤트` 한 쌍이 실제로 두 게임 모두에 그대로 들어맞았다. 단, **트리거 시점은 인터랙션 모델에 종속**된다는 점이 실제로 만들어보니 드러남 — 직소는 드래그가 매 프레임 연속으로 움직이므로 `PuzzleCompleteChecker`가 프레임 폴링으로 확인해야 하지만, 틀린그림찾기는 탭이 이산 이벤트라서 `DiffGameBoard.MarkFound()`가 탭 처리 시점에 동기적으로 `OnProgressChanged`/`OnCompleted`를 바로 발행함 (폴링 불필요). → Core Spec에 넣을 건 "progress+onComplete 인터페이스" 자체이지, "프레임 폴링"이라는 구현 방식은 아님. 호출 시점(매 프레임 vs 상태변경 시점)은 게임 룰 스펙에 위임.
- **힌트/보조 로직 — 확정, 검증된 패턴**: `HintManager.OnHintRequested` 이벤트 + 호출 측이 명시적으로 `ConsumeHint()`를 호출하는 2단계 분리 구조로 재설계 후, **HintManager 코드 자체는 두 게임 사이에 단 한 줄도 안 바뀜** (파일 복사+이름 변경만). 각 씬 컨트롤러가 `OnHintRequested → 장르별 힌트 효과 실행 → 효과가 실제로 뭔가 찾았을 때만 ConsumeHint()` 를 조합. (직소는 `AutoSnap()`, 틀린그림찾기는 `DiffGameBoard.RequestHint()`가 미발견 영역 하나를 반짝임 처리 후 성공 여부 반환.) → **Core Spec 확정 항목**: 힌트 재화 관리(일일 리셋/스톡파일/rewarded 적립)와 힌트의 실제 효과는 반드시 이벤트로 분리. 하나의 메서드에 재화 차감 + 효과를 같이 넣지 않는다.
- **인터랙션 이벤트 모델 — 공유 안 하는 게 맞다는 게 확정됨**: 직소(드래그+인접판정) vs 틀린그림찾기(탭+좌표 히트테스트, `DiffRegion.IsHit`)는 코드를 단 한 줄도 공유하지 않았고, 억지로 공유 인터페이스를 만들려는 시도조차 하지 않는 게 맞았다. Core Spec은 이 레이어를 추상화하지 않는다 — 게임 룰 스펙에 완전히 위임.
- **공용 UI/시스템 레이어 — 가장 큰 발견**: 실제로 만들어보니 "겹치는 부분"의 대부분은 게임 로직이 아니라 **UI·시스템 레이어**였다. `SceneLoader`/`BackButtonHandler`/`GameManager`/`SaveManager`/`AdManager`(+ `IAdProvider`/`LevelPlayAdProvider`)/`AudioManager`(절차적 합성 기법 포함)/`HintManager`는 파일 복사 + 네임스페이스·클래스명 변경만으로 100% 재사용됐고, `PaletteUI`/`DesignTokens`/`LocalizedTextBinder`/`SafeAreaTopExtender`/`SettingsPopupUI`/`GalleryController` 패턴 등 UI 컴포넌트도 거의 그대로 재사용 가능했다. 실제 장르별로 새로 짠 코드는 **`LevelDataSO` 서브타입 1개 + 게임보드 컴포넌트 1개(`DiffGameBoard`) + 소스 데이터 임포터 1개(`DiffGameImporter`)** 뿐이었다. → 에이전트가 3번째 게임부터 실제로 "생성"해야 할 코드 범위는 이 3가지로 좁혀도 될 가능성이 높음.
  - 공용 컴포넌트 중 파라미터화가 필요했던 지점은 딱 하나: `PaletteUI.BuildPiecesBadge`가 `"조각"`을 하드코딩하고 있어서 `unit` 파라미터(기본값 유지, 호출 측에서 `"개"` 등으로 override)로 일반화함. 나머지는 손댈 필요 없었음.
- **주의 — 재사용 시 실수 발생 지점**: LevelSelect/Gallery 같은 "허브" 화면 컨트롤러를 파일 복사가 아니라 "직소와 같은 패턴으로 다시 작성"하는 방식으로 만들었더니, `SafeAreaTopExtender`(노치 대응)와 헤더의 설정(Settings) 진입점처럼 겉으로 잘 안 보이는 크로스커팅 요소를 빠뜨렸다. → **교훈**: Systems 레이어처럼 UI 허브 컨트롤러도 "패턴 참고해서 재작성"보다 "원본 파일 복사 후 리네임"이 안전하다. 특히 SafeArea 처리와 Settings 진입점은 눈에 잘 안 띄는 필수 계약이라 체크리스트화 필요.

### 씬 구조 & 내비게이션 시맨틱스 — 4개 게임 공통으로 고정된 계약

코드에는 있지만 이 문서엔 명시된 적 없던 결정이라 여기 확정해둠 (4개 게임 전부 동일하게 적용됨):

- **5개 씬 고정**: MainMenu / LevelSelect / Gameplay / Complete / Gallery. 장르가 바뀌어도 이 5개 구조 자체는 안 바뀜.
- **MainMenu = 최초 실행 전용 스플래시**, **LevelSelect = 실질적 허브**. `BackButtonHandler`/각 씬의 "메인" 탭·뒤로가기는 전부 **LevelSelect** 로 귀결되어야 하고, MainMenu 로 돌아가면 안 됨(재실행 시에만 다시 봄). SpotTheDifference 에서 Gallery 의 "메인" 탭이 실수로 MainMenu 를 가리켰다가 사용자가 잡아낸 적 있음 — 새 장르마다 GalleryController/LevelSelectController 의 nav 콜백이 이 규칙을 지키는지 재확인.
- **Unity 프로젝트 세팅 요구사항** (모두 부트스트랩 시 `ProjectSettings.asset` 복사로 자동 상속되지만, 밑바닥부터 새로 만들 경우 반드시 재설정해야 함):
  - Player Settings → Active Input Handling = **Both** (StandaloneInputModule 과 새 Input System 충돌 방지)
  - `Assets/Scripts/`, `Assets/Scripts/Editor/`, `Assets/Tests/EditMode/` 3개 asmdef(메인/Editor/테스트) — EditMode 테스트가 존재하려면 필수. asmdef 를 처음 도입하는 시점에 `versionDefines`(예: `LEVELPLAY_AVAILABLE`) 가 처음으로 활성화되면서 이전엔 컴파일된 적 없던 `#if` 코드 블록이 갑자기 컴파일 대상이 되는 경우가 있었음(SpotTheDifference 에서 `Unity.LevelPlay` 참조 누락으로 실제 발생) — asmdef 를 새로 추가할 때는 참조 목록을 소스 프로젝트와 정확히 대조할 것.

### PlayMode E2E 테스트 — 디스플레이 없이도 실제 조작 흐름을 자동 검증하는 패턴 (4번째 게임에서 확정, 2026-08-03)

원래 "실제 조작감은 사람만 확인 가능"이라 여겼으나, jigsaw(`MyGames`)의 `DragSnapFlowTests.cs`가 이미 이걸 자동화하고 있었음을 재발견 — 4개 게임 전부에 이식 완료. **핵심 원리**: `target.GetComponent<IPointerClickHandler>().OnPointerClick(new PointerEventData(EventSystem.current) { ... })`처럼 인터페이스 메서드를 GameObject에 **직접 호출**하면 `EventSystem`의 레이캐스팅을 완전히 우회하므로, 실제 화면 렌더링/Canvas 히트테스트가 전혀 필요 없다 → `-batchmode -nographics -runTests -testPlatform PlayMode`로 완전 자동 실행 가능. 진짜로 자동화 불가능한 건 "화면이 실제로 예쁘게 보이는지"(시각적 완성도)뿐, "탭하면 상태가 올바르게 바뀌는지"(기능적 흐름)는 전부 자동화 대상.

- **씬 전환 4종은 장르 무관 고정 템플릿**: `SceneTransitionTests.cs` (TC_ST_001~004 — MainMenu Play→LevelSelect, LevelSelect 타일→Gameplay, Gameplay Back→**LevelSelect**(MainMenu 아님, 위 내비게이션 계약과 동일), Complete Next→다음 레벨). `[UnitySetUp]`에서 `SceneLoader.UseFader = false`로 페이드 대기 없이 즉시 전환.
- **장르별 `GameplayFlowTests.cs`는 매번 새로 작성해야 하지만, 공통 장애물은 "셔플/랜덤이라 정답 좌표를 미리 알 수 없다"는 것 하나뿐** — 해법은 게임보드에 **`Debug*` 테스트 지원 메서드**를 추가해서 "지금 상태 기준 유효한 탭 대상"을 런타임에 질의하는 것(정해진 시드를 강제하려 하지 말 것). 실증된 3가지 패턴:
  - 좌표 히트테스트 장르(SpotTheDifference): `DebugGetScreenPositionForRegion(index)` — 정답 좌표를 화면 좌표로 역산
  - 인접/스왑 장르(SlidingPuzzle): `DebugGetAdjacentPieceIndex()` / `DebugIsPieceAdjacentToBlank(i)` — "지금 유효한 수"를 질의 (인접 후보가 여러 개일 수 있어 특정 인덱스 하나만 반환하는 메서드로는 "비인접" 판정에 flaky 위험이 있었음 → 별도의 단일 인덱스 질의 메서드를 하나 더 추가해서 해결)
  - 관계 기반 장르(MemoryMatch): `DebugFindMatchingPair()` / `DebugPairIdAt(i)` — 짝 관계를 직접 질의
  - 클릭 판정 자체가 좌표를 안 보는 장르(SlidingPuzzle/MemoryMatch의 `TileClickTarget`/`CardClickTarget`처럼 `OnPointerClick`이 인자를 무시하는 경우)는 `PointerEventData`에 좌표를 채울 필요조차 없음 — GameObject 특정만 정확하면 됨
  - 이 컨벤션은 jigsaw의 `PuzzlePiece.SetSnappedForTest(true)` 선례와 동일한 원칙(비공개 필드/랜덤성 때문에 테스트가 불가능해지는 지점에 테스트 전용 public 메서드를 추가)의 연장선.
- **PlayMode 전용 asmdef는 EditMode와 다르다**: `includePlatforms`를 비워두고(Editor 전용 아님), `UnityEditor.TestRunner` 참조 없이 `UnityEngine.TestRunner` + 메인 어셈블리만 참조.

### Unity 프레임워크 특유의 결정 — LevelData 표현 방식

HTML 프로토타입 단계엔 없던, Unity ScriptableObject 특유의 결정. 지금 `LevelDataSO`는 단일 클래스에 `pieceCount`/`imageBucket`(직소 전용)과 `levelNumber`(공통)가 섞여 있음. 두 번째 게임 데이터 만들 때 선택지:
- (a) 완전히 별도 ScriptableObject 타입 생성 — **채택**. "실제로 겹치는 부분만 나중에 승격" 원칙과 일치. 지금 억지로 상속/공통 베이스 만들면 구현체 1개짜리 인터페이스와 같은 함정.
- (b) 공통 베이스 클래스 상속 — 보류 (3단계에서 겹치는 필드 확인되면 전환 고려)
- (c) 한 SO에 장르별 필드 계속 추가 — 기각 (LevelDataSO 이미 "[Legacy] 구버전 필드" 누적 중이라 반복하면 안 됨)

---

## 2. Game Rule Spec (게임 타입별 규칙 명세, 게임당 1개 문서)

### Game Rule Spec 체크리스트 (4개 게임에서 얻은 교훈을 모은 것 — 5번째 게임부터는 구현 전에 이 항목을 전부 채울 것)

지금까지는 이 항목들을 하나씩 빠뜨렸다가 실제 플레이 후에야 발견하는 패턴이 반복됐다. 다음 장르부터는 스펙 선작성 단계에서 아래를 전부 명시적으로 채워서, 같은 종류의 재작업을 줄인다.

1. **좌표/상태 표현**: 그리드 셀 vs 자유 좌표, 정답 상태를 어떤 자료구조로 표현하는지
2. **인터랙션 모델**: 드래그/탭+히트테스트/탭+스왑/탭+비교 중 어디에 속하는지 — Core Spec은 이 레이어를 추상화하지 않으므로 매번 새로 정의
3. **생성/셔플 알고리즘**: 항상 유효한(풀 수 있는) 상태를 어떻게 보장할지 — 가능하면 "사후 검증"이 아니라 "사전 보장"(예: 슬라이딩 퍼즐의 역산 셔플)으로 설계
4. **완료 판정 트리거**: 프레임 폴링(연속 인터랙션) vs 상태변경 시점 동기 판정(이산 인터랙션) — 인터랙션 모델에 종속
5. **힌트 — 무엇을 얼마나 보여줘야 실제로 도움이 되는지**: "미완료 요소 하나만 반짝"은 요소가 서로 독립적인 장르에서만 유효함. 요소 간 관계(짝, 인접 등)가 핵심인 장르는 관계 전체를 보여줘야 함(메모리 매칭에서 처음 발견). 항상 "자동 풀이는 아님" 원칙은 유지.
6. **난이도 파라미터**: 값 간격을 산술적으로 균등 분할(예 4,6,8)하지 말고 체감 난이도 곡선(대략 2배씩 증가, 예 4,8,12)으로 설계 — 균등 분할은 실제 플레이에서 "차이가 안 느껴진다"는 피드백으로 이어짐(메모리 매칭에서 확인)
7. **레벨 수 & 허브 UI 규모**: 레벨 수가 많으면(10개+) LevelSelect/Gallery 도 축소판이 아니라 직소 수준의 풀피델리티(Progress 카드, 층 구분 등)가 필요 — "레벨 3개짜리 축소판"을 그대로 복사하면 안 됨(슬라이딩 퍼즐에서 확인)
8. **"완성 이미지" 유무**: 이 장르에 완성된 단일 그림이 있는지 없는지. 있으면 Complete 화면/레벨 썸네일에 그 이미지를 보여주면 되지만, 없으면(카드 뒤집기처럼 복수 이미지로 구성된 장르) 대표 이미지 하나만 보여주는 기존 패턴을 그대로 쓰면 안 됨(메모리 매칭에서 확인)
9. **미리보기가 스포일러가 되는지**: 장르 자체가 "정보 은닉"이 핵심 메커니즘이면(카드 뒤집기, 틀린그림찾기 등) LevelSelect 썸네일/Complete 화면에 미리 이미지를 보여주는 게 스포일러가 되지 않는지 확인. 스포일러라면 진행 전(LevelSelect)엔 텍스트만, 이미 끝난 후(Gallery/Complete)엔 이미지를 보여주는 식으로 시점을 구분
10. **에셋 형태**: 원본 이미지 1장 재사용인지, 복수 이미지(배열) 필요인지 — `LevelDataSO` 필드가 단일 `Sprite` 냐 `Sprite[]` 냐를 여기서 미리 결정

### 직소 퍼즐 (완료된 게임 — 여기서 항목을 역으로 채워서 템플릿 검증용으로 사용)
- **조각 분할 알고리즘**: `JigsawGrid.DetermineGrid` — 이미지 종횡비 + pieceCount(제곱수만, 36/64)로 cols×rows 결정. 정사각 PNG 전제라 셀도 정사각으로 보장됨
- **스냅 판정 거리(허용 오차)**: 정답 스냅 = `cellW × 0.35`. 인접 조각 짝맞춤 = `cellW × 0.3` (별도 tolerance)
- **회전 허용 여부**: 없음 (직소는 회전 없음 — 결정된 디자인)
- **완료 판정 로직**: 매 프레임 폴링, 전체 조각 `IsSnapped` 전수 확인
- **힌트/보조 로직**: 미스냅 조각 중 랜덤 1개 자동 스냅 (`HintManager.UseHint` → `PuzzlePiece.AutoSnap`)

### 틀린그림찾기 Spot the Difference (완료, 2026-08-02)
- **좌표 정의 방식**: 그리드 셀 개념 없음. `DiffRegion { x, y, width, height, centerX, centerY }` — 원본 이미지 픽셀 좌표(좌상단 기준). 히트테스트는 원형이 아니라 **중심점 기준 축정렬 사각형(AABB)** — `DiffRegion.IsHit(px, py, tolerancePx)`가 `|px-centerX| <= width/2+tolerance && |py-centerY| <= height/2+tolerance`로 판정
- **데이터 생성 알고리즘**: 이미지 비교 알고리즘으로 외부 생성된 `{difficulty}_diff_result.json`을 그대로 받아씀 (자체 diff 계산 로직 없음) — `diff_regions` 중 `confirmed:true`만 정답으로 채택(`DiffGameImporter`), `confirmed:false`는 계산 과정의 노이즈로 간주해 폐기
- **탭 허용 오차(tolerance)**: 40px (원본 픽셀 기준) — 모바일 손가락 탭 여유. 마커 크기는 발견/힌트 마커 `max(원본 대비 스케일 × 2.0, 84px)`, 오답(X) 마커 고정 84px
- **인터랙션 모델**: 원본(상단, 비인터랙티브 대조용) / 변형(하단, 탭 판정) 위아래 2분할. **오직 변형 이미지 탭만 판정 대상** — 원본을 탭해도 무시
- **힌트/보조 로직**: 미발견 `DiffRegion` 중 하나를 골드색으로 반짝이며 5초 후 자동 제거 (자동 정답처리 아님, 위치만 안내) — `DiffGameBoard.RequestHint()`
- **완료 판정 로직**: `foundCount == total` — 매 탭마다 동기 비교, 프레임 폴링 없음
- **난이도 파라미터**: JSON의 `difficulty`(easy/medium/hard)를 `LevelDataSO.Difficulty` enum으로 매핑. 난이도별 자체 규칙 분기(정답 개수/힌트 여부 등)는 없음 — diff 개수 자체가 JSON에 이미 난이도별로 다르게 들어있어 데이터로 흡수됨

### 슬라이딩 퍼즐 Sliding Puzzle (3번째 게임 — 구현 전 Spec 선작성, 2026-08-02)

> 이번엔 순서를 바꿔서 실험한다: 지금까지는 구현하면서 Rule Spec을 역으로 채웠지만, 세 번째 게임은 **아래 Spec을 먼저 확정하고 그대로 구현**해서 "에이전트가 스펙만 보고 생성"하는 워크플로우를 리허설한다. 구현 중 스펙과 실제가 어긋나는 지점이 생기면 그 자체가 중요한 기록 대상.

- **에셋**: 직소 퍼즐용 정사각 PNG 16장 **재사용** (신규 소스 불필요 — 사용자 확정)
- **그리드 분할**: 직소의 "제곱수만(36/64)" 관례는 슬라이딩 퍼즐엔 안 맞음 — 타일이 개별 조작 대상이라 그 정도 개수면 탭하기엔 너무 작음. 슬라이딩 퍼즐 전용 그리드 크기를 새로 정의: Easy 3×3(8-puzzle), Medium 4×4(15-puzzle), Hard 5×5(24-puzzle). 마지막 한 칸은 항상 빈 칸(blank)
- **인터랙션 모델 — 탭 이동 확정 (사용자 결정)**: 빈 칸과 상하좌우로 인접한 타일을 탭하면 그 타일이 빈 칸으로 슬라이드. 인접하지 않은 타일 탭은 무시. 드래그 방식은 채택 안 함(오조작 방지, 구현 단순화)
- **셔플 알고리즘 — 항상 풀 수 있는 상태 보장**: parity 계산으로 유효 순열을 검증하는 대신, **정렬된 상태에서 유효한 이동(빈 칸 인접 스왑)을 N번 랜덤하게 역산 적용**해서 셔플 생성 — 이 방식은 항상 풀 수 있는 상태만 만들어내므로 별도 solvability 검증/재시도 로직이 필요 없음. (5번 문서 "검증 체크리스트"의 "셔플 후 반드시 풀 수 있는 상태인지 자동 검증" 항목은 이 생성 방식 자체로 자동 충족됨 — 사후 검증이 아니라 사전 보장으로 설계)
- **완료 판정**: 모든 타일이 정렬된 인덱스와 일치 + 빈 칸이 마지막 위치. `progress`는 "정위치 타일 수 / 전체 타일 수". 탭(이산 이벤트) 시점에 동기 판정 — 틀린그림찾기와 동일한 트리거 패턴(폴링 아님)
- **힌트/보조 로직**: 실제 다음 수를 계산하는 솔버는 만들지 않음(범위 밖) — 직소/틀린그림찾기와 동일한 관례(자동 풀이 아니라 "위치 안내"만)를 따라, 정위치가 아닌 타일 하나를 원본 대조 이미지 기준으로 반짝여서 "여기로 가야 한다"는 목표 위치만 안내
- **LevelData**: 기존 두 SO 타입과 별개로 신규 서브타입 생성 (`levelNumber`, `gridSize`, `sourceSprite`) — Core Spec 결정(완전 별도 SO 타입) 그대로 적용
- **예상되는 인터랙션 모델 재확인**: 드래그(직소) / 탭+좌표히트테스트(틀린그림찾기) / **탭+그리드인덱스스왑(슬라이딩)** — 세 번째도 앞의 둘과 다른 입력 primitive. Core Spec이 인터랙션 레이어를 추상화하지 않기로 한 결정이 다시 한번 맞다는 증거가 될 전망

> Claude Code 작업 지침: 두 게임의 Rule Spec을 나란히 놓고 비교했을 때, 공통으로 뽑아낼 수 있는 항목이 있으면 Core Spec으로 승격. 게임별로만 존재하는 항목은 이 문서에 남김.

### 슬라이딩 퍼즐 — 구현 완료 후 검증 결과 (2026-08-02)

`~/casual-game-agent/SlidingPuzzle/` 프로젝트로 완성. **Core Spec 가설이 그대로 확인됨**: SpotTheDifference 프로젝트 전체를 복사(rsync) + 네임스페이스 일괄 치환(`SpotDiff`→`SlidingPuzzle`)만으로 Systems/UI/Core 레이어(SceneLoader, BackButtonHandler, GameManager, SaveManager, AdManager, AudioManager, HintManager, PaletteUI, DesignTokens, SettingsPopupUI, SafeAreaTopExtender, LocalizedTextBinder 등 전부)가 **단 한 줄도 수정 없이** 그대로 재사용됐고, 실제로 새로 작성한 건 정확히 3가지뿐이었다:
1. `LevelDataSO` 서브타입 (`sourceSprite`, `gridSize`)
2. `SlidingBoard`(렌더링/입력) + `SlidingGridState`(순수 그리드 로직 — 셔플/인접판정/완료판정/solvability)
3. `SlidingPuzzleImporter` (jigsaw 프로젝트의 정사각 PNG 16장을 그대로 재사용해서 LevelDataSO 16개 생성)

**순수 로직 분리 원칙이 3번째로 재확인됨**: `DiffRegion.IsHit`(틀린그림찾기)에 이어 이번엔 `SlidingGridState`를 MonoBehaviour(`SlidingBoard`)와 분리된 순수 C# 클래스로 뽑아서 셔플/solvability를 유닛 테스트 가능하게 만듦. → **Core Spec에 원칙으로 승격**: 게임 판정/상태 로직은 항상 Unity 오브젝트 의존 없는 순수 클래스로 만들고, MonoBehaviour 는 렌더링·입력 어댑터 역할만 한다.

**스펙과 실제 구현이 어긋난 지점 (사전 작성했던 Game Rule Spec 대비)**:
- 힌트: 스펙엔 "목표 위치를 원본 대조 이미지 기준으로 안내"라고 썼지만, 실제로는 범위를 줄여 "정위치가 아닌 타일 자체를 반짝임"으로 단순화함(목표 슬롯에 미리보기 고스트를 그리는 건 스코프 초과로 판단, 직소/틀린그림찾기와 동일하게 "자동 풀이 아님"이라는 원칙만 지킴).
- 미리보기 썸네일: 스펙에 없었지만 구현 중 반드시 필요하다는 게 드러남 — 조각이 사진 파편이라 원본을 안 보여주면 사실상 못 풂. LevelData 를 그림으로 표현하는 장르는 전부 "완성 예시 이미지" UI 요소가 암묵적으로 필요할 가능성 → 다음 장르에서도 확인 필요.

**구현 중 새로 발견된, 프로젝트 공통(3개 게임 전부에 잠재) 함정 2건 — Core Spec/CLAUDE.md 급 교훈으로 승격**:
1. **DOTween UI 모듈 미포함**: 이 프로젝트 계열(MyGames/SpotDiff/SlidingPuzzle 전부)에 복사되는 DOTween 배포본은 UI 전용 shortcut(`RectTransform.DOAnchorPos`, `Image.DOColor`, `CanvasGroup.DOFade` 등)이 동작하지 않는다 — `SceneFader.cs`에 이미 회피 주석("DOTween UI 모듈 의존 회피")이 남아있었지만 그 이유를 이번에 처음 실제로 겪음(신규 코드가 그 API 를 처음 호출해서 컴파일 에러로 드러남). **항상 `DOTween.To(getter, setter, target, duration)` core API 로 우회**할 것 — Transform 자체의 `DOScale`/`DOMove`/`DOPunchScale` 등은 core 라 문제 없음.
2. **텍스처 임포터 기본 타입이 Sprite 가 아님**: 새 PNG 를 코드로 복사해 넣고 `AssetDatabase.ImportAsset(..., ForceSynchronousImport)` 만 호출하면 기본 Texture Type("Default")으로 임포트되어 `LoadAssetAtPath<Sprite>` 가 항상 null 반환. `DiffGameImporter`엔 이미 `ConfigureAsSprite()`(textureType=Sprite, spriteImportMode=Single, SaveAndReimport)가 있었지만, `SlidingPuzzleImporter`를 새로 작성할 때 이 스텝을 빠뜨렸다가 배치모드 검증에서 즉시 잡힘. **에셋 임포터를 새로 짤 때마다 텍스처 타입 강제 설정 스텝이 필수 체크리스트 항목**.

**검증 방법**: Unity 배치모드(`-batchmode -nographics -quit`)로 컴파일 확인 → `-executeMethod` 로 Import/SceneSetup/Localization 도구 순서대로 실행 → `-runTests -testPlatform EditMode` 로 34개 테스트(HintManager 8 + LevelDataValidator 3 + SlidingGridState 22 + Addressables 1) 전부 통과 확인. 사용자가 직접 Unity Hub 로 열어서 플레이 확인하는 절차만 남음(이 환경은 화면/오디오 접근 불가).

### 메모리 매칭 Memory Match (4번째 게임 — 구현 전 Spec 선작성, 워크플로우 실전 검증용, 2026-08-03)

> 이번엔 지금까지 정리한 1~5번 문서 + `scripts/bootstrap_project.sh`/`verify_project.sh` 를 실제로 사용해서 얼마나 빠르게/적은 시행착오로 완성되는지가 목적. 세 번째 게임과 달리 이번엔 카드 이미지가 **여러 장(쌍) 필요**하다는 점이 처음 겪는 케이스.

- **에셋**: 직소 퍼즐용 정사각 PNG 16장 재사용. 슬라이딩 퍼즐(원본 1장 재사용)과 달리 **여러 장을 쌍으로 씀** — LevelDataSO 에 `Sprite[]` 필요(단일 `sourceSprite` 아님). Core Spec 의 "완전 별도 SO 타입" 원칙은 그대로 유지하되, 필드 형태 자체가 처음으로 배열이 되는 사례.
- **난이도별 카드 수**: Easy 4쌍(8장, 2×4) / Medium 6쌍(12장, 3×4) / Hard 8쌍(16장, 4×4) — 16장 이미지 풀에서 필요한 개수만 골라 씀
- **인터랙션 모델 — 탭+2장비교 (새로운 패턴)**: 카드를 탭하면 뒤집힘. 이미 한 장이 뒤집혀 대기 중이면 두 번째 탭 시점에 동기 비교 — 일치하면 그대로 앞면 유지(매치), 불일치하면 잠깐 보여준 뒤 자동으로 다시 뒤집음(뒤집힌 상태에서 추가 탭 무시). 슬라이딩(탭+그리드스왑)/틀린그림찾기(탭+좌표히트테스트) 와 또 다른 세 번째 "탭" 변형 — Core Spec 이 인터랙션 레이어를 추상화 안 하기로 한 결정을 다시 검증할 좋은 사례
- **완료 판정**: `matchedPairs == totalPairs`. 탭(이산 이벤트) 시점에 동기 판정 — 폴링 없음(틀린그림찾기/슬라이딩과 동일 트리거 패턴)
- **힌트/보조 로직**: 아직 못 맞춘 카드 한 장을 잠깐 앞면으로 보여줬다가 다시 가림(자동 매치 아님, 위치만 안내) — 세 게임과 동일한 관례
- **LevelData**: 신규 서브타입. `levelNumber`, `difficulty`, `cardSprites (Sprite[])`, `pairCount`
- **순수 로직 클래스**: `MemoryMatchState` — 카드별 pairId/뒤집힘/매치 상태, `FlipCard(index)` (Invalid/FirstFlip/Match/Mismatch 반환), `ResolveMismatch()`(딜레이 후 호출측이 명시적으로 닫음). `SlidingGridState`/`DiffRegion.IsHit` 과 동일하게 MonoBehaviour 와 분리해서 유닛 테스트 가능하게 설계

### 메모리 매칭 — 구현 완료 후 검증 결과 (2026-08-03, 스크립트 자동화 첫 실사용)

`~/casual-game-agent/MemoryMatch/` 프로젝트로 완성. **이번엔 `scripts/bootstrap_project.sh` + `scripts/verify_project.sh` 를 실제로 처음 사용**:

- `bootstrap_project.sh SlidingPuzzle MemoryMatch SlidingPuzzle MemoryMatch` 한 줄로 부트스트랩 — rsync, 네임스페이스 치환, asmdef 리네임, productName 갱신까지 스크립트 버그 없이 한 번에 성공.
- 장르 전용 코드 3종 작성: `LevelDataSO`(`cardSprites[]`+`pairCount`, 처음으로 배열 필드), `MatchBoard`+`MemoryMatchState`(순수 로직 분리, 세 번째 게임과 동일 패턴), `MemoryMatchImporter`(16장 풀에서 레벨마다 회전 오프셋으로 pairCount 장씩 뽑음 — 같은 이미지 풀을 재사용하되 레벨마다 다른 조합).
- `verify_project.sh` 로 컴파일→Localization Setup→Importer→SceneSetup→StringTable→EditMode 테스트(22개, MemoryMatchState 10 + LevelDataValidator 3 + HintManager 8 + Addressables 1)까지 **한 줄 명령으로 전부 통과**. (첫 컴파일 시도에서 ShaderGraph 패키지의 일시적 캐시 오류가 한 번 났지만 — 슬라이딩 퍼즐 때도 겪었던 것과 동일한 transient 오류 — 재실행으로 바로 해결, 실제 코드 문제 아님.)
- **AudioManager 는 이번에도 코드 변경 0줄** — `PlayFound`/`PlayMiss`/`PlayLevelComplete` 그대로 재사용. 4개 게임째 같은 이름으로 계속 들어맞음 → Core Spec 확정 항목으로 승격해도 될 신뢰도.

**순 제작 시간**: 부트스트랩 완료(00:17) → 배치모드 전체 검증 통과(00:26) = **약 9분**. 2번째(142분) → 3번째(44분) → 4번째(9분): 스크립트 자동화 이후 확실히 한 단계 더 빨라짐. 다만 메모리 매칭 자체가 슬라이딩 퍼즐보다 로직이 단순(solvability 계산 등 없음)했던 점, 그리고 이 패턴을 세 번째로 반복하는 숙련도 향상도 같이 작용했을 것 — 스크립트 자동화만의 순효과는 아니라는 점을 감안해서 읽을 것.

**실제 플레이 후 드러난 스펙 공백 2건 (2026-08-03)**:
1. **Complete 화면의 "완성 이미지" 개념이 이 장르엔 안 맞음**: 직소/틀린그림찾기/슬라이딩 퍼즐은 전부 "완성된 그림 한 장"이 결과물이라 Complete 화면에 그 이미지를 보여주면 됐지만, 메모리 매칭은 애초에 완성된 단일 그림이 없다(카드 pairCount 장이 각각 독립적인 이미지). 기존 패턴대로 대표 이미지 한 장(`ThumbnailSprite`)만 보여줬더니 사용자가 "이건 카드 맞추기라 이미지가 들어가면 안 될 것 같다"고 정확히 지적 — 그 레벨에서 맞춘 카드 전부를 작은 그리드로 진열하는 방식으로 교체. **교훈**: Complete 화면의 "보상 시각화" 방식은 Core Spec이 아니라 게임 룰 스펙 소관 — "완성 이미지가 1개인 장르"와 "복수 이미지로 구성된 장르"를 구분하는 항목을 Game Rule Spec 템플릿에 추가할 필요.
2. **난이도별 체감 차이 부족**: 초기 Easy/Medium/Hard = 4/6/8쌍은 사용자 피드백으로 "차이가 작다"고 확인됨 → 4/8/12쌍으로 조정(간격을 배수 단위로 벌림). **교훈**: 난이도 파라미터는 산술적으로 균등 분할(4,6,8)하기보다 체감 난이도 곡선(대략 2배씩 증가)로 설계하는 게 안전 — 앞선 두 게임(그리드 크기 3/4/5, 조각수 그대로 비례)에서는 이 문제가 안 드러났었는데, 이번처럼 "칸 수 = 난이도 체감"이 직접적인 장르에서 처음 확인됨.
3. **힌트가 카드 한 장만 보여줘선 무의미함**: 처음엔 다른 세 게임과 동일하게 "미완료 요소 하나만 반짝"이는 패턴을 그대로 적용했는데, 매칭 게임은 짝 중 한 장만 보여줘봐야 나머지 짝 위치를 모르면 쓸모가 없다는 걸 사용자가 바로 지적함 → `FindHintPair()`로 바꿔서 짝 두 장을 동시에 보여주도록 수정. **교훈**: "미완료 요소 하나 반짝"이라는 힌트 패턴은 요소가 서로 독립적인 장르(틀린그림찾기/슬라이딩)에서만 유효하고, "요소 간 관계(짝)"가 힌트의 핵심인 장르에서는 관계 전체를 보여줘야 함 — Core Spec의 힌트 규칙("자동 풀이 아님, 위치만 안내")은 유지되지만 "무엇을 얼마나 보여줘야 실제로 도움이 되는가"는 게임 룰 스펙에서 장르별로 별도 검토 필요.
4. **레벨 썸네일도 스포일러 문제**: LevelSelect 타일/Gallery 셀에 다른 게임처럼 대표 이미지 한 장을 미리 보여줬는데, 이건 곧 "그 레벨 카드 중 하나가 이거다"라고 미리 알려주는 스포일러였음 → LevelSelect 는 이미지 없이 그라디언트 + "Level N" 텍스트로, Gallery(이미 클리어한 레벨만 노출이라 스포일러 걱정 없음)는 Complete 화면과 동일하게 맞춘 카드 전부를 미니 그리드로 보여주는 방식으로 변경. **교훈**: "레벨 목록에 미리보기 이미지를 보여준다"는 것도 Core Spec이 아니라 게임 룰 스펙 소관 — 장르 자체가 "정보 은닉"이 핵심 메커니즘이면(카드 뒤집기, 틀린그림찾기 등) 미리보기가 스포일러가 되지 않는지 항상 재확인 필요.

---

## 3. 에셋 파이프라인 문서

이미지 입력 → 게임별 처리(분할/차이 생성 등) → 게임 데이터로 변환하는 과정. **3개 게임(직소/틀린그림찾기/슬라이딩 퍼즐) 모두에서 실제로 이 형태의 Importer 를 만들어본 뒤 확정.**

**핵심 설계 원칙: 에셋 파이프라인과 코드 생성 단계는 분리한다.** 이 원칙은 세 번 모두 지켜졌고 문제가 없었다 — Claude Code(코드 생성 단계)는 "이미 표준화된 원재료"(이미지, 또는 이미지+좌표 JSON)만 받았지, 이미지 자체를 분석/비교하는 로직은 단 한 번도 짜지 않았다. 틀린그림찾기의 `diff_result.json`은 외부 이미지 비교 알고리즘이 이미 계산해서 넘겨준 결과물이었다. **즉, "AI로 에셋을 생성/가공"하는 단계가 있다면 그건 이 파이프라인보다 더 앞단(Claude Code 바깥)에서 일어나야 한다 — 확정.**

- 장르별 원본→가공 데이터 형태 (3개 실사례):
  - 직소 → 정사각 PNG 원본 그대로 (조각 분할은 런타임에 `JigsawGrid.DetermineGrid` 가 pieceCount 로 계산 — 별도 전처리 불필요했음)
  - 틀린그림찾기 → 원본/변형 이미지 2장 + `diff_result.json`(외부 생성, `confirmed:true` 만 채택)
  - 슬라이딩 퍼즐 → 정사각 PNG 원본 그대로 (직소 이미지 재사용, 그리드 분할도 런타임 계산 — 전처리 불필요)
  - **패턴**: 이미지 자체를 전처리해서 "잘라놓은 조각 파일"을 미리 만들어 둘 필요는 의외로 거의 없었다. 런타임에 원본 텍스처를 Rect 로 잘라 `Sprite.Create` 하는 것으로 충분했던 경우가 2/3 (직소, 슬라이딩). 진짜 전처리가 필요했던 건 "AI/알고리즘이 아니면 계산할 수 없는 데이터"(틀린그림찾기의 diff 좌표)뿐이었다.
- 코드 생성 단계(Claude Code)로 넘길 때는 원본 이미지가 아니라 **① 요청사항(게임 타입, 난이도 파라미터 등) + ② 파이프라인이 만든 가공된 에셋 데이터** 두 가지만 전달한다.

### Importer 공통 골격 (3개 프로젝트에서 실제로 반복된 절차 — `Editor/{Genre}Importer.cs` 템플릿)

1. **원본 소스 위치**: 프로젝트 폴더 바로 아래, `Assets/` 밖의 별도 폴더 (`diff_game_source/`, `jigsaw_source/`) — Unity 에셋 데이터베이스가 건드리지 않는 "대기 구역". Importer 가 여기서 읽어서 `Assets/Sprites/{Genre}/` 로 복사.
2. `AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport)` — 복사 직후 바로 로드하려면 필수(동기 임포트 강제).
3. **텍스처를 명시적으로 Sprite 타입으로 재설정** (`TextureImporter.textureType = Sprite`, `spriteImportMode = Single`, `SaveAndReimport()`) — **필수 스텝, 두 프로젝트에서 실제로 빠뜨려서 버그가 난 지점**. 새 PNG 는 기본적으로 Sprite 로 임포트되지 않는다.
4. `LevelDataSO` 에셋을 경로 기준으로 로드해서 있으면 필드 갱신, 없으면 `ScriptableObject.CreateInstance` + `AssetDatabase.CreateAsset` — **idempotent**(재실행 안전)해야 함.
5. `EditorUtility.SetDirty` → `AssetDatabase.SaveAssets()` → `AssetDatabase.Refresh()`.
6. 메뉴 컨벤션: `{ProjectName}/Import {Genre} Levels`, 완료 후 `EditorUtility.DisplayDialog` 로 결과 요약.
7. 장르 전용 로직은 이 골격 위에 얹는다 — 틀린그림찾기는 4번 앞에 JSON 파싱(`JsonUtility.FromJson`)+`confirmed` 필터링이 추가됐을 뿐, 골격 자체는 안 바뀜.

### 오디오(BGM) Importer — 이미지와 별개의 두 번째 에셋 파이프라인

이미지 말고도 실제로 반복 사용한 Importer 가 하나 더 있음 — `Editor/AudioSetupTool.cs`(`SpotDiff`→`SlidingPuzzle`→`MemoryMatch` 그대로 재사용, 코드 변경 0줄):

1. 원본 소스 위치: `Assets/Audio/BGM/` (이미지처럼 `Assets/` 밖 대기 폴더가 아니라 폴더 자체가 `Assets/` 안에 있음 — 오디오 파일은 텍스처 타입 재설정 같은 게 필요 없어서 굳이 바깥에 둘 이유가 없었음).
2. `AssetDatabase.FindAssets("t:AudioClip", new[] { BgmFolder })` 로 폴더 스캔 → 파일명 오름차순 정렬 → `SerializedObject` 로 `AudioManager.bgmClips` 배열에 일괄 할당.
3. 기존 배열과 값이 동일하면 skip (idempotent), `AudioManager` 가 씬에 없으면 자동 생성.
4. SFX(found/miss/levelComplete/completeScene)는 절차적 합성이 기본값이라 이 Importer 대상이 아님 — **오디오 에셋이 없어도 게임이 무음이 되지 않는다는 게 이 파이프라인의 핵심 설계**(이미지와 반대로, 오디오는 "에셋이 없어도 동작"을 기본으로 하고 실제 파일은 선택적 업그레이드).

---

## 4. 에이전트 오케스트레이션 문서

3개 게임을 만드는 과정 자체가 이 워크플로우의 리허설이었다 — 아래는 실제로 반복 수행했던 절차를 그대로 옮긴 것 (가상의 이상적 워크플로우가 아니라 실증된 절차).

1. **장르 판별 & Game Rule Spec 확보**: 장르가 정해지면, 이 문서의 "2. Game Rule Spec"에 해당 항목이 있으면 재사용. 없으면 **구현 전에 먼저 작성**(3번째 게임에서 검증된 순서 — "스펙 먼저 → 그대로 구현")하고 사람 확인을 받은 뒤 진행. 회귀 있으면(스펙과 실제가 다르면) 반드시 스펙 문서에 기록.
2. **프로젝트 부트스트랩 — 빈 프로젝트에서 시작하지 않는다**: 실제로 제일 잘 통했던 방법은, 가장 최근에 완성되고 버그가 다 잡힌 **기존 프로젝트를 통째로 복사(rsync) + 네임스페이스 일괄 치환**하는 것이었다 (SpotTheDifference → SlidingPuzzle). Core Spec 레이어(Systems/UI/Core)가 이 방식으로 코드 수정 없이 100% 재사용됐다. 빈 프로젝트에서 하나씩 다시 짜는 것보다 압도적으로 안전 — SafeArea 처리, Settings 진입점 같은 크로스커팅 요소를 빠뜨릴 위험이 없다. **`scripts/bootstrap_project.sh` 로 스크립트화 완료** (2026-08-03).
3. **에셋 파이프라인 실행**: 3번 문서의 Importer 골격에 장르 전용 파싱을 얹어서 원본 소스 → `Sprite` + `LevelDataSO` 변환.
4. **장르 전용 코드 3종 생성** (Core Spec 가설 — 3개 게임에서 계속 참이었음):
   - `LevelDataSO` 서브타입 (완전 별도 타입, 상속 안 함)
   - 게임보드 컴포넌트 — MonoBehaviour(렌더링/입력) + 그 안에서 분리된 순수 C# 상태/판정 클래스(예: `SlidingGridState`, `DiffRegion.IsHit`) 한 쌍
   - Importer (위 3번 골격 기반)
5. **씬/로컬라이제이션 재조립**: `SceneSetupTool` 패턴으로 5개 씬(MainMenu/LevelSelect/Gameplay/Complete/Gallery) 재구성. `LocalizationStringTableTool` 로 텍스트 채우기 — 공용 키(`nav_home`, `settings_*` 등)는 그대로 두고 장르 전용 키만 추가.
6. **배치모드 자동 검증** (사람 개입 없이 실행 가능, 실제로 이 세션에서 매번 수행한 순서):
   `-batchmode -nographics -quit` 컴파일 확인 → `-executeMethod` 로 Importer → SceneSetupTool → LocalizationSetupTool → LocalizationStringTableTool 순서대로 실행 → `-runTests -testPlatform EditMode` 로 유닛 테스트 전수 통과 확인. **`scripts/verify_project.sh` 로 스크립트화 완료** (2026-08-03) — `verify_project.sh <project_dir> <executeMethod...>` 한 줄로 전 과정 실행.
7. **사람 최종 확인 — 생략 불가한 게이트**: 배치모드는 컴파일/데이터/로직만 검증하고 **화면 레이아웃/오디오/전반적 "느낌"은 검증하지 못한다**. 실제로 이 한계 때문에 노치 클리핑, 갤러리 캡션 누락, "허브 화면 완성도가 기대와 다름" 같은 문제를 전부 사람이 스크린샷/플레이로 찾아냈다. 에이전트 워크플로우의 마지막 단계는 항상 사람의 눈이다.
8. **스펙 갱신**: 구현 중 스펙과 실제가 어긋난 지점, 새로 발견된 함정(예: DOTween UI 모듈 미포함, 텍스처 임포터 기본 타입)을 즉시 이 문서에 기록 — 다음 장르 작업 시작 전에 반드시 재확인해서 같은 실수 반복 방지.

---

## 5. 검증/QA 체크리스트

배치모드로 자동 확인 가능한 것과, 사람이 봐야만 하는 것을 명확히 분리한다 (3개 게임 실사례 기반).

### 자동 검증 (배치모드, 사람 개입 불필요)

- **컴파일**: `-batchmode -nographics -quit` 종료 코드 0, `error CS` grep 결과 0건
- **에셋 임포트 무결성**: `LevelDataSO` 에셋의 스프라이트 참조가 `{fileID: 0}`(깨진 참조)이 아닌지 — `.asset` 파일을 grep 해서 즉시 확인 가능 (실제로 이 방법으로 두 번 버그를 잡음)
- **씬 등록**: 5개 씬 전부 `EditorBuildSettings.scenes` 에 등록됐는지
- **로컬라이제이션 커버리지**: 코드에서 쓰는 `entryKey` 전체를 정규식으로 추출해서 `UI_ROWS` 배열에 다 있는지 대조 (이 세션에서 매 게임마다 실제로 수행한 절차)
- **EditMode 유닛 테스트**: 장르별 순수 로직(완료판정/힌트/장르 특유 불변식)이 유닛 테스트로 존재하고 전부 통과하는지. `DiffRegion.IsHit`, `SlidingGridState` 처럼 판정 로직을 MonoBehaviour 에서 분리해야 이 테스트가 가능해진다는 게 3개 게임 모두에서 확인된 전제조건.
- **PlayMode E2E 테스트 (4번째 게임에서 4개 게임 전부에 이식 완료)**: 씬 전환 4종(`SceneTransitionTests.cs`) + 장르별 실제 탭→판정 흐름(`GameplayFlowTests.cs`)이 `IPointerClickHandler` 직접 호출 방식이라 디스플레이 없이 `-runTests -testPlatform PlayMode`로 완전 자동 실행됨. 자세한 내용/컨벤션은 위 Core Spec "PlayMode E2E 테스트" 절 참고. `verify_project.sh`가 EditMode 다음에 이걸 자동으로 이어서 실행함.
- **장르 특유 불변식 — 실증된 예시**:
  - 슬라이딩 퍼즐: 셔플 결과가 항상 이론상 solvable 한지 (parity 공식, `SlidingGridStateTests.TC_SG_005/007`)
  - 틀린그림찾기: 임포트된 `diffRegions` 개수가 JSON 의 `confirmed:true` 개수와 일치하는지, 좌표가 `sourceImageWidth/Height` 범위 안인지
  - 공통: `levelNumber` 가 1부터 끊김 없는 연속 정수인지, `Resources/Levels/` 자산 개수가 의도한 레벨 수와 일치하는지

### 사람 확인 필수 (배치모드로 검증 불가능한 영역 — 반드시 스크린샷/플레이로 확인)

- 실제 화면 레이아웃 (노치/SafeArea 클리핑, 텍스트 잘림/겹침, 마커·캡션 등이 실제로 "보이는지")
- 오디오 재생 여부 (배치모드는 오디오 디바이스에 접근 못 함)
- 허브 화면(LevelSelect/Gallery)의 UI 완성도가 레벨 수 규모에 맞는지 — 이번 슬라이딩 퍼즐에서 실제로 이 항목이 빠져서 문제가 됐음. **레벨 수가 일정 규모(예: 10개+) 이상이면 축소판 허브 UI(층 구분 생략 등)를 자동으로 쓰지 않도록 Importer/SceneSetupTool 단계에서 경고하는 규칙 추가를 향후 검토.**
- 애니메이션/트윈의 시각적 완성도(속도감, 이징 느낌, "손맛") — PlayMode E2E 테스트는 "탭하면 상태·위치가 올바르게 바뀌는가"까지만 자동 확인하고, "예쁘게/기분 좋게 바뀌는가"는 여전히 사람만 판단 가능 (조작 흐름 자체의 기능적 정확성은 위 자동 검증 항목으로 이동함)

### 실사용 중 발견한 테스트 워크플로우 함정 (프로젝트 버그 아님 — 착각하지 않도록 기록)

- **`Setup All Scenes` 실행 직후 Play 를 누르면 안 됨**: 이 메뉴는 MainMenu→LevelSelect→Gameplay→Complete→Gallery 순서로 씬을 열었다 닫으며 세팅하고, **마지막으로 처리한 Gallery.unity 를 열어둔 채로 끝난다.** 이 상태에서 바로 Play 를 누르면 (클리어한 레벨이 없는 새 프로젝트 기준) Gallery 의 빈 상태 화면만 보여서 "게임이 반응 없다"고 착각하기 쉽다 — 실제로 이 세션에서 사용자가 이걸로 헷갈린 적 있음. **Play 전에 항상 `Gameplay.unity` 나 `MainMenu.unity` 를 직접 열어둘 것.**
- **`ArgumentOutOfRangeException` in `UnityEditor.Search.SearchDatabase.GetDefaultSearchDatabase()`**: 콘솔에 반복적으로 뜨는 에러지만 Unity 엔진 자체의 내장 Search(QuickSearch) 모듈이 새 프로젝트에서 검색 인덱스가 아직 없을 때 나는 내부 버그 — 스택트레이스에 프로젝트 코드가 전혀 안 나오면 이 케이스. 컴파일/Play/테스트에 영향 없음, 무시해도 됨.

### 참고 — 게임별 소요시간 및 타임라인 (파일 타임스탬프 기준, 최종 정리 2026-08-03)

사람의 읽기/생각 시간은 제외하고, 실제 파일 생성/수정 타임스탬프만으로 측정.

**측정 방법**: 각 게임의 첫 파일 생성 ~ 마지막 파일 수정을 "총 구간"으로 잡음. 파일 활동이 5분 이상 끊긴 구간은 "공백"으로 분리하고, 공백은 5분까지만 활동 시간에 포함(그 이상은 제외)해서 "활동 시간"을 별도 산출 — 회의/디버깅 대화·배치모드 대기시간이 길게 낀 구간을 활동 시간에서 걸러내기 위함.

**1번째(직소)는 성격이 달라 표에서 분리함** — 이 프로젝트("에이전트로 여러 게임 찍어내기")의 재료가 아니라 **전제조건**이었다. 여기서 사용자가 Unity/게임 제작 기본기를 직접 익히고 실기기 검증까지 마쳤고, 그 결과물(코드베이스+실기기 검증된 아키텍처)이 있었기 때문에 2번째 게임부터 "뭐가 진짜 재사용 가능한가"를 판별하는 작업이 가능했음. 즉 2~4번째의 "재사용/자동화로 시간이 줄어드는" 이야기 자체가, 1번째에서 이미 한 번 제대로 만들어보고 검증한 게 없었다면 성립하지 않음.

| | 1번째 (Puzzle Pals, 직소) | 2번째 (SpotTheDifference) | 3번째 (SlidingPuzzle) | 4번째 (MemoryMatch) |
|---|---|---|---|---|
| 총 구간 | 약 19일(04-14 17:25~05-03 14:48), 작업일 9일 | 142분 (20:23→22:46, 08-02) | 44분 (23:06→23:49, 08-02) | 9분 (00:17→00:26, 08-03) |
| 최대 공백 | 해당없음 — 9개 캘린더 날짜로 자연 분리된 세션이라 게임 2~4의 "단일 세션 내 공백" 개념과 다름(하루 안에서 제일 컸던 공백은 05-01의 192분) | 37분 (21:19→21:56) | 23분 (23:24→23:47) | 없음 |
| 활동 시간 | **정확히 산정 불가 — 약 3.5시간(5분 캡 하한선) ~ 약 29.6시간(하루 첫~끝 파일 그대로 합, 상한선) 사이** | **약 69분** | **약 25분** | **9분** (공백 없어 총 구간과 동일) |

**08-02는 이 표에서 제외함**: 처음엔 08-02까지 포함해서 "3.5개월/10작업일"로 잡았었는데, 확인해보니 08-02는 새 구현이 아니라 몇 달 방치 후 재실행 과정에서 생긴 런타임 이슈를 재컴파일+재실행으로 고친 것뿐(사용자 확인). 그래서 직소 "제작" 구간은 **04-14~05-03, 9작업일**로 한정하고, 08-02는 아예 이 집계에서 뺐다. (CLAUDE.md의 "프로젝트를 몇 달 방치했다가 다시 열 때... 2026-08-02 확인: 재실행으로 해결됨" 항목이 이 사건.)

**1번째(직소)만 "활동 시간"을 단일 숫자가 아니라 범위로 표기하는 이유**: 2~4번째는 이 세션 내 대화 로그로 "5분 넘는 공백 = 실제로 회의/대기였는지"를 교차검증할 수 있었지만, 1번째는 당시 다른(현재 무료 요금제로 전환된) Claude 계정에서 진행돼 그 대화 로그에 접근할 방법이 없음. 그래서 "5분 캡" 하한선(3.5시간)과 "하루 첫 파일~마지막 파일을 그대로 합산"한 상한선(29.6시간, 그마저도 `.cs` 저장 이벤트만 집계한 것이라 씬 편집/에셋 준비/기본기 학습 시간은 여기에도 안 잡힘) 사이 어딘가라는 것만 확인 가능하고, 그 안의 정확한 지점은 알 수 없음. 사용자 본인 체감으로는 "더 오래 걸렸다"는 쪽 — 상한선에 더 가깝게 봐야 할 근거로 참고.

공백 성격(2~4번째): 2번째는 마커 가시성 버그 디버깅과 시기가 겹치나 대화 자체가 요약본에 압축돼 있어 순수 디버깅인지 자리 비움이 섞였는지 구분 불가. 3번째는 배치모드 자동 검증(Unity 여러 번 실행) 대기 + Gallery 캡션 버그 스크린샷 대화로 대부분 설명됨 — 순수 유휴 아님. 4번째는 공백 자체가 없음(단, "9분"이 측정하는 구간은 코드 작성 시작~배치검증 통과까지이고, 이후 사람이 실기기/에디터로 플레이해보고 피드백을 주는 시간은 이 구간 밖에 있음 — 아래 "9분 이후" 참고).

**4번째, "9분" 구간 안에 사람으로 인한 지연이 있었는가**: 없음. `00:17` 부트스트랩 실행부터 `00:23` 마지막 EditMode 테스트 작성, `00:26` `verify_project.sh` 통과까지 파일 수정 간격이 전부 1~3분 이내로 연속적 — 이 구간 자체는 순수 에이전트 작업이었고 사람이 끼어들어 기다리게 한 지점이 없음. 장르 선택(AskUserQuestion으로 "메모리 매칭 (추천)" 확정)은 이 9분 구간이 시작되기 **전에** 이미 끝나있던 결정이라 이 수치엔 애초에 안 들어감.

**"9분" 이후 — 실제 사람 개입 구간 (참고용, "제작 시간"엔 미포함)**: `00:23` 마지막 테스트 작성 이후 `00:44`(`CompleteSceneController.cs` 수정 시작)까지 **약 22분 공백**이 있음 — 이 구간이 사용자가 실제로 Unity 에디터에서 Play해보고 피드백 3건(완료 화면 스포일러/난이도/힌트)을 발견해서 전달한 시간. 즉 "9분"은 **"에이전트가 코드를 쓰고 스스로 검증하는 시간"**만 잰 것이고, "사람이 실제로 플레이해보고 판단하는 시간"은 애초에 다른 구간(피드백 반영 단계)으로 분리되어 있어 9분에 섞이지 않음 — 이게 정확한 경계선.

**결론**: 순수 제작 구간만 보면 142→44→9분으로, 활동 시간도 69→25→9분으로 뚜렷하게 줄어듦. 2→3번째는 Core Spec 재사용 효과, 3→4번째는 스크립트 자동화 + 반복 숙련도가 같이 작용(메모리 매칭이 슬라이딩 퍼즐보다 로직이 단순했던 것도 한 요인) — 어느 한 요인만으로 과대해석하지 않을 것. 3→4번째로 갈수록 "공백"의 성격도 "사람이 기다리는 시간"에서 "스크립트가 일하는 시간"으로 바뀐 게 중요한 변화. 그리고 이 모든 단축의 출발점은 1번째(직소)에서 사람이 직접 부딪히며 익힌 기본기와 실기기 검증이었다는 점 — 자동화는 "이미 한 번 제대로 만들어본 것"을 재사용하는 것이지, 안 만들어본 걸 처음부터 자동화한 게 아니었음.

### 게임별 주요 타임라인

**1번째 — Puzzle Pals / 직소 (04-14~05-03, 이 세션 이전 작업, 9 작업일)**
- `04-14` 코어 구조 착수 (`GameManager.cs`, `PieceData.cs`, `VFXManager.cs` 등)
- `04-16` SafeArea/반응형 그리드/IAP/로컬라이제이션 세팅 도구 등 시스템 레이어 확장 (하루 중 가장 긴 활동, 5분캡 40분)
- `04-19`~`04-22` `UnityAdsProvider.cs`, 직소 알고리즘 핵심(`EarClipTriangulator.cs`/`JigsawGrid.cs`) 착수 + 첫 EditMode/PlayMode 테스트(`SnapLogicTests.cs`/`DragSnapFlowTests.cs`)
- `04-25`~`04-27` 게임플레이 루프(`PuzzleCompleteChecker.cs`/`SnapZone.cs`/`PieceBank.cs`), `HintManagerTests.cs`, `LevelDataSO.cs`/`LevelDataValidator.cs`, `PaintingImporter.cs`
- `05-01`~`05-03` 조각 인접 판정(`NeighborPairing.cs`), 직소 메시 빌더(`JigsawMeshBuilder.cs`/`JigsawPieceShape.cs`), `IAdProvider`/`AdManager`/`AudioManager`/`BackButtonHandler` 등 이후 Core Spec으로 확정된 핵심 파일 다수 완성(05-03이 5분캡 55분으로 가장 밀도 높은 날, 마지막 파일은 05-03 14:48) — **여기가 직소 "제작" 구간의 끝**
- (이후 약 3개월간 프로젝트 방치 — 이 표에서 다루는 "제작 시간"엔 포함 안 함)
- `08-02`(별도, 이 표엔 미포함): 방치 후 재개 시 발생한 stale 참조 런타임 이슈를 재컴파일+재실행으로 해결(신규 구현 아님, CLAUDE.md 기록 참고) — 이후 같은 날 2번째 게임(SpotTheDifference) 착수

**2번째 — SpotTheDifference (08-02)**
- `20:23` 프로젝트 스캐폴딩 시작 (ProjectSettings/manifest)
- `20:27` Systems/UI 16개 파일 일괄 복사+리네임
- `20:44` DiffGameImporter.cs · `20:51` AudioManager.cs(절차적 합성) · `21:09` PaletteUI.cs · `21:19` MainMenuController.cs
- **(37분 공백)**
- `21:56` CompleteSceneController.cs · `22:03` SceneLoader.cs · `22:07` BackButtonHandler.cs
- `22:12` DiffGameBoard.cs(핵심 게임플레이) · `22:13` EditMode 테스트 3종
- `22:26` LevelDataSO.cs/SettingsPopupUI.cs/LocalizationStringTableTool.cs · `22:31` SceneSetupTool.cs · `22:34` LevelSelectController.cs/GalleryController.cs
- `22:46` AudioSetupTool.cs(BGM) — 종료

**3번째 — SlidingPuzzle (08-02)**
- `23:06` rsync 복사 + 네임스페이스 일괄 치환
- `23:12` LevelDataValidator.cs/GameplaySceneController.cs/GameplayHeaderUI.cs/SceneSetupTool.cs/CompleteSceneController.cs · `23:14` MainMenuController.cs/SettingsPopupUI.cs
- `23:17` EditMode 테스트 · `23:18` SlidingGridState.cs(순수 로직) · `23:19` 테스트 · `23:21` SlidingBoard.cs(MonoBehaviour) · `23:24` SlidingPuzzleImporter.cs
- **(23분 공백 — 배치모드 검증 + Gallery 버그 대화)**
- `23:47` LevelDataSO.cs/GalleryController.cs 풀피델리티 포팅 · `23:49` LevelSelectController.cs/LocalizationStringTableTool.cs — 종료

**4번째 — MemoryMatch (08-03)**
- `00:17` `bootstrap_project.sh` 실행 → LevelDataSO.cs
- `00:18` MemoryMatchState.cs(순수 로직) · `00:19` MatchBoard.cs(MonoBehaviour) · `00:20` MemoryMatchImporter.cs/LevelDataValidator.cs
- `00:21` LevelSelectController.cs/GalleryController.cs 필드 어댑팅 · `00:22` LocalizationStringTableTool.cs · `00:23` EditMode 테스트 작성
- `00:26` `verify_project.sh` 전체 검증 통과 — **"9분" 구간 종료** (여기까지 공백 없음)
- **(약 22분 — 사람이 Unity 에디터로 실제 플레이해보고 피드백 3건 발견하는 시간, "제작 시간" 집계엔 미포함)**
- `00:44` CompleteSceneController.cs(완료화면 카드 진열) · `00:49` MemoryMatchState.cs(FindHintPair) · `00:52` LevelSelectController.cs(썸네일 스포일러 제거) · `00:54` LevelDataSO.cs(난이도 4/8/12) — 라이브 피드백 반영 완료

---

## 6. 새 게임을 요청할 때 사용자가 전달해야 할 것

3개 게임을 실제로 만들면서 보니, 매번 사람이 직접 결정해야 했던 항목은 소수였고 나머지는 이 문서의 절차(4번)나 스크립트가 자동/제안으로 처리했다. 다섯 번째 게임부터는 아래만 명시하면 나머지는 4번 문서 절차가 그대로 진행된다.

### 반드시 정해서 전달해야 하는 것 (사람만 결정 가능한 것들)

1. **장르**: 정해뒀으면 그대로 지정. 안 정했으면 "뭐로 할지 제안해줘"라고만 해도 됨 — 4번째 게임(메모리 매칭)도 실제로 이렇게 후보를 제시받아 사용자가 골랐음(AskUserQuestion으로 "메모리 매칭 (추천)" 선택).
2. **인터랙션 모델**: Core Spec이 이 레이어를 추상화하지 않기로 확정했으므로(1번 문서 "인터랙션 이벤트 모델" 항목), 장르가 정해져도 조작 방식이 여러 갈래로 갈릴 수 있으면 반드시 명시. 예: 슬라이딩 퍼즐 때 "드래그가 아니라 탭 이동"을 사용자가 직접 확정한 선례.
3. **에셋 소스**: 기존 프로젝트 이미지(예: 직소 16장) 재사용인지, 새 이미지 세트인지. 재사용이면 어느 프로젝트 것인지만 말하면 `bootstrap_project.sh`가 그대로 복사한다. 새 이미지면 원본 파일이 있는 폴더 경로를 전달.
4. **스펙 선작성 여부**: "스펙 먼저 써서 확인받고 그다음 그대로 구현" 방식을 원하면 명시(3번째 게임에서 검증된 방식, 재작업이 줄어들어 권장). 말 안 하면 기본은 구현하면서 역채움.
5. **장르 고유 판단이 필요한 지점 — Game Rule Spec 체크리스트(2번 문서) 10개 항목 중 코드로 자동 결정 안 되는 것들**: 특히 "완성 이미지 유무"/"미리보기가 스포일러인지"/"난이도 체감 곡선"은 실제로 매번 사람이 직접 플레이해보고서야 잡아낸 항목들이다(메모리 매칭에서 3건 다 뒤늦게 발견해서 수정). 여기에 대해 미리 의도가 있으면 그때 말해두면 재작업이 줄어듦 — 없으면 구현 후 첫 플레이 피드백으로 잡는 기존 방식 그대로 진행.

### 안 정해도 되는 것 (에이전트가 자동/제안으로 처리)

- Systems/UI 공용 레이어(SceneLoader, HintManager, AudioManager 등) — 전부 파일 복사로 재사용, 신경 쓸 필요 없음
- 5개 씬 구조/내비게이션 규칙(MainMenu=최초 진입 전용, LevelSelect=허브) — 이미 고정된 계약
- 로컬라이제이션 4개 언어 세팅 — `LocalizationStringTableTool`이 처리, `UI_ROWS`만 채우면 됨
- 오디오 BGM 파이프라인 — `AudioSetupTool`이 `Assets/Audio/BGM/`을 자동 스캔
- 배치모드 검증(컴파일/EditMode/PlayMode 테스트) — `verify_project.sh` 한 줄
- 난이도 파라미터의 정확한 숫자 — 체크리스트 기준(≈2배 간격)으로 초안을 제안한 뒤 AskUserQuestion으로 확인받음(메모리 매칭 난이도 4/8/12 확정 방식)
- PlayMode E2E 테스트용 `Debug*` 헬퍼/asmdef/테스트 파일 — 장르 결정되면 자동으로 같이 작성됨

### 최소 요청 예시

> "5번째 게임 만들어줘. 장르: [정했으면 이름, 안 정했으면 '추천해줘']. 이미지는 [기존 프로젝트명] 거 재사용 / 새 이미지 [경로]. 조작 방식은 [정했으면 명시, 안 정했으면 '알아서 제안해줘']."

이 정도만 있으면 부트스트랩 → 임포터 → 장르 전용 코드 3종 → 씬/로컬라이제이션 재조립 → 배치모드 자동 검증(EditMode+PlayMode) → 사람 최종 확인 요청까지 4번 문서 절차대로 자동 진행된다.

---

## 지금 당장의 액션 아이템

- [x] 두 번째 게임 확정 — 틀린그림찾기 (2026-08-02)
- [x] 직소 퍼즐 Game Rule Spec 역채움 (완료 — 위 항목 참고)
- [x] Unity LevelData 표현 방식 결정 — 완전 별도 SO 타입 채택
- [x] 틀린그림찾기 구현 완료 (게임플레이/힌트/광고/완료흐름/LevelSelect/Gallery/Settings/다국어/EditMode 테스트/오디오) — 2026-08-02
- [x] 틀린그림찾기 Game Rule Spec 확정 (위 항목 참고)
- [x] Core Spec 섹션을 두 게임 완성 기준으로 확정 (완료 판정/힌트/인터랙션모델/공용 UI 레이어 — 위 항목 참고)
- [x] 세 번째 게임 확정 — 슬라이딩 퍼즐, 직소용 이미지 재사용 (2026-08-02)
- [x] 슬라이딩 퍼즐 Game Rule Spec **구현 전 선작성** (조작방식=탭 이동 사용자 확정) — 위 항목 참고. "구현하며 역채움"이 아니라 "스펙 먼저 → 그대로 구현"으로 워크플로우 자체를 검증하는 게 목적
- [x] 슬라이딩 퍼즐 구현 완료 + 배치모드 컴파일/임포트/씬세팅/로컬라이제이션/EditMode 테스트(34개) 전부 검증 통과 — 2026-08-02
- [x] "Core Spec 공용 레이어만으로 충분했는지" 재검증 — **충분했음**, 신규 코드는 LevelDataSO 서브타입 + 게임보드(SlidingBoard/SlidingGridState) + 임포터 3가지뿐 (위 항목 참고)
- [x] 스펙-vs-실제 구현 어긋난 지점 기록 (힌트 단순화, 미리보기 썸네일 필요성 — 위 항목 참고)
- [x] 구현 중 발견된 프로젝트 공통 함정 2건 기록 (DOTween UI 모듈 미포함, 텍스처 임포터 기본 타입 — 위 항목 참고)
- [x] 사용자가 Unity Hub 로 직접 열어서 플레이 확인 — 정상 동작 확인 (2026-08-02)
- [x] **추가로 드러난 스펙 공백**: Game Rule Spec은 게임플레이 규칙만 다뤘지 LevelSelect/Gallery 같은 허브 화면의 "UI 완성도 기대치"는 다루지 않았음. 실제로 SpotTheDifference 때 축소판(레벨 3개라 층 구분/진행률 카드 생략)으로 만든 LevelSelect/Gallery를 SlidingPuzzle에 그대로 복사했더니, 레벨 16개(직소와 동일 규모)에서는 그 축소가 부적절하다는 게 사용자 피드백으로 드러남 → 직소의 `LevelSelectController`/`GalleryMuseumController` 풀버전을 이식(Progress 카드, Current Challenge 카드, ScrollRect+층 구분, 골드 액자, 작품명)해서 해결. **교훈**: 허브 화면의 UI 리치니스는 게임 규칙이 아니라 "레벨 수 규모"에 종속되므로, Game Rule Spec에 "레벨 수" 항목이 있으면 그걸로 허브 UI 축소 여부를 자동 판단할 수 있어야 함 — 향후 Core Spec/검증 체크리스트에 반영 필요
- [x] 3번(에셋 파이프라인)/4번(에이전트 오케스트레이션)/5번(검증 체크리스트) 문서 작성 완료 — 3개 게임 실사례 기반, 가상의 이상론이 아니라 실제 반복 수행한 절차를 그대로 옮김 (2026-08-02)
- [x] 2번째/3번째 게임 순 제작 시간 실측 (위 "참고" 섹션) — 142분 → 44분, 약 3.2배 단축 확인 (2026-08-03)
- [x] 4번 문서 절차 스크립트화 — `scripts/bootstrap_project.sh`(부트스트랩) + `scripts/verify_project.sh`(배치모드 자동 검증) 작성 완료, `scripts/README.md` 에 사용법 기록 (2026-08-03). 아직 실제 프로젝트로 라이브 테스트는 못 함(작업 중이던 프로젝트들이 Unity 에디터로 열려있어 락 충돌) — 4번째 게임 부트스트랩 때 처음 실사용하며 검증
- [x] 네 번째 게임(메모리 매칭)으로 스크립트 실전 검증 완료 — `bootstrap_project.sh`/`verify_project.sh` 둘 다 스크립트 버그 없이 한 번에 성공, 순 제작 시간 9분 (2026-08-03, 위 "메모리 매칭 — 구현 완료 후 검증 결과" 참고)
- [x] AudioManager 코드 변경 0줄이 4개 게임째 계속 유지됨 — Core Spec 확정 항목으로 최종 승격
- [x] 사용자가 Unity Hub 로 MemoryMatch 직접 플레이 → 실제 피드백 3건 반영 완료 (2026-08-03):
  - Complete 화면 "완성 이미지 한 장" → 매칭한 카드 전부 미니 그리드로 진열 (장르에 완성 그림이 없다는 걸 뒤늦게 발견 — 위 "실제 플레이 후 드러난 스펙 공백" 참고)
  - 난이도별 체감 차이 부족 → 쌍 개수 4/6/8 → 4/8/12 로 간격 확대
  - 힌트가 카드 한 장만 보여줘서 무의미 → `FindHintPair()`로 짝 두 장을 동시에 보여주도록 수정
  - (부수적으로) 레벨 썸네일이 카드 이미지를 미리 보여줘서 스포일러였던 것도 함께 발견·수정 (LevelSelect는 텍스트만, Gallery는 클리어 후라 스포일러 걱정 없어 미니 그리드로)
- [x] 게임별 소요시간/타임라인 최종 정리 — 총 구간 142→44→9분, 활동 시간(5분 캡 기준) 69→25→9분 (위 "참고 — 게임별 소요시간 및 타임라인" 섹션)
- [x] **문서 최종 검토 — 4개 게임 논의 기반으로 빠진 부분 보강** (2026-08-03):
  - Core Spec에 "5개 씬 구조 & 내비게이션 시맨틱스"(MainMenu=최초 진입 전용, LevelSelect=허브) + "Input Handling Both / asmdef 3종" 요구사항 명시 추가 — 코드에는 있었지만 문서엔 없었음
  - Game Rule Spec에 **10개 항목 체크리스트** 신규 작성 — 지금까지 매번 실플레이 후에야 발견했던 것들(힌트 표시량, 난이도 간격, 허브 UI 규모, 완성이미지 유무, 스포일러 여부, 에셋 형태)을 다음 장르는 구현 전에 미리 채우도록 함
  - 에셋 파이프라인 문서에 오디오(BGM) Importer 패턴 추가 — 이미지 Importer 만 있고 실제로 3개 게임에서 재사용한 `AudioSetupTool.cs` 패턴은 빠져있었음
  - 검증 체크리스트에 테스트 워크플로우 함정 2건 추가 — `Setup All Scenes` 직후 Gallery 씬이 열린 채로 남는 것(Play 눌러도 반응 없어 보이는 원인), QuickSearch 벤치성 에러(프로젝트 버그 아님)
- [x] **PlayMode E2E 자동화 도입 — "조작감은 사람만 확인 가능"이라는 전제 재검증** (2026-08-03): jigsaw(`MyGames`)에 이미 있던 `DragSnapFlowTests.cs` 패턴(`IPointerClickHandler` 직접 호출로 EventSystem 레이캐스팅 우회 → 디스플레이 없이 batchmode 실행 가능)을 4개 게임 전부에 이식 완료.
  - SpotTheDifference / SlidingPuzzle / MemoryMatch 각각에 `SceneTransitionTests.cs`(씬 전환 4종, 장르 무관 공통 템플릿) + `GameplayFlowTests.cs`(장르별 실제 탭→판정 흐름) + 전용 `*.PlayMode.Tests.asmdef` 작성
  - 셔플/랜덤 장애물 해결용 `Debug*` 테스트 지원 메서드를 게임보드에 추가 (`DiffGameBoard.DebugGetScreenPositionForRegion`, `SlidingBoard.DebugGetAdjacentPieceIndex`/`DebugIsPieceAdjacentToBlank`, `MatchBoard.DebugFindMatchingPair`/`DebugPairIdAt`) — jigsaw의 `SetSnappedForTest` 선례와 같은 원칙
  - `scripts/verify_project.sh`가 EditMode 다음에 PlayMode 테스트까지 자동으로 이어서 실행하도록 갱신 (`run_test_platform` 함수로 중복 제거)
  - 상세 내용은 위 Core Spec "PlayMode E2E 테스트" 절 + `scripts/README.md` 참고.
- [x] **PlayMode E2E 라이브 검증 완료** (2026-08-03): 사용자가 직접 `verify_project.sh` 3개 프로젝트 전부 실행 — 컴파일/Importer/SceneSetup/Localization/Audio/EditMode(17+34+22개)/PlayMode(6개씩) 전부 1회차에 통과. 스크립트·테스트 코드 둘 다 실사용 검증 완료.
- [x] **6번 문서 신규 작성 — "새 게임 요청 시 사용자가 전달해야 할 것"** (2026-08-03): 사용자가 매번 실제로 결정해야 했던 항목(장르/인터랙션 모델/에셋 소스/스펙 선작성 여부/Game Rule Spec 체크리스트 중 사람 판단 필요 항목)과, 자동/제안으로 처리되는 항목을 분리해서 최소 요청 템플릿으로 정리 — 위 "6. 새 게임을 요청할 때 사용자가 전달해야 할 것" 참고
- [ ] **다음 단계**: 다섯 번째 게임 후보 선정(4개 인터랙션 모델을 다 써봤으니 새로운 입력 방식 찾기가 점점 어려워짐 — 오히려 좋은 신호), 아니면 이쯤에서 1~6번 문서를 "확정 스펙"으로 동결하고 실제 서비스/배포 단계로 전환
