namespace Sandbox.Interfaces;

public interface IPoolObject<T> where T : class
{
	public void Init(T Args = null);
}
