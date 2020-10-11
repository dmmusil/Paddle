using System;
using System.Collections.Generic;

namespace DomainTactics.Messaging
{
    public class TypeMapper
    {
        private readonly Dictionary<Type, string> _typeToName = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> _nameToType = new Dictionary<string, Type>();

        public void Register(Type t, string name)
        {
            _typeToName[t] = name;
            _nameToType[name] = t;
        }

        public Type TypeFor(string name) => _nameToType[name];
        public string NameFor(Type type) => _typeToName[type];
    }
}