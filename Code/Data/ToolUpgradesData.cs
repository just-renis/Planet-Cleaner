using Sandbox.Enums;
namespace Sandbox.Data;

public sealed class ToolUpgradesData
{
	public bool IsOwned { get; set; }
	public Dictionary<EToolStat, int> Levels { get; set; }
}
