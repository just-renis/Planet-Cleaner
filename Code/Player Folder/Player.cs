public sealed class Player : Component
{
	protected override void OnUpdate()
	{
		if (IsProxy) return;
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
	//	if (Input.Pressed("Dash") && Data?.GetUpgradeAmount(EUpgradeName.UnlockDashAbility) == 1) Dash();
	//	if (Input.Pressed("Interact")) EventManager.TriggerPlayerInteractClicked();
	}
	private void ProcessEscape()
	{
	//	Input.EscapePressed = false;
	//	UiManager.ChangeUiState(EUi.SettingsMenu);
	}
}
