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
			//new PageConfig(null, "ContentPage", () => new ContentPageXaml { BindingContext = new ContentPageXaml() }),
			//new PageConfig(null, "Layouts", null),
			new PageConfig(null, "new Keyboard", null),
            new PageConfig(null, "Arrow", null),
            new PageConfig(null, "Bits", null),
            new PageConfig(null, "new Number", null),
            //new PageConfig(null, "Keyboard", null),
            //new PageConfig(null, "Number", null),

            new PageConfig(null, "Show Data",  () => new ShowDataXaml { BindingContext = new ViewModels.MarksViewModel() }),
            new PageConfig(null, "Show Data Keyboard",  () => new ShowDataXamlKeyboard { BindingContext = new ViewModels.MarksViewModel() }),
			//new PageConfig(null, "Cells", null),
			new PageConfig(null, "Tests", null),
            new PageConfig(null, userName+"Switch User",  () => new SwitchUserPage { BindingContext = new ViewModels.MarksViewModel() }),

			// Layouts
			new PageConfig("Layouts", "AbsoluteLayout", () => new AbsoluteLayoutXaml { BindingContext = new ViewModels.MarksViewModel() }),
            new PageConfig("Layouts", "ContentView", () => new ContentViewMain { BindingContext = new ViewModels.TransformViewModel() }),
            new PageConfig("Layouts", "FlexLayout", () => new FlexLayoutXaml { BindingContext = new ViewModels.CustomEventArgsViewModel() }),
            new PageConfig("Layouts", "Frame", () => new FrameXaml { BindingContext = new ViewModels.TransformViewModel() }),
            new PageConfig("Layouts", "Grid", () => new GridXaml { BindingContext = new ViewModels.TicTacToeViewModel() }),
            new PageConfig("Layouts", "ScrollView", () => new ScrollViewXaml { BindingContext = new ViewModels.CustomEventArgsViewModel() }),
            new PageConfig("Layouts", "StackLayout", () => new StackLayoutXaml { BindingContext = new ViewModels.TransformViewModel() }),
            new PageConfig("Layouts", "TabbedPage", () => new TabbedPageXaml { BindingContext = new ViewModels.CustomEventArgsViewModel() }),

			// Views
			//new PageConfig("Views", "ActivityIndicator", () => new ActivityIndicatorXaml { BindingContext = new ViewModels.CustomEventArgsViewModel() }),
			//new PageConfig("Views", "Piano Async", () => new BoxViewMain { BindingContext = new ViewModels.TextOnlyViewModel() }),
            //new PageConfig("Keyboard", "Async one number", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true,false,true,false) }),
            new PageConfig("Keyboard", "Sync one number", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true,true,true,false) }),
            new PageConfig("Keyboard", "Sync one number Blind", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true,true,true,false,false) }),
            new PageConfig("Keyboard", "Async decomposition not required new combinations", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true,false,false, false) }),
            new PageConfig("Keyboard", "Sync decomposition not required new combinations", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true,true,false,false) }),
            new PageConfig("Keyboard", "Sync decomposition not required new combinations Blind", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true,true,false,false, false) }),
            new PageConfig("Keyboard", "Async decomposition required new combinations", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true,false,false,true) }),
            new PageConfig("Keyboard", "Sync decomposition required new combinations", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true,true,false,true) }),
            new PageConfig("Keyboard", "Sync decomposition required new combinations Blind", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true,true,false,true, false) }),
			
            //new PageConfig("Piano", "Sync one number", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true,true,false,false) }),

            //new PageConfig("Views", "Piano Sync decomposition one by one", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true) }),
            //new PageConfig("Views", "Piano Sync decomposition one by one 2 layers", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true) }),
			//new PageConfig("Views", "CollectionView", () => new CollectionViewMain { BindingContext = new ViewModels.ListOfObjectsViewModel() }),
			/*new PageConfig("Views", "DatePicker", () => new DatePickerXaml { BindingContext = new ViewModels.ThreeDatesViewModel() }),
			new PageConfig("Views", "Editor", () => new EditorXaml { BindingContext = new ViewModels.ThreeStringsViewModel() }),
			new PageConfig("Views", "Entry", () => new EntryXaml { BindingContext = new ViewModels.ThreeStringsViewModel() }),
			new PageConfig("Views", "Image", () => new ImageXaml { BindingContext = new ViewModels.TransformImageViewModel() }),
			//new PageConfig("Views", "ImageButton", () => new ImageButtonXaml { BindingContext = new ViewModels.ImageButtonViewModel() }),
			new PageConfig("Views", "Label", () => new LabelXaml { BindingContext = new ViewModels.CustomEventArgsViewModel() }),
			new PageConfig("Views", "ListView", () => new ListViewMain { BindingContext = new ViewModels.ListOfStringsViewModel() }),
			new PageConfig("Views", "Picker", () => new PickerXaml { BindingContext = new ViewModels.PickerViewModel() }),
			new PageConfig("Views", "ProgressBar", () => new ProgressBarXaml { BindingContext = new ViewModels.ProgressBarViewModel() }),
			/*new PageConfig("Views", "SearchBar", () => new SearchBarXaml { BindingContext = new ViewModels.SearchBarViewModel() }),
			new PageConfig("Views", "Slider", () => new SliderXaml { BindingContext = new ViewModels.ThreeDoublesViewModel() }),
			new PageConfig("Views", "Stepper", () => new StepperXaml { BindingContext = new ViewModels.ThreeDoublesViewModel() }),
			new PageConfig("Views", "Switch", () => new SwitchXaml { BindingContext = new ViewModels.ThreeBooleansViewModel() }),
			new PageConfig("Views", "TableView", () => new TableViewXaml { BindingContext = new ViewModels.AllCellsViewModel() }),
			new PageConfig("Views", "TimePicker", () => new TimePickerXaml { BindingContext = new ViewModels.ThreeTimesViewModel() }),
			new PageConfig("Views", "WebView", () => new WebViewXaml { BindingContext = new ViewModels.CustomEventArgsViewModel() }),*/

			//TODO:
			//new PageConfig("Number", "Sync one number", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(false,true,true,false) }),
            new PageConfig("Number", "decomposition not required new combinations", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(false,false,false,false) }),
            new PageConfig("Number", "decomposition", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(false,false,false,true) }),
            new PageConfig("Number", "decomposition game", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(false,true,false,true) }),
            //new PageConfig("Views", "Piano Sync decomposition one by one", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true) }),
            //new PageConfig("Views", "Piano Sync decomposition one by one 2 layers", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true) }),
            new PageConfig("Number", "Multiplication", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(false,true,true, true) }),
     /*       new PageConfig("new Keyboard", "Async one number quick", () => new SimpleViewCellsPage(new GameConfig
    {
        KeyboardConfig = new KeyboardConfig
        {
            TextBoxesQuantity = 1,
            SecondsPressingToAnswer=1
        }
    })),*/

            new PageConfig("new Keyboard", "Async one number", () => new SimpleViewCellsPage(new GameConfig
    {
                GameName = "Async one number",
        KeyboardConfig = new KeyboardConfig
        {
            TextBoxesQuantity = 1
        }
    })),

    new PageConfig("new Keyboard", "Async one number From Num to Num", () => new SimpleViewCellsPage(new GameConfig
    {  GameName = "Async one number From Num to Num",
        FromNumToNum = true,
        KeyboardConfig = new KeyboardConfig
        {
            TextBoxesQuantity = 1
        }
    })),

    // Uncomment and update as needed
     new PageConfig("new Keyboard", "Async one number Impose edges", () => new SimpleViewCellsPage(new GameConfig
     {
         GameName = "Async one number Impose edges",
         KeyboardConfig = new KeyboardConfig
         {
             TextBoxesQuantity = 1,
             ImposeEdges = true
         }
     })),

    // new PageConfig("new Keyboard", "Async one number Impose edges From Num To Num", () => new SimpleViewCellsPage(new GameConfig
    // {
    //     KeyboardConfig = new KeyboardConfig
    //     {
    //         TextBoxesQuantity = 1,
    //         ImposeEdges = true,
    //         FromNumToNum = true
    //     }
    // })),

