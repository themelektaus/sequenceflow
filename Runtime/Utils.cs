using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


#if UNITY_EDITOR
using EditorGUI = UnityEditor.EditorGUI;
using IMGUIContainer = UnityEngine.UIElements.IMGUIContainer;
#endif

namespace Prototype.SequenceFlow
{
    public static class Utils
    {
        static HashSet<Type> _types;
        static HashSet<Type> types => _types ??= AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .ToHashSet();

        static readonly Dictionary<Type, HashSet<Type>> typeCache = new();

        public static HashSet<Type> GetAll<T>()
        {
            var key = typeof(T);

            if (!typeCache.TryGetValue(key, out var value))
            {
                typeCache.Add(
                    key,
                    value = types
                        .Where(key.IsAssignableFrom)
                        .Where(x => !x.IsAbstract)
                        .ToHashSet()
                );
            }

            return value;
        }

        public static void Set(ref int a, Operation operation, int b)
        {
            switch (operation)
            {
                case Operation.Set: a = b; break;
                case Operation.Add: a += b; break;
                case Operation.Subtract: a -= b; break;
                case Operation.Multiply: a *= b; break;
                case Operation.Divide: a /= b; break;
                case Operation.Unify:
                    if (a == 0) a = 1;
                    a += b * a * a;
                    break;
                case Operation.Difference: a = Mathf.Abs(a - b); break;
            }
        }

        public static void Set(ref float a, Operation operation, float b)
        {
            switch (operation)
            {
                case Operation.Set: a = b; break;
                case Operation.Add: a += b; break;
                case Operation.Subtract: a -= b; break;
                case Operation.Multiply: a *= b; break;
                case Operation.Divide: a /= b; break;
                case Operation.Unify:
                    if (a == 0) a = 1;
                    a += b * a * a;
                    break;
                case Operation.Difference: a = Mathf.Abs(a - b); break;
            }
        }

        public static void Set(ref Vector2 a, Operation operation, Vector2 b)
        {
            switch (operation)
            {
                case Operation.Set: a = b; break;
                case Operation.Add: a += b; break;
                case Operation.Subtract: a -= b; break;
                case Operation.Multiply: a = new(a.x * b.x, a.y * b.y); break;
                case Operation.Divide: a = new(a.x / b.x, a.y / b.y); break;
                case Operation.Unify:
                    float x = 0;
                    float y = 0;
                    Set(ref x, Operation.Unify, b.x);
                    Set(ref y, Operation.Unify, b.y);
                    a = new(x, y);
                    break;
                case Operation.Difference: throw new NotImplementedException();
            }
        }

        public static void Set(ref Vector3 a, Operation operation, Vector3 b)
        {
            switch (operation)
            {
                case Operation.Set: a = b; break;
                case Operation.Add: a += b; break;
                case Operation.Subtract: a -= b; break;
                case Operation.Multiply: a = new(a.x * b.x, a.y * b.y, a.z * b.z); break;
                case Operation.Divide: a = new(a.x / b.x, a.y / b.y, a.z / b.z); break;
                case Operation.Unify:
                    float x = 0;
                    float y = 0;
                    float z = 0;
                    Set(ref x, Operation.Unify, b.x);
                    Set(ref y, Operation.Unify, b.y);
                    Set(ref z, Operation.Unify, b.z);
                    a = new(x, y, z);
                    break;
                case Operation.Difference: throw new NotImplementedException();
            }
        }

        public static void Set(ref string a, StringOperation operation, string b)
        {
            switch (operation)
            {
                case StringOperation.Set: a = b; break;
                case StringOperation.Append: a += b; break;
            }
        }

        public static bool Equals(int a, StatementType type, int b)
        {
            switch (type)
            {
                case StatementType.Equals: return a == b;
                case StatementType.NotEquals: return a != b;
                case StatementType.GreaterThan: return a > b;
                case StatementType.GreaterEqualsThan: return a >= b;
                case StatementType.LessThan: return a < b;
                case StatementType.LessEqualsThan: return a <= b;
            }
            return false;
        }

