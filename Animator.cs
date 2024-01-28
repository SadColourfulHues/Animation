using System.Runtime.CompilerServices;
using Godot;

namespace SadChromaLib.Animation;

/// <summary>
/// A helper class for managing a blend-tree animation controller
/// </summary>
public sealed class Animator
{
    private readonly AnimationTree _tree;

    public Animator(AnimationTree tree)
    {
        _tree = tree;
    }

    #region Setters

    public void SetTimeScale(StringName key, float scale) {
        _tree.Set(key, scale);
    }

    public void SetBlend(StringName key, float position) {
        _tree.Set(key, position);
    }

    public void SetBlend(StringName key, Vector2 position) {
        _tree.Set(key, position);
    }

    public void SetOneShot(
        StringName key,
        AnimationNodeOneShot.OneShotRequest request)
    {
        _tree.Set(key, (int) request);
    }

    public void SetState(StringName key, string value) {
        _tree.Set(key, value);
    }

    #endregion

    #region Blenders

    public void BlendFloat(StringName key, float value, float fac) {
        _tree.Set(key, Mathf.Lerp((float) _tree.Get(key), value, fac));
    }

    public void BlendPosition(StringName key, Vector2 value, float fac) {
        _tree.Set(key, ((Vector2) _tree.Get(key)).Lerp(value, fac));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendTimeScale(StringName key, float scale, float fac)
        => BlendFloat(key, scale, fac);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendInterpolated(StringName key, float position, float fac)
        => BlendFloat(key, position, fac);

    #endregion

    #region Getters

    /// <summary>
    /// Must only be called on a wrapped AnimationTree with a state machine as its root.
    /// </summary>
    /// <returns></returns>
    public AnimationNodeStateMachinePlayback GetPlayback() {
        return (AnimationNodeStateMachinePlayback) _tree.Get("parameters/playback");
    }

    #endregion

    #region Key Getters

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringName GetKeyBlendPosition(string keyName) {
        return $"parameters/{keyName}/blend_position";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringName GetKeyBlendAmount(string keyName) {
        return $"parameters/{keyName}/blend_amount";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringName GetKeyTimeScale(string keyName) {
        return $"parameters/{keyName}/scale";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringName GetKeyTransition(string keyName) {
        return $"parameters/{keyName}/transition_request";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringName GetKeyPlayback(string keyName) {
        return $"parameters/{keyName}/playback";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringName GetKeyOneShot(string keyName) {
        return $"parameters/{keyName}/request";
    }

    #endregion
}