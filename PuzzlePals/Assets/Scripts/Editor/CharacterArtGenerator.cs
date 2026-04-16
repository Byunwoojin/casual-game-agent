using UnityEngine;

namespace PuzzlePals.EditorTools
{
    /// <summary>
    /// 기하학적 도형 합성으로 25개 핵심 캐릭터를 PNG 픽셀버퍼에 그린다.
    /// 디자이너 아트 도착 전까지의 향상된 플레이스홀더.
    ///
    /// 사용:
    ///   var pixels = new Color32[Size * Size];
    ///   bool drew = CharacterArtGenerator.DrawCharacter("char_apple", pixels);
    ///   if (!drew) { /* shape library fallback */ }
    /// </summary>
    public static class CharacterArtGenerator
    {
        public const int Size = 384;     // 256 → 384 (2.25× 디테일)

        // 자주 쓰는 색
        private static readonly Color32 RED      = new Color32(220,  60,  60, 255);
        private static readonly Color32 PINK     = new Color32(255, 130, 170, 255);
        private static readonly Color32 ORANGE   = new Color32(245, 150,  60, 255);
        private static readonly Color32 YELLOW   = new Color32(250, 215,  60, 255);
        private static readonly Color32 GREEN    = new Color32( 90, 200, 100, 255);
        private static readonly Color32 DARKGREEN= new Color32( 50, 140,  60, 255);
        private static readonly Color32 BLUE     = new Color32( 80, 160, 230, 255);
        private static readonly Color32 PURPLE   = new Color32(170, 110, 220, 255);
        private static readonly Color32 BROWN    = new Color32(150,  95,  55, 255);
        private static readonly Color32 DARKBROWN= new Color32(100,  65,  40, 255);
        private static readonly Color32 GRAY     = new Color32(170, 170, 170, 255);
        private static readonly Color32 DARKGRAY = new Color32( 80,  80,  80, 255);
        private static readonly Color32 BLACK    = new Color32( 30,  30,  30, 255);
        private static readonly Color32 WHITE    = new Color32(245, 245, 245, 255);
        private static readonly Color32 GOLD     = new Color32(245, 195,  70, 255);

        /// <summary>charKey 에 대응하는 캐릭터 그림. 매칭 없으면 false 반환 (호출자가 fallback).</summary>
        public static bool DrawCharacter(string charKey, Color32[] px)
        {
            string key = Normalize(charKey);

            switch (key)
            {
                // 사물
                case "apple":     DrawApple(px);     return true;
                case "ball":      DrawBall(px);      return true;
                case "sun":       DrawSun(px);       return true;
                case "heart":     DrawHeart(px);     return true;
                case "star":      DrawStar(px);      return true;
                case "cup":       DrawCup(px);       return true;
                case "balloon":   DrawBalloon(px);   return true;
                case "flower":    DrawFlower(px);    return true;
                case "house":     DrawHouse(px);     return true;
                case "cake":      DrawCake(px);      return true;
                case "car":       DrawCar(px);       return true;
                case "boat":      DrawBoat(px);      return true;
                case "tree":      DrawTree(px);      return true;
                case "kite":      DrawKite(px);      return true;
                case "rocket":    DrawRocket(px);    return true;
                case "crown":     DrawCrown(px);     return true;
                case "diamond":   DrawDiamond(px);   return true;
                case "snowflake": DrawSnowflake(px); return true;
                case "cactus":    DrawCactus(px);    return true;

                // 동물
                case "cat":       DrawCat(px);       return true;
                case "dog":       DrawDog(px);       return true;
                case "fish":      DrawFish(px);      return true;
                case "rabbit":    DrawRabbit(px);    return true;
                case "owl":       DrawOwl(px);       return true;
                case "butterfly": DrawButterfly(px); return true;
                case "pig":       DrawPig(px);       return true;
                case "frog":      DrawFrog(px);      return true;
                case "duck":      DrawDuck(px);      return true;
                case "chick":     DrawChick(px);     return true;
                case "bear":      DrawBear(px);      return true;

                default: return false;
            }
        }

