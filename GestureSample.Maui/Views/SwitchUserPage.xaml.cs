using GestureSample.Maui.Data;
using GestureSample.Maui.Handlers;
using GestureSample.Maui.Views;
using GestureSample.Maui.Models;

namespace GestureSample.Maui.Views
{

	public partial class SwitchUserPage : ContentPage
    {

        private readonly IUserRepository _userRepo;

        public SwitchUserPage()
        {
            InitializeComponent();
            // Retrieve the repository if not injected
            _userRepo = ServiceHelper.GetService<IUserRepository>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var users = await _userRepo.GetUsersAsync();
            UsersCollectionView.ItemsSource = users;
        }

        private async void OnUserSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedUser = e.CurrentSelection?.FirstOrDefault() as User;
            if (selectedUser != null)
            {
                // Set this as the active user
                ActiveUserHelper.CurrentUserId = selectedUser.Id;

                // Go back to MainPage
                await Navigation.PopAsync();
            }
        }

        private async void OnAddUserClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateUserPage(_userRepo, firstUser: false));
        }
    }
}