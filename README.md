# Config.cs

## Installation
The library provides in NuGet Package.
This is available for .NET Standard 2.0 compatibles.

```
dotnet add package Config.cs --version 2.0.1
```

## Important Note
For the flexibility and performance and maintanance reasons, Public API has drastically changed from v1.0^.
Please note that there is no compatibility between v1 and v2.

## Quick Start
Usage is very simple.
Suppose that you want to perpetuate static class like this.
```cs
public static class TestClass
{
    public static int value1 = 1;
    public static string value2 = "abc";
    public static Alphabet alphabet = Alphabet.B;
    public static int[] arr = new[] { 1, 2, 3, 4 };

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
    A, B, C
}
```
First, Instantiate serializer.
You can control how to serialize/deserialize data by specifying [Resolver of Utf8Json](https://github.com/neuecc/Utf8Json#resolver)

```cs
using Config.cs;
using Utf8Json;
/*=================================*/

var serializer = new StaticJsonSerializer(typeof(ParentClass), Utf8Json.Resolvers.StandardResolver.Default);
```

You can call ```Config.cs.JsonSerializer```.```Serialize/Deserialize```
```cs
//static class -> byte[] (UTF8)
byte[] data = serializer.Save();

//byte[] -> directory write to static class
serializer.Load(byte[] data);
```
In this case, the decoded data will be
```json
{"value1":1,"value2":"abc","alphabet":"B","arr":[1,2,3,4],"ChildClass":{"value":1,"GrancChildClass":{"message":"hello"}}}
```

## Special Thanks
Thanks to [Utf8Json](https://github.com/neuecc/Utf8Json) library, the development was smooth, and thic package made stable and fast.

I owe to the author [neuecc](https://github.com/neuecc/).