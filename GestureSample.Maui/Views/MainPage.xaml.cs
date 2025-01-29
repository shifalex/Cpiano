using GestureSample.Maui;
using GestureSample.Views.Tests;
using GestureSample.Maui.Data;
using GestureSample.Maui.Handlers;
using GestureSample.Maui.Views;
using GestureSample.Views;
using GestureSample.Maui.Models;
using MongoDB.Driver.Core.Authentication;

namespace GestureSample.Views
{
    public partial class MainPage
    {
        private PageConfig[] AllPages = new PageConfig[]
        {
			// main page
            new PageConfig(null, "->", null),
            new PageConfig(null, "+ -", null),
            new PageConfig(null, "X : ", null),
            new PageConfig(null, "+-X:- mixed advanced ", null),
            new PageConfig(null, "&& ||", null),
            new PageConfig(null, "Data", null),

            new PageConfig(null, string.Format("Switch User({0})",ActiveUserHelper.CurrentUserName),  () => new SwitchUserPage { BindingContext = new ViewModels.MarksViewModel() }),

            new PageConfig(null, "Tutorial", null),

            new PageConfig("Data", "Show Data",  () => new ShowDataXaml { BindingContext = new ViewModels.MarksViewModel() }),
            new PageConfig("Data", "Show Data Keyboard",  () => new ShowDataXamlKeyboard { BindingContext = new ViewModels.MarksViewModel() }),

            // Views

            #region Tutorial
            new PageConfig("Tutorial", "one number Quick", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "one number Quick",
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            TextBoxesQuantity = 1,
            SecondsPressingToAnswer=1
        }
    })),

    new PageConfig("Tutorial", "one number", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "one number",
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            TextBoxesQuantity = 1
        }
    })),

    new PageConfig("Tutorial", "one number Blind", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "one number Blind",
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync
        }
    })),

   
    new PageConfig("Tutorial", "Arrow ", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "one number ",
         UIQuestionType=UIQuestionType.OnlyKeyboard,
         MaxSum=10,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true
        }
    })),
    new PageConfig("Tutorial", "+- Till 20 with sync keyboard help", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Till 20 with sync keyboard help",
        MaxAddend=20, MaxSum=20, VariableTypes= VariableTypes.OneCanBeSum,
        KeyboardConfig = new KeyboardConfig()
        {
            SyncType= SyncType.Sync,
            KeyboardOnlyForHelp = true
        }
    })),
            #endregion

            #region Arrow
            new PageConfig("->", "Ordinal With Key Numbers", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Arrow Sync one number Ordinal With Key Numbers",
         UIQuestionType=UIQuestionType.OnlyKeyboard,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true,
            ShowNumbersOnKeys = true,
            ArrowType=ArrowType.Rounded,
            SecondsPressingToAnswer=1
        }
    })),

    /*new PageConfig("->", "Cyclical Right With Key Numbers", () => new SimpleViewCellsPage(new GameConfig
    {
        UIQuestionType=UIQuestionType.OnlyKeyboard,
        QuestionOrder = QuestionOrder.CyclicalRight,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true,
            ShowNumbersOnKeys = true
        }
    })),*/
    new PageConfig("->", "Cyclical->", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Cyclical Right",
         UIQuestionType=UIQuestionType.OnlyKeyboard,
        QuestionOrder = QuestionOrder.CyclicalRight,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true
        }
    })),
    /*new PageConfig("->", "Cyclical Left With Key Numbers", () => new SimpleViewCellsPage(new GameConfig
    {
        UIQuestionType=UIQuestionType.OnlyKeyboard,
        QuestionOrder = QuestionOrder.CyclicalLeft,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true,
            ShowNumbersOnKeys = true
        }
    })),*/
    new PageConfig("->", "<-Cyclical", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Cyclical Left",
         UIQuestionType=UIQuestionType.OnlyKeyboard,
        QuestionOrder = QuestionOrder.CyclicalLeft,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true
        }
    })),
    /*new PageConfig("->", "Cyclical Mixed With Key Numbers", () => new SimpleViewCellsPage(new GameConfig
    {
        UIQuestionType=UIQuestionType.OnlyKeyboard,
        QuestionOrder = QuestionOrder.CyclicalMixed,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true,
            ShowNumbersOnKeys = true
        }
    })),*/
    new PageConfig("->", "<-Cyclical->", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Cyclical Mixed",
        UIQuestionType=UIQuestionType.OnlyKeyboard,
        QuestionOrder = QuestionOrder.CyclicalMixed,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true
        }
    })),
    new PageConfig("->", "Cyclical Mixed Ordinal", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Cyclical Mixed Ordinal",
        UIQuestionType=UIQuestionType.OnlyKeyboard,
        QuestionOrder = QuestionOrder.CyclicalMixed,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true,
            ShowNumbersOnKeys = true,
            ArrowType=ArrowType.Rounded,
            SecondsPressingToAnswer=1
        }
    })),
    /*new PageConfig("->", "From Left With Key Numbers", () => new SimpleViewCellsPage(new GameConfig
    {
        UIQuestionType=UIQuestionType.OnlyKeyboard,
        QuestionOrder = QuestionOrder.FromLeft,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true,
            ShowNumbersOnKeys = true
        }
    })),*/
    new PageConfig("->", "-> ->", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "From Left",
        UIQuestionType=UIQuestionType.OnlyKeyboard,
        QuestionOrder = QuestionOrder.FromLeft,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true
        }
    })),
    new PageConfig("->", "<- <-", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName =  "To Left",
        UIQuestionType=UIQuestionType.OnlyKeyboard,
        QuestionOrder = QuestionOrder.ToLeft,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true
        }
    })),
    new PageConfig("->", "From Left Ordinal", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "From Left Ordinal",
        UIQuestionType=UIQuestionType.OnlyKeyboard,
        QuestionOrder = QuestionOrder.FromLeft,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true,
            ShowNumbersOnKeys = true,
            ArrowType=ArrowType.Rounded
        }
    })),
    new PageConfig("->", "To Left Ordinal", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "To Left Ordinal",
        UIQuestionType=UIQuestionType.OnlyKeyboard,
        QuestionOrder = QuestionOrder.ToLeft,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true,
            ShowNumbersOnKeys = true,
            ArrowType=ArrowType.Rounded,
            SecondsPressingToAnswer=1
        } 
    })),
            #endregion

            #region Logic
            new PageConfig("&& ||", "Sync Hand To Keyboard", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Sync Hand To Keyboard",
        UIQuestionType = UIQuestionType.CanvasesHands,
        OperationList = new (){  Operation.Copy, Operation.Quantity },
        SecondsTillHideExercise = 2,
        SecondsTillAllowInput = 4,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync
        }
    })),

    new PageConfig("&& ||", "Sync Keyboard To Keyboard", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Sync Keyboard To Keyboard",
        UIQuestionType = UIQuestionType.LogicalKeyboards,
        OperationList = GameConfig.Operations.BitArray,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync
        }
    })),
    new PageConfig("&& ||", "Logic", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Logic",
        UIQuestionType = UIQuestionType.LogicalKeyboards,
        OperationList = GameConfig.Operations.Logical.Concat(GameConfig.Operations.BitArray).ToList(),
        OnlyToTen = true,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync
        }
    })),


            #endregion

            #region +-
        
    new PageConfig("+ -", "Level 1   - Sum<10", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 1",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 9,
        MaxSum=10,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 3,
        NumberOfTasksToWin=60,
        NumberOfMistakesToLose=5
    })),
     new PageConfig("+ -", "Level 1.1 - No repetition", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 1.1",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 9,
        MaxSum=10,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 1,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=3
    })),
    new PageConfig("+ -", "Level 2   - Addend<10", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 2",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 9,
        MaxSum=20,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 3,
        NumberOfTasksToWin=60,
        NumberOfMistakesToLose=5
    })),
    new PageConfig("+ -", "Level 2.1 - No repetition", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 2.1",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 9,
        MaxSum=20,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 1,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=3
    })),
    new PageConfig("+ -", "Level 2.2 - Only throuh 10 with Helping text boxes", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 2.2",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 9,
        MaxSum=20,
        OnlyThrougTen = true,
        VariableTypes = VariableTypes.OneCanBeSum,
        isHelpEntries = true,
        RepeatingTimesOfTriad = 1,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=3
    })),
    new PageConfig("+ -", "Level 2.3 - Only throuh 10", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 2.3",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 9,
        MaxSum=20,
        OnlyThrougTen = true,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 1,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=3
    })),

    new PageConfig("+ -", "Level 3   - Sum<100, Helping text boxes", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 3",
        MaxAddend=100, MaxSum=100, VariableTypes= VariableTypes.OneCanBeSum, isHelpEntries=true,
        OnlyThrougTen = true
    })),
     new PageConfig("+ -", "Level 3.1 - Relative rules", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 3.1",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 98,
        MaxSum=100,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 1,
        OnlyCloseTriad = true,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=3
    })),
    new PageConfig("+ -", "Level 3.2 - free practice", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 3.2",
        MaxAddend=100, MaxSum=100, VariableTypes= VariableTypes.OneCanBeSum, isHelpEntries=false,
        OnlyThrougTen = true
    })),

    new PageConfig("+ -", "Level 4   - Sum<200", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 4",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 198,
        MaxSum=200,
        VariableTypes = VariableTypes.OneCanBeSum,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=4
    })),
    new PageConfig("+ -", "Sum = 90", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Sum=90",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 88,
        MinSum = 90,
        MaxSum=90,
        VariableTypes = VariableTypes.OneNoSum,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=4
    })),
    new PageConfig("+ -", "Sum = 180", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Sum =180",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 178,
        MinSum = 180,
        MaxSum=180,
        VariableTypes = VariableTypes.OneNoSum,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=4
    })),
            #endregion
