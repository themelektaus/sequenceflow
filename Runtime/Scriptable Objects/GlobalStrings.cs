using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype.SequenceFlow
{
    [CreateAssetMenu]
    public class GlobalStrings : ScriptableObject
    {
        public const string NEW = "(New)";

        [SerializeField]
        List<string> strings = new();

        public string[] GetAllStrings(bool includeNew)
        {
            var result = Enumerable.Empty<string>();

            if (includeNew)
                result = result.Append(NEW);

            return result
                .Concat(strings)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();
        }

        public virtual bool AddString(string @string)
        {
            if (GetAllStrings(true).Contains(@string))
                return false;

            strings.Add(@string);
            return true;
        }
    }
}
