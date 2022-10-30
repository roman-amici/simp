// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Simp.AST;
using Simp.CodeGeneration.ASM;
using Simp.Parser;
using Simp.Scanner;

void RunASM(IList<Declaration> declarations, string asmFile)
{
    asmFile ??= Path.GetTempFileName();
    var generator = new ASMGenerator(asmFile);
    if (!generator.Generate(declarations))
    {
        Environment.Exit(3);
    }

    var listFile = Path.GetTempFileName();
    var outFile = Path.GetTempFileName();
    var proc = Process.Start(
        new ProcessStartInfo()
        {
            FileName = "nasm",
            Arguments = $"-f elf64 -g -F dwarf {asmFile} -l {listFile} -o {outFile}",
            RedirectStandardOutput = true,
        });

    proc?.WaitForExit();
    if (proc?.ExitCode != 0)
    {
        Console.WriteLine("An error occurred generating assembly");
        Environment.Exit(1);
    }

    var outputFile = Path.GetTempFileName();
    proc = Process.Start(
        new ProcessStartInfo()
        {
            FileName = "gcc",
            Arguments = $"-o {outputFile} {outFile} -no-pie",
            RedirectStandardOutput = true
        }
    );

    proc?.WaitForExit();
    if (proc?.ExitCode != 0)
    {
        Console.WriteLine("An error occurred linking program");
        Environment.Exit(2);
    }

    proc = Process.Start(
        new ProcessStartInfo()
        {
            FileName = outputFile,
            RedirectStandardOutput = true
        }
    );

    proc?.WaitForExit();
    Console.WriteLine($"Code: {proc?.ExitCode}");
}

void RunLLVM(IList<Declaration> declarations, string llvmFile)
{
    var builder = new Simp.CodeGeneration.LLVM.Builder();
    builder.Build(declarations);
    builder.Generate(llvmFile);
}

if (args.Length < 1)
{
    Console.WriteLine("Usage [file]");
    return;
}

var filename = args[0];

string irFile = null;
if (args.Length > 1)
{
    irFile = args[1];
}

var contents = File.ReadAllText(filename);

var scanner = new BasicScanner();
var tokens = scanner.ScanFile(contents, filename);

if (scanner.IsValid)
{
    var parser = new RecursiveDescentParser(tokens);
    var declarations = parser.Parse();
    if (parser.IsValid)
    {
        RunLLVM(declarations, irFile);
    }
}


