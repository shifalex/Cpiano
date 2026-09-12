using GestureSample.Maui.Data;
using GestureSample.Maui.Data.SQLite;
using GestureSample.Maui.Models.CustomStages;
using GestureSample.Maui.Handlers;

namespace GestureSample.Maui.Views.CustomStages
{
    public class CustomStageFlowPage : ContentPage
    {
        private readonly CustomStageDefinitionRepository _stageRepository;
        private readonly CustomStageFlowDefinitionRepository _flowRepository;
        private readonly Entry _nameEntry = new() { Placeholder = "Flow name", BackgroundColor = Colors.White };
        private readonly VerticalStackLayout _currentFlowLayout = new() { Spacing = 8 };
        private readonly VerticalStackLayout _availableStagesLayout = new() { Spacing = 8 };
        private readonly VerticalStackLayout _savedFlowsLayout = new() { Spacing = 8 };
        private readonly List<CustomStageFlowItem> _items = new();
        private List<CustomStageDefinition> _allStages = new();
        private Guid? _editingFlowId;

        public CustomStageFlowPage()
        {
            _stageRepository = ServiceHelper.GetService<CustomStageDefinitionRepository>();
            _flowRepository = ServiceHelper.GetService<CustomStageFlowDefinitionRepository>();
            Title = "Custom Stage Flows";
            BackgroundColor = Colors.Beige;

            Button saveButton = new() { Text = "Save Flow", BackgroundColor = Colors.MediumPurple, TextColor = Colors.White };
            saveButton.Clicked += async (_, __) => await SaveFlowAsync();

            Button clearButton = new() { Text = "Clear Flow", BackgroundColor = Colors.Gray, TextColor = Colors.White };
            clearButton.Clicked += (_, __) => ClearForm();

            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = new Thickness(16, 12),
                    Spacing = 12,
                    Children =
                    {
                        CreateTitle("Define flow from stored custom stages"),
                        new Label
                        {
                            Text = "This first version stores stage order and reuse. It does not auto-run the full flow yet.",
                            FontSize = 12,
                            TextColor = Colors.DarkSlateGray
                        },
                        CreateLabeled("Flow name", _nameEntry),
                        new HorizontalStackLayout { Spacing = 8, Children = { saveButton, clearButton } },
                        CreateTitle("Current flow"),
                        _currentFlowLayout,
                        CreateTitle("Available stages"),
                        _availableStagesLayout,
                        CreateTitle("Saved flows"),
                        _savedFlowsLayout
                    }
                }
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ReloadAsync();
        }

        private async Task ReloadAsync()
        {
            _allStages = await _stageRepository.GetByUserAsync(GetCurrentUserId());
            RebuildAvailableStages();
            RebuildCurrentFlow();
            await RebuildSavedFlowsAsync();
        }

        private async Task SaveFlowAsync()
        {
            Guid? userId = GetCurrentUserId();
            if (userId == null)
            {
                await DisplayAlert("No user", "Please choose a user first.", "OK");
                return;
            }

            if (_items.Count == 0)
            {
                await DisplayAlert("Empty flow", "Please add at least one stage.", "OK");
                return;
            }

            string name = _nameEntry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlert("Missing name", "Please give the flow a name.", "OK");
                return;
            }

            CustomStageFlowDefinition flow = new()
            {
                Id = _editingFlowId ?? Guid.NewGuid(),
                UserId = userId.Value,
                Name = name,
                Items = _items.Select(item => new CustomStageFlowItem { StageId = item.StageId }).ToList()
            };

