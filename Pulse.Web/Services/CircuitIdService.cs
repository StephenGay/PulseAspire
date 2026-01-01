// Services/CircuitIdService.cs (updated in Blazor UI project)
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;

namespace Pulse.Web.Services;  // Adjust namespace

public class CircuitIdService : CircuitHandler
{
    private readonly ICircuitState _circuitState;
    private readonly ILogger<CircuitIdService> _logger;

    public CircuitIdService(ICircuitState circuitState, ILogger<CircuitIdService> logger)
    {
        _circuitState = circuitState;
        _logger = logger;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _circuitState.CurrentCircuitId = circuit.Id;
        _logger.LogInformation("Circuit opened with ID: {CircuitId}", circuit.Id);
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        try
        {
            _circuitState.CurrentCircuitId = null;
            _logger.LogInformation("Circuit closed with ID: {CircuitId}", circuit.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during circuit closure for ID: {CircuitId}", circuit.Id);
        }
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}

//// BlazorUI/Services/CircuitIdService.cs
//using Microsoft.AspNetCore.Components.Server.Circuits;
//using Microsoft.Extensions.Logging;

//public class CircuitIdService : CircuitHandler
//{
//    public readonly ILogger<CircuitIdService> _logger;
//    public CircuitIdService(ILogger<CircuitIdService> logger)
//    {
//        _logger = logger;
//    }
//    public string? CurrentCircuitId { get; private set; }

//    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
//    {
//        CurrentCircuitId = circuit.Id;
//        _logger.LogInformation("Circuit opened with ID: {CircuitId}", circuit.Id);
//        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
//    }

//    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
//    {
//        try
//        {
//            CurrentCircuitId = null;
//            _logger.LogInformation("Circuit closed with ID: {CircuitId}", circuit.Id);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error during circuit closure for ID: {CircuitId}", circuit.Id);
//        }
//        return base.OnCircuitClosedAsync(circuit, cancellationToken);
//    }
//}