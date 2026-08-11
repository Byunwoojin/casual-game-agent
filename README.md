# 캐주얼 게임 에이전트

Unity 캐주얼 퍼즐 게임을 반복 생산하는 에이전트 워크플로우를 만든 개인 프로젝트. 4개 장르(직소/틀린그림찾기/슬라이딩 퍼즐/메모리 매칭)를 순서대로 직접 구현하면서, 무엇이 정말로 공통이고 무엇이 장르마다 다른지를 추측이 아니라 실증으로 판별했다.

## 어떻게 만들어나갔는지

개인 프로젝트에서도 같은 원칙을 적용해, Unity 캐주얼 퍼즐 게임을 반복 생산하는 에이전트를 단계적으로 구축했다. 이게 가능했던 출발점은 1번째 게임(직소 퍼즐)에서 게임의 기본 구조(씬 전환, 광고/저장/힌트/오디오 시스템, UI, 에셋 파이프라인)를 처음부터 끝까지 직접 완성하고 실기기까지 검증해둔 것이었다. 2번째 게임(틀린그림찾기)을 만들며 이 결과물을 그대로 복제·수정해보니, 장르 고유의 메인 게임 로직 3가지(레벨 데이터 타입, 게임보드, 임포터)를 제외한 나머지 전부가 코드 수정 없이 재사용된다는 걸 확인했다(142분 소요). 이 경계를 3번째 게임에 그대로 적용하자 제작 시간이 44분(3.2배 단축)으로 줄었고, 이때 반복되는 절차(프로젝트 복제, 배치모드 검증)를 스크립트 2개로 자동화했다. 4번째 게임에 처음 실전 투입한 결과 순수 제작 시간(코드 작성+자동 검증 기준)이 9분으로 2번째 대비 약 16배 단축됐다. 이 과정에서 EditMode/PlayMode E2E 테스트 134개를 자동화해 배치모드로 회귀를 검증하되, 실기기 검수와 최종 판단은 계속 사람이 맡는 경계는 그대로 유지했다.

## 게임 4종

| 순서 | 게임 | 폴더 | 인터랙션 | 비고 |
|---|---|---|---|---|
| 1 | Puzzle Pals (직소) | [`PuzzlePals/`](PuzzlePals) | 드래그 + 인접판정 | 기반 프로젝트 — 여기서 익힌 구조를 이후 3개가 재사용 |
| 2 | 틀린그림찾기 | [`SpotTheDifference/`](SpotTheDifference) | 탭 + 좌표 히트테스트 | 공용 레이어(Systems/UI) 재사용 경계를 처음 확인 |
| 3 | 슬라이딩 퍼즐 | [`SlidingPuzzle/`](SlidingPuzzle) | 탭 + 인덱스 스왑 | 구현 전에 스펙(Game Rule Spec) 먼저 작성하는 워크플로우 검증 |
| 4 | 메모리 매칭 | [`MemoryMatch/`](MemoryMatch) | 탭 + 쌍 비교 | 부트스트랩/검증 스크립트 실전 투입 검증 |

## 문서 / 스크립트

- [`document/casual-game-agent-design.md`](document/casual-game-agent-design.md) — Core Spec(공용 아키텍처) / Game Rule Spec(장르별 규칙 10항목 체크리스트) / 에셋 파이프라인 / 에이전트 오케스트레이션 8단계 / 검증 체크리스트 / 게임별 소요시간·타임라인 / 신규 게임 요청 가이드
- [`document/resume_project_brief.md`](document/resume_project_brief.md) — 이 프로젝트 요약(이력서/포트폴리오 문구화용 원료 문서)
- [`scripts/bootstrap_project.sh`](scripts/bootstrap_project.sh) — 가장 최근에 완성된 프로젝트를 새 장르 프로젝트로 복제 + 네임스페이스/asmdef/productName 일괄 치환
- [`scripts/verify_project.sh`](scripts/verify_project.sh) — 컴파일 확인 → Importer/씬세팅/로컬라이제이션/오디오 파이프라인 실행 → EditMode/PlayMode 테스트까지 배치모드 자동 검증

## 기술 스택

Unity 6000.4.2f1, C#, 2D URP, Unity Input System, Unity Localization(ko/en/ja/zh-Hans), Unity IAP, DOTween, IronSource(LevelPlay), NUnit(EditMode/PlayMode), bash, Python.
