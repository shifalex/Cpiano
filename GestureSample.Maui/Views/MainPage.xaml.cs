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
            new PageConfig(null, "->", null, true),
            new PageConfig(null, "+ -", null),
            new PageConfig(null, "X : ", null),
            new PageConfig(null, "+-X:- mixed advanced ", null),
            new PageConfig(null, "&& ||", null, true),
            new PageConfig(null, "Data", null),

            new PageConfig(null, string.Format("Switch User({0})",ServiceHelper.GetService<CurrentUserSession>().ActiveUser==null?"":ServiceHelper.GetService<CurrentUserSession>().ActiveUser.Name),  () => new SwitchUserPage { BindingContext = new ViewModels.MarksViewModel() }),

            new PageConfig(null, "Tutorial", null, true),

            new PageConfig("Data", "Show Data",  () => new ShowDataXaml { BindingContext = new ViewModels.MarksViewModel() }),
            new PageConfig("Data", "Show Data Keyboard",  () => new ShowDataXamlKeyboard { BindingContext = new ViewModels.MarksViewModel() }, true),

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
        NumberOfTasksToWin=40,
        NumberOfMistakesToLose=5
    })),
    new PageConfig("+ -", "Level 2.2 - Only through 10, Helping text boxes", () => new SimpleViewCellsPage(new GameConfig
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
        NumberOfTasksToWin=30,
        NumberOfMistakesToLose=4
    })),
    new PageConfig("+ -", "Level 2.3 - Only through 10", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 2.3",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 9,
        MaxSum=20,
        OnlyThrougTen = true,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 1,
        NumberOfTasksToWin=30,
        NumberOfMistakesToLose=4
    })),
    new PageConfig("+ -", "Level 3   - BIG+small, Only through, Helping text boxes", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 3",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 9,
        MinAddend2 = 12,
        MaxAddend2 = 89,
        MaxSum=100,
        OnlyThrougTen = true,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 1,
        NumberOfTasksToWin=30,
        NumberOfMistakesToLose=4,
        isHelpEntries=true
    })),
    new PageConfig("+ -", "Level 3.1 - BIG+small, Only through", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 3.1",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 9,
        MinAddend2 = 12,
        MaxAddend2 = 89,
        MaxSum=100,
        OnlyThrougTen = true,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 1,
        NumberOfTasksToWin=30,
        NumberOfMistakesToLose=4
    })),
    new PageConfig("+ -", "Level 3.2 - Sum<100, Helping text boxes", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 3.2",
        MaxAddend=100, 
        MaxSum=100, 
        VariableTypes= VariableTypes.OneCanBeSum, 
        isHelpEntries=true,
        OnlyThrougTen = true
    })),
    /* new PageConfig("+ -", "Level 3.3 - Relative rules", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 3.3",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 98,
        MaxSum=100,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 1,
        OnlyCloseTriad = true,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=3
    })),*/
    new PageConfig("+ -", "Level 3.3 - free practice", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Level 3.4",
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
        NumberOfTasksToWin=40,
        NumberOfMistakesToLose=5,
        EnforceOperationLabel=true
    })),
    new PageConfig("X : ", "Level2   - Multiplicators(2 to 9) of Multiplication Table", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Multiplication - Level 2",
        OperationList = new() { Operation.Multiplication},
        MinAddend = 2,
        MaxAddend = 9,
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

        };


        #region MainPage code
        private readonly UserRepository _userRepo;
        private double _screenSize;

        public MainPage(string title, IEnumerable<PageConfig> contents)
        {
            _userRepo = ServiceHelper.GetService<UserRepository>();
            var displayInfo = DeviceDisplay.MainDisplayInfo;
            double widthInches = displayInfo.Width / displayInfo.Density;
            double heightInches = displayInfo.Height / displayInfo.Density;
            _screenSize = Math.Sqrt(Math.Pow(widthInches, 2) + Math.Pow(heightInches, 2));
            if (ServiceHelper.GetService<CurrentUserSession>() == null || ServiceHelper.GetService<CurrentUserSession>().ActiveUser == null)
                Navigation.PushAsync(new SplashPage());
            
           /* if (Navigation.NavigationStack.Count ==0 &&)
                while (Navigation.NavigationStack.Count > 2)
                {

                    Console.WriteLine(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2].Title);
                    var previousPage = Navigation.NavigationStack[Navigation.NavigationStack.Count - 2];
                    Navigation.RemovePage(previousPage);
                }
            }*/


            Title = title;
            contents ??= AllPages.Where(pc => pc.Parent == null && (_screenSize>=1100 || !pc.IsLargeScreenOnly));
            BindingContext = contents;

            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            // Check how many pages are in the navigation stack
            int pageCount = Navigation.NavigationStack.Count;

            //TODO: change to work without the title hack. or at least make a Main Title string
            if (pageCount > 1 && Title!= "Control Categories")
            {
                // We are NOT on the root page; 
                // use the former (default) back function:
                return base.OnBackButtonPressed();
            }
            else
            {
                // We ARE on the root page (or there's only 1 page),
                // so do custom logic—e.g., exit on Android/Windows:
#if ANDROID
        Platform.CurrentActivity?.FinishAffinity(); 
#elif WINDOWS
        System.Environment.Exit(0);
#endif

                // Return true to indicate we have handled it 
                // (i.e., do NOT pop any page).
                return true;
            }
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
                    await Navigation.PushAsync(page);
                }
                else
                {
                    // a menu page
                    var subpage = item.Title;
                    var contents = AllPages.Where(pc => pc.Parent == subpage && (_screenSize >=1100 || !pc.IsLargeScreenOnly));
                    var page = new MainPage(subpage, contents);
                    await Navigation.PushAsync(page);
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

            public bool IsLargeScreenOnly { get; }

            public PageConfig(string parent, string title, Func<Page> pageConstructor, bool largeScreenOnly = false)
            {
                Parent = parent;
                Title = title;
                PageConstructor = pageConstructor;
                IsLargeScreenOnly = largeScreenOnly;
            }
        }

        #endregion class PageConfig
    }
}
