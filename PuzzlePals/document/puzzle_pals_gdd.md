# Puzzle Pals — Baby Shape Puzzle Game
## Game Design Document v1.0

---

## Overview
- **Genre**: Casual Puzzle / Educational (Ages 1–5)
- **Core Loop**: 쪼개진 도형 조각을 드래그해서 실루엣에 맞추면 귀여운 캐릭터/사물이 완성되고 다음 퍼즐로 넘어감
- **Session Length**: 1–3분 (퍼즐 1개당 30초~1분)
- **Platform**: Mobile (Unity 6 LTS, 2D URP)
- **Total Levels**: 100개 (사물 50 + 동물 50, 난이도 점진 상승)
- **Monetization**: 광고 기반 (인터스티셜 매 5 퍼즐) + 힌트 시스템 + IAP

---

## Core Mechanic
플레이어는 화면 하단에 흩어진 도형 조각들을 손가락으로 드래그한다.
조각이 올바른 위치 근처에 놓이면 자동으로 스냅되며 제자리를 찾는다.
모든 조각이 맞춰지면 캐릭터가 살아나는 애니메이션(눈 깜빡임, 점프)이 재생되고 클리어 연출이 나온다.

---

## Level Content — 100 Levels

### 사물 (Objects) — Levels 1–50

| # | Name (EN) | Korean | Pieces | Difficulty |
|---|-----------|--------|--------|------------|
| 1 | Apple | 사과 | 2 | ★☆☆☆☆ |
| 2 | Ball | 공 | 2 | ★☆☆☆☆ |
| 3 | Sun | 태양 | 2 | ★☆☆☆☆ |
| 4 | Heart | 하트 | 2 | ★☆☆☆☆ |
| 5 | Star | 별 | 3 | ★☆☆☆☆ |
| 6 | Cup | 컵 | 3 | ★☆☆☆☆ |
| 7 | Balloon | 풍선 | 3 | ★☆☆☆☆ |
| 8 | Flower | 꽃 | 3 | ★☆☆☆☆ |
| 9 | House | 집 | 3 | ★☆☆☆☆ |
| 10 | Cake | 케이크 | 3 | ★☆☆☆☆ |
| 11 | Car | 자동차 | 4 | ★★☆☆☆ |
| 12 | Train | 기차 | 4 | ★★☆☆☆ |
| 13 | Boat | 배 | 4 | ★★☆☆☆ |
| 14 | Airplane | 비행기 | 4 | ★★☆☆☆ |
| 15 | Bicycle | 자전거 | 4 | ★★☆☆☆ |
| 16 | Umbrella | 우산 | 4 | ★★☆☆☆ |
| 17 | Rainbow | 무지개 | 4 | ★★☆☆☆ |
| 18 | Ice Cream | 아이스크림 | 4 | ★★☆☆☆ |
| 19 | Pencil | 연필 | 4 | ★★☆☆☆ |
| 20 | Book | 책 | 4 | ★★☆☆☆ |
| 21 | Tree | 나무 | 5 | ★★☆☆☆ |
| 22 | Kite | 연 | 5 | ★★☆☆☆ |
| 23 | Lollipop | 막대사탕 | 5 | ★★☆☆☆ |
| 24 | Piano | 피아노 | 5 | ★★★☆☆ |
| 25 | Rocket | 로켓 | 5 | ★★★☆☆ |
| 26 | Camera | 카메라 | 5 | ★★★☆☆ |
| 27 | Guitar | 기타 | 5 | ★★★☆☆ |
| 28 | Lighthouse | 등대 | 5 | ★★★☆☆ |
| 29 | Clock | 시계 | 5 | ★★★☆☆ |
| 30 | Castle | 성 | 6 | ★★★☆☆ |
| 31 | Submarine | 잠수함 | 6 | ★★★☆☆ |
| 32 | Telescope | 망원경 | 6 | ★★★☆☆ |
| 33 | Lantern | 등불 | 6 | ★★★☆☆ |
| 34 | Tractor | 트랙터 | 6 | ★★★★☆ |
| 35 | Spaceship | 우주선 | 6 | ★★★★☆ |
| 36 | Bridge | 다리 | 6 | ★★★★☆ |
| 37 | Windmill | 풍차 | 6 | ★★★★☆ |
| 38 | Ferris Wheel | 관람차 | 7 | ★★★★☆ |
| 39 | Hot Air Balloon | 열기구 | 7 | ★★★★☆ |
| 40 | Crown | 왕관 | 7 | ★★★★☆ |
| 41 | Diamond | 다이아몬드 | 7 | ★★★★☆ |
| 42 | Snowflake | 눈꽃 | 7 | ★★★★☆ |
| 43 | Treasure Chest | 보물상자 | 7 | ★★★★☆ |
| 44 | Cactus | 선인장 | 7 | ★★★★★ |
| 45 | Pirate Ship | 해적선 | 8 | ★★★★★ |
| 46 | Robot | 로봇 | 8 | ★★★★★ |
| 47 | Fire Truck | 소방차 | 8 | ★★★★★ |
| 48 | Space Station | 우주정거장 | 8 | ★★★★★ |
| 49 | Dragon Ball | 드래곤볼 | 8 | ★★★★★ |
| 50 | Magic Wand | 마법 지팡이 | 8 | ★★★★★ |

