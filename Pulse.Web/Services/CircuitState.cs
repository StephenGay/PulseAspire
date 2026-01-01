namespace Pulse.Web.Services;

public interface ICircuitState
{
    string? CurrentCircuitId { get; set; }
}

public class CircuitState : ICircuitState
{
    public string? CurrentCircuitId { get; set; }
}
