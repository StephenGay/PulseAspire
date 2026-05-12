# RollerShaft Entity Configuration & Migration Guide

## Overview
This guide explains the RollerShaft entity configuration for Entity Framework Core and provides migration instructions.

## File Structure

```
Pulse.Models/
├── Rollers/
│   └── RollerShaft.cs                    (Entity model)
├── Configurations/
│   └── RollerShaftConfiguration.cs       (EF Core configuration)
└── DbContext.cs                          (Main context - needs update)
```

## Configuration Details

### RollerShaftConfiguration.cs

The configuration file defines:

#### 1. **Table Mapping**
```csharp
builder.ToTable("RollerShafts", schema: "dbo");
```
- Creates table `dbo.RollerShafts` in SQL Server
- Follows naming convention: Entity plural

#### 2. **Column Configuration**

| Property | Database Type | Nullable | Default | Comment |
|----------|---------------|----------|---------|---------|
| Id | int | ✗ | - | Primary Key |
| Length | decimal(10,2) | ✗ | - | mm |
| OuterDiameter | decimal(10,2) | ✗ | - | mm |
| BearingBoreDiameter | decimal(10,2) | ✗ | - | mm |
| KeywayWidth | decimal(8,2) | ✓ | 0 | mm |
| KeywayDepth | decimal(8,2) | ✓ | 0 | mm |
| KeywayLength | decimal(8,2) | ✓ | 0 | mm |
| EndFaceThickness | decimal(8,2) | ✓ | 0 | mm |
| Material | nvarchar(50) | ✗ | '45Steel' | Material type |
| SurfaceRoughness | decimal(6,2) | ✓ | 3.2 | µm |
| OuterDiameterTolerance | decimal(6,3) | ✓ | 0.1 | ± mm |
| BearingBoreTolerance | decimal(6,3) | ✓ | 0.05 | ± mm |
| KeywayTolerance | decimal(6,3) | ✓ | 0.1 | ± mm |
| CoverCompound | nvarchar(100) | ✓ | NULL | Compound name |
| CoverCompoundThickness | decimal(8,2) | ✓ | NULL | mm |
| CoverOverbuild | decimal(8,2) | ✓ | NULL | mm |
| AxialPosition | decimal(10,2) | ✓ | 0 | mm from left |
| ShaftPosition | nvarchar(10) | ✗ | 'Center' | Left/Center/Right |
| RadialOffset | decimal(8,2) | ✓ | 0 | mm offset |
| HeatTreatmentState | int | ✓ | 0 | Enum value |
| Coating | int | ✓ | 0 | Enum value |
| TorqueCapacity | decimal(10,2) | ✓ | 0 | Nm |
| CreatedDate | datetime2 | ✗ | GETUTCDATE() | UTC timestamp |
| ModifiedDate | datetime2 | ✓ | NULL | UTC timestamp |
| CreatedBy | nvarchar(100) | ✓ | NULL | User ID/name |
| ModifiedBy | nvarchar(100) | ✓ | NULL | User ID/name |

#### 3. **Indexes**

```sql
-- Single column indexes
IX_RollerShaft_ShaftPosition      -- For filtering by position
IX_RollerShaft_AxialPosition      -- For sorting by position
IX_RollerShaft_OuterDiameter      -- For filtering by size
IX_RollerShaft_Material           -- For material type queries

-- Composite index
IX_RollerShaft_PositionAxial      -- For assembly positioning queries
```

#### 4. **Check Constraints**

```sql
-- Validation constraints enforced at database level
CK_RollerShaft_BoreLessThanOuter      -- BearingBoreDiameter < OuterDiameter
CK_RollerShaft_KeywayNonNegative      -- Keyway dimensions >= 0
CK_RollerShaft_LengthPositive         -- Length > 0
CK_RollerShaft_DiameterPositive       -- OuterDiameter > 0
CK_RollerShaft_ValidPosition          -- ShaftPosition IN ('Left', 'Center', 'Right')
```

## Integration Steps

### Step 1: Register Configuration in DbContext

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
	// Apply all configurations from assembly
	modelBuilder.ApplyConfigurationsFromAssembly(typeof(RollerShaftConfiguration).Assembly);

	// Or apply individually:
	modelBuilder.ApplyConfiguration(new RollerShaftConfiguration());

	base.OnModelCreating(modelBuilder);
}
```

### Step 2: Add to DbContext DbSet

```csharp
public DbSet<RollerShaft> RollerShafts { get; set; }
```

### Step 3: Create Migration

```bash
# From Package Manager Console in Pulse.Models or Pulse.Web project
Add-Migration AddRollerShaftEntity -Project Pulse.Models

# Or using .NET CLI
dotnet ef migrations add AddRollerShaftEntity --project Pulse.Models
```

### Step 4: Review Generated Migration

The migration will include:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
	migrationBuilder.CreateTable(
		name: "RollerShafts",
		schema: "dbo",
		columns: new Dictionary<string, ColumnDefinition>
		{
			// ... all columns defined above
		},
		constraints: new Action<TableBuilder>[]
		{
			// ... all check constraints and foreign keys
		});

	migrationBuilder.CreateIndex(
		name: "IX_RollerShaft_ShaftPosition",
		schema: "dbo",
		table: "RollerShafts",
		column: "ShaftPosition");

	// ... more indexes
}
```

