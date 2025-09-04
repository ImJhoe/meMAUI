using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClinicaApp.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private bool isRefreshing;

        protected virtual void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = !string.IsNullOrEmpty(message);
        }

        protected virtual void ClearError()
        {
            ErrorMessage = string.Empty;
            HasError = false;
        }

        [RelayCommand]
        protected virtual async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        protected virtual void ShowLoading(bool show = true)
        {
            IsLoading = show;
        }
    }
}