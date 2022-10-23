using System.Text.RegularExpressions;

namespace Simp.CodeGeneration
{
    public partial class ASMGenerator
    {
        static Regex PushNumber = new Regex(@"push (\-)?[0-9]+", RegexOptions.Compiled);
        static Regex Number = new Regex(@"(\-)?[0-9]+", RegexOptions.Compiled);

        void Clear(string reg)
        {
            Add($"xor {reg},{reg}");
        }

        void SetCompare(string set)
        {
            Clear("rdx");
            Add("cmp rax, rbx");
            Add($"{set} dl");
            Add("push rdx");
        }

        void Add(string value)
        {
            FunctionText.Add(value);
        }

        void AddLabel(string value)
        {
            Add($"{value}:");
        }

        void Pop(string register)
        {
            // Replace push+pop of literal with mov
            if (PushNumber.Match(FunctionText[^1]).Success)
            {
                var number = Number.Match(FunctionText[^1]).Value;
                FunctionText.RemoveAt(FunctionText.Count - 1);
                Add($"mov {register}, {number}");

            }
            // Replace push+pop of register entirely
            else if (FunctionText[^1] == $"push {register}")
            {
                FunctionText.RemoveAt(FunctionText.Count - 1);
            }
            else
            {
                Add($"pop {register}");
            }
        }
    }
}