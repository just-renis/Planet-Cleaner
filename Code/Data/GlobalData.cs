using System;
using Sandbox.Enums;
namespace Sandbox.Data;

public sealed class GlobalData : Component
{
	public struct TrashGroup
	{
		[Property] public List<Model> Models { get; set; }
	}
	
	public static GlobalData Instance { get; private set; }
	[Property] private Dictionary<EPrestigeUpgradeName, float> UpgradesValues { get; set; }
	[Property] private Dictionary<EPrestigeUpgradeName, int> UpgradesMaxAmounts { get; set; }
	[Property] private Dictionary<EDefaultAttribute, float> DefaultAttributes { get; set; }
	[Property] private Dictionary<ETool, ToolData> ToolsInformation { get; set; }
	[Property] private Dictionary<EPlayerUpgrade, PlayerUpgradeData> PlayerUpgradesInformation { get; set; }
	[Property] private Dictionary<ETrashType, TrashGroup> TrashTypes { get; set; }

	public float GetDefaultPlayerData(EPlayerUpgrade InPlayerUpgradeName)
	{
		return PlayerUpgradesInformation.TryGetValue(InPlayerUpgradeName, out PlayerUpgradeData UpgradeData) ? UpgradeData.DefaultValue : 0f;
	}
	public float GetProcessedUpgradeValue(int InUpgradeAmount, EPrestigeUpgradeName InUpgradeName) { return Math.Clamp(InUpgradeAmount, 0, GetUpgradeMaxAmount(InUpgradeName)) * GetUpgradeValue(InUpgradeName); }
	public float GetUpgradeValue(EPrestigeUpgradeName InUpgradeName) { return UpgradesValues.GetValueOrDefault(InUpgradeName, 0f); }
	public float GetDefaultAttributeValue(EDefaultAttribute InDefaultAttribute) { return DefaultAttributes.GetValueOrDefault(InDefaultAttribute, 0f); }
	public ToolData GetToolData(ETool InTool) { return ToolsInformation[InTool]; }
	public Model GetRandomTrashModel(ETrashType InTrashType)
	{
		if (!TrashTypes.TryGetValue(InTrashType, out TrashGroup OutTrashGroup) || OutTrashGroup.Models == null) return null;
		int Count = OutTrashGroup.Models.Count;
		if (Count == 0) return null;
		int RandomIndex = Game.Random.Int(0, Count - 1);
		return OutTrashGroup.Models[RandomIndex];
	}
	public Dictionary<ETool, ToolData> GetToolsInformation() { return ToolsInformation; }
	public Dictionary<EPlayerUpgrade, PlayerUpgradeData> GetPlayerUpgradesInformation() { return PlayerUpgradesInformation; }
	
	protected override void OnAwake() { Instance = this; }
	protected override void OnDestroy() { if (Instance == this) Instance = null; }

	private int GetUpgradeMaxAmount(EPrestigeUpgradeName InUpgradeName) { return UpgradesMaxAmounts.GetValueOrDefault(InUpgradeName, 0); }
}
