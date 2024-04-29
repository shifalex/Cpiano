using GestureSample.Maui;
using GestureSample.Views.Tests;

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
            new PageConfig(null, "new Number", null),
            new PageConfig(null, "Keyboard", null),
            new PageConfig(null, "Number", null),

            new PageConfig(null, "Show Data",  () => new ShowDataXaml { BindingContext = new ViewModels.MarksViewModel() }),
			//new PageConfig(null, "Cells", null),
			new PageConfig(null, "Tests", null),

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
            new PageConfig("Keyboard", "Async one number", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true,false,true,false) }),
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
			new PageConfig("new Keyboard", "Async one number", () => new SimpleViewCellsPage(GameType.GuessOne,false,true,false,true)) ,
            new PageConfig("new Keyboard", "Async one number From Num to Num", () => new SimpleViewCellsPage(GameType.GuessOne,false,true,false,true,false,true)) ,
            new PageConfig("new Keyboard", "Async one number Impose edges", () => new SimpleViewCellsPage(GameType.GuessOne,false,true,false,true,true)) ,
            new PageConfig("new Keyboard", "Async one number Impose edges From Num To Num", () => new SimpleViewCellsPage(GameType.GuessOne,false,true,false,true,true,true)) ,

            new PageConfig("new Keyboard", "Sync one number", () => new SimpleViewCellsPage(GameType.GuessOne,false,true, true,true)),
            new PageConfig("new Keyboard", "Sync one number Blind", () => new SimpleViewCellsPage(GameType.GuessOne,false,true, true)),
            new PageConfig("new Keyboard", "Sync one number Blind Impose edges", () => new SimpleViewCellsPage(GameType.GuessOne,false,true, true, false, true)),

            new PageConfig("new Keyboard", "Async decomposition not required new combinations  Right hand Left hand", () => new SimpleViewCellsPage(GameType.SimpleDecompositionGame,false,true,false,true)),
            new PageConfig("new Keyboard", "Sync decomposition not required new combinations  Right hand Left hand", () => new SimpleViewCellsPage(GameType.SimpleDecompositionGame,false,true,true,true)),
            new PageConfig("new Keyboard", "Sync decomposition not required new combinations Blind  Right hand Left hand", () => new SimpleViewCellsPage(GameType.SimpleDecompositionGame,false,true,true)),
            new PageConfig("new Keyboard", "Async decomposition required new combinations  Right hand Left hand", () => new SimpleViewCellsPage(GameType.SimpleDecompositionGame,true,true,false,true)),
            new PageConfig("new Keyboard", "Sync decomposition required new combinations Right hand Left hand", () => new SimpleViewCellsPage(GameType.SimpleDecompositionGame,true,true,true,true)),
            new PageConfig("new Keyboard", "Sync decomposition required new combinations Blind  Right hand Left hand", () => new SimpleViewCellsPage(GameType.SimpleDecompositionGame,true,true,true)),
            new PageConfig("new Keyboard", "Async decomposition not required new combinations Full", () => new SimpleViewCellsPage(GameType.DecompositionGameFull,false,true,false,true)),
            new PageConfig("new Keyboard", "Sync decomposition not required new combinations Full", () => new SimpleViewCellsPage(GameType.DecompositionGameFull,false,true,true,true)),
            new PageConfig("new Keyboard", "Sync decomposition not required new combinations Blind Full", () => new SimpleViewCellsPage(GameType.DecompositionGameFull,false,true,true)),
            new PageConfig("new Keyboard", "Async decomposition required new combinations Full", () => new SimpleViewCellsPage(GameType.DecompositionGameFull,true,true,false,true)),
            new PageConfig("new Keyboard", "Sync decomposition required new combinations Full", () => new SimpleViewCellsPage(GameType.DecompositionGameFull,true,true,true,true)),
            new PageConfig("new Keyboard", "Sync decomposition required new combinations Blind Full", () => new SimpleViewCellsPage(GameType.DecompositionGameFull,true,true,true)),
            new PageConfig("new Keyboard", "Sync decomposition required new combinations Blind Full Sync", () => new SimpleViewCellsPage(GameType.SimpleDecompositionGame,true,true,true,false, false, false, true)),


            new PageConfig("new Number", "decomposition not required new combinations(up to 5)", () => new SimpleViewCellsPage(GameType.SimpleDecompositionGame)),
            new PageConfig("new Number", "decomposition(up to 5)", () => new SimpleViewCellsPage(GameType.SimpleDecompositionGame,true)),
            new PageConfig("new Number", "decomposition not required new combinations(up to 10)", () => new SimpleViewCellsPage(GameType.DecompositionGameFull)),
            new PageConfig("new Number", "decomposition(up to 10)", () => new SimpleViewCellsPage(GameType.DecompositionGameFull,true)),
            new PageConfig("new Number", "decomposition game Through 10 With kebyboard", () => new SimpleViewCellsPage(GameType.DecompositionGameWithKeyboardHelp, false,true,false)),
            new PageConfig("new Number", "decomposition game Full With kebyboard", () => new SimpleViewCellsPage(GameType.DecompositionGameFullWithKeyboardHelp, false,true,false)),
            new PageConfig("new Number", "decomposition game", () => new SimpleViewCellsPage(GameType.DecompositionGame)),
			
            //new PageConfig("Views", "Piano Sync decomposition one by one", () => new ButtonXaml { BindingContext = new ViewModels.ButtonViewModel(true) })),
            //new PageConfig("Views", "Piano Sync decomposition one by one 2 layers", () => new SimpleViewCellsPage(true,false,true,false)),
            new PageConfig("new Number", "Multiplication", () => new SimpleViewCellsPage(GameType.Multiplication)),

			// Cells
			new PageConfig("Cells", "TextCell", () => new TextCellMain { BindingContext = new ViewModels.ListOfStringsViewModel() }),
			new PageConfig("Cells", "ImageCell", () => new ImageCellMain { BindingContext = new ViewModels.ListOfObjectsViewModel() }),
			new PageConfig("Cells", "All Cells", () => new AllCellsXaml { BindingContext = new ViewModels.AllCellsViewModel() }),

			// Tests
			new PageConfig("Tests", "Clear in Handler", () => new DisposeInHandlerPage()),
			new PageConfig("Tests", "Horizontal ScrollView", () => new HorizontalScrollViewXaml { BindingContext = new ViewModels.CustomEventArgsViewModel() }),
			new PageConfig("Tests", "BigButton", () => new BigButtonPage()),
			new PageConfig("Tests", "ViewCells", () => new SimpleViewCellsPage(GameType.SimpleDecompositionGame)),
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

		public MainPage(string title, IEnumerable<PageConfig> contents)
		{
			Title = title;
			if (contents == null)
				contents = AllPages.Where(pc => pc.Parent == null);
			BindingContext = contents;

			InitializeComponent();
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
			catch(Exception ex)
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
