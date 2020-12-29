using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MPGApp
{

    public class TeamConverter : JsonConverter<MPGLeagueTeams.Teams>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == typeof(MPGLeagueTeams.Teams))
            {
                return true;
            }

            return false;
        }

        public override MPGLeagueTeams.Teams Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var teams = new MPGLeagueTeams.Teams();
            teams.mpg_teams = new List<MPGLeagueTeams.Teams.Mpg_Team>();
            if (reader.TokenType != JsonTokenType.Null)
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var startDepth = reader.CurrentDepth;

                    MPGLeagueTeams.Teams.Mpg_Team t = null;
                    String currentProperty = "";
                    bool newTeam = false;

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            currentProperty = reader.GetString();

                            if (currentProperty.Contains("mpg_team_"))
                                newTeam = true;
                        }
                        else if (reader.TokenType == JsonTokenType.String)
                        {
                            switch (currentProperty)
                            {
                                case "id":
                                    t.id = reader.GetString();
                                    break;
                                case "name":
                                    t.name = reader.GetString();
                                    break;
                                default:
                                    break;
                            }
                        }
                        else if (reader.TokenType == JsonTokenType.StartArray)
                        {
                            t.players = System.Text.Json.JsonSerializer.Deserialize<List<MPGLeagueTeams.Teams.Mpg_Team.Player>>(ref reader);
                        }
                        else if (reader.TokenType == JsonTokenType.StartObject && newTeam)
                        {
                            t = new MPGLeagueTeams.Teams.Mpg_Team();
                        }
                        else if (reader.TokenType == JsonTokenType.EndObject && newTeam)
                        {
                            teams.mpg_teams.Add(t);
                            newTeam = false;
                        }
                        else if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == startDepth)
                            return teams;
                    }
                }
            }
            return teams;
        }

        public override void Write(Utf8JsonWriter writer, MPGLeagueTeams.Teams value, JsonSerializerOptions options)
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
