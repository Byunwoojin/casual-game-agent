// TC-GF-001 ~ TC-GF-002 : 실제 탭 → 판정 흐름 (PlayMode E2E)
// DiffGameBoard.DebugGetScreenPositionForRegion() 로 diff_result.json 기반 결정적 좌표를 화면 좌표로
// 역산해서, 실제 사용자가 탭했을 때와 동일한 경로(IPointerClickHandler)를 태운다.
// ※ 전제: Gameplay 씬이 Build Settings 에 등록되어 있고, Resources/Levels/Level_001 에셋이 존재해야 함.
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using SpotDiff.Gameplay;

namespace SpotDiff.Tests.PlayMode
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

        // TC-GF-001: 정답 위치(diffRegions[0] 중심)를 탭하면 FoundCount 가 1 증가한다
        [UnityTest]
        public IEnumerator TC_GF_001_TapCorrectRegion_IncreasesFoundCount()
        {
            var board = Object.FindAnyObjectByType<DiffGameBoard>();
            Assert.IsNotNull(board, "DiffGameBoard not found");
            Assert.IsNotNull(board.Level, "Level 이 로드되지 않음 — Resources/Levels/Level_001 확인 필요");
            Assert.Greater(board.Level.diffRegions.Length, 0);

            int before = board.FoundCount;
            var screenPos = board.DebugGetScreenPositionForRegion(0);
            SimulateClick(board.DebugTransformedImageGO, screenPos);
            yield return null;

            Assert.AreEqual(before + 1, board.FoundCount, "정답 위치 탭 시 FoundCount 가 1 증가해야 함");
        }

        // TC-GF-002: 이미지 바깥(letterbox 영역)을 탭하면 판정에 아무 영향이 없어야 한다
        [UnityTest]
        public IEnumerator TC_GF_002_TapOutsideImage_DoesNotAffectFoundCount()
        {
            var board = Object.FindAnyObjectByType<DiffGameBoard>();
            Assert.IsNotNull(board);

            int before = board.FoundCount;
            var screenPos = new Vector2(-500f, -500f); // 화면/이미지 영역 완전히 밖
            SimulateClick(board.DebugTransformedImageGO, screenPos);
            yield return null;

            Assert.AreEqual(before, board.FoundCount, "이미지 바깥 탭은 FoundCount 에 영향을 주면 안 됨");
        }

        private static void SimulateClick(GameObject target, Vector2 screenPos)
        {
            Assert.IsNotNull(target, "탭 대상 GameObject 가 null");
            var handler = target.GetComponent<IPointerClickHandler>();
            Assert.IsNotNull(handler, $"{target.name} 에 IPointerClickHandler 가 없음");
            handler.OnPointerClick(new PointerEventData(EventSystem.current) { position = screenPos });
        }
    }
}
