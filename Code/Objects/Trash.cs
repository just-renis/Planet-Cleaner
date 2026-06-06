using Sandbox.Enums;
using Sandbox.Interfaces;
using Sandbox.Managers;
namespace Sandbox.Objects;

public sealed class Trash : Component, IPoolObject, ICollectable
{
	public static int LiveCount { get; private set; }
	
	public void Init()
	{
		Transform.ClearInterpolation();
	}
	public void Collect()
	{
		PoolingManager.Instance.ReturnObjectToPool(EPoolObject.Trash, GameObject);
	}

	protected override void OnEnabled() { LiveCount++; }
	protected override void OnDisabled() { LiveCount--; }
}
