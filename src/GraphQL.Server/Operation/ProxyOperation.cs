using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL.Client;
using GraphQL.Language.AST;
using GraphQL.Server.Exceptions;
using GraphQL.Types;

namespace GraphQL.Server.Operation
{
    public class ProxyOperation<TInterface> : IOperation where TInterface : class, IOperation
    {
        public string Url { get; private set; }
        private Func<GraphClient<TInterface>> GetGraphClient { get; set; }
        public Dictionary<string, Func<ResolveFieldContext<object>, string, object, object>> PostOperations { get; set; }

        public ProxyOperation(Func<GraphClient<TInterface>> getGraphClient)
        {
            GetGraphClient = getGraphClient;
            PostOperations = new Dictionary<string, Func<ResolveFieldContext<object>, string, object, object>>();
        }
        public void Register(ApiSchema schema)
        {
            var methods = typeof(TInterface).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
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
                    throw new GraphException($"An operation method must have one input parameter. Operation: {typeof(TInterface).Name}.{methodInfo.Name}");
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
                    var query = graphClient.AddSelectionQuery(fieldName, inputModel, context.FieldAst.SelectionSet.Selections.OfType<Field>());
                    if (isQuery)
                    {
                        graphClient.RunQueries();
                    }
                    else
                    {
                        graphClient.RunMutations();
                    }
                    var output = query.Data.ToObject(methodInfo.ReturnType);
                    if (PostOperations.ContainsKey(fieldName))
                    {
                        output = PostOperations[fieldName](context, fieldName, output);
                    }
                    return output;
                });
            }
        }

        public void AddPostOperation(string operationName, Func<ResolveFieldContext<object>, string, object, object> postFunction)
        {
            PostOperations[StringExtensions.PascalCase(operationName)] = postFunction;
        }
    }
}
