using System.Collections.Generic;
using UnityEngine;

namespace EndlessCrowdRunner.Audio
{
    /// <summary>
    /// Singleton manager coordinating audio effects with pooling and player mute preference.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private const string PrefSoundEnabled = "ECR_SoundEnabled";
        private readonly Dictionary<SoundEffectType, AudioClip> _clipCache = new Dictionary<SoundEffectType, AudioClip>();
        private readonly List<AudioSource> _sourcePool = new List<AudioSource>(16);

        public bool IsSoundEnabled { get; private set; } = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            IsSoundEnabled = PlayerPrefs.GetInt(PrefSoundEnabled, 1) == 1;

            // Pre-generate procedural clips
            foreach (SoundEffectType type in System.Enum.GetValues(typeof(SoundEffectType)))
            {
                _clipCache[type] = ProceduralAudioGenerator.Generate(type);
            }

            // Create initial AudioSource pool
            for (int i = 0; i < 8; i++)
            {
                CreateNewSource();
            }
        }

        private AudioSource CreateNewSource()
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f; // 2D flat audio
            _sourcePool.Add(src);
            return src;
        }

        public void ToggleSound()
        {
            SetSoundEnabled(!IsSoundEnabled);
        }

        public void SetSoundEnabled(bool enabled)
        {
            IsSoundEnabled = enabled;
            PlayerPrefs.SetInt(PrefSoundEnabled, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void PlaySound(SoundEffectType type, float volumeMultiplier = 1.0f)
        {
            if (!IsSoundEnabled) return;
            if (!_clipCache.TryGetValue(type, out AudioClip clip) || clip == null) return;

            AudioSource available = null;
            for (int i = 0; i < _sourcePool.Count; i++)
            {
                if (!_sourcePool[i].isPlaying)
                {
                    available = _sourcePool[i];
                    break;
                }
            }

            if (available == null)
            {
                available = CreateNewSource();
            }

            available.volume = Mathf.Clamp01(volumeMultiplier);
            available.clip = clip;
            available.Play();
        }
    }
}
