using Newtonsoft.Json;

namespace GraphQL.Server
{
    public class GraphQLError
    {
        public string Detail { get; set; }
        public string Message { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StackTrace { get; set; }

        public GraphQLError(string message, string stackTrace)
        {
            Message = message;
            StackTrace = stackTrace;
        }
    }
}