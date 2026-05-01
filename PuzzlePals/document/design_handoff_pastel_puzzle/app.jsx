// 앱 — 디자인 캔버스 + Tweaks
const { useState } = React;

function App() {
  const [tweaks, setTweak] = useTweaks(/*EDITMODE-BEGIN*/{
    "palette": "garden",
    "theme": "art"
  }/*EDITMODE-END*/);

  const P = window.PALETTES[tweaks.palette] || window.PALETTES.garden;
  const themeKey = tweaks.theme;

  // 안드로이드 프레임 wrapper
  const Phone = ({ children }) => (
    <window.AndroidDevice width={412} height={892}>
      <div style={{ position: "relative", width: "100%", height: "100%", overflow: "hidden" }}>
        {children}
      </div>
    </window.AndroidDevice>
  );

  return (
    <>
      <window.DesignCanvas>
        <window.DCSection id="home" title="메인 / 홈 화면" subtitle="도장깨기 모델 · 30~50개 라이브러리 적합">
          <window.DCArtboard id="home-seq" label="★ A · 순차 잠금 그리드 (도장깨기) — 추천" width={460} height={940}>
            <Phone><window.HomeSequential palette={P} theme={themeKey} /></Phone>
          </window.DCArtboard>
          <window.DCArtboard id="home-b" label="B · 스토리 큰 카드" width={460} height={940}>
            <Phone><HomeStory palette={P} theme={themeKey} /></Phone>
          </window.DCArtboard>
          <window.DCArtboard id="home-c" label="C · 미니멀 리스트" width={460} height={940}>
            <Phone><HomeMinimal palette={P} theme={themeKey} /></Phone>
          </window.DCArtboard>
          <window.DCArtboard id="home-a" label="D · 갤러리 그리드 (참고용)" width={460} height={940}>
            <Phone><HomeGallery palette={P} theme={themeKey} /></Phone>
          </window.DCArtboard>
        </window.DCSection>

        <window.DCSection id="gallery" title="나의 갤러리" subtitle="완성한 작품 모음 · '다시 풀기' 광고 노출 추가 기회">
          <window.DCArtboard id="gallery-museum" label="A · 미술관 액자 진열" width={460} height={940}>
            <Phone><window.GalleryMuseum palette={P} theme={themeKey} /></Phone>
          </window.DCArtboard>
          <window.DCArtboard id="gallery-codex" label="B · 도감 그리드 + 다시 풀기" width={460} height={940}>
            <Phone><window.GalleryCodex palette={P} theme={themeKey} /></Phone>
          </window.DCArtboard>
        </window.DCSection>

        <window.DCSection id="play" title="게임 플레이 화면" subtitle="퍼즐 맞추기 · 보드 + 트레이">
          <window.DCArtboard id="play-a" label="A · 정통 트레이" width={460} height={940}>
            <Phone><PlayClassic palette={P} theme={themeKey} /></Phone>
          </window.DCArtboard>
          <window.DCArtboard id="play-b" label="B · 미리보기 + 카테고리" width={460} height={940}>
            <Phone><PlayPreview palette={P} theme={themeKey} /></Phone>
          </window.DCArtboard>
        </window.DCSection>

        <window.DCSection id="complete" title="완성 화면" subtitle="순수 축하 + 다음 퍼즐 흐름">
          <window.DCArtboard id="complete-a" label="A · 셀러브레이션 액자" width={460} height={940}>
            <Phone><CompleteCelebration palette={P} theme={themeKey} /></Phone>
          </window.DCArtboard>
          <window.DCArtboard id="complete-b" label="B · 컬렉션 도감 안내" width={460} height={940}>
            <Phone><CompleteCollection palette={P} theme={themeKey} /></Phone>
          </window.DCArtboard>
        </window.DCSection>

        <window.DCSection id="ad" title="보상형 광고 모달" subtitle="힌트 진입 시 즉시 광고 (전환율 최적화)">
          <window.DCArtboard id="ad-a" label="즉시 광고 진행" width={460} height={940}>
            <Phone><RewardedAdModal palette={P} /></Phone>
          </window.DCArtboard>
        </window.DCSection>
      </window.DesignCanvas>

      <TweaksPanel title="Tweaks">
        <TweakSection label="컬러 팔레트" />
        <TweakRadio
          label="팔레트"
          value={tweaks.palette}
          onChange={(v) => setTweak("palette", v)}
          options={["garden", "peach", "sage", "lavender"]}
        />
        <TweakSection label="퍼즐 테마" />
        <TweakRadio
          label="테마"
          value={tweaks.theme}
          onChange={(v) => setTweak("theme", v)}
          options={["flower", "dessert", "art"]}
        />
        <TweakSection label="광고 수익 지점" />
        <div style={{ fontSize: 12, lineHeight: 1.6, color: "rgba(60,40,30,0.75)", padding: "0 4px" }}>
          • <b>홈/갤러리/플레이</b>: 하단 배너 상시<br />
          • <b>힌트 버튼</b>: 즉시 보상형 광고<br />
          • <b>다음 퍼즐 진입</b>: 전면 광고 (3-5판)<br />
          • <b>갤러리 다시 풀기</b>: 재플레이 광고
        </div>
      </TweaksPanel>
    </>
  );
}

ReactDOM.createRoot(document.getElementById("root")).render(<App />);
