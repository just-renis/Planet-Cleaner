using System;
using Sandbox.Citizen;
using Sandbox.Data;
using Sandbox.Enums;
using Sandbox.Interfaces;
using Sandbox.Managers;

public sealed class Player : Component
{
	[Property] private float Sensitivity { get; set; } = 0.2f;
	[Property] private float CameraDistance { get; set; } = 200f;
	[Property] private float CameraHeight { get; set; } = 50f;
	[Property] private float DefaultMovementSpeed { get; set; } = 2500f;
	[Property] private float JumpStrength { get; set; } = 350f;
	[Property] private float DefaultMoneyMultiplier { get; set; } = 1f;
	[Property] private int DefaultTrashCapacity { get; set; } = 10;
	[Property] private Rigidbody PlayerRigidbody { get; set; }
	[Property] private CitizenAnimationHelper AnimationHelper { get; set; }
	private bool IsGrounded;
	private float MovementSpeed;
	private float CollectRadius;
	private float CollectCooldownTimer;
	private float CollectCooldown;
	private float MoneyMultiplier;
	private int MaximumCollectAmount;
	private int TrashCapacity;
	private Angles CurrentCameraRotation;
	private CameraComponent Camera;
	private PlayerData Data;
	private GlobalData GlobalInstance;
	
	public int GetTrashCollected() { return Data.GetCurrentTrashAmount(); }
	public int GetTrashCapacity() { return TrashCapacity; }
	public double GetMoney() { return Data.GetMoney(); }
	
	protected override void OnUpdate()
	{
		if (IsProxy) return;
		ProcessInput();
		if (Time.Now < CollectCooldownTimer) return;
		CollectCooldownTimer = Time.Now + CollectCooldown;
		ScanAndCollect();
	}
	protected override void OnStart()
	{
		if (IsProxy) return;
		GlobalInstance = GlobalData.Instance;
		SoundManager.Instance.Play2D(ESound.MainSoundtrack);
		Camera = Scene.GetComponentInChildren<CameraComponent>();
		EventManager.OnPlayerDataRequired += GetPlayerData;
		EventManager.OnRequestPlayerRef += GetThisRef;
		EventManager.OnTrashSold += SellTrash;
		EventManager.OnUiStateChanged += UiStateChanged;
		EventManager.OnUpgradePurchased += PrestigeUpgradePurchased;
		EventManager.OnToolUpgradePurchased += ToolUpgradePurchased;
		EventManager.OnToolChanged += ApplyToolStats;
		LoadSaveData();
		ApplyUpgradesOnJoin();
	}
	protected override void OnDestroy()
	{
		if (Data != null && Data.GetIsPendingForSave()) PlayerData.ApplySave(Data);
		if (IsProxy) return;
		EventManager.OnPlayerDataRequired -= GetPlayerData;
		EventManager.OnRequestPlayerRef -= GetThisRef;
		EventManager.OnTrashSold -= SellTrash;
		EventManager.OnUiStateChanged -= UiStateChanged;
		EventManager.OnUpgradePurchased -= PrestigeUpgradePurchased;
		EventManager.OnToolUpgradePurchased -= ToolUpgradePurchased;
		EventManager.OnToolChanged -= ApplyToolStats;
	}

	private PlayerData GetPlayerData() { return Data; }
	private Player GetThisRef() { return this; }

