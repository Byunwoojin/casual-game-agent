# Puzzle Pals — 개발 진행 상황

작성일: 2026-04-16

---

## 프로젝트 개요

- **게임**: 유아용 캐릭터 조각 맞추기 퍼즐 (100 레벨, 2-8 조각)
- **플랫폼**: Unity 6.4.2f1, 2D URP, 모바일 세로
- **경로**: `/Users/woojin_1/MyGames`
- **참고 문서**: [puzzle_pals_gdd.md](puzzle_pals_gdd.md), [puzzle_pals_test_spec.md](puzzle_pals_test_spec.md)

---

## 완성된 기능

### Core Gameplay
- **PuzzleBoard** — 2~8 조각 분할 (1x2, 1x3, 2x2, 5조각, 2x3, 7조각, 2x4)
- **PuzzlePiece** — 드래그/스냅, 0.9x scale punch, snap PunchScale, **Piece Drift (레벨 81+)**
- **PuzzleCompleteChecker** — 완성 감지 + 이벤트
- **GameplayLoader** — `LevelManager.currentLevelIndex` → `Resources.Load<LevelDataSO>` → 조각/스냅존 동적 스폰

### 레벨 시스템
- **100 LevelDataSO** — GDD 캐릭터 표 기반 자동 생성 (`Assets/Resources/Levels/Level_001~100.asset`)
- **난이도 커브** (GDD §Difficulty Curve): snapThreshold 1.8 → 0.8, enablePieceDrift 81+
- **Character 이름 키** — `char_apple`, `char_cat` 등 (Localization 호환)
- **Placeholder 스프라이트 100종** — 10종 도형 (원/사각/삼각/다이아/별/하트/꽃/십자/구름/초승달) + 캐릭터별 색상

### UI
- **LevelSelect** — 100 버튼 ScrollView + GridLayout, 클리어 레벨 ★ 배지, 잠금 회색, **HOME + Settings 버튼 (좌/우상단)**
- **MainMenu** — PLAY 버튼 (DOPunchScale 루프) + Settings 버튼
- **Gameplay 헤더** (`GameplayHeaderUI`) — 상단 바: HOME 버튼 (좌) + 현재 레벨 번호 (중앙 위) + 캐릭터 이름 (Localized, 중앙 아래)
- **Complete** — NEXT 버튼 + HOME 버튼 (좌상단)
- **SettingsPopup** — 4개 언어 버튼 (ko/en/ja/zh-Hans) + SFX/BGM 슬라이더
- **ShopPopup** — 힌트 3종 구매 버튼 (5/20/Unlimited) + 광고 보기 버튼
- **SceneFader** — DontDestroyOnLoad 싱글톤, 0.2s 검정 페이드 (SceneLoader 자동 경유)
- **SafeAreaFitter** — iOS 노치/Android 네비바 회피
- **ResponsiveGridColumns** — 캔버스 폭에 따라 3~7 컬럼 자동
- **LocalizedTextBinder** — Legacy Text를 Localization key에 바인딩
- **표준 모바일 반응형** (letterbox 폐기, 코너 앵커 채택)
  - `EnsureUIRoot()` — Canvas 풀스크린 stretch + SafeAreaFitter (iOS 노치 / Android 네비바 회피)
  - 코너 버튼은 `AnchorTopLeft` / `AnchorTopRight` 헬퍼로 좌/우상단 코너에 고정 → 화면 비율과 무관하게 항상 보임
  - Player Settings 에서 Portrait 잠금 권장 — 가로 회전 차단

### Game Feel (DOTween)
- 조각 집기: DOScale(0.9, 0.1s) + pop SFX
- 스냅 성공: DOPunchScale(0.2, 0.3s) + 노란 별 파티클 5 burst + ding SFX
- 레벨 클리어: Camera DOShakePosition(0.4, 0.3) + 무지개 파티클 50 burst + fanfare SFX
- **캐릭터 완성 애니메이션** — 조각들 DOJump 2회 + DOPunchScale + DOPunchRotation (눈/손 부분 sprite 필요한 부분은 진짜 아트 대기)

