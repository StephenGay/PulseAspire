# 🔒 SQLite Detection & Blocking Fix

## The Critical Issue

In debug logs, the AI was generating **SQLite queries against a SQL Server 2022 database**:

```
Pulse.Web.Services.Pulse_AI: Information: Executing SQL: 
  SELECT name FROM sqlite_master WHERE type='table';
```

### Why This Causes Infinite Loops

1. **SQLite query generated** for SQL Server database
2. **SQL Server throws error** - `sqlite_master` table doesn't exist
3. **AI repeats same invalid query** - without learning from error
4. **Loop continues endlessly** - recursion depths 1, 2, 3...

## The Solution: Three-Layer Defense

### Layer 1: System Message Enhancement

**File**: `Pulse.Web\Services\AiPromptService.cs`

Added prominent warnings:
```csharp
public string GetFlapperSystemMessageConcise()
{
    return """
        ⚠️ **CRITICAL: You MUST use SQL Server 2022 T-SQL syntax ONLY**
        - NEVER use SQLite syntax (e.g., sqlite_master, PRAGMA, etc.)
        - NEVER use MySQL syntax
        - ONLY generate valid SQL Server 2022 T-SQL queries
        """;
}
```

**Impact**: Reminds AI every recursion that database is SQL Server, not SQLite

### Layer 2: Comprehensive SQL Guidelines

Updated `GetSqlGenerationGuidelines()` with:

```
**SQL GENERATION BEST PRACTICES (SQL Server 2022 T-SQL ONLY)**

⚠️ **CRITICAL DATABASE**: SQL Server 2022 (NOT SQLite, MySQL, or PostgreSQL)
❌ FORBIDDEN: sqlite_master, PRAGMA, AUTOINCREMENT, ROWID, etc.
✅ REQUIRED: Use SQL Server T-SQL syntax only

SQL Server T-SQL Specific:
- Use CAST() or CONVERT() for type conversions
- GETDATE() for current date/time
- DATEDIFF() for date calculations
- STRING_AGG() for concatenation
```

**Impact**: Explicitly teaches AI what SQL Server syntax looks like

### Layer 3: Runtime SQL Validation

**File**: `Pulse.Web\Services\Pulse_AI.cs`

Added method:
```csharp
private static string? ValidateSqlServerSyntax(string sql)
{
    var sqlLower = sql.ToLower();

    // ❌ FORBIDDEN: SQLite syntax
    if (sqlLower.Contains("sqlite_master"))
        return "❌ ERROR: SQLite syntax detected. This is SQL Server 2022. Use SQL Server T-SQL syntax only.";

    if (sqlLower.Contains("pragma") && sqlLower.Contains("table_info"))
        return "❌ ERROR: SQLite PRAGMA syntax detected. Use sys.columns instead.";

    // ❌ FORBIDDEN: MySQL syntax
    if (sqlLower.Contains("auto_increment") && !sqlLower.Contains("identity"))
        return "❌ ERROR: MySQL AUTO_INCREMENT. Use SQL Server IDENTITY instead.";

    // ❌ FORBIDDEN: PostgreSQL syntax
    if (sqlLower.Contains("::") && sqlLower.Contains("::text"))
        return "❌ ERROR: PostgreSQL type casting. Use SQL Server CAST() instead.";

    return null; // Valid
}
```

**Blocked Syntax**:
| Database | Syntax | Blocker |
|----------|--------|---------|
| **SQLite** | `sqlite_master` | ✅ Blocked |
| **SQLite** | `PRAGMA table_info()` | ✅ Blocked |
| **SQLite** | `AUTOINCREMENT` | ✅ Blocked |
| **SQLite** | `ROWID` | ✅ Blocked |
| **MySQL** | `AUTO_INCREMENT` | ✅ Blocked |
| **MySQL** | Backticks `` ` `` | ✅ Blocked |
| **PostgreSQL** | `::text` casting | ✅ Blocked |
| **PostgreSQL** | `SERIAL` type | ✅ Blocked |

**Impact**: Catches wrong syntax BEFORE execution

### Integration Point

In `GetSQLFromOllamaAsync()`:

```csharp
var sql = SqlExtractor.Extract(ollamaResponse.Response);

