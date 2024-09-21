using System;

using UnityEngine;

namespace Prototype.SequenceFlow
{
    public class GlobalStringAttribute : PropertyAttribute
    {
        public string Name;

        public GlobalStringAttribute(string name)
        {
            Name = name;
        }
    }
}
