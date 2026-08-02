// TC-GF-001 ~ TC-GF-002 : 실제 탭 → 슬라이드 이동 흐름 (PlayMode E2E)
// 셔플이 랜덤이라 미리 정해진 타일을 클릭할 수 없으므로, SlidingBoard.DebugGetAdjacentPieceIndex() /
// DebugIsPieceAdjacentToBlank() 로 "지금" 빈 칸과 인접/비인접인 타일을 런타임에 질의해서 사용한다.
// TileClickTarget.OnPointerClick 은 클릭 좌표를 보지 않으므로(스크린 좌표 무관, GameObject 자체가
// 판정 대상), 좌표 역산 없이 바로 IPointerClickHandler 를 태울 수 있다.
// ※ 전제: Gameplay 씬이 Build Settings 에 등록되어 있고, Resources/Levels/Level_001 에셋이 존재해야 함.
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using SlidingPuzzle.Gameplay;

namespace SlidingPuzzle.Tests.PlayMode
{
    public class GameplayFlowTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            yield return null; // GameplaySceneController.Start() 가 board.Build() 호출할 시간
        }

        // TC-GF-001: 빈 칸과 인접한 타일을 탭하면 그 타일의 화면상 위치가 바뀐다(스왑 성공)
        [UnityTest]
        public IEnumerator TC_GF_001_TapAdjacentTile_MovesToNewPosition()
        {
            var board = Object.FindAnyObjectByType<SlidingBoard>();
            Assert.IsNotNull(board, "SlidingBoard not found");
            Assert.Greater(board.TotalCount, 0);

            int pieceIdx = board.DebugGetAdjacentPieceIndex();
            Assert.GreaterOrEqual(pieceIdx, 0, "빈 칸에 인접한 조각은 항상 존재해야 함(셔플 불변식)");

            var tileGO = board.DebugGetTileGO(pieceIdx);
            Assert.IsNotNull(tileGO);
            var rt = (RectTransform)tileGO.transform;
            Vector2 before = rt.anchoredPosition;

            SimulateClick(tileGO);
            yield return new WaitForSeconds(0.3f); // 슬라이드 트윈(0.15s) 완료 대기

            Assert.AreNotEqual(before, rt.anchoredPosition, "인접 타일을 탭하면 빈 칸 위치로 이동해야 함");
        }

        // TC-GF-002: 빈 칸과 인접하지 않은 타일을 탭하면 아무 일도 일어나지 않는다
        [UnityTest]
        public IEnumerator TC_GF_002_TapNonAdjacentTile_DoesNotMove()
        {
            var board = Object.FindAnyObjectByType<SlidingBoard>();
            Assert.IsNotNull(board);

            int nonAdjacent = -1;
            for (int i = 0; i < board.TotalCount; i++)
            {
                if (!board.DebugIsPieceAdjacentToBlank(i)) { nonAdjacent = i; break; }
            }
            Assert.GreaterOrEqual(nonAdjacent, 0, "비인접 조각을 찾지 못함(그리드가 너무 작은가?)");

            var tileGO = board.DebugGetTileGO(nonAdjacent);
            Assert.IsNotNull(tileGO);
            var rt = (RectTransform)tileGO.transform;
            Vector2 before = rt.anchoredPosition;
            int beforeFound = board.FoundCount;

            SimulateClick(tileGO);
            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(before, rt.anchoredPosition, "인접하지 않은 타일은 탭해도 움직이면 안 됨");
            Assert.AreEqual(beforeFound, board.FoundCount, "무효 탭은 FoundCount 에 영향을 주면 안 됨");
        }

        private static void SimulateClick(GameObject target)
        {
            var handler = target.GetComponent<IPointerClickHandler>();
            Assert.IsNotNull(handler, $"{target.name} 에 IPointerClickHandler 가 없음");
            handler.OnPointerClick(new PointerEventData(EventSystem.current));
        }
    }
}
