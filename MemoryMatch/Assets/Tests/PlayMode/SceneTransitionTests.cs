// TC-ST-001 ~ TC-ST-004 : 씬 전환 (PlayMode) — MyGames(Puzzle Pals) 의 SceneTransitionTests 패턴 이식.
// ※ 전제: MainMenu, LevelSelect, Gameplay, Complete 씬이 Build Settings 에 등록되어 있어야 함
// 이 프로젝트는 "LevelSelect 가 실질적 허브" 컨벤션이라, Gameplay 뒤로가기가 MainMenu 가 아니라
// LevelSelect 로 가는 게 정상.
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MemoryMatch.Core;

namespace MemoryMatch.Tests.PlayMode
{
    public class SceneTransitionTests
    {
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

        // TC-ST-001: MainMenu PlayButton 클릭 → LevelSelect 씬
        [UnityTest]
        public IEnumerator TC_ST_001_MainMenu_PlayButton_LoadsLevelSelect()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return null;

            var playBtn = FindButtonByName("PlayButton");
            Assert.IsNotNull(playBtn, "PlayButton not found in MainMenu");

            playBtn.onClick.Invoke();
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual("LevelSelect", SceneManager.GetActiveScene().name);
        }

        // TC-ST-002: LevelSelect 첫 번째 타일 클릭 → Gameplay 씬, currentLevelIndex == 0
        [UnityTest]
        public IEnumerator TC_ST_002_LevelSelect_Tile01_LoadsGameplay()
        {
            yield return SceneManager.LoadSceneAsync("LevelSelect");
            yield return null;

            var tileBtn = FindButtonByName("Tile_01");
            Assert.IsNotNull(tileBtn, "Tile_01 not found in LevelSelect");

            tileBtn.onClick.Invoke();
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual("Gameplay", SceneManager.GetActiveScene().name);
            Assert.AreEqual(0, LevelManager.Instance.currentLevelIndex);
        }

        // TC-ST-003: Gameplay 뒤로가기(Back) 클릭 → LevelSelect
        [UnityTest]
        public IEnumerator TC_ST_003_Gameplay_BackButton_LoadsLevelSelect()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;

            var backBtn = FindButtonByName("Back");
            Assert.IsNotNull(backBtn, "Back button not found in Gameplay header");

            backBtn.onClick.Invoke();
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual("LevelSelect", SceneManager.GetActiveScene().name);
        }

        // TC-ST-004: Complete NextButton 클릭 → 다음 레벨 Gameplay, currentLevelIndex 증가
        [UnityTest]
        public IEnumerator TC_ST_004_Complete_NextButton_LoadsNextLevel()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            if (LevelManager.Instance != null) LevelManager.Instance.currentLevelIndex = 0;

            yield return SceneManager.LoadSceneAsync("Complete");
            yield return null;

            var nextBtn = FindButtonByName("NextButton");
            Assert.IsNotNull(nextBtn, "NextButton not found in Complete scene");

            nextBtn.onClick.Invoke();
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual("Gameplay", SceneManager.GetActiveScene().name);
            Assert.AreEqual(1, LevelManager.Instance.currentLevelIndex);
        }

        private static Button FindButtonByName(string name)
        {
            var go = GameObject.Find(name);
            return go != null ? go.GetComponent<Button>() : null;
        }
    }
}
