// See https://aka.ms/new-console-template for more information

using Simp.CodeGeneration;
using Simp.Parser;
using Simp.Scanner;

if (args.Length < 1)
{
    Console.WriteLine("Usage [file]");
    return;
}

var filename = args[0];

var contents = File.ReadAllText(filename);

var scanner = new BasicScanner();
var tokens = scanner.ScanFile(contents, filename);

if (scanner.IsValid)
{
    var parser = new RecursiveDescentParser(tokens);
    var statements = parser.Parse();
    if (parser.IsValid)
    {
        var generator = new ASMGenerator($"example_asm/test.asm");
        generator.GenFunction(statements);
        generator.Save();
    }
}


