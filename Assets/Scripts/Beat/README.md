# Beat Timing (issue #1)

Rhythm system for the prototype: beat clock, Perfect/Good/Miss judging, visual beat cue, and tick sound.
Built and tested in Unity 6000.3.22f1 (the project's version). No art or audio assets needed.

## Try it
1. Open `Assets/Scenes/BeatTest.unity`. If it's missing, recreate it with **Tools → Beat Timing → Create Test Scene**.
2. Press Play, click in the Game view, and press Space when the ring flashes.

It doesn't touch `SampleScene` or `InputSystem_Actions`. The test scene reads the keyboard directly.

## Test controls
| Key | Action |
|---|---|
| Space | Dash (placeholder) |
| 1 | Mute the tick sound, to check the game still works with sound off |
| 2 | Hide the ring, to check it still works from sound alone |
| 3 / 4 | BPM −5 / +5 |
| 5 | Reset stats (test scene only) |

The number keys are test-only and won't clash with movement keys. Space is the placeholder Dash.

The placeholder square dashes 4 units on a Perfect, 2.5 on a Good, and shakes red on a Miss.

## What's in it
| Script | Job |
|---|---|
| `BeatConductor` | Beat clock based on `AudioSettings.dspTime`, so it stays in sync with the audio. BPM, optional music track, `OnBeat` event. |
| `TimingJudge` | `Judge()` → Perfect / Good / Miss plus the offset in ms. `OnJudged` event. Allows only one scored press per beat, so mashing doesn't work. |
| `BeatPulseRing` | **Main visual cue.** A ring shrinks onto the character, turns cyan in the Good window and gold in the Perfect window, and flashes on the beat. Add it to the player. |
| `BeatIconPulse` | Secondary cue. The Dash icon pulses on each beat and lights up while a press would count. Works with a UI Image or a SpriteRenderer. |
| `BeatMetronome` | A tick on each beat (a louder one every 4th), scheduled to the audio clock. It supports the visual cue but isn't required. |
| `BeatTestHarness` | Test scene only. Not for the real game. |

## Tuning (Inspector)
- **TimingJudge:** Perfect window (0.10s), Good window (0.25s), Input Latency.
- **BeatConductor:** BPM (90), First Beat Offset (when using a real music track), Start Delay.
- **BeatPulseRing:** start and end size, how early the ring appears, colors.

**Calibrating latency:** play about 20 presses in the test scene. If the "Average hit offset" is consistently around +40ms, set Input Latency to 0.04.

Note: at 90 BPM a beat lasts 0.667s. A ±0.25s Good window covers 75% of it, so there's only a 0.17s gap that counts as a Miss. If Miss feels too hard to get, narrow the Good window or lower the BPM.

## For teammates: using it from Dash (#2) or the barrier (#4)
```csharp
using BeatTiming;

// In the Dash code, when the Dash button is pressed:
Judgement j = TimingJudge.Instance.Judge();
switch (j.Tier)
{
    case Accuracy.Perfect: Dash(longDistance); break;
    case Accuracy.Good:    Dash(normalDistance); break;
    case Accuracy.Miss:    FailDash(); break;
}

// Anything else can listen without calling Judge():
TimingJudge.Instance.OnJudged += j => Debug.Log(j);   // "PERFECT +32ms"
BeatConductor.Instance.OnBeat += beat => { /* pulse the barrier, etc. */ };
```
Only the Dash code should call `Judge()`, once per press. Everything else should subscribe to the events.

**Shortcut:** select the Player and click **Tools → Beat Timing → Add To Current Scene**. This adds the Beat System, the ring sized to the player, and `BeatDebugInput`, which lets Space trigger a scored press until the real Dash exists. Remove `BeatDebugInput` once Dash calls `Judge()` itself.

To add it by hand: place one GameObject with `BeatConductor` + `TimingJudge` + `BeatMetronome`, add `BeatPulseRing` to the player, and add `BeatIconPulse` to the Dash icon.

## Dash modes (PlayerDash → Mode)
Switchable in the Inspector, even during Play, so the team can compare them:
| Mode | Press pattern |
|---|---|
| **Two Beats** (default) | Press on a beat, then again on the very next beat |
| **Single Beat** | Every on-beat press dashes |
| **Double Tap** | Press on a beat, then again on the half-beat right after it. A soft half-beat tick plays in this mode. |

A dash is Perfect (long, and breaks the blue enemy) only if every press in the pattern was Perfect.
`JudgementPopup` on the Player shows PERFECT / GOOD / MISS above the player on every press.
