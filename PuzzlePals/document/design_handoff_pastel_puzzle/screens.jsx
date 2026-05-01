// 캐주얼 직소 게임 화면 컴포넌트
// 따뜻한 파스텔 정원 톤, 3060 여성 타겟, 가독성 우선

// ───── 팔레트 ─────
const PALETTES = {
  garden: {
    name: "파스텔 정원",
    bg: "#FBF6EE",         // cream
    surface: "#FFFFFF",
    surfaceAlt: "#F4ECDF", // warm cream
    primary: "#D88A7A",    // dusty rose
    primaryDeep: "#B7634F",
    accent: "#9CB28A",     // sage
    accentSoft: "#E5EBD5",
    gold: "#E0B86A",
    text: "#3D2E26",
    textSoft: "#8A7567",
    textMuted: "#B8A595",
    line: "#EADFCD",
    danger: "#D9636A",
    starterA: "#F7E3D2", // peach
    starterB: "#E5EBD5", // sage tint
  },
  peach: {
    name: "피치 크림",
    bg: "#FEF3EC",
    surface: "#FFFFFF",
    surfaceAlt: "#FCE5D5",
    primary: "#EB8B6B",
    primaryDeep: "#C66241",
    accent: "#C19BB7",
    accentSoft: "#F0E1EB",
    gold: "#E8B36C",
    text: "#3F2C24",
    textSoft: "#8C6E63",
    textMuted: "#BFA395",
    line: "#F1DDCC",
    danger: "#D9636A",
    starterA: "#FBD9C2",
    starterB: "#F0E1EB",
  },
  sage: {
    name: "세이지 우드",
    bg: "#F5F1E8",
    surface: "#FFFFFF",
    surfaceAlt: "#EAE2CF",
    primary: "#9DB089",
    primaryDeep: "#6B8260",
    accent: "#C8A27C",
    accentSoft: "#EFE2CF",
    gold: "#D6B27A",
    text: "#36322A",
    textSoft: "#7C7466",
    textMuted: "#B0A795",
    line: "#E2D9C4",
    danger: "#C77160",
    starterA: "#E5EBD5",
    starterB: "#EFE2CF",
  },
  lavender: {
    name: "라벤더 베리",
    bg: "#F6F1F4",
    surface: "#FFFFFF",
    surfaceAlt: "#ECE0E8",
    primary: "#B295C2",
    primaryDeep: "#7E5F94",
    accent: "#D49AA8",
    accentSoft: "#F1DEE3",
    gold: "#D8B57E",
    text: "#3A2E3B",
    textSoft: "#7E6F80",
    textMuted: "#B5A4B6",
    line: "#E8DDE5",
    danger: "#C76B7A",
    starterA: "#E2D2EA",
    starterB: "#F1DEE3",
  },
};

// ───── 퍼즐 테마 (실제 이미지 대신 SVG 그라데이션 + 라벨로 placeholder) ─────
const THEMES = {
  flower: {
    name: "꽃 · 정원",
    items: [
      { id: "rose", label: "장미 정원", hue1: "#F4C0B5", hue2: "#A8C68F", icon: "🌹" },
      { id: "tulip", label: "튤립밭", hue1: "#F7D2C2", hue2: "#D9A6B8", icon: "🌷" },
      { id: "lavender", label: "프로방스 라벤더", hue1: "#C9B4D9", hue2: "#A5C29B", icon: "💜" },
      { id: "sunflower", label: "해바라기", hue1: "#F4D27A", hue2: "#A8C68F", icon: "🌻" },
      { id: "cherry", label: "벚꽃길", hue1: "#F8DCE0", hue2: "#E0BFC9", icon: "🌸" },
      { id: "garden", label: "영국식 정원", hue1: "#C8D9B0", hue2: "#E8C9A8", icon: "🌿" },
      { id: "peony", label: "모란꽃", hue1: "#F2BCBE", hue2: "#D8A0B0", icon: "🌺" },
      { id: "iris", label: "붓꽃", hue1: "#B0AFD8", hue2: "#C7B7CD", icon: "💐" },
    ],
  },
  dessert: {
    name: "디저트",
    items: [
      { id: "macaron", label: "파리 마카롱", hue1: "#F3C5C5", hue2: "#C9D9B5", icon: "🧁" },
      { id: "cake", label: "딸기 케이크", hue1: "#F4C0B5", hue2: "#FFFFFF", icon: "🍰" },
      { id: "tea", label: "애프터눈 티", hue1: "#E8C8A0", hue2: "#D9A89C", icon: "🍵" },
      { id: "bakery", label: "프렌치 베이커리", hue1: "#E8C198", hue2: "#C5A375", icon: "🥐" },
      { id: "coffee", label: "라떼아트", hue1: "#D5B89C", hue2: "#8C6F5C", icon: "☕" },
      { id: "fruit", label: "과일 플래터", hue1: "#F4B0A0", hue2: "#C9D9B5", icon: "🍓" },
      { id: "donut", label: "도넛 진열장", hue1: "#F2C9CE", hue2: "#E5C29A", icon: "🍩" },
      { id: "choco", label: "벨기에 초콜릿", hue1: "#B68A6E", hue2: "#5E3A2A", icon: "🍫" },
    ],
  },
  art: {
    name: "명화",
    items: [
      { id: "degas-pink", label: "분홍 발레리나", hue1: "#F0D5C8", hue2: "#C9A8B0", icon: "🩰", img: "assets/art-degas-pink.png" },
      { id: "ballerina-blue", label: "푸른 발레리나", hue1: "#5A8AB0", hue2: "#A88C5C", icon: "💃", img: "assets/art-ballerina-blue.png" },
      { id: "bridge", label: "강가의 다리", hue1: "#A8C5D0", hue2: "#B8D0A8", icon: "🎨", img: "assets/art-bridge.png" },
      { id: "socrates", label: "철학자의 마지막", hue1: "#8A7560", hue2: "#3A2E26", icon: "🏛", img: "assets/art-socrates.png" },
      { id: "monet", label: "수련의 정원", hue1: "#A8C5D0", hue2: "#B8D0A8", icon: "🎨" },
      { id: "vangogh", label: "별이 빛나는 밤", hue1: "#4A6B9C", hue2: "#1F2D52", icon: "🌌" },
      { id: "klimt", label: "황금의 키스", hue1: "#E0B86A", hue2: "#9B7530", icon: "✨" },
      { id: "vermeer", label: "진주 귀걸이", hue1: "#3A4858", hue2: "#9C8A6B", icon: "🎭" },
    ],
  },
};

