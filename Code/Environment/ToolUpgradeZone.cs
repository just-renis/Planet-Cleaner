using Sandbox.Components;
using Sandbox.Enums;
using Sandbox.Managers;

public sealed class ToolUpgradeZone : CustomTrigger
{
	protected override void OnTriggerEnter(GameObject InGameObject)
	{
		UiManager.ChangeUiState(EUi.ToolUpgrades);
	}
}
