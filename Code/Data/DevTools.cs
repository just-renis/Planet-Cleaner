using Sandbox.Managers;

namespace Sandbox.Data;

public sealed class DevTools : Component
{
	[Property, Group("Save File")] 
	private float DebugMoneyAmount { get; set; } = 1000f;

	[Button("Reset Save File"), Group("Save File")]
	public void ResetSave()
	{
		if (!Game.IsEditor) return;
		if (FileSystem.Data.FileExists("save.dat")) FileSystem.Data.DeleteFile("save.dat");
		else Log.Warning("No save file found to delete.");
	}
	[Button("Add Money"), Group("Save File")]
	public void AddMoney()
	{
		if (!Game.IsEditor || GetPlayerSaveData() == null) return;
		GetPlayerSaveData().AddMoney(DebugMoneyAmount);
	}
	[Button("Remove Money"), Group("Save File")]
	public void SubtractMoney()
	{
		if (!Game.IsEditor || GetPlayerSaveData() == null) return;
		GetPlayerSaveData().SubtractMoney(DebugMoneyAmount);
	}

	private PlayerData GetPlayerSaveData()
	{
		return EventManager.TriggerGetPlayerData();
	}
}
