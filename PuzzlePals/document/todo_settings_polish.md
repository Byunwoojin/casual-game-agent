# 설정 팝업 마무리 작업 (대부분 완료)

**최종 업데이트**: 2026-04-27 — Localization 마이그레이션 완료
**남은 작업**: Unity 검증 + 사운드 파일 추가 (외부 자산)

---

## 완료된 작업 ✓

### ① 슬라이더 닫힘 버그 수정
- `Assets/Scripts/UI/SettingsPopupUI.cs` — panel 에 `panelClickAbsorb` Button 추가 → 자식 click 흡수, root.Button(Hide) propagation 차단

### ② 다국어 시스템 (글로벌 출시 결정 → B2 전면 진행)
- `Editor/LocalizationStringTableTool.cs` UI_ROWS 에 14개 신규 키 + ko/en/ja/zh-Hans 4개 언어 번역 추가
- `UI/DesignTokens.cs` `BuildLocalizedText` 헬퍼 추가 (LocalizedTextBinder 자동 부착)
- 5개 UI 파일 마이그레이션:
  - `UI/GameplayHeaderUI.cs` — 4 버튼 라벨 + 3 hint 토스트 + tap_to_close
  - `UI/LevelSelectController.cs` — locked toast
  - `UI/SettingsPopupUI.cs` — 4개 라벨 (설정 / 언어 / 효과음 / 배경음)
  - `Gameplay/CompleteSceneController.cs` — `LEVEL {0} 완성!` (LocalizedString.Arguments) + 다음 레벨
  - `Gameplay/ProgressCelebrationController.cs` — 50/75/edges 토스트

### ③ CanvasScaler 통일 (이전 작업)
- 6곳 `matchWidthOrHeight = 0f` 통일 — 길쭉한 폰 가로 잘림 방지

---

## 남은 작업 (Unity / 외부 자산)

### Step 1: Unity 메뉴 실행 (1분)
**`PuzzlePals → Populate String Tables` 메뉴 실행**
- UI StringTable 에 14개 신규 키 적용. 재실행 안전(idempotent)

### Step 2: 검증 — 빌드 또는 에디터 플레이
1. Settings 팝업 → 슬라이더 좌우 드래그 → **닫히지 않으면** 슬라이더 버그 OK
2. Settings 팝업 → 언어 버튼 클릭 (한국어/English/日本語/中文) → **UI 텍스트가 즉시 변경**되면 다국어 OK
3. L2 진행 → 50%/75%/edges 토스트 + Complete 씬 "Level N Complete!" → **선택한 언어로 표시**되면 OK

### Step 3: 사운드 파일 추가 (외부 자산 필요)
- 사운드 파일 (`.wav` / `.ogg`) 을 `Assets/Audio/` 같은 폴더에 임포트
- AudioManager 인스펙터의 4개 슬롯에 드래그
- **재생 호출 점검 필요** — `PlayPop` / `PlayDing` / `PlayFanfare` / BGM 시작이 실제로 호출되는지 확인 (호출 안 되면 클립 있어도 무음)
- 무료 사운드 출처: Freesound, Mixkit, ZapSplat

---

## 새 텍스트 추가 시 워크플로우 (향후 운영)

룰은 `CLAUDE.md` + `memory/feedback_localization_workflow.md` 에 박혀있음. 요약:

1. `Editor/LocalizationStringTableTool.cs` → `UI_ROWS` 배열에 한 줄 추가 (key + ko + en + ja + zh-Hans)
2. 코드에선 `DesignTokens.BuildLocalizedText(parent, name, entryKey, fallback, ...)` 사용
3. format 텍스트는 `LocalizedString` + `Arguments` 직접 사용
4. Unity 메뉴 `PuzzlePals → Populate String Tables` 실행
