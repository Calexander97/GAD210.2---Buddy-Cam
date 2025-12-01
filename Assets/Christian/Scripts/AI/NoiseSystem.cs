using System;
using UnityEngine;

/// Lightweight global script for AI hearing.
/// Call NoiseSystem.Emit(worldPos, loudness) to broadcast a sound.
public static class NoiseSystem
{
    // loudness: 1 = baseline, >1 louder, <1 quieter
    public static event Action<Vector2, float> OnNoise;

    public static void Emit(Vector2 pos, float loudness = 1f)
        => OnNoise?.Invoke(pos, Mathf.Max(0f, loudness));
}
