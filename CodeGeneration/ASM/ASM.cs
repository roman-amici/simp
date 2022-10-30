using Simp.AST;
using Simp.Common;
using Simp.StaticAnalysis;

namespace Simp.CodeGeneration.ASM
{
    public partial class ASMGenerator
    {
        public string FilePath { get; private set; }

        IList<string> Data { get; set; }
        IList<string> BSS { get; set; }
        IList<string> Text { get; set; }

        IList<string> FunctionText { get; set; }

        int Label { get; set; } = 0;

        Dictionary<string, Declaration> GlobalResolver { get; set; } = new Dictionary<string, Declaration>();
        Resolver Resolver { get; set; }
        string FunctionReturnLabel { get; set; } = String.Empty;

        string NextLabel()
        {
            var s = $"l{Label}";
            Label++;
            return s;
        }

        public ASMGenerator(string filePath)
        {
            FilePath = filePath;
            SetupData();
            SetupBSS();
            SetupText();
        }

        void SetupData()
        {
            Data = new List<string>() { "section .data" };
        }

        void SetupBSS()
        {
            BSS = new List<string>() { "section .bss" };
        }

        void SetupText()
        {
            Text = new List<string>()
            {
                "section .text",
                "\t global main",
            };
        }

        public bool Generate(IList<Declaration> declarations)
        {
            try
            {
                foreach (var declaration in declarations)
                {
                    GenDeclaration(declaration);
                }
            }
            catch (TokenError e)
            {
                Console.WriteLine(e);
                return false;
            }

            Save();
            return true;

        }

        void Save()
        {
            using var stream = new StreamWriter(FilePath);
            foreach (var line in Data)
            {
                stream.WriteLine(line);
            }
            foreach (var line in BSS)
            {
                stream.WriteLine(line);
            }
            foreach (var line in Text)
            {
                stream.WriteLine(line);
            }
        }
    }
}