### 동물 (Animals) — Levels 51–100

| # | Name (EN) | Korean | Pieces | Difficulty |
|---|-----------|--------|--------|------------|
| 51 | Cat | 고양이 | 2 | ★☆☆☆☆ |
| 52 | Dog | 강아지 | 2 | ★☆☆☆☆ |
| 53 | Duck | 오리 | 2 | ★☆☆☆☆ |
| 54 | Fish | 물고기 | 3 | ★☆☆☆☆ |
| 55 | Chick | 병아리 | 3 | ★☆☆☆☆ |
| 56 | Rabbit | 토끼 | 3 | ★☆☆☆☆ |
| 57 | Bear | 곰 | 3 | ★☆☆☆☆ |
| 58 | Penguin | 펭귄 | 3 | ★☆☆☆☆ |
| 59 | Pig | 돼지 | 3 | ★☆☆☆☆ |
| 60 | Frog | 개구리 | 3 | ★★☆☆☆ |
| 61 | Elephant | 코끼리 | 4 | ★★☆☆☆ |
| 62 | Giraffe | 기린 | 4 | ★★☆☆☆ |
| 63 | Lion | 사자 | 4 | ★★☆☆☆ |
| 64 | Monkey | 원숭이 | 4 | ★★☆☆☆ |
| 65 | Cow | 젖소 | 4 | ★★☆☆☆ |
| 66 | Horse | 말 | 4 | ★★☆☆☆ |
| 67 | Sheep | 양 | 4 | ★★☆☆☆ |
| 68 | Panda | 판다 | 4 | ★★☆☆☆ |
| 69 | Koala | 코알라 | 4 | ★★☆☆☆ |
| 70 | Turtle | 거북이 | 4 | ★★☆☆☆ |
| 71 | Butterfly | 나비 | 5 | ★★★☆☆ |
| 72 | Owl | 부엉이 | 5 | ★★★☆☆ |
| 73 | Peacock | 공작새 | 5 | ★★★☆☆ |
| 74 | Crab | 게 | 5 | ★★★☆☆ |
| 75 | Octopus | 문어 | 5 | ★★★☆☆ |
| 76 | Flamingo | 플라밍고 | 5 | ★★★☆☆ |
| 77 | Hedgehog | 고슴도치 | 5 | ★★★☆☆ |
| 78 | Fox | 여우 | 5 | ★★★☆☆ |
| 79 | Deer | 사슴 | 5 | ★★★☆☆ |
| 80 | Parrot | 앵무새 | 5 | ★★★☆☆ |
| 81 | Hippo | 하마 | 6 | ★★★★☆ |
| 82 | Crocodile | 악어 | 6 | ★★★★☆ |
| 83 | Polar Bear | 북극곰 | 6 | ★★★★☆ |
| 84 | Zebra | 얼룩말 | 6 | ★★★★☆ |
| 85 | Gorilla | 고릴라 | 6 | ★★★★☆ |
| 86 | Kangaroo | 캥거루 | 6 | ★★★★☆ |
| 87 | Narwhal | 일각고래 | 6 | ★★★★☆ |
| 88 | Axolotl | 아홀로틀 | 6 | ★★★★☆ |
| 89 | Sloth | 나무늘보 | 7 | ★★★★☆ |
| 90 | Chameleon | 카멜레온 | 7 | ★★★★☆ |
| 91 | Dragon | 용 | 7 | ★★★★★ |
| 92 | Phoenix | 불사조 | 7 | ★★★★★ |
| 93 | Unicorn | 유니콘 | 7 | ★★★★★ |
| 94 | Mermaid Cat | 인어고양이 | 7 | ★★★★★ |
| 95 | Dinosaur | 공룡 | 8 | ★★★★★ |
| 96 | Shark | 상어 | 8 | ★★★★★ |
| 97 | Toucan | 투칸 | 8 | ★★★★★ |
| 98 | Snow Leopard | 설표 | 8 | ★★★★★ |
| 99 | Giant Squid | 대왕오징어 | 8 | ★★★★★ |
| 100 | Rainbow Dragon | 무지개 용 | 8 | ★★★★★ |

