using Sandbox.Enums;
namespace Sandbox.Data;

public class ToolUpgradeData
{
	[Property] public int MaxLevel { get; set { field = value; AdjustPricesList(); } } = 1;
	[Property] public float Value { get; set; }
	[Property] public EToolStat ToolStat { get; set; }
	[Property] public string Description { get; set; }
	[Property, ImageAssetPath] public string Icon { get; set; }
	[Property] public List<int> Prices { get; set; }

	public override string ToString() { return ToolStat.ToString(); }
	
	private void AdjustPricesList()
	{
		Prices ??= [];
		while (Prices.Count < MaxLevel) Prices.Add(0);
		while (Prices.Count > MaxLevel) Prices.RemoveAt(Prices.Count - 1);
	}
}
