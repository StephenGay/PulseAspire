using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.Rollers
{
    public partial class RollerShaft
    {
        #region Identity
        /// <summary>
        /// Primary key identifier for the shaft.
        /// </summary>
        public int Id { get; set; }

        public required int ClientRollerSpecificationID { get; set; }
        #endregion

        #region Core Geometry
        /// <summary>
        /// Total length of the shaft (including end faces).
        /// </summary>
        public double Length { get; set; }
        /// <summary>
        /// Outer diameter of the shaft.
        /// </summary>
        public double OuterDiameter { get; set; }
        /// <summary>
        /// Inner bore diameter that mates with the bearing.
        /// </summary>
        public double BearingBoreDiameter { get; set; }
        /// <summary>
        /// Width of the keyway (if present).
        /// </summary>
        public double KeywayWidth { get; set; }
        /// <summary>
        /// Depth of the keyway.
        /// </summary>
        public double KeywayDepth { get; set; }
        /// <summary>
        /// Length of the keyway along the shaft.
        /// </summary>
        public double KeywayLength { get; set; }
        /// <summary>
        /// Thickness of the shaft end faces (used for mounting/bearing clearance).
        /// </summary>
        public double EndFaceThickness { get; set; }
        #endregion

        #region Material & Finish
        /// <summary>
        /// Material of the shaft (e.g., "20Cr", "45Steel", "AlloyX").
        /// </summary>
        public string Material { get; set; }
        /// <summary>
        /// Surface roughness (Ra) in micrometres.
        /// </summary>
        public double SurfaceRoughness { get; set; }
        #endregion

        #region Tolerances
        /// <summary>
        /// Tolerance for outer diameter (± mm).
        /// </summary>
        public double OuterDiameterTolerance { get; set; }
        /// <summary>
        /// Tolerance for bearing bore diameter (± mm).
        /// </summary>
        public double BearingBoreTolerance { get; set; }
        /// <summary>
        /// Tolerance for keyway dimensions (± mm).
        /// </summary>
        public double KeywayTolerance { get; set; }
        #endregion

        #region Cover
        /// <summary>
        /// Cover compound used on the shaft (if applicable).
        /// </summary>
        public string? CoverCompound { get; set; }

        public double? CoverCompoundThickness { get; set; } = null;
        public double? CoverOverbuild { get; set; } = null;
        #endregion

        #region Positioning
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
        #endregion

        #region Additional Properties

        /// <summary>
        /// Heat-treatment state of the shaft.
        /// </summary>
        public HeatTreatment HeatTreatmentState { get; set; }
        /// <summary>
        /// Surface coating applied to the shaft.
        /// </summary>
        public SurfaceCoating Coating { get; set; }
        /// <summary>
        /// Maximum torque (Nm) the shaft can transmit without failure.
        /// </summary>
        public double TorqueCapacity { get; set; }
        #endregion

        public bool IsActive { get; set; } = true;

        #region Audit Fields
        /// <summary>
        /// UTC timestamp when the record was created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// UTC timestamp when the record was last modified.
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// User ID or name who created this record.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// User ID or name who last modified this record.
        /// </summary>
        public string? ModifiedBy { get; set; }
        #endregion
        #region Constructors
        /// <summary>
        /// Default constructor – creates an empty shaft definition.
        /// </summary>
        public RollerShaft() { }

        /// <summary>
        /// Convenience constructor to initialise all core properties.
        /// </summary>
        public RollerShaft(
            double length,
            double outerDiameter,
            double bearingBoreDiameter,
            double keywayWidth,
            double keywayDepth,
            double keywayLength,
            double endFaceThickness,
            string material,
            string coverCompound,
            double? coverCompoundThickness,
            double? coverOverbuild,
            double surfaceRoughness,
            HeatTreatment heatTreatment,
            SurfaceCoating coating,
            double torqueCapacity)
        {
            Length = length;
            OuterDiameter = outerDiameter;
            BearingBoreDiameter = bearingBoreDiameter;
            KeywayWidth = keywayWidth;
            KeywayDepth = keywayDepth;
            KeywayLength = keywayLength;
            EndFaceThickness = endFaceThickness;
            Material = material;
            SurfaceRoughness = surfaceRoughness;
            HeatTreatmentState = heatTreatment;
            Coating = coating;
            TorqueCapacity = torqueCapacity;
            CoverCompound = coverCompound;
            CoverCompoundThickness = coverCompoundThickness;
            CoverOverbuild = coverOverbuild;
        }

        #endregion

        #region Utility Methods
        /// <summary>
        /// Calculates the shaft’s cross-sectional area (outer minus bore).
        /// </summary>
        public double CrossSectionalArea()
        {
            double outerArea = Math.PI * Math.Pow(OuterDiameter / 2.0, 2);
            double boreArea = Math.PI * Math.Pow(BearingBoreDiameter / 2.0, 2);
            return outerArea - boreArea;
        }
        /// <summary>
        /// Returns a human-readable description of the shaft.
        /// </summary>
        public override string ToString()
        {
            return $"RollerShaft: {Length}mm × Ø{OuterDiameter}mm, bore Ø{BearingBoreDiameter}mm, " +
                   $"keyway {KeywayWidth}×{KeywayDepth}×{KeywayLength}mm, material {Material}";
        }

        /// <summary>
        /// Validates that all dimensions are within realistic bounds and tolerances.
        /// Throws <see cref="ArgumentException"/> if a check fails.
        /// </summary>
        public void ValidateDimensions()
        {
            if (Length <= 0)
                throw new ArgumentException("Length must be > 0 mm.");
            if (OuterDiameter <= 0)
                throw new ArgumentException("OuterDiameter must be > 0 mm.");
            if (BearingBoreDiameter <= 0 || BearingBoreDiameter >= OuterDiameter)
                throw new ArgumentException("BearingBoreDiameter must be > 0 and < OuterDiameter.");
            if (KeywayWidth < 0 || KeywayDepth < 0 || KeywayLength < 0)
                throw new ArgumentException("Keyway dimensions cannot be negative.");
            if (KeywayWidth > OuterDiameter / 2)
                throw new ArgumentException("Keyway width cannot exceed half the outer diameter.");
            if (EndFaceThickness < 0)
                throw new ArgumentException("EndFaceThickness cannot be negative.");
            // Tolerance checks
            if (OuterDiameterTolerance < 0)
                throw new ArgumentException("OuterDiameterTolerance cannot be negative.");
            if (BearingBoreTolerance < 0)
                throw new ArgumentException("BearingBoreTolerance cannot be negative.");
            if (KeywayTolerance < 0)
                throw new ArgumentException("KeywayTolerance cannot be negative.");
        }



        #endregion

        
    }

    public partial class RollerShaft
    {
        public string CreateThreeJSObject(string objectName = "shaft", string materialName = "shaftMaterial")
        {
            // Convert dimensions from mm to meters (Three.js uses meters by convention)
            double lengthM = Length / 1000.0;
            double outerDiaM = OuterDiameter / 1000.0;
            double boreDiaM = BearingBoreDiameter / 1000.0;
            // Three.js CylinderGeometry: radiusTop, radiusBottom, height, radialSegments, heightSegments
            // We’ll create a simple cylinder and then subtract a smaller cylinder (the bore)
            // For a real keyway you’d need a CSG library – here we just show the basic shape.
            var sb = new StringBuilder();
            sb.AppendLine("// Three.js representation of the roller shaft");
            sb.AppendLine($"const {materialName} = new THREE.MeshStandardMaterial({{ color: 0x888888 }});");
            sb.AppendLine($"const {objectName} = new THREE.Group();");
            // Outer cylinder
            sb.AppendLine($"const outerGeom = new THREE.CylinderGeometry(");
            sb.AppendLine($"    {outerDiaM / 2.0:F4}, // radiusTop");
            sb.AppendLine($"    {outerDiaM / 2.0:F4}, // radiusBottom");
            sb.AppendLine($"    {lengthM:F4},        // height");
            sb.AppendLine($"    64,                  // radialSegments");
            sb.AppendLine($"    1                    // heightSegments");
            sb.AppendLine($");");
            sb.AppendLine($"const outerMesh = new THREE.Mesh(outerGeom, {materialName});");
            sb.AppendLine($"outerMesh.rotation.x = Math.PI / 2; // align with Y-axis");
            sb.AppendLine($"{objectName}.add(outerMesh);");
            // Bore (inner cylinder) – we’ll just add it as a separate mesh for visualisation
            sb.AppendLine($"const boreGeom = new THREE.CylinderGeometry(");
            sb.AppendLine($"    {boreDiaM / 2.0:F4}, // radiusTop");
            sb.AppendLine($"    {boreDiaM / 2.0:F4}, // radiusBottom");
            sb.AppendLine($"    {lengthM:F4},        // height");
            sb.AppendLine($"    64,                  // radialSegments");
            sb.AppendLine($"    1                    // heightSegments");
            sb.AppendLine($");");
            sb.AppendLine($"const boreMesh = new THREE.Mesh(boreGeom, new THREE.MeshBasicMaterial({{ color: 0x000000, opacity: 0.5, transparent: true }}));");
            sb.AppendLine($"boreMesh.rotation.x = Math.PI / 2;");
            sb.AppendLine($"{objectName}.add(boreMesh);");
            // Optional: add a simple keyway as a box cut-out (placeholder)
            sb.AppendLine($"// Keyway placeholder – a thin box that can be subtracted with CSG if needed");
            sb.AppendLine($"const keywayGeom = new THREE.BoxGeometry(");
            sb.AppendLine($"    {KeywayWidth / 1000.0:F4}, // width");
            sb.AppendLine($"    {KeywayDepth / 1000.0:F4}, // height");
            sb.AppendLine($"    {KeywayLength / 1000.0:F4} // depth");
            sb.AppendLine($");");
            sb.AppendLine($"const keywayMesh = new THREE.Mesh(keywayGeom, new THREE.MeshBasicMaterial({{ color: 0xff0000, opacity: 0.3, transparent: true }}));");
            sb.AppendLine($"keywayMesh.position.set(0, 0, 0);");
            sb.AppendLine($"{objectName}.add(keywayMesh);");
            sb.AppendLine($"// Export the group so it can be added to a scene");
            sb.AppendLine($"export default {objectName};");
            return sb.ToString();
        }
        /// <summary>
        /// Convenience wrapper that writes the Three.js code to a file.
        /// </summary>
        /// <param name="filePath">Full path to the output .js file.</param>
        /// <param name="objectName">Variable name for the Three.js object.</param>
        /// <param name="materialName">Three.js material name.</param>
        public void WriteThreeJSFile(string filePath, string objectName = "shaft", string materialName = "shaftMaterial")
        {
            var jsCode = CreateThreeJSObject(objectName, materialName);
            File.WriteAllText(filePath, jsCode);
        }
    }
    
}