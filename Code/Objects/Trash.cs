using Sandbox.Enums;
using Sandbox.Interfaces;
using Sandbox.Managers;
using Sandbox.Special_Data;
namespace Sandbox.Objects;

public sealed class Trash : Component, IPoolObject<TrashArgs>, ICollectable
{
	[Property] private ModelRenderer Renderer { get; set; }
	[Property] private ModelCollider ModelCollider { get; set; }
	public static int LiveCount { get; private set; }
	
	public void Init(TrashArgs InArgs)
	{
		Transform.ClearInterpolation();
		Renderer.Model = InArgs.TrashModel;
		ModelCollider.Model = InArgs.TrashModel;
	}
	public void Collect()
	{
		PoolingManager.Instance.ReturnObjectToPool(EPoolObject.Trash, GameObject);
	}

	protected override void OnEnabled() { LiveCount++; }
	protected override void OnDisabled() { LiveCount--; }
}
