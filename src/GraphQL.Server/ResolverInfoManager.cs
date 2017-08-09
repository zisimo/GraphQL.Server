using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<ResolverInfo> Create(ResolveFieldContext<object> context)
        {
            return Create(context, null, null);
        }

        public IEnumerable<ResolverInfo> Create(ResolveFieldContext<object> context, object source, ResolverInfo parentResolverInfo)
        {
            if (source == null)
            {
                source = context.Source;
            }
            if (source == null)
            {
                return null;
            }
            var output = new List<ResolverInfo>();
            var sourceType = source.GetType();
            var isArray = sourceType.IsArray;
            var isEnumerable = sourceType != typeof(string) && sourceType.GetInterfaces().Any(t => t.Name.Contains(nameof(IEnumerable)));
            if (isArray || isEnumerable)
            {
                var items = (IEnumerable) source;
                foreach (var item in items)
                {
                    output.Add(CreateResolverInfo(context, item, parentResolverInfo));
                }
            }
            else
            {
                output.Add(CreateResolverInfo(context, source, parentResolverInfo));
            }

            return output;
        }

        private ResolverInfo CreateResolverInfo(ResolveFieldContext<object> context, object source, ResolverInfo parentResolverInfo)
        {
            if (!_resolverInfo.ContainsKey(source))
            {
                _resolverInfo[source] = (ResolverInfo)Activator.CreateInstance(typeof(ResolverInfo), context, source);
            }
            _resolverInfo[source].SetParentResolverInfo(parentResolverInfo);
            return _resolverInfo[source];
        }
    }
}