// ───── 공통 SVG 아이콘 ─────
const Icon = ({ name, size = 22, color = "currentColor", strokeWidth = 1.8 }) => {
  const paths = {
    back: <path d="M15 18l-6-6 6-6" />,
    settings: (
      <>
        <circle cx="12" cy="12" r="3" />
        <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 1 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 1 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 1 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 1 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z" />
      </>
    ),
    trophy: (
      <>
        <path d="M8 21h8M12 17v4M7 4h10v5a5 5 0 1 1-10 0V4z" />
        <path d="M17 5h3v3a3 3 0 0 1-3 3M7 5H4v3a3 3 0 0 0 3 3" />
      </>
    ),
    home: <path d="M3 11l9-8 9 8v9a2 2 0 0 1-2 2h-4v-7h-6v7H5a2 2 0 0 1-2-2v-9z" />,
    calendar: (
      <>
        <rect x="3" y="5" width="18" height="16" rx="2" />
        <path d="M3 10h18M8 3v4M16 3v4" />
      </>
    ),
    lock: (
      <>
        <rect x="5" y="11" width="14" height="10" rx="2" />
        <path d="M8 11V7a4 4 0 1 1 8 0v4" />
      </>
    ),
    play: <path d="M6 4l14 8-14 8V4z" fill={color} stroke="none" />,
    star: <path d="M12 2l3.1 6.5 7 1-5 4.9 1.2 7L12 18l-6.3 3.4 1.2-7-5-4.9 7-1z" fill={color} stroke={color} />,
    coin: (
      <>
        <circle cx="12" cy="12" r="9" />
        <path d="M12 7v10M9 9h4a2 2 0 0 1 0 4H9M9 13h5a2 2 0 0 1 0 4H9" />
      </>
    ),
    bulb: (
      <>
        <path d="M9 21h6M10 17h4" />
        <path d="M12 3a6 6 0 0 0-4 10.5c.6.7 1.2 1.5 1.5 2.5h5c.3-1 .9-1.8 1.5-2.5A6 6 0 0 0 12 3z" />
      </>
    ),
    eye: (
      <>
        <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7z" />
        <circle cx="12" cy="12" r="3" />
      </>
    ),
    image: (
      <>
        <rect x="3" y="3" width="18" height="18" rx="2" />
        <circle cx="9" cy="9" r="2" />
        <path d="M21 15l-5-5L5 21" />
      </>
    ),
    pause: (
      <>
        <rect x="6" y="4" width="4" height="16" />
        <rect x="14" y="4" width="4" height="16" />
      </>
    ),
    swap: <path d="M7 4v16M7 4l-3 3M7 4l3 3M17 20V4M17 20l-3-3M17 20l3-3" />,
    check: <path d="M5 13l4 4L19 7" />,
    x: <path d="M6 6l12 12M18 6L6 18" />,
    plus: <path d="M12 5v14M5 12h14" />,
    sparkle: (
      <>
        <path d="M12 3v4M12 17v4M3 12h4M17 12h4M5.6 5.6l2.8 2.8M15.6 15.6l2.8 2.8M5.6 18.4l2.8-2.8M15.6 8.4l2.8-2.8" />
      </>
    ),
    flower: (
      <>
        <circle cx="12" cy="12" r="2" />
        <path d="M12 4a3 3 0 1 1 0 6M12 14a3 3 0 1 1 0 6M4 12a3 3 0 1 1 6 0M14 12a3 3 0 1 1 6 0" />
      </>
    ),
    crown: <path d="M3 18h18M5 18l-2-9 5 4 4-7 4 7 5-4-2 9" />,
    chevron: <path d="M9 6l6 6-6 6" />,
    share: (
      <>
        <circle cx="18" cy="5" r="3" />
        <circle cx="6" cy="12" r="3" />
        <circle cx="18" cy="19" r="3" />
        <path d="M8.6 13.5l6.8 4M15.4 6.5l-6.8 4" />
      </>
    ),
    gift: (
      <>
        <rect x="3" y="8" width="18" height="13" rx="1" />
        <path d="M3 12h18M12 8v13M8 8a3 3 0 1 1 4-3 3 3 0 1 1 4 3" />
      </>
    ),
  };
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none"
      stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round">
      {paths[name]}
    </svg>
  );
};

// ───── 퍼즐 이미지 placeholder (그라데이션 + 라벨 + 직소 그리드 오버레이) ─────
const PuzzleImage = ({ item, size = 160, showGrid = true, completed = false, locked = false }) => {
  const { hue1, hue2, label, icon, img } = item;
  const gridLines = [];
  if (showGrid) {
    const cells = 4;
    for (let i = 1; i < cells; i++) {
      const p = (i / cells) * 100;
      gridLines.push(
        <line key={`v${i}`} x1={`${p}%`} y1="0" x2={`${p}%`} y2="100%" stroke="rgba(255,255,255,0.45)" strokeWidth="0.6" />
      );
      gridLines.push(
        <line key={`h${i}`} x1="0" y1={`${p}%`} x2="100%" y2={`${p}%`} stroke="rgba(255,255,255,0.45)" strokeWidth="0.6" />
      );
    }
  }
  // 실제 이미지가 있으면 그것 사용, 없으면 그라데이션 placeholder
  if (img) {
    return (
      <div style={{
        position: "relative", width: "100%", height: "100%",
        overflow: "hidden",
        filter: locked ? "saturate(0.05) brightness(0.92)" : "none",
      }}>
        <img src={img} alt={label} style={{
          width: "100%", height: "100%", objectFit: "cover", display: "block",
        }} />
        {showGrid && !completed && (
          <svg viewBox="0 0 100 100" preserveAspectRatio="none"
            style={{ position: "absolute", inset: 0, width: "100%", height: "100%" }}>
            {gridLines}
          </svg>
        )}
      </div>
    );
  }
  return (
    <div style={{
      position: "relative", width: "100%", height: "100%",
      background: `linear-gradient(135deg, ${hue1} 0%, ${hue2} 100%)`,
      filter: locked ? "saturate(0.15) brightness(0.92)" : "none",
      overflow: "hidden",
    }}>
      {/* 풍경 인상을 주는 부드러운 도형 */}
      <div style={{
        position: "absolute", left: "-10%", bottom: "-30%", width: "70%", aspectRatio: "1",
        background: `radial-gradient(circle, ${hue2} 0%, transparent 70%)`, opacity: 0.7,
      }} />
      <div style={{
        position: "absolute", right: "-15%", top: "-20%", width: "60%", aspectRatio: "1",
        background: `radial-gradient(circle, ${hue1} 0%, transparent 70%)`, opacity: 0.6,
      }} />
      {/* placeholder 라벨 */}
      <div style={{
        position: "absolute", inset: 0, display: "flex", flexDirection: "column",
        alignItems: "center", justifyContent: "center", color: "rgba(40,30,25,0.55)",
        fontSize: size * 0.28, fontWeight: 600, gap: 4,
      }}>
        <span style={{ fontSize: size * 0.32 }}>{icon}</span>
        <span style={{
          fontSize: Math.max(10, size * 0.075), fontWeight: 500,
          letterSpacing: "-0.02em", padding: "2px 8px",
          background: "rgba(255,255,255,0.55)", borderRadius: 4,
        }}>{label}</span>
      </div>
      {/* 직소 그리드 */}
      {showGrid && !completed && (
        <svg viewBox="0 0 100 100" preserveAspectRatio="none"
          style={{ position: "absolute", inset: 0, width: "100%", height: "100%" }}>
          {gridLines}
        </svg>
      )}
    </div>
  );
};

// ───── 직소 조각 (브레이 형태) ─────
const JigsawPiece = ({ item, x, y, w, h, totalW, totalH, knobs }) => {
  // knobs: { top, right, bottom, left } 1=outie, -1=innie, 0=flat
  const k = 0.18; // knob 크기 비율
  const sw = w, sh = h;
  // path
  const top = knobs.top === 0 ? `L ${sw} 0`
    : `L ${sw * 0.35} 0 C ${sw * 0.35} ${sh * k * (-knobs.top)}, ${sw * 0.65} ${sh * k * (-knobs.top)}, ${sw * 0.65} 0 L ${sw} 0`;
  const right = knobs.right === 0 ? `L ${sw} ${sh}`
    : `L ${sw} ${sh * 0.35} C ${sw + sw * k * knobs.right} ${sh * 0.35}, ${sw + sw * k * knobs.right} ${sh * 0.65}, ${sw} ${sh * 0.65} L ${sw} ${sh}`;
  const bottom = knobs.bottom === 0 ? `L 0 ${sh}`
    : `L ${sw * 0.65} ${sh} C ${sw * 0.65} ${sh + sh * k * knobs.bottom}, ${sw * 0.35} ${sh + sh * k * knobs.bottom}, ${sw * 0.35} ${sh} L 0 ${sh}`;
  const left = knobs.left === 0 ? `L 0 0`
    : `L 0 ${sh * 0.65} C ${-sw * k * knobs.left} ${sh * 0.65}, ${-sw * k * knobs.left} ${sh * 0.35}, 0 ${sh * 0.35} L 0 0`;
  const d = `M 0 0 ${top} ${right} ${bottom} ${left} Z`;
  const id = `clip-${item.id}-${x}-${y}`;
  // pad svg viewBox to include knobs
  const pad = Math.max(sw, sh) * k;
  return (
    <svg
      width={sw + pad * 2}
      height={sh + pad * 2}
      viewBox={`${-pad} ${-pad} ${sw + pad * 2} ${sh + pad * 2}`}
      style={{ display: "block", filter: "drop-shadow(0 2px 4px rgba(0,0,0,0.18))" }}
    >
      <defs>
        <clipPath id={id} clipPathUnits="userSpaceOnUse">
          <path d={d} />
        </clipPath>
        <linearGradient id={`g-${id}`} x1="0" y1="0" x2="1" y2="1">
          <stop offset="0%" stopColor={item.hue1} />
          <stop offset="100%" stopColor={item.hue2} />
        </linearGradient>
      </defs>
      <g clipPath={`url(#${id})`}>
        {item.img ? (
          <image href={item.img} x={-x * sw} y={-y * sh}
            width={totalW} height={totalH} preserveAspectRatio="xMidYMid slice" />
        ) : (
          <>
            <rect x={-x * sw} y={-y * sh} width={totalW} height={totalH} fill={`url(#g-${id})`} />
            <circle cx={totalW * 0.7 - x * sw} cy={totalH * 0.3 - y * sh} r={totalW * 0.25} fill={item.hue1} opacity="0.5" />
            <circle cx={totalW * 0.3 - x * sw} cy={totalH * 0.75 - y * sh} r={totalW * 0.3} fill={item.hue2} opacity="0.45" />
          </>
        )}
      </g>
      <path d={d} fill="none" stroke="rgba(255,255,255,0.7)" strokeWidth="0.8" />
    </svg>
  );
};

