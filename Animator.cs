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

    public void SetTimeScale(string key, float scale) {
        _tree.Set(key, scale);
    }

    public void SetBlend(string key, float position) {
        _tree.Set(key, position);
    }

    public void SetBlend(string key, Vector2 position) {
        _tree.Set(key, position);
    }

    public void SetOneShot(
        string key,
        AnimationNodeOneShot.OneShotRequest request)
    {
        _tree.Set(key, (int) request);
    }

    /// <summary>
    /// Shorthand for SetOneShot([key], AnimationNodeOneShot.OneShotRequest.Fire)
    /// </summary>
    public void OneShotFire(string key) {
        _tree.Set(key, (int) AnimationNodeOneShot.OneShotRequest.Fire);
    }

    /// <summary>
    /// Shorthand for SetOneShot([key], AnimationNodeOneShot.OneShotRequest.Abort)
    /// </summary>
    public void OneShotStop(string key) {
        _tree.Set(key, (int) AnimationNodeOneShot.OneShotRequest.Abort);
    }

    /// <summary>
    /// Shorthand for SetOneShot([key], AnimationNodeOneShot.OneShotRequest.FadeOut)
    /// </summary>
    public void OneShotFadeOut(string key) {
        _tree.Set(key, (int) AnimationNodeOneShot.OneShotRequest.FadeOut);
    }

    public void SetState(string key, string value) {
        _tree.Set(key, value);
    }

    #endregion

    #region Blenders

    public void BlendFloat(string key, float value, float fac) {
        _tree.Set(key, Mathf.Lerp((float) _tree.Get(key), value, fac));
    }

    public void BlendPosition(string key, Vector2 value, float fac) {
        _tree.Set(key, ((Vector2) _tree.Get(key)).Lerp(value, fac));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendTimeScale(string key, float scale, float fac)
        => BlendFloat(key, scale, fac);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendInterpolated(string key, float position, float fac)
        => BlendFloat(key, position, fac);

    #endregion

    #region Getters

    /// <summary>
    /// Returns the current animation's root motion offset and rotation relative to a given 3D node.
    /// </summary>
    public RootMotionInfo GetRootMotion(Node3D relativeTo)
    {
        Quaternion rotation = relativeTo.Quaternion * _tree.GetRootMotionRotation();
        Vector3 offset = rotation * _tree.GetRootMotionPosition();

        return new(offset, rotation.GetEuler(relativeTo.RotationOrder));
    }

    /// <summary>
    /// Returns the current animation's root motion offset and rotation relative to a given 3D node.
    /// <para>
    /// (This variant restricts rotation around the Y-axis.)
    /// </para>
    /// </summary>
    public RootMotionInfo GetRootMotionY(Node3D relativeTo)
    {
        Quaternion rotation = relativeTo.Quaternion * _tree.GetRootMotionRotation();
        Vector3 offset = rotation * _tree.GetRootMotionPosition();

        Vector3 rotationEuler = relativeTo.Rotation;
        rotationEuler.Y = rotation.GetEuler(relativeTo.RotationOrder).Y;

        return new(offset, rotationEuler);
    }

    /// <summary>
    /// Must only be called on a wrapped AnimationTree with a state machine as its root.
    /// </summary>
    /// <returns></returns>
    public AnimationNodeStateMachinePlayback GetPlayback() {
        return (AnimationNodeStateMachinePlayback) _tree.Get("parameters/playback");
    }

    /// <summary>
    /// Returns an animation node in a blend tree.
    /// (Will return null if the root node is not a blend tree, or if the node in question does not exist.)
    /// </summary>
    /// <param name="nodeName">The name of the node in the blend tree.</param>
    /// <param name="node">An AnimationNode variable to write the node to.</param>
    /// <returns></returns>
    public bool GetBlendTreeNode(string nodeName, out AnimationNode node)
    {
        if (_tree?.TreeRoot is not AnimationNodeBlendTree root ||
            !root.HasNode(nodeName))
        {
            node = null;
            return false;
        }

        node = root.GetNode(nodeName);
        return true;
    }

    /// <summary>
    /// Returns an animation node in a blend tree.
    /// (Will return null if the root node is not a blend tree, or if the node in question does not exist.)
    /// </summary>
    /// <param name="nodeName">The name of the node in the blend tree.</param>
    /// <param name="node">An AnimationNode variable to write the node to.</param>
    /// <returns></returns>
    public bool GetBlendTreeNode<T>(string nodeName, out T node)
        where T: AnimationNode
    {
        if (_tree?.TreeRoot is not AnimationNodeBlendTree root ||
            !root.HasNode(nodeName) ||
            root.GetNode(nodeName) is not T treeNode)
        {
            node = null;
            return false;
        }

        node = treeNode;
        return true;
    }

    #endregion

    #region Key Getters

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetKeyBlendPosition(string keyName) {
        return $"parameters/{keyName}/blend_position";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetKeyBlendAmount(string keyName) {
        return $"parameters/{keyName}/blend_amount";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetKeyTimeScale(string keyName) {
        return $"parameters/{keyName}/scale";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetKeyTransition(string keyName) {
        return $"parameters/{keyName}/transition_request";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetKeyPlayback(string keyName) {
        return $"parameters/{keyName}/playback";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetKeyOneShot(string keyName) {
        return $"parameters/{keyName}/request";
    }

    #endregion

    #region Structs

    public readonly struct RootMotionInfo
    {
        public readonly Vector3 Offset;
        public readonly Vector3 Rotation;

        public RootMotionInfo(Vector3 offset, Vector3 rotation)
        {
            Offset = offset;
            Rotation = rotation;
        }
    }

    #endregion
}