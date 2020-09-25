using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Config.cs
{
    public class SerializeOption
    {
        public bool EnumAsString = false;
        public BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.Static;
    }
}
