using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQL.Server
{
    public class ResolverInfo
    {
        public ResolveFieldContext<object> Context { get; }
        public ResolverInfo ParentResolverInfo { get; private set; }
        public object Source { get; }
        public IEnumerable<Field> Selection { get; private set; }

        public ResolverInfo(ResolveFieldContext<object> context, object source)
        {
            Context = context;
            Source = source;
            Selection = Context.FieldAst.SelectionSet.Selections.OfType<Field>();
        }

        public object[] GetParents()
        {
            var output = new List<object>();
            var resolverInfo = this;
            while (resolverInfo?.ParentResolverInfo != null && resolverInfo.ParentResolverInfo != resolverInfo)
            {
                resolverInfo = resolverInfo.ParentResolverInfo;
                if (resolverInfo.Source == null) break;
                output.Add(resolverInfo.Source);
            }
            return output.ToArray();
        }

        public TParent GetParent<TParent>() where TParent : class 
        {
            var parentType = typeof(TParent);
            var resolverInfo = this;
            while (resolverInfo?.ParentResolverInfo != null && resolverInfo.ParentResolverInfo != resolverInfo)
            {
                resolverInfo = resolverInfo.ParentResolverInfo;
                if (resolverInfo.Source.GetType().FullName == parentType.FullName)
                {
                    return resolverInfo.Source as TParent;
                }
            }
            return null;
        }

        public TSource GetSource<TSource>() where TSource : class
        {
            return Source as TSource;
        }

        public void SetParentResolverInfo(ResolverInfo parentResolverInfo)
        {
            ParentResolverInfo = parentResolverInfo;
        }
    }
}
