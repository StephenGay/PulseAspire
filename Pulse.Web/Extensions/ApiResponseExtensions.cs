using Pulse.Models.Api;
using Microsoft.AspNetCore.Components.Forms;

namespace Pulse.Web.Extensions
{
    /// <summary>
    /// Extension methods for ApiResponse in Blazor components.
    /// </summary>
    public static class ApiResponseExtensions
    {
        /// <summary>
        /// Matches response success/failure with handlers (like pattern matching).
        /// </summary>
        public static async Task<TResult> MatchAsync<T, TResult>(
            this ApiResponse<T> response,
            Func<T, Task<TResult>> onSuccess,
            Func<ApiResponse<T>, Task<TResult>> onFailure) where T : class
        {
            if (response.Success && response.Data != null)
                return await onSuccess(response.Data);
            else
                return await onFailure(response);
        }

        /// <summary>
        /// Matches response with sync handlers.
        /// </summary>
        public static TResult Match<T, TResult>(
            this ApiResponse<T> response,
            Func<T, TResult> onSuccess,
            Func<ApiResponse<T>, TResult> onFailure) where T : class
        {
            if (response.Success && response.Data != null)
                return onSuccess(response.Data);
            else
                return onFailure(response);
        }

        /// <summary>
        /// Converts validation errors to Blazor EditForm format.
        /// </summary>
        public static void PopulateEditContext<T>(
            this ApiResponse<T> response,
            Microsoft.AspNetCore.Components.Forms.EditContext editContext) where T : class
        {
            if (!response.HasValidationErrors) return;

            var messages = new ValidationMessageStore(editContext);

            foreach (var (fieldName, errors) in response.Errors!)
            {
                var field = editContext.Field(fieldName);
                // Clear previous messages for this field then add new ones
                messages.Clear(field);
                foreach (var error in errors)
                {
                    messages.Add(field, error);
                }
            }

            // Notify the EditContext that the validation state changed so UI updates
            editContext.NotifyValidationStateChanged();
        }
    }
}