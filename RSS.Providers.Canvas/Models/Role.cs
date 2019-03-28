using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rss.Providers.Canvas.Helpers;
using RSS.Providers.Canvas.Models.Enums;

namespace RSS.Providers.Canvas.Models
{
    public class Role
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "role")]
        [JsonConverter(typeof(RoleNamesConverter))]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "role_id")]
        public string RoleId { get; set; }

        [JsonProperty(PropertyName = "workflow_state")]
        [JsonConverter(typeof(StringEnumConverter))]
        public WorkflowState WorkflowState { get; set; }
    }
}
