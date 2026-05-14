using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.Rollers
{
    public class RollerModelPartVisibility
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool Visible { get; set; } = true;
        public float Opacity { get; set; } = 1.0f;

        // Constructor for safety
        public RollerModelPartVisibility()
        {
            Name = string.Empty;
            DisplayName = string.Empty;
            Visible = true;
            Opacity = 1.0f;
        }
    }
}