        // ─────────────────────────────────────────────
        // 사물 드로어 (좌표는 Size 384 기준, 중앙 192,192)
        // ─────────────────────────────────────────────

        private static void DrawApple(Color32[] px)
        {
            // 줄기
            FillRect(px, 184, 60, 16, 50, DARKBROWN);
            // 잎
            FillOval(px, 220, 80, 36, 20, GREEN);
            // 몸체
            FillCircle(px, 192, 220, 140, RED);
            // 하이라이트
            FillOval(px, 145, 175, 30, 18, new Color32(255, 180, 180, 200));
        }

        private static void DrawBall(Color32[] px)
        {
            FillCircle(px, 192, 192, 150, RED);
            // 흰색 면 곡선 패턴 (단순화: 가로 밴드)
            FillOval(px, 192, 192, 150, 40, WHITE);
            FillOval(px, 130, 192, 50, 130, WHITE);
            FillOval(px, 254, 192, 50, 130, WHITE);
        }

        private static void DrawSun(Color32[] px)
        {
            // 광선 8개 — 사각형으로 단순화
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4f;
                int cx = 192 + (int)(Mathf.Cos(angle) * 130);
                int cy = 192 + (int)(Mathf.Sin(angle) * 130);
                FillCircle(px, cx, cy, 28, YELLOW);
            }
            FillCircle(px, 192, 192, 110, YELLOW);
            // 눈 + 미소
            FillCircle(px, 165, 175, 10, BLACK);
            FillCircle(px, 219, 175, 10, BLACK);
            FillArc(px, 192, 215, 40, 0.2f, Mathf.PI - 0.2f, 6, BLACK);
        }

        private static void DrawHeart(Color32[] px)
        {
            // 두 원 + 삼각형으로 하트
            FillCircle(px, 145, 160, 75, RED);
            FillCircle(px, 240, 160, 75, RED);
            FillTriangle(px, 76, 175, 308, 175, 192, 320, RED);
        }

        private static void DrawStar(Color32[] px)
        {
            // 5각 별 — 외부 5점 + 내부 5점 polygon
            var pts = new (int x, int y)[10];
            for (int i = 0; i < 10; i++)
            {
                float angle = -Mathf.PI / 2f + i * Mathf.PI / 5f;
                int r = (i % 2 == 0) ? 150 : 65;
                pts[i] = (192 + (int)(Mathf.Cos(angle) * r), 192 + (int)(Mathf.Sin(angle) * r));
            }
            // 폴리곤 = 중앙에서 부채꼴 삼각형 10개
            for (int i = 0; i < 10; i++)
            {
                int j = (i + 1) % 10;
                FillTriangle(px, 192, 192, pts[i].x, pts[i].y, pts[j].x, pts[j].y, YELLOW);
            }
        }

        private static void DrawCup(Color32[] px)
        {
            // 컵 몸체 (사다리꼴 → 사각형으로 단순화)
            FillRect(px, 90, 120, 180, 200, WHITE);
            // 손잡이
            FillCircle(px, 290, 220, 50, WHITE);
            FillCircle(px, 290, 220, 30, GRAY);
            // 받침
            FillRect(px, 70, 320, 220, 16, GRAY);
            // 김
            FillOval(px, 140, 75, 8, 35, GRAY);
            FillOval(px, 192, 60, 8, 40, GRAY);
            FillOval(px, 244, 75, 8, 35, GRAY);
        }

        private static void DrawBalloon(Color32[] px)
        {
            // 풍선
            FillOval(px, 192, 160, 110, 130, RED);
            FillOval(px, 160, 130, 25, 18, new Color32(255, 180, 180, 220));
            // 매듭
            FillTriangle(px, 180, 280, 204, 280, 192, 305, RED);
            // 끈 (지그재그)
            for (int y = 305; y < 360; y += 4)
            {
                int dx = ((y - 305) / 8) % 2 == 0 ? 4 : -4;
                FillRect(px, 192 + dx - 2, y, 4, 4, GRAY);
            }
        }

