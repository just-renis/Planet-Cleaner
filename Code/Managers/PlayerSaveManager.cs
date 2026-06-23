using Sandbox.Data;
namespace Sandbox.Managers;

public sealed class PlayerSaveManager : Component
{
	public static PlayerSaveManager Instance { get; private set; }
    
	public PlayerData Data { get; private set; }

	protected override void OnAwake()
	{
		if (Instance != null && Instance != this)
		{
			GameObject.Destroy();
			return;
		}
		Instance = this;
		LoadSaveData();
	}
	protected override void OnDestroy()
	{
		if (Instance == this) Instance = null;
	}

	private void LoadSaveData()
	{
		Data = PlayerData.Load(); 
	}
}

