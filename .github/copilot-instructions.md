# Copilot Instructions

## General Guidelines
- Always deserialize API responses to ApiResponse<T> when consuming endpoints from Pulse.ApiService in Blazor client applications. Extract the Data property from the ApiResponse<T> rather than deserializing directly to the inner type.