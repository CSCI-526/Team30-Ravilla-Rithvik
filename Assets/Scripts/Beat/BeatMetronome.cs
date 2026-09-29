using UnityEngine;

namespace BeatTiming
{
    /// <summary>
    /// Plays a short tick on every beat, scheduled on the audio clock so it lands exactly on time.
    /// Sound reinforces the visual cue; the game must still be playable with this muted.
    /// Generates its own click sound if no clip is assigned.
    /// </summary>
    public class BeatMetronome : MonoBehaviour
    {
        [SerializeField] BeatConductor conductor;
        [SerializeField] AudioClip tick;
        [SerializeField] AudioClip accentTick;
        [Tooltip("Every Nth beat uses the accent tick (0 = never).")]
        [SerializeField, Min(0)] int accentEvery = 4;
        [SerializeField, Range(0f, 1f)] float volume = 0.6f;
        [SerializeField] bool muted;
        [Tooltip("Also play a soft tick on every half-beat (used by the Double Tap dash).")]
        [SerializeField] bool offbeatTicks;
        [SerializeField, Range(0f, 1f)] float offbeatVolume = 0.3f;

        // Sources are alternated so a scheduled tick never cuts off the previous one.
        AudioSource[] sources;
        int nextSource;
        // Scheduling runs in half-beat steps: even steps are beats, odd steps are half-beats.
        int nextScheduledStep;
        const double LookAhead = 0.1;

        public bool Muted
        {
            get => muted;
            set => muted = value;
        }

        public bool OffbeatTicks
        {
            get => offbeatTicks;
            set => offbeatTicks = value;
        }

        void Awake()
        {
            if (conductor == null) conductor = GetComponent<BeatConductor>();
            if (conductor == null) conductor = BeatConductor.Instance;
            if (tick == null) tick = MakeClick("Tick", 1000f);
            if (accentTick == null) accentTick = MakeClick("AccentTick", 1500f);

            sources = new AudioSource[3];
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i] = gameObject.AddComponent<AudioSource>();
                sources[i].playOnAwake = false;
            }
        }

        void Update()
        {
            if (conductor == null || !conductor.IsRunning) return;

            // Skip any steps that are already in the past (e.g. after a hitch or restart).
            int firstFuture = Mathf.Max(0, Mathf.CeilToInt((float)conductor.SongBeats * 2f));
            if (nextScheduledStep < firstFuture) nextScheduledStep = firstFuture;

            double t = conductor.DspTimeOfBeat(0) + nextScheduledStep * conductor.SecondsPerBeat * 0.5;
            if (t - AudioSettings.dspTime > LookAhead) return;

            bool onBeat = nextScheduledStep % 2 == 0;
            if (!muted && (onBeat || offbeatTicks))
            {
                int beat = nextScheduledStep / 2;
                bool accent = onBeat && accentEvery > 0 && beat % accentEvery == 0;
                AudioSource s = sources[nextSource];
                nextSource = (nextSource + 1) % sources.Length;
                s.clip = accent ? accentTick : tick;
                s.volume = onBeat ? volume : offbeatVolume;
                s.PlayScheduled(t);
            }
            nextScheduledStep++;
        }

        static AudioClip MakeClick(string name, float frequency)
        {
            int rate = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 44100;
            int length = rate / 20; // 50ms
            var data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / rate;
                float envelope = Mathf.Exp(-t * 60f);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope;
            }
            AudioClip clip = AudioClip.Create(name, length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
