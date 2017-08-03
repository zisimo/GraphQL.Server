using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQL.Server
{
    public interface IResolverInfo
    {
        ResolveFieldContext<object> Context { get; set; }

        void AddParents(params object[] parents);
        object[] GetParents();
        TParent GetParent<TParent>() where TParent : class;
    }
    public class ResolverInfo<TSource> : IResolverInfo where TSource : class
    {
        private readonly List<object> _parents;

        public ResolveFieldContext<object> Context { get; set; }
        public TSource Source { get; set; }
        public IEnumerable<Field> Selection => Context.FieldAst.SelectionSet.Selections.OfType<Field>();

        public ResolverInfo(TSource source)
        {
            _parents = new List<object>();
            Source = source;
        }

        public void AddParents(params object[] parents)
        {
            _parents.AddRange(parents);
        }

        public object[] GetParents()
        {
            return _parents.ToArray();
        }

        public TParent GetParent<TParent>() where TParent : class 
        {
            var parentType = typeof(TParent);
            var parent = _parents.FirstOrDefault(p => p.GetType().FullName == parentType.FullName);
            return parent as TParent;
        }
    }
}
