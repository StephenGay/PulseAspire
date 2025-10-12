using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pulse.ApiService
{
    

    public class TypeConverter : JsonConverter<Type>
    {
        public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Deserialization optional; throw if not needed for security
            throw new NotImplementedException("Type deserialization not supported.");
        }

        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.FullName); // e.g., "System.Decimal" for decimal types
        }
    }
}
