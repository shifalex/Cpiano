using GestureSample.Maui;
using GestureSample.Maui.ViewModels;

namespace GestureSample.Views
{
	public partial class ShowDataXaml
	{
		public ShowDataXaml()
		{
			InitializeComponent();
			//StateList.ItemsSource = App.CurrentDB.GetStates();

			ShowData();	
        }

		public async void ShowData()
		{
            StateList.ItemsSource = await GestureSample.Maui.Data.StateConnection.Instance.GetStatesAsync();
        }
	}
}