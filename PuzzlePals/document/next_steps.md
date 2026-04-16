# Puzzle Pals — 남은 작업 리스트

마지막 업데이트: 2026-04-16
현재 상태: 핵심 루프 동작 OK, APK 빌드 가능, 출시 전 보완 작업 필요

---

## 🔴 출시 전 필수 (Must Have)

### 1. 실 광고 SDK 연동 (LevelPlay 마이그레이션)
- 현재: `UnityAdsProvider` stub (Console 로그만)
- 필요: Advertisement Legacy → `com.unity.services.levelplay` 마이그레이션
- 작업량: 1-2시간 (코드) + IronSource Dashboard 세팅 (10분)
- 영향: 수익화 핵심

### 2. AudioClip 4개 추가
- 현재: AudioManager 슬롯 비어 무음
- 필요: pop / ding / fanfare / bgm
- 추천 소스: https://pixabay.com/sound-effects (무료 상업 OK, 출처 표시 불필요), https://freesound.org (CC 라이선스)
- 작업: 다운로드 → AudioManager Inspector 슬롯에 드래그
- 작업량: 30분

### 3. 캐릭터 아트 업그레이드
- 현재: 25개 procedural + 75개 도형 (도형 수준)
- 옵션 A: AI 생성 (Bing Image Creator, 무료)
  - 100개 × 1-2분 = 1-2시간
  - 일관 스타일은 prompt suffix 로 확보
  - 참고: [character_prompts.md](character_prompts.md) (저장된 100개 프롬프트)
- 옵션 B: Kenney.nl / OpenGameArt 등 무료 팩 조합
  - 1-2시간, 라이선스 검토 필요
- 옵션 C: 외주 디자이너 ($500-2000)
- 임포트: `_Source/` 폴더에 PNG 드랍 → `PuzzlePals > Import Character Art (Auto Name Match)`

### 4. App Icon 디자인
- 현재: Unity 기본 아이콘
- 필요: 1024x1024 마스터 + 여러 사이즈 (Player Settings > Icon)
- 어드런스 / 라운드 / 레거시 모두 채우는 게 좋음
- 추천: 사과 (Level 1 캐릭터) 또는 무지개용 (Level 100 캐릭터) 모티브

### 5. IAP Sandbox 테스트
- 코드 인프라 완료 (`IAPManager`, 3종 상품)
- Google Play Console 에서 Product ID 등록 필수:
  - `puzzlepals_hint_5` ($0.99)
  - `puzzlepals_hint_20` ($2.99)
  - `puzzlepals_hint_unlimited` ($4.99)
- App Store Connect 도 동일 등록
- 테스터 계정으로 샌드박스 결제 검증

### 6. Privacy Policy + COPPA 준수
- **유아용 앱은 COPPA 필수** (미국)
- 한국 개인정보처리방침 작성
- 광고 SDK 가 아동 데이터 수집 안 하도록 설정 필수
  - Unity Ads / LevelPlay 의 "Designed for Families" 플래그
  - Google Play 의 "Designed for Families" 프로그램 가입 검토
- 추천: privacypolicytemplate.net 등으로 초안 + 변호사 검토

### 7. Splash Screen
- 현재: Unity 기본 (Made with Unity 워터마크)
- Unity Personal 라이선스 = 워터마크 제거 불가
- Unity Pro 라이선스 필요 또는 Made with Unity 워터마크 수용
- Player Settings > Splash Image 에서 로고/색상 커스터마이즈

### 8. 스토어 자료
- **Play Store**:
  - 앱 아이콘 512x512
  - 피처 그래픽 1024x500
  - 스크린샷 2-8장 (폰 + 태블릿)
  - 짧은 설명 (80자) + 긴 설명 (4000자)
  - 카테고리: 교육 / 가족
- **App Store** (출시 시):
  - 스크린샷 (각 디바이스 사이즈별)
  - 앱 미리보기 영상 (선택)

### 9. 실 기기 성능 테스트
- 60fps 유지 (저사양 기기 포함)
- Frame Debugger / Profiler 로 측정
- 발열, 배터리 소모 체크
- 다양한 해상도 (16:9, 19.5:9, 4:3) 확인

### 10. ShopPopupUI ↔ HintManager 자동 연결
- 현재: Inspector 수동 드래그 필요
- 작업: SceneSetupTool 에 자동 wire 추가
- 작업량: 30분

---

## 🟡 출시 후 단기 개선 (Should Have)

### 11. 사운드 다양화
- 4개 SFX 외에 추가:
  - 레벨 진입 (whoosh)
  - 잘못된 스냅 (실패음, optional)
  - 버튼 클릭 (subtle)
  - 별 획득 (sparkle)
  - Tutorial / hint 사용 (chime)

### 12. 캐릭터 완성 애니메이션 강화
- 현재: 조각 점프 2회 + 흔들기
- GDD 원본: 눈 깜빡 → 점프 2회 → 손 흔들기 (눈/손 부분 sprite 필요)
- 진짜 캐릭터 아트 도착 후 부분별 sprite 조립 + Animator 작업