        private static void DrawFlower(Color32[] px)
        {
            // 5 꽃잎
            for (int i = 0; i < 5; i++)
            {
                float angle = i * 2f * Mathf.PI / 5f - Mathf.PI / 2f;
                int cx = 192 + (int)(Mathf.Cos(angle) * 90);
                int cy = 192 + (int)(Mathf.Sin(angle) * 90);
                FillCircle(px, cx, cy, 60, PINK);
            }
            // 중앙
            FillCircle(px, 192, 192, 50, YELLOW);
            // 줄기
            FillRect(px, 188, 270, 8, 100, DARKGREEN);
            // 잎
            FillOval(px, 165, 320, 25, 12, GREEN);
        }

        private static void DrawHouse(Color32[] px)
        {
            // 몸체
            FillRect(px, 80, 180, 224, 160, new Color32(245, 215, 175, 255));
            // 지붕
            FillTriangle(px, 60, 180, 324, 180, 192, 60, RED);
            // 문
            FillRect(px, 168, 240, 48, 100, BROWN);
            FillCircle(px, 207, 290, 4, GOLD);
            // 창문
            FillRect(px, 100, 210, 50, 50, BLUE);
            FillRect(px, 234, 210, 50, 50, BLUE);
            // 굴뚝
            FillRect(px, 240, 90, 28, 60, BROWN);
        }

        private static void DrawCake(Color32[] px)
        {
            // 3단 케이크
            FillRect(px,  80, 250, 224, 70, new Color32(255, 200, 220, 255)); // 하단
            FillRect(px, 110, 200, 164, 50, new Color32(255, 230, 200, 255)); // 중단
            FillRect(px, 140, 150, 104, 50, new Color32(255, 200, 220, 255)); // 상단
            // 촛불 막대
            FillRect(px, 188, 80, 8, 70, WHITE);
            // 불꽃
            FillOval(px, 192, 60, 12, 22, ORANGE);
            FillOval(px, 192, 65, 6, 12, YELLOW);
            // 장식 점들
            FillCircle(px, 130, 285, 8, RED);
            FillCircle(px, 192, 285, 8, BLUE);
            FillCircle(px, 254, 285, 8, GREEN);
        }

        private static void DrawCar(Color32[] px)
        {
            // 차체 하단 (직사각형)
            FillRect(px, 50, 200, 284, 80, RED);
            // 차체 상단 (지붕)
            FillRect(px, 110, 130, 164, 70, RED);
            // 창문
            FillRect(px, 125, 145, 65, 50, BLUE);
            FillRect(px, 195, 145, 65, 50, BLUE);
            // 바퀴
            FillCircle(px, 100, 290, 40, BLACK);
            FillCircle(px, 100, 290, 18, GRAY);
            FillCircle(px, 284, 290, 40, BLACK);
            FillCircle(px, 284, 290, 18, GRAY);
            // 헤드라이트
            FillCircle(px, 60, 230, 12, YELLOW);
        }

        private static void DrawBoat(Color32[] px)
        {
            // 선체 (사다리꼴 — 두 삼각형 + 사각형)
            FillTriangle(px, 50, 240, 130, 240, 100, 310, BROWN);
            FillRect(px, 100, 240, 184, 70, BROWN);
            FillTriangle(px, 284, 240, 360, 240, 314, 310, BROWN);
            // 돛대
            FillRect(px, 188, 60, 8, 180, DARKBROWN);
            // 돛
            FillTriangle(px, 192, 70, 192, 230, 290, 230, WHITE);
            // 깃발
            FillTriangle(px, 196, 60, 196, 90, 230, 75, RED);
            // 물결
            for (int x = 30; x < 360; x += 30)
                FillArc(px, x, 340, 18, Mathf.PI, Mathf.PI * 2f, 4, BLUE);
        }

        private static void DrawTree(Color32[] px)
        {
            // 줄기
            FillRect(px, 172, 220, 40, 140, DARKBROWN);
            // 잎 (3개 원 합성)
            FillCircle(px, 192, 130, 105, GREEN);
            FillCircle(px, 130, 175, 75, GREEN);
            FillCircle(px, 254, 175, 75, GREEN);
            // 사과
            FillCircle(px, 145, 180, 12, RED);
            FillCircle(px, 240, 165, 12, RED);
        }

