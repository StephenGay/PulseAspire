using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.CustomComponents
{
    public record ClientRollerDto(
    int ClientRollerID,
    int ClientRollerSpecificationID,
    string ClientRollerNumber,
    decimal ShellDiameter,
    decimal ShellLength,
    decimal ShellWeight,
    decimal CoverDiameter,
    string? ShellDefects,
    bool IsActive,
    ICollection<WorksOrderDto> WorksOrders);

    public record WorksOrderDto(
        int WorksOrderNo,
        string FullClientID,
        int PeriodID,
        DateTime DateStarted,
        string? Description,
        int WorkTypeID,
        decimal Quantity,
        string? DivisionID,
        int? ClientRollerSpecificationID,
        int? ClientRollerID,
        string? ClientRollNo,
        string? CoverCompoundCode,
        string? ClientOrderNo,
        string? ClientPRNo,
        string? ClientRFQNo,
        decimal? ShellLength,
        decimal? ShellDiameter,
        decimal? CoverDiameter,
        decimal UndelQty,
        decimal MaterialCost,
        decimal SellPrice,
        string? Status,
        // sub-objects (only the fields you need)
        WOCustomerDto? Customer,
        PeriodDto? Period,
        WorkTypeDto? WorkType,
        DivisionDto? Division,
        ClientRollerSpecificationDto? ClientRollerSpecification,
        ClientRollerDto? ClientRoller,   // minimal self-reference
        CompoundDto? Compound);

    public record WOCustomerDto(string FullClientID, string ClientName, string TaxCodeID, string FullChargeClientID, int CompanyID, string ClientID);
    public record PeriodDto(int PeriodID, string Month, string CalendarYear, string FinancialYear, DateTime StartDate);
    public record WorkTypeDto(int WorkTypeID, string WorkTypeName);
    public record DivisionDto(string DivisionID, string DivisionName, int CompanyID, int BranchID);
    public record ClientRollerSpecificationDto(int ClientRollerSpecificationID, string FullClientID);
    public record CompoundDto(string CompoundCode, string CompoundDescription, string CompoundType);
}