### 시스템 매니저
- **AudioManager** — 싱글톤, PlayPop/Ding/Fanfare + BGM 루프 + SFX/BGM 볼륨 (PlayerPrefs 연동)
- **VFXManager** — 싱글톤, 파티클 프리팹 2종 관리 (snap burst, complete burst)
- **AdManager** — 싱글톤, 매 5레벨 Interstitial 트리거, Unity Dashboard Game ID 설정됨 (iOS 6092072, Android 6092073)
- **IAPManager** — 싱글톤, Unity IAP 5.x IDetailedStoreListener, 3종 상품 + HintManager 자동 적립
- **HintManager** — 30분 자연 충전 (최대 3) + 일 3회 광고 + SnapZone Flash 반짝임 + IAP 구매 적립 / 무제한
- **SaveManager** — PlayerPrefs 기반 진행도/힌트 저장
- **LevelManager** — 현재 레벨 인덱스 싱글톤

### Localization
- **4개 언어 지원**: 한국어 (기본), English (폴백), 日本語, 简体中文
- **StringTableCollection 2개**:
  - `Characters`: 100개 캐릭터 이름 × 4 locales
  - `UI`: 18개 UI 키 × 4 locales
- **SceneSetupTool이 버튼 라벨 자동 바인딩** — PLAY/HOME/NEXT/Settings 등 모든 정적 텍스트 버튼에 `LocalizedTextBinder` 부착
- **Gameplay 헤더의 캐릭터 이름** — `LocalizedString("Characters", data.characterNameKey)` 으로 동적 조회 → 언어 변경 시 즉시 반영

### 씬 간 네비게이션 흐름
| From | To | 방법 |
|------|----|------|
| MainMenu | LevelSelect | PLAY 버튼 |
| MainMenu | (Settings 팝업) | Settings 버튼 (우상단) |
| LevelSelect | MainMenu | HOME 버튼 (좌상단) |
| LevelSelect | (Settings 팝업) | Settings 버튼 (우상단) |
| LevelSelect | Gameplay | 레벨 버튼 클릭 |
| Gameplay | MainMenu | HOME 버튼 (헤더 좌측) |
| Gameplay | Complete | 모든 조각 스냅 → 자동 (1.5s 후) |
| Complete | Gameplay (다음 레벨) | NEXT 버튼 |
| Complete | MainMenu | HOME 버튼 (좌상단) |
| Complete | LevelSelect | NEXT (마지막 레벨 100 후 자동) |

### Editor 자동화 도구 (13개 메뉴)
| 메뉴 | 기능 |
|------|------|
| `Generate All Level Assets` | 100 LevelDataSO 자동 생성 |
| `Generate Placeholder Character Sprites` | 100 PNG 생성 + 할당 |
| `Regenerate Placeholder Character Sprites (Force)` | 강제 재생성 (도형 라이브러리 업데이트 반영) |
| `Generate Puzzle Prefabs` | PuzzlePiece/SnapZone 프리팹 |
| `Setup Localization (Locales + Settings)` | Locale 4개 + LocalizationSettings |
| `Populate String Tables` | 100 char + 18 UI 키 × 4 locales 일괄 입력 |
| `Setup All Scenes` | 4개 씬 일괄 세팅 |
| `Setup MainMenu / LevelSelect / Gameplay / Complete Only` | 씬별 개별 세팅 |
| `Setup LevelSelect (Full Grid)` | 100 버튼 그리드 씬 구성 |
| `Setup Gameplay (Loader-driven)` | Gameplay 씬을 GameplayLoader 기반으로 마이그레이션 |
| `Setup FX (Managers + Placeholder Particles)` | Audio/VFX 매니저 + 파티클 프리팹 |
| `Wire Real Levels Pipeline` | 위 3개 단계 + FX 한 번에 |