// ═════════════════════════════════════════════════════════════════════
// 메인/홈 화면
// ═════════════════════════════════════════════════════════════════════

// 홈 안 A: 갤러리 그리드 (벤치마킹 구조 확장)
function HomeGallery({ palette: P, theme, onOpenStage }) {
  const items = THEMES[theme].items;
  return (
    <div style={{
      position: "absolute", inset: 0, background: P.bg,
      display: "flex", flexDirection: "column", color: P.text,
      fontFamily: '"Pretendard", "Apple SD Gothic Neo", system-ui, sans-serif',
    }}>
      {/* 헤더 */}
      <div style={{ padding: "14px 20px 8px", display: "flex", alignItems: "center", justifyContent: "space-between" }}>
        <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
          <div style={{
            width: 38, height: 38, borderRadius: "50%",
            background: `linear-gradient(135deg, ${P.starterA}, ${P.starterB})`,
            display: "flex", alignItems: "center", justifyContent: "center",
            fontSize: 18,
          }}>🌸</div>
          <div>
            <div style={{ fontSize: 17, fontWeight: 700, letterSpacing: "-0.02em" }}>꽃피는 퍼즐</div>
            <div style={{ fontSize: 11, color: P.textSoft, marginTop: -1 }}>오늘의 정원</div>
          </div>
        </div>
        <div style={{ display: "flex", alignItems: "center", gap: 6 }}>
          <CoinPill P={P} value="2,840" />
          <IconBtn P={P}><Icon name="settings" size={18} color={P.textSoft} /></IconBtn>
        </div>
      </div>

      {/* 일일 퍼즐 카드 (대형 강조) */}
      <div style={{ padding: "8px 20px 12px" }}>
        <div style={{
          background: `linear-gradient(135deg, ${P.starterA} 0%, ${P.starterB} 100%)`,
          borderRadius: 20, padding: 16, position: "relative", overflow: "hidden",
          display: "flex", alignItems: "center", gap: 14,
        }}>
          <div style={{
            width: 64, height: 64, borderRadius: 16, overflow: "hidden",
            border: `3px solid ${P.surface}`, flexShrink: 0,
            boxShadow: "0 4px 12px rgba(0,0,0,0.10)",
          }}>
            <PuzzleImage item={items[0]} size={64} showGrid={false} />
          </div>
          <div style={{ flex: 1, minWidth: 0 }}>
            <div style={{ fontSize: 11, color: P.primaryDeep, fontWeight: 600, letterSpacing: "0.05em" }}>
              ✨ 오늘의 일일 퍼즐
            </div>
            <div style={{ fontSize: 18, fontWeight: 700, marginTop: 2, color: P.text, letterSpacing: "-0.02em" }}>
              5월 1일 · 봄의 시작
            </div>
            <div style={{ fontSize: 12, color: P.textSoft, marginTop: 2 }}>
              완료시 <b style={{ color: P.primaryDeep }}>+50 코인</b> · 별 3개
            </div>
          </div>
          <button style={{
            ...btnStyle(P), padding: "10px 16px", fontSize: 13, fontWeight: 700,
            borderRadius: 999, background: P.surface, color: P.primaryDeep,
            display: "flex", alignItems: "center", gap: 4,
          }}>
            <Icon name="play" size={12} color={P.primaryDeep} /> 시작
          </button>
        </div>
      </div>

      {/* 진행도 바 */}
      <div style={{ padding: "0 20px 10px", display: "flex", alignItems: "center", gap: 8 }}>
        <div style={{ fontSize: 13, fontWeight: 600 }}>이달의 컬렉션</div>
        <div style={{ flex: 1, height: 6, background: P.line, borderRadius: 3, overflow: "hidden" }}>
          <div style={{ width: "37%", height: "100%", background: P.primary, borderRadius: 3 }} />
        </div>
        <div style={{ fontSize: 12, color: P.textSoft, fontWeight: 500 }}>11 / 30</div>
      </div>

      {/* 스테이지 그리드 */}
      <div style={{
        flex: 1, padding: "4px 16px 12px",
        display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12,
        overflow: "auto",
      }}>
        {items.slice(0, 8).map((it, i) => (
          <StageCard key={it.id} P={P} item={it} idx={i + 1}
            locked={i >= 3} progress={i === 0 ? 100 : i === 1 ? 64 : i === 2 ? 12 : 0}
            stars={i === 0 ? 3 : 0} pieces={i < 2 ? 36 : i < 4 ? 64 : 100}
            onClick={() => i < 3 && onOpenStage && onOpenStage(it)} />
        ))}
      </div>

      {/* 하단 배너 광고 */}
      <BannerAd P={P} />

      {/* 바텀 네비 */}
      <BottomNav P={P} active="home" />
    </div>
  );
}

