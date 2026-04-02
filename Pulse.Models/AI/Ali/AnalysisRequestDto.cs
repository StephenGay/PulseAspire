using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.AI.Ali
{
    public record AnalysisRequestDto(
    string KeyValue,
    string ApplicationUserId,
    int ContextualAreaID,
    string UserQuery,
    string AnalysisTitle
    );
}
    
