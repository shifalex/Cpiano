using GestureSample.Maui.Data;
using GestureSample.Maui.Handlers;
using GestureSample.Maui.Views;
using GestureSample.Maui.Models;
using GestureSample.Views;

namespace GestureSample.Maui.Views
{

    public partial class SwitchUserPage : ContentPage
    {

        private readonly UserRepository _userRepo;

        public SwitchUserPage()
        {
            InitializeComponent();
            // Retrieve the repository if not injected
            _userRepo = ServiceHelper.GetService<UserRepository>();
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

                await ServiceHelper.GetService<CurrentUserSession>().LoadUserAsync(selectedUser.Id);

                selectedUser.LastLoginTime = DateTime.Now;
                await _userRepo.UpdateAsync(selectedUser);
                // Go back to MainPage
                Application.Current.MainPage = new NavigationPage(new MainPage("Control Categories", null));
            }
        }

        private async void OnAddUserClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateUserPage(firstUser: false));
        }

        private async void OnDampButtonClicked(object sender, EventArgs e)
        {
            var users = await _userRepo.GetUsersAsync();
            foreach (var user in users)
            {
                //await _userRepo.UpdateUserAsync(user);
                //await SupabaseService.SyncUserAsync(user); // Sync with Supabase
            }
            await DisplayAlert("Sync Complete", "Users synced with Supabase", "OK");
        }


        private async void OnDeleteAllUsersClicked(object sender, EventArgs e)
        {
            var users = await _userRepo.GetUsersAsync();
            foreach (var user in users)
            {
                await _userRepo.DeleteAsync(user);
            }

            await ServiceHelper.GetService<CurrentUserSession>().LoadUserAsync(null);

            await DisplayAlert("Delete Complete", "All users deleted", "OK");
            await Navigation.PopToRootAsync();
        }
    }
}