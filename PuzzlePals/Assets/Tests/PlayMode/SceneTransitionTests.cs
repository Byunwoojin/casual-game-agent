// TC-ST-001 ~ TC-ST-004 : 씬 전환 (PlayMode)
// ※ 전제: MainMenu, LevelSelect, Gameplay, Complete 씬이 Build Settings에 등록되어 있어야 함
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PuzzlePals.Core;

namespace PuzzlePals.Tests.PlayMode
{
    public class SceneTransitionTests
    {
        // SceneFader 의 비동기 페이드(0.4s)가 테스트의 0.5s 대기와 타이밍 경합 가능 — 테스트 동안 즉시 로드
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneLoader.UseFader = false;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SceneLoader.UseFader = true;
            yield return null;
        }

        // TC-ST-001: MainMenu Play 버튼 클릭 → LevelSelect 씬
        [UnityTest]
        public IEnumerator TC_ST_001_MainMenu_PlayButton_LoadsLevelSelect()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return null;

            var playBtn = FindButtonByName("PlayButton");
            Assert.IsNotNull(playBtn, "PlayButton not found in MainMenu");

            playBtn.onClick.Invoke();
            yield return new WaitForSeconds(0.5f); // 씬 전환 대기

            Assert.AreEqual("LevelSelect", SceneManager.GetActiveScene().name,
                "Should load LevelSelect after clicking Play");
        }

        // TC-ST-002: LevelSelect 레벨 버튼 클릭 → Gameplay 씬, levelIndex == 0
        // Full Grid 셋업의 첫 번째 버튼은 LevelButton_001 (D3 포맷, level.levelNumber 기반)
        [UnityTest]
        public IEnumerator TC_ST_002_LevelSelect_Level1Button_LoadsGameplay()
        {
            yield return SceneManager.LoadSceneAsync("LevelSelect");
            yield return null;

            var levelBtn = FindButtonByName("LevelButton_001");
            Assert.IsNotNull(levelBtn, "LevelButton_001 not found in LevelSelect (Full Grid)");

            levelBtn.onClick.Invoke();
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual("Gameplay", SceneManager.GetActiveScene().name,
                "Should load Gameplay after clicking level button");

            Assert.AreEqual(0, LevelManager.Instance.currentLevelIndex,
                "currentLevelIndex should be 0 after selecting level 1");
        }

        // TC-ST-003: Gameplay 홈 버튼 클릭 → MainMenu 씬
        [UnityTest]
        public IEnumerator TC_ST_003_Gameplay_HomeButton_LoadsMainMenu()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;

            var homeBtn = FindButtonByName("HomeButton");
            Assert.IsNotNull(homeBtn, "HomeButton not found in Gameplay");

            homeBtn.onClick.Invoke();
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual("MainMenu", SceneManager.GetActiveScene().name,
                "Should load MainMenu after clicking Home");
        }

        // TC-ST-004: Complete 씬 다음 버튼 클릭 → 다음 레벨 Gameplay, levelIndex == 1
        [UnityTest]
        public IEnumerator TC_ST_004_Complete_NextButton_LoadsNextLevel()
        {
            // LevelManager 사전 설정
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            if (LevelManager.Instance != null)
                LevelManager.Instance.currentLevelIndex = 0;

            yield return SceneManager.LoadSceneAsync("Complete");
            yield return null;

            var nextBtn = FindButtonByName("NextButton");
            Assert.IsNotNull(nextBtn, "NextButton not found in Complete scene");

            nextBtn.onClick.Invoke();
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual("Gameplay", SceneManager.GetActiveScene().name,
                "Should load Gameplay after clicking Next");
            Assert.AreEqual(1, LevelManager.Instance.currentLevelIndex,
                "currentLevelIndex should advance to 1");
        }

        // ── 헬퍼 ─────────────────────────────────────────────

        private static Button FindButtonByName(string name)
        {
            var go = GameObject.Find(name);
            return go != null ? go.GetComponent<Button>() : null;
        }
    }
}
