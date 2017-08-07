using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQL.Server
{
    public interface IResolverInfo
    {
        ResolveFieldContext<object> Context { get; }
        IResolverInfo ParentResolverInfo { get; }
        object SourceObject { get; }
        IEnumerable<Field> Selection { get; }

        object[] GetParents();
        TParent GetParent<TParent>() where TParent : class;
        void SetParentResolverInfo(IResolverInfo sourceResolverInfo);
    }
    public class ResolverInfo<TSource> : IResolverInfo where TSource : class
    {
        public ResolveFieldContext<object> Context { get; }
        public IResolverInfo ParentResolverInfo { get; private set; }
        public object SourceObject { get; }
        public TSource Source => SourceObject as TSource;
        public IEnumerable<Field> Selection { get; private set; }

        public ResolverInfo(ResolveFieldContext<object> context, TSource source)
        {
            Context = context;
            SourceObject = source;
            Selection = Context.FieldAst.SelectionSet.Selections.OfType<Field>();
        }

        public object[] GetParents()
        {
            var output = new List<object>();
            var resolverInfo = this as IResolverInfo;
            while (resolverInfo?.ParentResolverInfo != null && resolverInfo.ParentResolverInfo != resolverInfo)
            {
                resolverInfo = resolverInfo.ParentResolverInfo;
                if (resolverInfo.SourceObject == null) break;
                output.Add(resolverInfo.SourceObject);
            }
            return output.ToArray();
        }

        public TParent GetParent<TParent>() where TParent : class 
        {
            var parentType = typeof(TParent);
            var resolverInfo = this as IResolverInfo;
            while (resolverInfo?.ParentResolverInfo != null && resolverInfo.ParentResolverInfo != resolverInfo)
            {
                resolverInfo = resolverInfo.ParentResolverInfo;
                if (resolverInfo.SourceObject.GetType().FullName == parentType.FullName)
                {
                    return resolverInfo.SourceObject as TParent;
                }
            }
            return null;
        }

        public void SetParentResolverInfo(IResolverInfo parentResolverInfo)
        {
            ParentResolverInfo = parentResolverInfo;
        }
    }
}
