using UnityEngine;
using EndlessCrowdRunner.Difficulty;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Environment;
using EndlessCrowdRunner.Camera;
using EndlessCrowdRunner.Pooling;
using EndlessCrowdRunner.Audio;
using EndlessCrowdRunner.UI;
using EndlessCrowdRunner.Visuals;
using EndlessCrowdRunner.Weapons;

namespace EndlessCrowdRunner.Core
{
    /// <summary>
    /// Runtime bootstrapper ensuring all core singletons, cameras, directional lighting,
    /// and game managers are automatically constructed and linked when the scene loads.
    /// Eliminates missing reference bugs even if executed in a blank Unity scene.
    /// </summary>
    public static class RuntimeBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeOnStartup()
        {
            // Ensure VisualAssetFactory
            if (Object.FindObjectOfType<VisualAssetFactory>() == null)
            {
                GameObject vafGo = new GameObject("[VisualAssetFactory]");
                vafGo.AddComponent<VisualAssetFactory>();
            }

            // Ensure SoundManager
            if (Object.FindObjectOfType<SoundManager>() == null)
            {
                GameObject smGo = new GameObject("[SoundManager]");
                smGo.AddComponent<SoundManager>();
            }

            // Ensure PoolManager
            if (Object.FindObjectOfType<PoolManager>() == null)
            {
                GameObject pmGo = new GameObject("[PoolManager]");
                pmGo.AddComponent<PoolManager>();
            }

            // Ensure CameraShake
            if (Object.FindObjectOfType<CameraShake>() == null)
            {
                GameObject csGo = new GameObject("[CameraShake]");
                csGo.AddComponent<CameraShake>();
            }

            // Ensure FloatingTextManager
            if (Object.FindObjectOfType<FloatingTextManager>() == null)
            {
                GameObject ftmGo = new GameObject("[FloatingTextManager]");
                ftmGo.AddComponent<FloatingTextManager>();
            }

            // Ensure Directional Light
            if (Object.FindObjectOfType<Light>() == null)
            {
                GameObject lightGo = new GameObject("Directional Light");
                Light light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1.0f, 0.96f, 0.90f);
                light.intensity = 1.25f;
                light.shadows = LightShadows.Hard;
                lightGo.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            }

            // Ensure Main Camera & CameraFollow
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.38f, 0.65f, 0.92f); // Vibrant stylized sky blue
                camGo.AddComponent<AudioListener>();
            }

            CameraFollow camFollow = cam.GetComponent<CameraFollow>();
            if (camFollow == null)
            {
                camFollow = cam.gameObject.AddComponent<CameraFollow>();
            }

            // Ensure Player Crowd Controller
            CrowdController crowd = Object.FindObjectOfType<CrowdController>();
            if (crowd == null)
            {
                GameObject crowdGo = new GameObject("PlayerCrowd");
                crowd = crowdGo.AddComponent<CrowdController>();
                crowdGo.AddComponent<WeaponSystem>();
            }

            // Ensure DifficultyScaler
            DifficultyScaler scaler = Object.FindObjectOfType<DifficultyScaler>();
            if (scaler == null)
            {
                GameObject scalerGo = new GameObject("DifficultyScaler");
                scaler = scalerGo.AddComponent<DifficultyScaler>();
            }

            // Ensure ChunkGenerator
            ChunkGenerator generator = Object.FindObjectOfType<ChunkGenerator>();
            if (generator == null)
            {
                GameObject genGo = new GameObject("ChunkGenerator");
                generator = genGo.AddComponent<ChunkGenerator>();
            }

            // Ensure UIManager
            if (Object.FindObjectOfType<UIManager>() == null)
            {
                GameObject uiGo = new GameObject("[UIManager]");
                uiGo.AddComponent<UIManager>();
            }

            // Ensure GameManager
            GameManager gm = Object.FindObjectOfType<GameManager>();
            if (gm == null)
            {
                GameObject gmGo = new GameObject("[GameManager]");
                gm = gmGo.AddComponent<GameManager>();
            }
        }
    }
}