        private static void DrawKite(Color32[] px)
        {
            // 다이아몬드
            FillTriangle(px, 192, 60, 90, 192, 192, 220, new Color32(255, 100, 150, 255));
            FillTriangle(px, 192, 60, 294, 192, 192, 220, new Color32(100, 180, 220, 255));
            FillTriangle(px, 90, 192, 192, 320, 192, 220, new Color32(255, 200, 100, 255));
            FillTriangle(px, 294, 192, 192, 320, 192, 220, new Color32(180, 220, 100, 255));
            // 십자 살
            FillRect(px, 191, 60, 2, 260, BLACK);
            FillRect(px, 90, 191, 204, 2, BLACK);
            // 꼬리
            for (int i = 0; i < 5; i++)
                FillTriangle(px, 192 - 8, 320 + i * 20, 192 + 8, 320 + i * 20, 192, 332 + i * 20, RED);
        }

        private static void DrawRocket(Color32[] px)
        {
            // 본체 (둥근 위, 사각 아래)
            FillCircle(px, 192, 130, 55, WHITE);
            FillRect(px, 137, 130, 110, 140, WHITE);
            // 창문
            FillCircle(px, 192, 165, 30, BLUE);
            // 날개
            FillTriangle(px, 137, 200, 137, 290, 90, 310, RED);
            FillTriangle(px, 247, 200, 247, 290, 294, 310, RED);
            // 불꽃
            FillTriangle(px, 160, 270, 224, 270, 192, 350, ORANGE);
            FillTriangle(px, 175, 280, 209, 280, 192, 340, YELLOW);
        }

        private static void DrawCrown(Color32[] px)
        {
            // 띠
            FillRect(px, 70, 220, 244, 70, GOLD);
            // 3개 뾰족
            FillTriangle(px, 70,  220, 130, 220, 100, 110, GOLD);
            FillTriangle(px, 162, 220, 222, 220, 192,  80, GOLD);
            FillTriangle(px, 254, 220, 314, 220, 284, 110, GOLD);
            // 보석
            FillCircle(px, 100, 110, 14, RED);
            FillCircle(px, 192,  80, 16, BLUE);
            FillCircle(px, 284, 110, 14, GREEN);
        }

        private static void DrawDiamond(Color32[] px)
        {
            // 다이아몬드 = 위 사다리꼴 + 아래 삼각형
            FillTriangle(px,  60, 160, 324, 160, 100, 100, BLUE);
            FillTriangle(px, 100, 100, 284, 100, 324, 160, BLUE);
            FillTriangle(px,  60, 160, 324, 160, 192, 340, new Color32(120, 190, 240, 255));
            // 광택 선
            FillRect(px, 100, 100, 184, 4, WHITE);
        }

        private static void DrawSnowflake(Color32[] px)
        {
            // 6개 가지
            FillRect(px, 188, 60, 8, 264, WHITE);
            FillRect(px, 60, 188, 264, 8, WHITE);
            // 대각선 (회전된 사각형 어려우니 점선으로)
            for (int i = -120; i <= 120; i += 8)
            {
                SetPixel(px, 192 + i, 192 + i, WHITE);
                SetPixel(px, 192 + i, 192 - i, WHITE);
                SetPixel(px, 192 + i + 1, 192 + i, WHITE);
                SetPixel(px, 192 + i + 1, 192 - i, WHITE);
                SetPixel(px, 192 + i, 192 + i + 1, WHITE);
                SetPixel(px, 192 + i, 192 - i + 1, WHITE);
            }
            // 중앙 + 가지 끝 장식
            FillCircle(px, 192, 192, 14, WHITE);
            FillCircle(px, 192,  60, 8, WHITE);
            FillCircle(px, 192, 324, 8, WHITE);
            FillCircle(px,  60, 192, 8, WHITE);
            FillCircle(px, 324, 192, 8, WHITE);
        }

