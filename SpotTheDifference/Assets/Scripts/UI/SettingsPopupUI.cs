using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using SpotDiff.Systems;

namespace SpotDiff.UI
{
    /// <summary>
    /// 설정 팝업 — 디자인 v0.6 (Pastel Puzzle / Settings).
    ///
    /// 시안: 3060 따뜻한 톤 · 골드 액자 시그니처. 한·영 통합 + 진동 토글.
    /// 변경: 진동 제외 (사용자 결정).
    ///
    /// 구조:
    ///   - Dim overlay (검은 ~50%, 외부 클릭 시 닫힘)
    ///   - 카드 (Surface, radius 24, 가운데 정렬)
    ///       · 우상단 Close (X, surfaceAlt 둥근 사각)
    ///       · "SETTINGS" small (Micro, TextSoft) + "설정" Display Bold + 가운데 dash
    ///       · 언어 · Language subhead + 4탭 캡슐 toggle (한국어/English/日本語/中文)
    ///       · 효과음 + value pill + 슬라이더
    ///       · 배경음악 + value pill + 슬라이더
    ///       · 하단 링크 (이용약관 · 개인정보 · 문의하기) + 버전
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class SettingsPopupUI : MonoBehaviour
    {
        private const string SfxVolumeKey = "Settings_SfxVolume";
        private const string BgmVolumeKey = "Settings_BgmVolume";

        // BCP-47 코드 / 표시 라벨 (네이티브)
        private static readonly (string code, string label)[] Languages =
        {
            ("ko",      "한국어"),
            ("en",      "English"),
            ("ja",      "日本語"),
            ("zh-Hans", "中文"),
        };

        private RectTransform _root;
        private Image[]       _langBgs;
        private Text[]        _langTexts;
        private Text          _sfxValueTxt;
        private Text          _bgmValueTxt;
        private bool          _built;

        public void Show()
        {
            gameObject.SetActive(true);
            EnsureBuilt();
            UpdateLanguageVisual();
        }

        public void Hide() => gameObject.SetActive(false);

        private void Awake() => EnsureBuilt();

        private void EnsureBuilt()
        {
            if (_built) return;
            _built = true;
            _root  = (RectTransform)transform;
            try
            {
                BuildUI();
                Debug.Log("[SettingsPopupUI] BuildUI 완료 (v0.6)");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SettingsPopupUI] BuildUI 실패: {e.Message}\n{e.StackTrace}");
            }
        }

        // ─────────────────────────────────────────────────────
        // UI 빌드
        // ─────────────────────────────────────────────────────

