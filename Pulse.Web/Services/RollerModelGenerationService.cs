//using Pulse.Models.Customers;
//using Pulse.Models.Rollers;
//using System.Numerics;

//namespace Pulse.Web.Services;

//public interface IRollerModelGenerationService
//{
//    Task<RollerModel3DDto> GenerateRollerModelAsync(string rollerId, ClientRoller rollerData);
//}

//public class RollerModelGenerationService : IRollerModelGenerationService
//{
//    private readonly ILogger<RollerModelGenerationService> _logger;
//    private readonly Pulse_AI pApi; // or your AI service

//    public RollerModelGenerationService(
//        ILogger<RollerModelGenerationService> logger,
//        Pulse_AI pApi)
//    {
//        _logger = logger;
//        this.pApi = pApi;
//    }

//    public async Task<RollerModel3DDto> GenerateRollerModelAsync(string rollerId, ClientRoller rollerData)
//    {
//        try
//        {
//            _logger.LogInformation("Generating 3D model for roller: {RollerId}", rollerId);

//            // Build prompt for AI to generate roller specifications
//            var prompt = BuildRollerPrompt(rollerData);

//            // Get AI response with specifications
//            var aiResponse = await pApi.AskPulseAIAsync(prompt);

//            // Parse AI response and generate geometry
//            var geometry = GenerateGeometry(rollerData, aiResponse);
//            var material = GenerateMaterial(rollerData);
//            var components = GenerateComponents(rollerData);

//            return new RollerModel3DDto
//            {
//                RollerId = rollerId,
//                RollerName = rollerData.ClientRollerNumber,
//                Geometry = geometry,
//                Material = material,
//                Components = components
//            };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to generate 3D model for roller {RollerId}", rollerId);
//            throw;
//        }
//    }

//    private string BuildRollerPrompt(ClientRoller roller)
//    {
//        return $@"Generate 3D roller specifications for:
//Name: {roller.ClientRollerNumber}
//Type: {roller.}
//Diameter: {roller.Diameter}mm
//Length: {roller.Length}mm
//Compound: {roller.Compound}
//Shell: {roller.Shell}
//Hardness: {roller.Hardness}

//Provide technical specifications in JSON format with:
//- surface_features: [list of features]
//- layer_thicknesses: {{compound: X, shell: Y}}
//- stress_points: [critical areas]";
//    }

//    private RollerGeometryDto GenerateGeometry(ClientRoller roller, string aiResponse)
//    {
//        // Generate cylinder geometry for roller
//        var (vertices, indices, normals) = GenerateCylinderGeometry(
//            roller.Diameter / 2f,  // radius
//            roller.Length,
//            segments: 32);

//        return new RollerGeometryDto
//        {
//            Vertices = vertices,
//            Indices = indices,
//            Normals = normals,
//            Radius = roller.Diameter / 2f,
//            Length = roller.Length
//        };
//    }

//    private RollerMaterialDto GenerateMaterial(ClientRoller roller)
//    {
//        // Map compound type to color
//        var colorMap = new Dictionary<string, string>
//        {
//            { "natural", "#D2B48C" },
//            { "black", "#1a1a1a" },
//            { "green", "#228B22" },
//            { "blue", "#4169E1" }
//        };

//        return new RollerMaterialDto
//        {
//            Color = colorMap.TryGetValue(roller.Compound?.ToLower() ?? "natural", out var color)
//                ? color
//                : "#808080",
//            Roughness = 0.6f,
//            Metalness = 0.2f,
//            Opacity = 1.0f
//        };
//    }

//    private List<RollerComponentDto> GenerateComponents(ClientRoller roller)
//    {
//        var components = new List<RollerComponentDto>();

//        // Add shell component
//        //if (!string.IsNullOrEmpty(roller.Shell))
//        {
//            components.Add(new RollerComponentDto
//            {
//                Name = "Shell",
//                Type = "shell",
//                Position = new float[] { 0, 0, 0 },
//                Color = "#C0C0C0",
//                //Properties = new() { { "material", roller.Shell } }
//            });
//        }

//        // Add compound layer
//        //if (!string.IsNullOrEmpty(roller.Compound))
//        {
//            components.Add(new RollerComponentDto
//            {
//                Name = "Compound",
//                Type = "compound",
//                Position = new float[] { 0, 0, 0 },
//                //Color = GetCompoundColor(roller.Compound),
//                //Properties = new() { { "type", roller.Compound } }
//            });
//        }

//        // Add stress points as visual indicators
//        components.Add(new RollerComponentDto
//        {
//            Name = "Stress Point",
//            Type = "indicator",
//            Position = new float[] { roller.Diameter / 4f, 0, 0 },
//            Color = "#FF6B6B",
//            Properties = new() { { "severity", "medium" } }
//        });

//        return components;
//    }

//    private (float[], int[], float[]) GenerateCylinderGeometry(float radius, float length, int segments)
//    {
//        var vertices = new List<float>();
//        var indices = new List<int>();
//        var normals = new List<float>();

//        // Generate cylinder vertices
//        for (int i = 0; i <= segments; i++)
//        {
//            float angle = (i / (float)segments) * 2 * MathF.PI;
//            float x = radius * MathF.Cos(angle);
//            float z = radius * MathF.Sin(angle);

//            // Top circle
//            vertices.AddRange(new[] { x, length / 2f, z });
//            normals.AddRange(new[] { x / radius, 0, z / radius });

//            // Bottom circle
//            vertices.AddRange(new[] { x, -length / 2f, z });
//            normals.AddRange(new[] { x / radius, 0, z / radius });
//        }

//        // Generate indices for cylinder sides
//        for (int i = 0; i < segments; i++)
//        {
//            int a = i * 2;
//            int b = a + 1;
//            int c = ((i + 1) % segments) * 2;
//            int d = c + 1;

//            indices.AddRange(new[] { a, b, c });
//            indices.AddRange(new[] { b, d, c });
//        }

//        return (vertices.ToArray(), indices.ToArray(), normals.ToArray());
//    }

//    private string GetCompoundColor(string compoundType) => compoundType?.ToLower() switch
//    {
//        "natural" => "#D2B48C",
//        "black" => "#1a1a1a",
//        "green" => "#228B22",
//        "blue" => "#4169E1",
//        _ => "#808080"
//    };
//}