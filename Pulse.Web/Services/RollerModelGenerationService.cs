using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pulse.Models.Customers;
using Pulse.Models.Rollers;

namespace Pulse.Web.Services;

public class RollerModelGenerationService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private bool _initialized = false;

    public RollerModelGenerationService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<bool> InitializeViewerAsync(ElementReference container)
    {
        try
        {
            var success = await _js.InvokeAsync<bool>("RollerViewer3D.init", container);
            if (success) _initialized = true;
            return success;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to initialize 3D viewer: {ex.Message}");
            return false;
        }
    }

    public async Task LoadRollerAsync(ClientRollerSpecification roller, List<RollerShaft> shafts)
    {
        if (!_initialized)
            throw new InvalidOperationException("3D Viewer must be initialized first.");

        if (roller == null)
            throw new ArgumentNullException(nameof(roller));

        await ClearSceneAsync();

        // 1. Roller Shell
        if (roller.ShellDiameter.HasValue && roller.ShellLength.HasValue)
        {
            await _js.InvokeVoidAsync("RollerViewer3D.addRoller",
                (float)roller.ShellDiameter.Value,
                (float)roller.ShellLength.Value);
        }

        // 2. Rubber Cover
        if (roller.CoverDiameter.HasValue && roller.CoverLength.HasValue)
        {
            float thickness = (float)((roller.CoverDiameter.Value - roller.ShellDiameter!.Value) / 2.0m);
            float coverLength = (float)(roller.CoverLength.Value - 30);

            await _js.InvokeVoidAsync("RollerViewer3D.addRubberCover",
                (float)(roller.CoverLeftOffset ?? 0),
                coverLength,
                thickness,
                0x1a1a1a);
        }

        // 3. Shafts
        if (shafts != null && shafts.Any())
        {
            double shellHalf = (double)(roller.ShellLength!.Value / 2.0m);
            double currentLeftPos = -shellHalf;
            double currentRightPos = shellHalf;

            foreach (var shaft in shafts.OrderBy(s => s.ShaftPosition))
            {
                string side = (shaft.ShaftPosition?.ToLower() ?? "center");
                double axialPos;

                if (side == "left")
                {
                    axialPos = currentLeftPos - (shaft.Length / 2.0);
                    currentLeftPos -= shaft.Length;
                }
                else
                {
                    axialPos = currentRightPos + (shaft.Length / 2.0);
                    currentRightPos += shaft.Length;
                }

                await _js.InvokeVoidAsync("RollerViewer3D.addShaft",
                    (float)shaft.OuterDiameter,
                    (float)shaft.Length,
                    (float)axialPos,
                    side,
                    (float)(shaft.RadialOffset ?? 0),
                    shaft.Id.ToString());
            }
        }

        // 4. Final positioning
        await _js.InvokeVoidAsync("RollerViewer3D.applyFinalPositioning");

        // 5. Enable post-processing (Bloom + FXAA)
        await _js.InvokeVoidAsync("RollerViewer3D.initPostProcessing");

        await _js.InvokeVoidAsync("RollerViewer3D.debugPositions");
    }

    // ====================== ANIMATION ======================
    public async Task SetSpinSpeedAsync(float speed = 0.004f)
        => await _js.InvokeVoidAsync("RollerViewer3D.setSpinSpeed", speed);

    public async Task StartSpinAsync() => await _js.InvokeVoidAsync("RollerViewer3D.startSpin");
    public async Task StopSpinAsync() => await _js.InvokeVoidAsync("RollerViewer3D.stopSpin");
    public async Task<bool> ToggleSpinAsync() => await _js.InvokeAsync<bool>("RollerViewer3D.toggleSpin");

    // ====================== CAMERA ======================
    public async Task ResetCameraAsync() => await _js.InvokeVoidAsync("RollerViewer3D.resetCamera");
    public async Task SetCameraPresetAsync(string preset) => await _js.InvokeVoidAsync("RollerViewer3D.setCameraPreset", preset);
    public async Task ZoomToFitAsync() => await _js.InvokeVoidAsync("RollerViewer3D.zoomToFit");

    public async Task ClearSceneAsync() => await _js.InvokeVoidAsync("RollerViewer3D.clearScene");

    public async Task ReloadAsync(ElementReference container, ClientRollerSpecification roller, List<RollerShaft> shafts)
    {
        await ClearSceneAsync();
        await Task.Delay(50);
        await LoadRollerAsync(roller, shafts);
    }

    // ====================== DIMENSIONS & LABELS ======================
    public async Task AddDimensionLinesAsync() => await _js.InvokeVoidAsync("RollerViewer3D.addDimensionLines");
    public async Task RemoveDimensionLinesAsync() => await _js.InvokeVoidAsync("RollerViewer3D.removeDimensionLines");
    public async Task AddMeasurementLabelsAsync() => await _js.InvokeVoidAsync("RollerViewer3D.addMeasurementLabels");
    public async Task RemoveMeasurementLabelsAsync() => await _js.InvokeVoidAsync("RollerViewer3D.removeMeasurementLabels");

    // ====================== EXPLODED VIEW ======================
    public async Task<bool> ToggleExplodedViewAsync()
    {
        try { return await _js.InvokeAsync<bool>("RollerViewer3D.toggleExplodedView"); }
        catch (Exception ex) { Console.Error.WriteLine($"Error: {ex.Message}"); return false; }
    }

    public async Task SetExplodedFactorAsync(double factor) => await _js.InvokeVoidAsync("RollerViewer3D.explodeView", factor);

    // ====================== VISIBILITY & OPACITY ======================
    public async Task<List<RollerModelPartVisibility>> GetPartsListAsync()
        => await _js.InvokeAsync<List<RollerModelPartVisibility>>("RollerViewer3D.getPartsList");

    public async Task ToggleVisibilityAsync(string meshName, bool visible)
        => await _js.InvokeVoidAsync("RollerViewer3D.toggleVisibility", meshName, visible);

    public async Task SetAllVisibilityAsync(bool visible)
        => await _js.InvokeVoidAsync("RollerViewer3D.setAllVisibility", visible);

    public async Task SetOpacityAsync(string meshName, float opacity)
        => await _js.InvokeVoidAsync("RollerViewer3D.setOpacity", meshName, opacity);

    public async Task ResetAllOpacitiesAsync()
        => await _js.InvokeVoidAsync("RollerViewer3D.resetAllOpacities");

    // ====================== POST-PROCESSING ======================
    public async Task EnablePostProcessingAsync()
        => await _js.InvokeVoidAsync("RollerViewer3D.initPostProcessing");

    public async Task DisablePostProcessingAsync()
        => await _js.InvokeVoidAsync("RollerViewer3D.disablePostProcessing");

    // ====================== INTERACTIVITY ======================
    public async Task EnableAdvancedInteractivityAsync()
        => await _js.InvokeVoidAsync("RollerViewer3D.enableAdvancedInteractivity");

    public async Task HideInfoPanelAsync()
        => await _js.InvokeVoidAsync("RollerViewer3D.hideInfoPanel");

    public async Task CaptureScreenshotAsync()
        => await _js.InvokeVoidAsync("RollerViewer3D.captureScreenshot");

    public async ValueTask DisposeAsync()
    {
        try { await _js.InvokeVoidAsync("RollerViewer3D.dispose"); }
        catch { }
    }
}