### 테스트 (43/43 통과, 100% 스펙 커버)
| 카테고리 | TC 개수 | 파일 |
|---------|--------|------|
| LevelData | 5 | `Tests/EditMode/LevelDataTests.cs` |
| PieceSplit | 6 | `Tests/EditMode/PieceSplitTests.cs` |
| SnapLogic | 5 | `Tests/EditMode/SnapLogicTests.cs` |
| HintManager | 8 | `Tests/EditMode/HintManagerTests.cs` |
| SaveManager | 4 | `Tests/EditMode/SaveManagerTests.cs` |
| LevelComplete | 4 | `Tests/EditMode/LevelCompleteTests.cs` |
| DragSnapFlow | 4 | `Tests/PlayMode/DragSnapFlowTests.cs` |
| SceneTransition | 4 | `Tests/PlayMode/SceneTransitionTests.cs` |
| AdTrigger | 3 | `Tests/PlayMode/AdTriggerTests.cs` |

---

## 설치된 외부 패키지

- `com.unity.localization` 1.5.5 — 4개 언어 지원
- `com.unity.purchasing` 5.2.1 — IAP
- `com.unity.ads` 4.17.0 (Advertisement Legacy, deprecated) — 스텁 상태
- DOTween (HOTween v2) 1.2.825 — Asset Store

### asmdef 구조
- `PuzzlePals` (runtime) — references: `Unity.Localization`, `Unity.Purchasing`
- `PuzzlePals.Editor` — references: `PuzzlePals`, `Unity.Localization`, `Unity.Localization.Editor`
- `PuzzlePals.EditMode.Tests`, `PuzzlePals.PlayMode.Tests`

---

## 스크립트 파일 구조

```
Assets/Scripts/
  Core/
    GameManager.cs
    LevelManager.cs
    SceneLoader.cs (페이드 전환 자동 경유)
  Data/
    LevelDataSO.cs
    LevelDataValidator.cs
  Editor/
    SceneSetupTool.cs (4개 씬 자동 세팅)
    LevelAssetGenerator.cs
    RealLevelsPipelineTool.cs (스프라이트 + 프리팹 + 씬 통합)
    FXSetupTool.cs
    LocalizationSetupTool.cs
    LocalizationStringTableTool.cs
    PuzzlePals.Editor.asmdef
  Gameplay/
    GameplaySceneController.cs (완성 감지 + 자동 씬 전환)
    GameplayLoader.cs (LevelDataSO 동적 스폰)
    CharacterCompleteAnimation.cs
  Interfaces/
    IAdProvider.cs
    IUIManager.cs
  Puzzle/
    PieceData.cs, PuzzleBoard.cs, PuzzlePiece.cs, SnapZone.cs
    PuzzleCompleteChecker.cs
  Systems/
    AdManager.cs, UnityAdsProvider.cs (스텁)
    AudioManager.cs, VFXManager.cs
    HintManager.cs
    SaveManager.cs
    IAPManager.cs
  UI/
    UIManager.cs
    LevelSelectController.cs (★ 배지 포함)
    GameplayHeaderUI.cs (상단 바: HOME + 레벨 + 캐릭터 이름)
    SettingsPopupUI.cs
    ShopPopupUI.cs
    SceneFader.cs
    LocalizedTextBinder.cs
    SafeAreaFitter.cs
    ResponsiveGridColumns.cs
    ButtonPulse.cs
  PuzzlePals.asmdef
```

---

## 수동 설정 필요 사항

### 코드로 못 하는 것

1. **AudioClip 4개** — popClip, dingClip, fanfareClip, bgmClip
   - freesound.org 에서 다운로드 → AudioManager 인스턴스 Inspector 슬롯에 할당
