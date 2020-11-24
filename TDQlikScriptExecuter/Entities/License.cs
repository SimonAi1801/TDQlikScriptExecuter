using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TDQlikScriptExecuter.Entities
{
    public class License
    {
        public string MacAddress { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("customer")]
        public string CustomerName { get; set; }

        [JsonPropertyName("validUntil")]
        public string ValidUntil { get; set; }

        public bool IsKeyValid => DateTime.Now <= Convert.ToDateTime(ValidUntil);

        public override string ToString()
        {
            return $"Key={Key};ValidUntil={ValidUntil}";
        }
    }
}
