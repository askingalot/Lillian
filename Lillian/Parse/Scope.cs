using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lillian.Parse
{
    public class Scope
    {
        public Scope Parent { get; }

        private readonly IDictionary<string, Expression> _internalScope = 
            new Dictionary<string, Expression>();

        public Scope(IDictionary<string, Expression> initialState) : this(null, initialState) { }
        public Scope(Scope parent = null, IDictionary<string, Expression>  initalState = null)
        {
            Parent = parent;
            if (initalState != null)
            {
                foreach (var kvp in initalState)
                {
                    _internalScope.Add(kvp);
                }
            }
        }

        public void Add(string key, Expression value) => _internalScope.Add(key, value);
        public Expression this[string key] => _internalScope[key];
        public bool ContainsKey(string key) => _internalScope.ContainsKey(key);
    }
}