using Sandbox.Enums;
namespace Sandbox.Data;

public class PlayerUpgradeData
{
	[Property] public int MaxLevel { get; set; }
	[Property] public int DefaultValue { get; set; }
	[Property] public List<float> Values { get; set; }
	[Property] public EPlayerUpgrade DisplayName { get; set; }
	[Property, ImageAssetPath] public string Icon { get; set; }
	[Property] public List<int> Prices { get; set; }
	[Property] public Dictionary<int, float> CachedCumulativeValues { get; set; } = [];

	public float GetCachedCumulativeValue(int InAmount) { return CachedCumulativeValues.GetValueOrDefault(InAmount, 0f); }
	public override string ToString() { return DisplayName.ToString(); }
	
	[Button("Recalculate Sums")]
	public void RecalculateSums()
	{
		if (Values == null || Values.Count == 0 || CachedCumulativeValues == null) return;
		List<int> Keys = new (CachedCumulativeValues.Keys);
		foreach (int LevelCount in Keys)
		{
			int ElementsToSum = System.Math.Min(LevelCount, Values.Count);
			CachedCumulativeValues[LevelCount] = 0f;
			for (int I = 0; I < ElementsToSum; I++) CachedCumulativeValues[LevelCount] += Values[I];
		}
	}
}