---

## Difficulty Curve

| Level Range | Piece Count | Snap Threshold | Piece Drift Speed |
|-------------|-------------|----------------|-------------------|
| 1–10 | 2–3 | 넓음 (1.8f) | 없음 (정적 배치) |
| 11–25 | 3–4 | 보통 (1.4f) | 없음 |
| 26–45 | 4–5 | 보통 (1.2f) | 없음 |
| 46–60 | 5–6 | 좁음 (1.0f) | 없음 |
| 61–80 | 6–7 | 좁음 (0.9f) | 없음 |
| 81–100 | 7–8 | 매우 좁음 (0.8f) | 선택적 미세 이동 |

> 유아 타겟이므로 실패 개념 없음 — 틀린 위치에 놓으면 조각이 원래 자리로 튕겨 돌아옴

---

## LevelDataSO 구조

### 개요
- 레벨마다 ScriptableObject 파일 1개 (`Level_01.asset` ~ `Level_100.asset`)
- 디자이너는 **원본 이미지 1장 + 조각 수**만 제공
- 조각 분할 및 스냅존 위치는 **코드가 자동 계산**

### 데이터 구조 (LevelDataSO.cs)
```csharp
[CreateAssetMenu(fileName = "Level_XX", menuName = "PuzzlePals/LevelData")]
public class LevelDataSO : ScriptableObject {
    [Header("기본 정보")]
    public int levelNumber;           // 레벨 번호 (1~100)
    public string characterNameKey;   // Localization Key (예: "char_apple")
    public Sprite characterSprite;    // 원본 캐릭터 이미지 1장 (디자이너 제공)
    public int pieceCount;            // 조각 수 (2~8, Difficulty Curve 참고)

    [Header("난이도")]
    public float snapThreshold;       // 스냅 민감도 (Difficulty Curve 참고)
    public bool enablePieceDrift;     // 레벨 81+ 미세 이동 여부

    [Header("비주얼")]
    public Color backgroundColor;     // 배경색 (파스텔 계열)
}
```

### 조각 자동 분할 로직 (PuzzleBoard.cs)
```
원본 characterSprite 1장
        ↓
pieceCount 값에 따라 이미지를 분할
  - 2조각: 상/하 또는 좌/우
  - 3조각: 상/중/하
  - 4조각: 2x2 그리드
  - 5~6조각: 불규칙 분할 (미리 정의된 분할 패턴 사용)
  - 7~8조각: 3x3 그리드 (일부 셀 병합)
        ↓
각 조각의 원본 위치 = 스냅존 위치로 자동 등록
        ↓
조각들을 화면 하단 1/3 영역에 랜덤 배치 (겹침 방지)
```

### 디자이너 작업 범위
| 항목 | 담당 | 비고 |
|------|------|------|
| 원본 캐릭터 이미지 | 디자이너 | 1레벨당 1장, 권장 512x512px |
| 조각 수 | 기획자 | Difficulty Curve 표 참고 |
| 배경색 | 디자이너 | 파스텔 계열 Color 값 |
| 조각 분할/위치 | 자동 계산 | 코드가 처리 |
| 스냅존 위치 | 자동 계산 | 원본 이미지 기준 자동 등록 |

### 폴더 구조
```
Assets/
  Resources/
    Levels/
      Level_01.asset   ← 사과 (2조각)
      Level_02.asset   ← 공 (2조각)
      ...
      Level_100.asset  ← 무지개 용 (8조각)
  Sprites/
    Characters/
      apple.png
      ball.png
      ...
      rainbow_dragon.png
```

---

## Hint System Design

### 힌트 획득 방법 (HintManager.cs 핵심 로직)

