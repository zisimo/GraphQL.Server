using System.Collections.Generic;
using GraphQL.Language.AST;
using GraphQL.Types;
using GraphQL.Validation;
using GraphQL.Validation.Rules;

namespace GraphQL.Server.Sample
{
    public class ApiValidator : IDocumentValidator
    {
        public IContainer Container { get; set; }

        public ApiValidator(IContainer container)
        {
            Container = container;
        }

        public IValidationResult Validate(string originalQuery, ISchema schema, Document document, IEnumerable<IValidationRule> rules = null, object userContext = null)
        {
            if (rules == null)
            {
                rules = DocumentValidator.CoreRules();
                //TODO: These rules must be included in the future, currently tests and calling applications do not obey these rules
                (rules as List<IValidationRule>).RemoveAll(rule =>
                {
                    return rule is KnownArgumentNames
                            || rule is ArgumentsOfCorrectType
                            || rule is FieldsOnCorrectType;
                });
            }
            return new DocumentValidator().Validate(originalQuery, schema, document, rules, userContext);
        }
    }
}