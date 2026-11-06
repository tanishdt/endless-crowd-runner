using UnityEngine;

namespace EndlessCrowdRunner.Audio
{
    /// <summary>
    /// Generates royalty-free audio clips procedurally at runtime using mathematical waveforms.
    /// Guarantees crisp sound feedback without external audio asset dependencies.
    /// </summary>
    public static class ProceduralAudioGenerator
    {
        private const int SampleRate = 44100;

        public static AudioClip Generate(SoundEffectType type)
        {
            switch (type)
            {
                case SoundEffectType.ButtonClick:
                    return CreateTone(880f, 0.05f, 0.3f);

                case SoundEffectType.UnitSpawn:
                    return CreateChirp(350f, 750f, 0.12f, 0.35f);

                case SoundEffectType.UnitDeath:
                    return CreateNoiseBurst(0.12f, 0.4f, 150f);

                case SoundEffectType.GatePositive:
                    return CreateHarmonicChord(new float[] { 523.25f, 659.25f, 783.99f }, 0.25f, 0.45f);

                case SoundEffectType.GateNegative:
                    return CreateDissonantBuzzer(160f, 0.25f, 0.45f);

                case SoundEffectType.GunshotPistol:
                    return CreateGunshot(0.08f, 0.5f, 600f);

                case SoundEffectType.GunshotSMG:
                    return CreateGunshot(0.05f, 0.4f, 800f);

                case SoundEffectType.GunshotShotgun:
                    return CreateGunshot(0.18f, 0.65f, 350f);

                case SoundEffectType.GunshotRifle:
                    return CreateGunshot(0.09f, 0.55f, 550f);

                case SoundEffectType.GunshotMinigun:
                    return CreateGunshot(0.04f, 0.45f, 900f);

                case SoundEffectType.ExplosionSmall:
                    return CreateNoiseBurst(0.35f, 0.6f, 120f);

                case SoundEffectType.ExplosionLarge:
                    return CreateNoiseBurst(0.65f, 0.8f, 75f);

                case SoundEffectType.BossSlam:
                    return CreateSubBassImpact(0.45f, 0.8f, 85f);

                case SoundEffectType.Powerup:
                    return CreateChirp(400f, 1200f, 0.3f, 0.5f);

                default:
                    return CreateTone(440f, 0.1f, 0.3f);
            }
        }

        private static AudioClip CreateTone(float frequency, float duration, float volume)
        {
            int samples = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float env = 1.0f - ((float)i / samples);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * env * volume;
            }

            AudioClip clip = AudioClip.Create("Tone", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateChirp(float startFreq, float endFreq, float duration, float volume)
        {
            int samples = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (float)i / SampleRate) * env * volume;
            }

            AudioClip clip = AudioClip.Create("Chirp", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateGunshot(float duration, float volume, float filterFreq)
        {
            int samples = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[samples];
            float last = 0f;

            for (int i = 0; i < samples; i++)
            {
                float progress = (float)i / samples;
                float env = Mathf.Exp(-progress * 12.0f);
                float noise = (Random.value * 2f - 1f);
                // Simple low-pass filter
                last = Mathf.Lerp(last, noise, 0.4f);
                data[i] = last * env * volume;
            }

            AudioClip clip = AudioClip.Create("Gunshot", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateNoiseBurst(float duration, float volume, float decay)
        {
            int samples = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float progress = (float)i / samples;
                float env = Mathf.Exp(-progress * (decay * 0.1f));
                float noise = (Random.value * 2f - 1f);
                data[i] = noise * env * volume;
            }

            AudioClip clip = AudioClip.Create("Explosion", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateHarmonicChord(float[] frequencies, float duration, float volume)
        {
            int samples = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / samples;
                float env = Mathf.Sin(progress * Mathf.PI);

                float wave = 0f;
                for (int f = 0; f < frequencies.Length; f++)
                {
                    wave += Mathf.Sin(2f * Mathf.PI * frequencies[f] * t);
                }
                wave /= frequencies.Length;

                data[i] = wave * env * volume;
            }

            AudioClip clip = AudioClip.Create("Chord", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateDissonantBuzzer(float baseFreq, float duration, float volume)
        {
            int samples = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / samples;
                float env = 1.0f - progress;
                // Square wave approximation with dissonance
                float wave1 = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * baseFreq * t));
                float wave2 = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * (baseFreq * 1.414f) * t));
                data[i] = (wave1 + wave2) * 0.5f * env * volume;
            }

            AudioClip clip = AudioClip.Create("Buzzer", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateSubBassImpact(float duration, float volume, float startFreq)
        {
            int samples = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float progress = (float)i / samples;
                float freq = Mathf.Lerp(startFreq, 30f, progress);
                float env = Mathf.Exp(-progress * 6f);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (float)i / SampleRate) * env * volume;
            }

            AudioClip clip = AudioClip.Create("SubBass", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
