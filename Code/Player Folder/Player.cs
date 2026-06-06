using Sandbox.Citizen;
using Sandbox.Interfaces;

public sealed class Player : Component
{
	[Property] private float DefaultCollectTimeRate { get; set; } = 0.5f;
	[Property] private float DefaultCollectRadius { get; set; } = 30f;
	[Property] private float Sensitivity { get; set; } = 0.2f;
	[Property] private float CameraDistance { get; set; } = 200f;
	[Property] private float CameraHeight { get; set; } = 50f;
	[Property] private float DefaultMovementSpeed { get; set; } = 2500f;
	[Property] private float JumpStrength { get; set; } = 350f;
	[Property] private Rigidbody PlayerRigidbody { get; set; }
	[Property] private CitizenAnimationHelper AnimationHelper { get; set; }
	private bool IsGrounded;
	private float NextCollectTime;
	private float MovementSpeed;
	private Angles CurrentCameraRotation;
	private CameraComponent Camera;
	
	protected override void OnUpdate()
	{
		if (IsProxy) return;
		ProcessInput();
		if (Time.Now < NextCollectTime) return;
		NextCollectTime = Time.Now + DefaultCollectTimeRate;
		//ScanAndCollect();
	}
	protected override void OnStart()
	{
		if (IsProxy) return;
		Camera = Scene.GetComponentInChildren<CameraComponent>();
		ApplyUpgradesOnJoin();
	}

	private void ProcessInput()
	{
		ProcessMouseInput();
		ProcessKeyboardInput();
	}
	private void ProcessMouseInput()
	{
		//if (UiManager.IsAnyUiOpen()) return;
	//	ProcessMouseRightClickInput();
	//	ProcessMouseLeftClickInput();
	}
	private void ProcessKeyboardInput()
	{
		if (Input.EscapePressed) ProcessEscape();
		ProcessMovementInput();
		ProcessCameraLook();
	//	if (UiManager.IsAnyUiOpen()) return;
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
	//	Input.EscapePressed = false;
	//	UiManager.ChangeUiState(EUi.SettingsMenu);
	}
	private void ScanAndCollect()
	{
		DebugOverlay.Sphere(new Sphere(WorldPosition, DefaultCollectRadius), Color.Red, 5);
		IEnumerable<GameObject> Targets = Scene.FindInPhysics(new Sphere(WorldPosition, DefaultCollectRadius));
		foreach (GameObject Target in Targets)
		{
			GameObject TargetObj = Target;
			if (!TargetObj.Tags.Has("collectible")) continue;
			if (TargetObj.Components.TryGet(out ICollectable Collectable)) Collectable.Collect(); 
		}
	}
	private void ApplyUpgradesOnJoin()
	{
		MovementSpeed = DefaultMovementSpeed;
	}
}
