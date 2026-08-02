using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlidingPuzzle.UI
{
    /// <summary>
    /// 디자인 시스템 v0.6 (Pastel Puzzle) — 4팔레트 + 따뜻한 파스텔 톤.
    ///
    /// 시안 출처: document/design_handoff_pastel_puzzle/README.md.
    /// 기존 DesignTokens (v0.4 카카오페이 노랑 CTA) 는 그대로 유지 — 신규 화면(HomeSequential 등)부터
    /// PaletteTokens 사용, 기존 화면(Gameplay 헤더 등) 은 점진 마이그레이션.
    ///
    /// 픽셀 단위: 시안은 360×640 viewport 기준. Unity ref 는 1080×1920 → 모든 px 토큰을 시안 × 3 으로 박음.
    /// </summary>
    public static class PaletteTokens
    {
        // ─────────────────────────────────────────────────────
        // Palette (색 묶음)
        // ─────────────────────────────────────────────────────

        public struct Palette
        {
            public string Key;         // "garden" | "peach" | "sage" | "lavender" — SaveManager 에 저장
            public string DisplayName; // "파스텔 정원" 등 — 설정 UI 표시용

            // 배경 / 카드
            public Color Bg;          // 화면 배경
            public Color Surface;     // 카드 / 모달 배경
            public Color SurfaceAlt;  // 보조 배경 (잠금 칸 등)

            // 강조
            public Color Primary;     // 메인 CTA, 강조 보더
            public Color PrimaryDeep; // hover/press, 진한 텍스트 강조
            public Color Accent;      // 세컨더리 (세이지 등)
            public Color AccentSoft;  // 세컨더리 틴트
            public Color Gold;        // 별점, 보상

            // 텍스트
            public Color Text;        // 본문
            public Color TextSoft;    // 보조 텍스트
            public Color TextMuted;   // 잠금/비활성

            // 디바이더 / 상태
            public Color Line;        // 보더, 디바이더
            public Color Danger;      // 에러, 경고

            // 그라데이션 (퍼즐 썸네일 placeholder 용)
            public Color StarterA;    // 그라데이션 시작
            public Color StarterB;    // 그라데이션 끝
        }

        public static Palette Garden   { get; private set; }
        public static Palette Peach    { get; private set; }
        public static Palette Sage     { get; private set; }
        public static Palette Lavender { get; private set; }

        /// <summary>현재 활성 팔레트. SetActive() 로 변경. 기본 garden.</summary>
        public static Palette Active { get; private set; }

        /// <summary>모든 팔레트 (설정 UI 의 토글 리스트 용).</summary>
        public static IReadOnlyList<Palette> All { get; private set; }

        /// <summary>팔레트 변경 시 발화 — 모든 빌드된 UI 가 재렌더 트리거하도록 구독.</summary>
        public static event Action OnPaletteChanged;

        static PaletteTokens()
        {
            Garden = new Palette
            {
                Key = "garden", DisplayName = "파스텔 정원",
                Bg          = H("#FBF6EE"),
                Surface     = H("#FFFFFF"),
                SurfaceAlt  = H("#F4ECDF"),
                Primary     = H("#D88A7A"),
                PrimaryDeep = H("#B7634F"),
                Accent      = H("#9CB28A"),
                AccentSoft  = H("#E5EBD5"),
                Gold        = H("#E0B86A"),
                Text        = H("#3D2E26"),
                TextSoft    = H("#8A7567"),
                TextMuted   = H("#B8A595"),
                Line        = H("#EADFCD"),
                Danger      = H("#D9636A"),
                StarterA    = H("#F7E3D2"),
                StarterB    = H("#E5EBD5"),
            };

            Peach = new Palette
            {
                Key = "peach", DisplayName = "피치 크림",
                Bg          = H("#FEF3EC"),
                Surface     = H("#FFFFFF"),
                SurfaceAlt  = H("#FCE5D5"),
                Primary     = H("#EB8B6B"),
                PrimaryDeep = H("#C66241"),
                Accent      = H("#C19BB7"),
                AccentSoft  = H("#F0E1EB"),
                Gold        = H("#E8B36C"),
                Text        = H("#3F2C24"),
                TextSoft    = H("#8C6E63"),
                TextMuted   = H("#BFA395"),
                Line        = H("#F1DDCC"),
                Danger      = H("#D9636A"),
                StarterA    = H("#FBD9C2"),
                StarterB    = H("#F0E1EB"),
            };

            Sage = new Palette
            {
                Key = "sage", DisplayName = "세이지 우드",
                Bg          = H("#F5F1E8"),
                Surface     = H("#FFFFFF"),
                SurfaceAlt  = H("#EAE2CF"),
                Primary     = H("#9DB089"),
                PrimaryDeep = H("#6B8260"),
                Accent      = H("#C8A27C"),
                AccentSoft  = H("#EFE2CF"),
                Gold        = H("#D6B27A"),
                Text        = H("#36322A"),
                TextSoft    = H("#7C7466"),
                TextMuted   = H("#B0A795"),
                Line        = H("#E2D9C4"),
                Danger      = H("#C77160"),
                StarterA    = H("#E5EBD5"),
                StarterB    = H("#EFE2CF"),
            };

            Lavender = new Palette
            {
                Key = "lavender", DisplayName = "라벤더 베리",
                Bg          = H("#F6F1F4"),
                Surface     = H("#FFFFFF"),
                SurfaceAlt  = H("#ECE0E8"),
                Primary     = H("#B295C2"),
                PrimaryDeep = H("#7E5F94"),
                Accent      = H("#D49AA8"),
                AccentSoft  = H("#F1DEE3"),
                Gold        = H("#D8B57E"),
                Text        = H("#3A2E3B"),
                TextSoft    = H("#7E6F80"),
                TextMuted   = H("#B5A4B6"),
                Line        = H("#E8DDE5"),
                Danger      = H("#C76B7A"),
                StarterA    = H("#E2D2EA"),
                StarterB    = H("#F1DEE3"),
            };

            All    = new[] { Garden, Peach, Sage, Lavender };
            Active = Garden;
        }

        /// <summary>키로 활성 팔레트 변경. 알 수 없는 키는 무시. 변경 시 OnPaletteChanged 발화.</summary>
        public static void SetActive(string key)
        {
            Palette next;
            switch (key)
            {
                case "garden":   next = Garden;   break;
                case "peach":    next = Peach;    break;
                case "sage":     next = Sage;     break;
                case "lavender": next = Lavender; break;
                default:
                    Debug.LogWarning($"[PaletteTokens] Unknown palette key: {key} — 변경 무시");
                    return;
            }
            if (next.Key == Active.Key) return;
            Active = next;
            OnPaletteChanged?.Invoke();
        }

        // ─────────────────────────────────────────────────────
        // Typography (Pretendard 기준, 시안 px × 3)
        // ─────────────────────────────────────────────────────
        // 시안 사용 예: 19px Display, 17px Title, 13~14px Body, 9~10px Micro/Label
        // Unity uGUI Text 는 letter-spacing 직접 미지원 — 시각적 임팩트는 fontSize/Weight 로 표현,
        // 0.04~0.06em uppercase 대용 micro/label 은 텍스트를 대문자로 작성하거나 자간 무시.

        public const int FontDisplay   = 57;  // 19 × 3 — 홈 헤더 (그림 한 점, 시작해 볼까요)
        public const int FontTitle     = 51;  // 17 × 3 — 화면 헤더
        public const int FontSubhead   = 45;  // 15 × 3 — "지금 도전" 카드 타이틀, 섹션 헤더
        public const int FontBodyLg    = 42;  // 14 × 3 — Body 큰
        public const int FontBody      = 39;  // 13 × 3 — Body 기본
        public const int FontCaption   = 36;  // 12 × 3 — Caption (전체 작품, 메타)
        public const int FontCaptionSm = 33;  // 11 × 3 — Caption 작은 (greeting, 보조 메타)
        public const int FontMicro     = 30;  // 10 × 3 — 타일 번호 등
        public const int FontLabel     = 27;  // 9 × 3  — "NOW", "지금 도전" 배지
        public const int FontCta       = 42;  // 14 × 3 — CTA 버튼

        // ─────────────────────────────────────────────────────
        // Spacing (시안 px × 3)
        // ─────────────────────────────────────────────────────

        public const float Sp4  = 12f;
        public const float Sp6  = 18f;
        public const float Sp8  = 24f;
        public const float Sp10 = 30f;
        public const float Sp12 = 36f;
        public const float Sp14 = 42f;
        public const float Sp16 = 48f;
        public const float Sp20 = 60f;
        public const float Sp24 = 72f;

        // ─────────────────────────────────────────────────────
        // Radius (시안 px × 3)
        // ─────────────────────────────────────────────────────

        public const float R4    = 12f;
        public const float R6    = 18f;
        public const float R8    = 24f;
        public const float R10   = 30f;
        public const float R12   = 36f;
        public const float R14   = 42f;
        public const float R16   = 48f;
        public const float Rpill = 9999f; // 캡슐 (사실상 height/2 로 자동)

        // ─────────────────────────────────────────────────────
        // Touch target (시안 px × 3)
        // ─────────────────────────────────────────────────────

        public const float TouchMin = 108f;  // 36 × 3 — 최소
        public const float TouchRec = 132f;  // 44 × 3 — 권장 (CTA)

        // ─────────────────────────────────────────────────────
        // Shadow 토큰 (Unity uGUI 는 Shadow/Outline 컴포넌트로 표현 — 참고용 값)
        // ─────────────────────────────────────────────────────
        // 시안 그림자: 따뜻한 갈색 톤 (rgba(60,40,30,*) 기반).
        // Unity 에서는 Image + Shadow 컴포넌트 effectColor 로 매핑.

        public static readonly Color ShadowSubtle   = new Color(0f,    0f,    0f,    0.04f); // 0 1px 3px
        public static readonly Color ShadowElevated = new Color(0.235f,0.157f,0.118f,0.05f); // 0 2px 8px (rgba(60,40,30))
        public static readonly Color ShadowModal    = new Color(0.235f,0.157f,0.118f,0.25f); // 0 20px 60px

        // ─────────────────────────────────────────────────────
        // Helper
        // ─────────────────────────────────────────────────────

        private static Color H(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
            Debug.LogWarning($"[PaletteTokens] Invalid hex: {hex}");
            return Color.magenta;
        }
    }
}
