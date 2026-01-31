using System.Buffers;

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
        // Use ArrayPool for better memory reuse
        var result = ArrayPool<T>.Shared.Rent(n);

        // Initialize objects from pool
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

    public void ReturnArray(T[] array, int usedLength)
    {
        // Return objects to pool
        lock (_lock)
        {
            for (var i = 0; i < usedLength; i++)
            {
                _pool.Push(array[i]);
            }
        }

        // Return array to ArrayPool
        ArrayPool<T>.Shared.Return(array, clearArray: true);
    }
}
