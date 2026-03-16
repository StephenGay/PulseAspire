using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.Rollers;

public class RollerModel3DDto
{
    public string RollerId { get; set; }
    public string RollerName { get; set; }
    public RollerGeometryDto Geometry { get; set; }
    public RollerMaterialDto Material { get; set; }
    public List<RollerComponentDto> Components { get; set; } = new();
}

public class RollerGeometryDto
{
    public float[] Vertices { get; set; } = Array.Empty<float>();
    public int[] Indices { get; set; } = Array.Empty<int>();
    public float[] Normals { get; set; } = Array.Empty<float>();
    public float Radius { get; set; }
    public float Length { get; set; }
}

public class RollerMaterialDto
{
    public string Color { get; set; } = "#808080";
    public float Roughness { get; set; } = 0.7f;
    public float Metalness { get; set; } = 0.3f;
    public float Opacity { get; set; } = 1.0f;
}

public class RollerComponentDto
{
    public string Name { get; set; }
    public string Type { get; set; } // "shell", "compound", "bearing", etc.
    public float[] Position { get; set; } = new float[3];
    public string Color { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}