﻿using System;
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

        public string CreateSavePoint(string name)
        {
            _savePositions.Add(name, CurrentPosition);
            return name;
        }

        public string CreateSavePoint()
        {
            return CreateSavePoint(Guid.NewGuid().ToString());
        }

        public void RevertToSavePoint(string name)
        {
            CurrentPosition = _savePositions[name];
            _savePositions.Remove(name);

            var avePositionsGreaterThanCurrent = _savePositions
                .Where(p => p.Value > CurrentPosition)
                .Select(p => p.Key)
                .ToList();

            foreach (var posName in avePositionsGreaterThanCurrent)
            {
                _savePositions.Remove(posName);
            }
        }

        public bool HasNext => (CurrentPosition + 1) < _tokens.Length;

        public Token Peek() => _tokens[CurrentPosition + 1];

        public bool MoveNext()
        {
            CurrentPosition++;
            return CurrentPosition < _tokens.Length;
        }

        public void Reset()
        {
            CurrentPosition = -1;
        }

        public Token Current
        {
            get
            {
                if (CurrentPosition >= _tokens.Length)
                    throw new OutOfTokensException();

                return _tokens[CurrentPosition];
            }
        }

        object IEnumerator.Current => Current;
        public void Dispose() { }
    }
    
}