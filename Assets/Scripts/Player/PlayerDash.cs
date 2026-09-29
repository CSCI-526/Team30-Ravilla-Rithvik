using BeatTiming;
using UnityEngine;
using UnityEngine.InputSystem;

// Which press pattern triggers a dash. Switchable in the Inspector (even during Play) to compare them.
public enum DashMode
{
    TwoBeats,   // press on a beat, then again on the very next beat
    SingleBeat, // every on-beat press dashes straight away
    DoubleTap   // press on a beat, then again on the half-beat right after it ("tap-tap")
}

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerMovement))]
public class PlayerDash : MonoBehaviour
{
    public DashMode mode = DashMode.TwoBeats;

    //Dash distance depends on how close to the beat Space was pressed
    public float perfectDashDistance = 4f;
    public float goodDashDistance = 2.5f;
    public float dashDuration = 0.12f;

    public bool IsDashing => dashTimer > 0f;
    // True while the current dash came from two Perfect presses (only these break blue enemies)
    public bool IsPerfectDash => IsDashing && perfectDash;
    // True after a successful first press, waiting for the second press (next beat or half-beat)
    public bool IsCharged => chargedBeat != NoCharge;

    private const int NoCharge = int.MinValue;

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private float normalGravity;
    private float dashTimer;
    private float dashSpeed;
    private bool perfectDash;
    private int chargedBeat = NoCharge;
    private Accuracy chargedTier;
    private DashMode lastMode;
    private BeatMetronome metronome;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        normalGravity = rb.gravityScale;
        lastMode = mode;
        metronome = FindFirstObjectByType<BeatMetronome>();
    }

    void Update()
    {
        TimingJudge judge = TimingJudge.Instance;
        if (judge == null) return;

        // Switching mode mid-play (e.g. from the Inspector) starts fresh
        if (mode != lastMode)
        {
            chargedBeat = NoCharge;
            lastMode = mode;
        }

        // The half-beat tick helps players find the second tap
        if (metronome != null) metronome.OffbeatTicks = mode == DashMode.DoubleTap;

        // Drop the charge once the second press's window has passed, so the UI doesn't show a stale charge
        if (IsCharged && judge.Conductor != null && judge.Conductor.Now > ChargeExpiry(judge))
        {
            chargedBeat = NoCharge;
        }

        // Judge in Update so the press is timed on the exact frame it happened
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && !IsDashing)
        {
            HandlePress(judge);
        }
    }

    private double ChargeExpiry(TimingJudge judge)
    {
        BeatConductor conductor = judge.Conductor;
        if (mode == DashMode.DoubleTap)
            return conductor.DspTimeOfBeat(chargedBeat) + conductor.SecondsPerBeat * 0.5 + judge.HalfBeatGoodWindow + judge.InputLatency;
        return conductor.DspTimeOfBeat(chargedBeat + 1) + judge.GoodWindow + judge.InputLatency;
    }

    // Cancels an active dash and any stored charge, e.g. when the player respawns
    public void ResetDash()
    {
        if (IsDashing) rb.gravityScale = normalGravity;
        dashTimer = 0f;
        chargedBeat = NoCharge;
    }

    private void HandlePress(TimingJudge judge)
    {
        if (mode == DashMode.SingleBeat)
        {
            Judgement single = judge.Judge();
            if (single.IsHit) StartDash(single.Tier == Accuracy.Perfect);
            return;
        }

        if (mode == DashMode.DoubleTap && IsCharged)
        {
            // Second tap is timed against the half-beat after the first one
            Judgement second = judge.JudgeHalfBeat(chargedBeat);
            if (second.IsHit) StartDash(chargedTier == Accuracy.Perfect && second.Tier == Accuracy.Perfect);
            chargedBeat = NoCharge;
            return;
        }

        HandleChargedPress(judge.Judge());
    }

    // Two on-beat presses: the first charges the dash, the second (next beat, TwoBeats mode) fires it
    private void HandleChargedPress(Judgement judgement)
    {
        // Miss: lose the charge, the pulse ring flashes red
        if (!judgement.IsHit)
        {
            chargedBeat = NoCharge;
            return;
        }

        if (mode == DashMode.TwoBeats && IsCharged && judgement.Beat == chargedBeat + 1)
        {
            // Perfect dash only if both presses were Perfect
            bool bothPerfect = chargedTier == Accuracy.Perfect && judgement.Tier == Accuracy.Perfect;
            StartDash(bothPerfect);
            chargedBeat = NoCharge;
            return;
        }

        // First press, or the previous charge was too old: this press starts a new charge
        chargedBeat = judgement.Beat;
        chargedTier = judgement.Tier;
    }

    private void FixedUpdate()
    {
        if (!IsDashing) return;

        // Straight horizontal dash, no gravity
        rb.linearVelocity = new Vector2(dashSpeed, 0f);

        dashTimer -= Time.fixedDeltaTime;
        if (dashTimer <= 0f)
        {
            rb.gravityScale = normalGravity;
        }
    }

    private void StartDash(bool perfect)
    {
        perfectDash = perfect;
        float distance = perfect ? perfectDashDistance : goodDashDistance;
        dashSpeed = movement.FacingDirection * distance / dashDuration;
        dashTimer = dashDuration;
        rb.gravityScale = 0f;
    }
}
