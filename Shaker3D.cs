using Godot;

using SadChromaLib.Utils.Random;

namespace SadChromaLib.Animation;

/// <summary> A utility object that can generate 'shaking' of varying intensities. </summary>
public sealed partial class Shaker3D : RefCounted
{
	private ShakerType _type;
	private ShakerMethod _method;

	private Node3D _target;
	private Vector3 _originalOffset;
	private Vector3 _originalRotation;

	private float _shakeMax = 1.0f;
	private float _shakeRotationScale = 0.05f;
	private float _shakeDecaySpeed = 0.9f;
	private float _shakeIntensity = 0.0f;
	private float _shakeAmount = 0.0f;
	private float _noiseTime = 0.0f;
	private float _time = 1.0f;

	private bool _affectPosX = true;
	private bool _affectPosY = false;
	private bool _affectPosZ = true;

	private bool _affectRotX = true;
	private bool _affectRotY = true;
	private bool _affectRotZ = false;

	private FastNoiseLite _noiseGenerator;
	private int _noiseId;

	private bool _isActive;

	public Shaker3D(Node3D target, float intensity = 1.0f, ShakerType type = ShakerType.Positional, ShakerMethod method = ShakerMethod.Random)
	{
		SetMethod(method);
		SetType(type);

		SetTarget(target);

		SetIntensity(intensity);
		_isActive = false;
	}

	#region Main Functions

	/// <summary> Stops the shaker's process loop. (This method is automatically called when shakeAmount drops to zero.) </summary>
	public void Stop()
	{
		_time = 1.0f;
		_noiseTime = 0.0f;
		_isActive = false;

		if (_method == ShakerMethod.Noise) {
			UpdateNoiseId();
		}
	}

	/// <summary> Start shaking the target object. (Note: amount is expected to be a positive value clamped between 0.0 - 1.0, apply the appropriate value depending on the intended effect. e.g. 0.15 - light, 0.45 - strong, 1.0 - full.) </summary>
	public void Shake(float amount)
	{
		_time = 1.0f;
		_isActive = true;
		_shakeAmount = Mathf.Min(_shakeMax, _shakeAmount + amount);
	}

	/// <summary> Executes the shaker's process loop. </summary>
	public void Exec(double delta)
	{
		Exec((float) delta);
	}

	/// <summary> Executes the shaker's process loop. </summary>
	public void Exec(float delta)
	{
		// Prevent unnecessary computations when the shaker is inactive
		if (!_isActive)
			return;

		// Calculate displacement
		Vector3 displace = new Vector3(
			GenerateShake(1),
			GenerateShake(2),
			GenerateShake(3)
		)
		.Normalized();

		displace.X *= _shakeAmount;
		displace.Y *= _shakeAmount;
		displace.Z *= _shakeAmount;

		// Apply displacement
		switch (_type) {
			case ShakerType.Positional:
				DisplacePosition(displace, _shakeAmount);
				break;

			case ShakerType.Rotational:
				DisplaceRotation(displace, _shakeAmount);
				break;

			case ShakerType.PositionAndRotation:
				DisplacePosition(displace, _shakeAmount);
				DisplaceRotation(displace, _shakeAmount);
				break;
		}

		// Continue processng until the shaker becomes inactive
		_time += delta;
		_noiseTime += delta;
		_shakeAmount = Mathf.Max(0.0f, _shakeAmount - (Mathf.Pow(_time, _shakeDecaySpeed) * delta));

		// Test for inactivity
		if (_shakeAmount <= 0.0f) {
			Stop();
		}
	}

	#endregion

	#region Configurators

	/// <summary> Returns a RefCounted of the shaker's noise generator. (Use this to configure its properties.) </summary>
	public FastNoiseLite GetNoiseGenerator()
	{
		return _noiseGenerator;
	}

	/// <summary> Updates the shaker's target object. </summary>
	public void SetTarget(Node3D target)
	{
		_target = target;
		Calibrate();
	}

