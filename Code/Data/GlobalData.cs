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
	[Property] private Dictionary<EUpgradeName, float> UpgradesValues { get; set; }
	[Property] private Dictionary<EUpgradeName, int> UpgradesMaxAmounts { get; set; }
	[Property] private Dictionary<EDefaultAttribute, float> DefaultAttributes { get; set; }
	[Property] private Dictionary<ETool, ToolData> ToolsInformation { get; set; }
	[Property] private Dictionary<ETrashType, TrashGroup> TrashTypes { get; set; }

	public float GetProcessedUpgradeValue(int InUpgradeAmount, EUpgradeName InUpgradeName) { return Math.Clamp(InUpgradeAmount, 0, GetUpgradeMaxAmount(InUpgradeName)) * GetUpgradeValue(InUpgradeName); }
	public float GetUpgradeValue(EUpgradeName InUpgradeName) { return UpgradesValues.GetValueOrDefault(InUpgradeName, 0f); }
	public float GetDefaultAttributeValue(EDefaultAttribute InDefaultAttribute) { return DefaultAttributes.GetValueOrDefault(InDefaultAttribute, 0f); }
	public Model GetRandomTrashModel(ETrashType InTrashType)
	{
		if (!TrashTypes.TryGetValue(InTrashType, out TrashGroup OutTrashGroup) || OutTrashGroup.Models == null) return null;
		int Count = OutTrashGroup.Models.Count;
		if (Count == 0) return null;
		int RandomIndex = Game.Random.Int(0, Count - 1);
		return OutTrashGroup.Models[RandomIndex];
	}
	public Dictionary<ETool, ToolData> GetToolsInformation() { return ToolsInformation; }
	
	protected override void OnAwake() { Instance = this; }
	protected override void OnDestroy() { if (Instance == this) Instance = null; }

	private int GetUpgradeMaxAmount(EUpgradeName InUpgradeName) { return UpgradesMaxAmounts.GetValueOrDefault(InUpgradeName, 0); }
}
