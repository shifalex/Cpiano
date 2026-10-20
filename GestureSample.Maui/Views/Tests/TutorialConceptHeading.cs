using System.Diagnostics;
using GestureSample.Maui.Handlers;

namespace GestureSample.Views.Tests;

/// <summary>A visible concept cue with cancellable narration in the user's voice language.</summary>
internal sealed class TutorialConceptHeading : Label
{
    private string _source = "Segment length";
    private CancellationTokenSource? _speech;

    public TutorialConceptHeading()
    {
        FontSize = 40;
        FontAttributes = FontAttributes.Bold;
        TextColor = Colors.White;
        HorizontalTextAlignment = TextAlignment.Center;
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await SpeakAsync();
        GestureRecognizers.Add(tap);
    }

    public void SetConcept(string source)
    {
        _source = source;
        // These tutorial pages use Hebrew copy by default, including their headings.
        var language = LanguagePreferences.Get(gripping: true);
        Text = AppLanguage.Translate(source, language == InterfaceLanguage.English ? InterfaceLanguage.Hebrew : language);
        FlowDirection = LanguagePreferences.Get(gripping: true) == InterfaceLanguage.Hebrew
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    public async Task SpeakAsync(CancellationToken cancellationToken = default)
    {
        CancelSpeech();
        using var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _speech = source;
        try
        {
            if (!await AppLanguage.SpeakAsync(_source, gripping: true, cancellationToken: source.Token))
                Debug.WriteLine("[Tutorial title] No voice available for the selected narration language.");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            // An unavailable platform speech service must not block the lesson.
            Debug.WriteLine($"[Tutorial title] Narration unavailable: {ex.Message}");
        }
        finally { if (ReferenceEquals(_speech, source)) _speech = null; }
    }

    public void CancelSpeech() => _speech?.Cancel();
}
