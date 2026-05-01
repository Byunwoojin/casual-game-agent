// ═════════════════════════════════════════════════════════════════════
// 추가 화면 — HomeMap (도장깨기 레벨 맵), GalleryMuseum, GalleryCodex
// ═════════════════════════════════════════════════════════════════════

const { PALETTES, THEMES, PuzzleImage, Icon, IconBtn, CoinPill, BannerAd, BottomNav } = window;

// ─────────────────────────────────────────────────────────────────────
// HomeSequential — 순차 잠금 그리드 (도장깨기 · 3060 감성)
// 잡지 아카이브 / 도감 메타포: 클리어한 작품 컬러, 다음 한 칸 강조, 나머지 잠금
// ─────────────────────────────────────────────────────────────────────
function HomeSequential({ palette: P, theme }) {
  const items = THEMES[theme].items;
  const total = 30; // 30~50 라이브러리 가정
  const currentLevel = 14; // 다음 도전 = 14번
  const cleared = currentLevel - 1; // 13개 완성

  // 30개 슬롯 생성: 각각 작품 + 상태
  const slots = Array.from({ length: total }).map((_, i) => {
    const it = items[i % items.length];
    return {
      ...it,
      uniqueId: `${it.id}-${i}`,
      level: i + 1,
      state: i + 1 < currentLevel ? "cleared" : i + 1 === currentLevel ? "current" : "locked",
      stars: i + 1 < currentLevel ? ((i * 7 + 1) % 3) + 1 : 0,
      pieces: 36 + Math.min(Math.floor(i / 5), 4) * 24,
    };
  });

  // 3열로 보이도록 9의 배수에 맞춰 표시 (30 → 30개 = 10행)

  return (
    <div style={{
      position: "absolute", inset: 0, color: P.text,
      fontFamily: '"Pretendard", "Apple SD Gothic Neo", system-ui, sans-serif',
      display: "flex", flexDirection: "column",
      background: P.bg,
    }}>
      {/* 상단 헤더 */}
      <div style={{
        padding: "14px 20px 12px",
        background: P.surface,
        borderBottom: `1px solid ${P.line}`,
        display: "flex", alignItems: "center", justifyContent: "space-between",
      }}>
        <div>
          <div style={{ fontSize: 11, color: P.textSoft, fontWeight: 500 }}>오늘도 반가워요</div>
          <div style={{ fontSize: 19, fontWeight: 700, letterSpacing: "-0.025em", marginTop: 1 }}>
            그림 한 점, 시작해 볼까요
          </div>
        </div>
        <div style={{ display: "flex", alignItems: "center", gap: 6 }}>
          <CoinPill P={P} value="2,840" />
          <IconBtn P={P}><Icon name="settings" size={18} color={P.textSoft} /></IconBtn>
        </div>
      </div>

      {/* 진행도 + 다음 마일스톤 */}
      <div style={{
        padding: "14px 20px 14px",
        background: P.surface,
        borderBottom: `1px solid ${P.line}`,
      }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "baseline", marginBottom: 8 }}>
          <div style={{ fontSize: 13, fontWeight: 700, letterSpacing: "-0.01em" }}>
            <span style={{ color: P.primaryDeep, fontWeight: 800 }}>{cleared}점</span>
            <span style={{ color: P.textSoft, fontWeight: 500 }}> / {total}점 완성</span>
          </div>
          <div style={{ fontSize: 11, color: P.textSoft, fontWeight: 600 }}>
            다음 마일스톤까지 <b style={{ color: P.text }}>+2점</b>
          </div>
        </div>
        <div style={{ position: "relative", height: 6, background: P.line, borderRadius: 3, overflow: "hidden" }}>
          <div style={{
            width: `${(cleared / total) * 100}%`, height: "100%",
            background: `linear-gradient(90deg, ${P.primary}, ${P.gold})`, borderRadius: 3,
          }} />
          {/* 마일스톤 점 (5/10/15/20/25/30) */}
          {[5, 10, 15, 20, 25].map((m) => (
            <div key={m} style={{
              position: "absolute", left: `${(m / total) * 100}%`, top: "50%",
              width: 2, height: 6, background: P.surface, transform: "translateX(-1px) translateY(-3px)",
            }} />
          ))}
        </div>
      </div>

      {/* "지금 도전" 강조 카드 */}
      <div style={{ padding: "14px 16px 8px" }}>
        <div style={{
          background: P.surface,
          borderRadius: 16,
          border: `1px solid ${P.line}`,
          padding: 12,
          display: "flex", alignItems: "center", gap: 12,
          boxShadow: `0 2px 8px rgba(60,40,30,0.05)`,
        }}>
          <div style={{
            width: 64, height: 64, borderRadius: 10,
            overflow: "hidden", flexShrink: 0,
            border: `1px solid ${P.line}`,
            position: "relative",
          }}>
            <PuzzleImage item={slots[currentLevel - 1]} size={64} showGrid={false} />
            <div style={{
              position: "absolute", inset: 0,
              background: `linear-gradient(135deg, transparent 50%, ${P.primary}30 100%)`,
            }} />
          </div>
          <div style={{ flex: 1, minWidth: 0 }}>
            <div style={{
              display: "inline-flex", alignItems: "center", gap: 4,
              padding: "2px 7px", borderRadius: 4,
              background: P.primary, color: "#fff",
              fontSize: 9, fontWeight: 800, letterSpacing: "0.04em",
            }}>
              지금 도전
            </div>
            <div style={{ fontSize: 15, fontWeight: 700, marginTop: 5, letterSpacing: "-0.02em" }}>
              {currentLevel}. {slots[currentLevel - 1].label}
            </div>
            <div style={{ fontSize: 11, color: P.textSoft, marginTop: 2 }}>
              {slots[currentLevel - 1].pieces}조각 · 약 8분
            </div>
          </div>
          <button style={{
            padding: "10px 16px", border: "none", borderRadius: 999,
            background: P.text, color: P.surface,
            fontSize: 13, fontWeight: 700, fontFamily: "inherit", cursor: "pointer",
            display: "flex", alignItems: "center", gap: 4,
          }}>
            계속하기 <Icon name="chevron" size={12} color={P.surface} />
          </button>
        </div>
      </div>

      {/* 그리드 (전체 작품) */}
      <div style={{
        flex: 1, overflow: "auto",
        padding: "8px 16px 16px",
      }}>
        {/* 섹션 라벨 */}
        <div style={{
          display: "flex", alignItems: "center", gap: 8,
          padding: "8px 0 12px",
        }}>
          <div style={{ fontSize: 12, color: P.textSoft, fontWeight: 700, letterSpacing: "0.06em" }}>
            전체 작품
          </div>
          <div style={{ flex: 1, height: 1, background: P.line }} />
          <div style={{ fontSize: 11, color: P.textMuted, fontWeight: 500 }}>
            순서대로 열려요
          </div>
        </div>

        {/* 3열 그리드 */}
        <div style={{
          display: "grid", gridTemplateColumns: "1fr 1fr 1fr",
          gap: 10,
        }}>
          {slots.map((s) => (
            <SequentialTile key={s.uniqueId} P={P} slot={s} />
          ))}
        </div>
      </div>

      <BannerAd P={P} />
      <BottomNav P={P} active="home" />
    </div>
  );
}

