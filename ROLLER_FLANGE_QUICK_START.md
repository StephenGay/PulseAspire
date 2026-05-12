# RollerFlange Quick Start Guide

## 5-Minute Setup

### Step 1: Register Configuration (DbContext.cs)
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
	modelBuilder.ApplyConfiguration(new RollerFlangeConfiguration());
	base.OnModelCreating(modelBuilder);
}
```

### Step 2: Add DbSet
```csharp
public DbSet<RollerFlange> RollerFlanges { get; set; }
```

### Step 3: Create Migration
```bash
Add-Migration AddRollerFlangeEntity
Update-Database
```

### Step 4: Done!
Your database is ready. 🎉

---

## Common Operations

### Create Flange
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
	CreatedBy = userId
};

RollerFlangeValidator.ValidateAndThrow(flange);
dbContext.RollerFlanges.Add(flange);
await dbContext.SaveChangesAsync();
```

### Get Flanges by Size
```csharp
var flanges = await dbContext.RollerFlanges
	.Where(f => f.OuterDiameter == 150)
	.ToListAsync();
```

### Get by Bolt Pattern
```csharp
var fourBolts = await dbContext.RollerFlanges
	.Where(f => f.BoltSetCount == 4 && f.BoltHoleDiameter == 12)
	.ToListAsync();
```

### Update Flange
```csharp
flange.TorqueCapacity = 300;
flange.ModifiedDate = DateTime.UtcNow;
flange.ModifiedBy = userId;
await dbContext.SaveChangesAsync();
```

### Delete Flange
```csharp
dbContext.RollerFlanges.Remove(flange);
await dbContext.SaveChangesAsync();
```

---

## Validation Examples

### ✅ Valid Flange
```csharp
var flange = new RollerFlange
{
	OuterDiameter = 150,
	InnerDiameter = 100,
	Width = 20,
	BoltSetCount = 4,
	BoltHoleDiameter = 12,
	BoltHolePitch = 110
};

flange.Validate().ValidateAll(); // ✓ No errors
```

### ❌ Invalid Flange - Bore too large
```csharp
var flange = new RollerFlange
{
	OuterDiameter = 150,
	InnerDiameter = 160  // ERROR: > OuterDiameter
};

flange.Validate().ValidateGeometry().GetErrors();
// → ["InnerDiameter must be less than OuterDiameter"]
```

### ❌ Invalid Flange - Bolt hole too large
```csharp
var flange = new RollerFlange
{
	InnerDiameter = 100,
	BoltHoleDiameter = 120  // ERROR: > InnerDiameter
};

flange.Validate().ValidateBoltHoles().GetErrors();
// → ["BoltHoleDiameter must be less than InnerDiameter"]
```

---

## Database Schema at a Glance

| Column | Type | Default | Nullable |
|--------|------|---------|----------|
| Id | int | Auto | ✗ |
| OuterDiameter | decimal(10,2) | - | ✗ |
| InnerDiameter | decimal(10,2) | - | ✗ |
| Width | decimal(8,2) | - | ✗ |
| BoltSetCount | int | 4 | ✗ |
| BoltHoleDiameter | decimal(8,2) | 12 | ✗ |
| BoltHolePitch | decimal(10,2) | - | ✗ |
| BoltHoleCountPerSet | int | 4 | ✗ |
| Material | nvarchar(50) | 45Steel | ✗ |
| CreatedDate | datetime2 | GETUTCDATE() | ✗ |

---

## Common Flange Specifications

### 4-Bolt Flange (Standard)
```csharp
new RollerFlange
{
	OuterDiameter = 150,
	InnerDiameter = 100,
	Width = 20,
	BoltSetCount = 4,
	BoltHoleDiameter = 12,    // M12
	BoltHolePitch = 110
}
```

### 6-Bolt Flange (Large)
```csharp
new RollerFlange
{
	OuterDiameter = 200,
	InnerDiameter = 130,
	Width = 25,
	BoltSetCount = 6,
	BoltHoleDiameter = 16,    // M16
	BoltHolePitch = 150
}
```

### Flange with Lip
```csharp
new RollerFlange
{
	OuterDiameter = 180,
	InnerDiameter = 120,
	Width = 20,
	LipThickness = 5,         // Projects 5mm
	BoltSetCount = 4,
	BoltHoleDiameter = 12,
	BoltHolePitch = 130
}
```

---

## Troubleshooting

| Error | Solution |
|-------|----------|
| "RollerFlange not part of model" | Add `ApplyConfiguration()` in DbContext |
| "Check constraint violation" | Validate with `RollerFlangeValidator` |
| "BoltHole too large" | Bolt diameter must be < bore diameter |
| "Inner > Outer" | Inner diameter must be < outer diameter |
| Migration fails | Check if table already exists |

---

## API Endpoint Example

```csharp
[HttpPost("api/flanges")]
public async Task<IActionResult> CreateFlange(RollerFlange flange)
{
	try
	{
		RollerFlangeValidator.ValidateAndThrow(flange);
		_context.RollerFlanges.Add(flange);
		await _context.SaveChangesAsync();
		return CreatedAtAction(nameof(GetFlange), new { id = flange.Id }, flange);
	}
	catch (InvalidOperationException ex)
	{
		return BadRequest(ex.Message);
	}
}

[HttpGet("api/flanges/size/{diameter}")]
public async Task<IActionResult> GetBySize(double diameter)
{
	var flanges = await _context.RollerFlanges
		.Where(f => f.OuterDiameter == diameter)
		.ToListAsync();

	return Ok(flanges);
}

[HttpGet("api/flanges/bolts/{boltCount}")]
public async Task<IActionResult> GetByBoltCount(int boltCount)
{
	var flanges = await _context.RollerFlanges
		.Where(f => f.BoltSetCount == boltCount)
		.ToListAsync();

	return Ok(flanges);
}
```

---

**Ready to integrate? Start with Step 1 above!** 🚀
