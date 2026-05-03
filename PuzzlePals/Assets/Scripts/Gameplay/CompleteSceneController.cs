using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using PuzzlePals.Core;
using PuzzlePals.Data;
using PuzzlePals.Systems;
using PuzzlePals.UI;

namespace PuzzlePals.Gameplay
{
    /// <summary>
    /// Complete 씬 — 디자인 v0.6 (Pastel Puzzle / CompleteCelebration).
    ///
    /// 시안: document/design_handoff_pastel_puzzle/screens.jsx 의 CompleteCelebration.
    /// 구조 (위 → 아래):
    ///   - 배경: radial gradient (starterA top → bg edge) — vertical gradient 로 근사
    ///   - 상단: gold pill "레벨 N 클리어" + "완성하셨어요!" Display + 부제
    ///   - 가운데: 흰 액자 (-1.5° 회전) + 안에 작품 이미지
    ///   - 작품 캡션: 작품명 + "나만의 갤러리에 저장되었어요"
    ///   - 하단: CTA 2개 — primary gradient "다음 그림 시작하기" + outline "갤러리 보기" + 안내 텍스트
    ///   - 액자 등장 후 컨페티 burst
    ///
    /// 테스트 호환: NextButton / HomeButton 이름 유지 (SceneTransitionTests TC_ST_004).
    /// </summary>
    [DisallowMultipleComponent]
    public class CompleteSceneController : MonoBehaviour
    {
        // ── Reference (1080×1920) ──
        private const float RefW = 1080f;

        // ── 시안 px × 3 ──
        private const float TopBlockY        = 90f;   // 상단 시작 (SafeArea inset 위)
        private const float PillH            = 96f;   // height (시안 ×3 보다 약간 키움 — 사용자 피드백)
        private const float PillFontSize     = 42f;   // 시안 11 → 더 큰 14×3 = 42
        private const float PillTopMargin    = 18f;   // 6
        private const float TitleTopMargin   = 36f;   // 12
        private const float TitleH           = 96f;   // fontSize 28 line-height
        private const float SubtitleTopMargin = 12f;  // 4
        private const float SubtitleH        = 48f;   // fontSize 13 line-height

        private const float FrameSize        = 660f;  // 220 × 3
        private const float FramePadding     = 30f;   // 10 × 3
        private const float FrameInnerPadding = 18f;  // 안쪽 여백 (작품과 액자 매트 사이)

        private const float CaptionTitleSize  = 48f;  // fontSize 16
        private const float CaptionSubtitleSize = 36f; // fontSize 12

        private const float CtaPrimaryH      = 156f;  // padding 16+16+content
        private const float CtaSecondaryH    = 120f;  // padding 12+12+content
        private const float CtaGap           = 30f;   // 10
        private const float NoticeH          = 36f;
        private const float NoticeTopMargin  = 30f;
        private const float BottomPad        = 48f;   // 16 × 3

        // SafeArea
        private float _topInsetRef;
        private float _bottomInsetRef;

        private void Start()
        {
            BuildUI();
            AudioManager.Instance?.PlayLevelComplete();
        }

