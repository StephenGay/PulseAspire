using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pulse.Models.Production.Layout
{
    public class FactoryZone
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(5)]
        public string DivisionId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; } = 150;
        public double Height { get; set; } = 100;
        public string Color { get; set; } = "#3b82f6"; // Fluent blue by default
        public string? Notes { get; set; }
    }
}
