using UnityEngine;

namespace GatosVsRatos
{
    public sealed class AudioKit
    {
        private readonly AudioSource source;
        private readonly AudioSource musicSource;
        private readonly AudioClip click;
        private readonly AudioClip rapidShot;
        private readonly AudioClip heavyShot;
        private readonly AudioClip catapult;
        private readonly AudioClip impact;
        private readonly AudioClip upgrade;
        private readonly AudioClip victory;
        private readonly AudioClip defeat;
        private readonly AudioClip menuMusic;
        private readonly AudioClip battleMusic;

        public bool MusicEnabled { get; private set; }

        public AudioKit(AudioSource audioSource, AudioSource backgroundMusicSource)
        {
            source = audioSource;
            musicSource = backgroundMusicSource;
            click = Tone("Click", 660f, 0.055f, 0.15f, false);
            rapidShot = Tone("RapidShot", 165f, 0.055f, 0.12f, true);
            heavyShot = Tone("HeavyShot", 92f, 0.18f, 0.22f, true);
            catapult = Tone("Catapult", 280f, 0.14f, 0.18f, false);
            impact = Tone("Impact", 110f, 0.09f, 0.12f, true);
            upgrade = Sweep("Upgrade", 420f, 880f, 0.3f, 0.16f);
            victory = Sweep("Victory", 440f, 1040f, 0.75f, 0.2f);
            defeat = Sweep("Defeat", 260f, 95f, 0.7f, 0.18f);
            menuMusic = ComposeMenuMusic();
            battleMusic = ComposeBattleMusic();
            MusicEnabled = PlayerPrefs.GetInt("GVR_Music", 1) == 1;
            musicSource.loop = true;
            musicSource.volume = 0.28f;
        }

        public void Click() => source.PlayOneShot(click);
        public void Impact() => source.PlayOneShot(impact);
        public void Upgrade() => source.PlayOneShot(upgrade);
        public void Victory() => source.PlayOneShot(victory);
        public void Defeat() => source.PlayOneShot(defeat);

        public void PlayMenuMusic() => PlayMusic(menuMusic);
        public void PlayBattleMusic() => PlayMusic(battleMusic);

        public bool ToggleMusic()
        {
            MusicEnabled = !MusicEnabled;
            PlayerPrefs.SetInt("GVR_Music", MusicEnabled ? 1 : 0);
            PlayerPrefs.Save();
            if (MusicEnabled)
            {
                if (musicSource.clip != null) musicSource.Play();
            }
            else musicSource.Pause();
            return MusicEnabled;
        }

        private void PlayMusic(AudioClip clip)
        {
            if (musicSource.clip == clip)
            {
                if (MusicEnabled && !musicSource.isPlaying) musicSource.Play();
                return;
            }
            musicSource.clip = clip;
            if (MusicEnabled) musicSource.Play();
        }

        public void Shoot(TowerKind kind)
        {
            source.PlayOneShot(kind == TowerKind.Metralhadora ? rapidShot : kind == TowerKind.Bazuca ? heavyShot : catapult);
        }

        private static AudioClip Tone(string name, float frequency, float duration, float volume, bool noisy)
        {
            const int sampleRate = 44100;
            int length = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - i / (float)length;
                float noise = noisy ? Random.Range(-0.22f, 0.22f) : 0f;
                data[i] = (Mathf.Sin(t * frequency * Mathf.PI * 2f) + noise) * envelope * volume;
            }
            var clip = AudioClip.Create(name, length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip Sweep(string name, float from, float to, float duration, float volume)
        {
            const int sampleRate = 44100;
            int length = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[length];
            float phase = 0f;
            for (int i = 0; i < length; i++)
            {
                float p = i / (float)length;
                phase += Mathf.Lerp(from, to, p) / sampleRate * Mathf.PI * 2f;
                float envelope = Mathf.Sin(p * Mathf.PI);
                data[i] = Mathf.Sin(phase) * envelope * volume;
            }
            var clip = AudioClip.Create(name, length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip ComposeMenuMusic()
        {
            float[] melody =
            {
                523.25f, 0, 659.25f, 0, 783.99f, 659.25f, 587.33f, 0,
                523.25f, 0, 440.00f, 523.25f, 587.33f, 0, 392.00f, 0,
                440.00f, 0, 523.25f, 0, 659.25f, 587.33f, 523.25f, 0,
                392.00f, 440.00f, 523.25f, 0, 493.88f, 0, 523.25f, 0
            };
            float[] bass = { 130.81f, 110.00f, 146.83f, 98.00f };
            return ComposeSong("MenuMusic", 108f, melody, bass, false);
        }

        private static AudioClip ComposeBattleMusic()
        {
            float[] melody =
            {
                587.33f, 587.33f, 698.46f, 0, 783.99f, 698.46f, 587.33f, 0,
                523.25f, 523.25f, 587.33f, 0, 698.46f, 659.25f, 523.25f, 0,
                587.33f, 0, 880.00f, 783.99f, 698.46f, 0, 587.33f, 523.25f,
                466.16f, 523.25f, 587.33f, 698.46f, 659.25f, 0, 587.33f, 0
            };
            float[] bass = { 146.83f, 116.54f, 130.81f, 98.00f };
            return ComposeSong("BattleMusic", 132f, melody, bass, true);
        }

        private static AudioClip ComposeSong(string name, float bpm, float[] melody, float[] bass, bool battle)
        {
            const int sampleRate = 44100;
            float stepDuration = 60f / bpm * 0.5f;
            float duration = stepDuration * melody.Length;
            int length = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float time = i / (float)sampleRate;
                int step = Mathf.Min(melody.Length - 1, Mathf.FloorToInt(time / stepDuration));
                float stepPhase = (time % stepDuration) / stepDuration;
                float noteEnvelope = Mathf.Pow(Mathf.Sin(Mathf.Clamp01(stepPhase) * Mathf.PI), 0.55f);
                float melodySample = melody[step] <= 0f ? 0f : SoftSquare(time * melody[step]) * noteEnvelope * 0.18f;

                int bassStep = (step / 8) % bass.Length;
                float bassEnvelope = 0.58f + 0.42f * Mathf.Sin(stepPhase * Mathf.PI);
                float bassSample = Triangle(time * bass[bassStep]) * bassEnvelope * 0.13f;

                float beatPhase = (time % (stepDuration * 2f)) / (stepDuration * 2f);
                float kick = battle ? Mathf.Sin(time * Mathf.Lerp(105f, 52f, beatPhase) * Mathf.PI * 2f) * Mathf.Exp(-beatPhase * 13f) * 0.16f : 0f;
                float shimmer = battle && step % 2 == 1 ? PseudoNoise(i) * Mathf.Exp(-stepPhase * 18f) * 0.035f : 0f;
                float pad = Mathf.Sin(time * bass[bassStep] * 2f * Mathf.PI) * 0.035f;
                data[i] = Mathf.Clamp((melodySample + bassSample + kick + shimmer + pad) * 0.82f, -0.82f, 0.82f);
            }

            var clip = AudioClip.Create(name, length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float Triangle(float cycles)
        {
            return 2f * Mathf.Abs(2f * (cycles - Mathf.Floor(cycles + 0.5f))) - 1f;
        }

        private static float SoftSquare(float cycles)
        {
            return (float)System.Math.Tanh(Mathf.Sin(cycles * Mathf.PI * 2f) * 2.2f);
        }

        private static float PseudoNoise(int sample)
        {
            float value = Mathf.Sin(sample * 12.9898f) * 43758.5453f;
            return (value - Mathf.Floor(value)) * 2f - 1f;
        }
    }
}
