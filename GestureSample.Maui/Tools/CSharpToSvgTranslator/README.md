# CSharpToSvgTranslator

Standalone CLI utility for translating a focused `Microsoft.Maui.Graphics.ICanvas` drawing subset from C# source into SVG.

## Usage

```powershell
dotnet run --project Tools\CSharpToSvgTranslator -- Views\Tests\SomeDrawable.cs output.svg --width 300 --height 200 --title "Preview"
```

If `output.svg` is omitted, the utility writes next to the input file with the `.svg` extension.

## Supported Drawing Subset

- `canvas.FillColor`, `canvas.StrokeColor`, `canvas.StrokeSize`
- `Colors.Name`, `Colors.Name.WithAlpha(...)`, and simple color variables
- `canvas.FillRectangle(...)`, `canvas.DrawRectangle(...)`
- `canvas.FillRoundedRectangle(...)`, `canvas.DrawRoundedRectangle(...)`
- `PathF` with `MoveTo`, `LineTo`, `Close`, then `canvas.FillPath(...)` or `canvas.DrawPath(...)`
- `canvas.SaveState()`, `canvas.RestoreState()`, `canvas.Translate(...)`, `canvas.Rotate(...)`
- Simple numeric expressions using literals, variables, `+`, `-`, `*`, `/`, and parentheses

This is intentionally not a general-purpose C# executor. It translates recognizable drawing statements safely from source text and ignores unsupported code.
