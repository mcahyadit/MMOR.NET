using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MMOR.Utils.Debugging
{
    public static class Debugging
    {
        private static readonly JsonSerializerSettings debugJsonSettings = new()
        {
            Formatting = Formatting.Indented,
            Converters = { new RichConverter() }
        };

        public static string RichDebug(this object obj)
        {
            string jsonSerialized = JsonConvert.SerializeObject(obj, debugJsonSettings);

            string result = Regex.Replace(jsonSerialized,
                @"\[\s*((?:\d+(?:\.\d+)?|true|false)(?:\s*,\s*(?:\d+(?:\.\d+)?|true|false))*\s*)\]", match =>
                {
                    string innerContent = Regex.Replace(match.Groups[1].Value, @"\s+", "");
                    innerContent = innerContent.Replace(",", ", ");
                    return $"[ {innerContent} ]";
                }, RegexOptions.Multiline);

            return result;
        }

        private class RichConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) =>
                typeof(IEnumerable<string>).IsAssignableFrom(objectType) &&
                objectType != typeof(string) &&
                objectType != typeof(byte[]);

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }
                writer.WriteStartArray();
                var collection = (IEnumerable<string>)value;
                foreach (string item in collection)
                    writer.WriteValue(item);
                writer.WriteEndArray();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer) => serializer.Deserialize(reader, objectType);
        }
    }
}