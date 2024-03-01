using UnityEngine;

#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
#endif

namespace Prototype.SequenceFlow
{
    [CreateAssetMenu]
    public class EventTypes : GlobalStrings
    {
#if UNITY_EDITOR
        [SerializeField]
        bool generateNow;

        public override bool AddString(string @string)
        {
            if (!base.AddString(@string))
                return false;

            Generate();
            return true;
        }

        void OnValidate()
        {
            if (generateNow)
            {
                generateNow = false;
                Generate();
            }
        }
        void Generate()
        {
            var file = new FileInfo(AssetDatabase.GetAssetPath(this));

            var className = name.Replace(" ", "");
            if (!className.EndsWith("enum", StringComparison.InvariantCultureIgnoreCase))
                className += "Enum";

            var path = Path.Combine(file.DirectoryName, $"{className}.cs");

            var enums = string.Join(",\r\n    ", GetAllStrings(false));
            var contents = $"public enum {className}\r\n{{\r\n    {enums}\r\n}}\r\n";

            File.WriteAllText(path, contents);

            AssetDatabase.Refresh();
        }
#endif
    }
}