// 홈 안 B: 스토리 카드형 (가로 큰 카드)
function HomeStory({ palette: P, theme, onOpenStage }) {
  const items = THEMES[theme].items;
  return (
    <div style={{
      position: "absolute", inset: 0, background: P.bg,
      display: "flex", flexDirection: "column", color: P.text,
      fontFamily: '"Pretendard", "Apple SD Gothic Neo", system-ui, sans-serif',
    }}>
      {/* 헤더 */}
      <div style={{
        padding: "16px 20px 12px",
        background: `linear-gradient(180deg, ${P.starterA}55 0%, ${P.bg} 100%)`,
      }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
          <div>
            <div style={{ fontSize: 12, color: P.textSoft, fontWeight: 500 }}>오늘도 반가워요</div>
            <div style={{ fontSize: 22, fontWeight: 700, letterSpacing: "-0.025em", marginTop: 2 }}>
              어떤 그림을<br />완성해 볼까요?
            </div>
          </div>
          <div style={{ display: "flex", flexDirection: "column", gap: 6, alignItems: "flex-end" }}>
            <CoinPill P={P} value="2,840" />
            <HeartPill P={P} value="5" />
          </div>
        </div>
      </div>

      {/* 카테고리 칩 */}
      <div style={{ display: "flex", gap: 8, padding: "0 20px 12px", overflowX: "auto" }}>
        {[
          { k: "all", label: "전체" }, { k: "new", label: "신규 ✨" },
          { k: "easy", label: "쉬워요" }, { k: "weekly", label: "이번 주 인기" },
          { k: "art", label: "명화" }
        ].map((c, i) => (
          <div key={c.k} style={{
            padding: "8px 14px", borderRadius: 999, fontSize: 13, fontWeight: 600,
            background: i === 0 ? P.primary : P.surface,
            color: i === 0 ? "#fff" : P.textSoft,
            border: i === 0 ? "none" : `1px solid ${P.line}`,
            whiteSpace: "nowrap",
          }}>{c.label}</div>
        ))}
      </div>

      {/* 추천 큰 카드 */}
      <div style={{ padding: "0 20px 10px" }}>
        <div style={{
          borderRadius: 22, overflow: "hidden", position: "relative",
          height: 150, background: P.surface,
          boxShadow: "0 4px 16px rgba(60,40,30,0.06)",
        }}>
          <div style={{ position: "absolute", inset: 0 }}>
            <PuzzleImage item={items[0]} size={300} showGrid={false} />
          </div>
          <div style={{
            position: "absolute", inset: 0,
            background: "linear-gradient(180deg, rgba(0,0,0,0) 30%, rgba(40,25,20,0.55) 100%)",
          }} />
          <div style={{
            position: "absolute", left: 16, top: 12, padding: "4px 10px",
            background: P.gold, borderRadius: 999, fontSize: 11, fontWeight: 700,
            color: "#fff", letterSpacing: "0.02em",
          }}>👑 오늘의 추천</div>
          <div style={{
            position: "absolute", left: 16, right: 16, bottom: 14, color: "#fff",
            display: "flex", alignItems: "flex-end", justifyContent: "space-between",
          }}>
            <div>
              <div style={{ fontSize: 16, fontWeight: 700, letterSpacing: "-0.02em" }}>
                {items[0].label}
              </div>
              <div style={{ fontSize: 11, opacity: 0.85, marginTop: 2 }}>36 조각 · 약 8분</div>
            </div>
            <button style={{
              ...btnStyle(P), padding: "9px 18px", fontSize: 13, fontWeight: 700,
              borderRadius: 999, background: P.surface, color: P.primaryDeep,
            }}>플레이</button>
          </div>
        </div>
      </div>

      {/* 섹션: 컬렉션 */}
      <div style={{ flex: 1, overflow: "auto", paddingBottom: 4 }}>
        <div style={{ padding: "10px 20px 6px", display: "flex", justifyContent: "space-between", alignItems: "baseline" }}>
          <div style={{ fontSize: 15, fontWeight: 700, letterSpacing: "-0.02em" }}>이달의 컬렉션</div>
          <div style={{ fontSize: 12, color: P.textSoft, fontWeight: 500 }}>전체 보기 ›</div>
        </div>
        <div style={{ display: "flex", gap: 10, padding: "0 20px 8px", overflowX: "auto" }}>
          {items.slice(0, 6).map((it, i) => (
            <div key={it.id} style={{ flexShrink: 0, width: 110 }}>
              <div style={{
                width: 110, height: 110, borderRadius: 14, overflow: "hidden",
                background: P.surface, position: "relative",
                boxShadow: "0 2px 8px rgba(60,40,30,0.06)",
                opacity: i >= 4 ? 0.55 : 1,
              }}>
                <PuzzleImage item={it} size={110} showGrid={false} locked={i >= 4} />
                {i >= 4 && (
                  <div style={{
                    position: "absolute", inset: 0, display: "flex",
                    alignItems: "center", justifyContent: "center",
                  }}>
                    <Icon name="lock" size={20} color="rgba(255,255,255,0.95)" />
                  </div>
                )}
              </div>
              <div style={{ fontSize: 12, fontWeight: 600, marginTop: 6, color: P.text }}>{it.label}</div>
              <div style={{ fontSize: 10, color: P.textSoft, marginTop: 1 }}>
                {i === 0 ? "★★★ 완료" : i === 1 ? "진행 64%" : `${36 + i * 8} 조각`}
              </div>
            </div>
          ))}
        </div>
      </div>

      <BannerAd P={P} />
      <BottomNav P={P} active="home" />
    </div>
  );
}

// 홈 안 C: 미니멀 / 우드 톤 - 한 줄당 1개 큰 카드
function HomeMinimal({ palette: P, theme, onOpenStage }) {
  const items = THEMES[theme].items;
  return (
    <div style={{
      position: "absolute", inset: 0, background: P.bg,
      display: "flex", flexDirection: "column", color: P.text,
      fontFamily: '"Pretendard", "Apple SD Gothic Neo", system-ui, sans-serif',
    }}>
      {/* 미니멀 헤더 */}
      <div style={{
        padding: "16px 20px 14px",
        display: "flex", alignItems: "center", justifyContent: "space-between",
        borderBottom: `1px solid ${P.line}`,
      }}>
        <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
          <div style={{ fontSize: 19, fontWeight: 700, letterSpacing: "-0.02em" }}>퍼즐 정원</div>
          <div style={{
            padding: "2px 8px", borderRadius: 999, background: P.accentSoft,
            fontSize: 10, fontWeight: 600, color: P.primaryDeep,
          }}>VIP</div>
        </div>
        <div style={{ display: "flex", gap: 14, alignItems: "center" }}>
          <CoinPill P={P} value="2,840" minimal />
          <Icon name="trophy" size={20} color={P.textSoft} />
          <Icon name="settings" size={20} color={P.textSoft} />
        </div>
      </div>

      {/* 일일 퍼즐 — 텍스트 위주 */}
      <div style={{ padding: "16px 20px 0" }}>
        <div style={{ fontSize: 11, color: P.primaryDeep, fontWeight: 700, letterSpacing: "0.08em" }}>
          DAILY · 5월 1일
        </div>
        <div style={{ fontSize: 24, fontWeight: 700, letterSpacing: "-0.025em", marginTop: 4 }}>
          {items[0].label}
        </div>
        <div style={{ fontSize: 13, color: P.textSoft, marginTop: 2 }}>
          매일 자정 새 퍼즐이 열려요
        </div>
      </div>

      {/* 데일리 카드 큰 이미지 */}
      <div style={{ padding: "12px 20px 0" }}>
        <div style={{
          borderRadius: 14, overflow: "hidden", position: "relative", height: 140,
          background: P.surface, boxShadow: "0 1px 3px rgba(0,0,0,0.05)",
        }}>
          <PuzzleImage item={items[0]} size={300} showGrid={false} />
          <button style={{
            position: "absolute", right: 14, bottom: 14,
            ...btnStyle(P), padding: "10px 22px", fontSize: 14, fontWeight: 700,
            borderRadius: 999, background: P.text, color: P.surface,
          }}>플레이 →</button>
        </div>
      </div>

      {/* 컬렉션 리스트 */}
      <div style={{
        flex: 1, overflow: "auto", padding: "16px 20px 8px",
      }}>
        <div style={{ fontSize: 11, color: P.textSoft, fontWeight: 700, letterSpacing: "0.08em", marginBottom: 10 }}>
          나의 컬렉션 · 11 / 30
        </div>
        {items.slice(1, 5).map((it, i) => (
          <div key={it.id} style={{
            display: "flex", alignItems: "center", gap: 12,
            padding: "10px 0", borderBottom: i < 3 ? `1px solid ${P.line}` : "none",
          }}>
            <div style={{
              width: 56, height: 56, borderRadius: 10, overflow: "hidden", flexShrink: 0,
              opacity: i >= 2 ? 0.5 : 1,
            }}>
              <PuzzleImage item={it} size={56} showGrid={false} locked={i >= 2} />
            </div>
            <div style={{ flex: 1, minWidth: 0 }}>
              <div style={{ fontSize: 14, fontWeight: 600, letterSpacing: "-0.01em" }}>
                {it.label}
              </div>
              <div style={{ fontSize: 11, color: P.textSoft, marginTop: 2 }}>
                {i === 0 ? "진행 64% · 마저 풀기" : i === 1 ? "새로 잠금해제!" : "다음 단계 잠금"}
              </div>
            </div>
            {i < 2 ? (
              <Icon name="chevron" size={18} color={P.textSoft} />
            ) : (
              <Icon name="lock" size={16} color={P.textMuted} />
            )}
          </div>
        ))}
      </div>

      <BannerAd P={P} />
      <BottomNav P={P} active="home" minimal />
    </div>
  );
}

// ═════════════════════════════════════════════════════════════════════
// 게임 플레이 화면
// ═════════════════════════════════════════════════════════════════════

// 플레이 안 A: 정통 — 큰 보드 + 하단 트레이
function PlayClassic({ palette: P, theme }) {
  const item = THEMES[theme].items[0];
  return (
    <div style={{
      position: "absolute", inset: 0, background: P.bg,
      display: "flex", flexDirection: "column", color: P.text,
      fontFamily: '"Pretendard", "Apple SD Gothic Neo", system-ui, sans-serif',
    }}>
      {/* 상단 툴바 */}
      <div style={{
        padding: "10px 14px", display: "flex", alignItems: "center", gap: 8,
        borderBottom: `1px solid ${P.line}`, background: P.surface,
      }}>
        <IconBtn P={P} size={36}><Icon name="back" size={18} color={P.text} /></IconBtn>
        <div style={{ flex: 1, display: "flex", alignItems: "center", gap: 6, justifyContent: "center" }}>
          <div style={{
            display: "flex", alignItems: "center", gap: 4,
            padding: "6px 12px", borderRadius: 999, background: P.accentSoft,
            color: P.primaryDeep, fontSize: 12, fontWeight: 700,
          }}>
            <Icon name="sparkle" size={12} color={P.primaryDeep} strokeWidth={2.4} />
            36 / 72
          </div>
        </div>
        <IconBtn P={P} size={36}><Icon name="pause" size={16} color={P.text} /></IconBtn>
        <IconBtn P={P} size={36} highlight badge="3">
          <Icon name="bulb" size={16} color="#fff" />
        </IconBtn>
      </div>

      {/* 진행 바 */}
      <div style={{ height: 4, background: P.line, position: "relative" }}>
        <div style={{
          width: "50%", height: "100%",
          background: `linear-gradient(90deg, ${P.primary}, ${P.gold})`,
        }} />
      </div>

      {/* 보드 영역 */}
      <div style={{
        flex: 1, padding: "16px 14px 12px", display: "flex",
        flexDirection: "column", alignItems: "center", justifyContent: "center",
        background: `radial-gradient(ellipse at center, ${P.surfaceAlt} 0%, ${P.bg} 100%)`,
      }}>
        <div style={{
          width: 320, maxWidth: "100%", aspectRatio: "1", borderRadius: 8,
          background: `repeating-linear-gradient(45deg, ${P.line}, ${P.line} 10px, ${P.surface} 10px, ${P.surface} 20px)`,
          opacity: 0.5, position: "relative", overflow: "hidden",
        }}>
          {/* 부분적으로 맞춰진 조각들 */}
          <div style={{ position: "absolute", inset: 0 }}>
            <PuzzleBoardFilled item={item} P={P} progress={0.5} />
          </div>
        </div>
      </div>

      {/* 트레이 (드래그 가능한 조각들) */}
      <div style={{
        background: P.surface, borderTop: `1px solid ${P.line}`,
        padding: "12px 14px 10px",
      }}>
        <div style={{
          display: "flex", justifyContent: "space-between",
          alignItems: "center", marginBottom: 8,
        }}>
          <div style={{ fontSize: 12, fontWeight: 600, color: P.textSoft }}>
            남은 조각 · <b style={{ color: P.text, fontWeight: 700 }}>36</b>
          </div>
        </div>
        <div style={{
          display: "flex", gap: 10, overflowX: "auto", padding: "4px 0 8px",
        }}>
          {[0, 1, 2, 3].map((i) => (
            <div key={i} style={{
              flexShrink: 0, width: 64, height: 64,
              display: "flex", alignItems: "center", justifyContent: "center",
              background: P.surfaceAlt, borderRadius: 10,
              padding: 4,
            }}>
              <JigsawPiece item={item} x={i % 2} y={Math.floor(i / 2)} w={48} h={48}
                totalW={96} totalH={96}
                knobs={{ top: i < 2 ? 0 : 1, right: i % 2 === 0 ? 1 : 0, bottom: i >= 2 ? 0 : -1, left: i % 2 === 0 ? 0 : -1 }} />
            </div>
          ))}
        </div>
      </div>

      <BannerAd P={P} />
    </div>
  );
}

// 플레이 안 B: 미리보기 + 카테고리 분리 트레이
function PlayPreview({ palette: P, theme }) {
  const item = THEMES[theme].items[0];
  return (
    <div style={{
      position: "absolute", inset: 0, background: P.bg,
      display: "flex", flexDirection: "column", color: P.text,
      fontFamily: '"Pretendard", "Apple SD Gothic Neo", system-ui, sans-serif',
    }}>
      {/* 상단 */}
      <div style={{
        padding: "10px 14px 8px", display: "flex",
        alignItems: "center", gap: 10, background: P.surface,
        borderBottom: `1px solid ${P.line}`,
      }}>
        <IconBtn P={P} size={36}><Icon name="back" size={18} color={P.text} /></IconBtn>
        {/* 미리보기 썸네일 */}
        <div style={{
          width: 44, height: 44, borderRadius: 8, overflow: "hidden",
          border: `2px solid ${P.primary}`, flexShrink: 0,
        }}>
          <PuzzleImage item={item} size={44} showGrid={false} />
        </div>
        <div style={{ flex: 1, minWidth: 0 }}>
          <div style={{ fontSize: 13, fontWeight: 700, letterSpacing: "-0.01em" }}>
            {item.label}
          </div>
          <div style={{ display: "flex", gap: 8, marginTop: 2 }}>
            <span style={{ fontSize: 11, color: P.primaryDeep, fontWeight: 600 }}>50% 완료 · 36조각 남음</span>
          </div>
        </div>
        <IconBtn P={P} size={36}><Icon name="eye" size={16} color={P.text} /></IconBtn>
        <IconBtn P={P} size={36}><Icon name="pause" size={16} color={P.text} /></IconBtn>
      </div>

      {/* 보드 */}
      <div style={{
        flex: 1, padding: "12px 14px", display: "flex",
        flexDirection: "column", alignItems: "center", justifyContent: "center",
        background: P.bg,
      }}>
        <div style={{
          width: "100%", aspectRatio: "1", borderRadius: 12,
          background: P.surface, border: `1px dashed ${P.line}`,
          padding: 8, position: "relative", overflow: "hidden",
          boxShadow: `inset 0 1px 4px rgba(0,0,0,0.04)`,
        }}>
          <div style={{ position: "absolute", inset: 8 }}>
            <PuzzleBoardFilled item={item} P={P} progress={0.5} />
          </div>
        </div>
        {/* 도구 행 */}
        <div style={{
          display: "flex", gap: 8, marginTop: 12, width: "100%",
        }}>
          <ToolBtn P={P} icon="bulb" label="힌트" badge="3" highlight />
          <ToolBtn P={P} icon="image" label="원본 보기" />
          <ToolBtn P={P} icon="swap" label="자동 정렬" />
        </div>
      </div>

      {/* 트레이 */}
      <div style={{
        background: P.surface, borderTop: `1px solid ${P.line}`,
      }}>
        <div style={{
          display: "flex", justifyContent: "space-between", alignItems: "center",
          padding: "10px 14px 6px",
        }}>
          <div style={{ fontSize: 12, fontWeight: 600, color: P.textSoft }}>
            남은 조각 · <b style={{ color: P.text, fontWeight: 700 }}>36</b>
          </div>
        </div>
        <div style={{
          display: "flex", gap: 8, padding: "0 14px 12px", overflowX: "auto",
        }}>
          {[0, 1, 2, 3, 4].map((i) => (
            <div key={i} style={{
              flexShrink: 0, width: 56, height: 56,
              display: "flex", alignItems: "center", justifyContent: "center",
              background: P.surfaceAlt, borderRadius: 8, padding: 4,
            }}>
              <JigsawPiece item={item} x={i % 2} y={Math.floor(i / 2)} w={42} h={42}
                totalW={84} totalH={84}
                knobs={{ top: i < 3 ? 0 : 1, right: 1, bottom: i >= 3 ? 0 : -1, left: 0 }} />
            </div>
          ))}
        </div>
      </div>

      <BannerAd P={P} />
    </div>
  );
}

// ═════════════════════════════════════════════════════════════════════
// 완성/보상 화면 (광고 핵심 지점)
// ═════════════════════════════════════════════════════════════════════

// 완성 안 A: 셀러브레이션 + 명확한 광고 진입점
function CompleteCelebration({ palette: P, theme }) {
  const item = THEMES[theme].items[0];
  return (
    <div style={{
      position: "absolute", inset: 0, color: P.text,
      fontFamily: '"Pretendard", "Apple SD Gothic Neo", system-ui, sans-serif',
      background: `radial-gradient(circle at top, ${P.starterA} 0%, ${P.bg} 60%)`,
      display: "flex", flexDirection: "column",
    }}>
      {/* 색종이 점들 */}
      <Confetti P={P} />

      <div style={{ flex: 1, padding: "20px 20px 0", overflow: "auto", position: "relative", zIndex: 1 }}>
        <div style={{ textAlign: "center", marginTop: 6 }}>
          <div style={{
            display: "inline-flex", alignItems: "center", gap: 6,
            padding: "5px 12px", borderRadius: 999, background: P.gold,
            fontSize: 11, fontWeight: 700, color: "#fff", letterSpacing: "0.04em",
          }}>
            <Icon name="crown" size={12} color="#fff" /> 레벨 11 클리어
          </div>
          <div style={{ fontSize: 28, fontWeight: 800, letterSpacing: "-0.03em", marginTop: 10, lineHeight: 1.15 }}>
            완성하셨어요!
          </div>
          <div style={{ fontSize: 13, color: P.textSoft, marginTop: 4 }}>
            나만의 갤러리에 그림 한 점이 더해졌어요
          </div>
        </div>

        {/* 완성 이미지 + 액자 */}
        <div style={{
          margin: "20px auto 0", width: 220, height: 220,
          borderRadius: 8, padding: 10, background: P.surface,
          boxShadow: "0 8px 24px rgba(60,40,30,0.18), 0 0 0 1px rgba(60,40,30,0.06)",
          transform: "rotate(-1.5deg)",
        }}>
          <div style={{ width: "100%", height: "100%", borderRadius: 4, overflow: "hidden" }}>
            <PuzzleImage item={item} size={220} showGrid={false} completed />
          </div>
        </div>

        {/* 작품 캡션 */}
        <div style={{ textAlign: "center", marginTop: 18 }}>
          <div style={{ fontSize: 16, fontWeight: 700, letterSpacing: "-0.01em" }}>
            {item.label}
          </div>
          <div style={{ fontSize: 12, color: P.textSoft, marginTop: 3 }}>
            나만의 갤러리에 저장되었어요
          </div>
        </div>
      </div>

      {/* 하단 CTA */}
      <div style={{
        padding: "14px 20px 16px", background: P.surface,
        borderTop: `1px solid ${P.line}`,
      }}>
        <button style={{
          width: "100%", display: "flex", alignItems: "center", justifyContent: "center",
          gap: 8, padding: "16px", borderRadius: 16, border: "none",
          background: `linear-gradient(135deg, ${P.primary} 0%, ${P.primaryDeep} 100%)`,
          color: "#fff", fontWeight: 800, fontSize: 16,
          letterSpacing: "-0.01em", boxShadow: `0 4px 14px ${P.primary}60`,
          fontFamily: "inherit", cursor: "pointer",
        }}>
          다음 그림 시작하기 <Icon name="chevron" size={14} color="#fff" />
        </button>
        <div style={{ display: "flex", gap: 8, marginTop: 10 }}>
          <button style={{
            flex: 1, padding: "12px", border: `1px solid ${P.line}`,
            background: P.surface, color: P.textSoft, borderRadius: 14,
            fontWeight: 600, fontSize: 13, fontFamily: "inherit", cursor: "pointer",
          }}>갤러리 보기</button>
          <button style={{
            flex: 1, padding: "12px", border: `1px solid ${P.line}`,
            background: P.surface, color: P.textSoft, borderRadius: 14,
            fontWeight: 600, fontSize: 13, fontFamily: "inherit", cursor: "pointer",
            display: "flex", alignItems: "center", justifyContent: "center", gap: 6,
          }}>
            <Icon name="share" size={13} color={P.textSoft} /> 공유
          </button>
        </div>
        <div style={{
          marginTop: 10, textAlign: "center", fontSize: 10,
          color: P.textMuted, letterSpacing: "0.04em",
        }}>다음 그림 시작 시 짧은 광고가 나올 수 있어요</div>
      </div>
    </div>
  );
}

// 완성 안 B: 컬렉션 도감 스타일
function CompleteCollection({ palette: P, theme }) {
  const items = THEMES[theme].items;
  return (
    <div style={{
      position: "absolute", inset: 0, color: P.text,
      fontFamily: '"Pretendard", "Apple SD Gothic Neo", system-ui, sans-serif',
      background: P.bg, display: "flex", flexDirection: "column",
    }}>
      {/* 헤더 */}
      <div style={{
        padding: "16px 20px 12px", textAlign: "center",
        background: `linear-gradient(180deg, ${P.starterB}66 0%, ${P.bg} 100%)`,
        position: "relative",
      }}>
        <div style={{ position: "absolute", top: 12, right: 14 }}>
          <IconBtn P={P} size={32}><Icon name="x" size={14} color={P.textSoft} /></IconBtn>
        </div>
        <div style={{ fontSize: 11, color: P.primaryDeep, fontWeight: 700, letterSpacing: "0.1em" }}>
          NEW UNLOCKED
        </div>
        <div style={{ fontSize: 22, fontWeight: 800, letterSpacing: "-0.025em", marginTop: 4 }}>
          {items[0].label}
        </div>
        <div style={{ fontSize: 12, color: P.textSoft, marginTop: 4 }}>
          컬렉션에 추가되었어요 · 11 / 30
        </div>
      </div>

      {/* 도감 그리드 */}
      <div style={{
        padding: "16px 20px 0", overflow: "auto", flex: 1,
      }}>
        <div style={{
          display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 8,
        }}>
          {items.slice(0, 6).map((it, i) => (
            <div key={it.id} style={{
              aspectRatio: "1", borderRadius: 10, overflow: "hidden",
              position: "relative", background: P.surface,
              border: i === 0 ? `2px solid ${P.gold}` : `1px solid ${P.line}`,
              boxShadow: i === 0 ? `0 0 16px ${P.gold}50` : "none",
              opacity: i === 0 ? 1 : i < 3 ? 0.95 : 0.4,
            }}>
              <PuzzleImage item={it} size={100} showGrid={false} locked={i >= 3} />
              {i === 0 && (
                <div style={{
                  position: "absolute", top: 4, left: 4, padding: "2px 6px",
                  background: P.gold, color: "#fff", fontSize: 9, fontWeight: 700,
                  borderRadius: 4,
                }}>NEW</div>
              )}
              <div style={{
                position: "absolute", bottom: 0, left: 0, right: 0,
                padding: "12px 6px 4px", color: "#fff", fontSize: 10, fontWeight: 600,
                background: "linear-gradient(180deg, transparent, rgba(0,0,0,0.5))",
                textAlign: "center",
              }}>{i < 3 ? `#${String(i + 9).padStart(2, "0")}` : "잠금"}</div>
            </div>
          ))}
        </div>

        {/* 도감 안내 */}
        <div style={{
          marginTop: 16, padding: 14, background: P.surface,
          borderRadius: 16, border: `1px solid ${P.line}`,
          display: "flex", alignItems: "center", gap: 10,
        }}>
          <div style={{
            width: 36, height: 36, borderRadius: 10, background: P.accentSoft,
            display: "flex", alignItems: "center", justifyContent: "center",
          }}>
            <Icon name="trophy" size={18} color={P.primaryDeep} />
          </div>
          <div style={{ flex: 1 }}>
            <div style={{ fontSize: 13, fontWeight: 700 }}>{2} / {THEMES[theme].items.length} 점 모았어요</div>
            <div style={{ fontSize: 11, color: P.textSoft, marginTop: 2 }}>
              꾸준히 모아 나만의 갤러리를 채워보세요
            </div>
          </div>
        </div>
      </div>

      {/* 하단 액션 */}
      <div style={{ padding: "12px 20px 14px", background: P.surface, borderTop: `1px solid ${P.line}` }}>
        <button style={{
          width: "100%", padding: "16px", border: "none",
          background: `linear-gradient(135deg, ${P.primary} 0%, ${P.primaryDeep} 100%)`,
          color: "#fff", borderRadius: 14, boxShadow: `0 4px 14px ${P.primary}50`,
          fontWeight: 700, fontSize: 15, fontFamily: "inherit", cursor: "pointer",
          display: "flex", alignItems: "center", justifyContent: "center", gap: 6,
        }}>
          다음 그림으로 <Icon name="chevron" size={14} color="#fff" />
        </button>
        <div style={{
          marginTop: 8, textAlign: "center", fontSize: 10,
          color: P.textMuted, letterSpacing: "0.04em",
        }}>다음 그림 시작 시 짧은 광고가 나올 수 있어요</div>
      </div>
    </div>
  );
}

// ═════════════════════════════════════════════════════════════════════
// 보상형 광고 모달 — 즉시 광고 진행 (전환율 최적화)
// ═════════════════════════════════════════════════════════════════════
function RewardedAdModal({ palette: P }) {
  return (
    <div style={{
      position: "absolute", inset: 0, color: P.text,
      fontFamily: '"Pretendard", "Apple SD Gothic Neo", system-ui, sans-serif',
      background: "#0F0A0E",
    }}>
      {/* 광고 영상 영역 (전면 광고가 즉시 재생) */}
      <div style={{
        position: "absolute", inset: 0,
        background: `
          radial-gradient(ellipse 80% 60% at 50% 30%, #4A2D5C 0%, #1A0E1F 60%, #0F0A0E 100%)
        `,
        display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center",
      }}>
        {/* 상단 광고 표시 + 닫기 (5초 후 활성화 안내) */}
        <div style={{
          position: "absolute", top: 0, left: 0, right: 0,
          padding: "14px 14px",
          display: "flex", justifyContent: "space-between", alignItems: "center",
          background: "linear-gradient(180deg, rgba(0,0,0,0.5) 0%, transparent 100%)",
        }}>
          <div style={{
            padding: "5px 10px", borderRadius: 6,
            background: "rgba(255,255,255,0.18)", color: "#fff",
            fontSize: 10, fontWeight: 700, letterSpacing: "0.06em",
          }}>광고 · AD</div>
          <div style={{
            display: "flex", alignItems: "center", gap: 8,
          }}>
            <div style={{
              padding: "5px 10px", borderRadius: 999,
              background: "rgba(255,255,255,0.12)", color: "rgba(255,255,255,0.85)",
              fontSize: 11, fontWeight: 700,
            }}>00:23 남음</div>
            <div style={{
              width: 28, height: 28, borderRadius: 14,
              background: "rgba(255,255,255,0.12)", color: "rgba(255,255,255,0.5)",
              display: "flex", alignItems: "center", justifyContent: "center",
              fontSize: 14, fontWeight: 700, cursor: "not-allowed",
            }}>×</div>
          </div>
        </div>

        {/* 가짜 광고 콘텐츠 (시각적 placeholder) */}
        <div style={{
          width: 220, height: 220, borderRadius: 24,
          background: "linear-gradient(135deg, #FF6B9D 0%, #C147E9 50%, #6E40C9 100%)",
          display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center",
          gap: 12, boxShadow: "0 20px 60px rgba(193, 71, 233, 0.4)",
        }}>
          <div style={{
            width: 56, height: 56, borderRadius: 16,
            background: "rgba(255,255,255,0.95)",
            display: "flex", alignItems: "center", justifyContent: "center",
            fontSize: 28,
          }}>🎮</div>
          <div style={{ color: "#fff", fontSize: 16, fontWeight: 800, letterSpacing: "-0.02em" }}>
            Sample Brand
          </div>
          <div style={{ color: "rgba(255,255,255,0.85)", fontSize: 11, fontWeight: 500 }}>
            지금 다운로드 받기
          </div>
        </div>

        <div style={{ marginTop: 28, color: "rgba(255,255,255,0.5)", fontSize: 11, fontWeight: 500 }}>
          광고 영상이 재생 중입니다
        </div>
      </div>

      {/* 하단 보상 안내 바 (광고 끝나면 받게 될 보상) */}
      <div style={{
        position: "absolute", bottom: 0, left: 0, right: 0,
        padding: "14px 16px 18px",
        background: "linear-gradient(180deg, transparent 0%, rgba(0,0,0,0.85) 40%)",
      }}>
        <div style={{
          display: "flex", alignItems: "center", gap: 12,
          padding: "12px 14px",
          background: "rgba(255,255,255,0.1)",
          backdropFilter: "blur(20px)",
          borderRadius: 16,
          border: "1px solid rgba(255,255,255,0.15)",
        }}>
          <div style={{
            width: 40, height: 40, borderRadius: 12,
            background: P.gold,
            display: "flex", alignItems: "center", justifyContent: "center",
            fontSize: 22,
            boxShadow: `0 4px 12px ${P.gold}80`,
          }}>💡</div>
          <div style={{ flex: 1 }}>
            <div style={{ fontSize: 11, color: "rgba(255,255,255,0.6)", fontWeight: 600 }}>
              광고 시청 후
            </div>
            <div style={{ fontSize: 14, fontWeight: 800, color: "#fff", marginTop: 1 }}>
              힌트 1개가 지급돼요
            </div>
          </div>
          <div style={{
            display: "flex", alignItems: "center", gap: 5,
            padding: "8px 12px", borderRadius: 999,
            background: "rgba(255,255,255,0.15)",
            fontSize: 11, fontWeight: 700, color: "#fff",
          }}>
            <span style={{
              width: 6, height: 6, borderRadius: "50%", background: "#FF5A5A",
            }} />
            진행 중
          </div>
        </div>
      </div>
    </div>
  );
}

// ═════════════════════════════════════════════════════════════════════
// 공통 작은 컴포넌트
// ═════════════════════════════════════════════════════════════════════

const btnStyle = (P) => ({
  border: "none", cursor: "pointer", fontFamily: "inherit",
  transition: "transform 0.1s",
});

function CoinPill({ P, value, minimal }) {
  if (minimal) return (
    <div style={{ display: "flex", alignItems: "center", gap: 4, fontSize: 13, fontWeight: 700, color: P.text }}>
      <span style={{ fontSize: 14 }}>🪙</span>{value}
    </div>
  );
  return (
    <div style={{
      display: "flex", alignItems: "center", gap: 4,
      padding: "5px 10px 5px 6px", borderRadius: 999,
      background: P.surface, border: `1px solid ${P.line}`,
      fontSize: 12, fontWeight: 700, color: P.text,
    }}>
      <div style={{
        width: 20, height: 20, borderRadius: "50%",
        background: P.gold, color: "#fff",
        display: "flex", alignItems: "center", justifyContent: "center",
        fontSize: 11, fontWeight: 800,
      }}>₩</div>
      {value}
    </div>
  );
}

function HeartPill({ P, value }) {
  return (
    <div style={{
      display: "flex", alignItems: "center", gap: 4,
      padding: "5px 10px 5px 6px", borderRadius: 999,
      background: P.surface, border: `1px solid ${P.line}`,
      fontSize: 12, fontWeight: 700, color: P.text,
    }}>
      <span style={{ fontSize: 12 }}>💗</span>{value}
    </div>
  );
}

function IconBtn({ P, children, size = 36, highlight, badge }) {
  return (
    <div style={{
      position: "relative",
      width: size, height: size, borderRadius: size / 2.4,
      background: highlight ? P.primary : P.surfaceAlt,
      display: "flex", alignItems: "center", justifyContent: "center",
      cursor: "pointer",
    }}>
      {children}
      {badge && (
        <div style={{
          position: "absolute", top: -3, right: -3, minWidth: 16, height: 16,
          padding: "0 4px", borderRadius: 8, background: P.gold,
          fontSize: 9, fontWeight: 800, color: "#fff",
          display: "flex", alignItems: "center", justifyContent: "center",
          border: `1.5px solid ${P.surface}`,
        }}>{badge}</div>
      )}
    </div>
  );
}

function ToolBtn({ P, icon, label, badge, highlight }) {
  return (
    <div style={{
      flex: 1, display: "flex", flexDirection: "column", alignItems: "center",
      gap: 3, padding: "8px 4px", borderRadius: 12,
      background: highlight ? P.primary + "12" : P.surface,
      border: `1px solid ${highlight ? P.primary + "55" : P.line}`,
      position: "relative",
    }}>
      <Icon name={icon} size={16} color={highlight ? P.primaryDeep : P.text} strokeWidth={1.8} />
      <div style={{ fontSize: 10, fontWeight: 600, color: highlight ? P.primaryDeep : P.textSoft }}>
        {label}
      </div>
      {badge && (
        <div style={{
          position: "absolute", top: 4, right: 8, minWidth: 14, height: 14,
          borderRadius: 7, background: P.gold, color: "#fff",
          fontSize: 9, fontWeight: 800, padding: "0 3px",
          display: "flex", alignItems: "center", justifyContent: "center",
        }}>{badge}</div>
      )}
    </div>
  );
}

function StageCard({ P, item, idx, locked, progress, stars, pieces, onClick }) {
  return (
    <div onClick={onClick} style={{
      position: "relative", borderRadius: 16, overflow: "hidden",
      background: P.surface, aspectRatio: "1",
      boxShadow: "0 2px 8px rgba(60,40,30,0.06)",
      cursor: locked ? "default" : "pointer",
    }}>
      <PuzzleImage item={item} size={150} showGrid={false} locked={locked} />
      {/* 번호 */}
      <div style={{
        position: "absolute", top: 8, left: 8, width: 26, height: 26, borderRadius: 8,
        background: "rgba(40,25,20,0.55)", color: "#fff", fontSize: 11, fontWeight: 700,
        display: "flex", alignItems: "center", justifyContent: "center",
      }}>{idx}</div>
      {/* 잠금 / 별 / 진행 */}
      {locked ? (
        <div style={{
          position: "absolute", inset: 0, display: "flex", alignItems: "center", justifyContent: "center",
        }}>
          <div style={{
            width: 36, height: 36, borderRadius: "50%", background: "rgba(255,255,255,0.85)",
            display: "flex", alignItems: "center", justifyContent: "center",
          }}>
            <Icon name="lock" size={18} color={P.textSoft} />
          </div>
        </div>
      ) : stars ? (
        <div style={{
          position: "absolute", top: 8, right: 8, padding: "3px 7px", borderRadius: 999,
          background: "rgba(255,255,255,0.92)", fontSize: 11, fontWeight: 700,
          color: P.gold, letterSpacing: "0.05em",
        }}>{"★".repeat(stars)}</div>
      ) : (
        <div style={{
          position: "absolute", top: 8, right: 8, padding: "3px 8px", borderRadius: 999,
          background: P.primary, fontSize: 10, fontWeight: 700, color: "#fff",
        }}>NEW</div>
      )}
      {/* 하단 정보 */}
      <div style={{
        position: "absolute", left: 0, right: 0, bottom: 0,
        padding: "16px 10px 8px",
        background: "linear-gradient(180deg, transparent, rgba(40,25,20,0.65))",
        color: "#fff",
      }}>
        <div style={{ fontSize: 11, fontWeight: 700, letterSpacing: "-0.01em" }}>{item.label}</div>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginTop: 2 }}>
          <div style={{ fontSize: 9, opacity: 0.85 }}>{pieces} 조각</div>
          {!locked && progress > 0 && progress < 100 && (
            <div style={{ fontSize: 9, fontWeight: 700, color: P.gold }}>{progress}%</div>
          )}
        </div>
        {!locked && progress > 0 && progress < 100 && (
          <div style={{
            marginTop: 4, height: 3, background: "rgba(255,255,255,0.3)",
            borderRadius: 2, overflow: "hidden",
          }}>
            <div style={{ width: `${progress}%`, height: "100%", background: P.gold }} />
          </div>
        )}
      </div>
    </div>
  );
}

