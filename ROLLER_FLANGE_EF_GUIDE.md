# RollerFlange Entity Configuration & Migration Guide

## Overview
This guide explains the RollerFlange entity configuration for Entity Framework Core and provides migration instructions.

## File Structure

```
Pulse.Models/
├── Rollers/
│   └── RollerFlange.cs                   (Entity model)
├── Configurations/
│   └── RollerFlangeConfiguration.cs      (EF Core configuration)
├── Validators/
│   └── RollerFlangeValidator.cs          (Validation logic)
└── DbContext.cs                          (Main context - needs update)
```

## Configuration Details

### RollerFlangeConfiguration.cs

The configuration file defines:

#### 1. **Table Mapping**
```csharp
builder.ToTable("RollerFlanges", schema: "dbo");
```
- Creates table `dbo.RollerFlanges` in SQL Server
- Follows naming convention: Entity plural

#### 2. **Column Configuration**

| Property | Database Type | Nullable | Default | Comment |
|----------|---------------|----------|---------|---------|
| Id | int | ✗ | - | Primary Key |
| OuterDiameter | decimal(10,2) | ✗ | - | mm |
| InnerDiameter | decimal(10,2) | ✗ | - | mm |
| Width | decimal(8,2) | ✗ | - | mm |
| BoltSetCount | int | ✗ | 4 | 4-point, 6-point, etc. |
| BoltHoleDiameter | decimal(8,2) | ✗ | 12 | M12, M16, M20 |
| BoltHolePitch | decimal(10,2) | ✗ | - | mm |
| BoltHoleCountPerSet | int | ✗ | 4 | Count per set |
| LipThickness | decimal(8,2) | ✓ | 0 | Optional projection |
| Material | nvarchar(50) | ✗ | '45Steel' | Material type |
| SurfaceRoughness | decimal(6,2) | ✓ | 3.2 | µm |
| OuterDiameterTolerance | decimal(6,3) | ✓ | 0.1 | ± mm |
| InnerDiameterTolerance | decimal(6,3) | ✓ | 0.05 | ± mm |
| WidthTolerance | decimal(6,3) | ✓ | 0.1 | ± mm |
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
IX_RollerFlange_Material              -- For filtering by material type
IX_RollerFlange_OuterDiameter        -- For filtering by size
IX_RollerFlange_BoltSetCount         -- For filtering by bolt pattern
IX_RollerFlange_BoltHoleDiameter     -- For filtering by bolt size

-- Composite index
IX_RollerFlange_DiameterBolts        -- For finding compatible flanges
```

#### 4. **Check Constraints**

```sql
-- Validation constraints enforced at database level
CK_RollerFlange_InnerLessThanOuter           -- InnerDiameter < OuterDiameter
CK_RollerFlange_DiamtersPositive             -- Both diameters > 0
CK_RollerFlange_WidthPositive                -- Width > 0
CK_RollerFlange_BoltHoleValid                -- BoltHole > 0 AND < InnerDiameter
CK_RollerFlange_BoltCountValid               -- BoltSetCount > 0 AND BoltHoleCountPerSet > 0
CK_RollerFlange_BoltPitchPositive            -- BoltHolePitch > 0
CK_RollerFlange_LipThicknessNonNegative      -- LipThickness >= 0
CK_RollerFlange_TolerancesNonNegative        -- All tolerances >= 0
```

## Validator Implementation

### RollerFlangeValidator.cs

Provides fluent validation with methods:

#### Validation Categories

| Method | Validates |
|--------|-----------|
| `ValidateGeometry()` | Diameters, width, lip thickness |
| `ValidateBoltHoles()` | Bolt specifications, pitch, patterns |
| `ValidateTolerances()` | Tolerance ranges and reasonableness |
| `ValidateMaterial()` | Material and surface properties |
| `ValidatePerformance()` | Performance specifications |
| `ValidateRealism()` | Manufacturing plausibility |

#### Usage Examples

```csharp
// Fluent validation
var validator = flange.Validate()
	.ValidateGeometry()
	.ValidateBoltHoles()
	.ValidateTolerances();

if (!validator.IsValid)
{
	var errors = validator.GetErrors();
	foreach (var error in errors)
		Console.WriteLine(error);
}

// Quick validation with throw
RollerFlangeValidator.ValidateAndThrow(flange);

// Silent validation
if (RollerFlangeValidator.TryValidate(flange, out var errors))
{
	// All valid
}

// Batch validation
var flanges = new List<RollerFlange> { /* ... */ };
var allErrors = flanges.ValidateAll();
```

## Integration Steps

### Step 1: Register Configuration in DbContext

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
	// Apply all configurations from assembly
	modelBuilder.ApplyConfigurationsFromAssembly(typeof(RollerFlangeConfiguration).Assembly);

	// Or apply individually:
	modelBuilder.ApplyConfiguration(new RollerFlangeConfiguration());

	base.OnModelCreating(modelBuilder);
}
```

### Step 2: Add to DbContext DbSet

```csharp
public DbSet<RollerFlange> RollerFlanges { get; set; }
```

### Step 3: Create Migration

```bash
# From Package Manager Console
Add-Migration AddRollerFlangeEntity -Project Pulse.Models

# Or using .NET CLI
dotnet ef migrations add AddRollerFlangeEntity --project Pulse.Models
```

