# Handoff: 꽃피는 퍼즐 (Pastel Puzzle) — 캐주얼 직소 게임

## Overview

3060 여성을 타겟으로 하는 캐주얼 직소(Jigsaw) 퍼즐 모바일 게임의 UI 시안 패키지입니다. 광고 수익 모델을 핵심 동선에 통합하며, "도장깨기" 메타포로 30~50점의 그림 라이브러리를 순차적으로 잠금 해제하는 진행 구조를 가집니다.

총 8개 화면, 4개 팔레트(파스텔 정원 / 피치 크림 / 세이지 우드 / 라벤더 베리), 3개 퍼즐 테마(꽃·디저트·명화)를 다룹니다.

---

## About the Design Files

이 번들의 파일들은 **HTML로 만들어진 디자인 레퍼런스(prototype)** 입니다. 그대로 빌드하기 위한 production code가 아니라, **의도된 외관과 동작을 보여주는 시안**입니다.

작업 목표: 이 HTML 시안들을 **타겟 코드베이스의 기존 환경(React Native / Flutter / Native iOS·Android 등)에서 그곳의 패턴·라이브러리에 맞춰 다시 구현**하는 것. 환경이 아직 없으면, 모바일 게임에 적합한 프레임워크(예: React Native, Flutter)를 선택해 구현하면 됩니다.

HTML/CSS/JSX 코드를 그대로 복사하지 마세요 — 디자인 토큰, 컴포넌트 구조, 상호작용 규칙을 추출해서 옮기세요.

## Fidelity

**High-fidelity (hifi)**: 색상, 타이포그래피, 스페이싱, 컴포넌트 디테일이 모두 결정된 상태입니다. 단, 다음은 **placeholder**이므로 실제 자산으로 교체 필요:

- **퍼즐 이미지**: SVG 그라데이션 + 라벨로 표현됨. 실제로는 명화/꽃/디저트 사진 자산 필요 (`screens.jsx`의 `THEMES.art.items[0..3]`은 일부 placeholder PNG 경로 가짐 — `assets/art-*.png`)
- **이모지 아이콘**: 일부 개략 표현용. 브랜드/전체 톤 결정 후 실제 일러스트로 대체
- **광고 배너**: 회색 박스 + "AD" 라벨 — AdMob/IronSource 등 SDK 연결

---

## Target Audience & Tone

- **3060 여성**: 만화 같은 게임 UI 회피, 잡지·아카이브 감성 선호
- **가독성 우선**: 본문 12-14px, 핵심 카피 17-22px (영문이 아닌 한글 기준), 터치 영역 최소 36px
- **따뜻한 파스텔**: 채도 낮은 크림·로즈·세이지·라벤더, 골드 액센트
- **Pretendard** 한글 폰트 (CDN: `https://cdn.jsdelivr.net/gh/orioncactus/pretendard@v1.3.9/dist/web/static/pretendard.min.css`)
  - 시스템: `"Pretendard", "Apple SD Gothic Neo", system-ui, sans-serif`

---

## Design Tokens

### Color Palettes (4종 · 사용자가 선택 가능)

#### 1. 파스텔 정원 (garden) — **기본**
| Token | Hex | Usage |
|---|---|---|
| `bg` | `#FBF6EE` | 화면 배경 |
| `surface` | `#FFFFFF` | 카드 / 모달 |
| `surfaceAlt` | `#F4ECDF` | 보조 배경 (잠금 칸 등) |
| `primary` | `#D88A7A` | 메인 CTA, 강조 |
| `primaryDeep` | `#B7634F` | 호버/딥 액센트 |
| `accent` | `#9CB28A` | 세컨더리 (세이지) |
| `accentSoft` | `#E5EBD5` | 세이지 틴트 |
| `gold` | `#E0B86A` | 별점, 보상 |
| `text` | `#3D2E26` | 본문 |
| `textSoft` | `#8A7567` | 보조 텍스트 |
| `textMuted` | `#B8A595` | 잠금/비활성 |
| `line` | `#EADFCD` | 보더, 디바이더 |
| `danger` | `#D9636A` | 에러, 경고 |
| `starterA` | `#F7E3D2` | 그라데이션 시작 (피치) |
| `starterB` | `#E5EBD5` | 그라데이션 끝 (세이지) |