function SequentialTile({ P, slot }) {
  const cleared = slot.state === "cleared";
  const current = slot.state === "current";
  const locked = slot.state === "locked";

  return (
    <div style={{
      position: "relative",
      borderRadius: 8, overflow: "hidden",
      background: cleared ? P.surface : P.surfaceAlt,
      border: current ? `2px solid ${P.primary}` : `1px solid ${P.line}`,
      boxShadow: current ? `0 4px 14px ${P.primary}40` : "none",
      transform: current ? "scale(1.02)" : "none",
    }}>
      {/* 이미지 영역 */}
      <div style={{
        width: "100%", aspectRatio: "1",
        position: "relative",
      }}>
        {cleared || current ? (
          <PuzzleImage item={slot} size={80} showGrid={false} />
        ) : (
          <div style={{
            width: "100%", height: "100%",
            background: P.surfaceAlt,
            position: "relative",
          }}>
            {/* 잠금 패턴 */}
            <div style={{
              position: "absolute", inset: 0,
              backgroundImage: `repeating-linear-gradient(45deg, ${P.line} 0 1px, transparent 1px 6px)`,
              opacity: 0.6,
            }} />
            <div style={{
              position: "absolute", inset: 0,
              display: "flex", alignItems: "center", justifyContent: "center",
            }}>
              <Icon name="lock" size={14} color={P.textMuted} />
            </div>
          </div>
        )}

        {/* current 오버레이 */}
        {current && (
          <div style={{
            position: "absolute", inset: 0,
            background: `linear-gradient(180deg, transparent 50%, ${P.primary}90 100%)`,
            display: "flex", alignItems: "flex-end", justifyContent: "center",
            paddingBottom: 4,
          }}>
            <div style={{
              fontSize: 8, fontWeight: 800, color: "#fff",
              letterSpacing: "0.06em",
            }}>NOW</div>
          </div>
        )}

        {/* 별점 (클리어) */}
        {cleared && (
          <div style={{
            position: "absolute", top: 3, right: 3,
            display: "flex", gap: 0.5,
            padding: "2px 4px",
            background: "rgba(0,0,0,0.5)",
            borderRadius: 999,
          }}>
            {Array.from({ length: 3 }).map((_, i) => (
              <span key={i} style={{
                fontSize: 7,
                color: i < slot.stars ? "#FFD86A" : "rgba(255,255,255,0.3)",
                lineHeight: 1,
              }}>★</span>
            ))}
          </div>
        )}
      </div>

      {/* 번호 라벨 */}
      <div style={{
        padding: "4px 5px 5px",
        background: current ? P.primary : P.surface,
      }}>
        <div style={{
          fontSize: 10,
          fontWeight: current ? 800 : 700,
          color: current ? "#fff" : (cleared ? P.text : P.textMuted),
          textAlign: "center",
          lineHeight: 1.1,
          letterSpacing: "-0.01em",
        }}>
          {String(slot.level).padStart(2, "0")}
        </div>
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────────────────────────────
// GalleryMuseum — 미술관 액자 진열 (3060 감성)
// ─────────────────────────────────────────────────────────────────────
function GalleryMuseum({ palette: P, theme }) {
  const items = THEMES[theme].items;
  // 더 많은 항목을 시뮬레이션 (50개 라이브러리 가정)
  const collection = [];
  for (let i = 0; i < 18; i++) {
    const it = items[i % items.length];
    collection.push({
      ...it,
      uniqueId: `${it.id}-${i}`,
      cleared: i < 13,
      stars: i < 13 ? (i % 3) + 1 : 0,
      level: i + 1,
    });
  }

  return (
    <div style={{
      position: "absolute", inset: 0, color: P.text,
      fontFamily: '"Pretendard", "Apple SD Gothic Neo", system-ui, sans-serif',
      display: "flex", flexDirection: "column",
      background: `linear-gradient(180deg, ${P.surfaceAlt} 0%, ${P.bg} 50%)`,
    }}>
      {/* 헤더 */}
      <div style={{
        padding: "14px 16px 10px",
        display: "flex", alignItems: "center", gap: 10,
      }}>
        <IconBtn P={P} size={36}><Icon name="back" size={18} color={P.text} /></IconBtn>
        <div style={{ flex: 1 }}>
          <div style={{ fontSize: 11, color: P.textSoft, fontWeight: 500 }}>나의 갤러리</div>
          <div style={{ fontSize: 17, fontWeight: 700, letterSpacing: "-0.02em" }}>그림 수집관</div>
        </div>
        <div style={{
          padding: "6px 12px", borderRadius: 999,
          background: P.surface, border: `1px solid ${P.line}`,
          fontSize: 11, fontWeight: 700, color: P.text,
        }}>
          <b style={{ color: P.primaryDeep }}>13</b> / {collection.length}
        </div>
      </div>

      {/* 진행률 */}
      <div style={{ padding: "0 16px 12px" }}>
        <div style={{
          padding: "10px 14px",
          background: P.surface, borderRadius: 12,
          border: `1px solid ${P.line}`,
          display: "flex", alignItems: "center", gap: 10,
        }}>
          <div style={{ flex: 1 }}>
            <div style={{ fontSize: 11, color: P.textSoft, marginBottom: 4 }}>다음 작품까지</div>
            <div style={{ position: "relative", height: 6, background: P.line, borderRadius: 3, overflow: "hidden" }}>
              <div style={{
                width: "72%", height: "100%",
                background: `linear-gradient(90deg, ${P.primary}, ${P.gold})`, borderRadius: 3,
              }} />
            </div>
          </div>
          <div style={{ fontSize: 11, fontWeight: 700, color: P.primaryDeep }}>
            레벨 14 진행중
          </div>
        </div>
      </div>

      {/* 미술관 벽 — 액자 진열 */}
      <div style={{
        flex: 1, overflow: "auto",
        padding: "8px 16px 16px",
      }}>
        {/* 상단 벽지 효과 */}
        <div style={{
          padding: "16px 12px 20px",
          background: P.surface,
          borderRadius: 16,
          border: `1px solid ${P.line}`,
          backgroundImage: `repeating-linear-gradient(0deg, transparent 0 14px, ${P.line}40 14px 15px)`,
        }}>
          <div style={{ fontSize: 11, color: P.textSoft, fontWeight: 600, letterSpacing: "0.06em", textAlign: "center", marginBottom: 14 }}>
            ━━━ FIRST FLOOR ━━━
          </div>

          {/* 액자 그리드 */}
          <div style={{
            display: "grid", gridTemplateColumns: "1fr 1fr 1fr",
            gap: 14,
          }}>
            {collection.slice(0, 9).map((it, i) => (
              <FrameCard key={it.uniqueId} P={P} item={it} small={i % 4 === 1} />
            ))}
          </div>
        </div>

        {/* 두번째 벽 */}
        <div style={{
          marginTop: 12,
          padding: "16px 12px 20px",
          background: P.surface,
          borderRadius: 16,
          border: `1px solid ${P.line}`,
          backgroundImage: `repeating-linear-gradient(0deg, transparent 0 14px, ${P.line}40 14px 15px)`,
        }}>
          <div style={{ fontSize: 11, color: P.textSoft, fontWeight: 600, letterSpacing: "0.06em", textAlign: "center", marginBottom: 14 }}>
            ━━━ SECOND FLOOR ━━━
          </div>
          <div style={{
            display: "grid", gridTemplateColumns: "1fr 1fr 1fr",
            gap: 14,
          }}>
            {collection.slice(9, 18).map((it, i) => (
              <FrameCard key={it.uniqueId} P={P} item={it} small={i % 4 === 2} />
            ))}
          </div>
        </div>
      </div>

      <BottomNav P={P} active="collection" />
    </div>
  );
}

function FrameCard({ P, item, small }) {
  const cleared = item.cleared;
  return (
    <div style={{
      display: "flex", flexDirection: "column", alignItems: "center", gap: 4,
    }}>
      {/* 액자 */}
      <div style={{
        width: "100%",
        aspectRatio: small ? "0.85" : "1",
        background: cleared
          ? `linear-gradient(145deg, #E8C99A 0%, #B98552 50%, #E8C99A 100%)`
          : P.line,
        padding: small ? 5 : 6,
        borderRadius: 4,
        boxShadow: cleared
          ? "0 6px 14px rgba(60,40,20,0.22), 0 0 0 1px rgba(60,40,20,0.1)"
          : "0 1px 3px rgba(0,0,0,0.05)",
        position: "relative",
      }}>
        <div style={{
          width: "100%", height: "100%",
          background: cleared ? "#FFFEFA" : P.surfaceAlt,
          padding: 3, borderRadius: 1,
          overflow: "hidden",
          position: "relative",
        }}>
          <div style={{
            width: "100%", height: "100%",
            position: "relative",
            opacity: cleared ? 1 : 0.25,
            filter: cleared ? "none" : "grayscale(0.9)",
          }}>
            <PuzzleImage item={item} size={100} showGrid={false} />
          </div>
          {!cleared && (
            <div style={{
              position: "absolute", inset: 0, display: "flex", alignItems: "center", justifyContent: "center",
            }}>
              <Icon name="lock" size={14} color={P.textMuted} />
            </div>
          )}
        </div>
        {/* 별점 (클리어한 작품) */}
        {cleared && item.stars > 0 && (
          <div style={{
            position: "absolute", top: -6, right: -4,
            background: P.gold, color: "#fff",
            padding: "2px 5px", borderRadius: 6,
            fontSize: 8, fontWeight: 800,
            display: "flex", gap: 1, alignItems: "center",
            boxShadow: "0 2px 4px rgba(0,0,0,0.2)",
          }}>
            {"★".repeat(item.stars)}
          </div>
        )}
      </div>
      {/* 캡션 */}
      <div style={{
        fontSize: 9, color: cleared ? P.text : P.textMuted,
        fontWeight: cleared ? 600 : 500,
        textAlign: "center", lineHeight: 1.2,
        maxWidth: "100%", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap",
        width: "100%",
      }}>
        {cleared ? item.label : `LV ${item.level}`}
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────────────────────────────
// GalleryCodex — 도감 그리드 (다시 풀기 CTA = 광고 노출 추가)
// ─────────────────────────────────────────────────────────────────────
function GalleryCodex({ palette: P, theme }) {
  const items = THEMES[theme].items;
  const collection = [];
  for (let i = 0; i < 24; i++) {
    const it = items[i % items.length];
    collection.push({
      ...it,
      uniqueId: `${it.id}-${i}`,
      cleared: i < 13,
      stars: i < 13 ? ((i + 1) % 3) + 1 : 0,
      replays: i < 13 ? Math.floor(Math.random() * 4) : 0,
      level: i + 1,
    });
  }

  const categories = [
    { k: "all", l: "전체", n: collection.length, a: true },
    { k: "cleared", l: "완성", n: 13 },
    { k: "locked", l: "잠금", n: 11 },
    { k: "perfect", l: "★3", n: 4 },
  ];

  return (
    <div style={{
      position: "absolute", inset: 0, color: P.text,
      fontFamily: '"Pretendard", "Apple SD Gothic Neo", system-ui, sans-serif',
      display: "flex", flexDirection: "column",
      background: P.bg,
    }}>
      {/* 헤더 */}
      <div style={{
        padding: "14px 16px 10px",
        background: P.surface, borderBottom: `1px solid ${P.line}`,
      }}>
        <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 10 }}>
          <IconBtn P={P} size={36}><Icon name="back" size={18} color={P.text} /></IconBtn>
          <div style={{ flex: 1 }}>
            <div style={{ fontSize: 11, color: P.textSoft, fontWeight: 500 }}>나의 갤러리</div>
            <div style={{ fontSize: 17, fontWeight: 700, letterSpacing: "-0.02em" }}>그림 도감</div>
          </div>
          <IconBtn P={P} size={36}><Icon name="swap" size={16} color={P.textSoft} /></IconBtn>
        </div>

        {/* 통계 */}
        <div style={{ display: "flex", gap: 6 }}>
          {[
            { l: "수집", v: "13/50" },
            { l: "★3 완벽", v: "4" },
            { l: "총 클리어", v: "21" },
          ].map((s) => (
            <div key={s.l} style={{
              flex: 1, padding: "8px 4px", textAlign: "center",
              background: P.surfaceAlt, borderRadius: 10,
            }}>
              <div style={{ fontSize: 10, color: P.textSoft, fontWeight: 500 }}>{s.l}</div>
              <div style={{ fontSize: 14, fontWeight: 800, color: P.text, marginTop: 1 }}>{s.v}</div>
            </div>
          ))}
        </div>
      </div>

      {/* 필터 칩 */}
      <div style={{
        padding: "10px 16px 8px", background: P.surface,
        display: "flex", gap: 6, overflowX: "auto",
      }}>
        {categories.map((c) => (
          <div key={c.k} style={{
            padding: "6px 12px", borderRadius: 999,
            background: c.a ? P.text : P.surfaceAlt,
            color: c.a ? P.surface : P.textSoft,
            fontSize: 12, fontWeight: 700,
            whiteSpace: "nowrap",
          }}>
            {c.l} <span style={{ opacity: 0.7, fontWeight: 500 }}>{c.n}</span>
          </div>
        ))}
      </div>

      {/* 그리드 */}
      <div style={{
        flex: 1, overflow: "auto",
        padding: "12px 16px 16px",
        display: "grid", gridTemplateColumns: "1fr 1fr 1fr",
        gap: 10,
      }}>
        {collection.map((it) => (
          <CodexCard key={it.uniqueId} P={P} item={it} />
        ))}
      </div>

      {/* 하단: 다시 풀기 권유 (광고 노출 추가 기회) */}
      <div style={{
        padding: "10px 14px", background: P.surface,
        borderTop: `1px solid ${P.line}`,
        display: "flex", alignItems: "center", gap: 10,
      }}>
        <div style={{
          width: 36, height: 36, borderRadius: 10,
          background: P.accentSoft, color: P.primaryDeep,
          display: "flex", alignItems: "center", justifyContent: "center",
          fontSize: 18,
        }}>♻️</div>
        <div style={{ flex: 1, minWidth: 0 }}>
          <div style={{ fontSize: 12, fontWeight: 700, lineHeight: 1.2 }}>다시 풀어보기</div>
          <div style={{ fontSize: 10, color: P.textSoft, marginTop: 1 }}>난이도 ↑ 도전하면 ★3 완벽 가능</div>
        </div>
        <button style={{
          padding: "8px 14px", border: "none", borderRadius: 999,
          background: P.text, color: P.surface,
          fontSize: 11, fontWeight: 700, fontFamily: "inherit", cursor: "pointer",
        }}>골라보기</button>
      </div>
    </div>
  );
}

function CodexCard({ P, item }) {
  const cleared = item.cleared;
  return (
    <div style={{
      position: "relative",
      borderRadius: 10, overflow: "hidden",
      background: cleared ? P.surface : P.surfaceAlt,
      border: `1px solid ${cleared ? P.line : P.line}`,
      boxShadow: cleared ? "0 1px 3px rgba(0,0,0,0.04)" : "none",
    }}>
      {/* 이미지 */}
      <div style={{
        width: "100%", aspectRatio: "1",
        position: "relative",
        opacity: cleared ? 1 : 1,
      }}>
        {cleared ? (
          <PuzzleImage item={item} size={120} showGrid={false} />
        ) : (
          <div style={{
            width: "100%", height: "100%",
            background: P.surfaceAlt,
            display: "flex", alignItems: "center", justifyContent: "center",
            position: "relative",
          }}>
            <div style={{
              position: "absolute", inset: 0,
              backgroundImage: `repeating-linear-gradient(45deg, ${P.line} 0 1px, transparent 1px 8px)`,
              opacity: 0.5,
            }} />
            <Icon name="lock" size={20} color={P.textMuted} />
          </div>
        )}

        {/* 별점 오버레이 */}
        {cleared && (
          <div style={{
            position: "absolute", top: 4, right: 4,
            display: "flex", gap: 1,
            padding: "3px 5px",
            background: "rgba(0,0,0,0.55)",
            borderRadius: 999,
          }}>
            {Array.from({ length: 3 }).map((_, i) => (
              <span key={i} style={{
                fontSize: 8,
                color: i < item.stars ? "#FFD86A" : "rgba(255,255,255,0.3)",
                lineHeight: 1,
              }}>★</span>
            ))}
          </div>
        )}

        {/* 다시 푼 횟수 */}
        {cleared && item.replays > 0 && (
          <div style={{
            position: "absolute", bottom: 4, left: 4,
            padding: "2px 6px", borderRadius: 6,
            background: "rgba(0,0,0,0.55)", color: "#fff",
            fontSize: 9, fontWeight: 700,
          }}>
            ×{item.replays + 1}
          </div>
        )}
      </div>

      {/* 캡션 */}
      <div style={{ padding: "5px 6px 6px" }}>
        <div style={{
          fontSize: 10, fontWeight: 700,
          color: cleared ? P.text : P.textMuted,
          overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap",
          lineHeight: 1.2,
        }}>
          {cleared ? item.label : `LV ${item.level}`}
        </div>
        <div style={{
          fontSize: 9, color: P.textMuted, marginTop: 1,
          fontWeight: 500,
        }}>
          {cleared ? "완성" : "잠금"}
        </div>
      </div>
    </div>
  );
}

// 전역으로 노출
Object.assign(window, { HomeSequential, GalleryMuseum, GalleryCodex });
