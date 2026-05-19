# PulseAspire Naming Conventions

**Domain:** Entity Framework Core + Database Schema  
**Last Updated:** May 2026  
**Applies to:** All new development in `Pulse.Models`

## Purpose
This document establishes clear and modern naming standards for database tables, entities, and related classes.  
The goal is to improve readability, maintainability, and alignment with current .NET best practices while respecting existing legacy tables.

## 1. Table Names

| Rule                           | Recommended                     | Avoid                 | Notes                    |
|--------------------------------|---------------------------------|-----------------------|--------------------------|
| Use **plural** nouns           | `InventoryTypes`                | `InventoryType`       | Preferred for new tables |
| **Do not** use "Master" suffix | `InventoryTypes`                | `InventoryTypeMaster` | Legacy naming            |
| Use PascalCase                 | `InventoryTypeDivisionSettings` | -                     | -                        |
| Keep existing legacy tables    | `RollerTypeMaster`              | -                     | Do not rename old tables |

**Guideline:** All **new** tables must follow plural naming. Do not create new tables ending in `Master`.

## 2. Entity Class Names

Use the **singular** form of the main domain concept.

| Table Name                      | Entity Class                   |
|---------------------------------|--------------------------------|
| `InventoryTypes`                | `InventoryType`                |
| `InventoryGroups`               | `InventoryGroup`               |
| `InventoryItems`                | `InventoryItem`                |
| `InventoryTypeDivisionSettings` | `InventoryTypeDivisionSetting` |

## 3. DbSet Properties (`PulseDbContext`)

Use clear **plural** names:

```csharp
public DbSet<InventoryType> InventoryTypes { get; set; }
public DbSet<InventoryGroup> InventoryGroups { get; set; }
public DbSet<InventoryItem> InventoryItems { get; set; }
public DbSet<InventoryTypeDivisionSetting> InventoryTypeDivisionSettings { get; set; }
```
Legacy DbSet properties (e.g. RollerTypeMaster) may remain unchanged.

## 4. Entity Type Configuration (Maps)

**Location**: Pulse.Models/PulseContext/Maps/  
**File naming**: [EntityName]Map.cs  
**Examples**:
- InventoryTypeMap.cs
- InventoryGroupMap.cs
- InventoryTypeDivisionSettingMap.cs

Always configure the table name explicitly:
```csharp
builder.ToTable("InventoryTypes");
```
## 5. Folder Structure in Pulse.Models
```
Pulse.Models/
├── Inventory/                          ← New or growing domain areas
│   ├── InventoryType.cs
│   ├── InventoryGroup.cs
│   ├── InventoryItem.cs
│   ├── InventoryTypeDivisionSetting.cs
│   └── InventoryGroupDivisionSetting.cs
├── Production/
├── Rollers/
├── Organizational/
├── PulseContext/
│   └── Maps/
├── AI/
└── ...
```
Group related entities into their own folder when the domain becomes substantial.

## 6. Handling Legacy Tables

Existing tables using the Master suffix (must not be renamed).
New features and modules should follow the modern plural naming convention.
We will gradually adopt cleaner naming for new work rather than refactoring old tables.

## 7. Column and Property Naming

| Category              | Convention                       | Example                        |
|-----------------------|----------------------------------|--------------------------------|
| Primary Key           | Id or [Entity]Id                 | IdInventoryType, Id            |
| Foreign Key           | [RelatedEntity]ID                | InventoryTypeID, DivisionID    |
| Booleans              | IsXxx or descriptive verb        | KeepStock, IsActive            |
| Navigation Properties | Singular / Plural as appropriate | InventoryType, InventoryGroups |
| Dates                 | CreatedDate, ModifiedDate        | -                              |

Use .HasColumnName("ActualColumnName") in the map when C# property names differ from database columns.

## 8. Quick Reference

|Item           |New Development        |Legacy Tables         |
|---------------|-----------------------|----------------------|
|Table Name     |Plural (InventoryTypes)|Keep as-is (XxxMaster)|
|Entity Class   |Singular               |Singular              |
|DbSet Property |Plural                 |Can keep old name     |
|Map File       |[Name]Map.cs           |-                     |
|"Master" suffix|Do not use             |Already exists        |

## Notes

- These conventions apply primarily to the Pulse.Models project and Entity Framework layer.
- When in doubt, favour readability and consistency with modern .NET practices.
- This document should be updated when significant new patterns are adopted.


Maintained by the development team