        private void BuildUI()
        {
            var p = PaletteTokens.Active;

            // 자체 Canvas
            var canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0f;

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            _root.anchorMin = Vector2.zero;
            _root.anchorMax = Vector2.one;
            _root.offsetMin = Vector2.zero;
            _root.offsetMax = Vector2.zero;

            // 1) Dim 배경 (외부 클릭 → 닫힘)
            var dim = gameObject.GetComponent<Image>();
            if (dim == null) dim = gameObject.AddComponent<Image>();
            dim.color         = new Color(0f, 0f, 0f, 0.5f);
            dim.raycastTarget = true;

            var dimBtn = gameObject.GetComponent<Button>();
            if (dimBtn == null) dimBtn = gameObject.AddComponent<Button>();
            dimBtn.targetGraphic = dim;
            dimBtn.transition    = Selectable.Transition.None;
            dimBtn.onClick.RemoveAllListeners();
            dimBtn.onClick.AddListener(Hide);

            // 기존 Panel 자식 정리 (재호출 안전)
            var existing = transform.Find("Panel");
            if (existing != null)
            {
                if (Application.isPlaying) Destroy(existing.gameObject);
                else                       DestroyImmediate(existing.gameObject);
            }

            // 2) 카드 — 진동 제외로 height 축소 (시안 비율)
            var panel = PaletteUI.BuildSurface(transform, "Panel", p.Surface, 24);
            var prt = (RectTransform)panel.transform;
            prt.anchorMin = new Vector2(0.5f, 0.5f);
            prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.pivot     = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(900f, 1100f);
            prt.anchoredPosition = Vector2.zero;

            // 카드 안 클릭 흡수 (자식 슬라이더 click 이 dim 으로 새지 않도록)
            var panelBtn = panel.AddComponent<Button>();
            panelBtn.transition    = Selectable.Transition.None;
            panelBtn.targetGraphic = panel.GetComponent<Image>();

            // 3) Close 우상단
            BuildCloseButton(prt);

            // 4) SETTINGS small label (uppercase, letter-spacing 흉내 — Unity legacy 자간 X)
            var settingsLbl = DesignTokens.BuildText(prt, "SettingsLabel", "SETTINGS",
                PaletteTokens.FontMicro, FontStyle.Bold, p.TextSoft, TextAnchor.MiddleCenter);
            settingsLbl.horizontalOverflow = HorizontalWrapMode.Overflow;
            var slRt = (RectTransform)settingsLbl.transform;
            slRt.anchorMin = new Vector2(0.5f, 1f);
            slRt.anchorMax = new Vector2(0.5f, 1f);
            slRt.pivot     = new Vector2(0.5f, 1f);
            slRt.sizeDelta = new Vector2(300f, 40f);
            slRt.anchoredPosition = new Vector2(0f, -54f);

            // 5) "설정" Display title
            var title = DesignTokens.BuildLocalizedText(prt, "Title",
                "settings_title", "설정",
                PaletteTokens.FontDisplay, FontStyle.Bold, p.Text, TextAnchor.MiddleCenter);
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            title.verticalOverflow   = VerticalWrapMode.Overflow;
            var trt = (RectTransform)title.transform;
            trt.anchorMin = new Vector2(0.5f, 1f);
            trt.anchorMax = new Vector2(0.5f, 1f);
            trt.pivot     = new Vector2(0.5f, 1f);
            trt.sizeDelta = new Vector2(500f, 100f);
            trt.anchoredPosition = new Vector2(0f, -100f);

            // 6) 가운데 dash (얇은 line 또는 dot — 단순 line)
            var divider = new GameObject("Divider",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            divider.transform.SetParent(prt, worldPositionStays: false);
            var dvRt = (RectTransform)divider.transform;
            dvRt.anchorMin = new Vector2(0.5f, 1f);
            dvRt.anchorMax = new Vector2(0.5f, 1f);
            dvRt.pivot     = new Vector2(0.5f, 1f);
            dvRt.sizeDelta = new Vector2(60f, 3f);
            dvRt.anchoredPosition = new Vector2(0f, -220f);
            divider.GetComponent<Image>().color = p.Line;
            divider.GetComponent<Image>().raycastTarget = false;

            // 7) 언어 · Language subhead + 4탭 — 신규 키 (mixed 표기 보존)
            BuildSectionHeader(prt, "settings_lang_subhead", "언어 · Language", -260f);
            BuildLanguagePill(prt, -340f);

            // 8) 효과음 subhead + value + slider
            float savedSfx = PlayerPrefs.GetFloat(SfxVolumeKey, 0.6f);
            BuildSectionHeader(prt, "settings_sfx", "효과음", -490f);
            _sfxValueTxt = BuildValueChip(prt, $"{Mathf.RoundToInt(savedSfx * 100)}", -490f);
            BuildSliderRow(prt, "SfxSlider", savedSfx, OnSfxChanged, -580f);

            // 9) 배경음악 subhead + value + slider — settings_bgm_v6 (구 settings_bgm 은 "배경음")
            float savedBgm = PlayerPrefs.GetFloat(BgmVolumeKey, 0.75f);
            BuildSectionHeader(prt, "settings_bgm_v6", "배경음악", -700f);
            _bgmValueTxt = BuildValueChip(prt, $"{Mathf.RoundToInt(savedBgm * 100)}", -700f);
            BuildSliderRow(prt, "BgmSlider", savedBgm, OnBgmChanged, -790f);

            // 10) 버전 표기 (이용약관/개인정보/문의하기 링크 제외 — 사용자 결정)
            BuildVersionLabel(prt, -1010f);
        }

        // ─────────────────────────────────────────────────────
        // Close 버튼
        // ─────────────────────────────────────────────────────

        private void BuildCloseButton(RectTransform parent)
        {
            const float btnSize = 96f;
            var btn = PaletteUI.BuildIconBtn(parent, "CloseButton", "close", btnSize, highlight: false);
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-30f, -30f);
            btn.onClick.AddListener(Hide);
        }

        // ─────────────────────────────────────────────────────
        // Section header
        // ─────────────────────────────────────────────────────

        // 시안: 좌측 정렬 subhead (FontCaption 36, Bold, Text). "언어 · Language" 같은 mixed 텍스트.
        // 명시적 anchor (0,1) top-left + 충분한 sizeDelta.
        private static Text BuildSectionHeader(RectTransform parent, string entryKey, string fallback, float anchoredY)
        {
            var p = PaletteTokens.Active;
            var lbl = DesignTokens.BuildLocalizedText(parent, $"Section_{entryKey}", entryKey, fallback,
                PaletteTokens.FontCaption, FontStyle.Bold, p.Text, TextAnchor.MiddleLeft);
            lbl.horizontalOverflow = HorizontalWrapMode.Overflow;
            lbl.verticalOverflow   = VerticalWrapMode.Overflow;
            var rt = (RectTransform)lbl.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot     = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(700f, 60f);
            rt.anchoredPosition = new Vector2(36f, anchoredY); // 좌측 36 inset
            return lbl;
        }

