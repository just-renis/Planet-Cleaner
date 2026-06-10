using Sandbox.Enums;
namespace Sandbox.Data;

public sealed class ToolData
{
	public class ToolUpgrade
	{
		[Property] public int MaxLevel { get; set; }
		[Property] public string DisplayName { get; set; }
		[Property, ImageAssetPath] public string Icon { get; set; }
		[Property] public List<int> Prices { get; set; }
	}

	[Property] public int BaseUnlockPrice { get; set; }
	[Property] public string ToolName { get; set; }
	[Property] public string Description { get; set; }
	[Property, ImageAssetPath] public string IconName { get; set; }
	[Property] public Dictionary<EToolStat, ToolUpgrade> Upgrades { get; set; }
}
