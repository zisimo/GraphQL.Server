using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.Server
{
    public class GraphQLOutput
    {
        public object Data { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<GraphQLError> Errors { get; set; }

        public GraphQLOutput()
        {
        }

        public GraphQLOutput(object data, ExecutionError[] exceptions)
        {
            Data = data;
            if (exceptions != null && exceptions.Length > 0)
            {
                Errors = new List<GraphQLError>();
                foreach (var exception in exceptions)
                {
                    var error = new GraphQLError(exception.Message, GetExceptionInformation(exception));
                    var innerException = exception.InnerException;
                    while (innerException != null)
                    {
                        error.Detail = innerException.Message;
                        innerException = innerException.InnerException;
                    }
                    Errors.Add(error);
                }
            }
        }

        private static string GetExceptionInformation(Exception exception)
        {
            if (exception == null) return string.Empty;
            return $"====={exception.Message}={exception.StackTrace}{GetExceptionInformation(exception.InnerException)}";
        }

        public T GetData<T>(string operation)
        {
            string objectJson = null;

            if (Data == null) return default(T);

            if (Data is JObject)
            {
                objectJson = JsonConvert.SerializeObject((Data as JObject)[operation]);
            }
            if (Data is Dictionary<string, object>)
            {
                objectJson = JsonConvert.SerializeObject((Data as Dictionary<string, object>)[operation]);
            }
            return JsonConvert.DeserializeObject<T>(objectJson);
        }

        public object GetRawData(string operation)
        {
            string objectJson = null;

            if (Data == null) return null;

            if (Data is JObject)
            {
                objectJson = JsonConvert.SerializeObject((Data as JObject)[operation]);
            }
            if (Data is Dictionary<string, object>)
            {
                objectJson = JsonConvert.SerializeObject((Data as Dictionary<string, object>)[operation]);
            }
            return JsonConvert.DeserializeObject<object>(objectJson);
        }

        public GraphData<T> GetGraphData<T>(string operation)
        {
            return GraphData<T>.Create(this, GetData<T>(operation), GetRawData(operation));
        }
    }

    public class GraphData<T> : GraphQLOutput
    {
        public object RawData { get; set; }
        public T Data { get; set; }

        public static GraphData<T> Create(GraphQLOutput graphQlOutput, T data, object rawData)
        {
            var output = new GraphData<T>()
            {
                RawData = rawData,
                Data = data,
                Errors = graphQlOutput.Errors
            };
            return output;
        }

        public TProperty GetProperty<TProperty>(string propertyName)
        {
            if (RawData == null) return default(TProperty);
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var objectJson = JsonConvert.SerializeObject(RawData, settings);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(objectJson);
            if (dictionary[propertyName] == null) return default(TProperty);
            objectJson = JsonConvert.SerializeObject(dictionary[propertyName], settings);
            return JsonConvert.DeserializeObject<TProperty>(objectJson);
        }
    }
}