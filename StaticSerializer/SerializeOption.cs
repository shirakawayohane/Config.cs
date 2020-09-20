using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace TypedConfig.Serialization
{
    public class SerializeOption
    {
        public bool EnumAsString = false;
        public BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.Static;
    }
}
