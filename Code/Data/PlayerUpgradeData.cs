using Sandbox.Enums;
namespace Sandbox.Data;

public class PlayerUpgradeData
{
	[Property] public int MaxLevel { get; set { field = value; AdjustPricesList(); } } = 1;
	[Property] public int DefaultValue { get; set; }
	[Property] public List<float> Values { get; set; }
	[Property] public EPlayerUpgrade DisplayName { get; set; }
	[Property, ImageAssetPath] public string Icon { get; set; }
	[Property] public List<int> Prices { get; set; }

	public override string ToString() { return DisplayName.ToString(); }
	
	private void AdjustPricesList()
	{
		Prices ??= [];
		while (Prices.Count < MaxLevel) Prices.Add(0);
		while (Prices.Count > MaxLevel) Prices.RemoveAt(Prices.Count - 1);
		Values ??= [];
		while (Values.Count < MaxLevel) Values.Add(0);
		while (Values.Count > MaxLevel) Values.RemoveAt(Values.Count - 1);
	}
}
