using System.Globalization;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace CSharpToSvgTranslator;

public sealed record SvgTranslationOptions(
    float Width = 100,
    float Height = 100,
    string? Title = null,
    string Background = "transparent");

public sealed class MauiCanvasToSvgTranslator
{
    private static readonly Regex FloatAssignmentRegex = new(
        @"^(?:var|float|double|int)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*(?<value>.+?);$",
        RegexOptions.Compiled);

    private static readonly Regex RectAssignmentRegex = new(
        @"^(?:var|RectF)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*new\s+RectF\s*\((?<args>[^)]*)\)\s*;$",
        RegexOptions.Compiled);

    private static readonly Regex ColorAssignmentRegex = new(
        @"^(?:var|Color)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*(?<value>.+?);$",
        RegexOptions.Compiled);

    private static readonly Regex PathAssignmentRegex = new(
        @"^(?:var|PathF)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*new\s*\(?\s*\)?\s*;$",
        RegexOptions.Compiled);

    private readonly Dictionary<string, float> _numbers = new(StringComparer.Ordinal);
    private readonly Dictionary<string, SvgPaint> _colors = new(StringComparer.Ordinal);
    private readonly Dictionary<string, SvgRect> _rects = new(StringComparer.Ordinal);
    private readonly Dictionary<string, SvgPath> _paths = new(StringComparer.Ordinal);
    private readonly Stack<string> _savedTransforms = new();
    private readonly StringBuilder _elements = new();

    private string _fill = "none";
    private float _fillOpacity = 1f;
    private string _stroke = "black";
    private float _strokeOpacity = 1f;
    private float _strokeWidth = 1f;
    private string _transform = string.Empty;

    public string Translate(string csharpSource, SvgTranslationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(csharpSource);
        options ??= new SvgTranslationOptions();

        Reset(options);

        foreach (string rawLine in csharpSource.Replace("\r\n", "\n").Split('\n'))
        {
            string line = StripTrailingComment(rawLine).Trim();
            if (line.Length == 0)
                continue;

            TranslateLine(line);
        }

        return BuildDocument(options);
    }

    public string TranslateFile(string csharpFilePath, SvgTranslationOptions? options = null)
        => Translate(File.ReadAllText(csharpFilePath), options);

    private void Reset(SvgTranslationOptions options)
    {
        _numbers.Clear();
        _colors.Clear();
        _rects.Clear();
        _paths.Clear();
        _savedTransforms.Clear();
        _elements.Clear();

        _numbers["dirtyRect.X"] = 0;
        _numbers["dirtyRect.Y"] = 0;
        _numbers["dirtyRect.Width"] = options.Width;
        _numbers["dirtyRect.Height"] = options.Height;
        _numbers["Width"] = options.Width;
        _numbers["Height"] = options.Height;

        _fill = "none";
        _fillOpacity = 1f;
        _stroke = "black";
        _strokeOpacity = 1f;
        _strokeWidth = 1f;
        _transform = string.Empty;
    }

    private void TranslateLine(string line)
    {
        if (TryReadColorAssignment(line, "FillColor", out string fill, out float fillOpacity))
        {
            _fill = fill;
            _fillOpacity = fillOpacity;
            return;
        }

        if (TryReadColorAssignment(line, "StrokeColor", out string stroke, out float strokeOpacity))
        {
            _stroke = stroke;
            _strokeOpacity = strokeOpacity;
            return;
        }

        if (TryReadStrokeSize(line, out float strokeSize))
        {
            _strokeWidth = strokeSize;
            return;
        }

        Match rectMatch = RectAssignmentRegex.Match(line);
        if (rectMatch.Success && TryReadRect(rectMatch.Groups["args"].Value, out SvgRect rect))
        {
            _rects[rectMatch.Groups["name"].Value] = rect;
            return;
        }

        Match colorMatch = ColorAssignmentRegex.Match(line);
        if (colorMatch.Success && TryReadColor(colorMatch.Groups["value"].Value, out string color, out float opacity))
        {
            _colors[colorMatch.Groups["name"].Value] = new SvgPaint(color, opacity);
            return;
        }

        Match pathMatch = PathAssignmentRegex.Match(line);
        if (pathMatch.Success)
        {
            _paths[pathMatch.Groups["name"].Value] = new SvgPath();
            return;
        }

        if (TryReadPathCommand(line) || TryReadCanvasState(line) || TryReadShapeCommand(line))
            return;

        Match floatMatch = FloatAssignmentRegex.Match(line);
        if (floatMatch.Success && TryEvaluate(floatMatch.Groups["value"].Value, out float value))
            _numbers[floatMatch.Groups["name"].Value] = value;
    }

