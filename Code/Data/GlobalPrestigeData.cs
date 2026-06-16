namespace Sandbox.Data;

public sealed class GlobalPrestigeData
{
	[Property] private float EarningsBoost { get; set; } = 0.1f;
	
	public float GetEarningsBoost() { return EarningsBoost; }
}
