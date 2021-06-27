using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nandro.Nano
{
    class NanoSubscriptionOptions
    {
        [JsonPropertyName("accounts")]
        public IEnumerable<string> Accounts { get; set; }

        public NanoSubscriptionOptions(string account)
        {
            Accounts = new List<string> { account };
        }
    }

    class NanoSocketAction
    {
        [JsonPropertyName("action")]
        public string Action { get; set; }
        [JsonPropertyName("topic")]
        public string Topic { get; set; }
        [JsonPropertyName("ack")]
        public bool Ack { get; set; }
        [JsonPropertyName("options")]
        public NanoSubscriptionOptions Options { get; set; }

        public NanoSocketAction(string action, string topic, string account)
        {
            Action = action;
            Topic = topic;
            Ack = true;
            Options = new NanoSubscriptionOptions(account);
        }
    }

    public class NanoAckResponse
    {
        [JsonPropertyName("ack")]
        public string Ack { get; set; }
        [JsonPropertyName("time")]
        public string Time { get; set; }
        [JsonPropertyName("id")]
        public string Id;
    }

    public class NanoBlock
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonPropertyName("previous")]
        public string Previous { get; set; }

        [JsonPropertyName("representative")]
        public string Representative { get; set; }

        [JsonPropertyName("balance")]
        public string Balance { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("link_as_account")]
        public string LinkAsAccount { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; }

        [JsonPropertyName("work")]
        public string Work { get; set; }

        [JsonPropertyName("subtype")]
        public string Subtype { get; set; }
    }

    public class NanoConfirmationMessage
    {
        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonPropertyName("amount")]
        public string Amount { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("confirmation_type")]
        public string ConfirmationType { get; set; }

        [JsonPropertyName("block")]
        public NanoBlock Block { get; set; }
    }

    public class NanoConfirmationResponse
    {
        [JsonPropertyName("topic")]
        public string Topic { get; set; }
        [JsonPropertyName("time")]
        public string Time { get; set; }
        [JsonPropertyName("message")]
        public NanoConfirmationMessage Message { get; set; }
    }
}
