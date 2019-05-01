using Newtonsoft.Json;
using System;


namespace Gac.Logistics.Aes.Api.Profile
{
    public class EmptyStringToNullJsonConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(string) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                var s = reader.Value;
                if (s == null || s is string)
                {
                    string value = (string)reader.Value;
                    return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                }

                if (s is long)
                {
                    string value = reader.Value.ToString();
                    return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                }
                return reader.Value;

            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return reader.Value;

            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //throw new NotImplementedException("Unnecessary because CanWrite is false. The type will skip the converter.");
        }
    }
}