new PageConfig("new Keyboard", "Sync one number Quick", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Sync one number Quick",
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            TextBoxesQuantity = 1,
            SecondsPressingToAnswer=1
        }
    })),

    new PageConfig("new Keyboard", "Sync one number", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Sync one number",
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            TextBoxesQuantity = 1
        }
    })),

    new PageConfig("new Keyboard", "Sync one number Blind", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Sync one number Blind",
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync
        }
    })),

    /*new PageConfig("Arrow", "Arrow Sync one number With Key Numbers", () => new SimpleViewCellsPage(new GameConfig
    {
        UIQuestionType=UIQuestionType.OnlyKeyboard,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true,
            ShowNumbersOnKeys = true
        }
    })),*/
    new PageConfig("Arrow", "Arrow Sync one number ", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Arrow Sync one number ",
         UIQuestionType=UIQuestionType.OnlyKeyboard,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            IsArrow = true
        }
    })),
    new PageConfig("Arrow", "Arrow Sync one number Ordinal With Key Numbers", () => new SimpleViewCellsPage(new GameConfig
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

    /*new PageConfig("Arrow", "Cyclical Right With Key Numbers", () => new SimpleViewCellsPage(new GameConfig
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
    new PageConfig("Arrow", "Cyclical Right", () => new SimpleViewCellsPage(new GameConfig
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
    /*new PageConfig("Arrow", "Cyclical Left With Key Numbers", () => new SimpleViewCellsPage(new GameConfig
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
    new PageConfig("Arrow", "Cyclical Left", () => new SimpleViewCellsPage(new GameConfig
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
    /*new PageConfig("Arrow", "Cyclical Mixed With Key Numbers", () => new SimpleViewCellsPage(new GameConfig
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
    new PageConfig("Arrow", "Cyclical Mixed", () => new SimpleViewCellsPage(new GameConfig
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
    new PageConfig("Arrow", "Cyclical Mixed Ordinal", () => new SimpleViewCellsPage(new GameConfig
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
    /*new PageConfig("Arrow", "From Left With Key Numbers", () => new SimpleViewCellsPage(new GameConfig
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
    new PageConfig("Arrow", "From Left", () => new SimpleViewCellsPage(new GameConfig
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
    new PageConfig("Arrow", "To Left", () => new SimpleViewCellsPage(new GameConfig
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
    new PageConfig("Arrow", "From Left Ordinal", () => new SimpleViewCellsPage(new GameConfig
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
    new PageConfig("Arrow", "To Left Ordinal", () => new SimpleViewCellsPage(new GameConfig
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

    new PageConfig("Bits", "Sync Hand To Keyboard", () => new SimpleViewCellsPage(new GameConfig
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

    new PageConfig("Bits", "Sync Keyboard To Keyboard", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Sync Keyboard To Keyboard",
        UIQuestionType = UIQuestionType.LogicalKeyboards,
        OperationList = GameConfig.Operations.BitArray,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync
        }
    })),
    new PageConfig("Bits", "Logic", () => new SimpleViewCellsPage(new GameConfig
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

   /* new PageConfig("new Keyboard", "Sync one number Blind Impose edges", () => new SimpleViewCellsPage(new GameConfig
    {
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            ImposeEdges = true
        }
    })),
   */
    // Uncomment and update as needed
    // new PageConfig("new Keyboard", "Async decomposition not required new combinations Right hand Left hand", () => new SimpleViewCellsPage(new GameConfig
    // {
    //     VariableTypes = VariableTypes.TwoNoSum,
    //     KeyboardConfig = new KeyboardConfig
    //     {
    //         TextBoxesQuantity = 2
    //     }
    // })),

    // new PageConfig("new Keyboard", "Sync decomposition not required new combinations Right hand Left hand", () => new SimpleViewCellsPage(new GameConfig
    // {
    //     KeyboardConfig = new KeyboardConfig
    //     {
    //        SyncType = SyncType.Sync,
    //        TextBoxesQuantity = 2
    //     }
    // })),

    // new PageConfig("new Keyboard", "Sync decomposition not required new combinations Blind Right hand Left hand", () => new SimpleViewCellsPage(new GameConfig
    // {
    //     KeyboardConfig = new KeyboardConfig
    //     {
    //         SyncType = SyncType.Sync
    //     }
    // })),

    // new PageConfig("new Keyboard", "Async decomposition required new combinations Right hand Left hand", () => new SimpleViewCellsPage(new GameConfig
    // {
    //     KeyboardConfig = new KeyboardConfig
    //     {
    //        SyncType = SyncType.Sync,
    //        TextBoxesQuantity = 2
   //     }
    // })),

    /*new PageConfig("new Keyboard", "Sync decomposition required new combinations Right hand Left hand", () => new SimpleViewCellsPage(new GameConfig
    {
        IsHistory = true,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync,
            TextBoxesQuantity = 2
        }
    })),*/

    new PageConfig("new Keyboard", "Sync decomposition required new combinations Blind Right hand Left hand", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Sync decomposition required new combinations Blind Right hand Left hand",
        IsHistory = true,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Sync
        }
    })),

    // Uncomment and update as needed
    // new PageConfig("new Keyboard", "Async decomposition not required new combinations Full", () => new SimpleViewCellsPage(new GameConfig
    // {
    //     GameType = GameType.FullDecomposition,
    //     IsHistory = false,
    //     KeyboardConfig = new KeyboardConfig
    //     {
    //         SyncType = SyncType.Sync,
    //         TextBoxesQuantity = 2
    //     }
    // })),

    // new PageConfig("new Keyboard", "Sync decomposition not required new combinations Full", () => new SimpleViewCellsPage(new GameConfig
    // {
    //     GameType = GameType.FullDecomposition,
    //     IsHistory = false,
    //     KeyboardConfig = new KeyboardConfig
    //     {
    //        KeysInRow=11,
    //         SyncType = SyncType.Sync,
    //         TextBoxesQuantity = 2
    //     }
    // })),

   /* new PageConfig("new Keyboard", "Sync decomposition not required new combinations Blind Full", () => new SimpleViewCellsPage(new GameConfig
    {
        MaxAddend=10,
        KeyboardConfig = new KeyboardConfig
        {KeysInRow = 11,
            SyncType = SyncType.Spatial
        }
    })),*/

    // Uncomment and update as needed
    // new PageConfig("new Keyboard", "Async decomposition required new combinations Full", () => new SimpleViewCellsPage(new GameConfig
    // {
    //     GameType = GameType.FullDecomposition,
    //     IsHistory = true,
    //     addendsNum = 2,
    //     KeyboardConfig = new KeyboardConfig
    //     {
    //        KeysInRow=11,
    //         SyncType = SyncType.Sync,
    //         TextBoxesQuantity = 2
    //     }
    // })),

    // new PageConfig("new Keyboard", "Sync decomposition required new combinations Full", () => new SimpleViewCellsPage(new GameConfig
    // {
    //     GameType = GameType.FullDecomposition,
    //     IsHistory = true,
    //     addendsNum = 2,
    //     KeyboardConfig = new KeyboardConfig
    //     {
    //        KeysInRow=11,
    //         SyncType = SyncType.Sync,
    //         TextBoxesQuantity = 2
    //     }
    // })),

     new PageConfig("new Keyboard", "Spatial decomposition required new combinations Blind to 5", () => new SimpleViewCellsPage(new GameConfig
    {
         GameName = "Spatial decomposition required new combinations Blind to 5",
         MinAddend=0,
         MinSum=1,
          MaxAddend=5,
        MaxSum=5,
        IsHistory = true,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Spatial
        }
    })),
    new PageConfig("new Keyboard", "Sync decomposition dummies spatial less then 5", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Sync decomposition dummies spatial less then 5",
        MinAddend=0,
        MinSum=1,
        MaxSum = 4,
        MaxAddend=5,
        IsHistory=true,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Spatial,
            DummiesArray = new[] {0,0,0,0,0 },
            LeftAddendIndex=5
        }
    })),
    new PageConfig("new Keyboard", "Sync decomposition spatial more then 5", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Sync decomposition spatial more then 5",
         MinAddend=5,
        MaxAddend=9,
        MinSum=6,
        MaxSum=9,
        MinAddend2 = 0,
        MaxAddend2 = 4,
        IsHistory=true,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Spatial,
            DummiesArray = new[] {1,1,1,1,1 }
        }
    })),

    /*new PageConfig("new Keyboard", "Sync decomposition dummies spatial less then 5 ON", () => new SimpleViewCellsPage(new GameConfig
    {
         MinAddend=1,
         MinSum=2,
        MaxSum = 4,
        MaxAddend=3,
        IsHistory=true,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Spatial,
            DummiesArray = new[] {0,0,0,0,0,1 }
        }
    })),
    new PageConfig("new Keyboard", "Sync decomposition spatial more then 5 ON", () => new SimpleViewCellsPage(new GameConfig
    {
         MinAddend=1,
        MinSum=7,
        MaxSum=9,
        MaxAddend=8,
        IsHistory=true,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.Spatial,
            DummiesArray = new[] {1,-1,-1,-1,-1, 1,1,1,1,1 }
        }
    })),*/
    new PageConfig("new Keyboard", "Spatial decomposition required new combinations Blind Full", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Spatial decomposition required new combinations Blind Full",
        MinAddend=0,
        MaxAddend=9,
        MaxSum= 9,
        MinSum=1,
        IsHistory = true,
        KeyboardConfig = new KeyboardConfig
        {
            KeysInRow=10,
            SyncType = SyncType.Spatial
        }
    })),

    new PageConfig("new Keyboard", "Sync decomposition required new combinations Blind Full Impose Edges", () => new SimpleViewCellsPage(new GameConfig
    {

        GameName = "Sync decomposition required new combinations Blind Full Impose Edges",
        MaxAddend=10,
        IsHistory = true,
        KeyboardConfig = new KeyboardConfig
        {
            KeysInRow=11,
            SyncType = SyncType.Sync,
            ImposeEdges = true
        }
    })),

    /*new PageConfig("new Keyboard", "HSync decomposition required new combinations Blind Full", () => new SimpleViewCellsPage(new GameConfig
    {
        IsHistory = true,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.HalfSync,
            WithoutZero = false
        }
    })),*/

    new PageConfig("new Keyboard", "HSync decomposition required new combinations", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName =  "HSync decomposition required new combinations",
        IsHistory = true,
        MinAddend=1,
        MinSum=2,
        MaxAddend=8,
        MaxSum=9,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.HalfSync,
            WithoutZero=true

        }
    })),

    new PageConfig("new Keyboard", "HSync decomposition required new combinations Impose Edges", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "HSync decomposition required new combinations Impose Edges",
        IsHistory = true,
        MinAddend=1,
        MinSum=2,
        MaxAddend=9,
        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.HalfSync,
            WithoutZero=true,
            ImposeEdges = true
        }
    })),

    new PageConfig("new Number", "decomposition not required new combinations(up to 5)", () => new SimpleViewCellsPage(new GameConfig
        {
        GameName = "decomposition not required new combinations(up to 5)"
        })),

    new PageConfig("new Number", "decomposition(up to 5)", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "decomposition(up to 5)",
        IsHistory = true
    })),

    new PageConfig("new Number", "decomposition not required new combinations(up to 10)", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "decomposition not required new combinations(up to 10)",
        MaxAddend=10
    })),

    new PageConfig("new Number", "decomposition(up to 10)", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "decomposition(up to 10)",
        MaxAddend=10,
        IsHistory = true
    })),

    new PageConfig("new Number", "Till 20 with sync keyboard help", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Till 20 with sync keyboard help",
        MaxAddend=20, MaxSum=20, VariableTypes= VariableTypes.OneCanBeSum,
        KeyboardConfig = new KeyboardConfig()
        {
            SyncType= SyncType.Sync,
            KeyboardOnlyForHelp = true
        }
    })),

    new PageConfig("new Number", "Till 100 with Helping TextBoxes", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Till 100 with Helping TextBoxes",
        MaxAddend=100, MaxSum=100, VariableTypes= VariableTypes.OneCanBeSum, isHelpEntries=true,
        OnlyThrougTen = true
    })),

    new PageConfig("new Number", "decomposition game Through 10 With keyboard Only Yellow", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "decomposition game Through 10 With keyboard Only Yellow",
        MaxAddend=20, MaxSum=20, VariableTypes= VariableTypes.OneCanBeSum, OnlyThrougTen= true,
        KeyboardConfig = new KeyboardConfig()
        {
            Rows = 2,
            KeyboardOnlyForHelp = true
        }
    })),

    new PageConfig("new Number", "decomposition game Through 10 With keyboard HalfSync", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "decomposition game Through 10 With keyboard HalfSync",
        MaxAddend=20, MaxSum=20, VariableTypes= VariableTypes.OneCanBeSum, OnlyThrougTen=true,

        KeyboardConfig = new KeyboardConfig
        {
            SyncType = SyncType.HalfSync,
            Rows=2,
            AddendsNum = 3,
            AllowRemoval = true,
            KeyboardOnlyForHelp = true
        }
    })),

    /*new PageConfig("new Number", "decomposition game Full With keyboard", () => new SimpleViewCellsPage(new GameConfig
    {
        GameType = GameType.DecompositionGame,
        MaxAddend=20, MaxSum=20, VariableTypes= VariableTypes.OneCanBeSum,
        KeyboardConfig = new KeyboardConfig()
    })),
    */
    new PageConfig("new Number", "decomposition game", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "decomposition game",
        UIQuestionType = UIQuestionType.DecompositionGame,
        VariableTypes = VariableTypes.OneCanBeSum
    })),

    // Uncomment and update as needed
    // new PageConfig("Views", "Piano Sync decomposition one by one", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true) }),
    // new PageConfig("Views", "Piano Sync decomposition one by one 2 layers", () => new SimpleViewCellsPage(new GameConfig
    // {
    //     IsHistory = true,
    //     addendsNum = 2,
    //     KeyboardConfig = new KeyboardConfig
    //     {
    //         SyncType = SyncType.Sync
    //     }
    // })),
     new PageConfig("new Number", "Basic Addition To ten with rules", () => new SimpleViewCellsPage(new GameConfig
    {
         GameName = "Basic Addition To ten with rules",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 9,
        MaxSum=10,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 1,
        OnlyCloseTriad = true,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=3
    })),
    new PageConfig("new Number", "Addition To ten Memorize Game", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Addition To ten Memorize Game",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 9,
        MaxSum=10,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 3,
        NumberOfTasksToWin=60,
        NumberOfMistakesToLose=5
    })),
    new PageConfig("new Number", "Addition Memorize Game", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Addition Memorize Game",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 9,
        MaxSum=20,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 3,
        NumberOfTasksToWin=60,
        NumberOfMistakesToLose=5
    })),

    new PageConfig("new Number", "Addition till 200 Game", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Addition till 200 Game",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 198,
        MaxSum=200,
        VariableTypes = VariableTypes.OneCanBeSum,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=4
    })),
    new PageConfig("new Number", "Addition for 180 Game", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Addition for 180 Game",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 178,
        MinSum = 180,
        MaxSum=180,
        VariableTypes = VariableTypes.OneNoSum,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=4
    })),
    new PageConfig("new Number", "Addition for 90 Game", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Addition for 90 Game",
        OperationList = new() { Operation.Sum},
        MinAddend = 2,
        MaxAddend = 88,
        MinSum = 90,
        MaxSum=90,
        VariableTypes = VariableTypes.OneNoSum,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=4
    })),
    new PageConfig("new Number", "Multiplication Memorize Game", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Multiplication Memorize Game",
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
    new PageConfig("new Number", "Multiplicators", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Multiplicators",
        OperationList = new() { Operation.Multiplication},
        MinAddend = 2,
        MaxAddend = 9,
        MaxSum=100,
        IsHistory=true,
        IsHistorySymetrical=true,
        VariableTypes = VariableTypes.TwoNoSum,
        EnforceOperationLabel=true
    })),
    new PageConfig("new Number", "Multiplication Negatives Memorize Game", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Multiplication Negatives Memorize Game",
        OperationList = new() { Operation.Multiplication},
         MinAddend = -10,
        MaxAddend = 10,
        MinSum = -100,
        MaxSum=100,
        VariableTypes = VariableTypes.OneCanBeSum,
        RepeatingTimesOfTriad = 3,
        NumberOfTasksToWin=60,
        NumberOfMistakesToLose=5,
        EnforceOperationLabel=true
    })),
    new PageConfig("new Number", "Multiplication till 50*10", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Multiplication till 50*10",
        OperationList = new() { Operation.Multiplication},
         MinAddend = 2,
        MaxAddend = 11,
        MaxAddend2 = 50,
        MinSum = 4,
        MaxSum=500,
        VariableTypes = VariableTypes.OneCanBeSum,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=4,
        EnforceOperationLabel=true
    })),
    new PageConfig("new Number", "Mixed Addition Multiplication Negatives", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName =  "Mixed Addition Multiplication Negatives",
        OperationList ={  Operation.Sum, Operation.Multiplication},
        MinAddend = -10,
        MaxAddend = 10,
        MinSum = -100,
        MaxSum=100,
        VariableTypes = VariableTypes.OneCanBeSum
    })),
    new PageConfig("new Number", "Equation Only Addition Multiplication Negatives(No division or subtraction)", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Equation Only Addition Multiplication Negatives(No division or subtraction)",
        UIQuestionType= UIQuestionType.SimpleEquation,
        OperationList ={  Operation.Sum, Operation.Multiplication},
        MinAddend = -10,
        MaxAddend = 10,
        MinSum = -100,
        MaxSum=100,
        VariableTypes = VariableTypes.OneCanBeSum
    })),
    new PageConfig("new Number", "Equations Full", () => new SimpleViewCellsPage(new GameConfig
    {
        GameName = "Equations Full",
        UIQuestionType= UIQuestionType.SimpleEquation,
        OperationList =GameConfig.Operations.Arithmetic,
        MinAddend = -10,
        MaxAddend = 10,
        MinSum = -100,
        MaxSum=100,
        VariableTypes = VariableTypes.OneCanBeSum,
        NumberOfTasksToWin=20,
        NumberOfMistakesToLose=5
    })),

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
        private readonly IUserRepository _userRepo;
        private static string userName = "";
        public MainPage()
        {
            InitializeComponent();
            // If you want constructor injection, you can do that;
            // for demonstration, let's just fetch from service provider:
            _userRepo = ServiceHelper.GetService<IUserRepository>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var users = await _userRepo.GetUsersAsync();
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
                    var user = await _userRepo.GetUserAsync(currentUserId.Value);
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
            _userRepo = ServiceHelper.GetService<IUserRepository>();
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