    private bool TryReadCanvasState(string line)
    {
        if (line is "canvas.SaveState();" or "canvas.SaveState()")
        {
            _savedTransforms.Push(_transform);
            return true;
        }

        if (line is "canvas.RestoreState();" or "canvas.RestoreState()")
        {
            _transform = _savedTransforms.Count > 0 ? _savedTransforms.Pop() : string.Empty;
            return true;
        }

        if (TryReadCall(line, "canvas.Translate", out string translateArgs))
        {
            string[] args = SplitArgs(translateArgs);
            if (args.Length >= 2 && TryEvaluate(args[0], out float x) && TryEvaluate(args[1], out float y))
                AppendTransform($"translate({Number(x)} {Number(y)})");
            return true;
        }

        if (TryReadCall(line, "canvas.Rotate", out string rotateArgs))
        {
            string[] args = SplitArgs(rotateArgs);
            if (args.Length >= 1 && TryEvaluate(args[0], out float angle))
                AppendTransform($"rotate({Number(angle)})");
            return true;
        }

        return false;
    }

    private bool TryReadPathCommand(string line)
    {
        int dot = line.IndexOf('.', StringComparison.Ordinal);
        if (dot <= 0)
            return false;

        string pathName = line[..dot].Trim();
        if (!_paths.TryGetValue(pathName, out SvgPath? path))
            return false;

        if (TryReadCall(line, pathName + ".MoveTo", out string moveArgs))
        {
            string[] args = SplitArgs(moveArgs);
            if (args.Length >= 2 && TryEvaluate(args[0], out float x) && TryEvaluate(args[1], out float y))
                path.Commands.Add($"M {Number(x)} {Number(y)}");
            return true;
        }

        if (TryReadCall(line, pathName + ".LineTo", out string lineArgs))
        {
            string[] args = SplitArgs(lineArgs);
            if (args.Length >= 2 && TryEvaluate(args[0], out float x) && TryEvaluate(args[1], out float y))
                path.Commands.Add($"L {Number(x)} {Number(y)}");
            return true;
        }

        if (line == pathName + ".Close();" || line == pathName + ".Close()")
        {
            path.Commands.Add("Z");
            return true;
        }

        return false;
    }

    private bool TryReadShapeCommand(string line)
    {
        if (TryReadCall(line, "canvas.FillRoundedRectangle", out string fillRoundedArgs))
        {
            AppendRoundedRect(fillRoundedArgs, fill: true, stroke: false);
            return true;
        }

        if (TryReadCall(line, "canvas.DrawRoundedRectangle", out string drawRoundedArgs))
        {
            AppendRoundedRect(drawRoundedArgs, fill: false, stroke: true);
            return true;
        }

        if (TryReadCall(line, "canvas.FillRectangle", out string fillRectArgs))
        {
            AppendRect(fillRectArgs, fill: true, stroke: false);
            return true;
        }

        if (TryReadCall(line, "canvas.DrawRectangle", out string drawRectArgs))
        {
            AppendRect(drawRectArgs, fill: false, stroke: true);
            return true;
        }

        if (TryReadCall(line, "canvas.FillPath", out string fillPathArgs))
        {
            AppendPath(fillPathArgs, fill: true, stroke: false);
            return true;
        }

        if (TryReadCall(line, "canvas.DrawPath", out string drawPathArgs))
        {
            AppendPath(drawPathArgs, fill: false, stroke: true);
            return true;
        }

        return false;
    }

