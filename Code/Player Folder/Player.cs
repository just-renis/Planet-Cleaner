using Sandbox.Citizen;

public sealed class Player : Component
{
	[Property] private float Sensitivity { get; set; } = 0.2f;
	[Property] private float CameraDistance { get; set; } = 200f;
	[Property] private float CameraHeight { get; set; } = 50f;
	[Property] private float MaxSpeed { get; set; } = 500f;
	[Property] private float Acceleration { get; set; } = 10f;
	[Property] private float Friction { get; set; } = 4f;
	[Property] private float JumpForce { get; set; } = 350f;
	[Property] private CharacterController Controller { get; set; }
	[Property] private CitizenAnimationHelper AnimHelper { get; set; }
	private Angles CurrentCameraRotation;
	private CameraComponent Camera;
	
	protected override void OnStart()
	{
		if (IsProxy) return;
		Camera = Scene.GetComponentInChildren<CameraComponent>();
	}
	protected override void OnUpdate()
	{
		if (IsProxy || Camera == null) return;
		ProcessInput();
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
	//	if (UiManager.IsAnyUiOpen()) return;
		ProcessMovementInput();
		ProcessCameraLook();
		UpdateAnimations();
	//	if (Input.Pressed("Dash") && Data?.GetUpgradeAmount(EUpgradeName.UnlockDashAbility) == 1) Dash();
	//	if (Input.Pressed("Interact")) EventManager.TriggerPlayerInteractClicked();
	}
	private void ProcessEscape()
	{
	//	Input.EscapePressed = false;
	//	UiManager.ChangeUiState(EUi.SettingsMenu);
	}
	private void ProcessMovementInput()
	{
		if (Controller == null) return;
		Vector3 MovementInput = Input.AnalogMove; 
		Vector3 WishDir = (WorldRotation * MovementInput).WithZ(0).Normal;
		Vector3 CurrentHorizontalVelocity = Controller.Velocity.WithZ(0);
		if (WishDir.Length.AlmostEqual(0) || Vector3.Dot(CurrentHorizontalVelocity, WishDir) < 0)
		{
			CurrentHorizontalVelocity = Vector3.Lerp(CurrentHorizontalVelocity, Vector3.Zero, Friction * Time.Delta);
		}
		Vector3 TargetVelocity = WishDir * MaxSpeed;
		CurrentHorizontalVelocity = Vector3.Lerp(CurrentHorizontalVelocity, TargetVelocity, Acceleration * Time.Delta);
		float VerticalVelocity = Controller.Velocity.z;
		if (!Controller.IsOnGround)
		{
			VerticalVelocity += Scene.PhysicsWorld.Gravity.z * Time.Delta;
		}
		else
		{
			if (VerticalVelocity < 0) VerticalVelocity = -20f;
			if (Input.Pressed("Jump")) VerticalVelocity = JumpForce;
		}
		Controller.Velocity = CurrentHorizontalVelocity.WithZ(VerticalVelocity);
		Controller.Move();
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
	private void UpdateAnimations()
	{
		if (AnimHelper == null || Controller == null) return;
		AnimHelper.WithVelocity(Controller.Velocity);
		AnimHelper.WithWishVelocity(Controller.Velocity.WithZ(0));
		AnimHelper.IsGrounded = Controller.IsOnGround;
		// AnimHelper.WithLookAt( Transform.Position + Transform.Rotation.Forward * 100f );
	}
}
