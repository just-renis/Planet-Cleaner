using System;
using Sandbox.Enums;
using Sandbox.Managers;

public sealed class GrabberSpecialization : Component
{
	[Property] private int DefaultCap { get; set; }
	[Property] private float DefaultCooldown { get; set; }
	[Property] private float DefaultRadius { get; set; }
	[Property] private float TrashJumpHeight { get; set; }
	private TimeSince CurrentCooldown;
	private PlayerSaveManager SaveManager;
	private List<SceneTraceResult> NearbyObjects = [];

	protected override void OnStart()
	{
		SaveManager = PlayerSaveManager.Instance;
	}
	protected override void OnUpdate()
	{
		if (!SaveManager.Data.IsPrestigeUpgradeUnlocked(EPrestigeUpgradeName.ToolsSpecialization)) return;
		if (CurrentCooldown < DefaultCooldown) return;
		CurrentCooldown = 0f;
		AttractTrash();
	}
	
	private Vector3 CalculateBallisticVelocity(Vector3 InStart, Vector3 InTarget, float InHeight, float InGravity)
	{
		if (InGravity <= 0f || InHeight <= 0f) return (InTarget - InStart).Normal * 500f; 
		float DisplacementZ = InTarget.z - InStart.z;
		Vector3 DisplacementHorizontal = new (InTarget.x - InStart.x, InTarget.y - InStart.y, 0f);
		float VelocityZ = MathF.Sqrt(2f * InGravity * InHeight);
		float TimeAscent = VelocityZ / InGravity;
		float TimeDescent = MathF.Sqrt(MathF.Abs(2f * (InHeight - DisplacementZ) / InGravity));
		float TotalTime = TimeAscent + TimeDescent;
		if (TotalTime <= 0f) return Vector3.Zero;
		Vector3 VelocityHorizontal = DisplacementHorizontal / TotalTime;
		return new Vector3(VelocityHorizontal.x, VelocityHorizontal.y, VelocityZ);
	}

	private void AttractTrash()
	{
		SceneTrace Trace = Scene.Trace.Ray(WorldPosition, WorldPosition).Radius(DefaultRadius).WithTag("collectible");
		NearbyObjects.Clear();
		foreach (SceneTraceResult Hit in Trace.RunAll()) NearbyObjects.Add(Hit);
		int PoolSize = NearbyObjects.Count;
		if (PoolSize == 0) return;
		float Gravity = MathF.Abs(Scene.PhysicsWorld.Gravity.z);
		int TargetsToTake = Math.Min(DefaultCap, PoolSize);
		for (int Index = 0; Index < TargetsToTake; Index++)
		{
			int RandomIndex = Game.Random.Int(0, PoolSize - 1);
			SceneTraceResult Hit = NearbyObjects[RandomIndex];
			NearbyObjects[RandomIndex] = NearbyObjects[PoolSize - 1];
			PoolSize--;
			GameObject TargetObject = Hit.GameObject;
			if (!TargetObject.IsValid()) continue;
			if (!TargetObject.Components.TryGet(out Rigidbody Rigidbody)) continue;
			Vector3 TrashPos = TargetObject.WorldPosition;
			float TrashZ = TrashPos.z;
			float DynamicHeight = MathF.Max(TrashZ, WorldPosition.z) + TrashJumpHeight - TrashZ;
			Vector3 LaunchVelocity = CalculateBallisticVelocity(TrashPos, WorldPosition, DynamicHeight, Gravity);
			LaunchVelocity.x += Game.Random.Float(-20f, 20f);
			LaunchVelocity.y += Game.Random.Float(-20f, 20f);
			Rigidbody.MotionEnabled = true;
			Rigidbody.Velocity = LaunchVelocity;
		}
	}
}
