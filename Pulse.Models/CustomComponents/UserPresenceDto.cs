using Microsoft.FluentUI.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.CustomComponents
{
    public record UserPresenceDto(
                    string UserId,
                    string UserName,
                    string FullName,
                    bool IsOnline,
                    PresenceStatus EffectiveStatus);
}
