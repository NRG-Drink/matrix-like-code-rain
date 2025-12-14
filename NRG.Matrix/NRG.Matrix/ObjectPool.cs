namespace NRG.Matrix;

public class ObjectPool<T> where T : new()
{
    private readonly Stack<T> _pool = new();

    public T Rent()
    {
        lock (_pool)
        {
            return _pool.Count > 0 ? _pool.Pop() : new T();
        }
    }

    public T[] Rent(int n)
        => Enumerable.Range(0, n).Select(e => Rent()).ToArray();

    public void Return(T obj)
    {
        lock (_pool)
        {
            _pool.Push(obj);
        }
    }
}
