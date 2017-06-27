using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client
{
    public class GraphOutput<T>
    {
        public T Data { get; set; }
        public GraphOutputError[] Errors { get; set; }
        public bool HasErrors => Errors.Length > 0;
        public string Query { get; set; }
        public string Variables { get; set; }

        public void ThrowErrors()
        {
            if (HasErrors)
            {
                throw new GraphClientException(JsonConvert.SerializeObject(Errors));
            }
        }
    }
    public class GraphOutput : GraphOutput<JObject>
    {
    }
}