        // ─────────────────────────────────────────────────────
        // 언어 4탭 캡슐 (외곽 surfaceAlt + 안 4 캡슐 탭)
        // ─────────────────────────────────────────────────────

        private void BuildLanguagePill(RectTransform parent, float anchoredY)
        {
            var p = PaletteTokens.Active;
            const float wrapH = 102f;
            const float wrapW = 828f; // 카드 width 900 - 좌우 36*2
            int wrapRadius = (int)(wrapH / 2f);

            var wrap = PaletteUI.BuildSurface(parent, "LangWrap", p.SurfaceAlt, wrapRadius);
            var wrt = (RectTransform)wrap.transform;
            wrt.anchorMin = new Vector2(0.5f, 1f);
            wrt.anchorMax = new Vector2(0.5f, 1f);
            wrt.pivot     = new Vector2(0.5f, 1f);
            wrt.sizeDelta = new Vector2(wrapW, wrapH);
            wrt.anchoredPosition = new Vector2(0f, anchoredY);

            _langBgs   = new Image[Languages.Length];
            _langTexts = new Text[Languages.Length];

            float tabW = (wrapW - 12f) / Languages.Length; // 좌우 6 inset
            float startX = 6f;
            for (int i = 0; i < Languages.Length; i++)
            {
                int idx = i;
                int tabRadius = (int)((wrapH - 12f) / 2f);
                var tab = PaletteUI.BuildSurface(wrap.transform, $"Lang_{Languages[idx].code}",
                    p.SurfaceAlt, tabRadius);
                var trt = (RectTransform)tab.transform;
                trt.anchorMin = new Vector2(0f, 0f);
                trt.anchorMax = new Vector2(0f, 1f);
                trt.pivot     = new Vector2(0f, 0.5f);
                trt.sizeDelta = new Vector2(tabW, -12f);
                trt.anchoredPosition = new Vector2(startX + idx * tabW, 0f);

                var btn = tab.AddComponent<Button>();
                btn.targetGraphic = tab.GetComponent<Image>();
                btn.onClick.AddListener(() => SelectLanguage(Languages[idx].code));
                _langBgs[idx] = tab.GetComponent<Image>();

                // 시안: subhead "언어" 보다 작게 (FontCaptionSm 33)
                var lbl = DesignTokens.BuildText(tab.transform, "Label", Languages[idx].label,
                    PaletteTokens.FontCaptionSm, FontStyle.Bold, p.TextSoft, TextAnchor.MiddleCenter);
                lbl.horizontalOverflow = HorizontalWrapMode.Overflow;
                var lrt = (RectTransform)lbl.transform;
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = Vector2.zero;
                lrt.offsetMax = Vector2.zero;
                _langTexts[idx] = lbl;
            }
        }

        // ─────────────────────────────────────────────────────
        // Value chip — 우측 상단 "60" / "75" pill
        // ─────────────────────────────────────────────────────

        private static Text BuildValueChip(RectTransform parent, string value, float anchoredY)
        {
            var p = PaletteTokens.Active;
            const float chipW = 96f;
            const float chipH = 48f;
            int radius = (int)(chipH / 2f);

            // 시안: 옅은 크림/베이지 톤 chip (AccentSoft 는 sage 라 안 맞음 → SurfaceAlt 사용)
            var chip = PaletteUI.BuildSurface(parent, $"ValueChip_{anchoredY}", p.SurfaceAlt, radius);
            var rt = (RectTransform)chip.transform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(chipW, chipH);
            rt.anchoredPosition = new Vector2(-36f, anchoredY);

            var lbl = DesignTokens.BuildText(chip.transform, "Lbl", value,
                PaletteTokens.FontLabel, FontStyle.Bold, p.PrimaryDeep, TextAnchor.MiddleCenter);
            lbl.horizontalOverflow = HorizontalWrapMode.Overflow;
            var lRt = (RectTransform)lbl.transform;
            lRt.anchorMin = Vector2.zero; lRt.anchorMax = Vector2.one;
            lRt.offsetMin = Vector2.zero; lRt.offsetMax = Vector2.zero;
            return lbl;
        }

