using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.Dtos.Customers;

 record CustomerMasterDto
(
    string FullClientID,
        string ClientName,
        string? Region,
        string? Province,
        string? Country,
        string? SalesRepName,
        string? IndustryName,
        DateTime? CreatedDate,
        bool Blocked,
        string? PhysicalAddress1,
        string? PhysicalAddress2,
        string? PhysicalAddress3,
        string? PostalAddress1,
        string? PostalAddress2,
        string? PostalAddress3,
        string? Phone,
        string? EMail,
        string? CompanyName,
        decimal? CreditLimit ,
        decimal? Ageing01 ,
        decimal? Ageing02 ,
        decimal? Ageing03 ,
        decimal? Ageing04 ,
        decimal? Ageing05 ,
        DateTime? LastVisitDate ,
        string? AiSummary ,
        DateTime? AiUpdated 
);