        private static void DrawCactus(Color32[] px)
        {
            // 화분
            FillRect(px, 110, 290, 164, 60, BROWN);
            FillRect(px, 100, 280, 184, 20, DARKBROWN);
            // 본체
            FillRect(px, 162, 110, 60, 200, GREEN);
            FillCircle(px, 192, 115, 30, GREEN);
            // 양팔
            FillRect(px, 100, 180, 70, 30, GREEN);
            FillRect(px, 100, 130, 30, 80, GREEN);
            FillCircle(px, 115, 130, 15, GREEN);
            FillRect(px, 214, 200, 70, 30, GREEN);
            FillRect(px, 254, 150, 30, 80, GREEN);
            FillCircle(px, 269, 150, 15, GREEN);
            // 가시
            for (int y = 130; y < 290; y += 25)
            {
                SetPixel(px, 175, y, BLACK); SetPixel(px, 176, y, BLACK);
                SetPixel(px, 209, y, BLACK); SetPixel(px, 210, y, BLACK);
            }
            // 꽃
            FillCircle(px, 192, 105, 18, PINK);
        }

        // ─────────────────────────────────────────────
        // 동물 드로어
        // ─────────────────────────────────────────────

        private static void DrawCat(Color32[] px)
        {
            var fur = ORANGE;
            // 귀
            FillTriangle(px, 80, 130, 130, 50, 165, 130, fur);
            FillTriangle(px, 219, 130, 254, 50, 304, 130, fur);
            FillTriangle(px, 100, 110, 138, 75, 158, 110, PINK); // 귀 안쪽
            FillTriangle(px, 226, 110, 246, 75, 284, 110, PINK);
            // 얼굴
            FillCircle(px, 192, 210, 130, fur);
            // 눈
            FillCircle(px, 145, 195, 16, BLACK);
            FillCircle(px, 239, 195, 16, BLACK);
            FillCircle(px, 148, 192, 5, WHITE);
            FillCircle(px, 242, 192, 5, WHITE);
            // 코
            FillTriangle(px, 180, 235, 204, 235, 192, 252, PINK);
            // 입
            FillArc(px, 178, 260, 18, 0, Mathf.PI, 4, BLACK);
            FillArc(px, 206, 260, 18, 0, Mathf.PI, 4, BLACK);
            // 수염
            FillRect(px, 90, 245, 50, 3, BLACK);
            FillRect(px, 90, 260, 50, 3, BLACK);
            FillRect(px, 244, 245, 50, 3, BLACK);
            FillRect(px, 244, 260, 50, 3, BLACK);
        }

        private static void DrawDog(Color32[] px)
        {
            var fur = BROWN;
            // 늘어진 귀
            FillOval(px, 95, 180, 35, 80, DARKBROWN);
            FillOval(px, 289, 180, 35, 80, DARKBROWN);
            // 얼굴
            FillCircle(px, 192, 200, 130, fur);
            // 주둥이
            FillOval(px, 192, 260, 70, 50, new Color32(220, 180, 130, 255));
            // 코
            FillOval(px, 192, 240, 22, 16, BLACK);
            // 입
            FillArc(px, 192, 280, 30, 0, Mathf.PI, 5, BLACK);
            // 눈
            FillCircle(px, 150, 190, 14, BLACK);
            FillCircle(px, 234, 190, 14, BLACK);
            FillCircle(px, 153, 187, 4, WHITE);
            FillCircle(px, 237, 187, 4, WHITE);
        }

        private static void DrawFish(Color32[] px)
        {
            // 몸통
            FillOval(px, 170, 192, 110, 70, BLUE);
            // 꼬리
            FillTriangle(px, 280, 130, 280, 254, 350, 192, BLUE);
            // 배지느러미
            FillTriangle(px, 150, 250, 200, 250, 175, 290, new Color32(60, 130, 200, 255));
            // 등지느러미
            FillTriangle(px, 150, 130, 200, 130, 175, 90, new Color32(60, 130, 200, 255));
            // 눈
            FillCircle(px, 110, 180, 16, WHITE);
            FillCircle(px, 110, 180, 8, BLACK);
            // 입
            FillArc(px, 80, 200, 14, -0.5f, 0.5f, 3, BLACK);
            // 비늘 점
            FillCircle(px, 200, 175, 6, new Color32(60, 130, 200, 255));
            FillCircle(px, 220, 200, 6, new Color32(60, 130, 200, 255));
            FillCircle(px, 200, 220, 6, new Color32(60, 130, 200, 255));
        }