function BannerAd({ P }) {
  return (
    <div style={{
      height: 56, background: "#F2EEE5",
      borderTop: `1px solid ${P.line}`,
      display: "flex", alignItems: "center", padding: "0 12px", gap: 10,
      position: "relative",
    }}>
      <div style={{
        position: "absolute", top: 4, left: 4, padding: "1px 5px",
        background: "#888", color: "#fff", fontSize: 8, fontWeight: 700,
        borderRadius: 3, letterSpacing: "0.05em",
      }}>AD</div>
      <div style={{
        width: 36, height: 36, borderRadius: 8,
        background: "linear-gradient(135deg, #C9A8E8, #8AC9D8)",
        display: "flex", alignItems: "center", justifyContent: "center",
        fontSize: 18,
      }}>🛍</div>
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{ fontSize: 12, fontWeight: 700, color: "#3A3026" }}>봄맞이 특가 · 최대 70% 할인</div>
        <div style={{ fontSize: 10, color: "#7A6D5C", marginTop: 1 }}>홈쇼핑 · 무료배송 5월 한정</div>
      </div>
      <button style={{
        padding: "6px 12px", borderRadius: 8, border: "none",
        background: "#3A6BD0", color: "#fff", fontSize: 11, fontWeight: 700,
        fontFamily: "inherit", cursor: "pointer",
      }}>더 보기</button>
    </div>
  );
}

