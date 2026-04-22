// TC-DS-001 ~ TC-DS-004 : 드래그 → 스냅 전체 흐름 (PlayMode)
// ※ 전제: "Gameplay" 씬이 Build Settings에 등록되어 있어야 함
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using PuzzlePals.Puzzle;

namespace PuzzlePals.Tests.PlayMode
{
    public class DragSnapFlowTests
    {
        private const string GameplayScene = "Gameplay";

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(GameplayScene);
            yield return null; // 한 프레임 대기
        }

        // TC-DS-001: 조각을 드래그하면 터치 위치를 따라 이동한다
        [UnityTest]
        public IEnumerator TC_DS_001_DragPiece_FollowsPointerPosition()
        {
            var piece = Object.FindAnyObjectByType<PuzzlePiece>();
            Assert.IsNotNull(piece, "PuzzlePiece not found in scene");

            Vector3 startPos = piece.transform.position;

            // PointerDown 시뮬레이션
            SimulatePointerDown(piece.gameObject);
            yield return null;

            // 새 위치로 PointerMove 시뮬레이션 (월드 → 스크린)
            Vector3 targetWorld = startPos + new Vector3(1f, 1f, 0f);
            SimulatePointerDrag(piece.gameObject, targetWorld);
            yield return null;

            // 조각이 움직였어야 한다 — AreNotEqual에는 delta 오버로드가 없으므로 Assert.That 사용
            Assert.That(piece.transform.position.x,
                Is.Not.EqualTo(startPos.x).Within(0.01f),
                "Piece should follow the drag gesture");
        }

        // TC-DS-002: 스냅존 근처(1.5f)에서 손을 떼면 스냅된다
        [UnityTest]
        public IEnumerator TC_DS_002_DropNearSnapZone_Snaps()
        {
            var piece = Object.FindAnyObjectByType<PuzzlePiece>();
            Assert.IsNotNull(piece, "PuzzlePiece not found");
            // piece의 OnPointerUp은 AssignedSnapZone 으로만 시도하므로 그 zone을 사용
            var zone  = piece.AssignedSnapZone;
            Assert.IsNotNull(zone, "piece.AssignedSnapZone is null — run PuzzlePals > Setup Gameplay first");

            float threshold = piece.SnapThreshold;

            // 조각을 스냅존에서 threshold 미만 거리에 배치
            piece.transform.position = zone.transform.position + new Vector3(threshold * 0.8f, 0f, 0f);
            piece.OriginalPosition   = piece.transform.position;

            SimulatePointerUp(piece.gameObject);
            yield return null;

            Assert.IsTrue(piece.IsSnapped, "Piece should snap when dropped within threshold");
            Assert.AreEqual(zone.transform.position.x, piece.transform.position.x, 0.01f);
        }

        // TC-DS-003: 스냅존 밖에서 손을 떼면 놓은 자리에 그대로 머무른다
        // (성인 피벗 v0.4 — 기존 "원위치 복귀" 폐기. 조각은 보드 위 어디든 자유 배치 가능)
        [UnityTest]
        public IEnumerator TC_DS_003_DropFarFromSnapZone_StaysWhereDropped()
        {
            var piece = Object.FindAnyObjectByType<PuzzlePiece>();
            Assert.IsNotNull(piece);
            var zone  = piece.AssignedSnapZone;
            Assert.IsNotNull(zone, "piece.AssignedSnapZone is null — run PuzzlePals > Setup Gameplay first");

            Vector3 originalPos = new Vector3(-5f, -3f, 0f);
            Vector3 droppedPos  = zone.transform.position + new Vector3(2.5f, 0f, 0f);
            piece.OriginalPosition   = originalPos;
            piece.transform.position = droppedPos;

            SimulatePointerUp(piece.gameObject);
            yield return null;

            Assert.IsFalse(piece.IsSnapped, "스냅존 밖 거리이므로 스냅되면 안 됨");
            Assert.AreEqual(droppedPos.x, piece.transform.position.x, 0.01f,
                "snap 실패 시 조각은 놓은 자리에 머물러야 함 (원위치 복귀 X)");
            Assert.AreEqual(droppedPos.y, piece.transform.position.y, 0.01f);
        }

        // TC-DS-004: 마지막 조각 스냅 후 1.5초 뒤 CompleteUI가 활성화된다
        [UnityTest]
        public IEnumerator TC_DS_004_LastPieceSnapped_CompleteUIShown_After1_5s()
        {
            var pieces = Object.FindObjectsByType<PuzzlePiece>(FindObjectsInactive.Exclude);
            Assert.IsTrue(pieces.Length >= 2, "Need at least 2 pieces");

            // 비활성 GameObject도 찾기 위해 Transform 전체 검색
            var completeUI = FindByNameIncludeInactive("CompleteUI");
            Assert.IsNotNull(completeUI, "CompleteUI GameObject must exist in scene");
            completeUI.SetActive(false);

            // 첫 번째 조각을 제외한 모든 조각 강제 스냅
            for (int i = 1; i < pieces.Length; i++)
                pieces[i].SetSnappedForTest(true);

            // 마지막 조각(pieces[0])을 자신의 AssignedSnapZone 근처에 배치 후 스냅
            var zone = pieces[0].AssignedSnapZone;
            Assert.IsNotNull(zone, "pieces[0].AssignedSnapZone is null");
            pieces[0].transform.position = zone.transform.position + Vector3.right * 0.5f;
            pieces[0].OriginalPosition   = pieces[0].transform.position;
            SimulatePointerUp(pieces[0].gameObject);

            // 1.5초 딜레이 대기
            yield return new WaitForSeconds(1.6f);

            Assert.IsTrue(completeUI.activeSelf, "CompleteUI should be active after last piece snaps");
        }

        private static GameObject FindByNameIncludeInactive(string name)
        {
            var transforms = Object.FindObjectsByType<Transform>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in transforms)
                if (t.name == name) return t.gameObject;
            return null;
        }

        // ─────────────────────────────────────────────────────
        // 입력 시뮬레이션 헬퍼
        // ─────────────────────────────────────────────────────

        private static void SimulatePointerDown(GameObject target)
        {
            var handler = target.GetComponent<IPointerDownHandler>();
            handler?.OnPointerDown(BuildPointerEvent(target.transform.position));
        }

        private static void SimulatePointerDrag(GameObject target, Vector3 worldPos)
        {
            var handler = target.GetComponent<IDragHandler>();
            handler?.OnDrag(BuildPointerEvent(worldPos));
        }

        private static void SimulatePointerUp(GameObject target)
        {
            var handler = target.GetComponent<IPointerUpHandler>();
            handler?.OnPointerUp(BuildPointerEvent(target.transform.position));
        }

        private static PointerEventData BuildPointerEvent(Vector3 worldPos)
        {
            var cam = Camera.main;

            // Vector3/Vector2 암묵적 변환 충돌 방지 — 명시적으로 Vector2 변환
            Vector2 screenPos = cam != null
                ? (Vector2)cam.WorldToScreenPoint(worldPos)
                : new Vector2(worldPos.x, worldPos.y);

            return new PointerEventData(EventSystem.current)
            {
                position = screenPos,
            };
        }
    }
}