        private void BuildUI()
        {
            _topInsetRef    = ComputeTopInsetRef();
            _bottomInsetRef = ComputeBottomInsetRef();

            // 1) Canvas
            var canvasGo = new GameObject("CompleteCanvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, worldPositionStays: false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(RefW, 1920f);
            scaler.matchWidthOrHeight  = 0f;

            var p = PaletteTokens.Active;

            // 2) 배경 — radial gradient 근사 (vertical gradient: top starterA → bottom bg)
            BuildBackground(canvas.transform, p);

            // 2.5) 정적 셀러브레이션 컨페티 — 화면 전체에 다양한 색 작은 직사각이 흩뿌려짐 (시안 Confetti)
            BuildStaticConfetti(canvas.transform, p);

            // 3) 좌상단 홈 버튼
            BuildHomeButton(canvas.transform);

            // 4) 레벨 데이터 로드
            int idx = LevelManager.Instance != null ? LevelManager.Instance.currentLevelIndex : 0;
            int levelNumber = idx + 1;
            var level = Resources.Load<LevelDataSO>($"Levels/Level_{levelNumber:D3}");

            // 5) 상단 블록 (anchor top, SafeArea 안)
            float topY = _topInsetRef + TopBlockY;
            BuildTopBlock(canvas.transform, p, levelNumber, ref topY);

            // 6) 액자 (가운데 — 약간 위로 offset)
            BuildPaintingFrame(canvas.transform, p, level);

            // 7) 작품 캡션 (액자 아래)
            BuildCaption(canvas.transform, p, level);

            // 8) 하단 CTA 2개 + 안내
            BuildBottomCtas(canvas.transform, p);
        }

        // ─────────────────────────────────────────────────────
        // Section builders
        // ─────────────────────────────────────────────────────

        private float ComputeTopInsetRef()
        {
            if (Screen.height <= 0) return 0f;
            float topInsetDevice = Screen.height - (Screen.safeArea.y + Screen.safeArea.height);
            float minInset = Mathf.Max(75f, Screen.height * 0.04f);
            float maxInset = Screen.height * 0.10f;
            if (topInsetDevice < minInset || topInsetDevice > maxInset)
                topInsetDevice = minInset;
            // Canvas height ref 변환 — 단순화로 ref 1920 사용 (matchWidthOrHeight=0 + width 1080 → 정확)
            return topInsetDevice * (1920f / Screen.height);
        }

        private float ComputeBottomInsetRef()
        {
            if (Screen.height <= 0) return 0f;
            float bottomInsetDevice = Screen.safeArea.y;  // home indicator 영역
            if (bottomInsetDevice < 0f) bottomInsetDevice = 0f;
            // 비정상 (화면의 8% 초과) 이면 0 으로 clamp (Android 시뮬레이터 reported 비정상 케이스 회피)
            if (bottomInsetDevice > Screen.height * 0.08f) bottomInsetDevice = 0f;
            return bottomInsetDevice * (1920f / Screen.height);
        }

        private void BuildBackground(Transform parent, PaletteTokens.Palette p)
        {
            // PageBg — bg color
            var bg = new GameObject("PageBg",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bg.transform.SetParent(parent, worldPositionStays: false);
            var brt = (RectTransform)bg.transform;
            brt.anchorMin = Vector2.zero;
            brt.anchorMax = Vector2.one;
            brt.offsetMin = Vector2.zero;
            brt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = p.Bg;

            // Top gradient (starterA top → bg bottom) — 화면 위 60% 영역에 fade
            var grad = new GameObject("TopGradient",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            grad.transform.SetParent(parent, worldPositionStays: false);
            var grt = (RectTransform)grad.transform;
            grt.anchorMin = new Vector2(0f, 0.4f);
            grt.anchorMax = new Vector2(1f, 1f);
            grt.offsetMin = Vector2.zero;
            grt.offsetMax = Vector2.zero;
            var gImg = grad.GetComponent<Image>();
            // PaletteUI.GradientVertical: bottom→top alpha. 우리는 top=starterA(opaque), bottom=transparent.
            // Sprite create: a=top (opaque starterA), b=bottom (transparent)
            gImg.sprite = PaletteUI.GradientVertical(
                new Color(p.StarterA.r, p.StarterA.g, p.StarterA.b, 0f),  // bottom (transparent)
                new Color(p.StarterA.r, p.StarterA.g, p.StarterA.b, 1f)); // top (opaque starterA)
            gImg.color = Color.white;
            gImg.raycastTarget = false;
        }

        private void BuildHomeButton(Transform parent)
        {
            var p = PaletteTokens.Active;
            var btn = PaletteUI.BuildIconBtn(parent, "HomeButton", "home", 108f, highlight: false);
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot     = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(108f, 108f);
            rt.anchoredPosition = new Vector2(45f, -(_topInsetRef + 45f));
            btn.onClick.AddListener(() => SceneLoader.LoadScene(SceneLoader.LevelSelect));
        }

        // ─────────────────────────────────────────────────────
        // 상단 — gold pill + "완성하셨어요!" + 부제
        // ─────────────────────────────────────────────────────

        private void BuildTopBlock(Transform parent, PaletteTokens.Palette p, int levelNumber, ref float topY)
        {
            // 1) Gold pill "레벨 N 클리어"
            string pillText = $"레벨 {levelNumber} 클리어";
            int pillRadius = (int)(PillH / 2f);

            var pill = PaletteUI.BuildSurface(parent, "ClearedPill", p.Gold, pillRadius);
            var pRt = (RectTransform)pill.transform;
            pRt.anchorMin = new Vector2(0.5f, 1f);
            pRt.anchorMax = new Vector2(0.5f, 1f);
            pRt.pivot     = new Vector2(0.5f, 1f);

            // 텍스트 임시 만들고 width 측정 — 사용자 피드백: 폰트 더 크게
            var pillLbl = DesignTokens.BuildLocalizedTextFormat(pill.transform, "Lbl",
                "complete_pill_clear", pillText, new object[] { levelNumber },
                (int)PillFontSize, FontStyle.Bold, Color.white);
            pillLbl.horizontalOverflow = HorizontalWrapMode.Overflow;
            float pillTextW = pillLbl.preferredWidth;
            float pillW = pillTextW + 96f; // 좌우 padding 48
            pRt.sizeDelta = new Vector2(pillW, PillH);
            pRt.anchoredPosition = new Vector2(0f, -topY);

            // 라벨 stretch
            var lblRt = (RectTransform)pillLbl.transform;
            lblRt.anchorMin = Vector2.zero;
            lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = Vector2.zero;
            lblRt.offsetMax = Vector2.zero;

            // pill 의 entrance — fade + scale
            pill.transform.localScale = Vector3.one * 0.85f;
            var pillImg = pill.GetComponent<Image>();
            var pillC = pillImg.color; pillC.a = 0f; pillImg.color = pillC;
            DOTween.To(() => pillImg.color.a,
                v => { var c = pillImg.color; c.a = v; pillImg.color = c; },
                1f, 0.4f).SetDelay(0.1f);
            pill.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetDelay(0.1f);

            topY += PillH + TitleTopMargin;

            // 2) "완성하셨어요!" Display
            var title = DesignTokens.BuildLocalizedText(parent, "Done",
                "complete_done_title", "완성하셨어요!",
                84, FontStyle.Bold, p.Text, TextAnchor.UpperCenter);
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            title.verticalOverflow   = VerticalWrapMode.Overflow;
            var tRt = (RectTransform)title.transform;
            tRt.anchorMin = new Vector2(0.5f, 1f);
            tRt.anchorMax = new Vector2(0.5f, 1f);
            tRt.pivot     = new Vector2(0.5f, 1f);
            tRt.sizeDelta = new Vector2(900f, TitleH);
            tRt.anchoredPosition = new Vector2(0f, -topY);
            tRt.localScale = Vector3.zero;
            tRt.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(0.3f);

            topY += TitleH + SubtitleTopMargin;

            // 3) 부제 "나만의 갤러리에 그림 한 점이 더해졌어요"
            // 구 키 complete_subtitle 은 "{0} 완성!" format 이라 v0.6 신규 키 complete_subtitle_v6 사용
            var sub = DesignTokens.BuildLocalizedText(parent, "Subtitle",
                "complete_subtitle_v6", "나만의 갤러리에 그림 한 점이 더해졌어요",
                PaletteTokens.FontBody, FontStyle.Normal, p.TextSoft, TextAnchor.UpperCenter);
            sub.horizontalOverflow = HorizontalWrapMode.Overflow;
            var sRt = (RectTransform)sub.transform;
            sRt.anchorMin = new Vector2(0.5f, 1f);
            sRt.anchorMax = new Vector2(0.5f, 1f);
            sRt.pivot     = new Vector2(0.5f, 1f);
            sRt.sizeDelta = new Vector2(900f, SubtitleH);
            sRt.anchoredPosition = new Vector2(0f, -topY);

            topY += SubtitleH;
        }

        // ─────────────────────────────────────────────────────
        // 액자 — 흰 surface, -1.5° 회전, 그림 cover
        // ─────────────────────────────────────────────────────

        private void BuildPaintingFrame(Transform parent, PaletteTokens.Palette p, LevelDataSO level)
        {
            // 액자 외곽 (FrameSize) — 흰 surface, radius 8×3=24
            var frame = PaletteUI.BuildSurface(parent, "Frame", p.Surface, 24);
            var fRt = (RectTransform)frame.transform;
            fRt.anchorMin = new Vector2(0.5f, 0.5f);
            fRt.anchorMax = new Vector2(0.5f, 0.5f);
            fRt.pivot     = new Vector2(0.5f, 0.5f);
            fRt.sizeDelta = new Vector2(FrameSize, FrameSize);
            fRt.anchoredPosition = new Vector2(0f, 90f); // 가운데 + 약간 위
            fRt.localRotation = Quaternion.Euler(0f, 0f, -1.5f);

            // shadow — Shadow 컴포넌트로 표현
            var shadow = frame.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0.235f, 0.157f, 0.118f, 0.18f);
            shadow.effectDistance = new Vector2(0f, -24f);

            // 안쪽 cover — RectMask2D 로 클리핑
            frame.AddComponent<RectMask2D>();

            if (level != null && level.characterSprite != null)
            {
                var cover = new GameObject("Cover",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
                    typeof(AspectRatioFitter));
                cover.transform.SetParent(frame.transform, worldPositionStays: false);
                var crt = (RectTransform)cover.transform;
                crt.anchorMin = Vector2.zero;
                crt.anchorMax = Vector2.one;
                crt.offsetMin = new Vector2(FrameInnerPadding, FrameInnerPadding);
                crt.offsetMax = new Vector2(-FrameInnerPadding, -FrameInnerPadding);

                var img = cover.GetComponent<Image>();
                img.sprite         = level.characterSprite;
                img.color          = Color.white;
                img.preserveAspect = false;
                img.raycastTarget  = false;

                var fitter = cover.GetComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                var rect = level.characterSprite.rect;
                fitter.aspectRatio = rect.height > 0 ? rect.width / rect.height : 1f;
            }

            // 엔트런스 — t=0.6 부터 scale 0 → 1, OutBack
            frame.transform.localScale = Vector3.zero;
            frame.transform.DOScale(1f, 0.6f).SetEase(Ease.OutBack).SetDelay(0.6f)
                .OnComplete(() =>
                {
                    if (Application.isPlaying)
                        SpawnConfettiBurst(parent, fRt.anchoredPosition);
                });
        }

        // ─────────────────────────────────────────────────────
        // 작품 캡션 — 작품명 + "나만의 갤러리에 저장되었어요"
        // ─────────────────────────────────────────────────────

        private void BuildCaption(Transform parent, PaletteTokens.Palette p, LevelDataSO level)
        {
            // Caption block 위치 — 액자 아래
            float captionTopY = -(90f - FrameSize * 0.5f - 60f); // 액자 bottom + margin

            // characterNameKey 는 사용자 데이터 — 다국어 X. 비어있을 때만 fallback "제목" (level_label_default 키).
            bool hasName = level != null && !string.IsNullOrEmpty(level.characterNameKey);
            string artworkName = hasName ? level.characterNameKey : "제목";

            // 작품명
            Text title = hasName
                ? DesignTokens.BuildText(parent, "ArtworkTitle", artworkName,
                    (int)CaptionTitleSize, FontStyle.Bold, p.Text, TextAnchor.UpperCenter)
                : DesignTokens.BuildLocalizedText(parent, "ArtworkTitle",
                    "level_label_default", artworkName,
                    (int)CaptionTitleSize, FontStyle.Bold, p.Text, TextAnchor.UpperCenter);
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            var tRt = (RectTransform)title.transform;
            tRt.anchorMin = new Vector2(0.5f, 0.5f);
            tRt.anchorMax = new Vector2(0.5f, 0.5f);
            tRt.pivot     = new Vector2(0.5f, 1f);
            tRt.sizeDelta = new Vector2(900f, CaptionTitleSize + 18);
            tRt.anchoredPosition = new Vector2(0f, captionTopY);
            tRt.localScale = Vector3.zero;
            tRt.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(0.9f);

            // 부제 "나만의 갤러리에 저장되었어요"
            var sub = DesignTokens.BuildLocalizedText(parent, "ArtworkSub",
                "complete_caption_saved", "나만의 갤러리에 저장되었어요",
                (int)CaptionSubtitleSize, FontStyle.Normal, p.TextSoft, TextAnchor.UpperCenter);
            var sRt = (RectTransform)sub.transform;
            sRt.anchorMin = new Vector2(0.5f, 0.5f);
            sRt.anchorMax = new Vector2(0.5f, 0.5f);
            sRt.pivot     = new Vector2(0.5f, 1f);
            sRt.sizeDelta = new Vector2(900f, CaptionSubtitleSize + 12);
            sRt.anchoredPosition = new Vector2(0f, captionTopY - (CaptionTitleSize + 18) - 9);
        }

        // ─────────────────────────────────────────────────────
        // 하단 CTA 2개 + 안내
        // ─────────────────────────────────────────────────────

        private void BuildBottomCtas(Transform parent, PaletteTokens.Palette p)
        {
            // bottom 영역 — sticky bottom. SafeArea bottom (home indicator) 위로 BottomPad 만큼 띄움.
            // height = NoticeH + NoticeTopMargin + CtaSecondaryH + CtaGap + CtaPrimaryH + BottomPad + bottomInset
            float bottomY = _bottomInsetRef + BottomPad + NoticeH + NoticeTopMargin;

            // 안내 (가장 아래)
            var notice = DesignTokens.BuildLocalizedText(parent, "AdNotice",
                "complete_ad_notice", "다음 그림 시작 시 짧은 광고가 나올 수 있어요",
                30, FontStyle.Normal, p.TextMuted, TextAnchor.MiddleCenter);
            var nRt = (RectTransform)notice.transform;
            nRt.anchorMin = new Vector2(0f, 0f);
            nRt.anchorMax = new Vector2(1f, 0f);
            nRt.pivot     = new Vector2(0.5f, 0f);
            nRt.sizeDelta = new Vector2(0f, NoticeH);
            nRt.anchoredPosition = new Vector2(0f, _bottomInsetRef + BottomPad);

            // CTA Secondary — "갤러리 보기" outline
            var galleryBtn = BuildOutlineButton(parent, "GalleryButton", "갤러리 보기", "complete_btn_gallery");
            var gRt = (RectTransform)galleryBtn.transform;
            gRt.anchorMin = new Vector2(0f, 0f);
            gRt.anchorMax = new Vector2(1f, 0f);
            gRt.pivot     = new Vector2(0.5f, 0f);
            gRt.sizeDelta = new Vector2(-(60f * 2f), CtaSecondaryH); // 좌우 inset 60 (20×3)
            gRt.anchoredPosition = new Vector2(0f, bottomY);
            galleryBtn.onClick.AddListener(OnGallery);

            bottomY += CtaSecondaryH + CtaGap;

            // CTA Primary — "다음 그림 시작하기" gradient
            var nextBtn = BuildPrimaryGradientButton(parent, "NextButton", "다음 그림 시작하기", "complete_btn_next");
            var ntRt = (RectTransform)nextBtn.transform;
            ntRt.anchorMin = new Vector2(0f, 0f);
            ntRt.anchorMax = new Vector2(1f, 0f);
            ntRt.pivot     = new Vector2(0.5f, 0f);
            ntRt.sizeDelta = new Vector2(-(60f * 2f), CtaPrimaryH);
            ntRt.anchoredPosition = new Vector2(0f, bottomY);
            nextBtn.onClick.AddListener(OnNext);

            // 엔트런스 — t=1.3 부터 등장
            nextBtn.transform.localScale = Vector3.zero;
            galleryBtn.transform.localScale = Vector3.zero;
            nextBtn.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(1.3f)
                .OnComplete(() =>
                {
                    if (Application.isPlaying)
                    {
                        nextBtn.transform.DOScale(1.04f, 0.7f)
                            .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                    }
                });
            galleryBtn.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(1.4f);
        }

        private Button BuildPrimaryGradientButton(Transform parent, string name, string label, string entryKey = null)
        {
            var p = PaletteTokens.Active;
            // PaletteUI.BuildSurface 가 단색 기반. gradient 는 별도 sprite 필요.
            // 단순화: primary color 단색 + radius 16×3=48. 시안의 gradient 효과는 추후.
            int radius = 48;
            var btnGo = PaletteUI.BuildSurface(parent, name, p.Primary, radius);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnGo.GetComponent<Image>();

            // shadow — Shadow 컴포넌트
            var shadow = btnGo.AddComponent<Shadow>();
            shadow.effectColor    = new Color(p.Primary.r, p.Primary.g, p.Primary.b, 0.4f);
            shadow.effectDistance = new Vector2(0f, -12f);

            // 라벨 — 흰 텍스트, fontSize 16 → 48
            var lbl = string.IsNullOrEmpty(entryKey)
                ? DesignTokens.BuildText(btnGo.transform, "Lbl", label,
                    48, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter)
                : DesignTokens.BuildLocalizedText(btnGo.transform, "Lbl", entryKey, label,
                    48, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            var lRt = (RectTransform)lbl.transform;
            lRt.anchorMin = Vector2.zero;
            lRt.anchorMax = Vector2.one;
            lRt.offsetMin = Vector2.zero;
            lRt.offsetMax = new Vector2(-30f, 0f); // 우측 chevron 자리

            // chevron — 우측 중앙
            var icon = DesignTokens.BuildIcon(btnGo.transform, "Chev", "chevron_right",
                42, Color.white);
            var iRt = (RectTransform)icon.transform;
            iRt.anchorMin = new Vector2(1f, 0.5f);
            iRt.anchorMax = new Vector2(1f, 0.5f);
            iRt.pivot     = new Vector2(1f, 0.5f);
            iRt.sizeDelta = new Vector2(42f, 42f);
            iRt.anchoredPosition = new Vector2(-48f, 0f);

            return btn;
        }

        private Button BuildOutlineButton(Transform parent, string name, string label, string entryKey = null)
        {
            var p = PaletteTokens.Active;
            int radius = 42; // 14 × 3
            var btnGo = PaletteUI.BuildSurface(parent, name, p.Surface, radius);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnGo.GetComponent<Image>();

            // 라벨 — TextSoft, fontSize 13 → 39
            var lbl = string.IsNullOrEmpty(entryKey)
                ? DesignTokens.BuildText(btnGo.transform, "Lbl", label,
                    39, FontStyle.Bold, p.TextSoft, TextAnchor.MiddleCenter)
                : DesignTokens.BuildLocalizedText(btnGo.transform, "Lbl", entryKey, label,
                    39, FontStyle.Bold, p.TextSoft, TextAnchor.MiddleCenter);
            var lRt = (RectTransform)lbl.transform;
            lRt.anchorMin = Vector2.zero;
            lRt.anchorMax = Vector2.one;
            lRt.offsetMin = Vector2.zero;
            lRt.offsetMax = Vector2.zero;

            return btn;
        }

        // ─────────────────────────────────────────────────────
        // 정적 셀러브레이션 컨페티 — 화면 전체에 흩뿌려진 작은 직사각 점들 (시안 Confetti)
        // 액자/CTA 위에는 안 그려지도록 raycastTarget=false + 얕은 sortingOrder.
        // ─────────────────────────────────────────────────────

        private static void BuildStaticConfetti(Transform parent, PaletteTokens.Palette p)
        {
            const int count = 60;  // 시안: 더 풍성한 분포
            Color[] palette =
            {
                p.Primary, p.Gold, p.Accent, p.PrimaryDeep, p.AccentSoft, p.StarterA,
                new Color(0.9f, 0.65f, 0.55f, 1f), // 따뜻한 살구
                new Color(0.85f, 0.7f, 0.55f, 1f), // 옅은 골드
            };

            var rng = new System.Random(42);
            float Range(float a, float b) => a + (float)rng.NextDouble() * (b - a);

            // 화면 ref 영역 (1080×1920 기준 + 긴 디바이스 대응 위해 hTop/hBot 더 크게)
            float w = 540f;
            float hTop = 1100f;  // 상단까지 더 위
            float hBot = 800f;   // 하단까지 (CTA 위)

            for (int i = 0; i < count; i++)
            {
                var dot = new GameObject($"Confetti_{i:D2}",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                dot.transform.SetParent(parent, worldPositionStays: false);
                var rt = (RectTransform)dot.transform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot     = new Vector2(0.5f, 0.5f);
                // 사이즈 작게 — 시안 톤
                rt.sizeDelta = new Vector2(Range(12f, 24f), Range(24f, 42f));
                rt.localRotation = Quaternion.Euler(0f, 0f, Range(-60f, 60f));

                // 균일 분포 — 화면 전체 (액자/CTA 와 raycast 충돌 안 함, 시각적으로만 겹침)
                float x = Range(-w, w);
                float y = Range(-hBot, hTop);

                rt.anchoredPosition = new Vector2(x, y);

                var img = dot.GetComponent<Image>();
                img.color         = palette[rng.Next(palette.Length)];
                img.raycastTarget = false;

                // entrance — fade in 살짝 stagger
                var c = img.color; c.a = 0f; img.color = c;
                float dur = (float)(0.3f + rng.NextDouble() * 0.5f);
                float delay = (float)(rng.NextDouble() * 0.6f);
                DOTween.To(() => img.color.a,
                    v => { var cc = img.color; cc.a = v; img.color = cc; },
                    1f, dur).SetDelay(delay);
            }
        }

        // ─────────────────────────────────────────────────────
        // 컨페티 burst (재사용) — 액자 등장 후 한 번
        // ─────────────────────────────────────────────────────

        private static void SpawnConfettiBurst(Transform parent, Vector2 centerAnchored)
        {
            const int   count       = 18;
            const float minDistance = 220f;
            const float maxDistance = 420f;
            const float duration    = 0.9f;

            var p = PaletteTokens.Active;
            Color[] palette =
            {
                p.Primary,
                p.Gold,
                p.Accent,
                p.PrimaryDeep,
                new Color(1f, 0.85f, 0.6f, 1f),
            };

            for (int i = 0; i < count; i++)
            {
                var dot = new GameObject($"Confetti_{i:D2}",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                dot.transform.SetParent(parent, worldPositionStays: false);
                var rt = (RectTransform)dot.transform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot     = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(Random.Range(14f, 26f), Random.Range(14f, 26f));
                rt.anchoredPosition = centerAnchored;

                var img = dot.GetComponent<Image>();
                img.sprite        = DesignTokens.CircleSprite;
                img.color         = palette[Random.Range(0, palette.Length)];
                img.raycastTarget = false;

                float angle = Random.Range(0f, Mathf.PI * 2f);
                float dist  = Random.Range(minDistance, maxDistance);
                Vector2 target = centerAnchored + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
                float dur = duration * Random.Range(0.85f, 1.15f);

                var rtCapture = rt;
                DOTween.To(() => rtCapture.anchoredPosition,
                           v => rtCapture.anchoredPosition = v,
                           target, dur).SetEase(Ease.OutQuad);
                rt.DOScale(0.3f, dur).SetEase(Ease.InQuad);
                DOTween.To(() => img.color.a,
                           v => { var c = img.color; c.a = v; img.color = c; },
                           0f, dur).SetEase(Ease.InQuad)
                       .OnComplete(() => { if (dot != null) Destroy(dot); });
            }
        }

        // ─────────────────────────────────────────────────────
        // 액션
        // ─────────────────────────────────────────────────────

        private static void OnNext()
        {
            var sl = FindAnyObjectByType<SceneLoader>(FindObjectsInactive.Include);
            if (sl == null)
            {
                var go = new GameObject("SceneLoader");
                sl = go.AddComponent<SceneLoader>();
            }
            sl.LoadNextLevel();
        }

        private static void OnGallery()
        {
            SceneLoader.LoadScene(SceneLoader.Gallery);
        }
    }
}
