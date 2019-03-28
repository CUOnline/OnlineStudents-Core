using Newtonsoft.Json;
using RSS.Providers.Canvas.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rss.Providers.Canvas.Helpers
{
    public class RoleNames
    {
        public const string AccountAdmin = "AccountAdmin";
        public const string EnrollmentManager = "Enrollment Manager";
        public const string HelpDesk = "Help Desk";
        public const string OutcomesAdmin = "Outcomes Admin";
        public const string SubAccountAdmin = "Sub-Account Admin";
        public const string Other = "Other";
    }

    public class RoleNamesConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumString = (string)reader.Value;

            switch (enumString)
            {
                case RoleNames.AccountAdmin:
                case RoleNames.EnrollmentManager:
                case RoleNames.HelpDesk:
                case RoleNames.OutcomesAdmin:
                case RoleNames.SubAccountAdmin:
                    {
                        return enumString;
                    }
                default:
                    {
                        return RoleNames.Other;
                    }
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }
}