            await _flowRepository.SaveOrUpdateAsync(flow);
            _editingFlowId = flow.Id;
            await RebuildSavedFlowsAsync();
        }

        private void RebuildAvailableStages()
        {
            _availableStagesLayout.Children.Clear();
            if (_allStages.Count == 0)
            {
                _availableStagesLayout.Children.Add(new Label { Text = "Create custom stages first." });
                return;
            }

            foreach (IGrouping<CustomStageKind, CustomStageDefinition> group in _allStages.GroupBy(stage => stage.StageKind))
            {
                _availableStagesLayout.Children.Add(new Label
                {
                    Text = CustomStageCatalog.GetDisplayName(group.Key),
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black
                });

                foreach (CustomStageDefinition stage in group)
                {
                    Button addButton = new() { Text = $"+ {stage.Name}", BackgroundColor = Colors.White, TextColor = Colors.Black };
                    addButton.Clicked += (_, __) =>
                    {
                        _items.Add(new CustomStageFlowItem { StageId = stage.Id });
                        RebuildCurrentFlow();
                    };
                    _availableStagesLayout.Children.Add(addButton);
                }
            }
        }

        private void RebuildCurrentFlow()
        {
            _currentFlowLayout.Children.Clear();
            if (_items.Count == 0)
            {
                _currentFlowLayout.Children.Add(new Label { Text = "No stages in the flow yet." });
                return;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                int index = i;
                CustomStageDefinition? stage = _allStages.FirstOrDefault(item => item.Id == _items[index].StageId);
                string title = stage == null ? "(Missing stage)" : $"{index + 1}. {stage.Name} [{CustomStageCatalog.GetDisplayName(stage.StageKind)}]";

                _currentFlowLayout.Children.Add(new Frame
                {
                    Padding = 8,
                    BackgroundColor = Colors.White,
                    BorderColor = Colors.LightGray,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children =
                        {
                            new Label { Text = title, TextColor = Colors.Black },
                            new HorizontalStackLayout
                            {
                                Spacing = 6,
                                Children =
                                {
                                    CreateMiniButton("Up", () => MoveItem(index, -1)),
                                    CreateMiniButton("Down", () => MoveItem(index, 1)),
                                    CreateMiniButton("Remove", () =>
                                    {
                                        _items.RemoveAt(index);
                                        RebuildCurrentFlow();
                                    })
                                }
                            }
                        }
                    }
                });
            }
        }

        private void MoveItem(int index, int delta)
        {
            int newIndex = index + delta;
            if (newIndex < 0 || newIndex >= _items.Count)
                return;
            (_items[newIndex], _items[index]) = (_items[index], _items[newIndex]);
            RebuildCurrentFlow();
        }

        private async Task RebuildSavedFlowsAsync()
        {
            _savedFlowsLayout.Children.Clear();
            List<CustomStageFlowDefinition> flows = await _flowRepository.GetByUserAsync(GetCurrentUserId());
            if (flows.Count == 0)
            {
                _savedFlowsLayout.Children.Add(new Label { Text = "No saved flows yet." });
                return;
            }

            foreach (CustomStageFlowDefinition flow in flows)
            {
                string summary = string.Join("  ->  ", flow.Items.Select(item => _allStages.FirstOrDefault(stage => stage.Id == item.StageId)?.Name ?? "(Missing)"));

                _savedFlowsLayout.Children.Add(new Frame
                {
                    Padding = 10,
                    BackgroundColor = Colors.White,
                    BorderColor = Colors.LightGray,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children =
                        {
                            new Label { Text = flow.Name, FontAttributes = FontAttributes.Bold, TextColor = Colors.Black },
                            new Label { Text = summary, FontSize = 12, TextColor = Colors.DarkSlateGray },
                            new HorizontalStackLayout
                            {
                                Spacing = 6,
                                Children =
                                {
                                    CreateMiniButton("Edit", () =>
                                    {
                                        _editingFlowId = flow.Id;
                                        _nameEntry.Text = flow.Name;
                                        _items.Clear();
                                        _items.AddRange(flow.Items.Select(item => new CustomStageFlowItem { StageId = item.StageId }));
                                        RebuildCurrentFlow();
                                    }),
                                    CreateMiniButton("Delete", async () =>
                                    {
                                        bool shouldDelete = await DisplayAlert("Delete flow", $"Delete '{flow.Name}'?", "Delete", "Cancel");
                                        if (!shouldDelete)
                                            return;
                                        await _flowRepository.DeleteByIdAsync(flow.Id);
                                        if (_editingFlowId == flow.Id)
                                            ClearForm();
                                        await RebuildSavedFlowsAsync();
                                    })
                                }
                            }
                        }
                    }
                });
            }
        }

        private void ClearForm()
        {
            _editingFlowId = null;
            _nameEntry.Text = string.Empty;
            _items.Clear();
            RebuildCurrentFlow();
        }

        private Guid? GetCurrentUserId() => ServiceHelper.GetService<CurrentUserSession>().ActiveUser?.Id;

        private static Label CreateTitle(string text) => new() { Text = text, FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Colors.Black };

        private static View CreateLabeled(string label, View view) => new VerticalStackLayout
        {
            Spacing = 4,
            Children = { new Label { Text = label, TextColor = Colors.Black }, view }
        };

        private static Button CreateMiniButton(string text, Action onClick)
        {
            Button button = new() { Text = text, FontSize = 12, Padding = new Thickness(10, 4), BackgroundColor = Colors.MediumPurple, TextColor = Colors.White };
            button.Clicked += (_, __) => onClick();
            return button;
        }

        private static Button CreateMiniButton(string text, Func<Task> onClick)
        {
            Button button = new() { Text = text, FontSize = 12, Padding = new Thickness(10, 4), BackgroundColor = Colors.MediumPurple, TextColor = Colors.White };
            button.Clicked += async (_, __) => await onClick();
            return button;
        }
    }
}
