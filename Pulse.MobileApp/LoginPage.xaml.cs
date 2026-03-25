using Pulse.MobileApp.Services;

namespace Pulse.MobileApp;

public partial class LoginPage : ContentPage
{
    private readonly PulseApiService _apiService;

    public LoginPage(PulseApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;

        // Validation
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Please enter both email and password");
            return;
        }

        // Show loading
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        LoginButton.IsEnabled = false;
        ErrorLabel.IsVisible = false;

        try
        {
            var result = await _apiService.LoginAsync(email, password);

            // Check for successful login with token in Data property (ApiResponse<AuthenticationToken> structure)
            if (result?.Success == true && result.Data?.Token != null)
            {
                // Login successful - navigate to main app
                await Shell.Current.GoToAsync("///mainpage");
            }
            else
            {
                ShowError(result?.Message ?? "Login failed. Please try again.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error: {ex.Message}");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            LoginButton.IsEnabled = true;
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}