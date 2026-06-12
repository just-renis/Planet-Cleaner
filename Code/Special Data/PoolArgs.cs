namespace Sandbox.Special_Data;

public class PoolArgs { }

public class TrashArgs(Model InModel) : PoolArgs
{
	public Model TrashModel { get; set; } = InModel;
}
