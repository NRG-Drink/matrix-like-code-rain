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
    {
        var result = new T[n];
        for (var i = 0; i < n; i++)
        {
            result[i] = Rent();
        }
        return result;
    }

    public void Return(T obj)
    {
        lock (_pool)
        {
            _pool.Push(obj);
        }
    }
}
