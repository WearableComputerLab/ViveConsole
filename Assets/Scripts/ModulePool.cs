using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModulePool : MonoBehaviour
{
    private class ObjectPool<T> where T : UnityEngine.Object
    {
        private T _prefab;
        private Transform _root;
        private Stack<T> _poolStorage;
        private readonly int DEFAULT_POOL_SIZE = 10;

        public ObjectPool(T t, Transform root)
        {
            _prefab = t;
            _root = root;
            _poolStorage = new Stack<T>();
        }

        public T Prefab()
        {
            return _prefab;
        }

        public void Return(T t)
        {
            if (_poolStorage.Contains(t))
                throw new System.Exception($"{t} already in pool");

            var go = t as GameObject;
            go?.transform.SetParent(_root, false);
            go?.SetActive(false);
            _poolStorage.Push(t);
        }

        public T Request()
        {
            if (_poolStorage.Count == 0)
            {
                for (int i = 0; i < DEFAULT_POOL_SIZE; ++i)
                {
                    var obj = Instantiate(_prefab, _root, false);
                    (obj as GameObject)?.SetActive(false);
                    (obj as MonoBehaviour)?.gameObject.SetActive(false);
                    _poolStorage.Push(obj);
                }
            }

            return _poolStorage.Pop();
        }
    }

    public List<ConsoleModule> _modulePrefabs = new List<ConsoleModule>();

    private readonly Dictionary<int, ObjectPool<ConsoleModule>> _objectPools = new Dictionary<int, ObjectPool<ConsoleModule>>();

    void Start()
    {
        foreach (var m in _modulePrefabs)
        {
            _objectPools.Add(m._id, new ObjectPool<ConsoleModule>(m, transform));
        }
    }

    public ConsoleModule RequestModule(int id)
    {
        if (!_objectPools.ContainsKey(id))
            throw new System.Exception($"Module {id} has no prefab set");
        var newModule = _objectPools[id].Request();
        newModule.gameObject.SetActive(true);
        return newModule;
    }

    public void ReturnModule(ConsoleModule module)
    {
        if (!_objectPools.ContainsKey(module._id))
            throw new System.Exception($"Module identifier {module._id} not recognized");

        module.gameObject.SetActive(false);
        _objectPools[module._id].Return(module);
    }

    public ConsoleModule GetPrefab(int id)
    {
        if (!_objectPools.ContainsKey(id))
            throw new System.Exception($"Module {id} has no prefab set");
        return _objectPools[id].Prefab();
    }
}