```
[자연 충전]
- 30분마다 힌트 1개 자동 충전
- 최대 자연 충전 보유량: 3개
- 앱 종료 후에도 오프라인 시간 계산 (PlayerPrefs에 DateTime 저장)

[광고 시청으로 추가 획득]
- 하루 3번까지 광고 시청 → 힌트 1개 획득
- 자정 기준으로 리셋 (DateTime.Today 비교)
- UI: "광고 보기" 버튼 (하루 3회 소진 시 비활성화)

[구매]
- 힌트 5개 묶음: $0.99
- 힌트 20개 묶음: $2.99
- 힌트 무제한 (30일): $4.99
- 언제든지 구매 가능 (인게임 상점 또는 힌트 부족 시 팝업)
```

### HintManager.cs 상태 다이어그램

```
[힌트 사용 버튼 클릭]
        ↓
  hintCount > 0?
  ├─ YES → 힌트 사용, hintCount--, 정답 위치 0.8초 반짝임
  └─ NO  → 힌트 없음 팝업
              ├─ [광고 보기] → adWatchedToday < 3? → 광고 재생 → hintCount++
              │                                    └─ 오늘 3회 초과 → 비활성
              └─ [구매하기] → IAP 상점 팝업
```

---

## Localization — 전세계 언어 지원

### 지원 언어 (LocalizationManager.cs)

| Code | Language | 언어 |
|------|----------|------|
| ko | Korean | 한국어 |
| en | English | 영어 |
| ja | Japanese | 日本語 |
| zh-CN | Chinese Simplified | 简体中文 |
| zh-TW | Chinese Traditional | 繁體中文 |
| es | Spanish | Español |
| fr | French | Français |
| de | German | Deutsch |
| pt | Portuguese | Português |
| it | Italian | Italiano |
| ru | Russian | Русский |
| ar | Arabic | العربية |
| hi | Hindi | हिन्दी |
| id | Indonesian | Bahasa Indonesia |
| tr | Turkish | Türkçe |
| th | Thai | ภาษาไทย |
| vi | Vietnamese | Tiếng Việt |
| nl | Dutch | Nederlands |
| pl | Polish | Polski |
| sv | Swedish | Svenska |

### 구현 방식

```
Unity Localization Package (com.unity.localization) 사용
- String Tables: UI 텍스트 전부 (버튼, 메뉴, 캐릭터 이름)
- Asset Tables: 언어별 폰트 (아랍어 RTL 처리, 중국어/일본어 CJK 폰트)
- 시작 시 SystemLanguage 자동 감지 → 미지원 언어는 English 기본값
- 설정 메뉴에서 수동 변경 가능
```

---

## Game Feel Targets

- **Juice**:
  - 조각 집을 때: 0.9x로 살짝 눌리는 Scale DOTween
  - 스냅 성공: 별 파티클 5개 burst + "딩!" SFX + Punch Scale
  - 완성 시: 무지개 파티클 폭발 + DOShakePosition (0.3, 0.4s) + 동요풍 팡파레
  - 캐릭터 완성 애니메이션: 눈 깜빡 → 점프 2회 → 손 흔들기
- **배경**: 파스텔 단색 (레벨마다 색상 변경, LevelDataSO에 `backgroundColor` 필드)
- **Sound Cue**: 조각 집기 → 팝, 스냅 → 딩, 완성 → 팡파레, BGM → 부드러운 동요 루프

---

## Unity Scene Structure