	/// <summary> Updates the shaker's displacement target type. </summary>
	public void SetType(ShakerType type)
	{
		_type = type;
	}

	/// <summary> Update the shaker's displacement generation method. </summary>
	public void SetMethod(ShakerMethod method)
	{
		_method = method;

		if (method == ShakerMethod.Noise) {
			_noiseGenerator = new FastNoiseLite();
			UpdateNoiseId();
		}
		else {
			_noiseGenerator = null;
		}
	}

	/// <summary> Sets how strong the generated displacement values should be. </summary>
	public void SetIntensity(float intensity)
	{
		_shakeIntensity = intensity;
	}

	/// <summary> Sets the maximum allowed shake blend value. (Note: Setting this to a value higher than 1.0 may cause unexpected behaviour.) </summary>
	public void SetShakeMax(float max)
	{
		_shakeMax = max;
	}

	/// <summary> Sets how much the rotational displacement is scaled. (Rotation is a bit more finnicky, so use a small value like 0.05.) </summary>
	public void SetRotationScale(float scale)
	{
		_shakeRotationScale = scale;
	}

	/// <summary> Sets how quickly the shaker's blend drops to zero. (Decay affects the drop amount exponentially. So, for the best effect, use values like 0.5, 1.0, or 2.0.) </summary>
	public void SetDecaySpeed(float decaySpeed)
	{
		_shakeDecaySpeed = decaySpeed;
	}

	/// <summary> If the shaker affects rotation, this method sets which axes are affected by the displacement. </summary>
	public void SetRotationTargets(bool x = true, bool y = true, bool z = false)
	{
		_affectRotX = x;
		_affectRotY = y;
		_affectRotZ = z;
	}

	/// <summary> If the shaker affects position, this method sets which axes are affected by the displacement. </summary>
	public void SetPositionTargets(bool x = true, bool y = false, bool z = true)
	{
		_affectPosX = x;
		_affectPosY = y;
		_affectPosZ = z;
	}

	/// <summary> Sets the shaker's 'initial' position. Only call this if the target object changes position. </summary>
	public void Calibrate()
	{
		if (_type == ShakerType.Positional || _type == ShakerType.PositionAndRotation) {
			_originalOffset = _target.Position;
		}

		if (_type == ShakerType.Rotational || _type == ShakerType.PositionAndRotation) {
			_originalRotation = _target.Rotation;
		}

		_time = 0.0f;
	}

	#endregion

	#region Helpers

	/// <summary> Updates the shaker's noise id. </summary>
	private void UpdateNoiseId()
	{
		_noiseId = RandomUtils.BasicInt() % 2048;
	}

	/// <summary> Generates displacement using the chosen shake method. (The parameter 'id' is only used by noise-based generators.) </summary>
	private float GenerateShake(int id)
	{
		return _method switch {
			ShakerMethod.Random => RandomUtils.Random(-1.0f, 1.0f),
			ShakerMethod.Noise => _noiseGenerator.GetNoise1D(_time * 2.0f * _noiseId * id),
			_ => 0.0f,
		};
	}

	/// <summary> Applies the shaker's positional displacement to the target object. </summary>
	private void DisplacePosition(Vector3 displace, float blend)
	{
		Vector3 position = _originalOffset;

		if (_affectPosX)
			position.X += displace.X;

		if (_affectPosY)
			position.Y += displace.Y;

		if (_affectPosZ)
			position.Z += displace.Z;

		_target.Position = _target.Position.Lerp(position, blend);
	}

	/// <summary> Applies the shaker's rotational displacement to the target object. </summary>
	private void DisplaceRotation(Vector3 displace, float blend)
	{
		Vector3 rotation = _originalRotation;

		if (_affectRotX)
			rotation.X += displace.X * _shakeRotationScale;

		if (_affectRotY)
			rotation.Y += displace.Y * _shakeRotationScale;

		if (_affectRotZ)
			rotation.Z += displace.Z * _shakeRotationScale;

		_target.Rotation = _target.Rotation.Lerp(rotation, blend);
	}

	#endregion
}