### 13. 일본어/중국어 번역 native 검토
- AI 초벌 번역 → native speaker review
- 특히 캐릭터 이름의 자연스러움 (예: "Mermaid Cat" 직역 vs 의역)
- 비용: 100단어 기준 $20-50 / 언어 (Fiverr, Gengo 등)

### 14. 추가 언어 (16개)
- GDD 목표: 20개
- 현재: 4개 (ko/en/ja/zh-Hans)
- 추가: zh-TW, es, fr, de, pt, it, ru, hi, id, tr, th, vi, nl, pl, sv, ar
- Localization 패키지 + 번역 데이터만 추가 (코드 변경 X)

### 15. Cloud Save
- 현재: PlayerPrefs (기기 변경 시 진행도 손실)
- 추천: Google Play Games Services (Android), Game Center (iOS)
- Unity Cloud Save 옵션도 있음

### 16. Analytics 연동
- Unity Analytics (간단) 또는 Firebase Analytics (강력)
- 레벨별 클리어 시간 / 이탈률 / 광고 노출 추적
- 난이도 튜닝 데이터로 활용

### 17. Crash Reporting
- Unity Cloud Diagnostics 또는 Firebase Crashlytics
- 출시 후 예상치 못한 에러 자동 수집

### 18. 첫 사용자 튜토리얼
- 첫 실행 시 손가락 드래그 가이드
- 첫 레벨에서만 표시, SaveManager 에 플래그 저장

### 19. 도전 과제 / 진행 보상
- 별 획득 외 (현재 1레벨 = 별1)
- 5/10/20/50/100 레벨 클리어 마일스톤
- 카테고리별 (사물 50 / 동물 50) 보상

---

## 🟢 장기 기능 확장 (Could Have)

### 20. 아랍어 RTL 지원
- Layout 자동 미러링
- 캐릭터 이름 RTL 렌더링
- LevelSelect 그리드 우→좌 방향

### 21. 새 게임 모드
- **타임 어택**: 60초 안에 N 레벨 클리어
- **챌린지**: 특정 캐릭터만 / 더 작은 조각
- **자유 모드**: 클리어한 레벨 자유 재플레이

### 22. 캐릭터 수집 도감
- 클리어한 100개 캐릭터 갤러리
- 부모님 자랑용 + 메타 진행감

### 23. 부모용 대시보드
- 자녀 플레이 시간 / 클리어 레벨 / 평균 시간
- 화면 시간 제한 (페이런털 컨트롤)
- iOS 의 "유아용 앱" 프로그램 활용

### 24. 멀티플레이어 (협동)
- 두 사람이 한 화면에서 조각 나눠 맞추기
- 가족 / 형제 자매용

---

## 📋 작업 우선순위 추천

**1주차 (출시 1차 준비)**
1. 실 광고 SDK 마이그레이션 (#1)
2. AudioClip 4개 추가 (#2)
3. App Icon 디자인 (#4)
4. ShopPopupUI 자동 연결 (#10)

**2주차 (콘텐츠 + 베타)**
5. 캐릭터 아트 (AI 생성 또는 외주) (#3)
6. IAP 상품 등록 + 샌드박스 테스트 (#5)
7. 실 기기 성능 테스트 (#9)
8. 첫 사용자 튜토리얼 (#18)

**3주차 (출시 준비)**
9. Privacy Policy + COPPA (#6)
10. Splash Screen 또는 Pro 라이선스 결정 (#7)
11. 스토어 자료 (#8)
12. Crash Reporting (#17)

**출시 후**
13. Analytics 데이터 보고 난이도 튜닝
14. 사용자 피드백 → 캐릭터 / UX 개선
15. 추가 언어 (#14)
16. Cloud Save (#15)

---

## 🔧 Editor 도구 추가 후보

- `PuzzlePals > Build Android APK` — Player Settings 자동 적용 + 빌드 + 버전 자동 증가
- `PuzzlePals > Wire ShopPopup ↔ HintManager` — 수동 드래그 자동화
- `PuzzlePals > Run All Quality Checks` — 컴파일 / 테스트 / 빌드 검증 한 번에
- `PuzzlePals > Localization Build Addressables` — Localization 출시 빌드 전처리

---

## 🎯 다음 세션 시작 시 추천 순서

1. **완료 못한 APK 빌드 마무리** — Active Input Handling = Old, Bundle Identifier 설정, Build And Run
2. **실 기기 동작 확인** — 화면 비율, 터치 반응, 헤더 위치, 레벨 진행
3. **위 우선순위 1주차 항목** 중 하나 선택해서 진행

---

**참고 문서**
- [puzzle_pals_gdd.md](puzzle_pals_gdd.md) — 게임 디자인 원본
- [puzzle_pals_test_spec.md](puzzle_pals_test_spec.md) — 테스트 스펙
- [progress_report.md](progress_report.md) — 현재 구현 상태 상세
- [character_prompts.md](character_prompts.md) — 캐릭터 100개 AI 프롬프트
