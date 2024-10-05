#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEditor;
#endif

using System.Collections.Generic;

using UnityEngine;
using System.Text;

namespace Prototype.SequenceFlow
{
    [CreateAssetMenu(menuName = "Prototype/Global Strings")]
    public class GlobalStrings : ScriptableObject
    {
        [SerializeField]
        List<string> strings = new();

#if UNITY_EDITOR
        public const string NEW = "(New)";

        [SerializeField]
        bool generateNow;

        public IEnumerable<string> EnumerateAllStrings(bool includeNew)
        {
            var result = Enumerable.Empty<string>();
            if (includeNew)
                result = result.Append(NEW);
            return result.Concat(strings.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        public bool AddString(string @string)
        {
            if (EnumerateAllStrings(includeNew: true).Contains(@string))
                return false;

            strings.Add(@string);
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
            File.WriteAllText(
                Path.Combine(
                    Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)),
                    $"{name}.cs"
                ),
                new StringBuilder()
                    .AppendLine($"public static class {GenerateValidName(name)}")
                    .AppendLine("{")
                    .Append("    ").AppendLine(
                        string.Join(
                            "\r\n    ",
                            EnumerateAllStrings(includeNew: false).Select(
                                x => $"public const string {GenerateValidName(x)} = \"{x}\";"
                            )
                        )
                    )
                    .AppendLine("}")
                    .AppendLine()
                    .ToString()
            );

            Editor.finishedDefaultHeaderGUI -= RefreshAssetDatabase;
            Editor.finishedDefaultHeaderGUI += RefreshAssetDatabase;
        }

        static void RefreshAssetDatabase(Editor editor)
        {
            Debug.Log(nameof(RefreshAssetDatabase));
            Editor.finishedDefaultHeaderGUI -= RefreshAssetDatabase;
            AssetDatabase.Refresh();
        }

        static string GenerateValidName(string @string) => Regex.Replace(
            Regex.Replace(@string, "[\\:\\-]", "_"), "[^A-Za-z0-9_]", ""
        ).Trim();
#endif
    }
}
