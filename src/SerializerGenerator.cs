using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using Utf8Json;

namespace Config.cs
{
    internal class SerializerGenerator
    {
        static AssemblyBuilder assembly { get; } = AssemblyBuilder.DefineDynamicAssembly(new System.Reflection.AssemblyName("Hoge"),
            AssemblyBuilderAccess.Run);

        static ModuleBuilder module { get; } = assembly.DefineDynamicModule("Formatters");

        public static Dictionary<Type, Func<IJsonFormatterResolver, byte[]>> Serializers = new Dictionary<Type, Func<IJsonFormatterResolver, byte[]>>();
        public static Dictionary<Type, Action<byte[], IJsonFormatterResolver>> Deserializers = new Dictionary<Type, Action<byte[], IJsonFormatterResolver>>();
        static Dictionary<Type, TypeInfo> InstanceTypeMap = new Dictionary<Type, TypeInfo>();

        public static void CreateMethod(Type staticType)
        {
            if (Serializers.ContainsKey(staticType)) return;
            var (instanceType, _, _) = CreateInstanceType(staticType);
            InstanceTypeMap[staticType] = instanceType;

            var serializeMethodInfo = typeof(JsonSerializer).GetMethods()
                .First(x => x.Name == nameof(JsonSerializer.Serialize)
                && x.IsStatic && x.IsPublic && x.IsGenericMethod && x.GetParameters().Length == 2
                && x.GetParameters()[1].ParameterType == typeof(IJsonFormatterResolver)).MakeGenericMethod(instanceType);

            var FromStatic = instanceType.GetRuntimeMethod("FromStatic", new Type[0]);
            var ToStatic = instanceType.GetRuntimeMethod("ToStatic", new Type[0]);

            DynamicMethod serializer = new DynamicMethod("serialize", typeof(byte[]), new[] { typeof(IJsonFormatterResolver) });
            var sil = serializer.GetILGenerator();
            sil.DeclareLocal(instanceType);
            sil.Emit(OpCodes.Ldloca_S, 0); //1
            sil.Emit(OpCodes.Initobj, instanceType); //0


            sil.Emit(OpCodes.Ldloca_S, 0); //1
            sil.Emit(OpCodes.Call, FromStatic); // 0 this.FromStatic();

            sil.Emit(OpCodes.Ldloc_0, 0); //1
            sil.Emit(OpCodes.Ldarg_0); //2
            sil.Emit(OpCodes.Call, serializeMethodInfo); //1
            sil.Emit(OpCodes.Ret); //0

            Serializers.Add(staticType, (Func<IJsonFormatterResolver, byte[]>)serializer.CreateDelegate(typeof(Func<IJsonFormatterResolver, byte[]>)));

            var deserializeMethodInfo = typeof(JsonSerializer).GetMethods()
                .First(x => x.Name == nameof(JsonSerializer.Deserialize)
                && x.IsStatic && x.IsPublic && x.IsGenericMethod && x.GetParameters().Length == 2
                && x.GetParameters()[0].ParameterType == typeof(byte[]) && x.GetParameters()[1].ParameterType == typeof(IJsonFormatterResolver)).MakeGenericMethod(instanceType);
            DynamicMethod deserializer = new DynamicMethod("deserialize", null, new[] { typeof(byte[]), typeof(IJsonFormatterResolver) });
            var dil = deserializer.GetILGenerator();
            dil.DeclareLocal(instanceType);
            dil.Emit(OpCodes.Ldarg_0); //1
            dil.Emit(OpCodes.Ldarg_1); //2
            dil.Emit(OpCodes.Call, deserializeMethodInfo); //1
            dil.Emit(OpCodes.Stloc_0); //0 // var instance = JsonSerializer.Deserialize<InstanceType>(arg);

            dil.Emit(OpCodes.Ldloca_S, 0); //1
            dil.Emit(OpCodes.Call, ToStatic); //0
            dil.Emit(OpCodes.Ret); // JsonSerializer.Deserialize<InstanceType>(instanceType /*arg*/).ToStatic(); return;
            Deserializers.Add(staticType, (Action<byte[], IJsonFormatterResolver>)deserializer.CreateDelegate(typeof(Action<byte[], IJsonFormatterResolver>)));
        }

        private static (TypeInfo, MethodInfo, MethodInfo) CreateInstanceType(Type staticType)
        {

            return BuildRec(staticType);
            (TypeInfo, MethodInfo, MethodInfo) BuildRec(Type sType)
            {
                var attributes = TypeAttributes.Public | TypeAttributes.SequentialLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;
                //if (!first)
                //{
                //    attributes = TypeAttributes.NestedPublic | TypeAttributes.SequentialLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;
                //}
                //first = false;
                var name = sType.Name + "Instance";
                var typeBuilder = module.DefineType(sType.Name + "Instance", attributes, typeof(ValueType));
                var fromStatic = typeBuilder.DefineMethod("FromStatic", MethodAttributes.Public);
                var toStatic = typeBuilder.DefineMethod("ToStatic", MethodAttributes.Public);
                var fromStaticIL = fromStatic.GetILGenerator();
                var toStaticIL = toStatic.GetILGenerator();

                foreach (var field in sType.GetFields())
                {
                    var df = typeBuilder.DefineField(field.Name, field.FieldType, FieldAttributes.Public);

                    {
                        fromStaticIL.Emit(OpCodes.Ldarg_0); //1
                        fromStaticIL.Emit(OpCodes.Ldsfld, field); //1
                        fromStaticIL.Emit(OpCodes.Stfld, df); //0
                        // this.Field = StaticClass.Field
                    }

                    {
                        toStaticIL.Emit(OpCodes.Ldarg_0); //1
                        toStaticIL.Emit(OpCodes.Ldfld, df); //1
                        toStaticIL.Emit(OpCodes.Stsfld, field); //0
                        //StaticClass.Field = this.Field
                    }
                }
                foreach (var nt in sType.GetNestedTypes().Except(new[] { sType }))
                {
                    var (type, fst, tst) = BuildRec(nt);
                    var df = typeBuilder.DefineField(nt.Name, type, FieldAttributes.Public);

                    {
                        //fromStaticIL.Emit(OpCodes.Ldarg_0); //1
                        //fromStaticIL.Emit(OpCodes.Ldflda, df); //2
                        //fromStaticIL.Emit(OpCodes.Initobj, type); //1
                        //fromStaticIL.Emit(OpCodes.Stfld, df); //0 this.Field = new FieldType();
                        fromStaticIL.Emit(OpCodes.Ldarg_0); //1
                        fromStaticIL.Emit(OpCodes.Ldflda, df); //1
                        fromStaticIL.Emit(OpCodes.Call, fst); //0
                        //this.Field
                    }

                    {
                        toStaticIL.Emit(OpCodes.Ldarg_0); //1
                        toStaticIL.Emit(OpCodes.Ldflda, df); //1
                        toStaticIL.Emit(OpCodes.Call, tst); //0
                    }
                }
                fromStaticIL.Emit(OpCodes.Ret);
                toStaticIL.Emit(OpCodes.Ret);

                return (typeBuilder.CreateTypeInfo(), fromStatic, toStatic);
            }

        }
    }
    public static class SerializerCache
    {
        public static readonly Dictionary<Type, Func<byte[]>> Serializers;
        public static readonly Dictionary<Type, Action<byte[]>> Deserializers;

    }

}
