using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MMOR.NET.Debugging {
  public static class Debugging {
    private static readonly JsonSerializerSettings kJsonSettings = new() {
      Formatting = Formatting.Indented,
      Converters = { new RichConverter() },
    };

    public static string RichDebug(this object obj) {
      string json_serialized = JsonConvert.SerializeObject(obj, kJsonSettings);

      string result = Regex.Replace(json_serialized,
          @"\[\s*((?:\d+(?:\.\d+)?|true|false)(?:\s*,\s*(?:\d+(?:\.\d+)?|true|false))*\s*)\]",
          match => {
            string inner_content = Regex.Replace(match.Groups[1].Value, @"\s+", "");
            inner_content        = inner_content.Replace(",", ", ");
            return $"[ {inner_content} ]";
          },
          RegexOptions.Multiline);

      return result;
    }

    private class RichConverter : JsonConverter {
      public override bool CanConvert(
          Type object_type) => typeof(IEnumerable<string>).IsAssignableFrom(object_type) &&
                              object_type != typeof(string) && object_type != typeof(byte[]);

      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        if (value == null) {
          writer.WriteNull();
          return;
        }
        writer.WriteStartArray();
        var collection = (IEnumerable<string>)value;
        foreach (string item in collection) writer.WriteValue(item);
        writer.WriteEndArray();
      }

      public override object ReadJson(JsonReader reader, Type object_type, object existing_value,
          JsonSerializer serializer) => serializer.Deserialize(reader, object_type);
    }
  }
}
