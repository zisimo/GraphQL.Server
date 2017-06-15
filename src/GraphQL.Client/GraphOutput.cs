using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client
{
    public class GraphOutput<T>
    {
        public T Data { get; set; }
        public GraphOutputError[] Errors { get; set; }
        public string Query { get; set; }
        public string Variables { get; set; }
    }
    public class GraphOutput : GraphOutput<JObject>
    {
    }
}