        private static void DrawRabbit(Color32[] px)
        {
            // 긴 귀 2개
            FillOval(px, 155, 80, 22, 70, WHITE);
            FillOval(px, 229, 80, 22, 70, WHITE);
            FillOval(px, 155, 80, 12, 50, PINK);
            FillOval(px, 229, 80, 12, 50, PINK);
            // 얼굴
            FillCircle(px, 192, 220, 110, WHITE);
            // 눈
            FillCircle(px, 158, 210, 12, BLACK);
            FillCircle(px, 226, 210, 12, BLACK);
            // 코
            FillTriangle(px, 184, 240, 200, 240, 192, 252, PINK);
            // 입
            FillArc(px, 178, 258, 14, 0, Mathf.PI, 3, BLACK);
            FillArc(px, 206, 258, 14, 0, Mathf.PI, 3, BLACK);
            // 이빨
            FillRect(px, 188, 268, 4, 12, WHITE);
            FillRect(px, 192, 268, 4, 12, WHITE);
        }

        private static void DrawOwl(Color32[] px)
        {
            var body = BROWN;
            // 몸통
            FillOval(px, 192, 200, 130, 150, body);
            // 배 (밝은 색)
            FillOval(px, 192, 230, 80, 100, new Color32(220, 180, 140, 255));
            // 큰 눈 흰자
            FillCircle(px, 145, 165, 50, WHITE);
            FillCircle(px, 239, 165, 50, WHITE);
            // 눈동자
            FillCircle(px, 145, 165, 25, BLACK);
            FillCircle(px, 239, 165, 25, BLACK);
            FillCircle(px, 152, 158, 8, WHITE);
            FillCircle(px, 246, 158, 8, WHITE);
            // 부리
            FillTriangle(px, 180, 200, 204, 200, 192, 230, ORANGE);
            // 귀깃
            FillTriangle(px,  90, 100, 130, 100, 110,  60, body);
            FillTriangle(px, 254, 100, 294, 100, 274,  60, body);
            // 발
            FillTriangle(px, 160, 320, 180, 320, 170, 340, ORANGE);
            FillTriangle(px, 204, 320, 224, 320, 214, 340, ORANGE);
        }

        private static void DrawButterfly(Color32[] px)
        {
            // 몸통
            FillOval(px, 192, 192, 8, 90, DARKGRAY);
            // 머리
            FillCircle(px, 192, 110, 14, DARKGRAY);
            // 더듬이
            FillRect(px, 180, 80, 2, 25, DARKGRAY);
            FillRect(px, 202, 80, 2, 25, DARKGRAY);
            FillCircle(px, 178, 80, 4, DARKGRAY);
            FillCircle(px, 206, 80, 4, DARKGRAY);
            // 윗날개
            FillOval(px, 110, 160, 70, 60, PURPLE);
            FillOval(px, 274, 160, 70, 60, PURPLE);
            // 아랫날개
            FillOval(px, 130, 240, 60, 50, PINK);
            FillOval(px, 254, 240, 60, 50, PINK);
            // 점 무늬
            FillCircle(px, 100, 160, 12, YELLOW);
            FillCircle(px, 284, 160, 12, YELLOW);
            FillCircle(px, 130, 240, 8, WHITE);
            FillCircle(px, 254, 240, 8, WHITE);
        }

        private static void DrawPig(Color32[] px)
        {
            // 얼굴
            FillCircle(px, 192, 210, 130, PINK);
            // 귀 (위 작은 삼각)
            FillTriangle(px, 105, 145, 150, 100, 150, 150, PINK);
            FillTriangle(px, 234, 150, 234, 100, 279, 145, PINK);
            // 코 큰 타원
            FillOval(px, 192, 235, 50, 35, new Color32(255, 170, 200, 255));
            // 콧구멍
            FillCircle(px, 178, 235, 6, BLACK);
            FillCircle(px, 206, 235, 6, BLACK);
            // 눈
            FillCircle(px, 145, 185, 12, BLACK);
            FillCircle(px, 239, 185, 12, BLACK);
            // 입
            FillArc(px, 192, 275, 22, 0, Mathf.PI, 4, DARKGRAY);
        }