#### 2. 피치 크림 (peach)
- `bg #FEF3EC` / `surface #FFFFFF` / `surfaceAlt #FCE5D5`
- `primary #EB8B6B` / `primaryDeep #C66241`
- `accent #C19BB7` / `accentSoft #F0E1EB`
- `gold #E8B36C` / `text #3F2C24` / `textSoft #8C6E63` / `textMuted #BFA395`
- `line #F1DDCC` / `danger #D9636A`
- `starterA #FBD9C2` / `starterB #F0E1EB`

#### 3. 세이지 우드 (sage)
- `bg #F5F1E8` / `surface #FFFFFF` / `surfaceAlt #EAE2CF`
- `primary #9DB089` / `primaryDeep #6B8260`
- `accent #C8A27C` / `accentSoft #EFE2CF`
- `gold #D6B27A` / `text #36322A` / `textSoft #7C7466` / `textMuted #B0A795`
- `line #E2D9C4` / `danger #C77160`
- `starterA #E5EBD5` / `starterB #EFE2CF`

#### 4. 라벤더 베리 (lavender)
- `bg #F6F1F4` / `surface #FFFFFF` / `surfaceAlt #ECE0E8`
- `primary #B295C2` / `primaryDeep #7E5F94`
- `accent #D49AA8` / `accentSoft #F1DEE3`
- `gold #D8B57E` / `text #3A2E3B` / `textSoft #7E6F80` / `textMuted #B5A4B6`
- `line #E8DDE5` / `danger #C76B7A`
- `starterA #E2D2EA` / `starterB #F1DEE3`

> Implementation: 4팔레트를 토큰 객체로 보관하고, 사용자 설정에서 토글. 기본은 garden.

### Typography Scale (Pretendard 기준)

| Role | Size | Weight | Letter-spacing |
|---|---|---|---|
| Display (홈 헤더) | 19px | 700 | -0.025em |
| Title (헤더) | 17px | 700 | -0.02em |
| Subhead | 15px | 700 | -0.02em |
| Body | 13-14px | 500-600 | 0 |
| Caption | 11-12px | 500-600 | 0 ~ 0.02em |
| Micro/Label | 9-10px | 700-800 | 0.04-0.06em (uppercase 대용) |
| CTA Button | 13-14px | 700 | -0.01em |

### Spacing & Radius

- **Spacing**: 4 / 6 / 8 / 10 / 12 / 14 / 16 / 20 / 24 px
- **Radius**: 4 / 6 / 8 / 10 / 12 / 14 / 16 / 999 (pill)
- **Touch target**: 최소 36px, 권장 44px (CTA)

### Shadows

- Card subtle: `0 1px 3px rgba(0,0,0,0.04)`
- Card elevated: `0 2px 8px rgba(60,40,30,0.05)`
- Card hover/focus: `0 4px 14px {primary}40`
- Modal: `0 20px 60px rgba(60,40,30,0.25)`

---

## Screens / Views

### 1. 홈 화면 — **A · 순차 잠금 그리드 (도장깨기)** ★ 추천

**Component**: `HomeSequential` (`more-screens.jsx`)

**Purpose**: 사용자가 앱 진입 시 메인 화면. "내가 깬 작품들 + 다음 도전 작품"을 한눈에 보여주며 진행 동기 부여.

**Layout** (top → bottom):
1. **Header** (padding 14px 20px 12px, surface bg, border-bottom 1px line)
   - Left: greeting "오늘도 반가워요" (11px, textSoft) + title "그림 한 점, 시작해 볼까요" (19px / 700 / -0.025em)
   - Right: CoinPill (`2,840`) + Settings IconButton
2. **Progress section** (padding 14px 20px, surface bg, border-bottom)
   - "13점 / 30점 완성" (left, primaryDeep + textSoft) + "다음 마일스톤까지 +2점" (right, 11px / 600)
   - Progress bar: 6px height, line bg, fill = linear-gradient(90deg, primary, gold). 5/10/15/20/25에 흰색 2x6px 마일스톤 ticks
3. **"지금 도전" Card** (padding 14px 16px 8px, 카드 자체는 padding 12px, radius 16, surface, line border)
   - 64x64 thumbnail (level 14 작품) + 2px primary border 강조
   - Badge "지금 도전" (primary bg, white, 9px / 800 / 0.04em letter-spacing)
   - Title "14. {label}" (15px / 700 / -0.02em)
   - Meta "{pieces}조각 · 약 8분" (11px, textSoft)
   - CTA "계속하기 ▶" (text bg, surface fg, 13px / 700, padding 10x16, radius 999)
4. **Section divider** (padding 8px 16px 12px)
   - Left: "전체 작품" (12px / 700 / 0.06em letter-spacing, textSoft) + flex-1 1px line
   - Right: "순서대로 열려요" (11px, textMuted)
