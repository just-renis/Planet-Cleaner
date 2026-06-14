using Sandbox.Components;
using Sandbox.Enums;
using Sandbox.Managers;

public sealed class PlayerUpgradeZone : CustomTrigger
{
	protected override void OnTriggerEnter(GameObject InGameObject)
	{
		UiManager.ChangeUiState(EUi.PlayerUpgrades);
	}
}
