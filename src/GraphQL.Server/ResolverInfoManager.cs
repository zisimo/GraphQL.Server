using System;
using System.Collections.Generic;
using GraphQL.Types;

namespace GraphQL.Server
{
    public class ResolverInfoManager
    {
        private readonly Dictionary<object, ResolverInfo> _resolverInfo;

        public ResolverInfoManager()
        {
            _resolverInfo = new Dictionary<object, ResolverInfo>();
        }

        public ResolverInfo Create(ResolveFieldContext<object> context, object source = null)
        {
            if (source == null)
            {
                source = context.Source;
            }
            if (!_resolverInfo.ContainsKey(source))
            {
                _resolverInfo[source] = (ResolverInfo)Activator.CreateInstance(typeof(ResolverInfo), context, source);
            }
            return _resolverInfo[source];
        }
    }
}
