namespace NRG.Matrix;

public class ObjectPool<T> where T : new()
{
    private readonly Lock _lock = new();
    private readonly Stack<T> _pool = new();

    public T Rent()
    {
        lock (_lock)
        {
            return _pool.Count > 0 ? _pool.Pop() : new T();
        }
    }

    public T[] Rent(int n)
    {
        var result = new T[n];

        lock (_lock)
        {
            for (var i = 0; i < n; i++)
            {
                result[i] = _pool.Count > 0 ? _pool.Pop() : new T();
            }
        }

        return result;
    }

    public void Return(T obj)
    {
        lock (_lock)
        {
            _pool.Push(obj);
        }
    }
}
