using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class ObjectPool : MonoBehaviour
    {
        private readonly Dictionary<string, Stack<Component>> _components = new();
        private readonly Dictionary<string, Stack<GameObject>> _gameObjects = new();

        private void Awake()
        {
            Application.lowMemory += ClearPool;
        }

        public GameObject Get(GameObject prototype, Transform parent = null)
        {
            var id = prototype.name;
            if (_gameObjects.TryGetValue(id, out var stack) && stack.TryPop(out var obj) && obj != null)
            {
                obj.transform.SetParent(parent, false);
                obj.gameObject.SetActive(true);
                return obj;
            }

            obj = Instantiate(prototype, parent);
            obj.gameObject.name = id;
            return obj;
        }

        public T Get<T>(T prototype, Transform parent = null, bool activate = true) where T : Component
        {
            var id = prototype.gameObject.name;
            if (_components.TryGetValue(id, out var stack) && stack.TryPop(out var obj) && obj != null)
            {
                obj.transform.SetParent(parent, false);
                obj.gameObject.SetActive(activate);
                return (T)obj;
            }

            obj = Instantiate(prototype, parent);
            obj.gameObject.name = id;
            return (T)obj;
        }

        public void Release(Component component)
        {
            if (component == null) return;

            component.gameObject.SetActive(false);
            component.transform.SetParent(transform, false);
            component.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (!_components.TryGetValue(component.gameObject.name, out var stack))
            {
                stack = new Stack<Component>();
                _components.Add(component.gameObject.name, stack);
            }

            stack.Push(component);
        }

        public void Release(GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);
            obj.transform.SetParent(transform, false);
            obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (!_gameObjects.TryGetValue(obj.name, out var stack))
            {
                stack = new Stack<GameObject>();
                _gameObjects.Add(obj.name, stack);
            }

            stack.Push(obj);
        }

        public void OnDestroy()
        {
            Application.lowMemory -= ClearPool;
        }

        private void ClearPool()
        {
            foreach (var objectsValue in _components)
            {
                foreach (var go in objectsValue.Value)
                {
                    if (go != null)
                    {
                        Destroy(go);
                    }
                }
            }

            _components.Clear();

            foreach (var objectsValue in _gameObjects)
            {
                foreach (var go in objectsValue.Value)
                {
                    if (go != null)
                    {
                        Destroy(go);
                    }
                }
            }

            _gameObjects.Clear();
        }
    }
}