        private static void DrawFrog(Color32[] px)
        {
            // 몸통
            FillCircle(px, 192, 220, 130, GREEN);
            // 배
            FillOval(px, 192, 260, 80, 60, new Color32(180, 230, 150, 255));
            // 눈 — 큰 흰 원 (몸체 위로 튀어나옴)
            FillCircle(px, 130, 130, 35, GREEN);
            FillCircle(px, 254, 130, 35, GREEN);
            FillCircle(px, 130, 130, 25, WHITE);
            FillCircle(px, 254, 130, 25, WHITE);
            FillCircle(px, 130, 130, 12, BLACK);
            FillCircle(px, 254, 130, 12, BLACK);
            // 입
            FillArc(px, 192, 240, 60, 0, Mathf.PI, 6, DARKGRAY);
        }

        private static void DrawDuck(Color32[] px)
        {
            var fur = YELLOW;
            // 몸통
            FillOval(px, 200, 230, 120, 90, fur);
            // 머리
            FillCircle(px, 130, 160, 75, fur);
            // 부리
            FillOval(px, 70, 165, 35, 18, ORANGE);
            // 눈
            FillCircle(px, 135, 145, 10, BLACK);
            FillCircle(px, 137, 143, 3, WHITE);
            // 꼬리
            FillTriangle(px, 290, 220, 340, 200, 320, 270, fur);
            // 발
            FillTriangle(px, 175, 320, 195, 320, 180, 345, ORANGE);
            FillTriangle(px, 215, 320, 235, 320, 220, 345, ORANGE);
        }

        private static void DrawChick(Color32[] px)
        {
            // 동그란 몸
            FillCircle(px, 192, 220, 110, YELLOW);
            // 머리
            FillCircle(px, 192, 130, 75, YELLOW);
            // 부리
            FillTriangle(px, 175, 135, 209, 135, 192, 160, ORANGE);
            // 눈
            FillCircle(px, 165, 115, 8, BLACK);
            FillCircle(px, 219, 115, 8, BLACK);
            // 발
            FillTriangle(px, 165, 320, 195, 320, 180, 345, ORANGE);
            FillTriangle(px, 200, 320, 230, 320, 215, 345, ORANGE);
            // 작은 날개
            FillOval(px, 110, 230, 30, 50, new Color32(240, 200, 60, 255));
            FillOval(px, 274, 230, 30, 50, new Color32(240, 200, 60, 255));
        }

        private static void DrawBear(Color32[] px)
        {
            var fur = BROWN;
            // 귀
            FillCircle(px,  90, 120, 38, fur);
            FillCircle(px, 294, 120, 38, fur);
            FillCircle(px,  90, 120, 22, new Color32(220, 180, 140, 255));
            FillCircle(px, 294, 120, 22, new Color32(220, 180, 140, 255));
            // 얼굴
            FillCircle(px, 192, 210, 130, fur);
            // 주둥이 부분
            FillOval(px, 192, 245, 70, 50, new Color32(220, 180, 140, 255));
            // 코
            FillOval(px, 192, 225, 25, 18, BLACK);
            // 눈
            FillCircle(px, 150, 190, 12, BLACK);
            FillCircle(px, 234, 190, 12, BLACK);
            // 입
            FillArc(px, 192, 275, 24, 0, Mathf.PI, 5, BLACK);
        }

        // ─────────────────────────────────────────────
        // 픽셀 프리미티브
        // ─────────────────────────────────────────────

        private static void SetPixel(Color32[] px, int x, int y, Color32 c)
        {
            if ((uint)x >= Size || (uint)y >= Size) return;
            px[y * Size + x] = c;
        }

