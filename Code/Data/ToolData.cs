using Sandbox.Enums;
namespace Sandbox.Data;

public sealed class ToolData
{
	[Property] public int UnlockPrice { get; set; }
	[Property] public ETool ToolName { get; set; }
	[Property, ImageAssetPath] public string Icon { get; set; }
	[Property] public Dictionary<EToolStat, ToolUpgradeData> Upgrades { get; set; }
	
	[Property] public int Power { get; set; }
	[Property] public float Range { get; set; }
	[Property] public float CollectCooldown { get; set; }

	public float GetToolStatValue(EToolStat InToolStat)
	{
		if (Upgrades == null) return 0f;
		if (!Upgrades.TryGetValue(InToolStat, out ToolUpgradeData UpgradeData) || UpgradeData == null) return 0f;
		return UpgradeData.Value;
	}
	public override string ToString() { return ToolName.ToString(); }
}
