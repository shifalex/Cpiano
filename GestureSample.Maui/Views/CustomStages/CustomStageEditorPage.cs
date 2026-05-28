using GestureSample.Maui.Data;
using GestureSample.Maui.Data.SQLite;
using GestureSample.Maui.Models.CustomStages;
using GestureSample.Maui.Handlers;
using GestureSample.Views.Tests;

namespace GestureSample.Maui.Views.CustomStages
{
    public class CustomStageEditorPage : ContentPage
    {
        private sealed class OptionItem<T>
        {
            public string Label { get; init; } = string.Empty;
            public T Value { get; init; } = default!;
            public override string ToString() => Label;
        }

        private enum ArrowPromptFamily
        {
            OnKeyboardOnly,
            SpecialOnly,
            Mixed
        }

        private enum ArrowRouteFamily
        {
            CardinalOnly,
            OrdinalOnly,
            Mixed
        }

        private readonly CustomStageKind _kind;
        private readonly CustomStageDefinitionRepository _stageRepository;
        private readonly Entry _nameEntry = CreateEntry("Stage name");
        private readonly Entry _minAddendEntry = CreateNumericEntry();
        private readonly Entry _maxAddendEntry = CreateNumericEntry();
        private readonly Entry _minSumEntry = CreateNumericEntry();
        private readonly Entry _maxSumEntry = CreateNumericEntry();
        private readonly Entry _weightsEntry = CreateEntry("e.g. 10,10,10,10,50,5,1,1,1,1");
        private readonly Entry _tasksToWinEntry = CreateNumericEntry();
        private readonly Entry _mistakesToLoseEntry = CreateNumericEntry();
        private readonly Entry _keysInRowEntry = CreateNumericEntry();
        private readonly Entry _secondsToAnswerEntry = CreateNumericEntry();
        private readonly Picker _uiQuestionPicker = new();
        private readonly Picker _variableTypesPicker = new();
        private readonly Picker _numericInputPicker = new();
        private readonly Picker _syncTypePicker = new();
        private readonly Picker _arrowFeedbackPicker = new();
        private readonly Picker _arrowDirectionPicker = new();
        private readonly Picker _arrowPromptFamilyPicker = new();
        private readonly Picker _arrowRouteFamilyPicker = new();
        private readonly VerticalStackLayout _operationSelectionLayout = new() { Spacing = 6 };
        private readonly Switch _showPrevSwitch = new();
        private readonly Switch _onlyCloseTriadSwitch = new();
        private readonly Switch _keyboardHelpSwitch = new();
        private readonly Switch _allowImpossibleWeightedAnswerSwitch = new();
        private readonly Switch _onlyToTenSwitch = new();
        private readonly Switch _onlyThroughTenSwitch = new();
        private readonly Switch _dynamicArrowLengthSwitch = new();
        private readonly Switch _showNumbersSwitch = new();
        private readonly Switch _arrowMissingStartSwitch = new();
        private readonly Switch _arrowMissingLengthSwitch = new();
        private readonly Switch _arrowMissingEndSwitch = new();
        private readonly Switch _twoKeyboardsSwitch = new();
        private readonly Switch _onlyOneHandSwitch = new();
        private readonly Switch _isHelpNeededSwitch = new();
        private readonly Switch _allowOverlapSwitch = new();
        private readonly Switch _allowStrangeSwitch = new();
        private readonly Switch _allowInsideSwitch = new();
        private readonly Switch _allowSameSwitch = new();
        private readonly Switch _allowEmptySwitch = new();
        private readonly VerticalStackLayout _savedStagesLayout = new() { Spacing = 8 };
        private readonly Button _saveButton;
        private readonly Dictionary<Operation, Switch> _operationSwitches = new();
        private GameConfig? _loadedConfigSnapshot;
        private Guid? _editingId;
        private int[]? _suggestedWeightedStageWeights;

        public CustomStageEditorPage(CustomStageKind kind)
        {
            _kind = kind;
            _stageRepository = ServiceHelper.GetService<CustomStageDefinitionRepository>();
            Title = $"{CustomStageCatalog.GetDisplayName(kind)} Stage Builder";
            BackgroundColor = Colors.Beige;

            ConfigurePickers();
            _weightsEntry.Unfocused += (_, __) => TryApplySuggestedWeightedRange(forceApply: false);

            _saveButton = new Button { Text = "Save Stage", BackgroundColor = Colors.MediumPurple, TextColor = Colors.White };
            _saveButton.Clicked += async (_, __) => await SaveStageAsync();

            Button playButton = new() { Text = "Play Current", BackgroundColor = Colors.ForestGreen, TextColor = Colors.White };
            playButton.Clicked += async (_, __) => await PlayCurrentAsync();

            Button clearButton = new() { Text = "Clear Form", BackgroundColor = Colors.Gray, TextColor = Colors.White };
            clearButton.Clicked += (_, __) => LoadDefinitionIntoForm(null);

            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = new Thickness(16, 12),
                    Spacing = 12,
                    Children =
                    {
                        CreateSectionTitle($"{CustomStageCatalog.GetDisplayName(kind)} custom stages"),
                        CreateLabeledField("Name", _nameEntry),
                        BuildKindSpecificForm(),
                        CreateLabeledField("Tasks to win", _tasksToWinEntry),
                        CreateLabeledField("Mistakes to lose", _mistakesToLoseEntry),
                        new HorizontalStackLayout { Spacing = 8, Children = { _saveButton, playButton, clearButton } },
                        CreateSectionTitle("Saved stages"),
                        _savedStagesLayout
                    }
                }
            };

