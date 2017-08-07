using System;
using System.Collections.Generic;
using GraphQL.Types;

namespace GraphQL.Server
{
    public class ResolverInfoManager
    {
        private readonly Dictionary<object, IResolverInfo> _resolverInfo;

        public ResolverInfoManager()
        {
            _resolverInfo = new Dictionary<object, IResolverInfo>();
        }

        public IResolverInfo Create(ResolveFieldContext<object> context, object source = null)
        {
            if (source == null)
            {
                source = context.Source;
            }
            if (!_resolverInfo.ContainsKey(source))
            {
                var resolverInfoType = typeof(ResolverInfo<>).MakeGenericType(source.GetType());
                _resolverInfo[source] = (IResolverInfo)Activator.CreateInstance(resolverInfoType, context, source);
            }
            return _resolverInfo[source];
        }
    }
}