        private static void FillCircle(Color32[] px, int cx, int cy, int r, Color32 c)
        {
            int r2 = r * r;
            int xMin = Mathf.Max(0, cx - r);
            int xMax = Mathf.Min(Size - 1, cx + r);
            int yMin = Mathf.Max(0, cy - r);
            int yMax = Mathf.Min(Size - 1, cy + r);
            for (int y = yMin; y <= yMax; y++)
                for (int x = xMin; x <= xMax; x++)
                {
                    int dx = x - cx, dy = y - cy;
                    if (dx * dx + dy * dy <= r2) px[y * Size + x] = c;
                }
        }

        private static void FillOval(Color32[] px, int cx, int cy, int rx, int ry, Color32 c)
        {
            int xMin = Mathf.Max(0, cx - rx);
            int xMax = Mathf.Min(Size - 1, cx + rx);
            int yMin = Mathf.Max(0, cy - ry);
            int yMax = Mathf.Min(Size - 1, cy + ry);
            float rx2 = rx * rx, ry2 = ry * ry;
            for (int y = yMin; y <= yMax; y++)
                for (int x = xMin; x <= xMax; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    if ((dx * dx) / rx2 + (dy * dy) / ry2 <= 1f) px[y * Size + x] = c;
                }
        }

        private static void FillRect(Color32[] px, int x, int y, int w, int h, Color32 c)
        {
            int xMin = Mathf.Max(0, x);
            int yMin = Mathf.Max(0, y);
            int xMax = Mathf.Min(Size, x + w);
            int yMax = Mathf.Min(Size, y + h);
            for (int yy = yMin; yy < yMax; yy++)
                for (int xx = xMin; xx < xMax; xx++)
                    px[yy * Size + xx] = c;
        }

        // 삼각형 채우기 — barycentric coordinate test
        private static void FillTriangle(Color32[] px,
            int x1, int y1, int x2, int y2, int x3, int y3, Color32 c)
        {
            int xMin = Mathf.Max(0, Mathf.Min(x1, Mathf.Min(x2, x3)));
            int xMax = Mathf.Min(Size - 1, Mathf.Max(x1, Mathf.Max(x2, x3)));
            int yMin = Mathf.Max(0, Mathf.Min(y1, Mathf.Min(y2, y3)));
            int yMax = Mathf.Min(Size - 1, Mathf.Max(y1, Mathf.Max(y2, y3)));

            int denom = (y2 - y3) * (x1 - x3) + (x3 - x2) * (y1 - y3);
            if (denom == 0) return;

            for (int y = yMin; y <= yMax; y++)
                for (int x = xMin; x <= xMax; x++)
                {
                    float a = ((y2 - y3) * (x - x3) + (x3 - x2) * (y - y3)) / (float)denom;
                    float b = ((y3 - y1) * (x - x3) + (x1 - x3) * (y - y3)) / (float)denom;
                    float g = 1f - a - b;
                    if (a >= 0f && b >= 0f && g >= 0f) px[y * Size + x] = c;
                }
        }

        // 호 (arc) — 굵기 thickness, 각도 from~to
        private static void FillArc(Color32[] px, int cx, int cy, int radius,
            float angleFrom, float angleTo, int thickness, Color32 c)
        {
            int rOuter = radius;
            int rInner = Mathf.Max(0, radius - thickness);
            int rOuter2 = rOuter * rOuter;
            int rInner2 = rInner * rInner;
            int xMin = Mathf.Max(0, cx - rOuter);
            int xMax = Mathf.Min(Size - 1, cx + rOuter);
            int yMin = Mathf.Max(0, cy - rOuter);
            int yMax = Mathf.Min(Size - 1, cy + rOuter);

            for (int y = yMin; y <= yMax; y++)
                for (int x = xMin; x <= xMax; x++)
                {
                    int dx = x - cx, dy = y - cy;
                    int d2 = dx * dx + dy * dy;
                    if (d2 > rOuter2 || d2 < rInner2) continue;
                    float ang = Mathf.Atan2(dy, dx);
                    if (ang >= angleFrom && ang <= angleTo) px[y * Size + x] = c;
                }
        }

        // ─────────────────────────────────────────────
        // 키 정규화
        // ─────────────────────────────────────────────

        private static string Normalize(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            string s = raw.ToLowerInvariant();
            if (s.StartsWith("char_")) s = s.Substring(5);
            return s;
        }
    }
}