### Step 4: Review Generated Migration

The migration will include:
- Table creation with all columns and defaults
- All check constraints
- All indexes

### Step 5: Apply Migration to Database

```bash
# Update database
Update-Database -Project Pulse.Models

# Or using .NET CLI
dotnet ef database update --project Pulse.Models
```

## Usage Examples

### Create a Flange

```csharp
var flange = new RollerFlange
{
	OuterDiameter = 150,
	InnerDiameter = 100,
	Width = 20,
	BoltSetCount = 4,
	BoltHoleDiameter = 12,
	BoltHolePitch = 110,
	BoltHoleCountPerSet = 4,
	Material = "45Steel",
	SurfaceRoughness = 3.2,
	HeatTreatmentState = HeatTreatment.Tempered,
	Coating = SurfaceCoating.ZincRichEpoxy,
	TorqueCapacity = 250,
	CreatedBy = "system"
};

// Validate before saving
RollerFlangeValidator.ValidateAndThrow(flange);

dbContext.RollerFlanges.Add(flange);
await dbContext.SaveChangesAsync();
```

### Query Flanges by Specifications

```csharp
// Get flanges by size
var largeF langes = await dbContext.RollerFlanges
	.Where(f => f.OuterDiameter >= 200)
	.OrderBy(f => f.OuterDiameter)
	.ToListAsync();

// Get flanges by bolt pattern
var fourBoltFlanges = await dbContext.RollerFlanges
	.Where(f => f.BoltSetCount == 4)
	.ToListAsync();

// Get compatible flanges (by material and heat treatment)
var steelFlanges = await dbContext.RollerFlanges
	.Where(f => f.Material == "45Steel" && f.HeatTreatmentState == HeatTreatment.Tempered)
	.ToListAsync();
```

### Update a Flange

```csharp
var flange = await dbContext.RollerFlanges.FindAsync(flangeId);
if (flange != null)
{
	flange.TorqueCapacity = 300;
	flange.ModifiedDate = DateTime.UtcNow;
	flange.ModifiedBy = currentUserId;

	await dbContext.SaveChangesAsync();
}
```

### Delete a Flange

```csharp
var flange = await dbContext.RollerFlanges.FindAsync(flangeId);
if (flange != null)
{
	dbContext.RollerFlanges.Remove(flange);
	await dbContext.SaveChangesAsync();
}
```

## Related Entities

If RollerFlange should belong to a RollerSpecification or RollerAssembly, uncomment the relationship:

```csharp
builder.HasOne<ClientRollerSpecification>()
	.WithMany()
	.HasForeignKey("RollerSpecificationId")
	.OnDelete(DeleteBehavior.Cascade)
	.HasConstraintName("FK_RollerFlange_RollerSpecification");
```

Then add the property to RollerFlange:

```csharp
public int? RollerSpecificationId { get; set; }
```

## Performance Considerations

### Indexes
- **DiameterBolts composite index** optimizes finding compatible flanges
- Single indexes on frequently filtered columns (Material, BoltSetCount)

### Query Optimization
```csharp
// Good - Uses indexes
var flanges = await dbContext.RollerFlanges
	.Where(f => f.OuterDiameter == 200 && f.BoltSetCount == 4)
	.OrderBy(f => f.Material)
	.ToListAsync();

// Less optimal - May require full scan
var flanges = await dbContext.RollerFlanges
	.Where(f => f.Width > 15)
	.ToListAsync();
```

## Validation Rules

### Geometry Rules
- OuterDiameter > 0
- InnerDiameter > 0 AND < OuterDiameter
- Width > 0
- InnerDiameter/OuterDiameter ratio: 50-95% (warning if outside)

### Bolt Specifications
- BoltHoleDiameter > 0 AND < InnerDiameter
- BoltHolePitch > BoltHoleDiameter
- BoltSetCount > 0 (typical: 2, 4, 6, 8)
- BoltHoleCountPerSet > 0

### Performance
- Pitch/InnerDiameter ratio: 90-200% (warning if outside)

## Soft Delete Support

To add soft delete capability:

```csharp
// In RollerFlange.cs
public bool IsDeleted { get; set; } = false;

// In RollerFlangeConfiguration.cs
builder.HasQueryFilter(rf => !rf.IsDeleted);
```

Then use soft delete:

```csharp
flange.IsDeleted = true;
flange.ModifiedDate = DateTime.UtcNow;
await dbContext.SaveChangesAsync();
```

## Troubleshooting

### Issue: "The entity type 'RollerFlange' is not part of this model"

**Solution**: Ensure configuration is registered in DbContext:
```csharp
modelBuilder.ApplyConfiguration(new RollerFlangeConfiguration());
```

### Issue: "Check constraint violation" on insert

**Solution**: Validate data before inserting:
```csharp
RollerFlangeValidator.ValidateAndThrow(flange);
```

### Issue: "BoltHoleDiameter must be less than InnerDiameter"

**Solution**: Ensure bolt holes fit within the bore:
```csharp
if (flange.BoltHoleDiameter >= flange.InnerDiameter)
	throw new InvalidOperationException("Bolt holes too large for bore");
```

## Rollback

To rollback the migration:

```bash
# Remove the last migration
Remove-Migration

# Or revert to a specific migration
Update-Database -TargetMigration PreviousMigrationName
```
