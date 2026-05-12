using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.Rollers
{
    public enum SurfaceCoating
    {
        None,
        ZincRichEpoxy,
        Polyurethane,
        ThermalSprayAl2O3,
        ThermalSprayTiO2,
        Ceramic,
        Rubber,
        Other
    }

    /// Enumerates common heat‑treatment states for a roller shaft. ///
    public enum HeatTreatment
    {
        AsCast, //
        NoTreatment,
        Normalised,
        Quenched650to550degC,
        RapidCooling,
        Tempered, // 
        CaseHardened200to400degC,
        SurfaceHardening,
        Nitrided,
        NitrogenDiffusion,
        Other
    }
}
