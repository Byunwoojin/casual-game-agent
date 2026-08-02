using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MemoryMatch.Systems;

namespace MemoryMatch.EditorTools
{
    /// <summary>
    /// AudioManager.bgmClips 를 Assets/Audio/BGM/ 폴더 스캔으로 자동 연결.
    ///   - 폴더 안의 모든 AudioClip → bgmClips[] (이름 정렬, 셔플 순환)
    ///   - found/miss/levelComplete/completeScene 은 절차적 합성이라 자동 연결 X (AudioManager.Awake 가 처리)
    ///
    /// 메뉴: MemoryMatch > Setup Audio Clips
    /// idempotent — 재실행 안전. 씬에 AudioManager 가 없으면 자동 생성.
    /// </summary>
    public static class AudioSetupTool
    {
        private const string BgmFolder = "Assets/Audio/BGM";

        [MenuItem("MemoryMatch/Setup Audio Clips")]
        public static void SetupAudioClips()
        {
            var manager = Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            bool autoCreated = false;
            if (manager == null)
            {
                var go = new GameObject("AudioManager");
                manager = go.AddComponent<AudioManager>();
                autoCreated = true;
                Debug.Log($"[AudioSetupTool] AudioManager 자동 생성 — 씬: {go.scene.name}");
            }

            int connected = 0;
            int already   = 0;
            var report    = new System.Text.StringBuilder();

            connected += TrySetBgmFolder(manager, report, ref already);

            if (connected > 0 || autoCreated)
            {
                EditorUtility.SetDirty(manager);
                if (!EditorApplication.isPlaying)
                {
                    var scene = manager.gameObject.scene;
                    if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene);
                }
            }

            string msg = $"오디오 클립 자동 연결\n\n" +
                         (autoCreated ? $"  • AudioManager 자동 생성 (씬: {manager.gameObject.scene.name})\n" : "") +
                         $"  • 신규 연결: {connected}\n" +
                         $"  • 이미 연결됨: {already}\n\n" +
                         report.ToString() +
                         "\nfound/miss/levelComplete/completeScene 은 절차적 합성 (코드 fallback) — 인스펙터 미할당 OK." +
                         (autoCreated ? "\n\n⚠ 씬 저장 잊지 마세요 (Ctrl+S / ⌘S)" : "");
            Debug.Log("[AudioSetupTool] " + msg.Replace("\n", " | "));
            EditorUtility.DisplayDialog("MemoryMatch — Audio Setup", msg, "OK");
        }

        // Assets/Audio/BGM/ 폴더 스캔 → AudioManager.bgmClips 배열에 일괄 할당.
        // 정렬 기준: clip.name (파일명) 오름차순 — bgm_01, bgm_02, ... 순서 보장.
        private static int TrySetBgmFolder(AudioManager manager, System.Text.StringBuilder report, ref int already)
        {
            if (!AssetDatabase.IsValidFolder(BgmFolder))
            {
                report.AppendLine($"  - bgmClips: '{BgmFolder}' 폴더 없음 (skip)");
                return 0;
            }

            var clips = AssetDatabase.FindAssets("t:AudioClip", new[] { BgmFolder })
                .Select(g => AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(c => c != null)
                .OrderBy(c => c.name, System.StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (clips.Length == 0)
            {
                report.AppendLine($"  - bgmClips: '{BgmFolder}' 비어있음 (skip)");
                return 0;
            }

            var so   = new SerializedObject(manager);
            var prop = so.FindProperty("bgmClips");
            if (prop == null)
            {
                report.AppendLine($"  ✗ 필드 'bgmClips' 못 찾음 (코드 변경됐을 수 있음)");
                return 0;
            }

            // 기존 배열과 동일하면 skip — idempotent
            bool sameAll = prop.arraySize == clips.Length;
            if (sameAll)
            {
                for (int i = 0; i < clips.Length; i++)
                {
                    if (prop.GetArrayElementAtIndex(i).objectReferenceValue != clips[i])
                    {
                        sameAll = false;
                        break;
                    }
                }
            }
            if (sameAll)
            {
                already++;
                report.AppendLine($"  ✓ bgmClips: 이미 연결됨 ({clips.Length}곡)");
                return 0;
            }

            prop.arraySize = clips.Length;
            for (int i = 0; i < clips.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            }
            so.ApplyModifiedProperties();
            report.AppendLine($"  ✓ bgmClips ← {clips.Length}곡 ({string.Join(", ", clips.Select(c => c.name))})");
            return 1;
        }
    }
}