### Step 5: Apply Migration

```bash
# Update database
Update-Database -Project Pulse.Models

# Or using .NET CLI
dotnet ef database update --project Pulse.Models
```

## Usage Examples

### Create a Shaft

```csharp
var shaft = new RollerShaft
{
	Length = 1000,
	OuterDiameter = 60,
	BearingBoreDiameter = 30,
	Material = "45Steel",
	ShaftPosition = "Left",
	AxialPosition = 0,
	RadialOffset = 0,
	HeatTreatmentState = HeatTreatment.Tempered,
	Coating = SurfaceCoating.ZincRichEpoxy,
	TorqueCapacity = 500,
	CreatedBy = "system"
};

dbContext.RollerShafts.Add(shaft);
await dbContext.SaveChangesAsync();
```

### Query Shafts by Position

```csharp
// Get all left-side shafts
var leftShafts = await dbContext.RollerShafts
	.Where(s => s.ShaftPosition == "Left")
	.OrderBy(s => s.AxialPosition)
	.ToListAsync();

// Get all shafts sorted by position
var allShafts = await dbContext.RollerShafts
	.OrderBy(s => GetPositionOrder(s.ShaftPosition))
	.ThenBy(s => s.AxialPosition)
	.ToListAsync();
```

### Update a Shaft

```csharp
var shaft = await dbContext.RollerShafts.FindAsync(shaftId);
if (shaft != null)
{
	shaft.RadialOffset = 15;
	shaft.ModifiedDate = DateTime.UtcNow;
	shaft.ModifiedBy = currentUserId;

	await dbContext.SaveChangesAsync();
}
```

### Delete a Shaft

```csharp
var shaft = await dbContext.RollerShafts.FindAsync(shaftId);
if (shaft != null)
{
	dbContext.RollerShafts.Remove(shaft);
	await dbContext.SaveChangesAsync();
}
```

## Related Entities

The configuration includes commented examples for relationships. If RollerShaft should belong to a RollerSpecification, uncomment:

```csharp
builder.HasOne<ClientRollerSpecification>()
	.WithMany()
	.HasForeignKey("RollerSpecificationId")
	.OnDelete(DeleteBehavior.Cascade)
	.HasConstraintName("FK_RollerShaft_RollerSpecification");
```

Then add the property to RollerShaft:

```csharp
public int? RollerSpecificationId { get; set; }
```

## Performance Considerations

### Indexes
- **ShaftPosition + AxialPosition** composite index optimizes assembly queries
- Single indexes on frequently filtered columns (Material, OuterDiameter)

### Query Optimization
```csharp
// Good - Uses index
var shafts = await dbContext.RollerShafts
	.Where(s => s.ShaftPosition == "Left")
	.OrderBy(s => s.AxialPosition)
	.ToListAsync();

// Less optimal - Full scan
var shafts = await dbContext.RollerShafts
	.Where(s => s.Length > 500 && s.Material.Contains("Steel"))
	.ToListAsync();
```

## Soft Delete Support

To add soft delete capability, uncomment the query filter and add property:

```csharp
// In RollerShaft.cs
public bool IsDeleted { get; set; } = false;

// In RollerShaftConfiguration.cs
builder.HasQueryFilter(rs => !rs.IsDeleted);
```

Then update delete operations to use soft delete:

```csharp
shaft.IsDeleted = true;
shaft.ModifiedDate = DateTime.UtcNow;
await dbContext.SaveChangesAsync();
```

## Troubleshooting

### Issue: "The entity type 'RollerShaft' is not part of this model"

**Solution**: Ensure configuration is registered in DbContext:
```csharp
modelBuilder.ApplyConfiguration(new RollerShaftConfiguration());
```

### Issue: "Check constraint violation" on insert

**Solution**: Validate data before inserting:
```csharp
shaft.ValidateDimensions(); // Throws ArgumentException if invalid
```

### Issue: Migration fails due to existing table

**Solution**: If table exists, create a migration that checks:
```bash
Add-Migration FixRollerShaftMigration
```

Then in the Up() method, wrap in a condition:
```csharp
if (!migrationBuilder.ActiveProvider.Contains("SqlServer"))
	return;
```

## Rollback

To rollback the migration:

```bash
# Remove the last migration
Remove-Migration

# Or revert to a specific migration
Update-Database -TargetMigration PreviousMigrationName
```

## Additional Resources

- [EF Core Data Annotations](https://docs.microsoft.com/en-us/ef/core/modeling/data-annotations)
- [EF Core Fluent API](https://docs.microsoft.com/en-us/ef/core/modeling/relationships)
- [SQL Server Check Constraints](https://docs.microsoft.com/en-us/sql/relational-databases/tables/unique-constraints-and-check-constraints)