5. **3-column grid** (padding 0 16px 16px, gap 10px, gridTemplateColumns "1fr 1fr 1fr")
   - 30 tiles via `SequentialTile`
6. **Banner Ad** (`BannerAd` 컴포넌트, 50px 고정 높이)
7. **Bottom Nav** (`BottomNav`, 4-tab: home / collection / events / profile)

**SequentialTile component** (3가지 상태: cleared / current / locked)

| State | Border | Image | Caption row | Overlay |
|---|---|---|---|---|
| `cleared` (1~13) | 1px line | 컬러 PuzzleImage | 번호 (10px / 700, text color, surface bg) | top-right star badge (rgba(0,0,0,0.5) bg, ★1~3, gold/dim) |
| `current` (14) | 2px primary | 컬러 PuzzleImage + scale 1.02 + shadow `0 4px 14px {primary}40` | 번호 (10px / 800, white, primary bg) | bottom gradient `linear-gradient(180deg, transparent 50%, {primary}90 100%)` + "NOW" 라벨 (8px / 800 / 0.06em white) |
| `locked` (15+) | 1px line | surfaceAlt bg + diagonal hatch pattern (`repeating-linear-gradient(45deg, {line} 0 1px, transparent 1px 6px)`, opacity 0.6) + 14px lock icon center | 번호 (10px / 700, textMuted, surface bg) | — |

**State data**:
```js
{
  level: 1..30,
  state: 'cleared' | 'current' | 'locked',
  stars: 0..3,                 // cleared만 의미
  pieces: 36 + floor(i/5)*24,  // 36→60→84→108→132 점진 증가
  thumb: <image>,
  label: <string>,
}
```

**State transitions**:
- 사용자가 `current` 완료 → 해당 슬롯 `cleared` (별점 산정), 다음 슬롯 `locked → current`
- 잠긴 슬롯 탭: 토스트 "이전 작품을 먼저 완성해 주세요"

---

### 2. 홈 화면 (대안)

#### B · 스토리 큰 카드 (`HomeStory`)
- 단일 큰 히어로 카드(다음 작품 풀 사이즈 미리보기) + 아래 "이번 주 컬렉션" 가로 스크롤
- 신규 유저 / 큐레이션 강조 시 유리

#### C · 미니멀 리스트 (`HomeMinimal`)
- 세로 리스트, 1줄당 큰 카드 1개. 클리어/현재/잠금 상태 동일 적용
- 단순함이 강점이지만 30+ 항목엔 스크롤 길어짐

#### D · 갤러리 그리드 (`HomeGallery`) — 참고용
- 진행 개념 약함. 라이브러리 탐색용

---

### 3. 게임 플레이 화면 (2안)

#### A · 클래식 (`PlayClassic`)
- 상단 헤더 (back / 작품명 / 일시정지) + 타이머 + 조각 카운터(64/144 등) + 힌트 버튼
- 중앙: 4:3 퍼즐 보드(체커 패턴 빈 격자) + 부분 완성된 조각들
- 하단: 미정렬 조각 트레이 (가로 스크롤) + 배너 광고
- **힌트 버튼 → 보상형 광고 모달** 트리거

#### B · 미리보기 모드 (`PlayPreview`)
- 상단에 완성 이미지 토글 ("전체 보기" / "윤곽만") — 3060 사용자가 자주 사용
- 나머지는 클래식과 동일

---

### 4. 완성 화면 (2안)

#### A · 셀러브레이션 액자 (`CompleteCelebration`)
- 풀스크린 축하: 상단 색종이 SVG → 골드 액자에 완성 작품 큰 사이즈
- 메타: 작품명 + 소요 시간 + 별점 산정 (★1~3)
- CTA 2개: "갤러리에서 보기" (primary) + "다음 작품 →" (text bg)
- "다음 작품" 진입 시 3-5판마다 전면 광고

#### B · 컬렉션 도감 안내 (`CompleteCollection`)
- A와 동일하나 하단에 "13/30 → 14/30 수집 완료!" 진행 강조
- 5의 배수 도달 시 "축하 보상 +50 코인" 추가 메시지

---

### 5. 보상형 광고 모달 (`RewardedAdModal`)

