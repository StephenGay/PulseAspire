using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Pulse.Models.Api;

namespace Pulse.Web.Services
{
    /// <summary>
    /// Handles API exceptions in Blazor with appropriate UI feedback.
    /// </summary>
    public class ApiErrorHandler
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<ApiErrorHandler> _logger;
        private readonly AuthService _authService;
        private readonly NavigationManager _navigation;

        public ApiErrorHandler(
            IMessageService messageService,
            ILogger<ApiErrorHandler> logger,
            AuthService authService,
            NavigationManager navigation)
        {
            _messageService = messageService;
            _logger = logger;
            _authService = authService;
            _navigation = navigation;
        }

        /// <summary>
        /// Handles API exception with appropriate user feedback and logging.
        /// </summary>
        public async Task HandleAsync(PulseApiException ex, string? context = null)
        {
            _logger.LogError(ex, "API Error in {Context}: {Category}", context ?? "Unknown", ex.Category);

            // Handle specific error categories
            switch (ex.Category)
            {
                case ErrorCategory.Authentication:
                    await HandleAuthenticationErrorAsync(ex);
                    break;

                case ErrorCategory.Authorization:
                    HandleAuthorizationError(ex);
                    break;

                case ErrorCategory.Validation:
                    HandleValidationError(ex);
                    break;

                case ErrorCategory.RateLimited:
                    HandleRateLimitError(ex);
                    break;

                case ErrorCategory.Network:
                    HandleNetworkError(ex);
                    break;

                case ErrorCategory.Server:
                    HandleServerError(ex);
                    break;

                default:
                    HandleGenericError(ex);
                    break;
            }
        }

        private async Task HandleAuthenticationErrorAsync(PulseApiException ex)
        {
            _messageService.ShowMessageBar(options =>
            {
                options.Title = "Session Expired";
                options.Body = ex.UserMessage;
                options.Intent = MessageIntent.Warning;
                options.Section = "MESSAGES_TOP";
                options.Timeout = 5000;
            });

            // Clear auth state and redirect to login
            await _authService.ClearAsync();
            _navigation.NavigateTo("/login", forceLoad: true);
        }

        private void HandleAuthorizationError(PulseApiException ex)
        {
            _messageService.ShowMessageBar(options =>
            {
                options.Title = "Access Denied";
                options.Body = ex.UserMessage;
                options.Intent = MessageIntent.Error;
                options.Section = "MESSAGES_TOP";
                options.Timeout = 5000;
            });
        }

        private void HandleValidationError(PulseApiException ex)
        {
            if (ex.FieldErrors.Any())
            {
                var fieldSummary = string.Join(", ",
                    ex.FieldErrors.Select(f => $"{f.Key}: {string.Join("; ", f.Value)}"));

                _messageService.ShowMessageBar(options =>
                {
                    options.Title = "Validation Error";
                    options.Body = fieldSummary;
                    options.Intent = MessageIntent.Warning;
                    options.Section = "MESSAGES_TOP";
                    options.Timeout = 4000;
                });
            }
            else
            {
                HandleGenericError(ex);
            }
        }

        private void HandleRateLimitError(PulseApiException ex)
        {
            _messageService.ShowMessageBar(options =>
            {
                options.Title = "Too Many Requests";
                options.Body = "You're making requests too quickly. Please wait a moment.";
                options.Intent = MessageIntent.Warning;
                options.Section = "MESSAGES_TOP";
                options.Timeout = 6000;
            });
        }

        private void HandleNetworkError(PulseApiException ex)
        {
            _messageService.ShowMessageBar(options =>
            {
                options.Title = "Connection Error";
                options.Body = "Unable to reach the server. Please check your internet connection.";
                options.Intent = MessageIntent.Error;
                options.Section = "MESSAGES_TOP";
                options.Timeout = 5000;
            });
        }

        private void HandleServerError(PulseApiException ex)
        {
            _messageService.ShowMessageBar(options =>
            {
                options.Title = "Server Error";
                options.Body = ex.UserMessage;
                options.Intent = MessageIntent.Error;
                options.Section = "MESSAGES_TOP";
                options.Timeout = 5000;
            });

            // Include trace ID for support
            if (!string.IsNullOrEmpty(ex.Endpoint))
            {
                _logger.LogError("Server error from endpoint {Endpoint}: {ResponseContent}",
                    ex.Endpoint, ex.ResponseContent);
            }
        }

        private void HandleGenericError(PulseApiException ex)
        {
            _messageService.ShowMessageBar(options =>
            {
                options.Title = "Error";
                options.Body = ex.UserMessage;
                options.Intent = MessageIntent.Error;
                options.Section = "MESSAGES_TOP";
                options.Timeout = 5000;
            });
        }

        /// <summary>
        /// Suggests if operation should be retried automatically.
        /// </summary>
        public bool ShouldRetry(PulseApiException ex, int attemptCount = 0)
        {
            if (!ex.IsRetryable) return false;
            if (attemptCount >= 3) return false;

            return true;
        }

        /// <summary>
        /// Gets retry delay in milliseconds with exponential backoff.
        /// </summary>
        public int GetRetryDelay(int attemptCount)
        {
            return Math.Min(1000 * (int)Math.Pow(2, attemptCount), 10000);
        }
    }
}