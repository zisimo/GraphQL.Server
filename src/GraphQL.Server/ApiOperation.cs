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
                    var astArgs = context.FieldAst.Arguments.ToDictionary(argument => argument.Name);
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

        private static InputField[] CollectFields(Dictionary<string, Argument> astArguments)
        {
            var output = new List<InputField>();
            foreach (var argument in astArguments)
            {
                var field = new InputField() { Name = argument.Value.Name };
                var childArguments = argument.Value.Children.OfType<Argument>().ToDictionary(a => a.Name);
                field.Fields = CollectFields(childArguments);
                //if (argument.Value.Value is Dictionary<string, object>)
                //{
                //    var subFieldObjects = argument.Value.Children as Dictionary<string, object>;
                //    var subFieldArguments = subFieldObjects.ToDictionary(pair => pair.Key, pair => new Argument() { Name = pair.Key, Value = pair.Value });
                //    field.Fields = CollectFields(subFieldArguments);
                //}
                //if (argument.Value.Value is List<object>)
                //{
                //    var subFieldObjects = argument.Value.Value as List<object>;
                //    var fields = new List<InputField>();
                //    foreach (var subFieldObject in subFieldObjects)
                //    {
                //        var subField = new InputField() { Name = argument.Value.Name };
                //        if (subFieldObject is Dictionary<string, object>)
                //        {
                //            var objects = subFieldObject as Dictionary<string, object>;
                //            var args = objects.ToDictionary(pair => pair.Key, pair => new Argument() { Name = pair.Key, Value = pair.Value });
                //            subField.Fields = CollectFields(args);
                //        }
                //        fields.Add(subField);
                //    }
                //    field.Fields = fields.ToArray();
                //}
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