using System;
using System.Collections.Generic;

namespace Prototype.SequenceFlow
{
    public class GuidAttribute : Attribute
    {
        static readonly HashSet<string> guids = new();

        public readonly string guid;

        public GuidAttribute(string guid)
        {
            if (guid == string.Empty)
                throw new InvalidOperationException("Guid is empty");

            if (guids.Contains(guid))
                throw new InvalidOperationException($"GUID {guid} already exists");

            this.guid = guid;

            guids.Add(guid);
        }
    }
}
