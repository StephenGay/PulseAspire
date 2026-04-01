using System.Net;
using System.Text.Json.Serialization;

namespace Pulse.Models.Api
{
	/// <summary>
	/// Generic API response wrapper that provides a standardized envelope for all API responses.
	/// Designed specifically for Blazor component state management and provides functional programming patterns.
	/// </summary>
	/// <typeparam name="T">The type of data payload this response contains. Must be a reference type (class).</typeparam>
	/// <remarks>
	/// This class implements the Result pattern for API communications, wrapping both successful data and error states in a unified structure.
	/// It includes JSON serialization attributes optimized for camelCase API contracts and conditional property serialization.
	/// The class provides fluent methods for chaining operations (Map, OnSuccess, OnFailure, Finally) enabling functional composition.
	/// All instances automatically capture a UTC timestamp for audit and debugging purposes.
	/// </remarks>
	public class ApiResponse<T> where T : class
	{
		/// <summary>
		/// Indicates whether the API operation completed successfully without errors.
		/// </summary>
		[JsonPropertyName("success")]
		public bool Success { get; set; }

		/// <summary>
		/// The data payload returned by the API operation. Only populated when Success is true.
		/// </summary>
		[JsonPropertyName("data")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public T? Data { get; set; }

		/// <summary>
		/// A user-friendly message providing additional context about the operation result (success or failure).
		/// </summary>
		[JsonPropertyName("message")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string? Message { get; set; }

		/// <summary>
		/// Field-level validation errors organized as a dictionary where keys are field names and values are arrays of error messages.
		/// </summary>
		[JsonPropertyName("errors")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public Dictionary<string, string[]>? Errors { get; set; }

		/// <summary>
		/// The HTTP status code associated with this response (e.g., 200, 400, 404, 500).
		/// </summary>
		[JsonPropertyName("statusCode")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? StatusCode { get; set; }

		/// <summary>
		/// The UTC timestamp when this response was created, useful for auditing and debugging.
		/// </summary>
		[JsonPropertyName("timestamp")]
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;

		/// <summary>
		/// Initializes a new instance of the <see cref="ApiResponse{T}"/> class with default values.
		/// </summary>
		/// <remarks>
		/// This parameterless constructor is primarily used for JSON deserialization.
		/// For creating responses in code, use the static factory methods <see cref="SuccessResponse"/> or <see cref="ErrorResponse"/>.
		/// </remarks>
		public ApiResponse()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ApiResponse{T}"/> class representing a successful operation.
		/// </summary>
		/// <param name="data">The data payload to include in the response. Must not be null for a successful response.</param>
		/// <param name="message">Optional user-friendly message describing the success.</param>
		/// <remarks>
		/// This constructor automatically sets Success=true and StatusCode=200 (OK).
		/// Consider using the static <see cref="SuccessResponse"/> factory method for better readability.
		/// </remarks>
		public ApiResponse(T data, string? message = null)
		{
			Success = true;
			Data = data;
			Message = message;
			StatusCode = (int)HttpStatusCode.OK;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ApiResponse{T}"/> class representing a failed operation.
		/// </summary>
		/// <param name="message">User-friendly error message describing what went wrong.</param>
		/// <param name="errors">Optional dictionary of field-level validation errors.</param>
		/// <param name="statusCode">HTTP status code representing the error type. Defaults to 400 (Bad Request).</param>
		/// <remarks>
		/// This constructor automatically sets Success=false and Data=null.
		/// Consider using the static <see cref="ErrorResponse"/> factory method for better readability.
		/// </remarks>
		public ApiResponse(string message, Dictionary<string, string[]>? errors = null, int statusCode = 400)
		{
			Success = false;
			Message = message;
			Errors = errors;
			StatusCode = statusCode;
		}

		/// <summary>
		/// Creates a successful API response with data and an optional message.
		/// </summary>
		/// <param name="data">The data payload to include in the response. Must not be null for a successful response.</param>
		/// <param name="message">Optional user-friendly message describing the success (e.g., "Customer created successfully"). Defaults to null.</param>
		/// <returns>A new <see cref="ApiResponse{T}"/> instance with Success=true, StatusCode=200, and the provided data.</returns>
		/// <remarks>
		/// Use this factory method to create standardized successful responses throughout your API.
		/// The Timestamp property is automatically set to DateTime.UtcNow.
		/// This method is typically used in API controllers after successful operations.
		/// </remarks>
		/// <example>
		/// <code>
		/// var customer = new Customer { Id = 1, Name = "Acme Corp" };
		/// return ApiResponse&lt;Customer&gt;.SuccessResponse(customer, "Customer retrieved successfully");
		/// </code>
		/// </example>
		public static ApiResponse<T> SuccessResponse(T data, string? message = null) =>
			new(data, message);

		/// <summary>
		/// Creates a failure API response with an error message and optional validation errors.
		/// </summary>
		/// <param name="message">User-friendly error message describing what went wrong (e.g., "Customer not found" or "Invalid input data").</param>
		/// <param name="errors">Optional dictionary of field-level validation errors where the key is the field name and the value is an array of error messages for that field.</param>
		/// <param name="statusCode">HTTP status code representing the error type. Defaults to 400 (Bad Request). Common values: 400=Bad Request, 401=Unauthorized, 403=Forbidden, 404=Not Found, 500=Internal Server Error.</param>
		/// <returns>A new <see cref="ApiResponse{T}"/> instance with Success=false and the specified error details.</returns>
		/// <remarks>
		/// Use this factory method to create standardized error responses throughout your API.
		/// The Data property will be null for error responses.
		/// Validation errors can be field-specific (e.g., {"Email": ["Invalid format", "Already exists"]}) or general errors.
		/// The Timestamp property is automatically set to DateTime.UtcNow.
		/// This supports Blazor component error handling and form validation scenarios.
		/// </remarks>
		/// <example>
		/// <code>
		/// // Simple error
		/// return ApiResponse&lt;Customer&gt;.ErrorResponse("Customer not found", statusCode: 404);
		/// 
		/// // With validation errors
		/// var errors = new Dictionary&lt;string, string[]&gt;
		/// {
		///     { "Email", new[] { "Email is required", "Invalid email format" } },
		///     { "Phone", new[] { "Phone number is invalid" } }
		/// };
		/// return ApiResponse&lt;Customer&gt;.ErrorResponse("Validation failed", errors, 400);
		/// </code>
		/// </example>
		public static ApiResponse<T> ErrorResponse(
			string message,
			Dictionary<string, string[]>? errors = null,
			int statusCode = 400) =>
			new(message, errors, statusCode);

		/// <summary>
		/// Validates that the API response indicates success; throws a <see cref="PulseApiException"/> if the response failed.
		/// </summary>
		/// <param name="endpoint">The API endpoint that produced this response (e.g., "/api/customers/123"). Used in the exception message for debugging. Defaults to "/api/unknown".</param>
		/// <exception cref="PulseApiException">Thrown when Success is false. The exception contains the error message, validation errors, status code, and endpoint for detailed error handling.</exception>
		/// <remarks>
		/// This method implements a "fail-fast" pattern where you want to immediately halt execution on API errors.
		/// It's particularly useful in service layer methods where you need to propagate errors to higher layers.
		/// The thrown exception contains all error details (message, validation errors, status code) and can be caught and handled appropriately.
		/// Use this when you want exceptions for flow control; use <see cref="ToException"/> if you prefer checking for null.
		/// </remarks>
		/// <example>
		/// <code>
		/// var response = await apiClient.GetCustomerAsync(customerId);
		/// response.EnsureSuccess("/api/customers/{customerId}");
		/// // If we reach here, the response was successful
		/// var customer = response.Data;
		/// </code>
		/// </example>
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
		/// Converts a failed API response to a <see cref="PulseApiException"/> without throwing; returns null if the response was successful.
		/// </summary>
		/// <param name="endpoint">The API endpoint that produced this response (e.g., "/api/customers/123"). Used in the exception for debugging. Defaults to "/api/unknown".</param>
		/// <returns>
		/// A <see cref="PulseApiException"/> containing all error details if Success is false; 
		/// null if Success is true, indicating no exception should be processed.
		/// </returns>
		/// <remarks>
		/// This method provides a non-throwing alternative to <see cref="EnsureSuccess"/> for scenarios where you want to check for errors without using exception-based flow control.
		/// It's useful when you want to conditionally handle errors, log them, or transform them before propagation.
		/// The returned exception (if not null) contains the complete error context including message, validation errors, status code, and endpoint.
		/// This method follows the "null-object pattern" for success cases, making it safe to use in null-conditional operations.
		/// </remarks>
		/// <example>
		/// <code>
		/// var response = await apiClient.GetCustomerAsync(customerId);
		/// var exception = response.ToException("/api/customers/{customerId}");
		/// if (exception != null)
		/// {
		///     logger.LogError(exception, "API call failed");
		///     return HandleError(exception);
		/// }
		/// ProcessCustomer(response.Data);
		/// </code>
		/// </example>
		public PulseApiException? ToException(string endpoint = "/api/unknown") =>
			Success ? null : PulseApiException.FromApiResponse(new ApiResponse
			{
				Success = Success,
				Message = Message,
				Errors = Errors,
				StatusCode = StatusCode
			}, endpoint);

		/// <summary>
		/// Converts the response's StatusCode property to the strongly-typed <see cref="HttpStatusCode"/> enumeration.
		/// </summary>
		/// <returns>
		/// The <see cref="HttpStatusCode"/> enum value corresponding to the StatusCode integer.
		/// Returns <see cref="HttpStatusCode.OK"/> (200) if StatusCode is null.
		/// </returns>
		/// <remarks>
		/// This method provides type-safe access to HTTP status codes, enabling compile-time checking and IntelliSense support.
		/// The StatusCode property may be null in some edge cases; this method defaults to OK (200) for null values.
		/// Common returned values include: OK (200), BadRequest (400), Unauthorized (401), Forbidden (403), NotFound (404), InternalServerError (500).
		/// Use this when you need to perform logic based on specific HTTP status codes (e.g., retry logic for 429, different handling for 404 vs 500).
		/// </remarks>
		/// <example>
		/// <code>
		/// var response = await apiClient.GetCustomerAsync(customerId);
		/// var statusCode = response.GetHttpStatusCode();
		/// 
		/// if (statusCode == HttpStatusCode.NotFound)
		/// {
		///     // Handle not found scenario
		/// }
		/// else if (statusCode >= HttpStatusCode.InternalServerError)
		/// {
		///     // Handle server errors
		/// }
		/// </code>
		/// </example>
		public HttpStatusCode GetHttpStatusCode() =>
			StatusCode.HasValue ? (HttpStatusCode)StatusCode.Value : HttpStatusCode.OK;

		/// <summary>
		/// Gets a boolean indicating whether the response contains field-level validation errors.
		/// </summary>
		/// <value>
		/// true if the Errors dictionary is not null and contains at least one validation error; otherwise, false.
		/// </value>
		/// <remarks>
		/// This property provides a quick check for the presence of validation errors without needing to check if Errors is null first.
		/// Validation errors are typically structured as field-name keys with arrays of error messages as values.
		/// This is commonly used in Blazor forms to determine whether to display field-specific error messages.
		/// Returns false if Errors is null or if the Errors dictionary is empty.
		/// Note that this only checks for the presence of errors in the Errors dictionary; the Message property may contain additional error context.
		/// </remarks>
		/// <example>
		/// <code>
		/// var response = await apiClient.CreateCustomerAsync(customer);
		/// if (response.HasValidationErrors)
		/// {
		///     foreach (var fieldError in response.Errors)
		///     {
		///         DisplayFieldErrors(fieldError.Key, fieldError.Value);
		///     }
		/// }
		/// </code>
		/// </example>
		public bool HasValidationErrors => Errors?.Any() == true;

		/// <summary>
		/// Retrieves all validation error messages for a specific field from the response's Errors dictionary.
		/// </summary>
		/// <param name="fieldName">The name of the field to retrieve errors for (case-sensitive). Common examples: "Email", "PhoneNumber", "CustomerName".</param>
		/// <returns>
		/// An array of error message strings for the specified field if errors exist; 
		/// an empty array if no errors exist for that field or if the field is not present in the Errors dictionary.
		/// Never returns null, ensuring safe iteration without null checks.
		/// </returns>
		/// <remarks>
		/// This method provides safe, null-free access to field-specific validation errors for Blazor form components.
		/// Field names are case-sensitive and should match the property names used in your model validation.
		/// The method returns an empty array (not null) when no errors exist, making it safe to use in foreach loops without null checking.
		/// Typical usage is in Blazor EditForm validation display logic where you need to show errors below specific input fields.
		/// </remarks>
		/// <example>
		/// <code>
		/// var response = await apiClient.CreateCustomerAsync(customer);
		/// var emailErrors = response.GetFieldErrors("Email");
		/// 
		/// foreach (var error in emailErrors)
		/// {
		///     &lt;ValidationMessage&gt;@error&lt;/ValidationMessage&gt;
		/// }
		/// </code>
		/// </example>
		public string[] GetFieldErrors(string fieldName) =>
			Errors?.TryGetValue(fieldName, out var errors) == true ? errors : Array.Empty<string>();

		/// <summary>
		/// Transforms the response data from type T to type TNew using a selector function, preserving the response metadata.
		/// </summary>
		/// <typeparam name="TNew">The target type to transform the data into. Must be a reference type (class).</typeparam>
		/// <param name="selector">A function that takes the current data of type T and transforms it to type TNew. Only invoked if the response is successful.</param>
		/// <returns>
		/// A new <see cref="ApiResponse{TNew}"/> with the transformed data if successful;
		/// or a new error response of type TNew with the original error details preserved if failed.
		/// </returns>
		/// <remarks>
		/// This method implements a functional programming pattern (functor map) for transforming successful response data.
		/// If the response indicates failure, the selector is NOT invoked and the error details are propagated.
		/// All metadata (Message, StatusCode, Errors, Timestamp) is preserved in the transformation.
		/// Use <see cref="MapAsync{TNew}"/> for async transformations requiring I/O operations.
		/// </remarks>
		/// <example>
		/// <code>
		/// ApiResponse&lt;CustomerDto&gt; dtoResponse = await apiClient.GetCustomerAsync(id);
		/// ApiResponse&lt;Customer&gt; modelResponse = dtoResponse.Map(dto => new Customer 
		/// { 
		///     Id = dto.CustomerId, 
		///     Name = dto.CustomerName 
		/// });
		/// </code>
		/// </example>
		public ApiResponse<TNew> Map<TNew>(Func<T, TNew> selector) where TNew : class =>
			Success
				? new ApiResponse<TNew>(selector(Data!), Message) { StatusCode = StatusCode }
				: new ApiResponse<TNew>(Message, Errors, StatusCode ?? 400);

		/// <summary>
		/// Asynchronously transforms the response data from type T to type TNew using an async selector function, preserving the response metadata.
		/// </summary>
		/// <typeparam name="TNew">The target type to transform the data into. Must be a reference type (class).</typeparam>
		/// <param name="selector">An async function that takes the current data of type T and returns a Task&lt;TNew&gt;. May perform async I/O operations.</param>
		/// <returns>
		/// A Task resolving to a new <see cref="ApiResponse{TNew}"/> with the transformed data if successful;
		/// or a new error response of type TNew with the original error details preserved if failed.
		/// </returns>
		/// <remarks>
		/// This is the asynchronous version of <see cref="Map{TNew}"/>, designed for transformations requiring I/O operations.
		/// If the response indicates failure, the selector is NOT invoked and the error details are propagated immediately without awaiting.
		/// Essential when your transformation requires database lookups, external API calls, or other async operations.
		/// </remarks>
		/// <example>
		/// <code>
		/// ApiResponse&lt;CustomerDto&gt; dtoResponse = await apiClient.GetCustomerAsync(id);
		/// ApiResponse&lt;CustomerViewModel&gt; viewModelResponse = await dtoResponse.MapAsync(async dto => 
		/// {
		///     var orderCount = await dbContext.Orders.CountAsync(o => o.CustomerId == dto.Id);
		///     return new CustomerViewModel { Customer = dto, TotalOrders = orderCount };
		/// });
		/// </code>
		/// </example>
		public async Task<ApiResponse<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> selector) where TNew : class
		{
			if (!Success)
				return new ApiResponse<TNew>(Message, Errors, StatusCode ?? 400);

			var mappedData = await selector(Data!);
			return new ApiResponse<TNew>(mappedData, Message) { StatusCode = StatusCode };
		}

		/// <summary>
		/// Executes a side-effect action with the response data if the response is successful, then returns the original response for method chaining.
		/// </summary>
		/// <param name="action">An action to execute with the response data. Only invoked if Success=true and Data is not null.</param>
		/// <returns>The original <see cref="ApiResponse{T}"/> instance unchanged, enabling fluent method chaining.</returns>
		/// <remarks>
		/// This method implements a side-effect pattern for success scenarios without transforming the response.
		/// The action is NOT invoked if the response indicates failure or if Data is null.
		/// Common use cases include: logging, notifications, cache updates, analytics.
		/// Use <see cref="OnSuccessAsync"/> for async operations.
		/// </remarks>
		/// <example>
		/// <code>
		/// var response = await apiClient.CreateCustomerAsync(customer)
		///     .OnSuccess(c => logger.LogInformation("Customer {Id} created", c.Id))
		///     .OnSuccess(c => cache.Set($"customer:{c.Id}", c))
		///     .Finally(() => IsLoading = false);
		/// </code>
		/// </example>
		public ApiResponse<T> OnSuccess(Action<T> action)
		{
			if (Success && Data != null)
				action(Data);
			return this;
		}

		/// <summary>
		/// Asynchronously executes a side-effect action with the response data if the response is successful, then returns the original response for method chaining.
		/// </summary>
		/// <param name="action">An async function to execute with the response data. Only invoked if Success=true and Data is not null. May perform I/O operations.</param>
		/// <returns>A Task resolving to the original <see cref="ApiResponse{T}"/> instance unchanged, enabling fluent async method chaining.</returns>
		/// <remarks>
		/// This is the asynchronous version of <see cref="OnSuccess"/>, designed for side-effects requiring async operations.
		/// The action is NOT invoked if the response indicates failure; the method returns immediately without awaiting.
		/// Common use cases include: async logging, email notifications, database updates, distributed cache invalidation.
		/// </remarks>
		/// <example>
		/// <code>
		/// var response = await apiClient.CreateCustomerAsync(customer)
		///     .OnSuccessAsync(async c => 
		///     {
		///         await emailService.SendWelcomeEmailAsync(c.Email);
		///         await searchService.IndexCustomerAsync(c);
		///     });
		/// </code>
		/// </example>
		public async Task<ApiResponse<T>> OnSuccessAsync(Func<T, Task> action)
		{
			if (Success && Data != null)
				await action(Data);
			return this;
		}

		/// <summary>
		/// Executes a side-effect action with the entire response object if the response indicates failure, then returns the original response for method chaining.
		/// </summary>
		/// <param name="action">An action to execute with the failed response. Only invoked if Success=false. Receives the entire ApiResponse object.</param>
		/// <returns>The original <see cref="ApiResponse{T}"/> instance unchanged, enabling fluent method chaining.</returns>
		/// <remarks>
		/// This method implements a side-effect pattern for error scenarios without transforming the response.
		/// The action is NOT invoked if the response indicates success.
		/// Common use cases include: error logging, user notifications, failure analytics, rollback operations.
		/// Use <see cref="OnFailureAsync"/> for async operations.
		/// </remarks>
		/// <example>
		/// <code>
		/// var response = await apiClient.CreateCustomerAsync(customer)
		///     .OnSuccess(c => ShowNotification($"Customer {c.Name} created!"))
		///     .OnFailure(r => 
		///     {
		///         logger.LogError("Failed: {Message}", r.Message);
		///         if (r.HasValidationErrors)
		///             DisplayValidationErrors(r.Errors);
		///         ShowErrorNotification(r.GetErrorSummary());
		///     })
		///     .Finally(() => IsSubmitting = false);
		/// </code>
		/// </example>
		public ApiResponse<T> OnFailure(Action<ApiResponse<T>> action)
		{
			if (!Success)
				action(this);
			return this;
		}

		/// <summary>
		/// Asynchronously executes a side-effect action with the entire response object if the response indicates failure, then returns the original response for method chaining.
		/// </summary>
		/// <param name="action">An async function to execute with the failed response. Only invoked if Success=false. May perform async I/O operations.</param>
		/// <returns>A Task resolving to the original <see cref="ApiResponse{T}"/> instance unchanged, enabling fluent async method chaining.</returns>
		/// <remarks>
		/// This is the asynchronous version of <see cref="OnFailure"/>, designed for error handling requiring async operations.
		/// The action is NOT invoked if the response indicates success; the method returns immediately without awaiting.
		/// Common use cases include: async error logging, error notification emails, failure metrics recording, compensating transactions.
		/// </remarks>
		/// <example>
		/// <code>
		/// var response = await apiClient.CreateCustomerAsync(customer)
		///     .OnFailureAsync(async r => 
		///     {
		///         await monitoringService.LogErrorAsync(new ErrorLog
		///         {
		///             Message = r.Message,
		///             Errors = r.Errors,
		///             StatusCode = r.StatusCode
		///         });
		///         
		///         if (r.GetHttpStatusCode() >= HttpStatusCode.InternalServerError)
		///             await alertService.SendCriticalErrorAlertAsync(r.Message);
		///     });
		/// </code>
		/// </example>
		public async Task<ApiResponse<T>> OnFailureAsync(Func<ApiResponse<T>, Task> action)
		{
			if (!Success)
				await action(this);
			return this;
		}

		/// <summary>
		/// Executes a side-effect action regardless of whether the response indicates success or failure, then returns the original response for method chaining.
		/// </summary>
		/// <param name="action">An action to execute unconditionally. Always invoked regardless of Success value. Does not receive any parameters.</param>
		/// <returns>The original <see cref="ApiResponse{T}"/> instance unchanged, enabling fluent method chaining.</returns>
		/// <remarks>
		/// This method implements a "finally" pattern (similar to try-finally) for operations that must occur regardless of success or failure.
		/// The action is ALWAYS invoked, making it ideal for cleanup operations.
		/// Common use cases include: hiding loading spinners, closing modals, resetting form state, stopping timers.
		/// For async operations, consider wrapping this in an async method or using Task.Run.
		/// </remarks>
		/// <example>
		/// <code>
		/// var response = await apiClient.CreateCustomerAsync(customer)
		///     .OnSuccess(c => NavigateToCustomer(c.Id))
		///     .OnFailure(r => ShowErrorMessage(r.Message))
		///     .Finally(() => 
		///     {
		///         IsLoading = false;
		///         StateHasChanged();
		///     });
		/// </code>
		/// </example>
		public ApiResponse<T> Finally(Action action)
		{
			action();
			return this;
		}

		/// <summary>
		/// Generates a concatenated summary of all error messages from the response, suitable for displaying to users or logging.
		/// </summary>
		/// <returns>
		/// An empty string if successful;
		/// a semicolon-separated string of all validation error messages if validation errors exist;
		/// the Message property value if no validation errors exist but the response failed;
		/// "Unknown error" if the response failed but has neither validation errors nor a message.
		/// </returns>
		/// <remarks>
		/// This method provides a convenient way to display or log all errors in a single string without manual iteration.
		/// The method prioritizes validation errors (field-specific) over the general Message property.
		/// Validation errors are flattened from dictionary structure into a single semicolon-separated string.
		/// Useful for Blazor components displaying a single error message box or for logging systems.
		/// Never returns null; always returns a string (possibly empty for successful responses).
		/// </remarks>
		/// <example>
		/// <code>
		/// var response = await apiClient.CreateCustomerAsync(customer);
		/// if (!response.Success)
		/// {
		///     var errorSummary = response.GetErrorSummary();
		///     // Example: "Email is required; Email format invalid; Phone number required"
		///     await JSRuntime.InvokeVoidAsync("alert", errorSummary);
		///     logger.LogError("Customer creation failed: {Errors}", errorSummary);
		/// }
		/// </code>
		/// </example>
		public string GetErrorSummary() =>
			Success
				? string.Empty
				: Errors?.Any() == true
					? string.Join("; ", Errors.SelectMany(e => e.Value))
					: Message ?? "Unknown error";
	}

	/// <summary>
	/// Non-generic API response wrapper for operations that don't return data (e.g., delete operations).
	/// Provides standardized success/failure messaging without a data payload.
	/// </summary>
	/// <remarks>
	/// This class is used for API operations that only need to communicate success/failure status without returning data.
	/// Common use cases include: DELETE operations, void methods, status checks, health checks.
	/// It supports the same error handling patterns as the generic version but omits the Data property.
	/// </remarks>
	public class ApiResponse
	{
		/// <summary>
		/// Indicates whether the API operation completed successfully without errors.
		/// </summary>
		[JsonPropertyName("success")]
		public bool Success { get; set; }

		/// <summary>
		/// A user-friendly message providing additional context about the operation result.
		/// </summary>
		[JsonPropertyName("message")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string? Message { get; set; }

		/// <summary>
		/// Field-level validation errors organized as a dictionary.
		/// </summary>
		[JsonPropertyName("errors")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public Dictionary<string, string[]>? Errors { get; set; }

		/// <summary>
		/// The HTTP status code associated with this response.
		/// </summary>
		[JsonPropertyName("statusCode")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? StatusCode { get; set; }

		/// <summary>
		/// The UTC timestamp when this response was created.
		/// </summary>
		[JsonPropertyName("timestamp")]
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;

		/// <summary>
		/// Initializes a new instance of the <see cref="ApiResponse"/> class with default values.
		/// </summary>
		public ApiResponse()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ApiResponse"/> class with specified values.
		/// </summary>
		/// <param name="success">Whether the operation succeeded.</param>
		/// <param name="message">Optional user-friendly message.</param>
		/// <param name="errors">Optional validation errors.</param>
		/// <param name="statusCode">Optional HTTP status code. Defaults to 200 for success, 400 for failure.</param>
		public ApiResponse(bool success, string? message = null, Dictionary<string, string[]>? errors = null, int? statusCode = null)
		{
			Success = success;
			Message = message;
			Errors = errors;
			StatusCode = statusCode ?? (success ? (int)HttpStatusCode.OK : 400);
		}

		/// <summary>
		/// Creates a successful response without data.
		/// </summary>
		/// <param name="message">Optional success message (e.g., "Customer deleted successfully").</param>
		/// <returns>A new <see cref="ApiResponse"/> instance with Success=true and StatusCode=200.</returns>
		/// <remarks>
		/// Use this for operations that complete successfully but don't return data (e.g., DELETE, PATCH without response body).
		/// </remarks>
		/// <example>
		/// <code>
		/// return ApiResponse.SuccessResponse("Customer deleted successfully");
		/// </code>
		/// </example>
		public static ApiResponse SuccessResponse(string? message = null) =>
			new(true, message);

		/// <summary>
		/// Creates a failure response without data.
		/// </summary>
		/// <param name="message">User-friendly error message.</param>
		/// <param name="errors">Optional field-level validation errors.</param>
		/// <param name="statusCode">HTTP status code. Defaults to 400 (Bad Request).</param>
		/// <returns>A new <see cref="ApiResponse"/> instance with Success=false and the specified error details.</returns>
		/// <remarks>
		/// Use this for operations that fail without needing to return data.
		/// </remarks>
		/// <example>
		/// <code>
		/// return ApiResponse.ErrorResponse("Customer not found", statusCode: 404);
		/// </code>
		/// </example>
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
	/// Paginated response wrapper for list queries that need to be returned in pages.
	/// Extends <see cref="ApiResponse{T}"/> with pagination metadata.
	/// </summary>
	/// <typeparam name="T">The type of items in the paginated list.</typeparam>
	/// <remarks>
	/// This class is designed for efficient handling of large datasets by returning data in manageable pages.
	/// It includes navigation helpers (HasNextPage, HasPreviousPage) for building pagination UI components.
	/// All pagination calculations are performed automatically based on PageNumber, PageSize, and TotalCount.
	/// </remarks>
	public class PaginatedResponse<T> : ApiResponse<List<T>> where T : class
	{
		/// <summary>
		/// The current page number (1-based indexing).
		/// </summary>
		[JsonPropertyName("pageNumber")]
		public int PageNumber { get; set; }

		/// <summary>
		/// The number of items per page.
		/// </summary>
		[JsonPropertyName("pageSize")]
		public int PageSize { get; set; }

		/// <summary>
		/// The total count of items across all pages.
		/// </summary>
		[JsonPropertyName("totalCount")]
		public int TotalCount { get; set; }

		/// <summary>
		/// The total number of pages available (calculated from TotalCount and PageSize).
		/// </summary>
		[JsonPropertyName("totalPages")]
		public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

		/// <summary>
		/// Indicates whether there is a next page available after the current page.
		/// </summary>
		[JsonPropertyName("hasNextPage")]
		public bool HasNextPage => PageNumber < TotalPages;

		/// <summary>
		/// Indicates whether there is a previous page available before the current page.
		/// </summary>
		[JsonPropertyName("hasPreviousPage")]
		public bool HasPreviousPage => PageNumber > 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="PaginatedResponse{T}"/> class with default values.
		/// </summary>
		public PaginatedResponse()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PaginatedResponse{T}"/> class with pagination data.
		/// </summary>
		/// <param name="data">The list of items for the current page.</param>
		/// <param name="pageNumber">The current page number (1-based).</param>
		/// <param name="pageSize">The number of items per page.</param>
		/// <param name="totalCount">The total count of items across all pages.</param>
		/// <param name="message">Optional success message.</param>
		/// <remarks>
		/// This constructor automatically sets Success=true and StatusCode=200.
		/// TotalPages, HasNextPage, and HasPreviousPage are calculated automatically.
		/// </remarks>
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

		/// <summary>
		/// Creates a paginated response with the specified pagination details.
		/// </summary>
		/// <param name="items">The list of items for the current page.</param>
		/// <param name="pageNumber">The current page number (1-based).</param>
		/// <param name="pageSize">The number of items per page.</param>
		/// <param name="totalCount">The total count of items across all pages.</param>
		/// <param name="message">Optional success message.</param>
		/// <returns>A new <see cref="PaginatedResponse{T}"/> instance with the specified pagination data.</returns>
		/// <remarks>
		/// Use this factory method to create standardized paginated responses in your API endpoints.
		/// Pagination navigation properties (HasNextPage, HasPreviousPage, TotalPages) are calculated automatically.
		/// </remarks>
		/// <example>
		/// <code>
		/// var customers = await dbContext.Customers
		///     .Skip((pageNumber - 1) * pageSize)
		///     .Take(pageSize)
		///     .ToListAsync();
		///     
		/// var totalCount = await dbContext.Customers.CountAsync();
		/// 
		/// return PaginatedResponse&lt;Customer&gt;.Create(
		///     customers, 
		///     pageNumber: 1, 
		///     pageSize: 20, 
		///     totalCount: totalCount
		/// );
		/// </code>
		/// </example>
		public static PaginatedResponse<T> Create(
			List<T> items,
			int pageNumber,
			int pageSize,
			int totalCount,
			string? message = null) =>
			new(items, pageNumber, pageSize, totalCount, message);
	}

	/// <summary>
	/// Loading state manager for Blazor components to track async API operation status.
	/// Provides a simpler state model focused on loading, success, and failure states.
	/// </summary>
	/// <typeparam name="T">The type of data being loaded.</typeparam>
	/// <remarks>
	/// This class is specifically designed for Blazor component state management patterns.
	/// It tracks three distinct states: loading (IsLoading=true), success (Data populated), and failure (Error populated).
	/// Unlike ApiResponse, this class is mutable and intended for component-level state tracking.
	/// Use this in Blazor components to manage UI state during API calls (show spinners, errors, data).
	/// </remarks>
	public class LoadingState<T> where T : class
	{
		/// <summary>
		/// Indicates whether an async operation is currently in progress.
		/// </summary>
		public bool IsLoading { get; set; } = true;

		/// <summary>
		/// The loaded data. Only populated when the operation completes successfully.
		/// </summary>
		public T? Data { get; set; }

		/// <summary>
		/// The error message if the operation failed.
		/// </summary>
		public string? Error { get; set; }

		/// <summary>
		/// Field-level validation errors if the operation failed with validation errors.
		/// </summary>
		public Dictionary<string, string[]>? ValidationErrors { get; set; }

		/// <summary>
		/// Indicates the operation completed successfully with data.
		/// </summary>
		public bool IsSuccess => !IsLoading && Error == null && Data != null;

		/// <summary>
		/// Indicates the operation failed with an error.
		/// </summary>
		public bool IsFailed => !IsLoading && Error != null;

		/// <summary>
		/// Indicates the operation hasn't started or has no result yet.
		/// </summary>
		public bool IsInitial => IsLoading && Data == null && Error == null;

		/// <summary>
		/// Sets the loading state to success with the provided data.
		/// </summary>
		/// <param name="data">The successfully loaded data.</param>
		/// <remarks>
		/// This method clears IsLoading, Error, and ValidationErrors and sets Data.
		/// Call this method when your API operation completes successfully.
		/// Remember to call StateHasChanged() in your Blazor component after this.
		/// </remarks>
		/// <example>
		/// <code>
		/// var response = await apiClient.GetCustomerAsync(id);
		/// if (response.Success)
		/// {
		///     loadingState.SetSuccess(response.Data);
		///     StateHasChanged();
		/// }
		/// </code>
		/// </example>
		public void SetSuccess(T data)
		{
			IsLoading = false;
			Data = data;
			Error = null;
			ValidationErrors = null;
		}

		/// <summary>
		/// Sets the loading state to failure with the provided error details.
		/// </summary>
		/// <param name="error">The error message describing what went wrong.</param>
		/// <param name="validationErrors">Optional field-level validation errors.</param>
		/// <remarks>
		/// This method clears IsLoading and Data, and sets Error and ValidationErrors.
		/// Call this method when your API operation fails.
		/// Remember to call StateHasChanged() in your Blazor component after this.
		/// </remarks>
		/// <example>
		/// <code>
		/// var response = await apiClient.GetCustomerAsync(id);
		/// if (!response.Success)
		/// {
		///     loadingState.SetFailure(response.Message, response.Errors);
		///     StateHasChanged();
		/// }
		/// </code>
		/// </example>
		public void SetFailure(string error, Dictionary<string, string[]>? validationErrors = null)
		{
			IsLoading = false;
			Data = null;
			Error = error;
			ValidationErrors = validationErrors;
		}

		/// <summary>
		/// Sets the loading state to indicate an operation is in progress.
		/// </summary>
		/// <remarks>
		/// This method sets IsLoading=true and clears Error (but preserves existing Data).
		/// Call this method before starting an API operation.
		/// Remember to call StateHasChanged() in your Blazor component after this.
		/// </remarks>
		/// <example>
		/// <code>
		/// protected override async Task OnInitializedAsync()
		/// {
		///     customerState.SetLoading();
		///     StateHasChanged();
		///     
		///     var response = await apiClient.GetCustomerAsync(customerId);
		///     // Handle response...
		/// }
		/// </code>
		/// </example>
		public void SetLoading()
		{
			IsLoading = true;
			Error = null;
		}

		/// <summary>
		/// Resets the loading state to its initial state (loading with no data or errors).
		/// </summary>
		/// <remarks>
		/// This method sets IsLoading=true and clears all data, errors, and validation errors.
		/// Use this when you want to completely reset the component state (e.g., user navigates away and back).
		/// Remember to call StateHasChanged() in your Blazor component after this.
		/// </remarks>
		/// <example>
		/// <code>
		/// private void OnRefreshClicked()
		/// {
		///     customerState.Reset();
		///     StateHasChanged();
		///     await LoadCustomerAsync();
		/// }
		/// </code>
		/// </example>
		public void Reset()
		{
			IsLoading = true;
			Data = null;
			Error = null;
			ValidationErrors = null;
		}

		/// <summary>
		/// Creates a <see cref="LoadingState{T}"/> from an <see cref="ApiResponse{T}"/>.
		/// </summary>
		/// <param name="response">The API response to convert.</param>
		/// <returns>A new <see cref="LoadingState{T}"/> instance reflecting the response status.</returns>
		/// <remarks>
		/// This factory method provides a convenient way to convert an ApiResponse to a LoadingState.
		/// The returned state will have IsLoading=false and will be in either success or failure state.
		/// Use this to bridge between your API client (which returns ApiResponse) and your Blazor component state.
		/// </remarks>
		/// <example>
		/// <code>
		/// var response = await apiClient.GetCustomerAsync(id);
		/// customerState = LoadingState&lt;Customer&gt;.FromResponse(response);
		/// StateHasChanged();
		/// </code>
		/// </example>
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