#region mult and mixed
            new PageConfig("X : ", "Level 1   - Multiplication Table Memorize", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Multiplication - Level 1",
        OperationList = new() { Operation.Multiplication},
        MinAddend = 2,
        MaxAddend = 9,
        MaxSum=100,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 3,
        NumberOfTasksToWin=60,
        NumberOfMistakesToLose=5,
        EnforceOperationLabel=true
    })),
            new PageConfig("X : ", "Level 1.1 - No repetition", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Multiplication - Level 1.1",
        OperationList = new() { Operation.Multiplication},
        MinAddend = 2,
        MaxAddend = 9,
        MaxSum=100,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 1,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=3,
        EnforceOperationLabel=true
    })),
    new PageConfig("X : ", "Level2   - Multiplicators(2 to 10) of Multiplication Table", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Multiplication - Level 2",
        OperationList = new() { Operation.Multiplication},
        MinAddend = 2,
        MaxAddend = 10,
        MaxSum=100,
        IsHistory=true,
        IsHistorySymetrical=true,
        VariableTypes = VariableTypes.TwoNoSum,
        EnforceOperationLabel=true
    })),
    new PageConfig("X : ", "Level 3   - (-)Negatives", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Multiplication - Level 3",
        OperationList = new() { Operation.Multiplication},
         MinAddend = -10,
        MaxAddend = 10,
        MinSum = -100,
        MaxSum=100,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 1,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=3,
        EnforceOperationLabel=true
    })),
    new PageConfig("X : ", "Level 4   - Multiplication till 50*10", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Multiplication - Level 4",
        OperationList = new() { Operation.Multiplication},
         MinAddend = 2,
        MaxAddend = 11,
        MaxAddend2 = 50,
        MinSum = 4,
        MaxSum=500,
        VariableTypes = VariableTypes.OneCanBeSum,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=3,
        EnforceOperationLabel=true
    })),
    new PageConfig("+-X:- mixed advanced ", "Level 1 - Mixed Addition Multiplication Negatives", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName =  "Mixed - Level 1",
        OperationList ={  Operation.Sum, Operation.Multiplication},
        MinAddend = -10,
        MaxAddend = 10,
        MinSum = -100,
        MaxSum=100,
        VariableTypes = VariableTypes.OneCanBeSum,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=3
    })),
    new PageConfig("+-X:- mixed advanced ", "Level 2 - Equation Addition Multiplication Negatives (No division or subtraction)", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Mixed - Level 2",
        UIQuestionType= UIQuestionType.SimpleEquation,
        OperationList ={  Operation.Sum, Operation.Multiplication},
        MinAddend = -10,
        MaxAddend = 10,
        MinSum = -100,
        MaxSum=100,
        VariableTypes = VariableTypes.OneCanBeSum,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=3
    })),
    new PageConfig("+-X:- mixed advanced ", "Level 3 - Equations Full", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Mixed - Level 3",
        UIQuestionType= UIQuestionType.SimpleEquation,
        OperationList =GameConfig.Operations.Arithmetic,
        MinAddend = -10,
        MaxAddend = 10,
        MinSum = -100,
        MaxSum=100,
        VariableTypes = VariableTypes.OneCanBeSum,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=3
    })),
