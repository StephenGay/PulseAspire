using Pulse.Models.Compounds;
using Pulse.Models.CustomComponents;
using Pulse.Models.Customers;
using Pulse.Models.Misc;
using Pulse.Models.Organizational;
using Pulse.Models.Production;

public static class MappingExtensions
{
    public static ICollection<ClientRoller> ToClientRollers(this IEnumerable<ClientRollerDto> dtos) =>
        dtos.Select(dto => new ClientRoller
        {
            ClientRollerID = dto.ClientRollerID,
            ClientRollerSpecificationID = dto.ClientRollerSpecificationID,
            ClientRollerNumber = dto.ClientRollerNumber,
            ShellDiameter = dto.ShellDiameter,
            ShellLength = dto.ShellLength,
            ShellWeight = dto.ShellWeight,
            CoverDiameter = dto.CoverDiameter,
            ShellDefects = dto.ShellDefects,
            IsActive = dto.IsActive,
            WorksOrders = dto.WorksOrders?.Select(wo => new WorksOrder
            {
                WorksOrderNo = wo.WorksOrderNo,
                FullClientID = wo.FullClientID,
                PeriodID = wo.PeriodID,
                DateStarted = wo.DateStarted,
                Description = wo.Description,
                WorkTypeID = wo.WorkTypeID,
                Quantity = wo.Quantity,
                DivisionID = wo.DivisionID,
                ClientRollerSpecificationID = wo.ClientRollerSpecificationID,
                ClientRollerID = wo.ClientRollerID,
                ClientRollNo = wo.ClientRollNo,
                CoverCompoundCode = wo.CoverCompoundCode,
                ClientOrderNo = wo.ClientOrderNo,
                ClientPRNo = wo.ClientPRNo,
                ClientRFQNo = wo.ClientRFQNo,
                ShellLength = wo.ShellLength,
                ShellDiameter = wo.ShellDiameter,
                CoverDiameter = wo.CoverDiameter,
                UndelQty = wo.UndelQty,
                MaterialCost = wo.MaterialCost,
                SellPrice = wo.SellPrice,
                Status = wo.Status,
                // ---- navigation objects (minimal) ----
                Customer = wo.Customer != null
                    ? new Customer { FullClientID = wo.Customer.FullClientID, ClientName = wo.Customer.ClientName, TaxCodeID = wo.Customer.TaxCodeID, FullChargeClientID = wo.Customer.FullChargeClientID, CompanyID = wo.Customer.CompanyID, ClientID = wo.Customer.ClientID }
                    : null,
                Period = wo.Period != null
                    ? new Period { PeriodID = wo.Period.PeriodID, Month = wo.Period.Month, CalendarYear = wo.Period.CalendarYear, FinancialYear = wo.Period.FinancialYear, StartDate = wo.Period.StartDate }
                    : null,
                WorkType = wo.WorkType != null
                    ? new WorkType { WorkTypeID = wo.WorkType.WorkTypeID, WorkTypeName = wo.WorkType.WorkTypeName }
                    : null,
                Division = wo.Division != null
                    ? new Division { DivisionID = wo.Division.DivisionID, DivisionName = wo.Division.DivisionName, CompanyID = wo.Division.CompanyID, BranchID = wo.Division.BranchID }
                    : null,
                ClientRollerSpecification = wo.ClientRollerSpecification != null
                    ? new ClientRollerSpecification
                    {
                        ClientRollerSpecificationID = wo.ClientRollerSpecification.ClientRollerSpecificationID,
                        FullClientID = wo.ClientRollerSpecification.FullClientID
                    }
                    : null,
                ClientRoller = wo.ClientRoller != null
                    ? new ClientRoller
                    {
                        ClientRollerID = wo.ClientRoller.ClientRollerID,
                        ClientRollerNumber = wo.ClientRoller.ClientRollerNumber
                    }
                    : null,
                Compound = wo.Compound != null
                    ? new Compound { CompoundCode = wo.Compound.CompoundCode, CompoundDescription = wo.Compound.CompoundDescription, CompoundType = wo.Compound.CompoundType }
                    : null
            }).ToHashSet() ?? new HashSet<WorksOrder>()
        }).ToHashSet();

    public record MyResponse(IEnumerable<object> Results);
    public record NaturalLanguageQuery(string Text);
}