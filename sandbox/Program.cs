using System;
using Config.cs;
using System.Reflection;
using Config.cs;

class Program
{
    static void Main(string[] args)
    {
        var resolver = Utf8Json.Resolvers.StandardResolver.Default;
        var serializer = new StaticJsonSerializer(typeof(TestClass), resolver);
        var data = serializer.Save();

        Console.WriteLine(System.Text.Encoding.UTF8.GetString(data));

        TestClass.value1 = 425;
        TestClass.value2 = "fsjf";
        TestClass.ChildClass.value = 3.1415f;
        TestClass.ChildClass.GrancChildClass.message = "fkjdnfiu";

        serializer.Load(data);


    }
}
public static class TestClass
{
    public static int value1 = 1;
    public static string value2 = "abc";
    //public static Alphabet alphabet = Alphabet.B;
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