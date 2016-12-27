using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lillian.Tokenize
{
    public class TokenEnumerator : IEnumerator<Token>
    {
        private readonly Token[] _tokens;
        private readonly IDictionary<string, int> _savePositions = 
            new Dictionary<string, int>(); 

        public TokenEnumerator(IEnumerable<Token> tokens)
        {
            _tokens = tokens.ToArray();
            CurrentPosition = -1;
        }

        public int CurrentPosition { get; private set; }

        public void CreateSavePoint(string name)
        {
            _savePositions.Add(name, CurrentPosition);
        }

        public void RevertToSavePoint(string name)
        {
            CurrentPosition = _savePositions[name];
            foreach (var savePos in _savePositions)
            {
                if (savePos.Value >= CurrentPosition)
                {
                    _savePositions.Remove(savePos.Key);
                }
            }
        }

        public bool MoveNext()
        {
            CurrentPosition++;
            return CurrentPosition < _tokens.Length;
        }

        public void Reset()
        {
            CurrentPosition = -1;
        }

        public Token Current => _tokens[CurrentPosition];

        object IEnumerator.Current => Current;
        public void Dispose() { }
    }
}