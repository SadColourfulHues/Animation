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

    public void SetTimeScale(StringName key, float scale)
    {
        _tree.Set(key, scale);
    }

    public void SetBlend(StringName key, float position)
    {
        _tree.Set(key, position);
    }

    public void SetBlend(StringName key, Vector2 position)
    {
        _tree.Set(key, position);
    }

    public void SetOneShot(StringName key, AnimationNodeOneShot.OneShotRequest request)
    {
        _tree.Set(key, (int) request);
    }

    public void SetState(StringName key, string value)
    {
        _tree.Set(key, value);
    }

    #endregion

    #region Blenders

    public void BlendFloat(StringName key, float value, float fac)
    {
        _tree.Set(key, Mathf.Lerp((float) _tree.Get(key), value, fac));
    }

    public void BlendPosition(StringName key, Vector2 value, float fac)
    {
        _tree.Set(key, ((Vector2) _tree.Get(key)).Lerp(value, fac));
    }

    public void BlendTimeScale(StringName key, float scale, float fac) => BlendFloat(key, scale, fac);
    public void BlendInterpolated(StringName key, float position, float fac) => BlendFloat(key, position, fac);

    #endregion

    #region Key Getters

    public static StringName GetKeyBlendPosition(string keyName)
    {
        return new($"parameters/{keyName}/blend_position");
    }

    public static StringName GetKeyBlendAmount(string keyName)
    {
        return new($"parameters/{keyName}/blend_amount");
    }

    public static StringName GetKeyTimeScale(string keyName)
    {
        return new($"parameters/{keyName}/scale");
    }

    public static StringName GetKeyTransition(string keyName)
    {
        return new($"parameters/{keyName}/transition_request");
    }

    public static StringName GetKeyOneShot(string keyName)
    {
        return new($"parameters/{keyName}/request");
    }

    #endregion
}