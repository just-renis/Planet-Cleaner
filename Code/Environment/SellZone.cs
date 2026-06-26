using Sandbox.Components;
using Sandbox.Enums;
using Sandbox.Managers;

public sealed class SellZone : CustomTrigger
{
	protected override void OnTriggerEnter(GameObject InGameObject)
	{
		EventManager.TriggerTrashSold();
	}
}
