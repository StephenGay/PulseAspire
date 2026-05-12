using Pulse.Models.Rollers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pulse.Models.Validators
{
    /// <summary>
    /// Validation helper for RollerFlange entities.
    /// Provides fluent validation and business rule checks.
    /// </summary>
    public class RollerFlangeValidator
    {
        private readonly RollerFlange _flange;
        private readonly List<string> _errors = new();

        public RollerFlangeValidator(RollerFlange flange)
        {
            _flange = flange ?? throw new ArgumentNullException(nameof(flange));
        }

        /// <summary>
        /// Validates core geometry dimensions.
        /// </summary>
        public RollerFlangeValidator ValidateGeometry()
        {
            if (_flange.OuterDiameter <= 0)
                _errors.Add("OuterDiameter must be greater than 0 mm");

            if (_flange.InnerDiameter <= 0)
                _errors.Add("InnerDiameter must be greater than 0 mm");

            if (_flange.InnerDiameter >= _flange.OuterDiameter)
                _errors.Add("InnerDiameter must be less than OuterDiameter");

            if (_flange.Width <= 0)
                _errors.Add("Width must be greater than 0 mm");

            if (_flange.LipThickness < 0)
                _errors.Add("LipThickness cannot be negative");

            return this;
        }

        /// <summary>
        /// Validates bolt hole specifications.
        /// </summary>
        public RollerFlangeValidator ValidateBoltHoles()
        {
            if (_flange.BoltHoleDiameter <= 0)
                _errors.Add("BoltHoleDiameter must be greater than 0 mm");

            if (_flange.BoltHoleDiameter >= _flange.InnerDiameter)
                _errors.Add("BoltHoleDiameter must be less than InnerDiameter");

            if (_flange.BoltHolePitch <= 0)
                _errors.Add("BoltHolePitch must be greater than 0 mm");

            if (_flange.BoltSetCount <= 0)
                _errors.Add("BoltSetCount must be greater than 0");

            if (_flange.BoltHoleCountPerSet <= 0)
                _errors.Add("BoltHoleCountPerSet must be greater than 0");

            // Validate pitch vs diameter
            if (_flange.BoltHolePitch <= _flange.BoltHoleDiameter)
                _errors.Add($"BoltHolePitch ({_flange.BoltHolePitch}mm) should be greater than BoltHoleDiameter ({_flange.BoltHoleDiameter}mm) to avoid interference");

            // Common bolt patterns
            var validBoltSets = new[] { 2, 4, 6, 8 };
            if (!validBoltSets.Contains(_flange.BoltSetCount))
                _errors.Add($"BoltSetCount {_flange.BoltSetCount} is unusual; typical values are: {string.Join(", ", validBoltSets)}");

            return this;
        }

        /// <summary>
        /// Validates tolerances.
        /// </summary>
        public RollerFlangeValidator ValidateTolerances()
        {
            if (_flange.OuterDiameterTolerance < 0)
                _errors.Add("OuterDiameterTolerance cannot be negative");

            if (_flange.InnerDiameterTolerance < 0)
                _errors.Add("InnerDiameterTolerance cannot be negative");

            if (_flange.WidthTolerance < 0)
                _errors.Add("WidthTolerance cannot be negative");

            // Tolerance should typically be < diameter/10
            if (_flange.OuterDiameterTolerance > _flange.OuterDiameter / 10)
                _errors.Add("OuterDiameterTolerance seems unusually large (> 10% of diameter)");

            if (_flange.InnerDiameterTolerance > _flange.InnerDiameter / 10)
                _errors.Add("InnerDiameterTolerance seems unusually large (> 10% of diameter)");

            return this;
        }

        /// <summary>
        /// Validates material and surface properties.
        /// </summary>
        public RollerFlangeValidator ValidateMaterial()
        {
            if (string.IsNullOrEmpty(_flange.Material))
                _errors.Add("Material is required");

            if (_flange.SurfaceRoughness < 0)
                _errors.Add("SurfaceRoughness cannot be negative");

            if (_flange.SurfaceRoughness > 50) // Typical max is ~25µm
                _errors.Add("SurfaceRoughness seems unusually high (typically < 25µm)");

            return this;
        }

        /// <summary>
        /// Validates performance specifications.
        /// </summary>
        public RollerFlangeValidator ValidatePerformance()
        {
            if (_flange.TorqueCapacity < 0)
                _errors.Add("TorqueCapacity cannot be negative");

            return this;
        }

        /// <summary>
        /// Validates that flange dimensions are realistic for common manufacturing scenarios.
        /// </summary>
        public RollerFlangeValidator ValidateRealism()
        {
            // Bore should typically be 50-95% of outer diameter
            double boreRatio = _flange.InnerDiameter / _flange.OuterDiameter;
            if (boreRatio < 0.5 || boreRatio > 0.95)
                _errors.Add($"Inner/Outer diameter ratio ({boreRatio:P0}) seems unusual for a flange");

            // Width should typically be 10-30% of outer diameter
            double widthRatio = _flange.Width / _flange.OuterDiameter;
            if (widthRatio < 0.05 || widthRatio > 0.5)
                _errors.Add($"Width/Diameter ratio ({widthRatio:P0}) seems unusual for a flange");

            // Bolt hole pitch should be close to inner diameter
            double pitchToInnerRatio = _flange.BoltHolePitch / _flange.InnerDiameter;
            if (pitchToInnerRatio < 0.9 || pitchToInnerRatio > 2.0)
                _errors.Add($"BoltHolePitch vs InnerDiameter ratio ({pitchToInnerRatio:F2}) seems unusual");

            return this;
        }

        /// <summary>
        /// Performs all validations.
        /// </summary>
        public RollerFlangeValidator ValidateAll()
        {
            return ValidateGeometry()
                .ValidateBoltHoles()
                .ValidateTolerances()
                .ValidateMaterial()
                .ValidatePerformance()
                .ValidateRealism();
        }

        /// <summary>
        /// Gets all validation errors.
        /// </summary>
        public IReadOnlyList<string> GetErrors() => _errors.AsReadOnly();

        /// <summary>
        /// Checks if validation passed (no errors).
        /// </summary>
        public bool IsValid => _errors.Count == 0;

        /// <summary>
        /// Gets validation errors as a single formatted string.
        /// </summary>
        public string GetErrorMessage() => string.Join("; ", _errors);

        /// <summary>
        /// Throws an exception if validation failed.
        /// </summary>
        public void ThrowIfInvalid()
        {
            if (!IsValid)
                throw new InvalidOperationException($"RollerFlange validation failed: {GetErrorMessage()}");
        }

        /// <summary>
        /// Static helper to validate and throw.
        /// </summary>
        public static void ValidateAndThrow(RollerFlange flange)
        {
            new RollerFlangeValidator(flange)
                .ValidateAll()
                .ThrowIfInvalid();
        }

        /// <summary>
        /// Static helper to validate silently.
        /// </summary>
        public static bool TryValidate(RollerFlange flange, out List<string> errors)
        {
            var validator = new RollerFlangeValidator(flange)
                .ValidateAll();

            errors = validator._errors;
            return validator.IsValid;
        }
    }

    /// <summary>
    /// Extension methods for RollerFlange validation.
    /// </summary>
    public static class RollerFlangeValidationExtensions
    {
        /// <summary>
        /// Creates a validator for the flange.
        /// </summary>
        public static RollerFlangeValidator Validate(this RollerFlange flange)
        {
            return new RollerFlangeValidator(flange);
        }

        /// <summary>
        /// Validates all flanges in a collection.
        /// </summary>
        public static Dictionary<int, List<string>> ValidateAll(this IEnumerable<RollerFlange> flanges)
        {
            var results = new Dictionary<int, List<string>>();

            foreach (var (flange, index) in flanges.Select((f, i) => (f, i)))
            {
                if (RollerFlangeValidator.TryValidate(flange, out var errors))
                {
                    if (errors.Count > 0)
                        results[index] = errors;
                }
            }

            return results;
        }
    }
}