2. **진짜 캐릭터 아트 100장** — 현재 플레이스홀더 도형 상태
   - **임포트 도구 자동화 완료** (`CharacterArtImporter.cs`):
     - Step 1: Flaticon/Iconify/game-icons.net 등에서 PNG 100장 다운로드
     - Step 2: `Assets/Sprites/Characters/_Source/` 폴더에 복사
     - Step 3: 메뉴 `PuzzlePals > Import Character Art (Auto Name Match)` 실행
       - 파일명이 `apple.png`, `cat.png` 등 GDD 영문명이면 자동 매칭
       - 또는 `char_apple.png` 식 prefix 도 OK
     - 또는: `(Sequential Order)` 메뉴 — 파일명이 generic(icon-01 등)일 때 알파벳 순서대로 1~100 할당
     - 자동으로 Sprite 임포트 설정 + LevelDataSO 할당
   - 슬라이싱은 `PuzzleBoard.SplitSprite` 가 런타임에 자동 처리 (사용자 개별 컷 작업 불필요)
3. **Unity Dashboard 설정**
   - Unity Ads: Game ID 이미 입력됨 (iOS 6092072, Android 6092073). Advertisement Legacy는 deprecated 상태라 LevelPlay 마이그레이션 필요 시 UnityAdsProvider 내부만 재작성
   - IAP: Google Play Console / App Store Connect 에서 Product ID 등록 (`puzzlepals_hint_5`, `puzzlepals_hint_20`, `puzzlepals_hint_unlimited`)
4. **ShopPopupUI ↔ UIManager._hintPopupPanel 연결**
   - Inspector에서 수동 드래그, 또는 Editor 도구로 자동화 가능 (미구현)

### 번역 품질 검토 필요
- 한국어/영어 (GDD 원문) ✓
- 일본어/중국어 간체 — 제가 초벌 번역. 네이티브 검토 권장

---

## 알려진 한계 / 개선점

1. **Unity Ads** — Advertisement Legacy 2026-01-31부로 deprecated. LevelPlay 마이그레이션 필요 (UnityAdsProvider 스텁만 남긴 상태)
2. **캐릭터 완성 애니메이션** — GDD "눈 깜빡 → 점프 → 손 흔들기"는 부분 sprite(눈/손) 필요. 현재는 조각 전체 점프 + 회전으로 대체
3. **추가 14개 언어** (es/fr/de/pt/it/ru/ar/hi/id/tr/th/vi/nl/pl/sv/zh-TW) — Locale 추가 후 번역 데이터만 채우면 됨 (코드 변경 X)
4. **아랍어 RTL** 미구현
5. **Cloud Save / Analytics** 미연동
6. **플레이 테스트 기반 난이도 튜닝** — 현재는 GDD 수치 그대로, 실제 타겟 연령 반응 수집 후 조정 필요

---

## 다음 단계 제안 (우선순위)

1. **캐릭터 아트 작업** (Flaticon 등에서 100장 확보 → 교체)
2. **AudioClip 4개 추가** (freesound.org)
3. **플레이 테스트** → 난이도 커브 조정
4. **Google Play Console / App Store Connect IAP 상품 등록** → 샌드박스 구매 테스트
5. **LevelPlay 마이그레이션** → 실 광고 수익화
6. **추가 언어 번역 채우기**
7. **스토어 빌드** (Android APK, iOS IPA)

---

## Git / 버전 관리

프로젝트는 Unity VCS (Plastic SCM) 사용 중. Git 저장소가 아님.

---

## 참고 / 트러블슈팅

- **커스텀 asmdef 있는 프로젝트**는 외부 패키지 assembly 명시적 references 필요. PuzzlePals.asmdef / PuzzlePals.Editor.asmdef 참고.
- **DOTween UI 확장** (Image.DOFade 등)이 모듈 문제로 안 먹히면 `DOTween.To` + 수동 setter로 우회 (SceneFader.cs 참고)
- **모든 Editor 도구는 idempotent** — 재실행해도 안전. 기존 에셋/씬 보존
- **기본 해상도**: 1080x1920 (portrait), matchWidthOrHeight=0.5 (균형)
