using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Utf8Json;

namespace Config.cs
{
    public class SerializeOption
    {
        public bool EnumAsString = false;
        public bool PrettyPrint = false;
        public BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.Static;
    }
}