```
Assets/
  Scenes/
    MainMenu.unity
    LevelSelect.unity
    Gameplay.unity
    Complete.unity
  Scripts/
    Core/
      GameManager.cs
      LevelManager.cs
      SceneLoader.cs
    Puzzle/
      PuzzlePiece.cs
      PuzzleBoard.cs
      SnapZone.cs
      PuzzleCompleteChecker.cs
    Data/
      LevelDataSO.cs
    Systems/
      HintManager.cs
      AdManager.cs
      IAPManager.cs
      AudioManager.cs
      LocalizationManager.cs
      SaveManager.cs
    UI/
      UIAnimator.cs
      MainMenuUI.cs
      LevelSelectUI.cs
      HUDController.cs
      CompleteUI.cs
      HintPopupUI.cs
      ShopPopupUI.cs
      SettingsUI.cs
  Prefabs/
    PuzzlePiece.prefab
    SnapZone.prefab
    CompletionVFX.prefab
    StarBurst.prefab
    CharacterComplete.prefab
    HintFlash.prefab
  ScriptableObjects/
    Levels/
      Level_001_Apple.asset
      Level_002_Ball.asset
      ... (100 total)
  UI/
    MainMenuCanvas
    LevelSelectCanvas
    HUDCanvas
    CompleteCanvas
    HintPopupCanvas
    ShopPopupCanvas
    SettingsCanvas
  Art/             ← 플레이스홀더: Unity 기본 도형 사용, 추후 교체
    Shapes/        ← 100개 캐릭터 실루엣 sprites (흰색 채움, 파스텔 윤곽선)
    Backgrounds/   ← 파스텔 단색 텍스처 (레벨별)
    UI/            ← 버튼, 아이콘, 패널 sprites
  Fonts/
    Default.ttf    ← 둥근 고딕체 (라틴)
    CJK.ttf        ← 한중일 폰트
    Arabic.ttf     ← 아랍어 RTL 폰트
    Hindi.ttf      ← 데바나가리 폰트
```

---

## Implementation Tasks

### Task 1 — Project Setup
- [ ] Unity 6 LTS, 2D URP 프로젝트 생성
- [ ] Target platform: Android (API 28+) / iOS
- [ ] 위 폴더 구조 생성
- [ ] Package Manager에서 설치:
  - `com.unity.localization` (Unity Localization)
  - `com.unity.cinemachine`
  - DOTween (Asset Store 무료)
  - Unity Ads SDK
  - Unity IAP (com.unity.purchasing)
- [ ] `LevelDataSO.cs` ScriptableObject 정의:
  ```csharp
  [CreateAssetMenu(fileName = "Level", menuName = "PuzzlePals/Level")]
  public class LevelDataSO : ScriptableObject {
      public int levelIndex;
      public string characterNameKey;    // Localization key
      public Color backgroundColor;      // 파스텔 단색 배경
      public Sprite[] pieceSprites;      // 조각 이미지들
      public Sprite completedSprite;     // 완성본 이미지
      public Vector2[] snapPositions;    // 각 조각의 월드 정답 위치
      public float snapThreshold;        // 난이도별 스냅 거리
      public int pieceCount;
  }
  ```
- [ ] 레벨 1–10 ScriptableObject 인스턴스 생성 (Unity Rect 기본 도형으로 플레이스홀더)
- [ ] 빌드 설정: Bundle ID, 앱 아이콘 (512x512 플레이스홀더)

### Task 2 — 드래그 & 스냅 핵심 메카닉
- [ ] `PuzzlePiece.cs`:
  ```csharp
  public class PuzzlePiece : MonoBehaviour {
      [HideInInspector] public SnapZone targetZone;
      [HideInInspector] public bool isSnapped = false;
      private bool isDragging;
      private Vector3 dragOffset;
      private Vector3 homePosition;  // 초기 배치 위치 (잘못 놓으면 복귀)
      private Camera mainCam;

      void Start() { homePosition = transform.position; mainCam = Camera.main; }

      void OnMouseDown() {
          if (isSnapped) return;
          isDragging = true;
          dragOffset = transform.position - GetMouseWorld();
          transform.DOScale(1.15f, 0.1f);
          AudioManager.Instance.PlayPickup();
          GetComponent<SpriteRenderer>().sortingOrder = 10; // 맨 앞으로
      }

      void OnMouseDrag() {
          if (!isDragging) return;
          transform.position = GetMouseWorld() + dragOffset;
      }

      void OnMouseUp() {
          isDragging = false;
          transform.DOScale(1.0f, 0.1f);
          SnapZone closest = FindClosestZone();
          if (closest != null && !closest.isOccupied) {
              Snap(closest);
          } else {
              transform.DOMove(homePosition, 0.2f).SetEase(Ease.OutBack);
          }
      }

      void Snap(SnapZone zone) {
          isSnapped = true;
          zone.Occupy();
          transform.DOMove(zone.transform.position, 0.15f).SetEase(Ease.OutBack);
          transform.DOScale(1.0f, 0.1f);
          GetComponent<SpriteRenderer>().sortingOrder = 1;
          AudioManager.Instance.PlaySnap();
          ParticleSystem ps = Instantiate(VFXManager.Instance.snapParticle, transform.position, Quaternion.identity);
          Destroy(ps.gameObject, 1f);
          PuzzleCompleteChecker.Instance.Check();
      }

      SnapZone FindClosestZone() {
          SnapZone[] zones = FindObjectsOfType<SnapZone>();
          SnapZone best = null; float bestDist = targetZone == null ?
              PuzzleBoard.CurrentLevel.snapThreshold : PuzzleBoard.CurrentLevel.snapThreshold;
          foreach (var z in zones) {
              float d = Vector2.Distance(transform.position, z.transform.position);
              if (d < bestDist && !z.isOccupied) { best = z; bestDist = d; }
          }
          return best;
      }

      Vector3 GetMouseWorld() {
          Vector3 mp = Input.mousePosition; mp.z = 10f;
          return mainCam.ScreenToWorldPoint(mp);
      }
  }
  ```
