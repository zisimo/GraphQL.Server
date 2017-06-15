using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using GraphQL.Client;
using GraphQL.Language.AST;
using GraphQL.Server.Exceptions;
using Newtonsoft.Json;

namespace GraphQL.Server.Operation
{
    public class ProxyOperation<T> : IOperation where T : class, IOperation
    {
        public string Url { get; private set; }
        private Func<GraphClient<T>> GetGraphClient { get; set; }

        public ProxyOperation(Func<GraphClient<T>> getGraphClient)
        {
            GetGraphClient = getGraphClient;
        }
        public void Register(ApiSchema schema)
        {
            var methods = typeof(T).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            foreach (var methodInfo in methods)
            {
                if (methodInfo.Name == nameof(IOperation.Register)) continue;

                var isQuery = Enumerable.Any(methodInfo.CustomAttributes, a => a.AttributeType == typeof(QueryAttribute));
                var isMutation = Enumerable.Any(methodInfo.CustomAttributes, a => a.AttributeType == typeof(MutationAttribute));
                if (!isQuery && !isMutation)
                {
                    isQuery = true;
                }

                var apiOperation = isQuery ? schema.Query : schema.Mutation;
                var parameters = methodInfo.GetParameters();
                if (parameters.Length != 1)
                {
                    throw new GraphException($"An operation method must have one input parameter. Operation: {typeof(T).Name}.{methodInfo.Name}");
                }
                var fieldName = StringExtensions.PascalCase(methodInfo.Name);
                var fieldDescription = "";
                var queryArguments = GraphArguments.FromModel(parameters[0].ParameterType).GetQueryArguments();
                // Add function as operation
                schema.MapOutput(methodInfo.ReturnType);
                var graphType = TypeLoader.GetGraphType(methodInfo.ReturnType);
                apiOperation.Field(graphType, fieldName, fieldDescription, queryArguments, context =>
                {
                    var inputModel = ApiOperation.GetInputFromContext(context, parameters[0].ParameterType);
                    var graphClient = GetGraphClient();
                    var outputModel = GetOutputModel(context.FieldAst.SelectionSet.Selections.OfType<Field>());
                    var output = isQuery ? graphClient.RunQuery(fieldName, inputModel, outputModel) : graphClient.RunMutation(fieldName, inputModel, outputModel);
                    return output;
                });
            }
        }

        private dynamic GetOutputModel(IEnumerable<Field> selections)
        {
            IDictionary<string, object> output = new ExpandoObject();
            foreach (var selection in selections)
            {
                var name = $"{selection.Name}";
                if (selection.Arguments.Any())
                {
                    var arguments = selection.Arguments.Select(a => $"{a.Name}:{JsonConvert.SerializeObject(a.Value)}");
                    name = $"{name}({string.Join(",", arguments)})";
                }
                //output.Add(name, GetOutputModel(selection.SelectionSet.Selections.OfType<Field>()));
                output[name] = GetOutputModel(selection.SelectionSet.Selections.OfType<Field>());
            }
            return output;
        }
    }
}
