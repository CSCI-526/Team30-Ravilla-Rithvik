using System;
using UnityEngine;

namespace BeatTiming
{
    /// <summary>
    /// The single source of truth for "where are we in the music".
    /// Time comes from the audio hardware clock (AudioSettings.dspTime), not frame time,
    /// so the beat never drifts away from the audio.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class BeatConductor : MonoBehaviour
    {
        public static BeatConductor Instance { get; private set; }

        [Header("Tempo")]
        [SerializeField, Min(1f)] float bpm = 90f;

        [Header("Music (optional - the clock runs without it)")]
        [SerializeField] AudioSource music;
        [Tooltip("Seconds from the start of the music clip to its first beat.")]
        [SerializeField] float firstBeatOffset = 0f;

        [Header("Start")]
        [SerializeField] bool playOnStart = true;
        [Tooltip("Delay before beat 0 so the audio can be scheduled precisely.")]
        [SerializeField, Min(0.1f)] float startDelay = 1f;

        /// <summary>Fired once per beat with the beat index (0, 1, 2, ...).</summary>
        public event Action<int> OnBeat;

        public float Bpm => bpm;
        public double SecondsPerBeat => 60.0 / bpm;
        public bool IsRunning { get; private set; }

        /// <summary>dspTime at which beat 0 happens.</summary>
        public double BeatZeroDspTime { get; private set; }

        /// <summary>
        /// Smooth audio-clock time. Use this instead of AudioSettings.dspTime for visuals, physics and input.
        /// It advances with game time (so it moves evenly in FixedUpdate as well as Update) and is gently
        /// pulled toward the audio clock, which itself only updates once per audio buffer.
        /// </summary>
        public double Now => Time.timeAsDouble + clockOffset;

        /// <summary>Seconds since beat 0 (negative during the start delay).</summary>
        public double SongTime => IsRunning ? Now - BeatZeroDspTime : 0.0;

        /// <summary>Position in beats, e.g. 4.25 = a quarter of the way from beat 4 to beat 5.</summary>
        public double SongBeats => SongTime / SecondsPerBeat;

        /// <summary>0..1 progress through the current beat (0 = on the beat, 0.99 = just before the next one).</summary>
        public float BeatPhase
        {
            get
            {
                double b = SongBeats;
                return b < 0 ? 0f : (float)(b - Math.Floor(b));
            }
        }

        public int NearestBeat => (int)Math.Round(SongBeats);
        public double DspTimeOfBeat(int beat) => BeatZeroDspTime + beat * SecondsPerBeat;

        /// <summary>Signed seconds from the nearest beat to the given clock time (negative = early).</summary>
        public double OffsetFromNearestBeat(double dspTime, out int nearestBeat)
        {
            double beats = (dspTime - BeatZeroDspTime) / SecondsPerBeat;
            nearestBeat = (int)Math.Round(beats);
            return dspTime - DspTimeOfBeat(nearestBeat);
        }

        // Now = game time + clockOffset; the offset follows (audio time - game time) slowly
        double clockOffset;
        double lastRawDsp;
        const double OffsetFollow = 0.05;   // share of the error corrected per audio update
        const double OffsetSnap = 0.05;     // seconds of drift after which we jump instead of easing
        int lastBeatFired = -1;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("More than one BeatConductor in the scene; using the first one.", this);
                return;
            }
            Instance = this;
            lastRawDsp = AudioSettings.dspTime;
            clockOffset = lastRawDsp - Time.timeAsDouble;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            if (playOnStart) Play();
        }

        public void Play()
        {
            double now = AudioSettings.dspTime;
            double musicStart = now + startDelay;
            BeatZeroDspTime = musicStart + firstBeatOffset;
            if (music != null && music.clip != null) music.PlayScheduled(musicStart);
            lastBeatFired = -1;
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
            if (music != null) music.Stop();
        }

        public void SetBpm(float newBpm)
        {
            if (newBpm <= 0f) return;
            // Keep the current beat position continuous when the tempo changes.
            double beats = SongBeats;
            bpm = newBpm;
            if (IsRunning) BeatZeroDspTime = Now - beats * SecondsPerBeat;
        }

        void Update()
        {
            // dspTime only advances once per audio buffer, so it "steps". Each time it moves,
            // ease the offset toward it rather than snapping, so Now never stalls or jumps.
            double raw = AudioSettings.dspTime;
            if (raw != lastRawDsp)
            {
                lastRawDsp = raw;
                double error = (raw - Time.timeAsDouble) - clockOffset;
                clockOffset += Math.Abs(error) > OffsetSnap ? error : error * OffsetFollow;
            }

            if (!IsRunning) return;

            int current = (int)Math.Floor(SongBeats);
            while (lastBeatFired < current)
            {
                lastBeatFired++;
                if (lastBeatFired >= 0) OnBeat?.Invoke(lastBeatFired);
            }
        }
    }
}
