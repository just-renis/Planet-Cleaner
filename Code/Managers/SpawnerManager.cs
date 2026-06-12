using System;
using Sandbox.Data;
using Sandbox.Enums;
using Sandbox.Interfaces;
using Sandbox.Objects;
using Sandbox.Special_Data;

namespace Sandbox.Managers;

public sealed class SpawnerManager(Scene Scene) : GameObjectSystem(Scene)
{
	private float TrashSpawnTimer;
	private const float TrashSpawnTime = 0.1f;
	private PoolingManager PoolingManager;
	
	public void Init()
	{
		PoolingManager = PoolingManager.Instance;
	}
	public void OnUpdate()
	{
		HandleCubeSpawning();
	}

	private void HandleCubeSpawning()
	{
		if (!PoolingManager.CanSpawnObject(EPoolObject.Trash)) return;
		TrashSpawnTimer += Time.Delta;
		if (TrashSpawnTimer < TrashSpawnTime) return;
		TrashSpawnTimer = 0f;
		GameObject Trash = PoolingManager.SpawnObjectFromPool(EPoolObject.Trash, new Vector3(Random.Shared.Float(-500f, 500f), Random.Shared.Float(-500f, 500f), 2030f));
		Trash.GetComponent<Trash>()?.Init(new TrashArgs(GlobalData.Instance.GetRandomTrashModel(ETrashType.EarthStartingArea)));
	}
}
