// TC-GF-001 ~ TC-GF-002 : 실제 탭 → 매칭 판정 흐름 (PlayMode E2E)
// 셔플이 랜덤이라 미리 정해진 카드를 클릭할 수 없으므로, MatchBoard.DebugFindMatchingPair() /
// DebugPairIdAt() 로 "지금" 상태 기준 유효한 짝/불일치 카드를 런타임에 질의해서 사용한다.
// CardClickTarget.OnPointerClick 은 클릭 좌표를 보지 않으므로(스크린 좌표 무관, GameObject 자체가
// 판정 대상), 좌표 역산 없이 바로 IPointerClickHandler 를 태울 수 있다.
// ※ 전제: Gameplay 씬이 Build Settings 에 등록되어 있고, Resources/Levels/Level_001 에셋이 존재해야 함.
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using MemoryMatch.Gameplay;

namespace MemoryMatch.Tests.PlayMode
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

        // TC-GF-001: 짝이 맞는 두 카드를 연달아 탭하면 FoundCount 가 1 증가한다
        [UnityTest]
        public IEnumerator TC_GF_001_TapMatchingPair_IncreasesFoundCount()
        {
            var board = Object.FindAnyObjectByType<MatchBoard>();
            Assert.IsNotNull(board, "MatchBoard not found");
            Assert.Greater(board.TotalCount, 0);

            var pair = board.DebugFindMatchingPair();
            Assert.IsTrue(pair.HasValue, "짝이 되는 카드 쌍을 찾지 못함");

            int before = board.FoundCount;
            SimulateClick(board.DebugGetCardGO(pair.Value.first));
            yield return null;
            SimulateClick(board.DebugGetCardGO(pair.Value.second));
            yield return null;

            Assert.AreEqual(before + 1, board.FoundCount, "짝이 맞는 두 카드를 연달아 탭하면 FoundCount 가 1 증가해야 함");
        }

        // TC-GF-002: 짝이 다른 두 카드를 탭하면 FoundCount 가 늘지 않고, 잠시 후 자동으로 다시 가려진다
        [UnityTest]
        public IEnumerator TC_GF_002_TapMismatchedPair_DoesNotIncreaseFoundCount()
        {
            var board = Object.FindAnyObjectByType<MatchBoard>();
            Assert.IsNotNull(board);

            var pair = board.DebugFindMatchingPair();
            Assert.IsTrue(pair.HasValue);
            int a = pair.Value.first;
            int b = -1;
            int cardCount = board.TotalCount * 2;
            for (int i = 0; i < cardCount; i++)
            {
                if (i == a || i == pair.Value.second) continue;
                if (board.DebugPairIdAt(i) != board.DebugPairIdAt(a)) { b = i; break; }
            }
            Assert.GreaterOrEqual(b, 0, "짝이 다른 카드를 찾지 못함(pairCount 가 너무 작은가?)");

            int before = board.FoundCount;
            SimulateClick(board.DebugGetCardGO(a));
            yield return null;
            SimulateClick(board.DebugGetCardGO(b));
            yield return null;

            Assert.AreEqual(before, board.FoundCount, "짝이 다른 카드는 즉시 FoundCount 를 늘리면 안 됨");

            yield return new WaitForSeconds(1.0f); // MismatchDelay(0.7s) 이후 자동으로 다시 가려짐
            Assert.AreEqual(before, board.FoundCount, "불일치 해소 후에도 FoundCount 는 그대로여야 함");
        }

        private static void SimulateClick(GameObject target)
        {
            Assert.IsNotNull(target, "탭 대상 GameObject 가 null");
            var handler = target.GetComponent<IPointerClickHandler>();
            Assert.IsNotNull(handler, $"{target.name} 에 IPointerClickHandler 가 없음");
            handler.OnPointerClick(new PointerEventData(EventSystem.current));
        }
    }
}
