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
            if (FormatterGenerator.Serializers.ContainsKey(staticType) == false)
            {
                FormatterGenerator.CreateMethod(staticType);
            }
            this.serializer = FormatterGenerator.Serializers[staticType];
            this.deserializer = FormatterGenerator.Deserializers[staticType];
        }
        public byte[] Save()
        {
            return serializer.Invoke(this.resolver);
        }
        public void Load(byte[] data)
        {
            deserializer.Invoke(data, this.resolver);
        }
    }
}
