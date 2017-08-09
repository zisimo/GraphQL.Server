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
            var checkedObjects = new List<object>();
            while (resolverInfo?.ParentResolverInfo != null && !checkedObjects.Contains(resolverInfo.Source))
            {
                checkedObjects.Add(resolverInfo.Source);
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
            var checkedObjects = new List<object>();
            while (resolverInfo?.ParentResolverInfo != null && !checkedObjects.Contains(resolverInfo.Source))
            {
                checkedObjects.Add(resolverInfo.Source);
                resolverInfo = resolverInfo.ParentResolverInfo;
                if (parentType.IsAssignableFrom(resolverInfo.Source.GetType()))
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

        public TUserContext GetUserContext<TUserContext>()
        {
            var resolverInfo = this;
            var checkedObjects = new List<object>();
            while (resolverInfo?.ParentResolverInfo != null && !checkedObjects.Contains(resolverInfo.Source))
            {
                if (resolverInfo.Context.UserContext != null)
                {
                    return (TUserContext)resolverInfo.Context.UserContext;
                }
                checkedObjects.Add(resolverInfo.Source);
                resolverInfo = resolverInfo.ParentResolverInfo;
            }
            return default(TUserContext);
        }

        public void SetParentResolverInfo(ResolverInfo parentResolverInfo)
        {
            if (parentResolverInfo != null && ParentResolverInfo == null)
            {
                ParentResolverInfo = parentResolverInfo;
            }
        }
    }
}
