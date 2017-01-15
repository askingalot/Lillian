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
        public bool ContainsKey(string key)
        {
            return _internalScope.ContainsKey(key) 
                || (Parent?.ContainsKey(key) ?? false);

        }

        public Expression this[string key]
        {
            get
            {
                if (_internalScope.ContainsKey(key))
                    return _internalScope[key];

                if (Parent != null)
                    return Parent[key];

                throw new KeyNotFoundException($"{key} not found in current scope.");
            }
        }
    }
}