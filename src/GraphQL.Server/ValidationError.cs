using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQL.Server
{
    public class ValidationError
    {
        public bool IsMissing { get; set; }
        public bool IsInvalid { get; set; }
        public QueryArgument Argument { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }

        public static ValidationError ValidateField<T>(List<ValidationError> errors, Dictionary<string, QueryArgument> args, Dictionary<string, IValue> astArgs, ResolveFieldContext context, string field)
        {
            return ValidateField(typeof(T), errors, args, astArgs, context, field);
        }
        public static ValidationError ValidateField(Type fieldType, List<ValidationError> errors, Dictionary<string, QueryArgument> args, Dictionary<string, IValue> astArgs, ResolveFieldContext<object> context, string field)
        {
            ValidationError error = null;
            if (args.ContainsKey(field))
            {
                var required = typeof(NonNullGraphType).IsAssignableFrom(args[field].Type) || typeof(NonNullGraphType<>).IsAssignableFrom(args[field].Type);
                var canBeNull = !fieldType.IsValueType || (Nullable.GetUnderlyingType(fieldType) != null);
                var canAssignType = context.Arguments.ContainsKey(field) && context.Arguments[field] != null && (fieldType.IsAssignableFrom(context.Arguments[field].GetType()));
                var isCorrectType = canAssignType || context.Arguments[field] == null && canBeNull;
                var isArrayOrDictionary = context.Arguments.ContainsKey(field) && context.Arguments[field] != null && (context.Arguments[field].GetType().HasElementType || context.Arguments[field] is Dictionary<string, object>);
                var isInvalid = !isArrayOrDictionary && astArgs.ContainsKey(field) && !isCorrectType;
                error = new ValidationError
                {
                    Argument = args[field],
                    Name = field,
                    Value = context.Arguments[field], //astArgs[field].Value,
                    IsMissing = required && !astArgs.ContainsKey(field),
                    IsInvalid = isInvalid
                };
            }
            if (error != null && (error.IsMissing || error.IsInvalid))
            {
                errors.Add(error);
                return error;
            }
            return null;
        }

        public static void ValidateObject(object obj)
        {
            if (obj == null) return;
            var context = new ValidationContext(obj, null, null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(obj, context, results, true);
            if (isValid) return;
            throw new ValidationException($"Validation failed[{string.Join(", ", results.Select(r => r.ErrorMessage))}]");
        }

        public static void Throw(ValidationError[] errors)
        {
            if (errors.Length == 0) return;
            var message = string.Empty;
            if (errors.Count(e => e.IsMissing) > 0) message += "Missing[" + string.Join(", ", errors.Where(e => e.IsMissing).Select(e => e.Name)) + "]";
            if (errors.Count(e => e.IsInvalid) > 0)
            {
                var errorMessages = new List<string>();
                foreach (var error in errors.Where(e => e.IsInvalid))
                {
                    errorMessages.Add(error.Name);
                }
                message += "Invalid[" + string.Join(", ", errorMessages) + "]";
            }
            throw new ValidationException(message);
        }
    }

}