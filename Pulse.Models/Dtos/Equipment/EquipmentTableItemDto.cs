using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.Dtos.Equipment;

public record EquipmentTableItemDto
(
    string? EquipmentID,
    string? Description,
    string? Category,
    string? ManufacturerName,
    string? Model,
    bool isOperational
);