- [ ] `SnapZone.cs`:
  ```csharp
  public class SnapZone : MonoBehaviour {
      public bool isOccupied = false;
      private SpriteRenderer sr;

      void Start() {
          sr = GetComponent<SpriteRenderer>();
          sr.color = new Color(1f, 1f, 1f, 0.25f); // 반투명 실루엣
      }
      public void Occupy() {
          isOccupied = true;
          sr.DOFade(0f, 0.2f); // 실루엣 사라짐
      }
      public void Flash() {  // 힌트 사용 시 반짝임
          sr.DOFade(0.8f, 0.3f).SetLoops(4, LoopType.Yoyo);
      }
  }
  ```
- [ ] `PuzzleBoard.cs`: LevelDataSO 기반으로 PuzzlePiece / SnapZone Instantiate, 조각 초기 배치 (화면 하단 1/3, 겹침 방지 알고리즘)
- [ ] 배경 색상: `Camera.main.backgroundColor = level.backgroundColor`

### Task 3 — 레벨 시스템 & 완성 감지
- [ ] `PuzzleCompleteChecker.cs` (싱글턴):
  ```csharp
  public void Check() {
      if (FindObjectsOfType<PuzzlePiece>().All(p => p.isSnapped)) OnPuzzleComplete();
  }
  void OnPuzzleComplete() {
      GameManager.Instance.SetState(GameState.Complete);
      CharacterComplete anim = Instantiate(characterCompletePrefab, Vector3.zero, Quaternion.identity);
      anim.Play(() => {
          VFXManager.Instance.PlayCompletion(Vector3.zero);
          AudioManager.Instance.PlayComplete();
          DOVirtual.DelayedCall(1.5f, () => UIManager.Instance.ShowCompletePanel());
      });
      SaveManager.Instance.MarkLevelComplete(LevelManager.Instance.currentLevelIndex);
      AdManager.Instance.OnLevelComplete();
  }
  ```
- [ ] `LevelManager.cs`:
  ```csharp
  public int currentLevelIndex;
  public LevelDataSO[] allLevels; // Inspector에서 100개 할당
  public static LevelDataSO CurrentLevel => Instance.allLevels[Instance.currentLevelIndex];
  public void LoadLevel(int index) { currentLevelIndex = index; SceneLoader.Instance.LoadGameplay(); }
  public void NextLevel() { LoadLevel(Mathf.Min(currentLevelIndex + 1, 99)); }
  ```
- [ ] `GameManager.cs` 상태 머신:
  ```csharp
  public enum GameState { Idle, Playing, Complete }
  public GameState State { get; private set; }
  public void SetState(GameState s) { State = s; OnStateChanged?.Invoke(s); }
  ```
- [ ] `SaveManager.cs`: `PlayerPrefs`로 클리어 레벨 저장, JSON 직렬화로 진행도 백업