    private void AppendRoundedRect(string argsText, bool fill, bool stroke)
    {
        string[] args = SplitArgs(argsText);
        if (!TryResolveRect(args, out SvgRect rect, out int nextArg))
            return;

        float radius = 0;
        if (args.Length > nextArg)
            TryEvaluate(args[nextArg], out radius);

        AppendElement(
            $"<rect x=\"{Number(rect.X)}\" y=\"{Number(rect.Y)}\" width=\"{Number(rect.Width)}\" height=\"{Number(rect.Height)}\" rx=\"{Number(radius)}\" ry=\"{Number(radius)}\"{Style(fill, stroke)}{Transform()} />");
    }

    private void AppendRect(string argsText, bool fill, bool stroke)
    {
        string[] args = SplitArgs(argsText);
        if (!TryResolveRect(args, out SvgRect rect, out _))
            return;

        AppendElement(
            $"<rect x=\"{Number(rect.X)}\" y=\"{Number(rect.Y)}\" width=\"{Number(rect.Width)}\" height=\"{Number(rect.Height)}\"{Style(fill, stroke)}{Transform()} />");
    }

    private void AppendPath(string argsText, bool fill, bool stroke)
    {
        string pathName = argsText.Trim();
        if (!_paths.TryGetValue(pathName, out SvgPath? path) || path.Commands.Count == 0)
            return;

        AppendElement(
            $"<path d=\"{SecurityElement.Escape(string.Join(" ", path.Commands))}\"{Style(fill, stroke)}{Transform()} />");
    }

    private bool TryResolveRect(string[] args, out SvgRect rect, out int nextArg)
    {
        rect = default;
        nextArg = 0;

        if (args.Length > 0 && _rects.TryGetValue(args[0].Trim(), out rect))
        {
            nextArg = 1;
            return true;
        }

        if (args.Length >= 4
            && TryEvaluate(args[0], out float x)
            && TryEvaluate(args[1], out float y)
            && TryEvaluate(args[2], out float width)
            && TryEvaluate(args[3], out float height))
        {
            rect = new SvgRect(x, y, width, height);
            nextArg = 4;
            return true;
        }

        return false;
    }

    private bool TryReadRect(string argsText, out SvgRect rect)
    {
        rect = default;
        string[] args = SplitArgs(argsText);
        if (args.Length < 4)
            return false;

        if (!TryEvaluate(args[0], out float x)
            || !TryEvaluate(args[1], out float y)
            || !TryEvaluate(args[2], out float width)
            || !TryEvaluate(args[3], out float height))
        {
            return false;
        }

        rect = new SvgRect(x, y, width, height);
        return true;
    }

    private bool TryReadColorAssignment(string line, string property, out string color, out float opacity)
    {
        color = "none";
        opacity = 1f;

        string prefix = "canvas." + property;
        if (!line.StartsWith(prefix, StringComparison.Ordinal))
            return false;

        int equals = line.IndexOf('=', StringComparison.Ordinal);
        if (equals < 0)
            return false;

        string expression = line[(equals + 1)..].Trim().TrimEnd(';').Trim();
        return TryReadColor(expression, out color, out opacity);
    }

    private bool TryReadColor(string expression, out string color, out float opacity)
    {
        color = "none";
        opacity = 1f;

        if (_colors.TryGetValue(expression.Trim(), out SvgPaint paint))
        {
            color = paint.Color;
            opacity = paint.Opacity;
            return true;
        }

        Match colorsMatch = Regex.Match(
            expression,
            @"Colors\.(?<name>[A-Za-z0-9_]+)(?:\.WithAlpha\((?<alpha>[^)]*)\))?");
        if (colorsMatch.Success)
        {
            color = ColorNameToSvg(colorsMatch.Groups["name"].Value);
            if (colorsMatch.Groups["alpha"].Success && TryEvaluate(colorsMatch.Groups["alpha"].Value, out float alpha))
                opacity = Math.Clamp(alpha, 0f, 1f);

            return true;
        }

        Match hexMatch = Regex.Match(
            expression,
            @"Color\.FromArgb\(""(?<hex>#[A-Fa-f0-9]{6,8})""\)");
        if (hexMatch.Success)
        {
            color = hexMatch.Groups["hex"].Value[..7];
            return true;
        }

        return false;
    }

