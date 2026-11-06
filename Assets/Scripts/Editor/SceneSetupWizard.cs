#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using EndlessCrowdRunner.Difficulty;
using EndlessCrowdRunner.Core;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Environment;
using EndlessCrowdRunner.Camera;
using EndlessCrowdRunner.Pooling;
using EndlessCrowdRunner.Audio;
using EndlessCrowdRunner.UI;
using EndlessCrowdRunner.Visuals;
using EndlessCrowdRunner.Weapons;

namespace EndlessCrowdRunner.Editor
{
    /// <summary>
    /// Editor tool providing one-click scene setup, config generation, and validation.
    /// Accessible via menu 'Tools/Endless Crowd Runner/Setup Game Scene'.
    /// </summary>
    public static class SceneSetupWizard
    {
        [MenuItem("Tools/Endless Crowd Runner/Setup Game Scene")]
        public static void SetupScene()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            Undo.RegisterFullObjectHierarchyUndo(activeScene.GetRootGameObjects(), "Setup Crowd Runner Scene");

            // 1. Create or load GameConfig
            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Resources/GameConfig.asset");
            if (config == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                config = ScriptableObject.CreateInstance<GameConfig>();
                AssetDatabase.CreateAsset(config, "Assets/Resources/GameConfig.asset");
                AssetDatabase.SaveAssets();
                Debug.Log("[SceneSetupWizard] Created GameConfig at Assets/Resources/GameConfig.asset");
            }

            // 2. Ensure Core Managers Root
            GameObject coreRoot = GameObject.Find("[CORE]");
            if (coreRoot == null) coreRoot = new GameObject("[CORE]");

            EnsureComponent<VisualAssetFactory>(coreRoot);
            EnsureComponent<SoundManager>(coreRoot);
            EnsureComponent<PoolManager>(coreRoot);
            EnsureComponent<CameraShake>(coreRoot);
            EnsureComponent<FloatingTextManager>(coreRoot);
            EnsureComponent<UIManager>(coreRoot);

            var gm = EnsureComponent<GameManager>(coreRoot);

            // 3. Setup Directional Light
            Light light = Object.FindObjectOfType<Light>();
            if (light == null)
            {
                GameObject lightGo = new GameObject("Directional Light");
                light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1.0f, 0.96f, 0.90f);
                light.intensity = 1.25f;
                light.shadows = LightShadows.Hard;
                lightGo.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            }

            // 4. Setup Main Camera
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.38f, 0.65f, 0.92f);
                camGo.AddComponent<AudioListener>();
            }

            var camFollow = EnsureComponent<CameraFollow>(cam.gameObject);

            // 5. Setup Gameplay Objects
            GameObject playerCrowd = GameObject.Find("PlayerCrowd");
            if (playerCrowd == null) playerCrowd = new GameObject("PlayerCrowd");
            var crowdCtrl = EnsureComponent<CrowdController>(playerCrowd);
            var weaponSys = EnsureComponent<WeaponSystem>(playerCrowd);

            GameObject scalerGo = GameObject.Find("DifficultyScaler");
            if (scalerGo == null) scalerGo = new GameObject("DifficultyScaler");
            var scaler = EnsureComponent<DifficultyScaler>(scalerGo);

            GameObject genGo = GameObject.Find("ChunkGenerator");
            if (genGo == null) genGo = new GameObject("ChunkGenerator");
            var gen = EnsureComponent<ChunkGenerator>(genGo);

            EditorSceneManager.MarkSceneDirty(activeScene);
            Debug.Log("[SceneSetupWizard] Endless Crowd Runner scene hierarchy configured successfully!");
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            T comp = target.GetComponent<T>();
            if (comp == null)
            {
                comp = target.AddComponent<T>();
            }
            return comp;
        }
    }
}
#endif
