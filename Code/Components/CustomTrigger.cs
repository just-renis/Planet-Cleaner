using System;
using Sandbox.Managers;
namespace Sandbox.Components;

public enum TriggerShape
{
	Box,
	Capsule,
	Sphere
}

public class CustomTrigger : Component
{
	[Property] public bool OnlyTriggerForHost { get; set; } = true;
	[Property] public TriggerShape Shape { get; set; } = TriggerShape.Box;
	[Property, Group("IsSetByTags")] public bool TriggerByTags { get; set; }
	[Property, Group("IsSetByTags")] 
	[ShowIf(nameof(TriggerByTags), true)]
	public string[] TriggerTags { get; set; }
	[Property, ShowIf(nameof(Shape), TriggerShape.Box)] 
	public Vector3 Size { get; set; } = new( 100 );
	[Property, ShowIf(nameof(Shape), TriggerShape.Capsule)] 
	public float CapsuleRadius { get; set; } = 50f;
	[Property, ShowIf(nameof(Shape), TriggerShape.Capsule)] 
	public float CapsuleHeight { get; set; } = 100f;
	[Property, ShowIf(nameof(Shape), TriggerShape.Sphere)]
	public float SphereRadius { get; set; } = 50f;
	public Action<GameObject> OnEnter;
	public Action<GameObject> OnExit;
	private int RunTraceHandle;
	
	private readonly HashSet<GameObject> Occupants = [];
	private readonly HashSet<GameObject> CurrentFrameOccupants = [];
	private readonly List<GameObject> RemoveOccupants = [];

	public void SetTraceState(bool InState)
	{
		if (InState) RunTraceHandle = TimerManager.AddTimer(RunTraceHandle, 0.1f, true, RunTraceCheck);
		else TimerManager.ClearTimer(RunTraceHandle);
	}

	protected override void OnStart() { SetTraceState(true); }
	protected override void OnDestroy() { SetTraceState(false); }
	protected virtual void OnTriggerEnter(GameObject InGameObject) { }
	protected virtual void OnTriggerExit(GameObject InGameObject) { }
	
	private SceneTrace BuildTrace()
	{
		SceneTrace Trace;
		switch (Shape)
		{
			case TriggerShape.Box:
				Trace = Scene.Trace.Ray(WorldPosition, WorldPosition).Size(Size);
				break;
			case TriggerShape.Sphere:
				Trace = Scene.Trace.Ray(WorldPosition, WorldPosition).Radius(SphereRadius);
				break;
			default:
				Vector3 HalfHeight = WorldRotation.Up * (CapsuleHeight * 0.5f);
				Vector3 Start = WorldPosition + HalfHeight;
				Vector3 End = WorldPosition - HalfHeight;
				Trace = Scene.Trace.Ray(Start, End).Radius(CapsuleRadius);
				break;
		}
		if (TriggerByTags && TriggerTags != null) Trace = Trace.WithAnyTags(TriggerTags);
		return Trace;
	}

	private void RunTraceCheck()
	{
		if (OnlyTriggerForHost && !Networking.IsHost) return;
		// ----------------------------------------------------DEBUG-----------------------------------------------------
		switch (Shape)
		{
			case TriggerShape.Box:
				DebugOverlay.Box(BBox.FromPositionAndSize(WorldPosition, Size), Color.Cyan, 0.1f);
				break;
			case TriggerShape.Sphere:
				DebugOverlay.Sphere(new Sphere(WorldPosition, SphereRadius), Color.Cyan, 0.1f);
				break;
			default:
				Vector3 HalfHeight = WorldRotation.Up * (CapsuleHeight * 0.5f);
				Vector3 Top = WorldPosition + HalfHeight;
				Vector3 Bottom = WorldPosition - HalfHeight;
				Capsule Capsule = new (Bottom, Top, CapsuleRadius);
				DebugOverlay.Capsule(Capsule, Color.Cyan, 0.1f);
				break;
		}
		RemoveOccupants.Clear();
		CurrentFrameOccupants.Clear();
		SceneTrace Trace = BuildTrace();
		IEnumerable<SceneTraceResult> Hits = Trace.RunAll();
		foreach (SceneTraceResult Hit in Hits)
		{
			if (!Hit.Hit) continue;
			GameObject Root = Hit.GameObject.Root;
			if (!Root.IsValid()) continue;
			CurrentFrameOccupants.Add(Root);
		}
		foreach (GameObject Object in CurrentFrameOccupants)
		{
			if (!Occupants.Add(Object)) continue;
			OnTriggerEnter(Object);
			OnEnter?.Invoke(Object);
		}
		foreach (GameObject Occupant in Occupants)
		{
			if (!Occupant.IsValid() || !CurrentFrameOccupants.Contains(Occupant)) RemoveOccupants.Add(Occupant);
		}
		foreach (GameObject Occupant in RemoveOccupants)
		{
			OnTriggerExit(Occupant);
			OnExit?.Invoke(Occupant);
			Occupants.Remove(Occupant);
		}
	}
}
