using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatch.UI
{
    /// <summary>
    /// 디자인 시스템 v0.6 (Pastel Puzzle) — 시안 공통 컴포넌트 Unity uGUI 빌더.
    ///
    /// 시안 출처: document/design_handoff_pastel_puzzle/screens.jsx + more-screens.jsx.
    /// 색·타이포·스페이싱 토큰은 PaletteTokens 참조. 텍스트/아이콘 빌더는 DesignTokens.BuildText/BuildIcon 재사용.
    ///
    /// 모든 빌더는 idempotent — 같은 parent 에 같은 name 으로 재호출 시 기존 자식 제거 후 재생성.
    /// 단, 호출 측에서 GameObject 생성/제거를 직접 관리하면 그쪽이 우선 (빌더는 단순히 새 GO 만들고 부착).
    /// </summary>
    public static class PaletteUI
    {
        // ─────────────────────────────────────────────────────
        // Sprite 캐시 — RoundedRect, Hatch, Gradient
        // ─────────────────────────────────────────────────────

        private static readonly Dictionary<int, Sprite> _roundedCache = new();

        /// <summary>
        /// 반경 r (px) 의 9-slice 가능 RoundedRect sprite. 흰색 fill, 가장자리 1px 안티앨리어싱.
        /// Image.type = Sliced + sprite.border = (r,r,r,r) 로 자동 설정됨.
        /// 사용 측: image.sprite = PaletteUI.RoundedRect(8); image.type = Image.Type.Sliced; image.color = bg;
        /// </summary>
        public static Sprite RoundedRect(int radius)
        {
            // 안전 캡: radius 999 (pill) 같이 큰 값은 9-slice border 가 sprite 크기보다 커지면 깨짐.
            // 호출 측은 height/2 로 클램프해서 호출하는 게 정석. 여기선 안전망으로 64 까지만.
            radius = Mathf.Clamp(radius, 1, 64);
            if (_roundedCache.TryGetValue(radius, out var s)) return s;

            int size = radius * 2 + 2; // 중앙 1×1 + 양옆 radius + 1px slack
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                // 가장 가까운 모서리까지 거리 계산
                float cx = x < radius ? radius : (x > size - radius - 1 ? size - radius - 1 : x);
                float cy = y < radius ? radius : (y > size - radius - 1 ? size - radius - 1 : y);
                float dx = x - cx, dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
            tex.SetPixels(pixels);
            tex.Apply();

            // pixelsPerUnit = 100 (Canvas referencePixelsPerUnit 기본값과 일치 — 9-slice border 가 1:1 px 로 그려짐).
            // ppu 가 1 이면 100배 확대된 border 가 그려져 element 가 작을 때 모서리가 전체를 덮음 (타원처럼 보임).
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                                       new Vector2(0.5f, 0.5f), 100f, 0,
                                       SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            sprite.name = $"RoundedRect_{radius}";
            _roundedCache[radius] = sprite;
            return sprite;
        }

        private static Sprite _hatchSprite;
        /// <summary>잠금 칸의 45도 대각선 hatch 패턴 (시안: repeating-linear-gradient(45deg, line 0 1px, transparent 1px 6px)).
        /// 색은 Image.color 로 tint — sprite 자체는 alpha mask 흰색.</summary>
        public static Sprite HatchPattern()
        {
            if (_hatchSprite != null) return _hatchSprite;
            const int size = 24; // 6 * 4 (시안 6px stripe 단위 × scale 3 ≈ 18 — 근사 24)
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                // 45도 대각선 — (x+y) % stripeWidth < lineWidth
                int diagonal = (x + y) % 8; // stripe = 8px (line 2px + gap 6px)
                pixels[y * size + x] = (diagonal < 2)
                    ? new Color(1f, 1f, 1f, 0.6f)
                    : new Color(0f, 0f, 0f, 0f);
            }
            tex.SetPixels(pixels);
            tex.Apply();
            _hatchSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                                         new Vector2(0.5f, 0.5f), 1f);
            _hatchSprite.name = "HatchPattern";
            return _hatchSprite;
        }

        /// <summary>가로 그라디언트 sprite (좌→우 colorA→colorB). progress bar fill 용.</summary>
        public static Sprite GradientHorizontal(Color a, Color b)
        {
            const int w = 64, h = 4;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color[w * h];
            for (int x = 0; x < w; x++)
            {
                float t = (float)x / (w - 1);
                Color c = Color.Lerp(a, b, t);
                for (int y = 0; y < h; y++) pixels[y * w + x] = c;
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 1f);
        }

        private static Sprite _goldFrameSprite;
        /// <summary>
        /// 골드 액자 diagonal gradient sprite (시안: 145deg #E8C99A → #B98552 → #E8C99A).
        /// 좌상단 옅은 골드 → 가운데 진한 골드 → 우하단 옅은 골드. 캐시 1개.
        /// </summary>
        public static Sprite GoldFrameGradient()
        {
            if (_goldFrameSprite != null) return _goldFrameSprite;
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            Color light = DesignTokens.Hex("#E8C99A");
            Color dark  = DesignTokens.Hex("#B98552");
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                // 145도 = 좌하단(0,0) → 우상단(size,size) 방향
                float t = (x + y) / (2f * (size - 1));
                // light 우세 + 가운데 좁은 dark stripe (시안 톤: 전체적으로 황색, 가운데만 살짝 어두움)
                // SmoothStep 으로 가운데 0.35~0.65 만 dark 비율 ↑
                float distFromMid = Mathf.Abs(t - 0.5f) * 2f; // 0(가운데)..1(끝)
                float k = 1f - Mathf.SmoothStep(0.2f, 0.7f, distFromMid); // 가운데 k=1 (dark), 끝 k=0 (light)
                pixels[y * size + x] = Color.Lerp(light, dark, k);
            }
            tex.SetPixels(pixels);
            tex.Apply();
            _goldFrameSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                                             new Vector2(0.5f, 0.5f), 100f);
            _goldFrameSprite.name = "GoldFrameGradient";
            return _goldFrameSprite;
        }

        /// <summary>세로 그라디언트 sprite (상→하 colorA→colorB). current 타일 하단 오버레이 용.</summary>
        public static Sprite GradientVertical(Color a, Color b)
        {
            const int w = 4, h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                // 시안은 transparent 50% → primary90 — 위 50% 는 0 alpha, 아래 50% 는 점진 alpha
                float t = (float)y / (h - 1); // 0 (bottom) → 1 (top)
                Color c = Color.Lerp(a, b, t);
                for (int x = 0; x < w; x++) pixels[y * w + x] = c;
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 1f);
        }

        // ─────────────────────────────────────────────────────
        // Surface 빌더 — 모든 카드/모달의 공통 base
        // ─────────────────────────────────────────────────────

        /// <summary>
        /// 둥근 모서리 + 색상 surface 생성. borderColor/borderWidth 인자는 시그니처 호환용 — 현재 NoOp.
        /// 정확한 1px frame 효과는 후속 9-slice border sprite 작업에서 추가 (시각 우선순위 낮음).
        /// 시각 강조가 필요한 경우 Shadow 컴포넌트 또는 사이즈 차이로 표현 (SequentialTile current state 등).
        /// </summary>
        public static GameObject BuildSurface(Transform parent, string name, Color bgColor,
                                              int radius, Color borderColor = default, float borderWidth = 0f)
        {
            var go = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, worldPositionStays: false);

            var img = go.GetComponent<Image>();
            img.sprite = RoundedRect(radius);
            img.type   = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1f;
            img.color  = bgColor;

            return go;
        }

        // ─────────────────────────────────────────────────────
        // IconBtn — 시안: 36×36 surfaceAlt 둥근 사각형 + 아이콘 중앙
        // ─────────────────────────────────────────────────────

        public static Button BuildIconBtn(Transform parent, string name, string iconKey,
                                          float size, bool highlight, int? badge = null)
        {
            var go = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, worldPositionStays: false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(size, size);

            int radius = Mathf.RoundToInt(size / 2.4f);
            var img = go.GetComponent<Image>();
            img.sprite = RoundedRect(radius);
            img.type   = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1f;
            img.color  = highlight ? PaletteTokens.Active.Primary : PaletteTokens.Active.SurfaceAlt;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;

            int iconSize = Mathf.RoundToInt(size * 0.5f);
            Color iconColor = highlight ? Color.white : PaletteTokens.Active.TextSoft;
            var icon = DesignTokens.BuildIcon(go.transform, "Icon", iconKey, iconSize, iconColor);
            var lrt = (RectTransform)icon.transform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;

            if (badge.HasValue)
            {
                var bdg = BuildSurface(go.transform, "Badge", PaletteTokens.Active.Gold, 24);
                var brt = (RectTransform)bdg.transform;
                brt.anchorMin = new Vector2(1f, 1f);
                brt.anchorMax = new Vector2(1f, 1f);
                brt.pivot     = new Vector2(1f, 1f);
                brt.sizeDelta = new Vector2(48f, 48f);
                brt.anchoredPosition = new Vector2(9f, 9f);
                var bTxt = DesignTokens.BuildText(bdg.transform, "Num", badge.Value.ToString(),
                    PaletteTokens.FontLabel, FontStyle.Bold, Color.white);
                var btrt = (RectTransform)bTxt.transform;
                btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one;
                btrt.offsetMin = Vector2.zero; btrt.offsetMax = Vector2.zero;
            }

            return btn;
        }

        // ─────────────────────────────────────────────────────
        // CoinPill — surface bg + gold 원형 + 값. 명시적 sizeDelta.
        // 시안: height ~26px → 78px. 원 20px → 60px. padding 좌6/우10 → 18/30. gap 4 → 12.
        // ─────────────────────────────────────────────────────

        public const float CoinPillHeight = 78f;       // 26 × 3
        public const float CoinPillCoinSize = 60f;     // 20 × 3
        public const float CoinPillPadLeft = 18f;      // 6 × 3
        public const float CoinPillPadRight = 30f;     // 10 × 3
        public const float CoinPillGap = 12f;          // 4 × 3

        /// <summary>CoinPill 의 자동 측정 width (값 텍스트 length 기반).</summary>
        public static float MeasureCoinPillWidth(string value)
        {
            // 값 텍스트의 preferredWidth — 임시 Text 로 측정
            int charCount = value?.Length ?? 1;
            float perChar = PaletteTokens.FontCaption * 0.6f; // 대략적 평균 — Pretendard 기준
            float valueWidth = perChar * charCount + 6f;
            return CoinPillPadLeft + CoinPillCoinSize + CoinPillGap + valueWidth + CoinPillPadRight;
        }

        public static GameObject BuildCoinPill(Transform parent, string name, string value)
        {
            float width  = MeasureCoinPillWidth(value);
            float height = CoinPillHeight;
            int   radius = Mathf.Min(39, (int)(height / 2f)); // pill — height/2 클램프

            var pill = BuildSurface(parent, name, PaletteTokens.Active.Surface, radius);
            var rt = (RectTransform)pill.transform;
            rt.sizeDelta = new Vector2(width, height);

            // 골드 원 — anchor left middle
            var coinGo = new GameObject("Coin",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            coinGo.transform.SetParent(pill.transform, worldPositionStays: false);
            var coinRt = (RectTransform)coinGo.transform;
            coinRt.anchorMin = new Vector2(0f, 0.5f);
            coinRt.anchorMax = new Vector2(0f, 0.5f);
            coinRt.pivot     = new Vector2(0f, 0.5f);
            coinRt.sizeDelta = new Vector2(CoinPillCoinSize, CoinPillCoinSize);
            coinRt.anchoredPosition = new Vector2(CoinPillPadLeft, 0f);
            var coinImg = coinGo.GetComponent<Image>();
            coinImg.sprite = DesignTokens.CircleSprite;
            coinImg.color  = PaletteTokens.Active.Gold;

            var sym = DesignTokens.BuildText(coinGo.transform, "Sym", "₩",
                PaletteTokens.FontCaption, FontStyle.Bold, Color.white);
            var srt = (RectTransform)sym.transform;
            srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
            srt.offsetMin = Vector2.zero; srt.offsetMax = Vector2.zero;

            // 값 텍스트 — anchor stretch from coin right
            var valTxt = DesignTokens.BuildText(pill.transform, "Value", value,
                PaletteTokens.FontCaption, FontStyle.Bold, PaletteTokens.Active.Text,
                TextAnchor.MiddleLeft);
            var valRt = (RectTransform)valTxt.transform;
            valRt.anchorMin = new Vector2(0f, 0f);
            valRt.anchorMax = new Vector2(1f, 1f);
            valRt.offsetMin = new Vector2(CoinPillPadLeft + CoinPillCoinSize + CoinPillGap, 0f);
            valRt.offsetMax = new Vector2(-CoinPillPadRight, 0f);

            return pill;
        }

        // ─────────────────────────────────────────────────────
        // SectionLabel — 좌측 라벨 + 가운데 디바이더 line + 우측 보조 텍스트
        // 명시적 anchor 배치. 호출 측에서 sizeDelta 명시.
        // ─────────────────────────────────────────────────────

        public static GameObject BuildSectionLabel(Transform parent, string name,
                                                    string leftText, string rightText,
                                                    Vector2 size,
                                                    string leftEntryKey = null,
                                                    string rightEntryKey = null)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, worldPositionStays: false);
            var rt = (RectTransform)row.transform;
            rt.sizeDelta = size;

            const float gap = 24f;

            // 좌측 라벨 — preferredWidth 측정 후 sizeDelta set
            var left = string.IsNullOrEmpty(leftEntryKey)
                ? DesignTokens.BuildText(row.transform, "Left", leftText,
                    PaletteTokens.FontCaption, FontStyle.Bold, PaletteTokens.Active.TextSoft,
                    TextAnchor.MiddleLeft)
                : DesignTokens.BuildLocalizedText(row.transform, "Left", leftEntryKey, leftText,
                    PaletteTokens.FontCaption, FontStyle.Bold, PaletteTokens.Active.TextSoft,
                    TextAnchor.MiddleLeft);
            left.horizontalOverflow = HorizontalWrapMode.Overflow;
            float leftW = left.preferredWidth + 6f;
            var lRt = (RectTransform)left.transform;
            lRt.anchorMin = new Vector2(0f, 0f);
            lRt.anchorMax = new Vector2(0f, 1f);
            lRt.pivot     = new Vector2(0f, 0.5f);
            lRt.sizeDelta = new Vector2(leftW, 0f);
            lRt.anchoredPosition = Vector2.zero;

            // 우측 보조
            var right = string.IsNullOrEmpty(rightEntryKey)
                ? DesignTokens.BuildText(row.transform, "Right", rightText,
                    PaletteTokens.FontCaptionSm, FontStyle.Normal, PaletteTokens.Active.TextMuted,
                    TextAnchor.MiddleRight)
                : DesignTokens.BuildLocalizedText(row.transform, "Right", rightEntryKey, rightText,
                    PaletteTokens.FontCaptionSm, FontStyle.Normal, PaletteTokens.Active.TextMuted,
                    TextAnchor.MiddleRight);
            right.horizontalOverflow = HorizontalWrapMode.Overflow;
            float rightW = right.preferredWidth + 6f;
            var rRt = (RectTransform)right.transform;
            rRt.anchorMin = new Vector2(1f, 0f);
            rRt.anchorMax = new Vector2(1f, 1f);
            rRt.pivot     = new Vector2(1f, 0.5f);
            rRt.sizeDelta = new Vector2(rightW, 0f);
            rRt.anchoredPosition = Vector2.zero;

            // 가운데 디바이더 (1px 라인) — left 끝 ~ right 시작 사이 stretch
            var divGo = new GameObject("Divider",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            divGo.transform.SetParent(row.transform, worldPositionStays: false);
            var dRt = (RectTransform)divGo.transform;
            dRt.anchorMin = new Vector2(0f, 0.5f);
            dRt.anchorMax = new Vector2(1f, 0.5f);
            dRt.pivot     = new Vector2(0.5f, 0.5f);
            dRt.sizeDelta = new Vector2(0f, 3f); // 1 × 3
            dRt.offsetMin = new Vector2(leftW + gap, dRt.offsetMin.y);
            dRt.offsetMax = new Vector2(-(rightW + gap), dRt.offsetMax.y);
            divGo.GetComponent<Image>().color = PaletteTokens.Active.Line;
            divGo.GetComponent<Image>().raycastTarget = false;

            return row;
        }

        // ─────────────────────────────────────────────────────
        // ProgressBar — 시안: height 6px, line bg, primary→gold 그라디언트 fill, 흰 milestone tick
        // ─────────────────────────────────────────────────────

        /// <summary>
        /// width 명시. fill 은 sizeDelta.x 직접 계산 (anchor stretch 안 씀 — RectMask2D 클리핑 의존도 ↓).
        /// </summary>
        public static GameObject BuildProgressBar(Transform parent, string name,
                                                   float ratio, int total, int[] milestones,
                                                   float width)
        {
            const float height = 18f; // 6 × 3
            const int radius = 9;     // 3 × 3

            // 외곽 트랙 — Mask + RoundedRect Image 로 둥근 모서리 클리핑 (fill 좌/우 끝도 둥글게)
            var track = BuildSurface(parent, name, PaletteTokens.Active.Line, radius);
            var trt = (RectTransform)track.transform;
            trt.sizeDelta = new Vector2(width, height);
            // 사각 RectMask2D 대신 둥근 Mask — Image sprite 가 RoundedRect 라 자식이 둥근 영역으로 클리핑됨
            var mask = track.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            float fillWidth = Mathf.Clamp01(ratio) * width;

            // Fill — anchor left middle, width = ratio × width
            if (fillWidth > 0.5f)
            {
                var fillGo = new GameObject("Fill",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                fillGo.transform.SetParent(track.transform, worldPositionStays: false);
                var fRt = (RectTransform)fillGo.transform;
                fRt.anchorMin = new Vector2(0f, 0f);
                fRt.anchorMax = new Vector2(0f, 1f);
                fRt.pivot     = new Vector2(0f, 0.5f);
                fRt.sizeDelta = new Vector2(fillWidth, 0f);
                fRt.anchoredPosition = Vector2.zero;
                var fImg = fillGo.GetComponent<Image>();
                fImg.sprite = GradientHorizontal(PaletteTokens.Active.Primary, PaletteTokens.Active.Gold);
                fImg.color  = Color.white;
                fImg.raycastTarget = false;
            }

            // Milestone ticks — anchor 위치 직접 계산
            if (milestones != null && total > 0)
            {
                foreach (var m in milestones)
                {
                    if (m <= 0 || m >= total) continue;
                    float xPos = ((float)m / total) * width; // 0 ~ width
                    var tick = new GameObject($"Tick_{m}",
                        typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    tick.transform.SetParent(track.transform, worldPositionStays: false);
                    var tRt = (RectTransform)tick.transform;
                    tRt.anchorMin = new Vector2(0f, 0.5f);
                    tRt.anchorMax = new Vector2(0f, 0.5f);
                    tRt.pivot     = new Vector2(0.5f, 0.5f);
                    tRt.sizeDelta = new Vector2(6f, height); // 2 × 3 폭
                    tRt.anchoredPosition = new Vector2(xPos, 0f);
                    tick.GetComponent<Image>().color = PaletteTokens.Active.Surface;
                    tick.GetComponent<Image>().raycastTarget = false;
                }
            }

            return track;
        }

        // ─────────────────────────────────────────────────────
        // BannerAd Placeholder — 영역만 잡아둠. 실제 광고는 SDK 가 이 자리에 그림.
        // 회색 박스 + 가운데 작은 "광고 영역" 라벨 (개발 중 시각 확인용).
        // ─────────────────────────────────────────────────────

        public const float BannerAdHeight = 168f; // 56 × 3

        public static GameObject BuildBannerAdPlaceholder(Transform parent, string name)
        {
            var banner = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            banner.transform.SetParent(parent, worldPositionStays: false);
            banner.GetComponent<Image>().color = DesignTokens.Hex("#EAE5DC");

            var lbl = DesignTokens.BuildText(banner.transform, "Lbl",
                "광고 영역",
                PaletteTokens.FontCaptionSm, FontStyle.Normal, DesignTokens.Hex("#A89F92"),
                TextAnchor.MiddleCenter);
            var lRt = (RectTransform)lbl.transform;
            lRt.anchorMin = Vector2.zero; lRt.anchorMax = Vector2.one;
            lRt.offsetMin = Vector2.zero; lRt.offsetMax = Vector2.zero;

            return banner;
        }

        // ─────────────────────────────────────────────────────
        // BottomNav — 4탭 (home / daily / collection / shop)
        // 활성 탭은 primary 색, 비활성은 textMuted
        // ─────────────────────────────────────────────────────

        public struct NavItem
        {
            public string Key;
            public string Label;     // fallback 한국어 (Locale 미로드 / 에디트 모드)
            public string IconKey;
            public string EntryKey;  // UI StringTable 의 다국어 키 (없으면 Label 그대로)
        }

        public static readonly NavItem[] DefaultNavItems = new[]
        {
            new NavItem { Key = "home",       Label = "메인", IconKey = "home",     EntryKey = "nav_home" },
            new NavItem { Key = "daily",      Label = "일일", IconKey = "calendar", EntryKey = "nav_daily" },
            new NavItem { Key = "collection", Label = "갤러리", IconKey = "image",  EntryKey = "nav_collection" },
            new NavItem { Key = "shop",       Label = "상점", IconKey = "gift",     EntryKey = "nav_shop" },
        };

        // NavH 162 = 54 × 3. 시안 padding 8+10 + content (icon 24 + gap 2 + label 12) = 54px.
        // 양 컨트롤러 (LevelSelect / Gallery) 의 NavH 상수와 일치 — 변경 시 양쪽 동기화.
        public const float NavHeight = 162f;

        public static GameObject BuildBottomNav(Transform parent, string name,
                                                 string activeKey, Action<string> onTabClicked,
                                                 NavItem[] items = null)
        {
            items ??= DefaultNavItems;
            const float height = NavHeight;

            var nav = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
                typeof(HorizontalLayoutGroup));
            nav.transform.SetParent(parent, worldPositionStays: false);

            var rt = (RectTransform)nav.transform;
            rt.sizeDelta = new Vector2(0f, height);

            nav.GetComponent<Image>().color = PaletteTokens.Active.Surface;

            // 상단 1px 보더 (별도 child)
            var border = new GameObject("TopBorder",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            border.transform.SetParent(nav.transform, worldPositionStays: false);
            var brt = (RectTransform)border.transform;
            brt.anchorMin = new Vector2(0f, 1f);
            brt.anchorMax = new Vector2(1f, 1f);
            brt.pivot     = new Vector2(0.5f, 1f);
            brt.sizeDelta = new Vector2(0f, 3f); // 1 × 3
            brt.anchoredPosition = Vector2.zero;
            border.GetComponent<Image>().color = PaletteTokens.Active.Line;
            border.GetComponent<Image>().raycastTarget = false;
            border.AddComponent<LayoutElement>().ignoreLayout = true;

            var hl = nav.GetComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(0, 0, 24, 30);
            hl.spacing = 0f;
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childForceExpandWidth = true;
            hl.childForceExpandHeight = true;

            foreach (var it in items)
            {
                bool active = it.Key == activeKey;
                var tab = BuildNavTab(nav.transform, it, active, onTabClicked);
                var tabLE = tab.AddComponent<LayoutElement>();
                tabLE.flexibleWidth = 1f;
            }

            return nav;
        }

        private static GameObject BuildNavTab(Transform parent, NavItem it, bool active,
                                              Action<string> onTabClicked)
        {
            var tab = new GameObject($"Tab_{it.Key}",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            tab.transform.SetParent(parent, worldPositionStays: false);

            var img = tab.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0); // transparent — 클릭만 받음
            var btn = tab.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onTabClicked?.Invoke(it.Key));

            Color tint = active ? PaletteTokens.Active.Primary : PaletteTokens.Active.TextMuted;
            // 시안 권장 사이즈 (3060 가독성): icon 24px → 72 (×3), label 12px → 36 (×3), gap 2px → 6.
            // tab 사용 가능 영역 = NavH(162) - HLG padding(24+30) = 108. iconPx 72 + gap 6 + label 36 = 114
            // → 라벨 36 안에 fontSize 36 fits (line-height 1.0). icon padding 0 으로 정확히 들어맞음.
            int iconPx = 72;
            int labelFont = 36;
            int spacing = 6;

            // icon: anchor middle, pivot bottom, y = spacing/2 (중앙에서 위로)
            // label: anchor middle, pivot top, y = -(spacing/2)
            var icon = DesignTokens.BuildIcon(tab.transform, "Icon", it.IconKey, iconPx, tint);
            var iRt = (RectTransform)icon.transform;
            iRt.anchorMin = new Vector2(0.5f, 0.5f);
            iRt.anchorMax = new Vector2(0.5f, 0.5f);
            iRt.pivot     = new Vector2(0.5f, 0f);
            iRt.sizeDelta = new Vector2(iconPx, iconPx);
            iRt.anchoredPosition = new Vector2(0f, spacing / 2f);

            var label = string.IsNullOrEmpty(it.EntryKey)
                ? DesignTokens.BuildText(tab.transform, "Label", it.Label,
                    labelFont, active ? FontStyle.Bold : FontStyle.Normal, tint,
                    TextAnchor.UpperCenter)
                : DesignTokens.BuildLocalizedText(tab.transform, "Label", it.EntryKey, it.Label,
                    labelFont, active ? FontStyle.Bold : FontStyle.Normal, tint,
                    TextAnchor.UpperCenter);
            // Truncate (default) 가 line-height 빡빡할 때 통째 잘라버림 → Overflow 명시
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow   = VerticalWrapMode.Overflow;
            var lRt = (RectTransform)label.transform;
            lRt.anchorMin = new Vector2(0.5f, 0.5f);
            lRt.anchorMax = new Vector2(0.5f, 0.5f);
            lRt.pivot     = new Vector2(0.5f, 1f);
            lRt.sizeDelta = new Vector2(240f, labelFont + 12);
            lRt.anchoredPosition = new Vector2(0f, -(spacing / 2f));

            return tab;
        }

        // ─────────────────────────────────────────────────────
        // SequentialTile — cleared / current / locked 3상태
        // ─────────────────────────────────────────────────────

        public enum TileState { Locked, Current, Cleared }

        public struct TileSlot
        {
            public int Level;          // 1-based 표시용
            public int Stars;          // 0~3
            public int Pieces;
            public string PiecesUnit;  // 카운트 뱃지 단위 텍스트 — null/빈값이면 "조각"(직소 기본값)
            public string Label;       // 작품명 (placeholder, 다국어 키는 호출 측 결정)
            public Sprite Sprite;      // 실제 퍼즐 이미지 (없으면 그라디언트 fallback)
            public Color GradA;        // sprite 없을 때 그라디언트 시작
            public Color GradB;        // 그라디언트 끝
            public TileState State;
        }

        public const float TileLabelHeight = 60f; // 번호 라벨 영역

        public static GameObject BuildSequentialTile(Transform parent, TileSlot slot, Action onClick)
        {
            var p = PaletteTokens.Active;
            int radius = 24; // 8 × 3

            // 외곽 카드 (cleared/current = surface, locked = surfaceAlt)
            Color bg = slot.State == TileState.Cleared ? p.Surface : p.SurfaceAlt;

            var tile = BuildSurface(parent, $"Tile_{slot.Level:D2}", bg, radius);
            var btn = tile.AddComponent<Button>();
            btn.targetGraphic = tile.GetComponent<Image>();
            if (onClick != null) btn.onClick.AddListener(() => onClick.Invoke());

            // current 시 scale + shadow (Shadow 컴포넌트 — Image 에만 effect)
            if (slot.State == TileState.Current)
            {
                tile.transform.localScale = Vector3.one * 1.02f;
                var shadow = tile.AddComponent<Shadow>();
                shadow.effectColor = new Color(p.Primary.r, p.Primary.g, p.Primary.b, 0.25f);
                shadow.effectDistance = new Vector2(0f, -12f); // 4 × 3
            }

            // 이미지 영역 (위쪽, 정사각). cellH = cellW + TileLabelHeight 이므로 imgArea height = cellW.
            // GridLayoutGroup cellSize 따름 — anchor stretch top + sizeDelta.y = -TileLabelHeight 로 윗부분만 차지.
            var imgArea = new GameObject("ImageArea",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
            imgArea.transform.SetParent(tile.transform, worldPositionStays: false);
            var imRt = (RectTransform)imgArea.transform;
            imRt.anchorMin = new Vector2(0f, 0f);
            imRt.anchorMax = new Vector2(1f, 1f);
            imRt.pivot     = new Vector2(0.5f, 1f);
            imRt.offsetMin = new Vector2(0f, TileLabelHeight); // bottom = TileLabelHeight 만큼 비움
            imRt.offsetMax = new Vector2(0f, 0f);
            imgArea.GetComponent<Image>().color = bg;

            // 모든 상태에서 퍼즐 이미지 표시 (sprite 또는 그라디언트 fallback)
            BuildPuzzleImagePlaceholder(imgArea.transform, "Image", slot);

            // 우상단 조각 수 뱃지 — 모든 상태 (cleared/current/locked) 에서 표시
            BuildPiecesBadge(imgArea.transform, slot.Pieces, unit: string.IsNullOrEmpty(slot.PiecesUnit) ? "조각" : slot.PiecesUnit);

            if (slot.State == TileState.Locked)
            {
                // dim overlay — 검은 50% (이미지 살짝 비쳐 보임)
                var dim = new GameObject("Dim",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                dim.transform.SetParent(imgArea.transform, worldPositionStays: false);
                var drt = (RectTransform)dim.transform;
                drt.anchorMin = Vector2.zero; drt.anchorMax = Vector2.one;
                drt.offsetMin = Vector2.zero; drt.offsetMax = Vector2.zero;
                dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
                dim.GetComponent<Image>().raycastTarget = false;

                // lock 아이콘 — 중앙. dim overlay 위에 그려지도록 마지막에 추가.
                int lockSize = 60; // 20 × 3
                var lockIcon = DesignTokens.BuildIcon(imgArea.transform, "Lock", "lock",
                    lockSize, Color.white);
                var lrt = (RectTransform)lockIcon.transform;
                lrt.anchorMin = new Vector2(0.5f, 0.5f);
                lrt.anchorMax = new Vector2(0.5f, 0.5f);
                lrt.pivot     = new Vector2(0.5f, 0.5f);
                lrt.sizeDelta = new Vector2(lockSize, lockSize);
                lrt.anchoredPosition = Vector2.zero;

                // dim 위에 조각 수 뱃지가 그려지도록 sibling 순서 조정 — 뱃지를 마지막으로
                var badgeT = imgArea.transform.Find("PiecesBadge");
                if (badgeT != null) badgeT.SetAsLastSibling();
            }
            else
            {
                if (slot.State == TileState.Current)
                {
                    // 하단 그라디언트 오버레이 + NOW 라벨
                    var ovGo = new GameObject("CurrentOverlay",
                        typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    ovGo.transform.SetParent(imgArea.transform, worldPositionStays: false);
                    var oRt = (RectTransform)ovGo.transform;
                    oRt.anchorMin = Vector2.zero; oRt.anchorMax = Vector2.one;
                    oRt.offsetMin = Vector2.zero; oRt.offsetMax = Vector2.zero;
                    var oImg = ovGo.GetComponent<Image>();
                    oImg.sprite = GradientVertical(
                        new Color(p.Primary.r, p.Primary.g, p.Primary.b, 0.56f), // bottom (시안 90 alpha = ~56%)
                        new Color(0, 0, 0, 0));                                   // top transparent
                    oImg.color = Color.white;
                    oImg.raycastTarget = false;

                    var nowTxt = DesignTokens.BuildText(ovGo.transform, "Now", "NOW",
                        PaletteTokens.FontLabel, FontStyle.Bold, Color.white,
                        TextAnchor.LowerCenter);
                    var nrt = (RectTransform)nowTxt.transform;
                    nrt.anchorMin = Vector2.zero; nrt.anchorMax = Vector2.one;
                    nrt.offsetMin = new Vector2(0f, 12f);  // 4 × 3 padding
                    nrt.offsetMax = new Vector2(0f, 0f);
                }
                // cleared 의 별점 뱃지는 조각 수 뱃지로 대체 — 위에서 모든 state 에 BuildPiecesBadge 호출.
            }

            // 하단 번호 라벨 — anchor bottom stretch, height TileLabelHeight
            var labelBg = new GameObject("LabelBg",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            labelBg.transform.SetParent(tile.transform, worldPositionStays: false);
            var lbRt = (RectTransform)labelBg.transform;
            lbRt.anchorMin = new Vector2(0f, 0f);
            lbRt.anchorMax = new Vector2(1f, 0f);
            lbRt.pivot     = new Vector2(0.5f, 0f);
            lbRt.sizeDelta = new Vector2(0f, TileLabelHeight);
            lbRt.anchoredPosition = Vector2.zero;
            var lbImg = labelBg.GetComponent<Image>();
            lbImg.color = slot.State == TileState.Current ? p.Primary : p.Surface;

            Color lblColor = slot.State == TileState.Current ? Color.white
                           : slot.State == TileState.Cleared ? p.Text
                           : p.TextMuted;
            var lbl = DesignTokens.BuildText(labelBg.transform, "Num", slot.Level.ToString("D2"),
                PaletteTokens.FontMicro,
                slot.State == TileState.Current ? FontStyle.Bold : FontStyle.Bold,
                lblColor);
            var nRt = (RectTransform)lbl.transform;
            nRt.anchorMin = Vector2.zero; nRt.anchorMax = Vector2.one;
            nRt.offsetMin = Vector2.zero; nRt.offsetMax = Vector2.zero;

            return tile;
        }

        /// <summary>
        /// 우상단 카운트 뱃지 — pill bg + 흰 텍스트 (예: "36조각", MemoryMatch 는 "6개").
        /// bgColor null = 검은 반투명 (LevelSelect 기본). Gallery 는 P.Gold 명시.
        /// unit — 직소는 "조각", MemoryMatch 는 "개" 처럼 게임마다 다른 단위 텍스트.
        /// </summary>
        public static void BuildPiecesBadge(Transform parent, int pieces, Color? bgColor = null, string unit = "조각")
        {
            const float h = 36f;
            const float padH = 18f;
            int radius = (int)(h / 2f);

            string text = $"{pieces}{unit}";
            Color bg = bgColor ?? new Color(0f, 0f, 0f, 0.55f);

            var bgGo = new GameObject("PiecesBadge",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGo.transform.SetParent(parent, worldPositionStays: false);
            var rt = (RectTransform)bgGo.transform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-9f, -9f);

            var img = bgGo.GetComponent<Image>();
            img.sprite = RoundedRect(radius);
            img.type   = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1f;
            img.color  = bg;

            var lbl = DesignTokens.BuildLocalizedTextFormat(bgGo.transform, "Lbl",
                "tile_pieces_format", text, new object[] { pieces },
                24, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            lbl.horizontalOverflow = HorizontalWrapMode.Overflow;
            lbl.verticalOverflow   = VerticalWrapMode.Overflow;
            float w = lbl.preferredWidth + padH * 2f;
            rt.sizeDelta = new Vector2(w, h);

            var lRt = (RectTransform)lbl.transform;
            lRt.anchorMin = Vector2.zero; lRt.anchorMax = Vector2.one;
            lRt.offsetMin = Vector2.zero; lRt.offsetMax = Vector2.zero;
        }

        // ─────────────────────────────────────────────────────
        // PuzzleImage Placeholder — sprite 있으면 그대로, 없으면 그라디언트 + 라벨
        // ─────────────────────────────────────────────────────

        public static GameObject BuildPuzzleImagePlaceholder(Transform parent, string name, TileSlot slot)
        {
            var go = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, worldPositionStays: false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.raycastTarget = false;

            if (slot.Sprite != null)
            {
                img.sprite = slot.Sprite;
                img.preserveAspect = false; // cover
            }
            else
            {
                // 그라디언트 fallback
                Color a = slot.GradA.a > 0 ? slot.GradA : PaletteTokens.Active.StarterA;
                Color b = slot.GradB.a > 0 ? slot.GradB : PaletteTokens.Active.StarterB;
                img.sprite = GradientVertical(a, b);
                img.color  = Color.white;

                // 라벨 (작품명) — 작아서 가독성 떨어지면 생략 가능
                if (!string.IsNullOrEmpty(slot.Label))
                {
                    var lbl = DesignTokens.BuildText(go.transform, "Lbl", slot.Label,
                        PaletteTokens.FontMicro, FontStyle.Bold,
                        new Color(PaletteTokens.Active.Text.r,
                                  PaletteTokens.Active.Text.g,
                                  PaletteTokens.Active.Text.b, 0.6f));
                    var lrt = (RectTransform)lbl.transform;
                    lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
                    lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
                }
            }

            return go;
        }

        // ─────────────────────────────────────────────────────
        // Toast — 짧은 안내 (잠긴 카드 탭, BottomNav 미구현 탭 등)
        // ─────────────────────────────────────────────────────

        public static GameObject BuildToast(Transform parent, string message, float duration = 1.5f,
                                             string entryKey = null)
        {
            var p = PaletteTokens.Active;
            var toast = BuildSurface(parent, "Toast", p.Text, 24);
            var rt = (RectTransform)toast.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(720f, 132f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one * 0.85f;

            var txt = string.IsNullOrEmpty(entryKey)
                ? DesignTokens.BuildText(toast.transform, "Msg", message,
                    PaletteTokens.FontBody, FontStyle.Bold, p.Surface)
                : DesignTokens.BuildLocalizedText(toast.transform, "Msg", entryKey, message,
                    PaletteTokens.FontBody, FontStyle.Bold, p.Surface);
            var trt = (RectTransform)txt.transform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

            if (Application.isPlaying)
            {
                var img = toast.GetComponent<Image>();
                var seq = DOTween.Sequence();
                seq.Append(toast.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
                seq.AppendInterval(duration);
                seq.Append(DOTween.To(() => img.color.a,
                    v =>
                    {
                        var c = img.color; c.a = v; img.color = c;
                        foreach (var t in toast.GetComponentsInChildren<Text>())
                        {
                            var tc = t.color; tc.a = v; t.color = tc;
                        }
                    }, 0f, 0.35f));
                seq.OnComplete(() => { if (toast != null) UnityEngine.Object.Destroy(toast); });
            }

            return toast;
        }
    }
}