**Trigger**: 힌트 버튼 탭 시 즉시
**Layout**: bottom sheet (radius 24 top, surface bg, padding 24)
- 아이콘 (40px 전구 + sparkle) + 타이틀 "광고 보고 힌트 받기" (18px / 700)
- 설명 "30초 광고를 끝까지 보면 힌트 1개를 드려요" (13px, textSoft)
- 보상 표시: "💡 힌트 +1 · 즉시 사용 가능" (accentSoft bg pill)
- CTA: "광고 보기" (primary bg, white, 14px / 700, padding 14x, radius 999) / "취소" (텍스트 버튼)

---

### 6. 갤러리 (2안)

#### A · 미술관 액자 진열 (`GalleryMuseum`)
- 1F / 2F 벽으로 구분된 액자 그리드
- 클리어 = 골드 액자(linear-gradient 145deg `#E8C99A → #B98552 → #E8C99A`, padding 5-6, shadow `0 6px 14px rgba(60,40,20,0.22)`)
- 잠금 = 회색 액자, opacity 0.25 + grayscale 0.9
- 별점 ★1~3 작품은 우상단 골드 배지

#### B · 도감 그리드 (`GalleryCodex`)
- 3열 그리드, 통계(13/50, ★3 4개, 총 클리어 21회) + 필터 칩(전체/완성/잠금/★3)
- 다시 푼 횟수 ×2, ×3 표시
- 하단 "다시 풀어보기" CTA → 재플레이 시 광고 1회 더 노출 가능 (수익 추가)

---

## Interactions & Behavior

### Navigation Flow
```
Home (Sequential) → tap "지금 도전" 카드 → Play (Classic or Preview)
                  → tap cleared 슬롯 → Gallery 해당 작품 상세 (다시 풀기 진입)
                  → tap locked 슬롯 → Toast "이전 작품을 먼저 완성해 주세요"
                  
Play → 힌트 버튼 → RewardedAdModal → "광고 보기" → (광고) → 힌트 적용
     → 완성 → CompleteCelebration → "다음 작품" → (3-5판마다 전면광고) → Play
                                  → "갤러리" → GalleryMuseum or Codex
```

### Animations
- **잠금 해제**: 다음 칸 locked → current 전환 시 hatch 패턴 fade-out + scale 0.95→1.02 + primary border draw-in (300ms ease-out)
- **별점 등장**: 완성 화면에서 ★ 1개씩 0.3s 간격 pop (scale 0→1.2→1, ease-back)
- **CTA tap**: scale 0.96 (100ms) + primary shadow expand
- **모달 진입**: bottom sheet slide-up 250ms ease-out + backdrop fade

### Tap States
- 모든 button/card: active 시 opacity 0.85 또는 scale 0.97
- locked 칸은 인터랙션 없음 (또는 토스트만)

---

## State Management

### 핵심 상태
```ts
interface AppState {
  // 진행
  currentLevel: number;        // 1..N (다음 도전할 레벨)
  clearedLevels: Map<number, { stars: 1|2|3; clearedAt: Date; replays: number }>;
  
  // 자원
  coins: number;
  hearts: number;              // 생명, 최대 5, 30분당 1 회복
  hints: number;
  
  // 설정
  paletteKey: 'garden' | 'peach' | 'sage' | 'lavender';
  themeKey: 'flower' | 'dessert' | 'art';
  bgmEnabled: boolean;
  sfxEnabled: boolean;
  
  // 광고
  lastInterstitialAt: Date;    // 3-5판마다 전면광고 쿨다운 체크용
  rewardedAdLoaded: boolean;
}

interface PlayState {
  level: number;
  pieces: Piece[];             // {id, correctPos, currentPos, placed}
  startedAt: Date;
  hintsUsed: number;
  paused: boolean;
}
```

### 데이터 페칭
- 작품 라이브러리: 클라이언트 번들 또는 CDN에서 메타 + 이미지 lazy-load
- 진행 상태: 로컬 우선 (AsyncStorage / SharedPreferences) + 서버 동기화 (선택적 계정 연동)

---

## 광고 수익 통합 지점 (★ 핵심)

| 위치 | 형태 | 트리거 | 빈도 |
|---|---|---|---|
| 홈/갤러리/플레이 하단 | 배너 (50dp) | 화면 진입 시 상시 | 항상 |
| 힌트 버튼 | 보상형 (Rewarded) | 사용자 탭 | 사용자 의지 |
| 다음 퍼즐 진입 | 전면 (Interstitial) | "다음 작품" 탭 | 3-5판마다 (쿨다운) |
| 갤러리 다시 풀기 | 전면 또는 보상형 | "다시 풀기" 탭 | 매 회 또는 첫 회 |

