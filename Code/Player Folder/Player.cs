using Sandbox.Citizen;
using Sandbox.Data;
using Sandbox.Enums;
using Sandbox.Interfaces;
using Sandbox.Managers;

public sealed class Player : Component
{
	[Property] private float DefaultCollectRadius { get; set; } = 30f;
	[Property] private float Sensitivity { get; set; } = 0.2f;
	[Property] private float CameraDistance { get; set; } = 200f;
	[Property] private float CameraHeight { get; set; } = 50f;
	[Property] private float DefaultMovementSpeed { get; set; } = 2500f;
	[Property] private float JumpStrength { get; set; } = 350f;
	[Property] private float DefaultCollectCooldown { get; set; } = 1f;
	[Property] private float DefaultMoneyMultiplier { get; set; } = 1f;
	[Property] private int DefaultCollectAmount { get; set; } = 1;
	[Property] private int DefaultTrashCapacity { get; set; } = 10;
	[Property] private Rigidbody PlayerRigidbody { get; set; }
	[Property] private CitizenAnimationHelper AnimationHelper { get; set; }
	private bool IsGrounded;
	private float MovementSpeed;
	private float CollectCooldownTimer;
	private float MoneyMultiplier;
	private int CurrentCollectAmount;
	private int TrashCapacity;
	private Angles CurrentCameraRotation;
	private CameraComponent Camera;
	private PlayerData Data;
	
	public int GetTrashCollected() { return Data.GetCurrentTrashAmount(); }
	public int GetTrashCapacity() { return TrashCapacity; }
	public double GetMoney() { return Data.GetMoney(); }
	
	protected override void OnUpdate()
	{
		if (IsProxy) return;
		ProcessInput();
		if (Time.Now < CollectCooldownTimer) return;
		CollectCooldownTimer = Time.Now + DefaultCollectCooldown;
		ScanAndCollect();
	}
	protected override void OnStart()
	{
		if (IsProxy) return;
		LoadSaveData();
		Camera = Scene.GetComponentInChildren<CameraComponent>();
		EventManager.OnPlayerDataRequired += GetPlayerData;
		EventManager.OnRequestPlayerRef += GetThisRef;
		EventManager.OnTrashSold += SellTrash;
		ApplyUpgradesOnJoin();
	}
	protected override void OnDestroy()
	{
		if (Data != null && Data.GetIsPendingForSave()) PlayerData.ApplySave(Data);
		if (IsProxy) return;
		EventManager.OnPlayerDataRequired -= GetPlayerData;
		EventManager.OnRequestPlayerRef -= GetThisRef;
		EventManager.OnTrashSold -= SellTrash;
	}

	private PlayerData GetPlayerData() { return Data; }
	private Player GetThisRef() { return this; }

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
		if (Input.Pressed("Score")) UiManager.ChangeUiState(EUi.UpgradesMenu);
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
		if (Data.GetCurrentTrashAmount() >= TrashCapacity) return;
		DebugOverlay.Sphere(new Sphere(WorldPosition, DefaultCollectRadius), Color.Red, 5);
		IEnumerable<GameObject> Targets = Scene.FindInPhysics(new Sphere(WorldPosition, DefaultCollectRadius));
		foreach (GameObject Target in Targets)
		{
			if (!Target.Tags.Has("collectible")) continue;
			if (!Target.Components.TryGet(out ICollectable Collectable)) continue;
			Collectable.Collect();
			Data.AddTrash(1);
			if (++CurrentCollectAmount < DefaultCollectAmount) continue;
			CurrentCollectAmount = 0;
			break;
		}
	}
	private void ApplyUpgradesOnJoin()
	{
		MovementSpeed = DefaultMovementSpeed;
		TrashCapacity = DefaultTrashCapacity;
		MoneyMultiplier = DefaultMoneyMultiplier;
	}
	private void LoadSaveData()
	{
		Data = PlayerData.Load();
		EventManager.TriggerMoneyChange(Data.GetMoney());
	}
}
