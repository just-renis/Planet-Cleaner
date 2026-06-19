using System;
using Sandbox.Enums;
namespace Sandbox.Managers;

public sealed class EventManager(Scene Scene) : GameObjectSystem(Scene)
{
	public static event Action OnAutoSaveFinished;
	public static event Action OnTrashSold;
	public static event Action OnPlayerDataLoaded;
	public static event Action OnToolChanged;
	public static event Action OnPrestige;
	public static event Action<int> OnUpgradesAvailableToBePurchased;
	public static event Action<double> OnMoneyChanged;
	public static event Action<EPrestigeUpgradeName> OnPrestigeUpgradePurchased;
	public static event Action<EToolStat> OnToolUpgradePurchased;
	public static event Action<EPlayerUpgrade> OnPlayerBaseUpgradePurchased;
	public static event Action<EUi, bool> OnUiStateChanged;
	public static event Func<Player> OnRequestPlayerRef;
	
	public static void TriggerAutoSaveFinished()
	{
		OnAutoSaveFinished?.Invoke();
	}
	public static void TriggerTrashSold()
	{
		OnTrashSold?.Invoke();
	}
	public static void TriggerPlayerDataLoaded()
	{
		OnPlayerDataLoaded?.Invoke();
	}
	public static void TriggerToolChange()
	{
		OnToolChanged?.Invoke();
	}
	public static void TriggerOnPrestige()
	{
		OnPrestige?.Invoke();
	}
	public static void TriggerUpgradesAvailableToBePurchased(int InAmount)
	{
		OnUpgradesAvailableToBePurchased?.Invoke(InAmount);
	}
	public static void TriggerMoneyChange(double InMoney)
	{
		OnMoneyChanged?.Invoke(InMoney);
	}
	public static void TriggerPrestigeUpgradePurchased(EPrestigeUpgradeName InUpgradeName)
	{
		OnPrestigeUpgradePurchased?.Invoke(InUpgradeName);
	}
	public static void TriggerToolUpgradePurchased(EToolStat InToolStatName)
	{
		OnToolUpgradePurchased?.Invoke(InToolStatName);
	}
	public static void TriggerBasePlayerUpgradePurchased(EPlayerUpgrade InPlayerUpgradeName)
	{
		OnPlayerBaseUpgradePurchased?.Invoke(InPlayerUpgradeName);
	}
	public static void TriggerUiStateChange(EUi InUi, bool RenderState)
	{
		OnUiStateChanged?.Invoke(InUi, RenderState);
	}
	public static Player TriggerGetPlayerRef()
	{
		return OnRequestPlayerRef?.Invoke();
	}
	public override void Dispose()
	{
		OnAutoSaveFinished = null;
		OnTrashSold = null;
		OnPlayerDataLoaded = null;
		OnToolChanged = null;
		OnPrestige = null;
		OnUpgradesAvailableToBePurchased = null;
		OnMoneyChanged = null;
		OnPrestigeUpgradePurchased = null;
		OnToolUpgradePurchased = null;
		OnPlayerBaseUpgradePurchased = null;
		OnUiStateChanged = null;
		OnRequestPlayerRef = null;
		base.Dispose();
	}
}
