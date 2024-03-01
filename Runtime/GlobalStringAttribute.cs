using System;

using UnityEngine;

namespace Prototype.SequenceFlow
{
    public class GlobalStringAttribute : PropertyAttribute
    {
        public Type Type;
        public string Name;

        public GlobalStringAttribute(Type type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}
