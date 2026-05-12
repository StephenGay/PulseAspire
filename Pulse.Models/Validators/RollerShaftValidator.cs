using Pulse.Models.Rollers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pulse.Models.Validators
{
    /// <summary>
    /// Validation helper for RollerShaft entities.
    /// Provides fluent validation and business rule checks.
    /// </summary>
    public class RollerShaftValidator
    {
        private readonly RollerShaft _shaft;
        private readonly List<string> _errors = new();

        public RollerShaftValidator(RollerShaft shaft)
        {
            _shaft = shaft ?? throw new ArgumentNullException(nameof(shaft));
        }

        /// <summary>
        /// Validates core geometry dimensions.
        /// </summary>
        public RollerShaftValidator ValidateGeometry()
        {
            if (_shaft.Length <= 0)
                _errors.Add("Length must be greater than 0 mm");

            if (_shaft.OuterDiameter <= 0)
                _errors.Add("OuterDiameter must be greater than 0 mm");

            if (_shaft.BearingBoreDiameter <= 0)
                _errors.Add("BearingBoreDiameter must be greater than 0 mm");

            if (_shaft.BearingBoreDiameter >= _shaft.OuterDiameter)
                _errors.Add("BearingBoreDiameter must be less than OuterDiameter");

            if (_shaft.EndFaceThickness < 0)
                _errors.Add("EndFaceThickness cannot be negative");

            return this;
        }

        /// <summary>
        /// Validates keyway dimensions.
        /// </summary>
        public RollerShaftValidator ValidateKeyway()
        {
            if (_shaft.KeywayWidth < 0)
                _errors.Add("KeywayWidth cannot be negative");

            if (_shaft.KeywayDepth < 0)
                _errors.Add("KeywayDepth cannot be negative");

            if (_shaft.KeywayLength < 0)
                _errors.Add("KeywayLength cannot be negative");

            if (_shaft.KeywayWidth > 0 && _shaft.KeywayWidth > _shaft.OuterDiameter / 2)
                _errors.Add("KeywayWidth cannot exceed half the outer diameter");

            if (_shaft.KeywayDepth > 0 && _shaft.KeywayDepth >= _shaft.OuterDiameter / 2)
                _errors.Add("KeywayDepth cannot exceed half the outer diameter");

            return this;
        }

        /// <summary>
        /// Validates tolerances.
        /// </summary>
        public RollerShaftValidator ValidateTolerances()
        {
            if (_shaft.OuterDiameterTolerance < 0)
                _errors.Add("OuterDiameterTolerance cannot be negative");

            if (_shaft.BearingBoreTolerance < 0)
                _errors.Add("BearingBoreTolerance cannot be negative");

            if (_shaft.KeywayTolerance < 0)
                _errors.Add("KeywayTolerance cannot be negative");

            // Tolerance should typically be < diameter/10
            if (_shaft.OuterDiameterTolerance > _shaft.OuterDiameter / 10)
                _errors.Add("OuterDiameterTolerance seems unusually large (> 10% of diameter)");

            return this;
        }

        /// <summary>
        /// Validates cover specifications.
        /// </summary>
        public RollerShaftValidator ValidateCover()
        {
            if (!string.IsNullOrEmpty(_shaft.CoverCompound))
            {
                if (_shaft.CoverCompoundThickness.HasValue && _shaft.CoverCompoundThickness <= 0)
                    _errors.Add("CoverCompoundThickness must be greater than 0 mm when cover compound is specified");

                if (_shaft.CoverOverbuild.HasValue && _shaft.CoverOverbuild <= 0)
                    _errors.Add("CoverOverbuild must be greater than 0 mm when cover compound is specified");
            }

            return this;
        }

        /// <summary>
        /// Validates positioning properties.
        /// </summary>
        public RollerShaftValidator ValidatePositioning()
        {
            if (string.IsNullOrEmpty(_shaft.ShaftPosition))
            {
                _errors.Add("ShaftPosition is required");
                return this;
            }

            var validPositions = new[] { "Left", "Center", "Right" };
            if (!validPositions.Contains(_shaft.ShaftPosition))
                _errors.Add($"ShaftPosition must be one of: {string.Join(", ", validPositions)}");

            if (_shaft.AxialPosition < 0)
                _errors.Add("AxialPosition cannot be negative");

            if (_shaft.RadialOffset.HasValue && _shaft.RadialOffset < 0)
                _errors.Add("RadialOffset cannot be negative");

            return this;
        }

        /// <summary>
        /// Validates material and surface properties.
        /// </summary>
        public RollerShaftValidator ValidateMaterial()
        {
            if (string.IsNullOrEmpty(_shaft.Material))
                _errors.Add("Material is required");

            if (_shaft.SurfaceRoughness < 0)
                _errors.Add("SurfaceRoughness cannot be negative");

            if (_shaft.SurfaceRoughness > 50) // Typical max is ~25µm
                _errors.Add("SurfaceRoughness seems unusually high (typically < 25µm)");

            return this;
        }

        /// <summary>
        /// Validates performance specifications.
        /// </summary>
        public RollerShaftValidator ValidatePerformance()
        {
            if (_shaft.TorqueCapacity < 0)
                _errors.Add("TorqueCapacity cannot be negative");

            return this;
        }

        /// <summary>
        /// Performs all validations.
        /// </summary>
        public RollerShaftValidator ValidateAll()
        {
            return ValidateGeometry()
                .ValidateKeyway()
                .ValidateTolerances()
                .ValidateCover()
                .ValidatePositioning()
                .ValidateMaterial()
                .ValidatePerformance();
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
                throw new InvalidOperationException($"RollerShaft validation failed: {GetErrorMessage()}");
        }

        /// <summary>
        /// Static helper to validate and throw.
        /// </summary>
        public static void ValidateAndThrow(RollerShaft shaft)
        {
            new RollerShaftValidator(shaft)
                .ValidateAll()
                .ThrowIfInvalid();
        }

        /// <summary>
        /// Static helper to validate silently.
        /// </summary>
        public static bool TryValidate(RollerShaft shaft, out List<string> errors)
        {
            var validator = new RollerShaftValidator(shaft)
                .ValidateAll();

            errors = validator._errors;
            return validator.IsValid;
        }
    }

    /// <summary>
    /// Extension methods for RollerShaft validation.
    /// </summary>
    public static class RollerShaftValidationExtensions
    {
        /// <summary>
        /// Creates a validator for the shaft.
        /// </summary>
        public static RollerShaftValidator Validate(this RollerShaft shaft)
        {
            return new RollerShaftValidator(shaft);
        }

        /// <summary>
        /// Validates all shafts in a collection.
        /// </summary>
        public static Dictionary<int, List<string>> ValidateAll(this IEnumerable<RollerShaft> shafts)
        {
            var results = new Dictionary<int, List<string>>();

            foreach (var (shaft, index) in shafts.Select((s, i) => (s, i)))
            {
                if (RollerShaftValidator.TryValidate(shaft, out var errors))
                {
                    if (errors.Count > 0)
                        results[index] = errors;
                }
            }

            return results;
        }
    }
}
