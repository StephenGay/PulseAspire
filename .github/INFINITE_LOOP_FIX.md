# 🔁 Infinite Loop Fix: Streaming Recursion Limits

## The Problem

When streaming is enabled and the AI encounters errors (e.g., trying to run invalid SQL), it gets stuck in an **infinite recursion loop**:

```
Loop 1 (qryLoop=0): User asks question
  ↓ AI generates invalid SQL query
  ↓ Tool execution fails
  ↓ Recursive call...

Loop 2 (qryLoop=1): Same invalid query attempted again
  ↓ Same error
  ↓ Recursive call...

Loop 3 (qryLoop=2): Infinite cycle...
```

This happens because:
1. **No recursion depth limit** - the method can recurse indefinitely
2. **Errors don't break the loop** - failed queries don't stop recursion
3. **Lost error context** - on recursive calls, previous error messages get trimmed away
4. **No attempt counter** - no tracking of repeated failures

## The Solution

### 1️⃣ **Maximum Recursion Depth**
```csharp
const int MAX_RECURSION_DEPTH = 3;

if (qryLoop >= MAX_RECURSION_DEPTH)
{
    yield return "Maximum recursion depth reached. Please start a new query.";
    yield break;
}
```
**Effect**: Stops endless recursion after 3 levels (user query → 1st tool call → 2nd tool call → stop)

### 2️⃣ **Maximum Failed Attempts**
```csharp
const int MAX_FAILED_ATTEMPTS = 2;

if (failedAttempts >= MAX_FAILED_ATTEMPTS)
{
    yield return "Too many failed attempts. Please rephrase your query.";
    yield break;
}
```
**Effect**: Tracks failed SQL executions and stops after 2 failures

### 3️⃣ **Preserve Error Context**
```csharp
// On recursive calls, keep 4 messages instead of 3
// This preserves error responses from failed queries
.TakeLast(4)  // Was: TakeLast(3)
```
**Effect**: AI sees error messages from previous attempts, learns not to repeat them

### 4️⃣ **Thread Recursion Parameter**
```csharp
// Method signature updated with failedAttempts parameter
private async IAsyncEnumerable<string> StreamChatWithFlapperAsync(
    ...
    int failedAttempts = 0)
{
    // Passed to recursive calls
    await foreach (var chunk in StreamChatWithFlapperAsync(
        messages, sModel, enableSearch, ct, qryLoop + 1, inThinking, failedAttempts))
    {
        yield return chunk;
    }
}
```

## What This Prevents

| Scenario | Before | After |
|----------|--------|-------|
| Invalid SQL in loop | ♾️ Infinite | ✅ Stops after 2 failures |
| Deep recursion | ♾️ Infinite | ✅ Stops after 3 levels |
| Lost error context | ❌ AI repeats errors | ✅ AI sees error messages |
| User experience | 😞 App hangs | 😊 Clear error message |

## Testing

To test the fix:

1. **Enable streaming** in Flapper settings ("Show Thinking")
2. **Ask an ambiguous question** that might trigger multiple tool calls
3. **Expected behavior**:
   - Max 3 recursion levels (original + 2 recursive calls)
   - Clear error messages if it hits limits
   - No infinite loops or app hangs

### Example Test Cases

❌ **This will hit recursion limit**:
```
User: "Tell me about everything"
→ AI tries query 1 → fails
→ AI tries query 2 → fails  
→ AI tries query 3 → fails
→ STOPS: "Maximum failed attempts"
```

✅ **This should work fine**:
```
User: "Show top 10 customers"
→ AI generates valid SQL
→ Results returned
→ Completes normally (qryLoop=0, no recursion)
```

## Related Code Changes

- **File**: `Pulse.Web\Components\Pages\AiZone\FlapperPg.razor`
- **Method**: `StreamChatWithFlapperAsync()`
- **Changes**:
  - Added `failedAttempts` parameter (default = 0)
  - Added `MAX_RECURSION_DEPTH = 3` constant
  - Added `MAX_FAILED_ATTEMPTS = 2` constant
  - Added checks at method entry to break loops early
  - Increased message history retention from 3 to 4 on recursive calls

## Performance Impact

- **Positive**: No infinite loops = much better UX
- **Neutral**: Slightly larger message list (4 vs 3) on recursive calls
- **Minimal**: Logic is just 2 integer comparisons

## Future Improvements

Consider implementing:
1. **Exponential backoff** - delay between retry attempts
2. **Query deduplication** - detect if same query is being retried
3. **Timeout-based exits** - max time for entire conversation
4. **Smart error recovery** - modify query based on error type instead of just failing
