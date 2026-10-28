using GestureSample.Maui.Models;

namespace GestureSample.Views.Tests;

public partial class SimpleViewCellsPage
{
    public void PreserveXorQuestionFrom(SimpleViewCellsPage previous)
    {
        if (_gamePlay is BitArrayGamePlay next && previous._gamePlay is BitArrayGamePlay current)
            next.PreserveNextXorQuestion(current.BitArrayQuestion, current.BitArrayQuestion2);
    }

    public async Task<bool> SetXorPracticeModeAsync(bool singleDifference, bool specialColors, bool completedTransition = false)
    {
        if ((_exerciseFeedbackInProgress || _exerciseGenerationInProgress || _exerciseNavigationBusy) &&
            !(completedTransition && _currentExerciseCompleted)) return false;
        if (_gamePlay is not BitArrayGamePlay game) return false;
        if (_exerciseReviewHistory.IsReviewing) ReturnToLiveExercise();
        _config.XorSingleDifferenceOnly = singleDifference;
        _config.IsSpecialColor = specialColors;
        _config.MaxSum = singleDifference ? 1 : 10;
        bool alreadySingle = game.BitArrayQuestion.Zip(game.BitArrayQuestion2, (a, b) => a != b).Count(different => different) == 1;
        if (singleDifference && !alreadySingle)
        {
            // Entering stage zero may need a simpler question, but retain one source.
            game.PreserveNextXorQuestion(game.BitArrayQuestion, game.BitArrayQuestion2);
            _pendingManualAdvance = null;
            await GenerateNextExerciseAsync();
        }
        else
        {
            // Presentation only: do not evaluate, regenerate, or clear the answer.
            if (_config.UsesCombinedLogicalKeyboard)
                _keyboardTask1.PianoInit(game.BitArrayQuestion.Concat(game.BitArrayQuestion2).ToArray());
            else
            {
                _keyboardTask1.PianoInit(game.BitArrayQuestion);
                _keyboardTask2.PianoInit(game.BitArrayQuestion2);
            }
            ApplyXorQuestionPalette();
        }
        return true;
    }

    private void ApplyXorQuestionPalette()
    {
        if (!_config.UsesSpecialLogicalKeyboardColors) return;
        _keyboardTask1.SpecialColors();
        if (!_config.UsesCombinedLogicalKeyboard) _keyboardTask2.SpecialColors();
    }
}
