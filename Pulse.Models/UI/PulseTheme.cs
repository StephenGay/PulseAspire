using Microsoft.FluentUI.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json;

namespace Pulse.Models.UI;

public class PulseTheme
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }          // Only one should be true (enforce in API)
    public bool IsDefault { get; set; }         // Fallback for new users / resets
    public bool IsSystem { get; set; }          // Prevent deletion of built-in themes

    public ThemeMode Mode { get; set; } = ThemeMode.Light;

    // FluentUI-friendly accent (optional preset)
    public OfficeColor? AccentOfficeColor { get; set; }   // e.g. Word, Excel, etc.
    public string? CustomAccentColor { get; set; }        // Hex override (#0078D4)

    public string? BackgroundImage { get; set; }
    // Core customizable properties (map to CSS vars)
    public string PrimaryColor { get; set; } = "#0F6CBD";
    public string BackgroundColor { get; set; } = "#F5F5F5";
    public string SurfaceColor { get; set; } = "#FFFFFF";
    public string TextPrimaryColor { get; set; } = "#1A1A1A";
    public string TextSecondaryColor { get; set; } = "#5C5C5C";
    public string SuccessColor { get; set; } = "#107C10";
    public string WarningColor { get; set; } = "#F7630C";
    public string ErrorColor { get; set; } = "#D13438";
    public string InfoColor { get; set; } = "#0078D4";

    public string FontFamily { get; set; } = "'Segoe UI Variable', 'Segoe UI', system-ui, sans-serif";
    public int BaseFontSize { get; set; } = 14;           // px
    public int BaseBorderRadius { get; set; } = 6;        // px

    // Flexible future-proof settings (JSON column)
    public string SettingsJson { get; set; } = "{}";

    [NotMapped]
    public ThemeSettings Settings
    {
        get => string.IsNullOrWhiteSpace(SettingsJson)
            ? new ThemeSettings()
            : JsonSerializer.Deserialize<ThemeSettings>(SettingsJson, JsonOptions)!;
        set => SettingsJson = JsonSerializer.Serialize(value, JsonOptions);
    }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}

public enum ThemeMode { Light, Dark, System }

public enum OfficeColor
{
    Word,
    Excel,
    PowerPoint,
    Outlook,
    Teams,
    OneNote,
    Access,
    Booking,
    Exchange,
    GroupMe,
    Office,
    OneDrive,
    PowerApps,
    Planner,
    PowerBI,
    Project,
    Publisher,
    SharePoint,
    Skype,
    Stream,
    Sway,
    Visio,
    Windows,
    Yammer,
    // ... add more as needed
    Custom // For user-defined colors
    // Add more as needed
}
public class ThemeSettings
{
    public Dictionary<string, string>? CustomCssVariables { get; set; } // e.g. "--my-brand": "#ff0000"
    public double? LineHeight { get; set; }
    public string? HeadingFontFamily { get; set; }
    // Add more as the builder grows
}

// Optional: DTO for API / mobile (keeps contracts clean)
public record ThemeDto(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    ThemeMode Mode,
    string PrimaryColor,
    // ... include other key fields or full entity for simplicity
    string SettingsJson
);
