using Godot;

namespace SadChromaLib.Animation;

/// <summary> A utility object that applies simple forces to a body. </summary>
public sealed partial class SimpleForce3D : RefCounted
{
	private bool _isActive;
	private float _timeActive;

	private Node3D _body;
	private Vector3 _forces;

	public SimpleForce3D(Node3D body)
	{
		_body = body;

		_timeActive = 1.0f;
		_forces = Vector3.Zero;
		_isActive = false;
	}

	/// <summary> Execute's the SimpleForce's step function. </summary>
	public Vector3 Exec(double delta)
	{
		return Exec((float) delta);
	}

	/// <summary> Execute's the SimpleForce's step function. </summary>
	public Vector3 Exec(float delta)
	{
		// No forces are active, terminate early
		if (!_isActive) {
			return Vector3.Zero;
		}

		// Once the active forces drop to zero, disable the controller
		if (_forces.LengthSquared() <= 1.0f) {
			ClearForces();
			_timeActive = 1.0f;
			return _forces;
		}

		float modFac = Mathf.Lerp(0.05f, 0.35f, _timeActive * 0.01f);

		_forces = _forces.Lerp(Vector3.Zero, modFac);
		_timeActive += delta;

		// Slowly drop forces back to zero
		return _forces;
	}

	#region State Methods

	/// <summary> Abruptly removes all forces acting on a body. </summary>
	public void ClearForces()
	{
		_forces = Vector3.Zero;
		_isActive = false;
	}

	/// <summary> Adds a force of a given direction and magnitude to a body. </summary>
	public void AddForce(Vector3 force)
	{
		_forces += force;
		_isActive = true;
	}

	/// <summary> Adds a force of a given magnitude that pushes the body forward. </summary>
	public void AddForwardForce(float intensity)
	{
		AddForce(_body.Basis.Z * intensity);
	}

	/// <summary> Adds a force of a given magnitude that pushes the body to its right side. </summary>
	public void AddSideForceRight(float intensity)
	{
		AddForce(_body.Basis.X * intensity);
	}

	#endregion
}