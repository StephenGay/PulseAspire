# Reset Password Page Implementation

## Overview
Created a new `ResetPassword.razor` page that corresponds to the forgot-password endpoint. The page allows users to reset their password using a token sent to their email.

## File Created
- **Path**: `Pulse.Web\Components\Pages\Security\ResetPassword.razor`
- **Route**: `/reset-password?token={token}&email={email}`

## How It Works

### 1. **User Flow**
1. User clicks "Forgot Password?" on the login page
2. Enters their email in the forgot password dialog
3. API sends password reset email with link: `/reset-password?token={token}&email={email}`
4. User clicks link in email
5. Reset password page loads with email and token pre-filled
6. User enters new password and confirms it
7. Form submits to `/Security/reset-password` endpoint
8. On success, redirects to login page

### 2. **API Integration**
The page interacts with two endpoints:

**Forgot Password Endpoint** (in Login.razor):
```
POST /Security/forgot-password
Body: { "email": "user@example.com" }
Returns: ApiResponse with success message
```

**Reset Password Endpoint** (in ResetPassword.razor):
```
POST /Security/reset-password
Body: { 
  "email": "user@example.com",
  "token": "{reset-token}",
  "newPassword": "NewPassword123!"
}
Returns: ApiResponse with success/failure message
```

### 3. **Features**
✅ Query parameter extraction (`token` and `email`)
✅ Password validation (passwords must match)
✅ Real-time UI feedback (enabled/disabled button based on password match)
✅ Error handling with specific messages for network, validation, and server errors
✅ Success message with 2-second delay before redirect
✅ Loading state during submission
✅ Cancel button to return to login
✅ Fluent UI styling matching your Login page design
✅ Proper async/await pattern matching your patterns

### 4. **Styling**
- Matches Login.razor dark theme (`rgb(0,0,0)` background)
- Uses Pulse red accent color (`#660000`)
- Responsive card layout with 400px minimum width
- White text on dark background

### 5. **Error Handling**
The page handles multiple error scenarios:
- **Invalid Link**: Missing token or email parameters
- **Network Error**: Connection issues
- **Validation Error**: Invalid or expired token
- **Server Error**: Server-side exceptions
- **Password Mismatch**: Visual feedback when passwords don't match

## Usage

### Email Template Integration
The forgot password email template should include a link like:
```html
<a href="https://localhost:7219/reset-password?token=[{ResetToken}]&email=[{UserEmail}]">
  Reset Your Password
</a>
```

Or in your email service, the reset link is already constructed correctly:
```csharp
var resetLink = $"{appBaseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(user.Email)}";
```

## Testing

### Test Scenario 1: Valid Reset
1. Navigate to login page
2. Click "Forgot Password?"
3. Enter valid email
4. Check email for reset link
5. Click reset link
6. Enter new password and confirm
7. Click "Reset Password"
8. Should see success message and redirect to login

### Test Scenario 2: Invalid Link
1. Manually navigate to `/reset-password` without parameters
2. Should see error message
3. Can click "Back to Login" button

### Test Scenario 3: Password Mismatch
1. Open reset page with valid token
2. Enter different passwords
3. Button should be disabled until they match

### Test Scenario 4: Expired Token
1. Open reset page with expired token
2. Enter matching passwords and submit
3. Should show "Invalid or expired reset link" error

## Notes
- The page follows your Blazor conventions and patterns
- Uses proper async/await for all API calls
- Implements error categories matching your PulseApiException structure
- Includes comprehensive logging for debugging
- Password reset token is generated and validated by ASP.NET Core Identity
