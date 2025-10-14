using System;
using System.Collections.Generic;

public class ObjectPool<T>
{
    public delegate T FactoryMethod();

    private List<T> _currentStock;
    public List<T> totalStock;
    private FactoryMethod _factoryMethod;
    private bool _isDynamic;
    private Action<T> _turnOnCallback;
    private Action<T> _turnOffCallback;

    public ObjectPool(FactoryMethod factoryMethod, Action<T> turnOnCallback, Action<T> turnOffCallback,
        int initialStock = 0, bool isDynamic = true)
    {
        _currentStock    = new List<T>();
        totalStock = new();
        _factoryMethod   = factoryMethod;
        _isDynamic       = isDynamic;
        _turnOnCallback  = turnOnCallback;
        _turnOffCallback = turnOffCallback;

        for (int i = 0; i < initialStock; i++)
        {
            var o = _factoryMethod();
            _turnOffCallback(o);
            _currentStock.Add(o);
            totalStock.Add(o);
        }
    }
    
    public T GetObject()
    {
        var result = default(T);
        if (_currentStock.Count > 0)
        {
            result = _currentStock[0];
            _currentStock.RemoveAt(0);
        }
        else if(_isDynamic)
            result = _factoryMethod();

        _turnOnCallback(result);
        return result;
    }
    
    public void ReturnObject(T obj)
    {
        _turnOffCallback(obj);
        _currentStock.Add(obj);
    }
}