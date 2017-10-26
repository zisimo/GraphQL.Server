using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using GraphQL.Server.Security;
using GraphQL.Types;
using Newtonsoft.Json;

namespace GraphQL.Server
{
    public class ApiOperation : GraphObject<object>
    {
        public ApiOperation(IContainer container, string name) : base(container)
        {
            Name = name;
        }

        public void AddQuery<TOutput, TInput>(Func<TInput, TOutput> function)
            where TOutput : class
            where TInput : class, new()
        {
            var wrappingFunction = new Func<object, object>(input => function((TInput)input));
            AddQuery(function.Method.Name, typeof(TInput), typeof(TOutput), wrappingFunction);
        }

        public void AddQuery(string fieldName, Type inputType, Type outputType, Func<object, object> function)
        {
            fieldName = fieldName.ToCamelCase();
            var fieldDescription = "";
            var arguments = GraphArguments.FromModel(inputType);
            var queryArguments = arguments.GetQueryArguments();
            // Add function as operation
            var graphType = TypeLoader.GetGraphType(outputType);
            Field(graphType, fieldName, fieldDescription, queryArguments, context =>
            {
                var inputModel = GetInputFromContext(context, inputType);
                ValidationError.ValidateObject(inputModel);

                return function.Invoke(inputModel);
            });
        }

        public static object GetInputFromContext(ResolveFieldContext<object> context, Type inputType)
        {
            var values = new Dictionary<string, object>();
            if (context.FieldAst.Arguments != null)
            {
                var arguments = GraphArguments.FromModel(inputType);
                var astArgs = context.FieldAst.Arguments.Children.OfType<Argument>().ToDictionary(a => a.Name, a => a.Value);
                foreach (var argument in arguments)
                {
                    if (!astArgs.ContainsKey(argument.Value.Name)) continue;
                    var jsonString = JsonConvert.SerializeObject(context.Arguments[argument.Value.Name]);
                    values[argument.Value.Name] = JsonConvert.DeserializeObject(jsonString, argument.Value.Type);
                }
            }
            var valuesJson = JsonConvert.SerializeObject(values);
            return JsonConvert.DeserializeObject(valuesJson, inputType);
        }

        public void AddQuery<TOutputObject, TInput>(Func<TInput, InputField[], object> function)
            where TOutputObject : GraphType
            where TInput : class, new()
        {
            var fieldName = function.Method.Name.ToCamelCase();
            var authFieldName = $"{fieldName}()";
            var fieldDescription = "";
            var arguments = GraphArguments.FromModel<TInput>();
            var queryArguments = arguments.GetQueryArguments();
            // Function authorization
            var functionAuth = function.Method.GetCustomAttributes(typeof(AuthorizeAttribute), true).FirstOrDefault() as AuthorizeAttribute;
            if (functionAuth == null && function.Method.DeclaringType != null)
            {
                functionAuth = function.Method.DeclaringType.GetCustomAttributes(typeof(AuthorizeAttribute), true).FirstOrDefault() as AuthorizeAttribute;
            }
            if (functionAuth != null)
            {
                var authMap = Container.GetInstance<AuthorizationMap>();
                authMap.Authorizations.Add(new Authorization()
                {
                    TargetName = authFieldName,
                    Roles = functionAuth.Claims
                });
            }
            // Add function as operation
            Field<TOutputObject>(fieldName, fieldDescription, queryArguments, context =>
            {
                AuthorizeFunction(Container, authFieldName);
                var values = new Dictionary<string, object>();
                InputField[] fields = null;
                if (context.FieldAst.Arguments != null)
                {
                    var astArgs = context.FieldAst.Arguments.Children.OfType<Argument>().ToDictionary(a => a.Name, a => a.Value);

                    foreach (var argument in arguments)
                    {
                        if (astArgs.ContainsKey(argument.Value.Name))
                        {
                            try
                            {
                                var jsonString = JsonConvert.SerializeObject(context.Arguments[argument.Value.Name]);
                                values[argument.Value.Name] = JsonConvert.DeserializeObject(jsonString, argument.Value.Type);
                            }
                            catch
                            {
                            }
                        }
                    }
                    fields = CollectFields(astArgs);
                }
                var valuesJson = JsonConvert.SerializeObject(values);
                var inputModel = JsonConvert.DeserializeObject<TInput>(valuesJson);
                ValidationError.ValidateObject(inputModel);
                return function.Invoke(inputModel, fields);
            });
        }

        private static InputField[] CollectFields(Dictionary<string, IValue> astArguments)
        {
            var output = new List<InputField>();
            foreach (var argument in astArguments)
            {
                var field = new InputField() { Name = argument.Key };
                if (argument.Value is ListValue)
                {
                    var list = (ListValue)argument.Value;
                    var fields = new List<InputField>();
                    foreach (var child in list.Values)
                    {
                        if (child is ObjectValue)
                        {
                            var value = (ObjectValue)child;
                            var childField = new InputField() { Name = argument.Key };
                            var childArguments = value.ObjectFields.ToDictionary(o => o.Name, o => o.Value);
                            childField.Fields = CollectFields(childArguments);
                            fields.Add(childField);
                        }
                    }
                    field.Fields = fields.ToArray();
                }
                if (argument.Value is ObjectValue)
                {
                    var value = (ObjectValue)argument.Value;
                    var childArguments = value.ObjectFields.ToDictionary(o => o.Name, o => o.Value);
                    field.Fields = CollectFields(childArguments);
                }
                output.Add(field);
            }
            return output.ToArray();
        }

        private static void AuthorizeFunction(IContainer container, string qualifiedFunctionName)
        {
            var permissions = container.GetInstance<UserPermissions>().Permissions;
            if (!container.GetInstance<AuthorizationMap>().Authorize(qualifiedFunctionName, permissions))
            {
                throw new AuthorizationException(qualifiedFunctionName);
            }
        }
    }
}