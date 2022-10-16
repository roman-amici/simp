namespace Simp.CodeGeneration
{
    public partial class ASMGenerator
    {
        void Clear(string reg)
        {
            Text.Add($"xor {reg},{reg}");
        }

        void SetCompare(string set)
        {
            Clear("rdx");
            Text.Add("cmp rax, rbx");
            Text.Add($"{set} dl");
            Text.Add("push rdx");
        }
    }
}