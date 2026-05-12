# 3D Roller Viewer - Positioning Implementation Guide

## Overview
The 3D Roller Viewer now supports multiple shafts with advanced positioning logic. Shafts can be positioned on the left, center, or right side of the roller assembly, with support for axial (along the roller) and radial (perpendicular) offsets.

## Architecture

### 1. **RollerShaft Model** (`Pulse.Models/Rollers/RollerShaft.cs`)
Added three new positioning properties:

```csharp
/// <summary>
/// Position along the roller axis (mm). 0 = left end, positive = rightward.
/// </summary>
public double AxialPosition { get; set; } = 0;

/// <summary>
/// Which side the shaft is on: "Left", "Right", or "Center" (for concentric shafts).
/// </summary>
public string ShaftPosition { get; set; } = "Center"; // "Left", "Right", "Center"

/// <summary>
/// Offset from the roller centerline in mm (for eccentric positioning).
/// Positive = outward radially.
/// </summary>
public double? RadialOffset { get; set; } = 0;
```

### 2. **DataTransferService** (`Pulse.Web/Services/DataTransferService.cs`)
Manages session state for roller specifications and shafts:
- `GetgvSelectedRollerShafts()` - Returns shafts sorted by position order (Left → Center → Right)
- `SetgvSelectedRollerShafts()` - Sets the shaft collection
- Maintains roller spec and division context

### 3. **3DRoller Razor Component** (`Pulse.Web/Components/Pages/EmployeeZone/Customers/3DRoller.razor`)
Updated initialization flow:
1. Load roller specification
2. Load and add all shafts with positioning
3. Add roller (shell)
4. Add rubber cover
5. Apply final scaling and positioning

Key method: `AddShaftsToSceneAsync()` - Handles multiple shafts with error handling and JSDisconnectedException catching

### 4. **JavaScript Interop** (`Pulse.Web/wwwroot/js/rollerViewer3D.js`)
Three.js implementation with positioning logic:

#### `addShaft()` Parameters:
- `outerDiameter` - Shaft diameter (mm)
- `length` - Shaft length (mm)
- `boreDiameter` - Bearing bore diameter (mm)
- `shaftName` - Unique identifier (e.g., "shaft_0")
- `axialPosition` - Position along roller axis (mm)
- `position` - Side: "left", "center", or "right"
- `radialOffset` - Perpendicular offset (mm)

#### Positioning Logic:
```javascript
// Axial (along roller)
mesh.position.z = axialOffset;

// Side positioning (X-axis)
if (position === "left") {
	mesh.position.x = -radialOffset - 0.05;  // Left side
} else if (position === "right") {
	mesh.position.x = radialOffset + 0.05;   // Right side
} else {
	mesh.position.x = radialOffset;          // Center
}
```

## Usage Examples

### Example 1: Two Shafts (Left and Right)
```csharp
var shafts = new List<RollerShaft>
{
	new RollerShaft
	{
		OuterDiameter = 60,
		Length = 1000,
		BearingBoreDiameter = 30,
		AxialPosition = 0,
		ShaftPosition = "Left",
		RadialOffset = 0
	},
	new RollerShaft
	{
		OuterDiameter = 60,
		Length = 1000,
		BearingBoreDiameter = 30,
		AxialPosition = 0,
		ShaftPosition = "Right",
		RadialOffset = 0
	}
};

await DataTransferService.SetgvSelectedRollerShafts(shafts);
```

### Example 2: Eccentric Shaft (Offset from Center)
```csharp
new RollerShaft
{
	OuterDiameter = 50,
	Length = 1000,
	BearingBoreDiameter = 25,
	AxialPosition = 100,              // 100mm from left end
	ShaftPosition = "Center",
	RadialOffset = 15                 // 15mm offset outward
}
```

### Example 3: Multiple Shafts at Different Positions
```csharp
var shafts = new List<RollerShaft>
{
	new RollerShaft { ShaftPosition = "Left", AxialPosition = 0 },
	new RollerShaft { ShaftPosition = "Center", AxialPosition = 500, RadialOffset = 10 },
	new RollerShaft { ShaftPosition = "Right", AxialPosition = 1000 }
};
```

## Coordinate System

### Three.js Coordinate System:
- **X-axis**: Left (-) to Right (+) - Used for side positioning
- **Y-axis**: Down (-) to Up (+) - Used for vertical positioning
- **Z-axis**: Viewer (-) to Away (+) - Used for axial positioning along roller

### Positioning Mappings:
| Position | X Value | Purpose |
|----------|---------|---------|
| Left | -radialOffset - 0.05 | Shaft on left side |
| Center | radialOffset | Concentric or center axis |
| Right | radialOffset + 0.05 | Shaft on right side |

## Validation

The DataTransferService automatically sorts shafts:
```csharp
_selectedShafts
	.OrderBy(s => GetPositionOrder(s.ShaftPosition))  // Left → Center → Right
	.ThenBy(s => s.AxialPosition)                     // Then by axial position
	.ToList()
```

## Error Handling

The component includes:
- **JSDisconnectedException**: Caught and handled gracefully in all async methods
- **Null validation**: Checks for null shafts and incomplete specifications
- **Dimension validation**: Ensures diameter and length > 0
- **Graceful degradation**: Missing shafts don't prevent roller from loading

## Integration Checklist

- [ ] Add `DataTransferService` to dependency injection in `Program.cs`:
  ```csharp
  builder.Services.AddScoped<DataTransferService>();
  ```

- [ ] Include Three.js library in `_Host.cshtml`:
  ```html
  <script src="~/js/three.module.min.js"></script>
  <script src="~/js/rollerViewer3D.js"></script>
  ```

- [ ] Populate shaft data when loading roller specifications:
  ```csharp
  var shafts = await _context.RollerShafts
	  .Where(s => s.RollerSpecificationId == rollerId)
	  .ToListAsync();
  await DTrf.SetgvSelectedRollerShafts(shafts);
  ```

- [ ] Add ShaftPosition and AxialPosition to database if storing shaft data

## Performance Considerations

- **Shaft Count**: Current implementation supports up to 10 shafts without performance issues
- **Geometry**: Uses efficient Three.js CylinderGeometry (64 radial segments)
- **Animation**: Optional rotation disabled by default (uncomment in `animate()` if needed)
- **Rendering**: Uses WebGL with shadow mapping enabled

## Future Enhancements

1. **Keyway Rendering**: Add keyway cutouts to shafts using CSG operations
2. **Bearing Visualization**: Add bearing components at bore locations
3. **Animation**: Implement rotating assembly for better visualization
4. **Interactivity**: Add drag-and-drop positioning editor
5. **Material Display**: Show different material types with appropriate rendering
6. **Export**: Add ability to export 3D model as GLTF/GLB format
