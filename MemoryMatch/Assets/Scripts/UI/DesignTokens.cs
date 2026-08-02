using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace MemoryMatch.UI
{
    /// <summary>
    /// 성인 피벗 v0.4 디자인 시스템 — 색상/타이포/간격/폰트 토큰 + 공용 UI 빌더.
    ///
    /// 모든 화면 빌더 (LevelSelect / Complete / GameplayHeader / Popup 등) 는
    /// 이 토큰을 통해 색·폰트·치수를 가져와 일관성 유지.
    ///
    /// 메모리 spec: design_system_v04.md (사용자 합의 2026-04-25).
    /// 톤: 대중적, 어둡지 않음, 따뜻+깔끔, 30-60 타겟. 카카오페이 풍.
    /// </summary>
    public static class DesignTokens
    {
        // ─────────────────────────────────────────────────────
        // Color Tokens
        // ─────────────────────────────────────────────────────

        public static readonly Color BgPage         = Hex("#FAF7F2"); // 따뜻한 크림
        public static readonly Color BgGradTop      = Hex("#E8F4FE"); // 옅은 하늘
        public static readonly Color BgGradBottom   = Hex("#FAF7F2");
        public static readonly Color SurfaceCard    = Hex("#FFFFFF"); // 카드/팝업 배경
        public static readonly Color BoardSurface   = Hex("#D9D9D9"); // PuzzleBoard 배경 (옅은 회색 — 페이지 BG 와 구분)

        public static readonly Color CtaPrimary     = Hex("#FFC940"); // 메인 CTA 노랑
        public static readonly Color CtaPrimaryDark = Hex("#E5B520"); // 노랑 hover/press
        public static readonly Color CtaPlay        = Hex("#4FC074"); // Play 초록
        public static readonly Color CtaDisabled    = Hex("#D1D5DB"); // 잠금 회색

        public static readonly Color TextPrimary    = Hex("#1F2937"); // 본문
        public static readonly Color TextSecondary  = Hex("#6B7280"); // 부제/메타
        public static readonly Color TextOnCta      = Hex("#1F2937"); // 노랑 CTA 위 텍스트
        public static readonly Color TextOnDark     = Hex("#FFFFFF"); // 어두운 배경 위 텍스트

        public static readonly Color CelebrateLight = Hex("#FFE066"); // Complete sunburst 밝은 끝
        public static readonly Color CelebrateDeep  = Hex("#FFC940"); // Complete sunburst 깊은 끝

        public static readonly Color OverlayLock    = new Color(0f, 0f, 0f, 0.55f); // 잠긴 레벨 카드

        // ─────────────────────────────────────────────────────
        // Typography
        // ─────────────────────────────────────────────────────

        public const int FontDisplay = 64;
        public const int FontHeading = 40;
        public const int FontBodyLg  = 28;
        public const int FontBody    = 22;
        public const int FontCaption = 18;

        // 폰트 lazy load — Resources/Fonts/NotoSansKR-{Regular,Bold}.ttf, 누락 시 LegacyRuntime 폴백
        private static Font _regular;
        private static Font _bold;
        public static Font Regular
        {
            get
            {
                if (_regular != null) return _regular;
                _regular = Resources.Load<Font>("Fonts/NotoSansKR-Regular");
                if (_regular == null)
                {
                    Debug.LogWarning("[DesignTokens] NotoSansKR-Regular 못 찾음 — LegacyRuntime 폴백. " +
                                     "Assets/Resources/Fonts/ 에 폰트 추가하세요.");
                    _regular = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                return _regular;
            }
        }
        public static Font Bold
        {
            get
            {
                if (_bold != null) return _bold;
                _bold = Resources.Load<Font>("Fonts/NotoSansKR-Bold");
                if (_bold == null) _bold = Regular; // Bold 없으면 Regular + FontStyle.Bold 로 대체
                return _bold;
            }
        }

        // Material Icons (Google) — 단일 ttf 로 1700+ 아이콘 (Unicode codepoint 으로 접근)
        private static Font _iconFont;
        public static Font IconFont
        {
            get
            {
                if (_iconFont != null) return _iconFont;
                _iconFont = Resources.Load<Font>("Fonts/MaterialIcons-Regular");
                if (_iconFont == null)
                {
                    Debug.LogWarning("[DesignTokens] MaterialIcons-Regular 못 찾음 — Regular 폴백 (아이콘 깨짐)");
                    _iconFont = Regular;
                }
                return _iconFont;
            }
        }

        /// <summary>아이콘 이름 → Material Icons Unicode codepoint.
        /// 새 아이콘 필요 시 https://fonts.google.com/icons 에서 codepoint 확인 후 추가.</summary>
        public static string Icon(string name)
        {
            // Material Icons 코드포인트 (PUA 영역) — char 캐스팅으로 명시적 hex 지정
            switch (name)
            {
                case "lock":        return ((char)0xE897).ToString();
                case "settings":    return ((char)0xE8B8).ToString();
                case "play_arrow":  return ((char)0xE037).ToString();
                case "check":       return ((char)0xE5CA).ToString();
                case "home":        return ((char)0xE88A).ToString();
                case "arrow_back":  return ((char)0xE5C4).ToString();
                case "refresh":     return ((char)0xE5D5).ToString();
                case "undo":        return ((char)0xE166).ToString();
                case "lightbulb":   return ((char)0xE0F0).ToString();
                case "visibility":  return ((char)0xE8F4).ToString();
                case "close":       return ((char)0xE5CD).ToString();
                case "star":        return ((char)0xE838).ToString();
                // v0.6 (Pastel Puzzle) 추가 — HomeSequential / BottomNav / 카드 CTA
                case "calendar":      return ((char)0xE935).ToString(); // BottomNav 일일
                case "image":         return ((char)0xE3F4).ToString(); // BottomNav 갤러리
                case "gift":          return ((char)0xE8F6).ToString(); // BottomNav 상점
                case "chevron_right": return ((char)0xE5CC).ToString(); // 계속하기 CTA
                case "coin":          return ((char)0xE263).ToString(); // CoinPill (monetization_on)
                // Gameplay (PlayClassic) 추가
                case "pause":         return ((char)0xE034).ToString(); // 일시정지
                case "sparkle":       return ((char)0xE838).ToString(); // (불사용) Counter sparkle
                case "puzzle":        return ((char)0xE87B).ToString(); // Counter 진행 표시 (extension)
                // SettingsPopup 슬라이더
                case "volume_down":   return ((char)0xE04D).ToString();
                case "volume_up":     return ((char)0xE050).ToString();
                case "music_note":    return ((char)0xE405).ToString();
                case "music_note_off":return ((char)0xE440).ToString();
                default:
                    Debug.LogWarning($"[DesignTokens] Unknown icon: {name}");
                    return "?";
            }
        }

        /// <summary>Material Icons 아이콘을 표시하는 Text 컴포넌트 생성.</summary>
        public static Text BuildIcon(Transform parent, string goName, string iconKey,
                                     int size, Color color,
                                     TextAnchor align = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(goName,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, worldPositionStays: false);

            var txt = go.GetComponent<Text>();
            txt.text          = Icon(iconKey);
            txt.alignment     = align;
            txt.color         = color;
            txt.fontSize      = size;
            txt.font          = IconFont;
            txt.raycastTarget = false;

            return txt;
        }

        // ─────────────────────────────────────────────────────
        // Spacing & Sizing
        // ─────────────────────────────────────────────────────

        public const float Space1 = 4f;
        public const float Space2 = 8f;
        public const float Space3 = 16f;
        public const float Space4 = 24f;
        public const float Space5 = 40f;
        public const float Space6 = 64f;

        public const float CardRadius   = 24f;
        public const float ButtonHeight = 80f;
        public const float PlayDiameter = 120f;

        // ─────────────────────────────────────────────────────
        // 공용 UI 빌더 (모든 컨트롤러가 호출)
        // ─────────────────────────────────────────────────────

        /// <summary>표준 Text 컴포넌트 — 폰트 자동 적용 + raycastTarget=false.</summary>
        public static Text BuildText(Transform parent, string name, string content,
                                     int fontSize, FontStyle style, Color color,
                                     TextAnchor align = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, worldPositionStays: false);

            var txt = go.GetComponent<Text>();
            txt.text          = content;
            txt.alignment     = align;
            txt.color         = color;
            txt.fontSize      = fontSize;
            txt.fontStyle     = style;
            txt.font          = (style == FontStyle.Bold) ? Bold : Regular;
            txt.raycastTarget = false;

            return txt;
        }

        /// <summary>
        /// Localize 가능한 텍스트 생성 — UI StringTable 의 entryKey 를 바인딩.
        /// Locale 변경 시 자동 갱신 (LocalizedTextBinder 부착).
        /// fallback 한국어 원문은 BuildText 와 동일한 시각 (에디트 모드 / Locale 미로드 시).
        /// 사용자 노출 텍스트는 모두 이 헬퍼를 사용 — 새 텍스트 추가 시 LocalizationStringTableTool.UI_ROWS 에도 키/번역 등록 필요.
        /// </summary>
        public static Text BuildLocalizedText(Transform parent, string name, string entryKey,
                                              string fallbackContent,
                                              int fontSize, FontStyle style, Color color,
                                              TextAnchor align = TextAnchor.MiddleCenter)
        {
            var txt = BuildText(parent, name, fallbackContent, fontSize, style, color, align);
            var binder = txt.gameObject.AddComponent<LocalizedTextBinder>();
            binder.EntryKey = entryKey;
            return txt;
        }

        /// <summary>
        /// Format placeholder ({0}, {1}, ...) 를 가진 다국어 텍스트 — LocalizedTextFormatBinder 컴포넌트 부착.
        /// fallbackFormatted 는 에디트 모드/Locale 미로드 시 그대로 표시되는 한국어 (이미 args 적용된 완성 문자열).
        /// args 는 LocalizedString 의 placeholder 에 채워질 값. Unity Localization Smart String 또는 string.Format 형식.
        /// LocalizedTextBinder 가 단순 string 만 지원해서 별도 binder.
        /// </summary>
        public static Text BuildLocalizedTextFormat(Transform parent, string name, string entryKey,
                                                    string fallbackFormatted, object[] args,
                                                    int fontSize, FontStyle style, Color color,
                                                    TextAnchor align = TextAnchor.MiddleCenter)
        {
            var txt = BuildText(parent, name, fallbackFormatted, fontSize, style, color, align);
            var binder = txt.gameObject.AddComponent<LocalizedTextFormatBinder>();
            binder.Arguments = args;
            binder.EntryKey  = entryKey;  // setter 가 Refresh 호출 → _ls 등록
            return txt;
        }

        /// <summary>Primary 캡슐 버튼 (노랑) — KakaoPay 풍.</summary>
        public static Button BuildPrimaryButton(Transform parent, string name, string label,
                                                Vector2 anchoredPos, float width, float height = 80f)
        {
            return BuildPillButton(parent, name, label, anchoredPos,
                size: new Vector2(width, height), fontSize: FontBodyLg,
                bgColor: CtaPrimary, fgColor: TextOnCta);
        }

        /// <summary>Play 원형 버튼 (초록) — 활성 레벨 카드 위에 올리는 용도.</summary>
        public static Button BuildPlayButton(Transform parent, string name,
                                             Vector2 anchoredPos, float diameter = 120f)
        {
            var btn = BuildPillButton(parent, name, "Play", anchoredPos,
                size: new Vector2(diameter, diameter), fontSize: FontBodyLg,
                bgColor: CtaPlay, fgColor: TextOnDark);
            return btn;
        }

        /// <summary>Ghost 버튼 — 배경 없음, 텍스트만 (작은 보조 액션).</summary>
        public static Button BuildGhostButton(Transform parent, string name, string label,
                                              Vector2 anchoredPos, Vector2 size, int fontSize = 22)
        {
            return BuildPillButton(parent, name, label, anchoredPos,
                size: size, fontSize: fontSize,
                bgColor: new Color(0, 0, 0, 0), fgColor: TextPrimary);
        }

        // ── 내부 공용 빌더 ──

        private static Button BuildPillButton(Transform parent, string name, string label,
                                              Vector2 anchoredPos, Vector2 size, int fontSize,
                                              Color bgColor, Color fgColor)
        {
            var go = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, worldPositionStays: false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            var img = go.GetComponent<Image>();
            img.color = bgColor;
            // 캡슐은 Image source sprite 가 없어서 사각형 — 모서리 둥글게는 sprite 9-slice 가 필요.
            // 간단히는 sprite 자리만 비우고 색상만. 더 깔끔한 pill 은 후속 sprite 로.

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;

            var lbl = BuildText(go.transform, "Label", label,
                fontSize, FontStyle.Bold, fgColor, TextAnchor.MiddleCenter);
            var lrt = (RectTransform)lbl.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            return btn;
        }

        // ─────────────────────────────────────────────────────
        // Sprite Cache — 외부 에셋 의존 없이 원형/캡슐 sprite 런타임 생성
        // ─────────────────────────────────────────────────────

        private static Sprite _circleSprite;
        /// <summary>안티앨리어싱된 흰색 원 sprite (64×64). Image.sprite 에 할당하면 Image 가 원형으로 보임.</summary>
        public static Sprite CircleSprite
        {
            get
            {
                if (_circleSprite != null) return _circleSprite;
                const int size = 64;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                tex.wrapMode   = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;

                var pixels = new Color[size * size];
                float r  = size * 0.5f - 0.5f;
                float cx = (size - 1) * 0.5f;
                float cy = (size - 1) * 0.5f;
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    float alpha = Mathf.Clamp01(r - d + 1f); // 1px 부드러운 가장자리
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
                tex.SetPixels(pixels);
                tex.Apply();

                _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                                              new Vector2(0.5f, 0.5f), size);
                _circleSprite.name = "DesignTokens.CircleSprite";
                return _circleSprite;
            }
        }

        private static Sprite _sunburstSprite;
        /// <summary>축하용 sunburst 패턴 sprite — 중앙에서 24개 노란 wedge 가 방사형으로 펴짐.
        /// Image.color 로 추가 tint 가능 (기본은 white/yellow alternating).</summary>
        public static Sprite SunburstSprite
        {
            get
            {
                if (_sunburstSprite != null) return _sunburstSprite;
                const int size = 512;
                const int rays = 24;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                tex.wrapMode   = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;

                var pixels = new Color[size * size];
                float cx = (size - 1) * 0.5f;
                float cy = (size - 1) * 0.5f;

                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    float angle = Mathf.Atan2(dy, dx);             // -PI..PI
                    float t     = (angle + Mathf.PI) / (Mathf.PI * 2f); // 0..1
                    int wedge   = (int)(t * rays) % rays;
                    pixels[y * size + x] = (wedge % 2 == 0) ? CelebrateLight : CelebrateDeep;
                }
                tex.SetPixels(pixels);
                tex.Apply();

                _sunburstSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                                                new Vector2(0.5f, 0.5f), size);
                _sunburstSprite.name = "DesignTokens.SunburstSprite";
                return _sunburstSprite;
            }
        }

        // ─────────────────────────────────────────────────────
        // 헬퍼
        // ─────────────────────────────────────────────────────

        public static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
            Debug.LogWarning($"[DesignTokens] Invalid hex: {hex}");
            return Color.magenta;
        }
    }
}
