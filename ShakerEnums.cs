namespace SadChromaLib.Animation;

/// <summary> Which parts of an object should be shook(eth). </summary>
public enum ShakerType
{
	Positional,
	Rotational,
	PositionAndRotation
}

/// <summary> The method used for generating the shake offset. </summary>
public enum ShakerMethod
{
	Random,
	Noise
}