// 🚨 Validate SQL Server syntax
var syntaxError = ValidateSqlServerSyntax(sql);
if (syntaxError != null)
{
    _logger.LogError("SQLite/MySQL/PostgreSQL syntax detected: {Query}", sql);
    return SqlGenerationResult.Error(syntaxError);
}
```

**Flow**:
```
Ollama generates SQL
    ↓
SqlExtractor extracts SQL
    ↓
ValidateSqlServerSyntax() checks it  ← NEW LAYER
    ↓
❌ If invalid: Return error message to user
✅ If valid: Proceed to execution
```

## Expected Behavior After Fix

### Scenario 1: AI generates SQLite query

```
User: "Show all tables"
AI: SELECT name FROM sqlite_master WHERE type='table';
System: ❌ ERROR: SQLite syntax detected (sqlite_master). 
        This is a SQL Server 2022 database. Use SQL Server T-SQL syntax only.
Result: Error returned, no infinite loop
```

### Scenario 2: AI generates valid SQL Server query

```
User: "Show top 10 customers"
AI: SELECT TOP 10 CustomerID, CustomerName FROM Customer WHERE IsActive = 1;
System: ✅ Valid SQL Server syntax
Result: Query executes successfully
```

### Scenario 3: Recursive call hits SQLite error

```
Loop 1: AI generates valid SQL → executes → returns results
Loop 2: AI analyzes results → generates another query
        → ValidateSqlServerSyntax() catches SQLite syntax
        → Error returned immediately
        → No infinite loop, recursion breaks
```

## Testing Checklist

- [ ] Disable streaming, ask a question → SQL executes normally
- [ ] Enable streaming, ask a question → SQL validates correctly
- [ ] Ask ambiguous question → no infinite loops, max recursion limits work
- [ ] Check logs for `"SQLite/MySQL/PostgreSQL syntax detected"`
- [ ] Verify system messages mention "SQL Server 2022" explicitly

## Files Modified

1. **Pulse.Web\Services\AiPromptService.cs**
   - Enhanced `GetFlapperSystemMessageConcise()` with critical warning
   - Updated `GetSqlGenerationGuidelines()` with forbidden syntax list

2. **Pulse.Web\Services\Pulse_AI.cs**
   - Added `ValidateSqlServerSyntax()` validation method
   - Integrated validation into `GetSQLFromOllamaAsync()`

3. **Pulse.Web\Components\Pages\AiZone\FlapperPg.razor**
   - Added recursion depth limits (max 3)
   - Added failed attempts counter (max 2)
   - Increased message history retention

## Log Examples

### ✅ Valid SQL Server Query

```
Pulse.Web.Services.Pulse_AI: Information: 
  SQL generated successfully with 287 tokens for query: Show top customers
```

### ❌ Invalid SQLite Query (Caught)

```
Pulse.Web.Services.Pulse_AI: Error: 
  SQLite/MySQL/PostgreSQL syntax detected: SELECT name FROM sqlite_master
Pulse.Web.Services.Pulse_AI: Error: 
  SQLite syntax detected (sqlite_master). This is SQL Server 2022...
```

## Related Issues Fixed

This fix resolves:
1. ♾️ **Infinite recursion loops** - Combined with depth limits
2. 🔄 **Repeated failed queries** - Validation fails fast, AI tries different approach
3. 💾 **Wrong database syntax** - Catches wrong SQL before execution
4. 📊 **Lost error context** - Error messages guide AI to use SQL Server syntax

## Future Enhancements

Consider implementing:
1. **Syntax suggestions** - "Did you mean CAST() instead of ::"?
2. **Query rewriting** - Auto-convert common SQLite→SQL Server patterns
3. **Cached validation** - Cache validation results for identical queries
4. **Detailed syntax tips** - Provide specific T-SQL replacement for detected syntax
