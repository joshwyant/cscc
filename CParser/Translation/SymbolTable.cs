using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace cscc.Translation
{
    class SymbolTable : IDictionary<string, Symbol>
    {
        public SymbolTable? Parent { get; }
        protected Dictionary<string, Symbol> Symbols { get; }

        public ICollection<string> Keys => Enumerate().Select(kvp => kvp.Key).ToList();

        public ICollection<Symbol> Values => Enumerate().Select(kvp => kvp.Value).ToList();

        public int Count => Enumerate().Count();

        public bool IsReadOnly => false;

        public Symbol this[string key] { get => Symbols.ContainsKey(key) ? Symbols[key] : Parent?[key]!; set => Symbols[key] = value; }

        public SymbolTable(SymbolTable? parent = null)
        {
            Parent = parent;
            Symbols = new Dictionary<string, Symbol>();
        }

        public bool ContainsKey(string key)
        {
            return Symbols.ContainsKey(key) || (Parent?.ContainsKey(key) ?? false);
        }

        public bool Remove(string key)
        {
            if (Symbols.Remove(key))
            {
                return true;
            }
            return Parent?.Remove(key) ?? false;
        }

        #nullable disable // workaround for https://github.com/dotnet/roslyn/issues/39681
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out Symbol value)
        {
            return (Symbols.TryGetValue(key, out value)
                || (Parent?.TryGetValue(key, out value) ?? false));
        }
        #nullable enable

        public void Add(KeyValuePair<string, Symbol> item)
        {
            Symbols.Add(item.Key, item.Value);
        }

        public void Add(string key, Symbol value)
        {
            Symbols.Add(key, value);
        }

        public void Clear()
        {
            Symbols.Clear();
        }

        public bool Contains(KeyValuePair<string, Symbol> item)
        {
            return Symbols.Contains(item) || (Parent?.Contains(item) ?? false);
        }

        public void CopyTo(KeyValuePair<string, Symbol>[] array, int arrayIndex)
        {
            foreach (var item in Enumerate())
            {
                array[arrayIndex++] = item;
            }
        }

        public bool Remove(KeyValuePair<string, Symbol> item)
        {
            return Remove(item.Key);
        }

        protected IEnumerable<KeyValuePair<string, Symbol>> Enumerate()
        {
            var visited = new HashSet<string>();
            for (SymbolTable? table = this; table != null; table = table.Parent)
            {
                foreach (var kvp in table.Symbols)
                {
                    if (visited.Add(kvp.Key))
                    {
                        yield return kvp;
                    }
                }
            }
        }

        public IEnumerator<KeyValuePair<string, Symbol>> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, Symbol>>)this).GetEnumerator();
        }
    }
}