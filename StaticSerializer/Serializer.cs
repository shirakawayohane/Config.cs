using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Utf8Json;

namespace Config.cs
{
    public class Serializer
    {
        protected readonly SerializeOption _option;
        protected readonly Type _type;
        public Serializer(Type staticType,SerializeOption option = default) {
            if (option != null)
            {
                this._option = option;
            } else
            {
                this._option = new SerializeOption();
            }
            _type = staticType;
        }

        public byte[] Serialize()
        {
            var writer = new JsonWriter();
            return serialize(ref writer);
        }
        private byte[] serialize(ref JsonWriter writer)
        {
            writer.WriteBeginObject();
            var fields = _type.GetFields(_option.BindingFlags);
            for(int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                writer.WritePropertyName(field.Name);
                switch(field.GetValue(null))
                {
                    case string str:
                        writer.WriteString(str);
                        break;
                    case bool b:
                        writer.WriteBoolean(b);
                        break;
                    case long l:
                        writer.WriteInt64(l);
                        break;
                    case ulong ul:
                        writer.WriteUInt64(ul);
                        break;
                    case int n:
                        writer.WriteInt32(n);
                        break;
                    case uint un:
                        break;
                    case short s:
                        writer.WriteInt32(s);
                        break;
                    case ushort us:
                        writer.WriteUInt32(us);
                        break;
                    case byte bt:
                        writer.WriteByte(bt);
                        break;
                    case sbyte sbt:
                        writer.WriteSByte(sbt);
                        break;
                    case float f:
                        writer.WriteSingle(f);
                        break;
                    case double d:
                        writer.WriteDouble(d);
                        break;
                    case Enum e:
                        if(_option.EnumAsString)
                        {
                            writer.WriteString(e.ToString());
                        } else
                        {
                            writer.WriteInt32(Convert.ToInt32(e));
                        }
                        break;
                    default:
                        writer.WriteNull();
                        break;
                }
                if (i >= fields.Length - 1) break;
                writer.WriteValueSeparator();
            }
            var subClasses = _type.GetNestedTypes(_option.BindingFlags);
            if(subClasses.Length > 0 && fields.Length > 0)
            {
                writer.WriteValueSeparator();
            }
            for(int i = 0; i < subClasses.Length; i++)
            {
                var type = subClasses[i];
                var serializer = new Serializer(type, _option);
                writer.WritePropertyName(type.Name);
                serializer.serialize(ref writer);
                if (i >= subClasses.Length - 1) break;
                writer.WriteValueSeparator();
            }
            writer.WriteEndObject();
            return writer.ToUtf8ByteArray();
        }

        public void Deserialize(byte[] bytes)
        {
            var reader = new JsonReader(bytes);
            deserialize(ref reader);
        }
        private void deserialize(ref JsonReader reader)
        {
            var fields = _type.GetFields(_option.BindingFlags);
            var nestedClasses = _type.GetNestedTypes(_option.BindingFlags);
            var fieldNames = fields.Select(x => x.Name);
            var nestedClassesNames = nestedClasses.Select(x => x.Name);

            reader.ReadIsBeginObject();
            while(!reader.ReadIsEndObject())
            {
                var propName = reader.ReadPropertyName();
                var field = fields.FirstOrDefault(fi => fi.Name == propName);
                if(field != null)
                {
                    var type = field.FieldType;
                    var value = field.GetValue(null);
                    switch (value)
                    {
                        case string str:
                            field.SetValue(null,reader.ReadString());
                            break;
                        case bool b:
                            field.SetValue(null,reader.ReadBoolean());
                            break;
                        case long l:
                            field.SetValue(null,reader.ReadInt64());
                            break;
                        case ulong ul:
                            field.SetValue(null,reader.ReadUInt64());
                            break;
                        case int n:
                            field.SetValue(null,reader.ReadInt32());
                            break;
                        case uint un:
                            break;
                        case short s:
                            field.SetValue(null,reader.ReadInt32());
                            break;
                        case ushort us:
                            field.SetValue(null,reader.ReadUInt32());
                            break;
                        case byte bt:
                            field.SetValue(null,reader.ReadByte());
                            break;
                        case sbyte sbt:
                            field.SetValue(null,reader.ReadSByte());
                            break;
                        case float f:
                            field.SetValue(null,reader.ReadSingle());
                            break;
                        case double d:
                            field.SetValue(null,reader.ReadDouble());
                            break;
                        case Enum e:
                            if(reader.GetCurrentJsonToken() == JsonToken.String)
                            {
                                var enumString = reader.ReadString();
                                try
                                {
                                    field.SetValue(null, Enum.Parse(field.FieldType, enumString));
                                }
                                finally { }
                            } else
                            {
                                field.SetValue(null, reader.ReadInt32());
                            }
                            break;
                    }
                } else
                {
                    var nestedClass = nestedClasses.FirstOrDefault(x => x.Name == propName);
                    if(nestedClass != null)
                    {
                        new Serializer(nestedClass,_option).deserialize(ref reader);
                    }
                }
                reader.ReadIsValueSeparator();
            }
        }
    }
}
