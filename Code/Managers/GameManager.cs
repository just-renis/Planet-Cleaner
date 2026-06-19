using Sandbox.Data;
using Sandbox.Enums;
namespace Sandbox.Managers;

public sealed class GameManager : Component
{
	[Property] private Dictionary<ESound, SoundEvent> Sounds = new();
	private bool IsHostLogicInitiated;
	private int AutoSavingHandle;
	private const float SaveInterval = 20f;

	private SpawnerManager SpawnerManager;
	
	protected override void OnStart()
	{
		SpawnerManager = Scene.GetSystem<SpawnerManager>();
		SpawnerManager.Init();
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
		TimerManager.ClearTimer(AutoSavingHandle);
		TimerManager.OnDestroy();
		UiManager.OnDestroy();
		SoundManager.Instance?.OnDestroy();
	}

	private void AutoSave()
	{
		PlayerData ActiveData = PlayerSaveManager.Instance.Data;
		if (ActiveData == null) return;
		if (!ActiveData.GetIsPendingForSave()) return;
		PlayerData.ApplySave(ActiveData);
	}
	private void SetupHostLogic()
	{
		IsHostLogicInitiated = true;
	}
}

