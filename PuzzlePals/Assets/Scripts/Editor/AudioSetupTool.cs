using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using PuzzlePals.Systems;

namespace PuzzlePals.EditorTools
{
    /// <summary>
    /// AudioManager 의 SFX 슬롯을 Assets/Audio/ 의 파일명 매칭으로 자동 연결.
    ///   - pair.ogg / pair.wav / pair.mp3   → pairClip
    ///   - fanfare.ogg / fanfare.wav / fanfare.mp3 → fanfareClip
    ///   - Assets/Audio/BGM/ 폴더 안의 모든 AudioClip → bgmClips[] (이름 정렬, 셔플 순환)
    ///   - dingClip 은 절차적 합성이라 자동 연결 X (AudioManager.Awake 가 처리)
    ///
    /// 메뉴: PuzzlePals > Setup Audio Clips
    /// idempotent — 재실행 안전. 씬에 AudioManager 가 있어야 동작.
    /// </summary>
    public static class AudioSetupTool
    {
        private const string AudioFolder = "Assets/Audio";
        private const string BgmFolder   = "Assets/Audio/BGM";

        [MenuItem("PuzzlePals/Setup Audio Clips")]
        public static void SetupAudioClips()
        {
            // 모든 로드된 씬에서 AudioManager 찾기 — 없으면 현재 활성 씬에 자동 생성
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

            connected += TrySet(manager, "pairClip",    "pair",    report, ref already);
            connected += TrySet(manager, "fanfareClip", "fanfare", report, ref already);
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
                         "\nding 은 절차적 합성 (코드 fallback) — 인스펙터 미할당 OK." +
                         (autoCreated ? "\n\n⚠ 씬 저장 잊지 마세요 (Ctrl+S / ⌘S)" : "");
            Debug.Log("[AudioSetupTool] " + msg.Replace("\n", " | "));
            EditorUtility.DisplayDialog("PuzzlePals — Audio Setup", msg, "OK");
        }

        // 필드명으로 SerializedObject 접근 → AudioClip 자동 할당.
        private static int TrySet(AudioManager manager, string fieldName, string baseName,
                                  System.Text.StringBuilder report, ref int already)
        {
            var so   = new SerializedObject(manager);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                report.AppendLine($"  ✗ 필드 '{fieldName}' 못 찾음 (코드 변경됐을 수 있음)");
                return 0;
            }

            // 이미 할당되어 있으면 skip
            if (prop.objectReferenceValue != null)
            {
                already++;
                report.AppendLine($"  ✓ {fieldName}: 이미 연결됨 ({prop.objectReferenceValue.name})");
                return 0;
            }

            // 확장자 .ogg / .wav / .mp3 순서로 검색
            AudioClip clip = null;
            foreach (var ext in new[] { ".ogg", ".wav", ".mp3" })
            {
                string path = $"{AudioFolder}/{baseName}{ext}";
                clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null) break;
            }

            if (clip == null)
            {
                report.AppendLine($"  - {fieldName}: {AudioFolder}/{baseName}.{{ogg|wav|mp3}} 없음 (skip)");
                return 0;
            }

            prop.objectReferenceValue = clip;
            so.ApplyModifiedProperties();
            report.AppendLine($"  ✓ {fieldName} ← {clip.name}");
            return 1;
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
