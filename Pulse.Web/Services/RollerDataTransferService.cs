using Pulse.Models.Customers;
using Pulse.Models.Organizational;
using Pulse.Models.Rollers;

namespace Pulse.Web.Services
{
    /// <summary>
    /// Service for transferring roller-related data between components via session/state management.
    /// </summary>
    public class RollerDataTransferService
    {
        private ClientRollerSpecification? _selectedRollerSpec;
        private List<RollerShaft>? _selectedShafts;
        private Division? _selectedDivision;
        private int _contextArea = 0;

        /// <summary>
        /// Sets the currently selected roller specification.
        /// </summary>
        public void SetgvSelectedRollSpec(ClientRollerSpecification? roller)
        {
            _selectedRollerSpec = roller;
        }

        /// <summary>
        /// Gets the currently selected roller specification.
        /// </summary>
        public Task<ClientRollerSpecification?> GetgvSelectedRollSpec()
        {
            return Task.FromResult(_selectedRollerSpec);
        }

        /// <summary>
        /// Sets the roller shafts for the current roller.
        /// </summary>
        public async Task SetgvSelectedRollerShafts(List<RollerShaft>? shafts)
        {
            _selectedShafts = shafts;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets the roller shafts for the current roller.
        /// Shafts are returned in their positional order (Left, Center, Right).
        /// </summary>
        public Task<List<RollerShaft>?> GetgvSelectedRollerShafts()
        {
            if (_selectedShafts is null || _selectedShafts.Count == 0)
                return Task.FromResult<List<RollerShaft>?>(null);

            // Sort by position: Left → Center → Right
            var sorted = _selectedShafts
                .OrderBy(s => GetPositionOrder(s.ShaftPosition))
                .ThenBy(s => s.AxialPosition)
                .ToList();

            return Task.FromResult<List<RollerShaft>?>(sorted);
        }

        /// <summary>
        /// Sets the currently selected division.
        /// </summary>
        public void SetgvSelectedDivision(Division? division)
        {
            _selectedDivision = division;
        }

        /// <summary>
        /// Gets the currently selected division.
        /// </summary>
        public Task<Division?> GetgvSelectedDivision()
        {
            return Task.FromResult(_selectedDivision);
        }

        /// <summary>
        /// Sets the context area (used for navigation/context tracking).
        /// </summary>
        public Task SetgvContextArea(int contextArea)
        {
            _contextArea = contextArea;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the current context area.
        /// </summary>
        public int GetgvContextArea() => _contextArea;

        /// <summary>
        /// Helper to determine sort order for shaft positions.
        /// </summary>
        private int GetPositionOrder(string? position)
        {
            return (position?.ToLower()) switch
            {
                "left" => 0,
                "center" => 1,
                "right" => 2,
                _ => 1 // Default to center
            };
        }
    }
}