        public static bool Equals(float a, StatementType type, float b)
        {
            switch (type)
            {
                case StatementType.Equals: return a == b;
                case StatementType.NotEquals: return a != b;
                case StatementType.GreaterThan: return a > b;
                case StatementType.GreaterEqualsThan: return a >= b;
                case StatementType.LessThan: return a < b;
                case StatementType.LessEqualsThan: return a <= b;
            }
            return false;
        }

        public static bool Equals(string a, StringStatementType type, string b)
        {
            switch (type)
            {
                case StringStatementType.Equals: return a == b;
                case StringStatementType.NotEquals: return a != b;
                case StringStatementType.StartsWith: return a.StartsWith(b);
                case StringStatementType.EndsWith: return a.EndsWith(b);
                case StringStatementType.StartMatch: return b.StartsWith(a);
                case StringStatementType.EndMatch: return b.EndsWith(a);
                case StringStatementType.LengthEquals: return a.Length == int.Parse(b);
            }
            return false;
        }

        public static bool Equals(UnityEngine.Object a, ObjectStatementType type, UnityEngine.Object b)
        {
            switch (type)
            {
                case ObjectStatementType.Equals: return a == b;
                case ObjectStatementType.NotEquals: return a != b;
            }
            return false;
        }

#if UNITY_EDITOR
        readonly static Dictionary<string, object> cache = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => cache.Clear();

        static string GetCacheKey(params object[] args)
        {
            return string.Join('|', args.Select(a => a.ToString()));
        }

        static T GetCache<T>(string key)
        {
            if (!cache.ContainsKey(key))
                return default;

            if (cache[key] is not null)
                return (T) cache[key];

            cache.Remove(key);
            return default;
        }

        public static Texture2D CreateTexture(Color color, int width, int height)
        {
            var key = GetCacheKey("texture", color, width, height);

            var result = GetCache<Texture2D>(key);

            if (result == null)
            {
                var pixels = new Color32[width * height];

                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = color;

                result = new(width, height) { filterMode = FilterMode.Point };
                result.SetPixels32(pixels);
                result.Apply();

                cache.TryAdd(key, result);
            }

            return result;
        }

        public static Texture2D CreateCircle(Color color, float radius, int padding)
        {
            var key = GetCacheKey("circle", color, radius, padding);

            var result = GetCache<Texture2D>(key);

            if (result is null)
            {
                result = CreateTexture(
                    Color.clear,
                    Mathf.FloorToInt(radius * 2.1f) + padding * 2,
                    Mathf.FloorToInt(radius * 2.1f) + padding * 2
                );

                int x, y, px, nx, py, ny, d;

                for (x = 0; x < radius; x++)
                {
                    d = (int) Mathf.Ceil(Mathf.Sqrt(radius * radius - x * x));
                    for (y = 0; y <= d; y++)
                    {
                        px = Mathf.RoundToInt(radius * 1.05f) + x;
                        nx = Mathf.RoundToInt(radius * 1.05f) - x;
                        py = Mathf.RoundToInt(radius * 1.05f) + y;
                        ny = Mathf.RoundToInt(radius * 1.05f) - y;
                        result.SetPixel(px + padding, py + padding, color);
                        result.SetPixel(nx + padding, py + padding, color);
                        result.SetPixel(px + padding, ny + padding, color);
                        result.SetPixel(nx + padding, ny + padding, color);
                    }
                }

                result.Apply();

                cache.TryAdd(key, result);
            }

            return result;
        }

        public static string StringToCamelCase(string @this)
        {
            var result = System.Text.RegularExpressions.Regex.Replace(@this, "([A-Z])", " $1");
            return result[0].ToString().ToUpper() + result[1..];
        }

        public static VisualElement CreatePropertyField(string label, Func<Rect> onGUI)
        {
            var container = new IMGUIContainer();
            container.AddToClassList("unity-base-field__input");
            (container.onGUIHandler = () =>
            {
                var rect = onGUI();
                container.style.width = rect.width;
                container.style.height = rect.height;
            })();

            var field = new VisualElement();
            field.AddToClassList("unity-base-field");
            field.Add(new Label(label) { style = { unityTextAlign = TextAnchor.UpperRight } });
            field.Add(container);
            return field;
        }
#endif
    }
}