    private bool TryReadStrokeSize(string line, out float strokeSize)
    {
        strokeSize = 1;
        const string prefix = "canvas.StrokeSize";
        if (!line.StartsWith(prefix, StringComparison.Ordinal))
            return false;

        int equals = line.IndexOf('=', StringComparison.Ordinal);
        return equals >= 0 && TryEvaluate(line[(equals + 1)..].Trim().TrimEnd(';'), out strokeSize);
    }

    private bool TryEvaluate(string expression, out float value)
    {
        value = 0;
        try
        {
            value = new ExpressionReader(expression, _numbers).Parse();
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static string[] SplitArgs(string argsText)
    {
        List<string> args = new();
        int depth = 0;
        int start = 0;

        for (int i = 0; i < argsText.Length; i++)
        {
            char ch = argsText[i];
            if (ch == '(')
                depth++;
            else if (ch == ')')
                depth--;
            else if (ch == ',' && depth == 0)
            {
                args.Add(argsText[start..i].Trim());
                start = i + 1;
            }
        }

        string last = argsText[start..].Trim();
        if (last.Length > 0)
            args.Add(last);

        return args.ToArray();
    }

    private static bool TryReadCall(string line, string methodName, out string args)
    {
        args = string.Empty;
        if (!line.StartsWith(methodName, StringComparison.Ordinal))
            return false;

        int open = line.IndexOf('(', methodName.Length);
        int close = line.LastIndexOf(')');
        if (open < 0 || close < open)
            return false;

        args = line[(open + 1)..close];
        return true;
    }

    private void AppendTransform(string transform)
    {
        _transform = string.IsNullOrWhiteSpace(_transform)
            ? transform
            : _transform + " " + transform;
    }

    private string Style(bool fill, bool stroke)
    {
        string fillStyle = fill ? _fill : "none";
        string strokeStyle = stroke ? _stroke : "none";
        string strokeWidth = stroke ? Number(_strokeWidth) : "0";

        string style = $" fill=\"{fillStyle}\" fill-opacity=\"{Number(_fillOpacity)}\" stroke=\"{strokeStyle}\" stroke-opacity=\"{Number(_strokeOpacity)}\" stroke-width=\"{strokeWidth}\"";
        if (stroke)
            style += " stroke-linejoin=\"round\" stroke-linecap=\"round\"";

        return style;
    }

    private string Transform()
        => string.IsNullOrWhiteSpace(_transform) ? string.Empty : $" transform=\"{_transform}\"";

    private void AppendElement(string element)
        => _elements.Append("  ").AppendLine(element);

    private string BuildDocument(SvgTranslationOptions options)
    {
        StringBuilder svg = new();
        svg.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        svg.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 ")
            .Append(Number(options.Width))
            .Append(' ')
            .Append(Number(options.Height))
            .Append("\" width=\"")
            .Append(Number(options.Width))
            .Append("\" height=\"")
            .Append(Number(options.Height))
            .AppendLine("\">");

        if (!string.IsNullOrWhiteSpace(options.Title))
            svg.Append("  <title>").Append(SecurityElement.Escape(options.Title)).AppendLine("</title>");

        if (!string.Equals(options.Background, "transparent", StringComparison.OrdinalIgnoreCase))
        {
            svg.Append("  <rect width=\"100%\" height=\"100%\" fill=\"")
                .Append(SecurityElement.Escape(options.Background))
                .AppendLine("\" />");
        }

        svg.Append(_elements);
        svg.AppendLine("</svg>");
        return svg.ToString();
    }

    private static string StripTrailingComment(string line)
    {
        int comment = line.IndexOf("//", StringComparison.Ordinal);
        return comment >= 0 ? line[..comment] : line;
    }

    private static string ColorNameToSvg(string colorName)
        => colorName switch
        {
            "Transparent" => "none",
            "SandyBrown" => "sandybrown",
            "DarkOrange" => "darkorange",
            _ => colorName.ToLowerInvariant()
        };

    private static string Number(float value)
        => value.ToString("0.###", CultureInfo.InvariantCulture);

    private readonly record struct SvgRect(float X, float Y, float Width, float Height);

    private readonly record struct SvgPaint(string Color, float Opacity);

    private sealed class SvgPath
    {
        public List<string> Commands { get; } = new();
    }

    private sealed class ExpressionReader
    {
        private readonly string _expression;
        private readonly IReadOnlyDictionary<string, float> _numbers;
        private int _position;

        public ExpressionReader(string expression, IReadOnlyDictionary<string, float> numbers)
        {
            _expression = Regex.Replace(
                    expression
                        .Replace("MathF.", string.Empty, StringComparison.Ordinal)
                        .Replace("Math.", string.Empty, StringComparison.Ordinal),
                    @"(?<=\d)f\b",
                    string.Empty,
                    RegexOptions.IgnoreCase)
                .Trim();
            _numbers = numbers;
        }

        public float Parse()
        {
            float value = ParseAdditive();
            SkipWhitespace();
            if (_position != _expression.Length)
                throw new FormatException("Unexpected expression tail.");
            return value;
        }

        private float ParseAdditive()
        {
            float value = ParseMultiplicative();
            while (true)
            {
                SkipWhitespace();
                if (TryConsume('+'))
                    value += ParseMultiplicative();
                else if (TryConsume('-'))
                    value -= ParseMultiplicative();
                else
                    return value;
            }
        }

        private float ParseMultiplicative()
        {
            float value = ParseUnary();
            while (true)
            {
                SkipWhitespace();
                if (TryConsume('*'))
                    value *= ParseUnary();
                else if (TryConsume('/'))
                    value /= ParseUnary();
                else
                    return value;
            }
        }

        private float ParseUnary()
        {
            SkipWhitespace();
            if (TryConsume('+'))
                return ParseUnary();
            if (TryConsume('-'))
                return -ParseUnary();
            return ParsePrimary();
        }

        private float ParsePrimary()
        {
            SkipWhitespace();
            if (TryConsume('('))
            {
                float parenthesized = ParseAdditive();
                if (!TryConsume(')'))
                    throw new FormatException("Missing closing parenthesis.");
                return parenthesized;
            }

            if (_position >= _expression.Length)
                throw new FormatException("Unexpected end of expression.");

            if (char.IsDigit(_expression[_position]) || _expression[_position] == '.')
                return ParseNumber();

            string identifier = ParseIdentifier();
            if (_numbers.TryGetValue(identifier, out float value))
                return value;

            throw new InvalidOperationException("Unknown identifier: " + identifier);
        }

        private float ParseNumber()
        {
            int start = _position;
            while (_position < _expression.Length
                && (char.IsDigit(_expression[_position]) || _expression[_position] == '.'))
            {
                _position++;
            }

            string text = _expression[start.._position];
            return float.Parse(text, CultureInfo.InvariantCulture);
        }

        private string ParseIdentifier()
        {
            int start = _position;
            while (_position < _expression.Length)
            {
                char ch = _expression[_position];
                if (!char.IsLetterOrDigit(ch) && ch != '_' && ch != '.')
                    break;
                _position++;
            }

            if (start == _position)
                throw new FormatException("Expected identifier.");

            return _expression[start.._position];
        }

        private bool TryConsume(char expected)
        {
            SkipWhitespace();
            if (_position >= _expression.Length || _expression[_position] != expected)
                return false;

            _position++;
            return true;
        }

        private void SkipWhitespace()
        {
            while (_position < _expression.Length && char.IsWhiteSpace(_expression[_position]))
                _position++;
        }
    }
}
