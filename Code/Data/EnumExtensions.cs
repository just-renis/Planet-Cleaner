using Sandbox.Enums;
namespace Sandbox.Data;

public static class EnumExtensions
{
	public static string GetDisplayName(this ETool InTool)
	{
		switch (InTool)
		{
			case ETool.Hands: return "Hands";
			case ETool.Grabber: return "Grabber";
			case ETool.SuperGrabber: return "Super Grabber";
			default: return InTool.ToString();
		}
	}
	public static string GetDisplayName(this EToolStat InStat)
	{
		switch (InStat)
		{
			case EToolStat.Power: return "POWER";
			case EToolStat.Range: return "RANGE";
			case EToolStat.Speed: return "SPEED";
			default: return InStat.ToString();
		}
	}
	public static string GetDisplayName(this EPlayerUpgrade InPlayerUpgrade)
	{
		switch (InPlayerUpgrade)
		{
			case EPlayerUpgrade.BagSize: return "Bag Size";
			case EPlayerUpgrade.MovementSpeed: return "Movement Speed";
			default: return InPlayerUpgrade.ToString();
		}
	}
}
