using Sandbox.Interfaces;
namespace Sandbox.Objects;

public sealed class Trash : Component, IPoolObject
{
	public static int LiveCount { get; private set; }
	
	public void Init()
	{
		Transform.ClearInterpolation();
	}
	
	protected override void OnEnabled() { LiveCount++; }
	protected override void OnDisabled() { LiveCount--; }
}
