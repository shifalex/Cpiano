using CSharpToSvgTranslator;

if (args.Length == 0 || args.Contains("--help"))
{
    PrintHelp();
    return args.Length == 0 ? 1 : 0;
}

string inputPath = args[0];
string outputPath = args.Length > 1 && !args[1].StartsWith("--", StringComparison.Ordinal)
    ? args[1]
    : Path.ChangeExtension(inputPath, ".svg");

float width = ReadFloatOption(args, "--width", 100);
float height = ReadFloatOption(args, "--height", 100);
string? title = ReadStringOption(args, "--title");
string background = ReadStringOption(args, "--background") ?? "transparent";

if (!File.Exists(inputPath))
{
    Console.Error.WriteLine($"Input file not found: {inputPath}");
    return 2;
}

var translator = new MauiCanvasToSvgTranslator();
string svg = translator.TranslateFile(
    inputPath,
    new SvgTranslationOptions(width, height, title, background));

Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? ".");
File.WriteAllText(outputPath, svg);
Console.WriteLine(outputPath);
return 0;

static float ReadFloatOption(string[] args, string name, float fallback)
{
    string? value = ReadStringOption(args, name);
    return float.TryParse(value, out float parsed) ? parsed : fallback;
}

static string? ReadStringOption(string[] args, string name)
{
    int index = Array.IndexOf(args, name);
    if (index < 0 || index + 1 >= args.Length)
        return null;

    return args[index + 1];
}

static void PrintHelp()
{
    Console.WriteLine("""
    CSharpToSvgTranslator

    Usage:
      CSharpToSvgTranslator <input.cs> [output.svg] [--width 300] [--height 200] [--title "Preview"] [--background white]

    Supports a focused Microsoft.Maui.Graphics ICanvas subset:
      canvas.FillColor / StrokeColor / StrokeSize
      canvas.FillRectangle / DrawRectangle
      canvas.FillRoundedRectangle / DrawRoundedRectangle
      PathF MoveTo / LineTo / Close with canvas.FillPath / DrawPath
      canvas.SaveState / RestoreState / Translate / Rotate
    """);
}
