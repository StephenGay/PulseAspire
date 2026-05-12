# RollerShaft Quick Start Guide

## 5-Minute Setup

### Step 1: Register Configuration (DbContext.cs)
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
	modelBuilder.ApplyConfiguration(new RollerShaftConfiguration());
	base.OnModelCreating(modelBuilder);
}
```

### Step 2: Add DbSet
```csharp
public DbSet<RollerShaft> RollerShafts { get; set; }
```

### Step 3: Create Migration
```bash
Add-Migration AddRollerShaftEntity
Update-Database
```

### Step 4: Register DataTransferService (Program.cs)
```csharp
builder.Services.AddScoped<DataTransferService>();
```

**Done!** Your database is ready.

---

## Common Operations

### Create Shaft
```csharp
var shaft = new RollerShaft
{
	Length = 1000,
	OuterDiameter = 60,
	BearingBoreDiameter = 30,
	Material = "45Steel",
	ShaftPosition = "Left",
	AxialPosition = 0,
	CreatedBy = userId
};

RollerShaftValidator.ValidateAndThrow(shaft);
dbContext.RollerShafts.Add(shaft);
await dbContext.SaveChangesAsync();
```

### Get Shafts by Position
```csharp
var leftShafts = await dbContext.RollerShafts
	.Where(s => s.ShaftPosition == "Left")
	.OrderBy(s => s.AxialPosition)
	.ToListAsync();
```

### Update Shaft
```csharp
shaft.RadialOffset = 15;
shaft.ModifiedDate = DateTime.UtcNow;
shaft.ModifiedBy = userId;
await dbContext.SaveChangesAsync();
```

### Delete Shaft
```csharp
dbContext.RollerShafts.Remove(shaft);
await dbContext.SaveChangesAsync();
```

### Batch Validate
```csharp
var errors = shafts.ValidateAll();
if (!errors.Any())
{
	// All valid
}
```

---

## Positioning Concepts

| Position | X Offset | Purpose | Example |
|----------|----------|---------|---------|
| **Left** | Negative | Left-end bearing | Motor side shaft |
| **Center** | Zero (or offset) | Main rotating axis | Concentric roller |
| **Right** | Positive | Right-end bearing | Opposite side shaft |

```csharp
// Left shaft (motor side)
new RollerShaft { ShaftPosition = "Left", AxialPosition = 0, RadialOffset = 0 }

// Center concentric shaft (main roller)
new RollerShaft { ShaftPosition = "Center", AxialPosition = 0, RadialOffset = 0 }

// Right shaft with offset
new RollerShaft { ShaftPosition = "Right", AxialPosition = 1000, RadialOffset = 15 }
```

---

## Validation Examples

### ✅ Valid Shaft
```csharp
var shaft = new RollerShaft
{
	Length = 1000,
	OuterDiameter = 60,
	BearingBoreDiameter = 30,
	ShaftPosition = "Center",
	AxialPosition = 500
};

shaft.Validate().ValidateAll(); // No errors
```

### ❌ Invalid Shaft - Bore too large
```csharp
var shaft = new RollerShaft
{
	OuterDiameter = 60,
	BearingBoreDiameter = 65 // ERROR: > OuterDiameter
};

shaft.Validate().ValidateAll().GetErrors();
// → ["BearingBoreDiameter must be less than OuterDiameter"]
```

### ❌ Invalid Shaft - Bad position
```csharp
var shaft = new RollerShaft
{
	ShaftPosition = "Middle" // ERROR: Must be Left/Center/Right
};

shaft.Validate().ValidatePositioning().GetErrors();
// → ["ShaftPosition must be one of: Left, Center, Right"]
```

---

## Database Schema at a Glance

| Column | Type | Default | Nullable |
|--------|------|---------|----------|
| Id | int | Auto | ✗ |
| Length | decimal(10,2) | - | ✗ |
| OuterDiameter | decimal(10,2) | - | ✗ |
| BearingBoreDiameter | decimal(10,2) | - | ✗ |
| Material | nvarchar(50) | 45Steel | ✗ |
| ShaftPosition | nvarchar(10) | Center | ✗ |
| AxialPosition | decimal(10,2) | 0 | ✓ |
| RadialOffset | decimal(8,2) | 0 | ✓ |
| CreatedDate | datetime2 | GETUTCDATE() | ✗ |
| ModifiedDate | datetime2 | NULL | ✓ |

---

## 3D Viewer Integration

**Already implemented!** The 3D viewer automatically:
- ✓ Loads shafts from DataTransferService
- ✓ Sorts by position (Left → Center → Right)
- ✓ Applies positioning to Three.js scene
- ✓ Renders shafts with proper offset

Just ensure shafts have correct `ShaftPosition` and `AxialPosition` values.

---

## Troubleshooting

| Error | Solution |
|-------|----------|
| "RollerShaft not part of model" | Call `ApplyConfiguration()` in DbContext |
| "Check constraint violation" | Validate with `RollerShaftValidator` |
| "Invalid ShaftPosition" | Use: "Left", "Center", or "Right" |
| Migration fails | Check if table already exists |
| Invalid bore | Bore must be < OuterDiameter |

---

## API Endpoint Example

```csharp
[HttpPost("api/shafts")]
public async Task<IActionResult> CreateShaft(RollerShaft shaft)
{
	try
	{
		RollerShaftValidator.ValidateAndThrow(shaft);
		_context.RollerShafts.Add(shaft);
		await _context.SaveChangesAsync();
		return CreatedAtAction(nameof(GetShaft), new { id = shaft.Id }, shaft);
	}
	catch (InvalidOperationException ex)
	{
		return BadRequest(ex.Message);
	}
}

[HttpGet("api/shafts/position/{position}")]
public async Task<IActionResult> GetByPosition(string position)
{
	var shafts = await _context.RollerShafts
		.Where(s => s.ShaftPosition == position)
		.OrderBy(s => s.AxialPosition)
		.ToListAsync();

	return Ok(shafts);
}
```

---

## Related Documentation

- **Full Setup**: `ROLLER_SHAFT_EF_GUIDE.md`
- **3D Viewer**: `ROLLER_VIEWER_POSITIONING_GUIDE.md`
- **Implementation Summary**: `ROLLER_SHAFT_IMPLEMENTATION_SUMMARY.md`

---

**Ready to integrate? Start with Step 1 above!** 🚀
