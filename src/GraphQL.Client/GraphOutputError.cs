using Newtonsoft.Json;

namespace GraphQL.Client
{
    public class GraphOutputError
    {
        public string Detail { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
