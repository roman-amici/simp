using System.Linq;

namespace Simp.StaticAnalysis
{
    public class MemResolver
    {
        List<Dictionary<string, string>> Scopes = new();

        Dictionary<string, string> CurrentScope => Scopes[^1];

        int FunctionArgumentIndex { get; set; }
        int VariableIndex { get; set; }

        public void EnterScope()
        {
            Scopes.Add(new Dictionary<string, string>());
        }

        public void ExitScope()
        {
            Scopes.RemoveAt(Scopes.Count - 1);
        }

        public string? DeclareVariable(string variable)
        {
            if (CurrentScope.ContainsKey(variable))
            {
                return null;
            }

            CurrentScope[variable] = $"%{variable}{VariableIndex++}";
            return CurrentScope[variable];
        }

        public string? DeclareFunctionArgument(string argument)
        {
            if (CurrentScope.ContainsKey(argument))
            {
                return null;
            }

            var idx = $"%{FunctionArgumentIndex++}";
            CurrentScope[argument] = idx;
            return idx;
        }

        public string? Lookup(string name)
        {
            foreach (var dict in Scopes.AsEnumerable().Reverse())
            {
                if (dict.ContainsKey(name))
                {
                    return dict[name];
                }
            }

            return null;
        }
    }
}