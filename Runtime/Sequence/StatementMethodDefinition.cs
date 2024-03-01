using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prototype.SequenceFlow
{
    public abstract class StatementMethodDefinition
    {
        static HashSet<StatementMethodDefinition> defaultInstances;

        public static HashSet<StatementMethodDefinition> GetDefaultInstances()
            => defaultInstances ??= Utils
                .GetAll<StatementMethodDefinition>()
                .Select(CreateInstance)
                .ToHashSet();

        public static StatementMethodDefinition GetDefaultInstance(string identifier)
            => GetDefaultInstances()
                .FirstOrDefault(
                    x => x.ToString() == identifier || x.menuPath == identifier
                );

        public static StatementMethodDefinition CreateInstance(string identifier)
            => CreateInstance(
                GetDefaultInstance(identifier).GetType(),
                setGuid: false
            );

        static StatementMethodDefinition CreateInstance(System.Type type)
            => CreateInstance(type, setGuid: true);

        static StatementMethodDefinition CreateInstance(System.Type type, bool setGuid)
        {
            var instance = System.Activator.CreateInstance(type) as StatementMethodDefinition;
            if (setGuid)
                instance.guid = type.GetCustomAttribute<GuidAttribute>(false)?.guid;
            instance.fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            return instance;
        }

        public FieldInfo[] fieldInfos { get; private set; }

        string guid { get; set; }

        public virtual Color color => Color.clear;
        public virtual float colorAlpha => .15f;
        public virtual string menuPath => GetType().Name;

        public Color styleColor => new(color.r, color.g, color.b, colorAlpha);

        protected Statement statement { get; private set; }

        protected Transform activator => statement.activator;
        protected MonoBehaviour owner => statement.owner;
        protected Transform executer => statement.executer;
        protected EventArgs e => statement.e;

        protected string GetNameCombinedWithScene(string name)
        {
            return $"[{SceneManager.GetActiveScene().name}] {name}";
        }

        public override string ToString()
        {
            return guid ?? GetType().FullName;
        }

        public void Prepare(Statement statement, object[] parameters)
        {
            this.statement = statement;

            for (int i = 0; i < parameters.Length; i++)
                fieldInfos[i].SetValue(this, parameters[i]);
        }

        public bool Invoke(Statement statement, object[] parameters)
        {
            Prepare(statement, parameters);
            return Check();
        }

        public abstract bool Check();
    }
}
