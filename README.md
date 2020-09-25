# Config.cs

## Installation
The library provides in NuGet Package.
Standard library is available for .NET Framework 4.5 and .NET Standard 2.0.

Type
```
Install-Package StaticSerializer -Version 1.0.0
```
in PackageManager Console to install package.

## Quick Start
Usage is very simple.
Suppose that you want to perpetuate static class like this.
```cs
public static class ParentClass
{
    public static int value1 = 1;
    public static string value2 = "abc";
    public static Alphabet alphabet = Alphabet.B;

    public static class ChildClass
    {
        public static float value = 1;
        public static class GrancChildClass
        {
            public static string message = "hello";
        }
    }
}
public enum Alphabet
{
    A,B,C
}
```
First, Instantiate serializer.
You can specify some option in Serializer Option class.

- If you want to serialize enum value to string for readablity, set EnumAsString Option to "true".
- By default, serializer deals with Public and Static property. You can specify binding flags like below.
```cs
var serializer = new Serializer(typeof(TestClass),
    new SerializeOption()
    {
        EnumAsString = true,
        BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
    });
```

You can call ```Config.cs.JsonSerializer```.```Serialize/Deserialize```
```cs
//static class -> byte[] (UTF8)
var data = serializer.Serialize();

//byte[] -> directory write to static class
serializer.Deserialize();
```

## Special Thanks
Thanks to [Utf8Json](https://github.com/neuecc/Utf8Json) library, the development was smooth, and thic package made stable and fast.

Thanks to author of Utf8Json, [neuecc](https://github.com/neuecc/)