using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prototype.SequenceFlow
{
    public abstract class SequenceMethodDefinition
    {
        static HashSet<SequenceMethodDefinition> defaultInstances;

        public static HashSet<SequenceMethodDefinition> GetDefaultInstances()
            => defaultInstances ??= Utils
                .GetAll<SequenceMethodDefinition>()
                .Select(CreateInstance)
                .ToHashSet();

        public static SequenceMethodDefinition GetDefaultInstance(string identifier)
            => GetDefaultInstances()
                .FirstOrDefault(
                    x => x.ToString() == identifier || x.menuPath == identifier
                );

        public static SequenceMethodDefinition CreateInstance(string identifier)
            => CreateInstance(
                GetDefaultInstance(identifier).GetType(),
                setGuid: false
            );

        static SequenceMethodDefinition CreateInstance(System.Type type)
            => CreateInstance(type, setGuid: true);

        static SequenceMethodDefinition CreateInstance(System.Type type, bool setGuid)
        {
            var instance = System.Activator.CreateInstance(type) as SequenceMethodDefinition;
            if (setGuid)
                instance.guid = type.GetCustomAttribute<GuidAttribute>(false)?.guid;
            instance.fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            return instance;
        }

        public FieldInfo[] fieldInfos { get; private set; }

        string guid { get; set; }

        public enum Mode { Absolute = 0, Relative = 1 }

        public enum Axis { X = 0, Y = 1, Z = 2 }

        public virtual Color color => Color.clear;
        public virtual float colorAlpha => .15f;
        public virtual string displayName => null;
        public virtual string menuPath => GetType().Name;
        public abstract bool waitable { get; }

        public Color styleColor => new(color.r, color.g, color.b, colorAlpha);

        protected Sequence sequence { get; private set; }
        protected Sequence.Data data => sequence.data;

        protected Transform activator => sequence.activator;
        protected MonoBehaviour owner => sequence.owner;
        protected Transform executer => sequence.executer;
        protected EventArgs e => sequence.e;

        protected void Prepare(Sequence sequence, object[] parameters)
        {
            this.sequence = sequence;

            for (int i = 0; i < parameters.Length; i++)
                fieldInfos[i].SetValue(this, parameters[i]);
        }

        protected string GetNameCombinedWithScene(string name)
        {
            return $"[{SceneManager.GetActiveScene().name}] {name}";
        }

        public override string ToString()
        {
            return guid ?? GetType().FullName;
        }

        public virtual void OnExitSequence()
        {

        }
    }
}
