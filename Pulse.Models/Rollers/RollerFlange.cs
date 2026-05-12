using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.Rollers
{
    public class RollerFlange
    {
        #region Identity
        /// <summary>
        /// Primary key identifier for the flange.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key reference to the parent ClientRollerSpecification.
        /// </summary>
        public int ClientRollerSpecificationId { get; set; }

        /// <summary>
        /// Position of the flange on the roller (e.g., Left, Right).
        /// </summary>
        public FlangePosition Position { get; set; }
        #endregion

        #region Core Geometry
        /// <summary>
        /// Outer diameter of the flange (mm).
        /// </summary>
        public double OuterDiameter { get; set; }
        /// <summary>
        /// Inner diameter of the flange (mm).  Must be smaller than <see cref="OuterDiameter"/>.
        /// </summary>
        public double InnerDiameter { get; set; }
        /// <summary>
        /// Width (thickness) of the flange (mm).
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// Number of bolt-hole sets (e.g., 4-point, 6-point pattern).
        /// </summary>
        public int BoltSetCount { get; set; }
        /// <summary>
        /// Diameter of each bolt hole (mm).  Typical values: 12 mm (M12), 16 mm (M16), 20 mm (M20).
        /// </summary>
        public double BoltHoleDiameter { get; set; }
        /// <summary>
        /// Center-to-center spacing between adjacent bolt holes (mm).
        /// </summary>
        public double BoltHolePitch { get; set; }
        /// <summary>
        /// Optional lip thickness that projects beyond the flange face (mm).
        /// </summary>
        public double LipThickness { get; set; }
        #endregion
        #region Material & Finish
        /// <summary>
        /// Material of the flange (e.g., “20Cr”, “45Steel”).
        /// </summary>
        public string Material { get; set; }
        /// <summary>
        /// Surface roughness (Ra) in micrometres.
        /// </summary>
        public double SurfaceRoughness { get; set; }
        #endregion
        #region Tolerances
        /// <summary>
        /// Tolerance for outer diameter (± mm).
        /// </summary>
        public double OuterDiameterTolerance { get; set; }
        /// <summary>
        /// Tolerance for inner diameter (± mm).
        /// </summary>
        public double InnerDiameterTolerance { get; set; }
        /// <summary>
        /// Tolerance for width (± mm).
        /// </summary>
        public double WidthTolerance { get; set; }
        #endregion
        #region Additional Properties
        /// <summary>
        /// Heat-treatment state of the flange.
        /// </summary>
        public HeatTreatment HeatTreatmentState { get; set; }
        /// <summary>
        /// Surface coating applied to the flange.
        /// </summary>
        public SurfaceCoating Coating { get; set; }
        /// <summary>
        /// Maximum torque (Nm) the flange can transmit without failure.
        /// </summary>
        public double TorqueCapacity { get; set; }
        #endregion

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
        public RollerFlange() { }
        public RollerFlange(
            double outerDiameter,
            double innerDiameter,
            double width,
            double boltHoleDiameter,
            double boltHolePitch,
            int boltSetCount,
            string material,
            double surfaceRoughness,
            HeatTreatment heatTreatment,
            SurfaceCoating coating,
            double torqueCapacity)
        {

            OuterDiameter = outerDiameter;
            InnerDiameter = innerDiameter;
            Width = width;
            BoltHoleDiameter = boltHoleDiameter;
            BoltHolePitch = boltHolePitch;
            BoltSetCount = boltSetCount;
            Material = material;
            SurfaceRoughness = surfaceRoughness;
            HeatTreatmentState = heatTreatment;
            Coating = coating;
            TorqueCapacity = torqueCapacity;
        }
        #endregion
        //#region Properties for Bolt Holes
        ///// <summary>
        ///// Diameter of each bolt hole (mm).  Should match the bolt size used on the flange.
        ///// </summary>
        //public double BoltHoleDiameter { get; set; }
        //    /// <summary>
        //    /// Center-to-center spacing between adjacent bolt holes (mm).
        //    /// </summary>
        //    public double BoltHolePitch { get; set; }
        //    /// <summary>
        //    /// Number of bolt holes per set (e.g., 4-point or 6-point pattern).
        //    /// </summary>
        public int BoltHoleCountPerSet { get; set; }
        //    #endregion
        #region Utility Methods
        /// <summary>
        /// Calculates the flange’s cross-sectional area (outer minus inner).
        /// </summary>
        public double CrossSectionalArea()
        {
            double outerArea = Math.PI * Math.Pow(OuterDiameter / 2.0, 2);
            double innerArea = Math.PI * Math.Pow(InnerDiameter / 2.0, 2);
            return outerArea - innerArea;
        }
        /// <summary>
        /// Validates that all dimensions are physically plausible and within tolerances.
        /// Throws <see cref="ArgumentException"/> if a check fails.
        /// </summary>
        public void ValidateDimensions()
        {
            if (OuterDiameter <= 0)
                throw new ArgumentException("OuterDiameter must be > 0 mm.");
            if (InnerDiameter <= 0 || InnerDiameter >= OuterDiameter)
                throw new ArgumentException("InnerDiameter must be > 0 and < OuterDiameter.");
            if (Width <= 0)
                throw new ArgumentException("Width must be > 0 mm.");
            if (BoltHoleDiameter <= 0)
                throw new ArgumentException("BoltHoleDiameter must be > 0 mm.");
            if (BoltHolePitch <= 0)
                throw new ArgumentException("BoltHolePitch must be > 0 mm.");
            if (BoltHoleCountPerSet <= 0)
                throw new ArgumentException("BoltHoleCountPerSet must be > 0.");
            // Tolerance checks
            if (OuterDiameterTolerance < 0)
                throw new ArgumentException("OuterDiameterTolerance cannot be negative.");
            if (InnerDiameterTolerance < 0)
                throw new ArgumentException("InnerDiameterTolerance cannot be negative.");
            if (WidthTolerance < 0)
                throw new ArgumentException("WidthTolerance cannot be negative.");
        }
        /// <summary>
        /// Generates a very small DXF file that contains a single cylinder representing the flange.
        /// This is a stub – replace with a proper CAD exporter in production.
        /// </summary>
        /// <param name="filePath">Full path to the output file.</param>
        public void GenerateCADFile(string filePath)
        {
            ValidateDimensions();
            var sb = new StringBuilder();
            sb.AppendLine("0");
            sb.AppendLine("SECTION");
            sb.AppendLine("2");
            sb.AppendLine("ENTITIES");
            sb.AppendLine("0");
            sb.AppendLine("LINE");
            sb.AppendLine("8");
            sb.AppendLine("0");
            sb.AppendLine("10");
            sb.AppendLine("0.0");
            sb.AppendLine("20");
            sb.AppendLine("0.0");
            sb.AppendLine("30");
            sb.AppendLine("0.0");
            sb.AppendLine("11");
            sb.AppendLine("0.0");
            sb.AppendLine("21");
            sb.AppendLine("0.0");
            sb.AppendLine("31");
            sb.AppendLine("0.0");
            sb.AppendLine("0");
            sb.AppendLine("ENDSEC");
            sb.AppendLine("0");
            sb.AppendLine("EOF");
            File.WriteAllText(filePath, sb.ToString());
        }
        /// <summary>
        /// Creates a JavaScript snippet that builds a Three.js object for this flange.
        /// The snippet can be imported into a Three.js scene.
        /// </summary>
        /// <param name="objectName">Variable name for the Three.js object.</param>
        /// <param name="materialName">Three.js material name.</param>
        /// <returns>JavaScript code as a string.</returns>
        public string CreateThreeJSObject(string objectName = "flange", string materialName = "flangeMat")
        {
            // Convert mm → m (Three.js convention)
            double outerRad = OuterDiameter / 2000.0;   // radius in m
            double innerRad = InnerDiameter / 2000.0;   // radius in m
            double height = Width / 1000.0;            // flange thickness in m
            var sb = new StringBuilder();
            sb.AppendLine("// Three.js representation of a roller flange");
            sb.AppendLine($"const {materialName} = new THREE.MeshStandardMaterial({{ color: 0x888888 }});");
            sb.AppendLine($"const {objectName} = new THREE.Group();");
            // Outer cylinder
            sb.AppendLine($"const outerGeom = new THREE.CylinderGeometry(");
            sb.AppendLine($"    {outerRad:F4}, // radiusTop");
            sb.AppendLine($"    {outerRad:F4}, // radiusBottom");
            sb.AppendLine($"    {height:F4},   // height");
            sb.AppendLine($"    64,            // radialSegments");
            sb.AppendLine($"    1             // heightSegments");
            sb.AppendLine($");");
            sb.AppendLine($"const outerMesh = new THREE.Mesh(outerGeom, {materialName});");
            sb.AppendLine($"outerMesh.rotation.x = Math.PI / 2; // align with Y-axis");
            sb.AppendLine($"{objectName}.add(outerMesh);");
            // Inner cylinder (bore) – shown as a semi-transparent mesh for visualisation
            sb.AppendLine($"const innerGeom = new THREE.CylinderGeometry(");
            sb.AppendLine($"    {innerRad:F4}, // radiusTop");
            sb.AppendLine($"    {innerRad:F4}, // radiusBottom");
            sb.AppendLine($"    {height:F4},   // height");
            sb.AppendLine($"    64,            // radialSegments");
            sb.AppendLine($"    1             // heightSegments");
            sb.AppendLine($");");
            sb.AppendLine($"const innerMesh = new THREE.Mesh(innerGeom, new THREE.MeshBasicMaterial({{ color: 0x000000, opacity: 0.4, transparent: true }}));");
    
            sb.AppendLine($"innerMesh.rotation.x = Math.PI / 2;");
            sb.AppendLine($"{objectName}.add(innerMesh);");
            // Optional: add bolt-hole placeholders (thin boxes) – can be subtracted with CSG if required
            sb.AppendLine($"// Bolt-hole placeholders – visible only for debugging");
            sb.AppendLine($"const holeGeom = new THREE.BoxGeometry(");
            sb.AppendLine($"    {BoltHoleDiameter / 1000.0:F4}, // width");
            sb.AppendLine($"    {height / 2.0:F4},          // height");
            sb.AppendLine($"    {height / 2.0:F4}           // depth");
            sb.AppendLine($");");
            sb.AppendLine($"const holeMesh = new THREE.Mesh(holeGeom, new THREE.MeshBasicMaterial({{ color: 0xff0000, opacity: 0.2, transparent: true }}));");
    
            sb.AppendLine($"holeMesh.position.set(0, 0, 0);");
            sb.AppendLine($"{objectName}.add(holeMesh);");
            sb.AppendLine($"export default {objectName};");
            return sb.ToString();
        }
        /// <summary>
        /// Convenience wrapper that writes the Three.js code to a file.
        /// </summary>
        /// <param name="filePath">Full path to the output .js file.</param>
        /// <param name="objectName">Variable name for the Three.js object.</param>
        /// <param name="materialName">Three.js material name.</param>
        public void WriteThreeJSFile(string filePath, string objectName = "flange", string materialName = "flangeMat")
        {
            var js = CreateThreeJSObject(objectName, materialName);
            File.WriteAllText(filePath, js);
        }
        #endregion
        #region Overrides
        public override string ToString()
        {
            return $"RollerFlange: Ø{OuterDiameter}mm × Ø{InnerDiameter}mm × {Width}mm, " +
                   $"bolt-hole {BoltHoleDiameter}mm × {BoltHolePitch}mm, material {Material}, " +
                   $"heat-treatment {HeatTreatmentState}, coating {Coating}, torque {TorqueCapacity}Nm";
        }
        #endregion
    }
}
