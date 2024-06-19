using GestureSample.Maui;
using GestureSample.Maui.Data;
using GestureSample.Maui.ViewModels;

namespace GestureSample.Views
{
    public partial class ShowDataXaml
	{    
		//private readonly RealmService _realmService;

		public ShowDataXaml()
		{
			InitializeComponent();
			//StateList.ItemsSource = App.CurrentDB.GetStates();
			//_realmService = new RealmService();
			//StateList.ItemsSource = _realmService.GetItems();
            ShowData();	
        }

		public async void ShowData()
		{
            StateList.ItemsSource = await StateConnection.Instance.GetStatesAsync();
        }
	}
}