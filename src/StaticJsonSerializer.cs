using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Utf8Json;
namespace Config.cs
{
    public class StaticJsonSerializer
    {
        Func<IJsonFormatterResolver, byte[]> serializer;
        Action<byte[], IJsonFormatterResolver> deserializer;
        IJsonFormatterResolver resolver;
        /// <param name="resolver">if null use default of Utf8Json</param>
        public StaticJsonSerializer(Type staticType, IJsonFormatterResolver resolver = null)
        {
            if (resolver == null)
            {
                this.resolver = Utf8Json.Resolvers.StandardResolver.Default;
            }
            else
            {
                this.resolver = resolver;
            }
            if (SerializerGenerator.Serializers.ContainsKey(staticType) == false)
            {
                SerializerGenerator.CreateMethod(staticType);
            }
            this.serializer = SerializerGenerator.Serializers[staticType];
            this.deserializer = SerializerGenerator.Deserializers[staticType];
        }

        /// <summary>
        /// Save static class directly into byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] Save()
        {
            return serializer.Invoke(this.resolver);
        }

        public string SaveJson(bool prettify = false)
        {
            var bytes = serializer.Invoke(this.resolver);
            if (prettify)
            {
                return JsonSerializer.PrettyPrint(bytes);
            }
            else
            {
                return Encoding.UTF8.GetString(bytes);
            }
        }


        /// <summary>
        /// Load data from json directory into static class.
        /// </summary>
        /// <param name="json"></param>
        public void LoadJson(string json)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            Load(bytes);
        }

        /// <summary>
        /// Load json bytes directly into static class.
        /// </summary>
        /// <param name="data"></param>
        public void Load(byte[] data)
        {
            deserializer.Invoke(data, this.resolver);
        }
    }
}