### 빈도 권장
- 3060 여성 타겟은 광고 거부감 적당 → 너무 자주 노출 시 이탈
- 전면 광고는 **3판당 1회** 시작, A/B로 5판당 1회와 비교
- 보상형은 사용자 명시적 선택이므로 노출 제한 없음

---

## Components Inventory

`screens.jsx`에 정의된 공통 컴포넌트:

- `Icon({ name, size, color, strokeWidth })` — 24x24 viewBox SVG. names: back, settings, trophy, home, calendar, lock, play, star, coin, bulb, eye, image, pause, swap, check, x, plus, sparkle, flower, crown 등
- `IconBtn({ size, children })` — 36x36 원형 surface 버튼
- `CoinPill({ value })` — 코인 잔액 pill (gold accent)
- `BannerAd({ P })` — 배너 광고 placeholder (50px 회색 + "AD" 라벨)
- `BottomNav({ active })` — 4-tab 하단 네비 (home / collection / events / profile)
- `PuzzleImage({ item, size, showGrid, locked })` — 퍼즐 썸네일 (그라데이션 + 라벨, 또는 `item.img`가 있으면 실제 이미지)

---

## Files in This Bundle

```
design_handoff_pastel_puzzle/
├── README.md                  ← 이 문서
├── index.html                 ← 시안 진입점 (모든 화면 캔버스)
├── app.jsx                    ← Design Canvas + Tweaks 패널 셋업
├── screens.jsx                ← 핵심 컴포넌트 + 팔레트/테마 + 5개 화면
├── more-screens.jsx           ← HomeSequential, GalleryMuseum, GalleryCodex
├── design-canvas.jsx          ← 디자인 캔버스 starter (옵션 비교용)
├── android-frame.jsx          ← Android 디바이스 프레임 (참고용)
├── tweaks-panel.jsx           ← Tweaks 컨트롤 starter
└── assets/                    ← placeholder 이미지 (명화 4점)
```

### 시안 실행
1. 폴더 전체를 로컬 서버 (`python -m http.server` 등) 로 서빙
2. `index.html` 열기 → 디자인 캔버스에서 모든 화면 동시 비교 가능
3. Tweaks 패널 (우상단 토글) 에서 팔레트/테마 변경

### 참고: 코드는 시안 데모용
- React 18 + Babel standalone 사용 (production 코드 아님)
- 모든 스타일 inline `style` 객체 — 토큰 추출 후 타겟 환경의 디자인 시스템에 맵핑하세요
- 컴포넌트 구조와 props 형태는 그대로 참고 가능

---

## Open Questions for Implementation

다음은 추가 의사결정 필요한 항목들입니다:

1. **퍼즐 이미지 자산 확보**: 30~50점 라이브러리. 저작권 free 명화 + 라이센스 확보된 사진 (꽃/디저트). 실 이미지 들어가면 디자인 인상 크게 변할 수 있음
2. **난이도 곡선**: 현재 시안은 36→60→84→108→132 조각으로 5레벨마다 +24. 실제 사용자 테스트로 조정 필요
3. **하트(생명) 시스템**: 시안엔 "5/5" 표시만. 회복 시간, 광고로 충전 등 메커니즘 구체화 필요
4. **챕터 구조**: 30~50개를 5~10개 단위 챕터로 묶을지 여부. 묶는다면 챕터 사이 인터스티셜 보상 화면 추가 필요
5. **출석 체크 / 일일 미션**: 시안엔 미포함. 리텐션 강화하려면 추가 권장
6. **상점 / IAP**: 코인·하트 구매. 광고 모델과 병행 시 광고 빈도 조정 필요
7. **튜토리얼**: 첫 사용자 온보딩 흐름 미정의

---

## Brand & Tone Notes

- **3060 여성 사용자 보이스**: 친근하지만 어른스럽게. "오늘도 반가워요", "그림 한 점, 시작해 볼까요" 같은 살짝 시적인 카피
- **이모지**: 본문엔 거의 없음. 액센트로만 (코인 ⊙, 별 ★, 자물쇠 SVG)
- **모서리**: 직각 아닌 둥근 모서리 우선 (8-16px). 단, 액자 / 진열장 표현 시 직각 가능
- **그림자**: 따뜻한 갈색 톤(`rgba(60,40,30, …)`) — 회색이 아닌 점 주의

---

문의: 이 시안 작업 컨텍스트로 돌아오면 됩니다. 추가 화면(상점/이벤트/온보딩) 또는 명세 보강이 필요하면 알려주세요.