function BottomNav({ P, active, minimal }) {
  const items = [
    { k: "home", l: "메인", icon: "home" },
    { k: "daily", l: "일일", icon: "calendar" },
    { k: "collection", l: "도감", icon: "image" },
    { k: "shop", l: "상점", icon: "gift" },
  ];
  return (
    <div style={{
      display: "flex", borderTop: `1px solid ${P.line}`, background: P.surface,
      padding: minimal ? "6px 0 8px" : "8px 0 10px",
    }}>
      {items.map((it) => {
        const a = it.k === active;
        return (
          <div key={it.k} style={{
            flex: 1, display: "flex", flexDirection: "column",
            alignItems: "center", gap: 2,
          }}>
            <Icon name={it.icon} size={20} color={a ? P.primary : P.textMuted} strokeWidth={a ? 2 : 1.7} />
            <div style={{ fontSize: 10, fontWeight: a ? 700 : 500, color: a ? P.primary : P.textMuted }}>{it.l}</div>
          </div>
        );
      })}
    </div>
  );
}

function PuzzleBoardFilled({ item, P, progress }) {
  // 4x4 그리드, progress 비율만큼 채워짐
  const cells = 4;
  const total = cells * cells;
  const filled = Math.floor(total * progress);
  const cellSize = 100 / cells;
  return (
    <div style={{ position: "absolute", inset: 0 }}>
      {Array.from({ length: total }).map((_, i) => {
        const x = i % cells;
        const y = Math.floor(i / cells);
        const placed = i < filled;
        return (
          <div key={i} style={{
            position: "absolute",
            left: `${x * cellSize}%`, top: `${y * cellSize}%`,
            width: `${cellSize}%`, height: `${cellSize}%`,
            padding: 1,
          }}>
            {placed ? (
              <div style={{
                width: "100%", height: "100%", overflow: "hidden",
                position: "relative",
              }}>
                <div style={{
                  position: "absolute", inset: 0,
                  width: `${cells * 100}%`, height: `${cells * 100}%`,
                  left: `-${x * 100}%`, top: `-${y * 100}%`,
                }}>
                  <PuzzleImage item={item} size={400} showGrid={false} />
                </div>
              </div>
            ) : (
              <div style={{
                width: "100%", height: "100%",
                background: "rgba(255,255,255,0.2)",
                border: "1px dashed rgba(0,0,0,0.08)",
              }} />
            )}
          </div>
        );
      })}
    </div>
  );
}

