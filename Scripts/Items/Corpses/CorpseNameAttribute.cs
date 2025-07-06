using System;

namespace Server
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CorpseNameAttribute(string name) : Attribute
    {
        public string Name => name;
    }
}
