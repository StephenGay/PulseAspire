using System.Net;
using System.Text.Json.Serialization;

namespace Pulse.Models.Api
{
    /// <summary>
    /// Generic API response wrapper for standardized responses.
    /// Optimized for Blazor component state management.
    /// </summary>
    public class ApiResponse<T> where T : class
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }

        [JsonPropertyName("message")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }

        [JsonPropertyName("errors")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string[]>? Errors { get; set; }

        [JsonPropertyName("statusCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? StatusCode { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ApiResponse()
        {
        }

        public ApiResponse(T data, string? message = null)
        {
            Success = true;
            Data = data;
            Message = message;
            StatusCode = (int)HttpStatusCode.OK;
        }

        public ApiResponse(string message, Dictionary<string, string[]>? errors = null, int statusCode = 400)
        {
            Success = false;
            Message = message;
            Errors = errors;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Creates a successful response.
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string? message = null) =>
            new(data, message);

        /// <summary>
        /// Creates a failure response.
        /// </summary>
        public static ApiResponse<T> ErrorResponse(
            string message,
            Dictionary<string, string[]>? errors = null,
            int statusCode = 400) =>
            new(message, errors, statusCode);

        /// <summary>
        /// Throws PulseApiException if response is not successful.
        /// </summary>
        public void EnsureSuccess(string endpoint = "/api/unknown")
        {
            if (!Success)
            {
                throw PulseApiException.FromApiResponse(new ApiResponse
                {
                    Success = Success,
                    Message = Message,
                    Errors = Errors,
                    StatusCode = StatusCode
                }, endpoint);
            }
        }

        /// <summary>
        /// Converts response to PulseApiException (if failed).
        /// </summary>
        public PulseApiException? ToException(string endpoint = "/api/unknown") =>
            Success ? null : PulseApiException.FromApiResponse(new ApiResponse
            {
                Success = Success,
                Message = Message,
                Errors = Errors,
                StatusCode = StatusCode
            }, endpoint);

        /// <summary>
        /// Gets the HTTP status code as HttpStatusCode enum.
        /// </summary>
        public HttpStatusCode GetHttpStatusCode() =>
            StatusCode.HasValue ? (HttpStatusCode)StatusCode.Value : HttpStatusCode.OK;

        /// <summary>
        /// Indicates if response has validation errors.
        /// </summary>
        public bool HasValidationErrors => Errors?.Any() == true;

        /// <summary>
        /// Gets validation errors for specific field.
        /// </summary>
        public string[] GetFieldErrors(string fieldName) =>
            Errors?.TryGetValue(fieldName, out var errors) == true ? errors : Array.Empty<string>();

        /// <summary>
        /// Maps response data to another type.
        /// </summary>
        public ApiResponse<TNew> Map<TNew>(Func<T, TNew> selector) where TNew : class =>
            Success
                ? new ApiResponse<TNew>(selector(Data!), Message) { StatusCode = StatusCode }
                : new ApiResponse<TNew>(Message, Errors, StatusCode ?? 400);

        /// <summary>
        /// Maps response data asynchronously.
        /// </summary>
        public async Task<ApiResponse<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> selector) where TNew : class
        {
            if (!Success)
                return new ApiResponse<TNew>(Message, Errors, StatusCode ?? 400);

            var mappedData = await selector(Data!);
            return new ApiResponse<TNew>(mappedData, Message) { StatusCode = StatusCode };
        }

        /// <summary>
        /// Applies side effect if successful.
        /// </summary>
        public ApiResponse<T> OnSuccess(Action<T> action)
        {
            if (Success && Data != null)
                action(Data);
            return this;
        }

        /// <summary>
        /// Applies side effect if successful (async).
        /// </summary>
        public async Task<ApiResponse<T>> OnSuccessAsync(Func<T, Task> action)
        {
            if (Success && Data != null)
                await action(Data);
            return this;
        }

        /// <summary>
        /// Applies side effect if failed.
        /// </summary>
        public ApiResponse<T> OnFailure(Action<ApiResponse<T>> action)
        {
            if (!Success)
                action(this);
            return this;
        }

        /// <summary>
        /// Applies side effect if failed (async).
        /// </summary>
        public async Task<ApiResponse<T>> OnFailureAsync(Func<ApiResponse<T>, Task> action)
        {
            if (!Success)
                await action(this);
            return this;
        }

        /// <summary>
        /// Executes action regardless of success/failure.
        /// </summary>
        public ApiResponse<T> Finally(Action action)
        {
            action();
            return this;
        }

        /// <summary>
        /// Gets a summary of all errors.
        /// </summary>
        public string GetErrorSummary() =>
            Success
                ? string.Empty
                : Errors?.Any() == true
                    ? string.Join("; ", Errors.SelectMany(e => e.Value))
                    : Message ?? "Unknown error";
    }

    /// <summary>
    /// Non-generic API response wrapper.
    /// </summary>
    public class ApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }

        [JsonPropertyName("errors")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string[]>? Errors { get; set; }

        [JsonPropertyName("statusCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? StatusCode { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ApiResponse()
        {
        }

        public ApiResponse(bool success, string? message = null, Dictionary<string, string[]>? errors = null, int? statusCode = null)
        {
            Success = success;
            Message = message;
            Errors = errors;
            StatusCode = statusCode ?? (success ? (int)HttpStatusCode.OK : 400);
        }

        public static ApiResponse SuccessResponse(string? message = null) =>
            new(true, message);

        public static ApiResponse ErrorResponse(
            string message,
            Dictionary<string, string[]>? errors = null,
            int statusCode = 400) =>
            new(false, message, errors, statusCode);

        /// <summary>
        /// Throws PulseApiException if response is not successful.
        /// </summary>
        public void EnsureSuccess(string endpoint = "/api/unknown")
        {
            if (!Success)
                throw PulseApiException.FromApiResponse(this, endpoint);
        }

        /// <summary>
        /// Converts response to PulseApiException (if failed).
        /// </summary>
        public PulseApiException? ToException(string endpoint = "/api/unknown") =>
            Success ? null : PulseApiException.FromApiResponse(this, endpoint);

        /// <summary>
        /// Gets the HTTP status code as HttpStatusCode enum.
        /// </summary>
        public HttpStatusCode GetHttpStatusCode() =>
            StatusCode.HasValue ? (HttpStatusCode)StatusCode.Value : HttpStatusCode.OK;

        /// <summary>
        /// Indicates if response has validation errors.
        /// </summary>
        public bool HasValidationErrors => Errors?.Any() == true;

        /// <summary>
        /// Gets validation errors for specific field.
        /// </summary>
        public string[] GetFieldErrors(string fieldName) =>
            Errors?.TryGetValue(fieldName, out var errors) == true ? errors : Array.Empty<string>();

        /// <summary>
        /// Gets a summary of all errors.
        /// </summary>
        public string GetErrorSummary() =>
            Success
                ? string.Empty
                : Errors?.Any() == true
                    ? string.Join("; ", Errors.SelectMany(e => e.Value))
                    : Message ?? "Unknown error";

        /// <summary>
        /// Applies side effect if successful.
        /// </summary>
        public ApiResponse OnSuccess(Action action)
        {
            if (Success)
                action();
            return this;
        }

        /// <summary>
        /// Applies side effect if failed.
        /// </summary>
        public ApiResponse OnFailure(Action<ApiResponse> action)
        {
            if (!Success)
                action(this);
            return this;
        }
    }

    /// <summary>
    /// Paginated response wrapper for list queries.
    /// </summary>
    public class PaginatedResponse<T> : ApiResponse<List<T>> where T : class
    {
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage => PageNumber < TotalPages;

        [JsonPropertyName("hasPreviousPage")]
        public bool HasPreviousPage => PageNumber > 1;

        public PaginatedResponse()
        {
        }

        public PaginatedResponse(
            List<T> data,
            int pageNumber,
            int pageSize,
            int totalCount,
            string? message = null)
        {
            Success = true;
            Data = data;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
            Message = message;
            StatusCode = (int)HttpStatusCode.OK;
        }

        public static PaginatedResponse<T> Create(
            List<T> items,
            int pageNumber,
            int pageSize,
            int totalCount,
            string? message = null) =>
            new(items, pageNumber, pageSize, totalCount, message);
    }

    /// <summary>
    /// Loading state for Blazor components.
    /// Use to manage component loading states before API calls.
    /// </summary>
    public class LoadingState<T> where T : class
    {
        public bool IsLoading { get; set; } = true;
        public T? Data { get; set; }
        public string? Error { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }

        public bool IsSuccess => !IsLoading && Error == null && Data != null;
        public bool IsFailed => !IsLoading && Error != null;
        public bool IsInitial => IsLoading && Data == null && Error == null;

        public void SetSuccess(T data)
        {
            IsLoading = false;
            Data = data;
            Error = null;
            ValidationErrors = null;
        }

        public void SetFailure(string error, Dictionary<string, string[]>? validationErrors = null)
        {
            IsLoading = false;
            Data = null;
            Error = error;
            ValidationErrors = validationErrors;
        }

        public void SetLoading()
        {
            IsLoading = true;
            Error = null;
        }

        public void Reset()
        {
            IsLoading = true;
            Data = null;
            Error = null;
            ValidationErrors = null;
        }

        /// <summary>
        /// Converts ApiResponse to LoadingState.
        /// </summary>
        public static LoadingState<T> FromResponse(ApiResponse<T> response)
        {
            var state = new LoadingState<T> { IsLoading = false };

            if (response.Success && response.Data != null)
            {
                state.SetSuccess(response.Data);
            }
            else
            {
                state.SetFailure(response.Message ?? "Unknown error", response.Errors);
            }

            return state;
        }
    }
}