using System;
using Sandbox.Data;
using Sandbox.Enums;
namespace Sandbox.Managers;

public sealed class EventManager(Scene Scene) : GameObjectSystem(Scene)
{
	public static event Action OnAutoSaveFinished;
	public static event Action<int> OnUpgradesAvailableToBePurchased;
	public static event Action<double> OnMoneyChanged;
	public static event Action<EUpgradeName> OnUpgradePurchased;
	public static event Action<EUi, bool> OnUiStateChanged;
	public static event Func<PlayerData> OnPlayerDataRequired;
	
	public static void TriggerAutoSaveFinished()
	{
		OnAutoSaveFinished?.Invoke();
	}
	public static void TriggerUpgradesAvailableToBePurchased(int InAmount)
	{
		OnUpgradesAvailableToBePurchased?.Invoke(InAmount);
	}
	public static void TriggerMoneyChange(double InMoney)
	{
		OnMoneyChanged?.Invoke(InMoney);
	}
	public static void TriggerUpgradePurchased(EUpgradeName InUpgradeName)
	{
		OnUpgradePurchased?.Invoke(InUpgradeName);
	}
	public static void TriggerUiStateChange(EUi InUi, bool RenderState)
	{
		OnUiStateChanged?.Invoke(InUi, RenderState);
	}
	public static PlayerData TriggerGetPlayerData()
	{
		return OnPlayerDataRequired?.Invoke();
	}
}
