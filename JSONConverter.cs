using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MPGApp
{

    public class TeamConverter : JsonConverter<Teams>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == typeof(Teams))
            {
                return true;
            }

            return false;
        }

        public override Teams Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var t = new Teams();
            if (reader.TokenType != JsonTokenType.Null)
            {
                if (JsonTokenType.StartArray != reader.TokenType)
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.String:
                            var sValue = reader.GetString();
                            //_ = double.TryParse(sValue, out ret);
                            break;
                        case JsonTokenType.Number:
                            //_ = reader.TryGetDouble(out ret);
                            break;
                        default:
                            break;

                    }
                }
            }
            return t;
        }

        public override void Write(Utf8JsonWriter writer, Teams value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
    public class StringToDoubleConverter : JsonConverter<double>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == typeof(double))
            {
                return true;
            }

            return false;
        }

        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            double ret = 0;
            if (reader.TokenType != JsonTokenType.Null)
            {
                if (JsonTokenType.StartArray != reader.TokenType)
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.String:
                            var sValue = reader.GetString();
                            _ = double.TryParse(sValue, out ret);
                            break;
                        case JsonTokenType.Number:
                            _ = reader.TryGetDouble(out ret);
                            break;
                        default:
                            break;

                    }
                }
            }
            return ret;
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToIntConverter : JsonConverter<int>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == typeof(int))
            {
                return true;
            }

            return false;
        }

        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int ret = 0;
            if (reader.TokenType != JsonTokenType.Null)
            {
                if (JsonTokenType.StartArray != reader.TokenType)
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.String:
                            var sValue = reader.GetString();
                            _ = int.TryParse(sValue, out ret);
                            break;
                        case JsonTokenType.Number:
                            _ = reader.TryGetInt32(out ret);
                            break;
                        default:
                            break;

                    }
                }
            }
            return ret;
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