### Task 4 — 힌트 시스템
- [ ] `HintManager.cs` (싱글턴):
  ```csharp
  const int MAX_FREE_HINTS = 3;
  const float HINT_RECHARGE_MINUTES = 30f;
  const int MAX_ADS_PER_DAY = 3;

  int hintCount;
  DateTime lastRechargeTime;
  int adWatchedToday;
  DateTime lastAdDate;

  void Update() {
      // 오프라인 충전 계산
      float minutesPassed = (float)(DateTime.Now - lastRechargeTime).TotalMinutes;
      if (minutesPassed >= HINT_RECHARGE_MINUTES && hintCount < MAX_FREE_HINTS) {
          int earned = Mathf.FloorToInt(minutesPassed / HINT_RECHARGE_MINUTES);
          hintCount = Mathf.Min(hintCount + earned, MAX_FREE_HINTS);
          lastRechargeTime = DateTime.Now;
          SaveHintData();
      }
      // 자정 광고 횟수 리셋
      if (DateTime.Today > lastAdDate.Date) { adWatchedToday = 0; lastAdDate = DateTime.Today; SaveHintData(); }
  }

  public void UseHint() {
      if (hintCount > 0) {
          hintCount--;
          SaveHintData();
          FlashCorrectZone();
      } else {
          UIManager.Instance.ShowHintPopup(); // 광고/구매 팝업
      }
  }

  public bool CanWatchAd() => adWatchedToday < MAX_ADS_PER_DAY;

  public void OnAdWatched() {
      adWatchedToday++;
      hintCount++;
      lastAdDate = DateTime.Today;
      SaveHintData();
  }

  void FlashCorrectZone() {
      // 아직 비어있는 SnapZone 중 랜덤 1개 선택하여 0.8초 반짝임
      var emptyZones = FindObjectsOfType<SnapZone>().Where(z => !z.isOccupied).ToArray();
      if (emptyZones.Length > 0) emptyZones[Random.Range(0, emptyZones.Length)].Flash();
  }

  void SaveHintData() {
      PlayerPrefs.SetInt("HintCount", hintCount);
      PlayerPrefs.SetString("LastRecharge", lastRechargeTime.ToString());
      PlayerPrefs.SetInt("AdWatchedToday", adWatchedToday);
      PlayerPrefs.SetString("LastAdDate", lastAdDate.ToString());
  }
  ```

### Task 5 — UI & 레벨 선택
- [ ] **MainMenuUI.cs**: 로고, Play 버튼 (DOTween Punch Scale 루프), 설정 버튼, 언어 선택 버튼
- [ ] **LevelSelectUI.cs**: ScrollView + GridLayoutGroup (5열), 레벨 버튼 100개, 클리어 시 별 표시, 잠금/해제 (PlayerPrefs)
- [ ] **HUDController.cs**:
  - 상단: 레벨 번호 (Localized)
  - 우상단: 힌트 버튼 (💡) + 현재 힌트 개수 배지
  - 좌상단: 홈 버튼
- [ ] **CompleteUI.cs**:
  - 별 3개 순서대로 팡! 팡! 팡! (DOTween Sequence, 0.3초 간격)
  - 캐릭터 이름 텍스트 (Localized: "🐶 강아지 완성!")
  - "다음" 버튼, "다시하기" 버튼, "홈" 버튼
- [ ] **HintPopupUI.cs**: "힌트가 없어요!" + 광고 시청 버튼 (남은 횟수 표시) + 구매 버튼
- [ ] **ShopPopupUI.cs**: 힌트 5개 / 20개 / 무제한 30일 IAP 버튼 3종
- [ ] **SettingsUI.cs**: BGM 볼륨, SFX 볼륨, 언어 선택 드롭다운

### Task 6 — Localization 구현
- [ ] Unity Localization Package 설정:
  ```
  Window > Asset Management > Localization Scene Controls
  20개 언어 Locale 추가
  ```
- [ ] String Table 생성: `UI_Strings`
  - 모든 버튼 텍스트, 캐릭터 이름, 팝업 메시지 항목 추가
  - 100개 캐릭터 이름 20개 언어로 번역 입력
- [ ] Asset Table: 언어별 폰트 할당 (아랍어 → RTL 활성화)
- [ ] `LocalizationManager.cs`:
  ```csharp
  void Awake() {
      SystemLanguage sys = Application.systemLanguage;
      string localeCode = MapSystemLanguage(sys); // 매핑 테이블
      LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales
          .Locales.FirstOrDefault(l => l.Identifier.Code == localeCode)
          ?? LocalizationSettings.AvailableLocales.Locales[0]; // 기본값 EN
  }
  public void SetLanguage(string code) { /* 수동 변경 */ }
  ```
- [ ] 아랍어 RTL 처리: `Canvas.rootCanvas` 에서 `RectTransform.localScale.x = -1` 미러링

### Task 7 — Audio & Juice
- [ ] `AudioManager.cs` (싱글턴):
  ```csharp
  public void PlayPickup() => PlaySFX(pickupClip, pitch: 1.0f);
  public void PlaySnap()   => PlaySFX(snapClip, pitch: 1.2f);
  public void PlayComplete() => PlaySFX(completeClip);
  public void PlayBGM()   => bgmSource.Play(); // loop, vol 0.4
  void PlaySFX(AudioClip c, float pitch = 1f) {
      sfxSource.pitch = pitch; sfxSource.PlayOneShot(c);
  }
  ```
