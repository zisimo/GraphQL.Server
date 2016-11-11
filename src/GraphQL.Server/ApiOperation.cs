using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Language;
using GraphQL.Language.AST;
using GraphQL.Types;
using GraphQL.Server.Security;
using Newtonsoft.Json;

namespace GraphQL.Server
{
    public class ApiOperation : GraphObject<object>
    {
        public ApiOperation(IContainer container, string name) : base(container)
        {
            Name = name;
        }

        public void AddQuery<TOutputObject, TInput>(Func<TInput, InputField[], object> function)
            where TOutputObject : GraphType
            where TInput : class, new()
        {
            var fieldName = StringExtensions.PascalCase(function.Method.Name);
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
                var errors = new List<ValidationError>();
                InputField[] fields = null;
                if (context.FieldAst.Arguments != null)
                {
                    var args = queryArguments.ToDictionary(argument => argument.Name);
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
                        ValidationError.ValidateField(argument.Value.Type, errors, args, astArgs, context, argument.Value.Name);
                    }
                    ValidationError.Throw(errors.ToArray());
                    fields = CollectFields(astArgs);
                }
                var valuesJson = JsonConvert.SerializeObject(values);
                var inputModel = JsonConvert.DeserializeObject<TInput>(valuesJson);
                ValidationError.ValidateObject(inputModel);
                try
                {
                    return function.Invoke(inputModel, fields);
                }
                catch (Exception ex)
                {
                    throw;
                }
            });
        }

        private static InputField[] CollectFields(Dictionary<string, IValue> astArguments)
        {
            var output = new List<InputField>();
            foreach (var argument in astArguments)
            {
                var field = new InputField() { Name = argument.Key };
                ObjectValue value = null;
                if (argument.Value is ListValue)
                {
                    var list = (ListValue) argument.Value;
                    value = (ObjectValue) list.Values.First();
                }
                else if (argument.Value is ObjectValue)
                {
                    value = (ObjectValue)argument.Value;
                }
                if (value != null)
                {
                    var childArguments = value.ObjectFields.ToDictionary(o => o.Name, o => o.Value);
                    field.Fields = CollectFields(childArguments);
                }
                output.Add(field);
            }
            return output.ToArray();
        }

        private static InputField[] CollectFields(Dictionary<string, Argument> astArguments)
        {
            var output = new List<InputField>();
            foreach (var argument in astArguments)
            {
                var field = new InputField() { Name = argument.Value.Name };
                var childArguments = argument.Value.Children.OfType<Argument>().ToDictionary(a => a.Name);
                field.Fields = CollectFields(childArguments);
                output.Add(field);
            }
            return output.ToArray();
        }

        private static void AuthorizeFunction(IContainer container, string qualifiedFunctionName)
        {
            if (!container.GetInstance<AuthorizationMap>().Authorize(qualifiedFunctionName))
            {
                throw new AuthorizationException(qualifiedFunctionName);
            }
        }
    }
}