            LoadDefinitionIntoForm(null);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RefreshSavedStagesAsync();
        }

        private void ConfigurePickers()
        {
            BuildOperationSelection();
            _uiQuestionPicker.ItemsSource = CreateOptions(
                new[] { UIQuestionType.OneText, UIQuestionType.ThreeTexts, UIQuestionType.ThreeAddends, UIQuestionType.SimpleEquation, UIQuestionType.TwoLinesTwoAddends },
                value => value.ToString());
            _variableTypesPicker.ItemsSource = CreateOptions(
                new[] { VariableTypes.OneCanBeSum, VariableTypes.OneNoSum, VariableTypes.SumOnly, VariableTypes.TwoNoSum, VariableTypes.Three },
                value => value.ToString());
            _numericInputPicker.ItemsSource = CreateOptions(
                new[] { NumericInputMode.AppKeypad, NumericInputMode.SystemKeyboard, NumericInputMode.Auto },
                value => value.ToString());
            _syncTypePicker.ItemsSource = CreateOptions(
                new[] { SyncType.Sync, SyncType.HalfSync, SyncType.Spatial, SyncType.None },
                value => value.ToString());
            _arrowDirectionPicker.ItemsSource = CreateOptions(
                new[] { ArrowDirectionMode.LeftToRight, ArrowDirectionMode.RightToLeft, ArrowDirectionMode.Alternating, ArrowDirectionMode.Random },
                value => value switch
                {
                    ArrowDirectionMode.LeftToRight => "Left to right",
                    ArrowDirectionMode.RightToLeft => "Right to left",
                    ArrowDirectionMode.Alternating => "Alternating",
                    ArrowDirectionMode.Random => "Random",
                    _ => value.ToString()
                });
            _arrowPromptFamilyPicker.ItemsSource = CreateOptions(
                new[] { ArrowPromptFamily.OnKeyboardOnly, ArrowPromptFamily.SpecialOnly, ArrowPromptFamily.Mixed },
                value => value switch
                {
                    ArrowPromptFamily.OnKeyboardOnly => "On keyboard",
                    ArrowPromptFamily.SpecialOnly => "Special prompt",
                    ArrowPromptFamily.Mixed => "Mixed",
                    _ => value.ToString()
                });
            _arrowRouteFamilyPicker.ItemsSource = CreateOptions(
                new[] { ArrowRouteFamily.CardinalOnly, ArrowRouteFamily.OrdinalOnly, ArrowRouteFamily.Mixed },
                value => value switch
                {
                    ArrowRouteFamily.CardinalOnly => "Cardinal",
                    ArrowRouteFamily.OrdinalOnly => "Ordinal",
                    ArrowRouteFamily.Mixed => "Mixed",
                    _ => value.ToString()
                });
            _arrowFeedbackPicker.ItemsSource = CreateOptions(
                new[] { ArrowFeedbackMode.Icon, ArrowFeedbackMode.CorrectResponse },
                value => value == ArrowFeedbackMode.Icon ? "Icon" : "Correct response");
        }

        private View BuildKindSpecificForm()
        {
            return _kind switch
            {
                CustomStageKind.PPWScheme => new VerticalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        CreateLabeledField("Operations", _operationSelectionLayout),
                        CreateLabeledField("Layout", _uiQuestionPicker),
                        CreateLabeledField("Variable type", _variableTypesPicker),
                        CreateLabeledField("Numeric keyboard", _numericInputPicker),
                        CreateMinMaxRow("Addends", _minAddendEntry, _maxAddendEntry),
                        CreateMinMaxRow("Sum", _minSumEntry, _maxSumEntry),
                        CreateSwitchField("Only to 10", _onlyToTenSwitch),
                        CreateSwitchField("Only through 10", _onlyThroughTenSwitch),
                        CreateSwitchField("Show previous", _showPrevSwitch),
                        CreateSwitchField("Only close triad", _onlyCloseTriadSwitch),
                        CreateSwitchField("Keyboard help", _keyboardHelpSwitch)
                    }
                },
                CustomStageKind.WeightedKeyboard => new VerticalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        CreateLabeledField("Keyboard weights (up to 10)", _weightsEntry),
                        CreateSwitchField("Allow XXX impossible answer", _allowImpossibleWeightedAnswerSwitch),
                        CreateMinMaxRow("Sum", _minSumEntry, _maxSumEntry),
                        new Label
                        {
                            Text = "Minimum and maximum sum default to the smallest and largest key weight, and you can change them after.",
                            FontSize = 12,
                            TextColor = Colors.DarkSlateGray
                        }
                    }
                },
                CustomStageKind.Arrow => new VerticalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        CreateLabeledField("Arrow direction", _arrowDirectionPicker),
                        CreateLabeledField("Sync type", _syncTypePicker),
                        CreateMinMaxRow("From / length", _minAddendEntry, _maxAddendEntry),
                        CreateMinMaxRow("Target sum", _minSumEntry, _maxSumEntry),
                        CreateLabeledField("Seconds to answer", _secondsToAnswerEntry),
                        CreateSwitchField("Only to 10", _onlyToTenSwitch),
                        CreateSwitchField("Only through 10", _onlyThroughTenSwitch),
                        CreateSwitchField("Dynamic arrow length", _dynamicArrowLengthSwitch),
                        CreateSwitchField("Show numbers on keys", _showNumbersSwitch),
                        CreateLabeledField("Prompt family", _arrowPromptFamilyPicker),
                        CreateLabeledField("Route family", _arrowRouteFamilyPicker),
                        CreateSectionTitle("Special arrow missing value"),
                        CreateSwitchField("Missing start (addend1)", _arrowMissingStartSwitch),
                        CreateSwitchField("Missing length (addend2)", _arrowMissingLengthSwitch),
                        CreateSwitchField("Missing end (sum)", _arrowMissingEndSwitch),
                        CreateLabeledField("Feedback", _arrowFeedbackPicker),
                        CreateSwitchField("Help available", _isHelpNeededSwitch)
                    }
                },
                _ => new VerticalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        CreateLabeledField("Operations", _operationSelectionLayout),
                        CreateLabeledField("Sync type", _syncTypePicker),
                        CreateLabeledField("Keys in row", _keysInRowEntry),
                        CreateLabeledField("Seconds to answer", _secondsToAnswerEntry),
                        CreateSwitchField("Only to 10", _onlyToTenSwitch),
                        CreateSwitchField("Two keyboards on one", _twoKeyboardsSwitch),
                        CreateSwitchField("Only one hand", _onlyOneHandSwitch),
                        CreateSwitchField("Help available", _isHelpNeededSwitch),
                        CreateSectionTitle("Group combinations"),
                        CreateSwitchField("Overlapping", _allowOverlapSwitch),
                        CreateSwitchField("Strange", _allowStrangeSwitch),
                        CreateSwitchField("One inside another", _allowInsideSwitch),
                        CreateSwitchField("Same", _allowSameSwitch),
                        CreateSwitchField("Empty", _allowEmptySwitch)
                    }
                }
            };
        }

        private async Task SaveStageAsync()
        {
            Guid? userId = GetCurrentUserId();
            if (userId == null)
            {
                await DisplayAlert("No user", "Please choose a user first.", "OK");
                return;
            }

            if (!await ValidateWeightedKeyboardStageAsync())
                return;

            string name = _nameEntry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlert("Missing name", "Please give the stage a name.", "OK");
                return;
            }

            CustomStageDefinition stage = new()
            {
                Id = _editingId ?? Guid.NewGuid(),
                UserId = userId.Value,
                Name = name,
                StageKind = _kind,
                Config = BuildConfigFromForm(name)
            };

            await _stageRepository.SaveOrUpdateAsync(stage);
            _editingId = stage.Id;
            _saveButton.Text = "Update Stage";
            await RefreshSavedStagesAsync();
        }

        private async Task PlayCurrentAsync()
        {
            if (!await ValidateWeightedKeyboardStageAsync())
                return;

            string name = _nameEntry.Text?.Trim();
            GameConfig config = BuildConfigFromForm(string.IsNullOrWhiteSpace(name) ? $"Custom {CustomStageCatalog.GetDisplayName(_kind)}" : name);
            await Navigation.PushAsync(new SimpleViewCellsPage(config));
        }

        private GameConfig BuildConfigFromForm(string name)
        {
            GameConfig config = _loadedConfigSnapshot != null
                ? CustomStageCatalog.DeepClone(_loadedConfigSnapshot)
                : CustomStageCatalog.CreateDefaultConfig(_kind);
            config.GameName = name;
            config.NumberOfTasksToWin = ReadInt(_tasksToWinEntry, config.NumberOfTasksToWin);
            config.NumberOfMistakesToLose = ReadInt(_mistakesToLoseEntry, config.NumberOfMistakesToLose);

            switch (_kind)
            {
                case CustomStageKind.PPWScheme:
                    config.OperationList = GetSelectedOperations(CustomStageKind.PPWScheme);
                    config.UIQuestionType = GetPickerValue(_uiQuestionPicker, UIQuestionType.ThreeTexts);
                    if (_variableTypesPicker.SelectedItem is OptionItem<VariableTypes> variableOption)
                        config.VariableTypes = variableOption.Value;
                    config.NumericInputMode = GetPickerValue(_numericInputPicker, NumericInputMode.AppKeypad);
                    config.MinAddend = ReadInt(_minAddendEntry, config.MinAddend);
                    config.MaxAddend = ReadInt(_maxAddendEntry, config.MaxAddend);
                    config.MinSum = ReadInt(_minSumEntry, config.MinSum);
                    config.MaxSum = ReadInt(_maxSumEntry, config.MaxSum);
                    config.OnlyToTen = _onlyToTenSwitch.IsToggled;
                    config.OnlyThrougTen = _onlyThroughTenSwitch.IsToggled;
                    config.ShowPrev = _showPrevSwitch.IsToggled;
                    config.OnlyCloseTriad = _onlyCloseTriadSwitch.IsToggled;
                    config.KeyboardConfig = _keyboardHelpSwitch.IsToggled ? new KeyboardConfig { KeyboardOnlyForHelp = true } : null;
                    break;
                case CustomStageKind.WeightedKeyboard:
                    int[] weightedStageWeights = ParseWeights(_weightsEntry.Text) ?? Array.Empty<int>();
                    config.OperationList = new List<Operation> { Operation.Sum };
                    config.UIQuestionType = UIQuestionType.OneText;
                    config.VariableTypes = VariableTypes.TwoNoSum;
                    config.NumericInputMode = NumericInputMode.AppKeypad;
                    config.MinSum = ReadInt(_minSumEntry, config.MinSum);
                    config.MaxSum = ReadInt(_maxSumEntry, config.MaxSum);
                    config.KeyboardConfig = new KeyboardConfig
                    {
                        SyncType = SyncType.Sync,
                        TextBoxesQuantity = 1,
                        Rows = 1,
                        KeysInRow = weightedStageWeights.Length == 0 ? config.KeyboardConfig?.KeysInRow ?? 10 : weightedStageWeights.Length,
                        SecondsPressingToAnswer = 2,
                        WeightsArray = weightedStageWeights,
                        ShowNumbersOnKeys = true,
                        AllowSumHeaderVisibilityToggle = false,
                        UseWeightedCustomStageTargets = true,
                        AllowImpossibleWeightedAnswer = _allowImpossibleWeightedAnswerSwitch.IsToggled
                    };
                    break;
                case CustomStageKind.Arrow:
                    ArrowDirectionMode arrowDirection = GetPickerValue(_arrowDirectionPicker, ArrowDirectionMode.LeftToRight);
                    config.MinAddend = ReadInt(_minAddendEntry, config.MinAddend);
                    config.MaxAddend = ReadInt(_maxAddendEntry, config.MaxAddend);
                    config.MinSum = ReadInt(_minSumEntry, config.MinSum);
                    config.MaxSum = ReadInt(_maxSumEntry, config.MaxSum);
                    config.OnlyToTen = _onlyToTenSwitch.IsToggled;
                    config.OnlyThrougTen = _onlyThroughTenSwitch.IsToggled;
                    config.KeyboardConfig ??= new KeyboardConfig();
                    config.KeyboardConfig.SyncType = GetPickerValue(_syncTypePicker, SyncType.Sync);
                    config.KeyboardConfig.ArrowDirectionMode = arrowDirection;
                    config.QuestionOrder = arrowDirection switch
                    {
                        ArrowDirectionMode.RightToLeft => QuestionOrder.ToLeft,
                        ArrowDirectionMode.Alternating => QuestionOrder.BackAndForth,
                        ArrowDirectionMode.Random => QuestionOrder.Random,
                        _ => QuestionOrder.FromLeft
                    };
                    config.KeyboardConfig.SecondsPressingToAnswer = ReadInt(_secondsToAnswerEntry, 2);
                    config.KeyboardConfig.IsArrowLengthDynamic = _dynamicArrowLengthSwitch.IsToggled;
                    config.KeyboardConfig.ShowNumbersOnKeys = _showNumbersSwitch.IsToggled;
                    config.KeyboardConfig.ArrowLabelExerciseMode = ArrowLabelExerciseMode.None;
                    ArrowPromptFamily promptFamily = GetPickerValue(_arrowPromptFamilyPicker, ArrowPromptFamily.OnKeyboardOnly);
                    config.KeyboardConfig.AllowedArrowPromptKinds = promptFamily switch
                    {
                        ArrowPromptFamily.SpecialOnly => ArrowPromptKindFlags.SpecialPrompt,
                        ArrowPromptFamily.Mixed => ArrowPromptKindFlags.OnKeyboard | ArrowPromptKindFlags.SpecialPrompt,
                        _ => ArrowPromptKindFlags.OnKeyboard
                    };
                    ArrowRouteFamily routeFamily = GetPickerValue(_arrowRouteFamilyPicker, ArrowRouteFamily.CardinalOnly);
                    config.KeyboardConfig.AllowedArrowRouteKinds = routeFamily switch
                    {
                        ArrowRouteFamily.OrdinalOnly => ArrowRouteKindFlags.Ordinal,
                        ArrowRouteFamily.Mixed => ArrowRouteKindFlags.Cardinal | ArrowRouteKindFlags.Ordinal,
                        _ => ArrowRouteKindFlags.Cardinal
                    };
                    config.KeyboardConfig.SpecialArrowMissingTargets =
                        (_arrowMissingStartSwitch.IsToggled ? MissingValueTargetFlags.Addend1 : MissingValueTargetFlags.None) |
                        (_arrowMissingLengthSwitch.IsToggled ? MissingValueTargetFlags.Addend2 : MissingValueTargetFlags.None) |
                        (_arrowMissingEndSwitch.IsToggled ? MissingValueTargetFlags.Sum : MissingValueTargetFlags.None);
                    config.KeyboardConfig.ArrowFeedbackMode = GetPickerValue(_arrowFeedbackPicker, ArrowFeedbackMode.Icon);
                    config.KeyboardConfig.IsHelpNeeded = _isHelpNeededSwitch.IsToggled;
                    config.KeyboardConfig.IsArrow = config.KeyboardConfig.AllowedArrowPromptKinds.HasFlag(ArrowPromptKindFlags.OnKeyboard);
                    break;
                case CustomStageKind.Logical:
                    config.OperationList = GetSelectedOperations(CustomStageKind.Logical);
                    config.OnlyToTen = _onlyToTenSwitch.IsToggled;
                    config.IsOnlyOneHand = _onlyOneHandSwitch.IsToggled;
                    config.TwoKeybordsOnOne = _twoKeyboardsSwitch.IsToggled;
                    config.AllowedGroupCombinations = BuildGroupCombinationMode();
                    config.KeyboardConfig ??= new KeyboardConfig();
                    config.KeyboardConfig.SyncType = GetPickerValue(_syncTypePicker, SyncType.Sync);
                    config.KeyboardConfig.KeysInRow = ReadInt(_keysInRowEntry, 6);
                    config.KeyboardConfig.SecondsPressingToAnswer = ReadInt(_secondsToAnswerEntry, 2);
                    config.KeyboardConfig.IsHelpNeeded = _isHelpNeededSwitch.IsToggled;
                    break;
            }

            return CustomStageCatalog.Normalize(_kind, config);
        }

        private async Task RefreshSavedStagesAsync()
        {
            _savedStagesLayout.Children.Clear();
            List<CustomStageDefinition> stages = await _stageRepository.GetByKindAsync(GetCurrentUserId(), _kind);

            if (stages.Count == 0)
            {
                _savedStagesLayout.Children.Add(new Label { Text = "No saved stages yet." });
                return;
            }

            foreach (CustomStageDefinition stage in stages)
                _savedStagesLayout.Children.Add(CreateSavedStageCard(stage));
        }

        private View CreateSavedStageCard(CustomStageDefinition stage)
        {
            Button editButton = CreateMiniButton("Edit", async () =>
            {
                LoadDefinitionIntoForm(stage);
                await Task.CompletedTask;
            });
            Button playButton = CreateMiniButton("Play", async () =>
            {
                await Navigation.PushAsync(new SimpleViewCellsPage(CustomStageCatalog.ClonePlayableConfig(stage)));
            });
            Button deleteButton = CreateMiniButton("Delete", async () =>
            {
                bool shouldDelete = await DisplayAlert("Delete stage", $"Delete '{stage.Name}'?", "Delete", "Cancel");
                if (!shouldDelete)
                    return;
                await _stageRepository.DeleteByIdAsync(stage.Id);
                if (_editingId == stage.Id)
                    LoadDefinitionIntoForm(null);
                await RefreshSavedStagesAsync();
            });

            return new Frame
            {
                Padding = 10,
                BackgroundColor = Colors.White,
                BorderColor = Colors.LightGray,
                Content = new VerticalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        new Label { Text = stage.Name, FontAttributes = FontAttributes.Bold, TextColor = Colors.Black },
                        new Label { Text = CustomStageCatalog.BuildSummary(stage), FontSize = 12, TextColor = Colors.DarkSlateGray },
                        new HorizontalStackLayout { Spacing = 6, Children = { editButton, playButton, deleteButton } }
                    }
                }
            };
        }

        private void LoadDefinitionIntoForm(CustomStageDefinition? stage)
        {
            CustomStageDefinition effectiveStage = stage ?? new CustomStageDefinition
            {
                StageKind = _kind,
                Name = string.Empty,
                Config = CustomStageCatalog.CreateDefaultConfig(_kind)
            };

            GameConfig config = CustomStageCatalog.Normalize(_kind, CustomStageCatalog.DeepClone(effectiveStage.Config));
            _loadedConfigSnapshot = CustomStageCatalog.DeepClone(config);
            _editingId = stage?.Id;
            _saveButton.Text = stage == null ? "Save Stage" : "Update Stage";
            _nameEntry.Text = stage?.Name ?? string.Empty;
            _tasksToWinEntry.Text = config.NumberOfTasksToWin.ToString();
            _mistakesToLoseEntry.Text = config.NumberOfMistakesToLose.ToString();

            switch (_kind)
            {
                case CustomStageKind.PPWScheme:
                    SetSelectedOperations(config.OperationList);
                    SetPickerValue(_uiQuestionPicker, config.UIQuestionType);
                    if (config.TryGetLegacyVariableType(out VariableTypes variableType))
                        SetPickerValue(_variableTypesPicker, variableType);
                    else
                        _variableTypesPicker.SelectedItem = null;
                    SetPickerValue(_numericInputPicker, config.NumericInputMode);
                    _minAddendEntry.Text = config.MinAddend.ToString();
                    _maxAddendEntry.Text = config.MaxAddend.ToString();
                    _minSumEntry.Text = config.MinSum.ToString();
                    _maxSumEntry.Text = config.MaxSum.ToString();
                    _onlyToTenSwitch.IsToggled = config.OnlyToTen;
                    _onlyThroughTenSwitch.IsToggled = config.OnlyThrougTen;
                    _showPrevSwitch.IsToggled = config.ShowPrev;
                    _onlyCloseTriadSwitch.IsToggled = config.OnlyCloseTriad;
                    _keyboardHelpSwitch.IsToggled = config.KeyboardConfig?.KeyboardOnlyForHelp == true;
                    _weightsEntry.Text = string.Empty;
                    _allowImpossibleWeightedAnswerSwitch.IsToggled = false;
                    _suggestedWeightedStageWeights = null;
                    break;
                case CustomStageKind.WeightedKeyboard:
                    _weightsEntry.Text = string.Join(",", config.KeyboardConfig?.WeightsArray ?? Array.Empty<int>());
                    _allowImpossibleWeightedAnswerSwitch.IsToggled = config.KeyboardConfig?.AllowImpossibleWeightedAnswer == true;
                    _minSumEntry.Text = config.MinSum.ToString();
                    _maxSumEntry.Text = config.MaxSum.ToString();
                    _suggestedWeightedStageWeights = config.KeyboardConfig?.WeightsArray?.ToArray();
                    TryApplySuggestedWeightedRange(forceApply: false);
                    break;
                case CustomStageKind.Arrow:
                    SetPickerValue(_arrowDirectionPicker, config.KeyboardConfig?.ArrowDirectionMode switch
                    {
                        ArrowDirectionMode.LeftToRight or ArrowDirectionMode.RightToLeft or ArrowDirectionMode.Alternating or ArrowDirectionMode.Random
                            => config.KeyboardConfig.ArrowDirectionMode,
                        _ => config.QuestionOrder switch
                        {
                            QuestionOrder.ToLeft => ArrowDirectionMode.RightToLeft,
                            QuestionOrder.BackAndForth => ArrowDirectionMode.Alternating,
                            QuestionOrder.Random => ArrowDirectionMode.Random,
                            _ => ArrowDirectionMode.LeftToRight
                        }
                    });
                    SetPickerValue(_syncTypePicker, config.KeyboardConfig?.SyncType ?? SyncType.Sync);
                    _minAddendEntry.Text = config.MinAddend.ToString();
                    _maxAddendEntry.Text = config.MaxAddend.ToString();
                    _minSumEntry.Text = config.MinSum.ToString();
                    _maxSumEntry.Text = config.MaxSum.ToString();
                    _secondsToAnswerEntry.Text = (config.KeyboardConfig?.SecondsPressingToAnswer ?? 2).ToString();
                    _onlyToTenSwitch.IsToggled = config.OnlyToTen;
                    _onlyThroughTenSwitch.IsToggled = config.OnlyThrougTen;
                    _dynamicArrowLengthSwitch.IsToggled = config.KeyboardConfig?.IsArrowLengthDynamic == true;
                    _showNumbersSwitch.IsToggled = config.KeyboardConfig?.ShowNumbersOnKeys == true;
                    ArrowPromptKindFlags promptKinds = config.KeyboardConfig?.AllowedArrowPromptKinds ?? ArrowPromptKindFlags.None;
                    if (promptKinds == ArrowPromptKindFlags.None)
                        promptKinds = config.KeyboardConfig?.ArrowLabelExerciseMode == ArrowLabelExerciseMode.None
                            ? ArrowPromptKindFlags.OnKeyboard
                            : ArrowPromptKindFlags.SpecialPrompt;
                    SetPickerValue(_arrowPromptFamilyPicker, promptKinds switch
                    {
                        ArrowPromptKindFlags.SpecialPrompt => ArrowPromptFamily.SpecialOnly,
                        ArrowPromptKindFlags.OnKeyboard | ArrowPromptKindFlags.SpecialPrompt => ArrowPromptFamily.Mixed,
                        _ => ArrowPromptFamily.OnKeyboardOnly
                    });

                    ArrowRouteKindFlags routeKinds = config.KeyboardConfig?.AllowedArrowRouteKinds ?? ArrowRouteKindFlags.None;
                    if (routeKinds == ArrowRouteKindFlags.None)
                        routeKinds = config.KeyboardConfig?.ArrowLabelExerciseMode == ArrowLabelExerciseMode.OrdinalStartAndLength ||
                                     config.KeyboardConfig?.ArrowType == ArrowType.Rounded
                            ? ArrowRouteKindFlags.Ordinal
                            : ArrowRouteKindFlags.Cardinal;
                    SetPickerValue(_arrowRouteFamilyPicker, routeKinds switch
                    {
                        ArrowRouteKindFlags.Ordinal => ArrowRouteFamily.OrdinalOnly,
                        ArrowRouteKindFlags.Cardinal | ArrowRouteKindFlags.Ordinal => ArrowRouteFamily.Mixed,
                        _ => ArrowRouteFamily.CardinalOnly
                    });

                    MissingValueTargetFlags missingTargets = config.KeyboardConfig?.SpecialArrowMissingTargets ?? MissingValueTargetFlags.None;
                    if (missingTargets == MissingValueTargetFlags.None)
                    {
                        missingTargets = config.KeyboardConfig?.ArrowLabelExerciseMode switch
                        {
                            ArrowLabelExerciseMode.StartAndEndWithMissingLength => MissingValueTargetFlags.Addend2,
                            ArrowLabelExerciseMode.EndAndLengthWithMissingStart => MissingValueTargetFlags.Addend1,
                            _ => MissingValueTargetFlags.Sum
                        };
                    }
                    _arrowMissingStartSwitch.IsToggled = missingTargets.HasFlag(MissingValueTargetFlags.Addend1);
                    _arrowMissingLengthSwitch.IsToggled = missingTargets.HasFlag(MissingValueTargetFlags.Addend2);
                    _arrowMissingEndSwitch.IsToggled = missingTargets.HasFlag(MissingValueTargetFlags.Sum);
                    SetPickerValue(_arrowFeedbackPicker, config.KeyboardConfig?.ArrowFeedbackMode ?? ArrowFeedbackMode.Icon);
                    _isHelpNeededSwitch.IsToggled = config.KeyboardConfig?.IsHelpNeeded == true;
                    break;
                case CustomStageKind.Logical:
                    SetSelectedOperations(config.OperationList);
                    SetPickerValue(_syncTypePicker, config.KeyboardConfig?.SyncType ?? SyncType.Sync);
                    _keysInRowEntry.Text = (config.KeyboardConfig?.KeysInRow ?? 6).ToString();
                    _secondsToAnswerEntry.Text = (config.KeyboardConfig?.SecondsPressingToAnswer ?? 2).ToString();
                    _onlyToTenSwitch.IsToggled = config.OnlyToTen;
                    _twoKeyboardsSwitch.IsToggled = config.TwoKeybordsOnOne;
                    _onlyOneHandSwitch.IsToggled = config.IsOnlyOneHand;
                    _isHelpNeededSwitch.IsToggled = config.KeyboardConfig?.IsHelpNeeded == true;
                    LoadGroupCombinationMode(config.AllowedGroupCombinations);
                    break;
            }
        }

        private Guid? GetCurrentUserId() => ServiceHelper.GetService<CurrentUserSession>().ActiveUser?.Id;

        private static Entry CreateEntry(string placeholder) => new() { Placeholder = placeholder, BackgroundColor = Colors.White };
        private static Entry CreateNumericEntry() => new() { Keyboard = Keyboard.Numeric, BackgroundColor = Colors.White, WidthRequest = 100 };
        private static Label CreateSectionTitle(string text) => new() { Text = text, FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Colors.Black };

        private static View CreateLabeledField(string label, View field) => new VerticalStackLayout
        {
            Spacing = 4,
            Children = { new Label { Text = label, TextColor = Colors.Black }, field }
        };

        private static View CreateMinMaxRow(string label, Entry minEntry, Entry maxEntry) => new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label { Text = label, TextColor = Colors.Black },
                new HorizontalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        new Label { Text = "Min", VerticalTextAlignment = TextAlignment.Center },
                        minEntry,
                        new Label { Text = "Max", VerticalTextAlignment = TextAlignment.Center },
                        maxEntry
                    }
                }
            }
        };

        private static View CreateSwitchField(string label, Switch toggle) => new HorizontalStackLayout
        {
            Spacing = 10,
            Children = { new Label { Text = label, VerticalTextAlignment = TextAlignment.Center, TextColor = Colors.Black }, toggle }
        };

        private static Button CreateMiniButton(string text, Func<Task> onClick)
        {
            Button button = new() { Text = text, FontSize = 12, Padding = new Thickness(10, 4), BackgroundColor = Colors.MediumPurple, TextColor = Colors.White };
            button.Clicked += async (_, __) => await onClick();
            return button;
        }

        private void BuildOperationSelection()
        {
            _operationSelectionLayout.Children.Clear();
            _operationSwitches.Clear();

            foreach (Operation operation in GetAvailableOperations(_kind))
            {
                Switch toggle = new();
                _operationSwitches[operation] = toggle;
                _operationSelectionLayout.Children.Add(CreateSwitchField(operation.ToDString(), toggle));
            }
        }

        private List<Operation> GetSelectedOperations(CustomStageKind kind)
        {
            List<Operation> selected = _operationSwitches
                .Where(item => item.Value.IsToggled)
                .Select(item => item.Key)
                .ToList();

            if (selected.Count == 0)
                selected.Add(GetAvailableOperations(kind).First());

            return selected;
        }

        private void SetSelectedOperations(IEnumerable<Operation> operations)
        {
            HashSet<Operation> selected = operations?.ToHashSet() ?? new HashSet<Operation>();
            foreach ((Operation operation, Switch toggle) in _operationSwitches)
                toggle.IsToggled = selected.Contains(operation);
        }

        private GroupCombinationMode BuildGroupCombinationMode()
        {
            GroupCombinationMode mode = GroupCombinationMode.None;
            if (_allowOverlapSwitch.IsToggled) mode |= GroupCombinationMode.Overlapping;
            if (_allowStrangeSwitch.IsToggled) mode |= GroupCombinationMode.Strange;
            if (_allowInsideSwitch.IsToggled) mode |= GroupCombinationMode.OneInsideAnother;
            if (_allowSameSwitch.IsToggled) mode |= GroupCombinationMode.Same;
            if (_allowEmptySwitch.IsToggled) mode |= GroupCombinationMode.Empty;
            return mode;
        }

        private void LoadGroupCombinationMode(GroupCombinationMode mode)
        {
            _allowOverlapSwitch.IsToggled = mode.HasFlag(GroupCombinationMode.Overlapping);
            _allowStrangeSwitch.IsToggled = mode.HasFlag(GroupCombinationMode.Strange);
            _allowInsideSwitch.IsToggled = mode.HasFlag(GroupCombinationMode.OneInsideAnother);
            _allowSameSwitch.IsToggled = mode.HasFlag(GroupCombinationMode.Same);
            _allowEmptySwitch.IsToggled = mode.HasFlag(GroupCombinationMode.Empty);
        }

        private static List<OptionItem<T>> CreateOptions<T>(IEnumerable<T> values, Func<T, string> labelFactory)
            => values.Select(value => new OptionItem<T> { Value = value, Label = labelFactory(value) }).ToList();

        private static T GetPickerValue<T>(Picker picker, T fallback)
            => picker.SelectedItem is OptionItem<T> option ? option.Value : fallback;

        private static void SetPickerValue<T>(Picker picker, T value)
        {
            IEnumerable<OptionItem<T>> options = (picker.ItemsSource as IEnumerable<OptionItem<T>>) ?? Enumerable.Empty<OptionItem<T>>();
            picker.SelectedItem = options.FirstOrDefault(item => EqualityComparer<T>.Default.Equals(item.Value, value)) ?? options.FirstOrDefault();
        }

        private static int ReadInt(Entry entry, int fallback)
            => int.TryParse(entry.Text, out int parsed) ? parsed : fallback;

        private async Task<bool> ValidateWeightedKeyboardStageAsync()
        {
            if (_kind != CustomStageKind.WeightedKeyboard)
                return true;

            int[]? weights = ParseWeights(_weightsEntry.Text);
            if (weights?.Length > 0)
                return true;

            await DisplayAlert("Missing weights", "Enter up to 10 positive keyboard weights for the weighted stage.", "OK");
            return false;
        }

        private void TryApplySuggestedWeightedRange(bool forceApply)
        {
            int[]? weights = ParseWeights(_weightsEntry.Text);
            if (weights == null || weights.Length == 0)
                return;

            int suggestedMin = weights.Min();
            int suggestedMax = weights.Max();

            bool shouldUpdateMin = forceApply ||
                                   string.IsNullOrWhiteSpace(_minSumEntry.Text) ||
                                   (_suggestedWeightedStageWeights != null &&
                                    int.TryParse(_minSumEntry.Text, out int currentMin) &&
                                    currentMin == _suggestedWeightedStageWeights.Min());

            bool shouldUpdateMax = forceApply ||
                                   string.IsNullOrWhiteSpace(_maxSumEntry.Text) ||
                                   (_suggestedWeightedStageWeights != null &&
                                    int.TryParse(_maxSumEntry.Text, out int currentMax) &&
                                    currentMax == _suggestedWeightedStageWeights.Max());

            if (shouldUpdateMin)
                _minSumEntry.Text = suggestedMin.ToString();

            if (shouldUpdateMax)
                _maxSumEntry.Text = suggestedMax.ToString();

            _suggestedWeightedStageWeights = weights.ToArray();
        }

        private static int[]? ParseWeights(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            string[] tokens = text
                .Split(new[] { ',', ';', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            List<int> weights = new();
            foreach (string token in tokens.Take(10))
            {
                if (!int.TryParse(token, out int parsed) || parsed <= 0)
                    continue;

                weights.Add(parsed);
            }

            return weights.Count == 0 ? null : weights.ToArray();
        }

        private static List<Operation> GetAvailableOperations(CustomStageKind kind) => kind switch
        {
            CustomStageKind.PPWScheme => new() { Operation.Sum, Operation.Minus, Operation.Multiplication, Operation.Divide },
            CustomStageKind.WeightedKeyboard => new() { Operation.Sum },
            CustomStageKind.Arrow => new() { Operation.Sum },
            _ => new() { Operation.Copy, Operation.Quantity, Operation.MoveBy, Operation.Mirror, Operation.Not, Operation.And, Operation.Or, Operation.ExclusiveOr, Operation.SUMM }
        };
    }
}