        // ─────────────────────────────────────────────────────
        // 슬라이더 row — 좌측 small icon + 슬라이더 + 우측 large icon
        // ─────────────────────────────────────────────────────

        private void BuildSliderRow(RectTransform parent, string name, float initialValue,
                                    UnityEngine.Events.UnityAction<float> onChanged, float anchoredY)
        {
            var p = PaletteTokens.Active;
            const float rowH = 60f;
            const float iconSize = 48f;
            const float sidePad = 36f;
            float rowW = 900f - sidePad * 2f;

            var row = new GameObject(name + "_Row", typeof(RectTransform));
            row.transform.SetParent(parent, worldPositionStays: false);
            var rRt = (RectTransform)row.transform;
            rRt.anchorMin = new Vector2(0.5f, 1f);
            rRt.anchorMax = new Vector2(0.5f, 1f);
            rRt.pivot     = new Vector2(0.5f, 1f);
            rRt.sizeDelta = new Vector2(rowW, rowH);
            rRt.anchoredPosition = new Vector2(0f, anchoredY);

            // 좌 icon (작게)
            var leftIcon = DesignTokens.BuildIcon(row.transform, "LeftIcon",
                name.Contains("Sfx") ? "volume_down" : "music_note_off", 36, p.TextSoft);
            var liRt = (RectTransform)leftIcon.transform;
            liRt.anchorMin = new Vector2(0f, 0.5f);
            liRt.anchorMax = new Vector2(0f, 0.5f);
            liRt.pivot     = new Vector2(0f, 0.5f);
            liRt.sizeDelta = new Vector2(iconSize, iconSize);
            liRt.anchoredPosition = new Vector2(0f, 0f);

            // 우 icon (큼)
            var rightIcon = DesignTokens.BuildIcon(row.transform, "RightIcon",
                name.Contains("Sfx") ? "volume_up" : "music_note", 48, p.TextSoft);
            var riRt = (RectTransform)rightIcon.transform;
            riRt.anchorMin = new Vector2(1f, 0.5f);
            riRt.anchorMax = new Vector2(1f, 0.5f);
            riRt.pivot     = new Vector2(1f, 0.5f);
            riRt.sizeDelta = new Vector2(iconSize, iconSize);
            riRt.anchoredPosition = new Vector2(0f, 0f);

            // 슬라이더 (가운데)
            float sliderInset = iconSize + 24f;
            var sliderGo = new GameObject(name, typeof(RectTransform));
            sliderGo.transform.SetParent(row.transform, worldPositionStays: false);
            var srt = (RectTransform)sliderGo.transform;
            srt.anchorMin = new Vector2(0f, 0.5f);
            srt.anchorMax = new Vector2(1f, 0.5f);
            srt.pivot     = new Vector2(0.5f, 0.5f);
            srt.offsetMin = new Vector2(sliderInset, 0f);
            srt.offsetMax = new Vector2(-sliderInset, 0f);
            srt.sizeDelta = new Vector2(srt.sizeDelta.x, 24f);

            // Track
            var trackImg = sliderGo.AddComponent<Image>();
            trackImg.sprite = PaletteUI.RoundedRect(6);
            trackImg.type   = Image.Type.Sliced;
            trackImg.pixelsPerUnitMultiplier = 1f;
            trackImg.color  = p.Line;

            // Mask 로 fill 둥근 끝
            var trackMask = sliderGo.AddComponent<Mask>();
            trackMask.showMaskGraphic = true;

            // Fill
            var fillGo = new GameObject("Fill",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillGo.transform.SetParent(sliderGo.transform, worldPositionStays: false);
            var fRt = (RectTransform)fillGo.transform;
            fRt.anchorMin = new Vector2(0f, 0f);
            fRt.anchorMax = new Vector2(initialValue, 1f);
            fRt.offsetMin = Vector2.zero;
            fRt.offsetMax = Vector2.zero;
            fillGo.GetComponent<Image>().color = p.Primary;
            fillGo.GetComponent<Image>().raycastTarget = false;

            // Handle (흰 원형)
            var handleGo = new GameObject("Handle",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handleGo.transform.SetParent(sliderGo.transform, worldPositionStays: false);
            var hRt = (RectTransform)handleGo.transform;
            hRt.anchorMin = new Vector2(initialValue, 0.5f);
            hRt.anchorMax = new Vector2(initialValue, 0.5f);
            hRt.pivot     = new Vector2(0.5f, 0.5f);
            hRt.sizeDelta = new Vector2(54f, 54f);
            hRt.anchoredPosition = Vector2.zero;
            var hImg = handleGo.GetComponent<Image>();
            hImg.sprite = DesignTokens.CircleSprite;
            hImg.color  = p.Surface;
            // Handle 그림자
            var hShadow = handleGo.AddComponent<Shadow>();
            hShadow.effectColor    = new Color(0f, 0f, 0f, 0.18f);
            hShadow.effectDistance = new Vector2(0f, -3f);

            var slider = sliderGo.AddComponent<Slider>();
            slider.minValue   = 0f;
            slider.maxValue   = 1f;
            slider.value      = initialValue;
            slider.fillRect   = fRt;
            slider.handleRect = hRt;

            slider.onValueChanged.AddListener(onChanged);
            slider.onValueChanged.AddListener(v =>
            {
                fRt.anchorMax = new Vector2(v, 1f);
                hRt.anchorMin = new Vector2(v, 0.5f);
                hRt.anchorMax = new Vector2(v, 0.5f);
                // value chip 갱신
                int pct = Mathf.RoundToInt(v * 100);
                if (name.Contains("Sfx") && _sfxValueTxt != null) _sfxValueTxt.text = $"{pct}";
                if (name.Contains("Bgm") && _bgmValueTxt != null) _bgmValueTxt.text = $"{pct}";
            });
        }

        // ─────────────────────────────────────────────────────
        // 하단 링크 + 버전
        // ─────────────────────────────────────────────────────

        private static void BuildBottomLinks(RectTransform parent, float anchoredY)
        {
            var p = PaletteTokens.Active;
            // 단일 텍스트 — "이용약관 · 개인정보 · 문의하기" (각 항목 클릭 미구현, placeholder)
            var lbl = DesignTokens.BuildText(parent, "BottomLinks",
                "이용약관 · 개인정보 · 문의하기",
                PaletteTokens.FontCaptionSm, FontStyle.Normal, p.TextSoft, TextAnchor.MiddleCenter);
            lbl.horizontalOverflow = HorizontalWrapMode.Overflow;
            var rt = (RectTransform)lbl.transform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(700f, 48f);
            rt.anchoredPosition = new Vector2(0f, anchoredY);
        }

        private static void BuildVersionLabel(RectTransform parent, float anchoredY)
        {
            var p = PaletteTokens.Active;
            string version = Application.version;
            if (string.IsNullOrEmpty(version)) version = "1.0.2";
            string fallback = $"v{version} · 틀린그림찾기";
            var lbl = DesignTokens.BuildLocalizedTextFormat(parent, "Version",
                "settings_version_format", fallback, new object[] { version },
                PaletteTokens.FontMicro, FontStyle.Normal, p.TextMuted, TextAnchor.MiddleCenter);
            lbl.horizontalOverflow = HorizontalWrapMode.Overflow;
            var rt = (RectTransform)lbl.transform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(500f, 36f);
            rt.anchoredPosition = new Vector2(0f, anchoredY);
        }

        // ─────────────────────────────────────────────────────
        // 액션
        // ─────────────────────────────────────────────────────

        private void SelectLanguage(string code)
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            foreach (var locale in locales)
            {
                if (locale.Identifier.Code == code)
                {
                    LocalizationSettings.SelectedLocale = locale;
                    Debug.Log($"[SettingsPopupUI] 언어 변경: {code}");
                    UpdateLanguageVisual();
                    return;
                }
            }
            Debug.LogWarning($"[SettingsPopupUI] Locale {code} 등록 안 됨");
        }

        private void UpdateLanguageVisual()
        {
            if (_langBgs == null || _langTexts == null) return;

            var p = PaletteTokens.Active;
            string current = LocalizationSettings.SelectedLocale != null
                ? LocalizationSettings.SelectedLocale.Identifier.Code
                : "ko";

            for (int i = 0; i < Languages.Length; i++)
            {
                bool selected = (Languages[i].code == current);
                if (_langBgs[i] != null)
                    _langBgs[i].color = selected ? p.Primary : p.SurfaceAlt;
                if (_langTexts[i] != null)
                    _langTexts[i].color = selected ? Color.white : p.TextSoft;
            }
        }

        private void OnSfxChanged(float value)
        {
            AudioManager.Instance?.SetSfxVolume(value);
            PlayerPrefs.SetFloat(SfxVolumeKey, value);
            PlayerPrefs.Save();
            // chip text 갱신은 BuildSliderRow 의 2nd listener 가 책임 (시각 fill/handle 과 함께 일괄)
        }

        private void OnBgmChanged(float value)
        {
            AudioManager.Instance?.SetBgmVolume(value);
            PlayerPrefs.SetFloat(BgmVolumeKey, value);
            PlayerPrefs.Save();
        }
    }
}
