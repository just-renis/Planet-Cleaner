using System;
using Sandbox.Data;
using Sandbox.Enums;
namespace Sandbox.Managers;

public sealed class GameManager : Component
{
	private bool IsHostLogicInitiated;
	private const float SaveInterval = 20f;
	private int AutoSavingHandle;
	//[Property] private List<GameObject> PlayerSpawners { get; set; }
	[Property] private Dictionary<ESound, SoundEvent> Sounds = new();

	private SpawnerManager SpawnerManager;
	
	protected override void OnStart()
	{
		SpawnerManager = Scene.GetSystem<SpawnerManager>();
		SpawnerManager.Init();
	//	EventManager.OnTeleportToSpawnLocation += GetSpawnerLocation;
		AutoSavingHandle = TimerManager.AddTimer(AutoSavingHandle, SaveInterval, true, AutoSave);
		UiManager.OnStart();
		SoundManager.Instance?.Init(Sounds);
	}
	protected override void OnUpdate()
	{
		TimerManager.OnUpdate();
		if (!Networking.IsHost) return;
		SpawnerManager.OnUpdate();
		if (!IsHostLogicInitiated) SetupHostLogic();
	}
	protected override void OnDestroy()
	{
	//	EventManager.OnTeleportToSpawnLocation -= GetSpawnerLocation;
		TimerManager.ClearTimer(AutoSavingHandle);
		TimerManager.OnDestroy();
		UiManager.OnDestroy();
		SoundManager.Instance?.OnDestroy();
	}

	/*private Vector3 GetSpawnerLocation()
	{
		return SpawnerManager[Random.Shared.Int(0, PlayerSpawners.Count - 1)].WorldPosition;
	}*/

	private void AutoSave()
	{
		PlayerData ActiveData = EventManager.TriggerGetPlayerData();
		if (ActiveData == null) return;
		if (!ActiveData.GetIsPendingForSave()) return;
		PlayerData.ApplySave(ActiveData);
	}
	private void SetupHostLogic()
	{
		IsHostLogicInitiated = true;
	}
}