	private void UiStateChanged(EUi InUi, bool InState)
	{
		if (!InState) return;
		PlayerRigidbody.Velocity = new Vector3(0, 0, PlayerRigidbody.Velocity.z);
		AnimationHelper.WithVelocity(Vector3.Zero);
	}
	private void SellTrash()
	{
		if (Data.GetCurrentTrashAmount() <= 0) return;
		Data.AddMoney(MoneyMultiplier * Data.GetCurrentTrashAmount());
		Data.ResetTrash();
	}
	private void ProcessInput()
	{
		ProcessMouseInput();
		ProcessKeyboardInput();
	}
	private void ProcessMouseInput()
	{
		if (UiManager.IsAnyUiOpen()) return;
	//	ProcessMouseRightClickInput();
	//	ProcessMouseLeftClickInput();
	}
	private void ProcessKeyboardInput()
	{
		if (Input.Pressed("Score")) UiManager.ChangeUiState(EUi.PrestigeUpgrades);
		if (Input.EscapePressed) ProcessEscape();
		if (UiManager.IsAnyUiOpen()) return;
		ProcessMovementInput();
		ProcessCameraLook();
	//	if (Input.Pressed("Dash") && Data?.GetUpgradeAmount(EUpgradeName.UnlockDashAbility) == 1) Dash();
	//	if (Input.Pressed("Interact")) EventManager.TriggerPlayerInteractClicked();
	}
	private void ProcessCameraLook()
	{
		Vector2 MouseInput = Input.MouseDelta;
		CurrentCameraRotation.yaw -= MouseInput.x * Sensitivity;
		CurrentCameraRotation.pitch += MouseInput.y * Sensitivity;
		CurrentCameraRotation.pitch = CurrentCameraRotation.pitch.Clamp(-20f, 60f);
		Rotation NextCameraRotation = Rotation.From(CurrentCameraRotation);
		Vector3 PlayerPosition = WorldPosition + Vector3.Up * CameraHeight;
		Vector3 CameraOffset = NextCameraRotation.Backward * CameraDistance;
		Camera.WorldPosition = PlayerPosition + CameraOffset;
		Camera.WorldRotation = NextCameraRotation;
		WorldRotation = Rotation.FromYaw(CurrentCameraRotation.yaw);
	}
	private void ProcessMovementInput()
	{
		PhysicsTraceResult GroundTrace = Scene.PhysicsWorld.Trace.Sphere(4f, WorldPosition + Vector3.Up * 5f, WorldPosition + Vector3.Down * 15f).WithoutTags("player").Run();
		IsGrounded = GroundTrace.Hit;
		Vector3 MovementInput = Input.AnalogMove; 
		Vector3 WishDir = (WorldRotation * MovementInput).WithZ(0).Normal;
		PlayerRigidbody.Velocity += WishDir * MovementSpeed * Time.Delta;
		if (Input.Pressed("Jump") && IsGrounded)
		{
			PlayerRigidbody.Velocity = PlayerRigidbody.Velocity.WithZ(JumpStrength);
			AnimationHelper.TriggerJump();
		}
		AnimationHelper.IsGrounded = IsGrounded;
		AnimationHelper.WithVelocity(PlayerRigidbody.Velocity);
	}
	private void ProcessEscape()
	{
	// Input.EscapePressed = false;
	//	UiManager.ChangeUiState(EUi.SettingsMenu);
	}
	private void ScanAndCollect()
	{
		int CurrentTrash = Data.GetCurrentTrashAmount();
		if (CurrentTrash >= TrashCapacity) return;
		int MaxBatchSize = Math.Min(MaximumCollectAmount, TrashCapacity - CurrentTrash);
		int ProcessedTrashCount = 0;
		DebugOverlay.Sphere(new Sphere(WorldPosition, CollectRadius), Color.Red, 5);
		IEnumerable<GameObject> Targets = Scene.FindInPhysics(new Sphere(WorldPosition, CollectRadius));
		foreach (GameObject Target in Targets)
		{
			if (ProcessedTrashCount >= MaxBatchSize) break;
			if (!Target.Tags.Has("collectible")) continue;
			if (!Target.Components.TryGet(out ICollectable Collectable)) continue;
			Collectable.Collect();
			ProcessedTrashCount++;
		}
		if (ProcessedTrashCount > 0) Data.AddTrash(ProcessedTrashCount);
	}
	private void ApplyUpgradesOnJoin()
	{
		MovementSpeed = DefaultMovementSpeed;
		TrashCapacity = DefaultTrashCapacity;
		MoneyMultiplier = DefaultMoneyMultiplier;
		ApplyToolStats();
		Dictionary<EPrestigeUpgradeName, int> OwnedPrestigeUpgrades = Data?.GetOwnedPrestigeUpgrades();
		if (OwnedPrestigeUpgrades == null) return;
		foreach (EPrestigeUpgradeName UpgradeKey in OwnedPrestigeUpgrades.Keys) PrestigeUpgradePurchased(UpgradeKey);
	}
	private void LoadSaveData()
	{
		Data = PlayerData.Load();
		EventManager.TriggerPlayerDataLoaded();
		EventManager.TriggerMoneyChange(Data.GetMoney());
	}
	private void PrestigeUpgradePurchased(EPrestigeUpgradeName InPrestigeUpgradeName)
	{
		/*switch (InPrestigeUpgradeName)
		{
			case EPrestigeUpgradeName.CollectRange1:
				float LessMoneyLoss1 = GlobalInstance.GetProcessedUpgradeValue(Data.GetUpgradeAmount(EUpgradeName.LessMoneyLoss1), EUpgradeName.LessMoneyLoss1);
				float LessMoneyLoss2 = GlobalInstance.GetProcessedUpgradeValue(Data.GetUpgradeAmount(EUpgradeName.LessMoneyLoss2), EUpgradeName.LessMoneyLoss2);
				MoneyLossToAliensPercentage = (GlobalInstance.GetDefaultAttributeValue(EDefaultAttribute.MoneyLossPercentageToAliens) - LessMoneyLoss1 - LessMoneyLoss2) * 0.01f; break;
			case EPrestigeUpgradeName.CollectSpeed1: AdditionalMoneyAmount = 1 + (int)GlobalInstance.GetProcessedUpgradeValue(Data.GetUpgradeAmount(EUpgradeName.MoreMoney1), EUpgradeName.MoreMoney1); break;
		}*/
	}
	private void ToolUpgradePurchased(EToolStat InToolStat)
	{
		ETool EquippedTool = Data.GetEquippedTool();
		ToolData EquippedToolData = GlobalInstance.GetToolData(EquippedTool);
		int CurrentLevel = Data.GetToolStatLevel(EquippedTool, InToolStat);
		float StatValue = EquippedToolData.GetToolStatValue(InToolStat);
		float AdditiveBonus = CurrentLevel * StatValue;
		float Multiplier = 1f + AdditiveBonus * 0.01f;
		switch (InToolStat)
		{
			case EToolStat.Power: MaximumCollectAmount = EquippedToolData.Power + (int)AdditiveBonus; break;
			case EToolStat.Range: CollectRadius = EquippedToolData.Range * Multiplier; break;
			case EToolStat.Speed: CollectCooldown = EquippedToolData.CollectCooldown / Multiplier; break;
		}
	}
	private void ApplyToolStats()
	{
		ETool EquippedTool = Data.GetEquippedTool();
		ToolData EquippedToolData = GlobalInstance.GetToolData(EquippedTool);
		MaximumCollectAmount = EquippedToolData.Power;
		CollectRadius = EquippedToolData.Range;
		CollectCooldown = EquippedToolData.CollectCooldown;
		foreach (EToolStat StatKey in EquippedToolData.Upgrades.Keys) ToolUpgradePurchased(StatKey);
	}
}
