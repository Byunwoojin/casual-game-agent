using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using PuzzlePals.Systems;

namespace PuzzlePals.EditorTools
{
    /// <summary>
    /// FX(파티클 + 사운드) 매니저 자동 세팅 도구.
    /// - Assets/VFX/ 에 SnapBurst.prefab, CompleteBurst.prefab 생성 (없으면)
    /// - Gameplay 씬에 AudioManager / VFXManager GameObject 부착 + 프리팹 와이어링
    /// 오디오 클립은 비워 둠 (사용자가 직접 할당).
    ///
    /// 사용법: Unity 상단 메뉴 → [PuzzlePals] → [Setup FX (Managers + Placeholder Particles)]
    /// </summary>
    public static class FXSetupTool
    {
        private const string VFXFolder         = "Assets/VFX";
        private const string SnapBurstPath     = "Assets/VFX/SnapBurst.prefab";
        private const string CompleteBurstPath = "Assets/VFX/CompleteBurst.prefab";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("PuzzlePals/Setup FX (Managers + Placeholder Particles)")]
        public static void SetupFX()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Play Mode 종료 필요",
                    "FX Setup 은 Edit Mode 에서만 동작합니다.", "OK");
                return;
            }

            EnsureFolder(VFXFolder);

            var snapPrefab     = LoadOrCreatePrefab(SnapBurstPath, "SnapBurst", ConfigureSnapBurst);
            var completePrefab = LoadOrCreatePrefab(CompleteBurstPath, "CompleteBurst", ConfigureCompleteBurst);

            var scene = EditorSceneManager.OpenScene(GameplayScenePath);

            // 재실행 안전 — 기존 매니저 GO 정리
            RemoveByName("AudioManager");
            RemoveByName("VFXManager");

            // AudioManager + 오디오 클립 자동 할당 (Assets/Audio/BGM 폴더 + Assets/Audio 의 SFX 파일들)
            var amGo = new GameObject("AudioManager");
            var am   = amGo.AddComponent<AudioManager>();
            AutoAssignAudioClips(am);

            // VFXManager + 프리팹 와이어링 (private SerializeField → SerializedObject 경유)
            var vmGo = new GameObject("VFXManager");
            var vm   = vmGo.AddComponent<VFXManager>();

            var so = new SerializedObject(vm);
            so.FindProperty("snapBurstPrefab").objectReferenceValue     = snapPrefab.GetComponent<ParticleSystem>();
            so.FindProperty("completeBurstPrefab").objectReferenceValue = completePrefab.GetComponent<ParticleSystem>();
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(vm);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[FXSetupTool] FX 세팅 완료 — Gameplay 씬에 AudioManager/VFXManager 부착, VFX 프리팹 2개 와이어링");
            EditorUtility.DisplayDialog("PuzzlePals",
                "FX 세팅 완료\n\n" +
                "생성/갱신:\n" +
                $"  • {SnapBurstPath}\n" +
                $"  • {CompleteBurstPath}\n" +
                "  • Gameplay 씬: AudioManager + VFXManager\n\n" +
                "남은 수동 작업: AudioManager 의 popClip / dingClip / fanfareClip 슬롯에\n" +
                "AudioClip 을 직접 할당하세요. (freesound.org 등에서 무료 SFX 다운로드)",
                "OK");
        }

        // ─────────────────────────────────────────────
        // Particle 프리팹 생성
        // ─────────────────────────────────────────────

        private static GameObject LoadOrCreatePrefab(string path, string goName, System.Action<ParticleSystem> configure)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject(goName);
            var ps = go.AddComponent<ParticleSystem>();
            configure(ps);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // GDD §Game Feel: 스냅 = 별 파티클 5 burst
        // (별 모양 텍스처가 없으므로 노란 점으로 대체 — 색/움직임/개수는 사양 일치)
        private static void ConfigureSnapBurst(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration         = 0.4f;
            main.loop             = false;
            main.startLifetime    = 0.5f;
            main.startSpeed       = 4f;
            main.startSize        = 0.25f;
            main.startColor       = new Color(1f, 0.95f, 0.4f, 1f);
            main.maxParticles     = 20;
            main.simulationSpace  = ParticleSystemSimulationSpace.World;
            main.stopAction       = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 5) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius    = 0.05f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.85f, 0.2f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = fade;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            var curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
            size.size = new ParticleSystem.MinMaxCurve(1f, curve);
        }

        // GDD §Game Feel: 완성 = 무지개 파티클 폭발
        private static void ConfigureCompleteBurst(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration         = 0.6f;
            main.loop             = false;
            main.startLifetime    = 1.0f;
            main.startSpeed       = 6f;
            main.startSize        = 0.3f;
            main.maxParticles     = 80;
            main.simulationSpace  = ParticleSystemSimulationSpace.World;
            main.stopAction       = ParticleSystemStopAction.Destroy;

            // 무지개: RandomColor 모드 그라디언트
            var rainbow = new Gradient();
            rainbow.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1.00f, 0.20f, 0.20f), 0.00f),
                    new GradientColorKey(new Color(1.00f, 0.60f, 0.20f), 0.17f),
                    new GradientColorKey(new Color(1.00f, 0.95f, 0.30f), 0.34f),
                    new GradientColorKey(new Color(0.30f, 0.85f, 0.30f), 0.50f),
                    new GradientColorKey(new Color(0.30f, 0.60f, 1.00f), 0.67f),
                    new GradientColorKey(new Color(0.55f, 0.30f, 0.85f), 0.84f),
                    new GradientColorKey(new Color(0.85f, 0.40f, 0.85f), 1.00f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });

            var startColor = new ParticleSystem.MinMaxGradient(rainbow);
            startColor.mode = ParticleSystemGradientMode.RandomColor;
            main.startColor = startColor;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 50) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius    = 0.1f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.7f),
                    new GradientAlphaKey(0f, 1f),
                });
            col.color = fade;
        }

        // ─────────────────────────────────────────────
        // 헬퍼
        // ─────────────────────────────────────────────

        private static void RemoveByName(string name)
        {
            var all = Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var go in all)
                if (go != null && go.name == name)
                    Object.DestroyImmediate(go);
        }

        private static void EnsureFolder(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder)) return;
            string parent = Path.GetDirectoryName(assetFolder).Replace('\\', '/');
            string leaf   = Path.GetFileName(assetFolder);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        // ─────────────────────────────────────────────
        // AudioManager 자동 클립 할당 (BGM 폴더 스캔 + SFX 파일명 매칭)
        // ─────────────────────────────────────────────

        private static void AutoAssignAudioClips(AudioManager am)
        {
            var so = new SerializedObject(am);

            // 1) bgmClips — Assets/Audio/BGM 폴더의 모든 AudioClip 자동 할당 (이름순 정렬)
            string[] bgmGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio/BGM" });
            System.Array.Sort(bgmGuids,
                (a, b) => string.Compare(AssetDatabase.GUIDToAssetPath(a),
                                         AssetDatabase.GUIDToAssetPath(b),
                                         System.StringComparison.Ordinal));

            var bgmProp = so.FindProperty("bgmClips");
            bgmProp.arraySize = bgmGuids.Length;
            for (int i = 0; i < bgmGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(bgmGuids[i]);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                bgmProp.GetArrayElementAtIndex(i).objectReferenceValue = clip;
            }

            // 2) SFX — 파일명 매칭으로 자동 할당
            TryAssignClipByName(so, "popClip",          "pop",            "Assets/Audio");
            TryAssignClipByName(so, "dingClip",         "ding",           "Assets/Audio");
            TryAssignClipByName(so, "pairClip",         "pair",           "Assets/Audio");
            TryAssignClipByName(so, "fanfareClip",      "fanfare",        "Assets/Audio");
            TryAssignClipByName(so, "levelCompleteClip","levelcomplete",  "Assets/Audio");
            TryAssignClipByName(so, "levelCompleteClip","level_complete", "Assets/Audio"); // 이름 변형 대응

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(am);

            Debug.Log($"[FXSetupTool] AudioManager 자동 할당 — bgmClips {bgmGuids.Length}개" +
                      (bgmGuids.Length == 0 ? " (Assets/Audio/BGM/ 폴더가 비어있음)" : ""));
        }

        // 인스펙터 필드명 + 파일명 substring (대소문자 무관) 으로 AudioClip 검색.
        // 이미 할당된 필드는 덮어쓰지 않음 (사용자 수동 할당 보존).
        private static void TryAssignClipByName(SerializedObject so, string fieldName, string nameSubstr, string searchFolder)
        {
            var prop = so.FindProperty(fieldName);
            if (prop == null) return;
            if (prop.objectReferenceValue != null) return; // 이미 할당됨

            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { searchFolder });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                if (!name.Contains(nameSubstr.ToLowerInvariant())) continue;
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                prop.objectReferenceValue = clip;
                return;
            }
        }
    }
}