#endregion 

            // Cells
            new PageConfig("Cells", "TextCell", () => new TextCellMain { BindingContext = new ViewModels.ListOfStringsViewModel() }),
            new PageConfig("Cells", "ImageCell", () => new ImageCellMain { BindingContext = new ViewModels.ListOfObjectsViewModel() }),
            new PageConfig("Cells", "All Cells", () => new AllCellsXaml { BindingContext = new ViewModels.AllCellsViewModel() }),

			// Tests
			new PageConfig("Tests", "Clear in Handler", () => new DisposeInHandlerPage()),
            new PageConfig("Tests", "Horizontal ScrollView", () => new HorizontalScrollViewXaml { BindingContext = new ViewModels.CustomEventArgsViewModel() }),
            new PageConfig("Tests", "BigButton", () => new BigButtonPage()),
			//new PageConfig("Tests", "ViewCells", () => new SimpleViewCellsPage(GameType.SimpleDecomposition)),
			new PageConfig("Tests", "Custom ListView", () => new CustomListViewPage { BindingContext = new ViewModels.ListOfStringsViewModel() }),
            new PageConfig("Tests", "ScrollView with Images", () => new ScrollViewWithImages { BindingContext = new ViewModels.CustomEventArgsViewModel() }),
            new PageConfig("Tests", "InputTransparent", () => new InputTransparent { BindingContext = new ViewModels.TextOnlyViewModel() }),
            new PageConfig("Tests", "Simple LongPress", () => new SimpleLongPress()),
            new PageConfig("Tests", "Page and ListView", () => new PageAndListView { BindingContext = new ViewModels.ListOfStringsViewModel() }),
			//new PageConfig("Tests", "Custom Button", () => new CustomButtonPage() { BindingContext = new ViewModels.ButtonViewModel() }),
			new PageConfig("Tests", "Nested Controls", () => new NestedControls()),
            new PageConfig("Tests", "BottomTabbedPage", () => new BottomTabbedPageXaml { BindingContext = new ViewModels.CustomEventArgsViewModel() }),
            new PageConfig("Tests", "Delete Bound Items", () => new DeleteBoundItems { BindingContext = new ViewModels.Tests.DeleteBoundItemsViewModel() }),
            new PageConfig("Tests", "Scaling X and Y seperately", () => new FrameScaleXYXaml { BindingContext = new ViewModels.TransformViewModel() }),
            new PageConfig("Tests", "Dynamically add Event handler", () => new DynamicallyAddHandler { BindingContext = new ViewModels.CustomEventArgsViewModel() }),
            new PageConfig("Tests", "Test first panning args", () => new PrintFirstPanning { BindingContext = new ViewModels.CustomEventArgsViewModel() }),
            new PageConfig("Tests", "Invisible Button", () => new InvisibleButtonPage()),
            new PageConfig("Tests", "Draggable Items on TabbedPage", () => new DraggableItemsOnTabbedPage { BindingContext = new ViewModels.Tests.DraggableItemsViewModel() }),
            new PageConfig("Tests", "Drag&Drop Items in FlexLayout", () => new DragAndDropPage { BindingContext = new ViewModels.Tests.DragAndDropViewModel() })
        };


        #region MainPage code
        private readonly UserRepository _userRepo;
        private static string userName = "";
        public MainPage()
        {
            InitializeComponent();
            // If you want constructor injection, you can do that;
            // for demonstration, let's just fetch from service provider:
            _userRepo = ServiceHelper.GetService<UserRepository>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var users = await _userRepo.GetAllAsync();
            if (users == null || !users.Any())
            {
                // No users exist, navigate to SplashPage
                await Navigation.PushAsync(new SplashPage(_userRepo));
            }
            else
            {
                // Handle cases where users exist
                var currentUserId = ActiveUserHelper.CurrentUserId;
                if (currentUserId.HasValue)
                {
                    var user = await _userRepo.GetByIdAsync(currentUserId.Value);
                    if (user != null)
                    {
                        // Optionally display a welcome message
                        //WelcomeLabel.Text = $"Welcome, {user.Name}!";
                        userName = user.Name;
                        return;
                    }
                }

                // Default message if no active user is set

                userName = "No active user found.";
            }
        }

            private async void OnSwitchUserClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GestureSample.Maui.Views.SwitchUserPage());
        }

        public MainPage(string title, IEnumerable<PageConfig> contents)
        {
            Title = title;
            contents ??= AllPages.Where(pc => pc.Parent == null);
            BindingContext = contents;

            InitializeComponent();
            _userRepo = ServiceHelper.GetService<UserRepository>();
        }

        private async void ListItem_Tapped(object sender, ItemTappedEventArgs e)
        {
            var item = (PageConfig)e.Item;

            try
            {
                if (item.PageConstructor != null)
                {
                    // a sample page
                    var page = item.PageConstructor.Invoke();
                    await App.MainNavigation.PushAsync(page);
                }
                else
                {
                    // a menu page
                    var subpage = item.Title;
                    var contents = AllPages.Where(pc => pc.Parent == subpage);
                    var page = new MainPage(subpage, contents);
                    await App.MainNavigation.PushAsync(page);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        #endregion

        #region class PageConfig

        public class PageConfig
        {
            public string Parent { get; }
            public string Title { get; }
            public Func<Page> PageConstructor { get; }

            public PageConfig(string parent, string title, Func<Page> pageConstructor)
            {
                Parent = parent;
                Title = title;
                PageConstructor = pageConstructor;
            }
        }

        #endregion class PageConfig
    }
}
