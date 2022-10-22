namespace Simp.StaticAnalysis
{
    public class Resolver
    {
        int _slot = 0;
        public int Slot
        {
            get => _slot;
            set
            {
                _slot = value;
                MaxSlots = Math.Max(_slot, MaxSlots);
            }
        }
        public int MaxSlots { get; private set; }
        public Scope CurrentScope { get; private set; } = new Scope();
        public int FunctionArgumentSlot { get; private set; } = -2; // Skip over rbp and ret value

        public Resolver()
        {
            Slot = 1; // Slot 0 is the base pointer
        }

        public int? FindSlot(string variable)
        {
            var scope = CurrentScope;
            while (scope is not null)
            {
                if (scope.VariableMapping.ContainsKey(variable))
                {
                    return scope.VariableMapping[variable];
                }
                else
                {
                    scope = scope.EnclosingScope;
                }
            }

            return null;
        }

        public int? DeclareVariable(string variable)
        {
            if (CurrentScope.VariableMapping.ContainsKey(variable))
            {
                return null;
            }

            CurrentScope.VariableMapping[variable] = Slot;
            return Slot++;
        }

        public int? DeclareFunctionArgument(string variable)
        {
            if (CurrentScope.VariableMapping.ContainsKey(variable))
            {
                return null;
            }

            CurrentScope.VariableMapping[variable] = FunctionArgumentSlot;
            return FunctionArgumentSlot--;
        }

        public (int?, int) DeclareArray(string variable, int size)
        {
            var variableLocation = DeclareVariable(variable);
            if (variableLocation is not null)
            {
                Slot += size;
            }

            return (variableLocation, Slot);
        }

        public void EnterScope()
        {
            var newScope = new Scope
            {
                EnclosingScope = CurrentScope
            };
            CurrentScope = newScope;
        }

        public void ExitScope()
        {
            Slot -= CurrentScope.VariableMapping.Count;
            CurrentScope = CurrentScope.EnclosingScope;
        }
    }

    public class Scope
    {
        public Dictionary<string, int> VariableMapping { get; private set; }
            = new Dictionary<string, int>();

        public Scope? EnclosingScope { get; set; }

    }
}