using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSS.Providers.Canvas.Models
{
    public class Account : BaseModel
    {
        [JsonProperty(PropertyName = "parent_account_id")]
        public string ParentAccountId { get; set; }

        [JsonProperty(PropertyName = "root_account_id")]
        public string RootAccountId { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        public string Uuid { get; set; }

        [JsonProperty(PropertyName = "sis_account_id")]
        public string SisAccountId { get; set; }
    }
}
