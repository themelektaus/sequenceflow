using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype.SequenceFlow
{
    [Serializable]
    public class SimpleData : IEnumerable<SimpleData.IObject>
    {
        public interface IObject
        {
            string GetName();
            Type GetValueType();
            object GetValue();
        }

        [Serializable]
        public class Boolean : IObject
        {
            public string name;
            public bool value;
            public string GetName() => name;
            public Type GetValueType() => typeof(bool);
            public object GetValue() => value;
        }

        [Serializable]
        public class Integer : IObject
        {
            public string name;
            public int value;
            public string GetName() => name;
            public Type GetValueType() => typeof(int);
            public object GetValue() => value;
        }

        [Serializable]
        public class Float : IObject
        {
            public string name;
            public float value;
            public string GetName() => name;
            public Type GetValueType() => typeof(float);
            public object GetValue() => value;
        }

        [Serializable]
        public class Object : IObject
        {
            public string name;
            public UnityEngine.Object value;
            public string GetName() => name;
            public Type GetValueType() => value ? value.GetType() : typeof(UnityEngine.Object);
            public object GetValue() => value;

        }

        [Serializable]
        public class String : IObject
        {
            public string name;
            public string value;
            public string GetName() => name;
            public Type GetValueType() => typeof(string);
            public object GetValue() => value;
        }

        [SerializeField] List<Boolean> bools = new();
        [SerializeField] List<Integer> ints = new();
        [SerializeField] List<Float> floats = new();
        [SerializeField] List<Object> objects = new();
        [SerializeField] List<String> strings = new();

        public void Clear()
        {
            bools.Clear();
            ints.Clear();
            floats.Clear();
            objects.Clear();
            strings.Clear();
        }

        public IEnumerator<IObject> GetEnumerator()
        {
            foreach (var @bool in bools)
                yield return @bool;

            foreach (var @int in ints)
                yield return @int;

            foreach (var @float in floats)
                yield return @float;

            foreach (var @string in strings)
                yield return @string;

            foreach (var @object in objects)
                yield return @object;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<string> GetNames()
        {
            foreach (var @bool in bools)
                yield return @bool.name;

            foreach (var @int in ints)
                yield return @int.name;

            foreach (var @float in floats)
                yield return @float.name;

            foreach (var @string in strings)
                yield return @string.name;

            foreach (var @object in objects)
                yield return @object.name;
        }

        public bool GetBool(string name)
        {
            TryGetBool(name, out bool value);
            return value;
        }

        public bool TryGetBool(string name, out bool value)
        {
            var @bool = bools.Where(x => x.name == name).FirstOrDefault();

            if (@bool is null)
            {
                value = false;
                return false;
            }

            value = @bool.value;
            return true;
        }

        public void SetBool(string name, bool value)
        {
            var @bool = bools.Where(x => x.name == name).FirstOrDefault();

            if (@bool is null)
            {
                @bool = new() { name = name };
                bools.Add(@bool);
            }

            @bool.value = value;
        }

        public int GetInteger(string name)
        {
            TryGetInteger(name, out int value);
            return value;
        }

        public bool TryGetInteger(string name, out int value)
        {
            var @int = ints.Where(x => x.name == name).FirstOrDefault();

            if (@int is null)
            {
                value = 0;
                return false;
            }

            value = @int.value;
            return true;
        }

        public void SetInteger(string name, int value)
        {
            var @int = ints.Where(x => x.name == name).FirstOrDefault();

            if (@int is null)
            {
                @int = new() { name = name };
                ints.Add(@int);
            }

            @int.value = value;
        }

        public float GetFloat(string name)
        {
            TryGetFloat(name, out float value);
            return value;
        }

        public bool TryGetFloat(string name, out float value)
        {
            var @float = floats.Where(x => x.name == name).FirstOrDefault();

            if (@float is null)
            {
                value = 0;
                return false;
            }

            value = @float.value;
            return true;
        }

        public void SetFloat(string name, float value)
        {
            var @float = floats.Where(x => x.name == name).FirstOrDefault();

            if (@float is null)
            {
                @float = new() { name = name };
                floats.Add(@float);
            }

            @float.value = value;
        }

        public bool TryGetString(string name, out string value)
        {
            var @string = strings.Where(x => x.name == name).FirstOrDefault();

            if (@string is null)
            {
                value = string.Empty;
                return false;
            }

            value = @string.value;
            return true;
        }

        public string GetString(string name)
        {
            TryGetString(name, out string value);
            return value;
        }

        public void SetString(string name, string value)
        {
            var @string = strings.Where(x => x.name == name).FirstOrDefault();

            if (@string is null)
            {
                @string = new() { name = name };
                strings.Add(@string);
            }

            @string.value = value;
        }

        public bool TryGetObject(string name, Type type, out UnityEngine.Object value)
        {
            var objects = this.objects.Where(x => x.name == name).ToList();

            Object @object;

            if (objects.Count == 0)
            {
                @object = null;
            }
            else if (objects.Count == 1)
            {
                @object = objects[0];
            }
            else
            {
                objects = objects.Where(
                    x => type.IsAssignableFrom(
                        x.value ? x.value.GetType() : x.GetValueType()
                    )
                ).ToList();

                if (objects.Count > 1)
                {
                    Debug.LogWarning("Found more than 1 possible parameter");
                    @object = null;
                }
                else
                {
                    @object = objects[0];
                }
            }

            if (@object is null)
            {
                value = null;
                return false;
            }

            value = @object.value;
            return true;
        }

        public T GetObject<T>() where T : UnityEngine.Object
        {
            return objects.Where(x => x.value is T).Select(x => x.value as T).FirstOrDefault();
        }

        public T GetComponent<T>() where T : UnityEngine.Object
        {
            return GetObject<T, GameObject>(x => x.GetComponent<T>());
        }

        public T GetComponentInChildren<T>() where T : UnityEngine.Object
        {
            return GetObject<T, GameObject>(x => x.GetComponentInChildren<T>());
        }

        public T GetComponentInParent<T>() where T : UnityEngine.Object
        {
            return GetObject<T, GameObject>(x => x.GetComponentInParent<T>());
        }

        public T GetObject<T>(Func<GameObject, T> conversionCallback) where T : UnityEngine.Object
        {
            return GetObject<T, GameObject>(conversionCallback);
        }

        public T GetObject<T, S>(Func<S, T> conversionCallback) where T : UnityEngine.Object where S : UnityEngine.Object
        {
            var result = objects.Where(x => x.value is T).Select(x => x.value as T).FirstOrDefault();
            if (result)
                return result;

            foreach (var @object in objects.Where(x => x.value is S).Select(x => x.value as S))
            {
                var alternativeResult = conversionCallback(@object);
                if (alternativeResult)
                    return alternativeResult;
            }
            return null;
        }

        public T GetObject<T, S1, S2>(Func<S1, T> conversionCallback1, Func<S2, T> conversionCallback2)
            where T : UnityEngine.Object
            where S1 : UnityEngine.Object
            where S2 : UnityEngine.Object
        {
            var result = objects.Where(x => x.value is T).Select(x => x.value as T).FirstOrDefault();
            if (result)
                return result;

            foreach (var @object in objects.Where(x => x.value is S1).Select(x => x.value as S1))
            {
                var alternativeResult = conversionCallback1(@object);
                if (alternativeResult)
                    return alternativeResult;
            }

            foreach (var @object in objects.Where(x => x.value is S2).Select(x => x.value as S2))
            {
                var alternativeResult = conversionCallback2(@object);
                if (alternativeResult)
                    return alternativeResult;
            }

            return null;
        }

        public void SetObject(string name, UnityEngine.Object value)
        {
            var @object = objects.Where(x => x.name == name).FirstOrDefault();

            if (@object is null)
            {
                @object = new() { name = name };
                objects.Add(@object);
            }

            @object.value = value;
        }
    }
}
