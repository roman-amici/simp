using Simp.AST;
using Simp.StaticAnalysis;

namespace Simp.CodeGeneration
{
    public partial class ASMGenerator
    {
        public string FilePath { get; private set; }

        IList<string> Data { get; set; }
        IList<string> BSS { get; set; }
        IList<string> Text { get; set; }

        IList<string> CurrentFunction { get; set; }

        int Label { get; set; } = 0;

        Resolver Resolver { get; set; }

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

        void EnterFunction()
        {
            CurrentFunction = new List<string>();
            Resolver = new Resolver();
        }

        void ExitFunction()
        {
            var offset = Resolver.MaxSlots * 8;

            // Function preamble
            Text.Add("push rbp");
            Text.Add("mov rbp, rsp");
            Text.Add($"sub rsp, ${offset}");

            foreach (var line in CurrentFunction)
            {
                Text.Add(line);
            }

            // TODO: ret;
        }

        public void GenFunction(IList<Statement> statements)
        {
            // TODO: named function, arguments etc;
            EnterFunction();
            GenStatements(statements);
            ExitFunction();
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
                "main:"
            };
        }

        public void Save()
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