- [ ] `UIAnimator.cs`:
  - 씬 전환: DOTween Fade (검정 오버레이, 0.3s in/out)
  - 버튼 터치: `DOPunchScale(Vector3.one * 0.15f, 0.3f)`
- [ ] CompletionVFX.prefab: 무지개색 별 파티클 Burst 100개, 2초
- [ ] SnapParticle.prefab: 별 모양 파티클 5개 Burst
- [ ] CharacterComplete.prefab: Animator로 Idle → Blink → Jump × 2 → Wave 시퀀스
- [ ] 카메라 셰이크 (완성 시): `DOShakePosition(0.4f, 0.3f, 10, 45f)`
- [ ] 모든 AudioClip: `// TODO: Replace with actual audio file` 주석 포함

### Task 8 — 광고 & IAP
- [ ] `AdManager.cs`:
  ```csharp
  int roundsSinceAd = 0;
  const int AD_INTERVAL = 5;

  void Init() => Advertisement.Initialize(gameId, testMode: true);

  public void OnLevelComplete() {
      roundsSinceAd++;
      if (roundsSinceAd >= AD_INTERVAL) { ShowInterstitial(); roundsSinceAd = 0; }
  }
  public void ShowInterstitial() => Advertisement.Show("Interstitial_Android");
  public void ShowRewarded(Action onSuccess) {
      // 광고 완료 콜백 → HintManager.Instance.OnAdWatched()
  }
  ```
- [ ] `IAPManager.cs` (Unity IAP):
  ```csharp
  // 상품 ID 정의
  const string HINTS_5    = "hints_5pack";      // $0.99
  const string HINTS_20   = "hints_20pack";     // $2.99
  const string HINTS_UNLIMITED = "hints_unlimited_30d"; // $4.99

  public void BuyHints5()       => controller.InitiatePurchase(HINTS_5);
  public void BuyHints20()      => controller.InitiatePurchase(HINTS_20);
  public void BuyUnlimited()    => controller.InitiatePurchase(HINTS_UNLIMITED);

  // ProcessPurchase 콜백에서 HintManager에 힌트 지급
  ```
- [ ] 테스트 모드: `testMode = true` (빌드 전 false로 변경)
- [ ] 영수증 검증: Unity Gaming Services 서버 사이드 검증 연결 (TODO 주석)

### Task 9 — Build & QA 체크리스트
- [ ] Android APK 빌드, API 28+ 에뮬레이터 실행
- [ ] iOS Xcode 빌드 확인 (시뮬레이터)
- [ ] 60fps 유지 확인 (Profiler로 드래그 중 프레임 드롭 체크)
- [ ] 터치 입력 반응성: 드래그 레이턴시 < 16ms
- [ ] 화면 비율: 9:16, 9:19.5, 4:3 (iPad) — UI 깨짐 없는지 확인 (Safe Area 적용)
- [ ] PlayerPrefs 힌트/진행도 저장 앱 종료 후 복구 확인
- [ ] 힌트 30분 충전 타이머 오프라인 정확성 테스트
- [ ] 하루 3회 광고 제한 자정 리셋 확인
- [ ] IAP 샌드박스 결제 테스트 (Android: 테스트 계정, iOS: Sandbox)
- [ ] 언어 20개 주요 화면 스크린샷 검수 (특히 아랍어 RTL, CJK 폰트)
- [ ] 레벨 1–20 전체 플레이스루 테스트
- [ ] 광고 플레이스홀더 정상 호출 확인

---

## Open Questions
- 캐릭터 아트 스타일 확정 필요 (제안: 두꺼운 윤곽선 + 파스텔 단색 채움, 눈 크고 귀여운 스타일)
- 힌트 무제한 30일 구독 vs 영구 구매 여부 TBD
- 레벨 클리어 후 스티커 수집 기능 추후 검토 (부모 참여형)
- 부모 대상 진행도 리포트 (어떤 퍼즐을 몇 번에 클리어했는지) 추후 기능
- 한국 앱 심의 등급 신청 필요 (전체이용가)
- Google Play / App Store 출시 지역 범위 TBD (전세계 권장)
