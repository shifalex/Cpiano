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

        private readonly CustomStageKind _kind;
        private readonly CustomStageDefinitionRepository _stageRepository;
        private readonly Entry _nameEntry = CreateEntry("Stage name");
        private readonly Entry _minAddendEntry = CreateNumericEntry();
        private readonly Entry _maxAddendEntry = CreateNumericEntry();
        private readonly Entry _minSumEntry = CreateNumericEntry();
        private readonly Entry _maxSumEntry = CreateNumericEntry();
        private readonly Entry _tasksToWinEntry = CreateNumericEntry();
        private readonly Entry _mistakesToLoseEntry = CreateNumericEntry();
        private readonly Entry _keysInRowEntry = CreateNumericEntry();
        private readonly Entry _secondsToAnswerEntry = CreateNumericEntry();
        private readonly Picker _uiQuestionPicker = new();
        private readonly Picker _variableTypesPicker = new();
        private readonly Picker _numericInputPicker = new();
        private readonly Picker _questionOrderPicker = new();
        private readonly Picker _syncTypePicker = new();
        private readonly VerticalStackLayout _operationSelectionLayout = new() { Spacing = 6 };
        private readonly Switch _showPrevSwitch = new();
        private readonly Switch _onlyCloseTriadSwitch = new();
        private readonly Switch _keyboardHelpSwitch = new();
        private readonly Switch _onlyToTenSwitch = new();
        private readonly Switch _onlyThroughTenSwitch = new();
        private readonly Switch _dynamicArrowLengthSwitch = new();
        private readonly Switch _showNumbersSwitch = new();
        private readonly Switch _roundedArrowSwitch = new();
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

        public CustomStageEditorPage(CustomStageKind kind)
        {
            _kind = kind;
            _stageRepository = ServiceHelper.GetService<CustomStageDefinitionRepository>();
            Title = $"{CustomStageCatalog.GetDisplayName(kind)} Stage Builder";
            BackgroundColor = Colors.Beige;

            ConfigurePickers();

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
            _questionOrderPicker.ItemsSource = CreateOptions(
                new[] { QuestionOrder.FromLeft, QuestionOrder.ToLeft, QuestionOrder.BackAndForth, QuestionOrder.Random, QuestionOrder.CyclicalLeft, QuestionOrder.CyclicalRight, QuestionOrder.CyclicalMixed },
                value => value.ToString());
            _syncTypePicker.ItemsSource = CreateOptions(
                new[] { SyncType.Sync, SyncType.HalfSync, SyncType.Spatial, SyncType.None },
                value => value.ToString());
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
                CustomStageKind.Arrow => new VerticalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        CreateLabeledField("Question order", _questionOrderPicker),
                        CreateLabeledField("Sync type", _syncTypePicker),
                        CreateMinMaxRow("From / length", _minAddendEntry, _maxAddendEntry),
                        CreateMinMaxRow("Target sum", _minSumEntry, _maxSumEntry),
                        CreateLabeledField("Seconds to answer", _secondsToAnswerEntry),
                        CreateSwitchField("Only to 10", _onlyToTenSwitch),
                        CreateSwitchField("Only through 10", _onlyThroughTenSwitch),
                        CreateSwitchField("Dynamic arrow length", _dynamicArrowLengthSwitch),
                        CreateSwitchField("Show numbers on keys", _showNumbersSwitch),
                        CreateSwitchField("Rounded arrow", _roundedArrowSwitch),
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
                case CustomStageKind.Arrow:
                    config.QuestionOrder = GetPickerValue(_questionOrderPicker, QuestionOrder.FromLeft);
                    config.MinAddend = ReadInt(_minAddendEntry, config.MinAddend);
                    config.MaxAddend = ReadInt(_maxAddendEntry, config.MaxAddend);
                    config.MinSum = ReadInt(_minSumEntry, config.MinSum);
                    config.MaxSum = ReadInt(_maxSumEntry, config.MaxSum);
                    config.OnlyToTen = _onlyToTenSwitch.IsToggled;
                    config.OnlyThrougTen = _onlyThroughTenSwitch.IsToggled;
                    config.KeyboardConfig ??= new KeyboardConfig();
                    config.KeyboardConfig.SyncType = GetPickerValue(_syncTypePicker, SyncType.Sync);
                    config.KeyboardConfig.IsArrow = true;
                    config.KeyboardConfig.SecondsPressingToAnswer = ReadInt(_secondsToAnswerEntry, 2);
                    config.KeyboardConfig.IsArrowLengthDynamic = _dynamicArrowLengthSwitch.IsToggled;
                    config.KeyboardConfig.ShowNumbersOnKeys = _showNumbersSwitch.IsToggled;
                    config.KeyboardConfig.ArrowType = _roundedArrowSwitch.IsToggled ? ArrowType.Rounded : ArrowType.Straight;
                    config.KeyboardConfig.IsHelpNeeded = _isHelpNeededSwitch.IsToggled;
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
                    break;
                case CustomStageKind.Arrow:
                    SetPickerValue(_questionOrderPicker, config.QuestionOrder);
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
                    _roundedArrowSwitch.IsToggled = config.KeyboardConfig?.ArrowType == ArrowType.Rounded;
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

        private static List<Operation> GetAvailableOperations(CustomStageKind kind) => kind switch
        {
            CustomStageKind.PPWScheme => new() { Operation.Sum, Operation.Minus, Operation.Multiplication, Operation.Divide },
            CustomStageKind.Arrow => new() { Operation.Sum },
            _ => new() { Operation.Copy, Operation.Quantity, Operation.MoveBy, Operation.Mirror, Operation.Not, Operation.And, Operation.Or, Operation.ExclusiveOr, Operation.SUMM }
        };
    }
}