function Confetti({ P }) {
  const colors = [P.primary, P.gold, P.accent, P.primaryDeep];
  const dots = Array.from({ length: 28 }).map((_, i) => ({
    left: (i * 73) % 100,
    top: (i * 41) % 80,
    size: 4 + (i % 4) * 2,
    rot: (i * 37) % 360,
    color: colors[i % colors.length],
  }));
  return (
    <div style={{ position: "absolute", inset: 0, pointerEvents: "none" }}>
      {dots.map((d, i) => (
        <div key={i} style={{
          position: "absolute", left: `${d.left}%`, top: `${d.top}%`,
          width: d.size, height: d.size * 1.4,
          background: d.color, borderRadius: 1,
          transform: `rotate(${d.rot}deg)`, opacity: 0.85,
        }} />
      ))}
    </div>
  );
}

function RewardChip({ P, icon, label }) {
  return (
    <div style={{
      flex: 1, display: "flex", alignItems: "center", gap: 6,
      padding: "8px 10px", background: P.surfaceAlt, borderRadius: 10,
      fontSize: 12, fontWeight: 700, color: P.text,
    }}>
      <span style={{ fontSize: 16 }}>{icon}</span>{label}
    </div>
  );
}

function BigReward({ P, icon, v }) {
  return (
    <div style={{
      display: "flex", flexDirection: "column", alignItems: "center", gap: 2,
      width: 50, padding: "6px 0", background: P.surfaceAlt, borderRadius: 10,
    }}>
      <span style={{ fontSize: 18 }}>{icon}</span>
      <span style={{ fontSize: 12, fontWeight: 800 }}>+{v}</span>
    </div>
  );
}

// expose
Object.assign(window, {
  PALETTES, THEMES,
  HomeGallery, HomeStory, HomeMinimal,
  PlayClassic, PlayPreview,
  CompleteCelebration, CompleteCollection,
  RewardedAdModal,
  PuzzleImage, Icon, IconBtn, CoinPill, BannerAd, BottomNav,
});
