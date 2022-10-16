using Simp.AST;

namespace Simp.CodeGeneration
{
    public partial class ASMGenerator
    {
        public string FilePath { get; private set; }

        IList<string> Data { get; set; }
        IList<string> BSS { get; set; }
        IList<string> Text { get; set; }

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