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

    /// <summary>
    /// Initialize the 3D viewer
    /// </summary>
    public async Task<bool> InitializeViewerAsync(ElementReference container)
    {
        try
        {
            var success = await _js.InvokeAsync<bool>("RollerViewer3D.init", container);
            if (success)
                _initialized = true;

            return success;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to initialize 3D viewer: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Main method - Load complete roller with all components
    /// </summary>
    public async Task LoadRollerAsync(
        ClientRollerSpecification roller,
        List<RollerShaft> shafts)
    {
        if (!_initialized)
            throw new InvalidOperationException("3D Viewer must be initialized first.");

        if (roller == null)
            throw new ArgumentNullException(nameof(roller));

        await ClearSceneAsync();

        // 1. Add Roller Shell
        if (roller.ShellDiameter.HasValue && roller.ShellLength.HasValue)
        {
            await _js.InvokeVoidAsync("RollerViewer3D.addRoller",
                (float)roller.ShellDiameter.Value,
                (float)roller.ShellLength.Value);
        }

        // 2. Add Rubber Cover
        if (roller.CoverDiameter.HasValue && roller.CoverLength.HasValue)
        {
            // Line 64 - Change 2.0 to 2.0m
            float thickness = (float)((roller.CoverDiameter.Value - roller.ShellDiameter!.Value) / 2.0m);
            float coverLength = (float)(roller.CoverLength.Value - 30); // slight inset

            await _js.InvokeVoidAsync("RollerViewer3D.addRubberCover",
                (float)(roller.CoverLeftOffset ?? 0),
                coverLength,
                thickness,
                0x1a1a1a); // dark rubber
        }

        // 3. Add Shafts
        if (shafts != null && shafts.Any())
        {
            double halfLength = (double)(roller.ShellLength!.Value / 2.0m);

            foreach (var shaft in shafts.Where(s => s.OuterDiameter > 0 && s.Length > 0))
            {
                double axialPos = shaft.AxialPosition;

                // Alternative if you store position from left end:
                // double axialPos = (shaft.PositionFromLeft ?? halfLength) - halfLength;

                await _js.InvokeVoidAsync("RollerViewer3D.addShaft",
                    (float)shaft.OuterDiameter,
                    (float)shaft.Length,
                    (float)axialPos,
                    shaft.ShaftPosition?.ToLower() ?? "center",
                    (float)(shaft.RadialOffset ?? 0),
                    shaft.Id.ToString());
            }
        }

        // 4. Final positioning + camera
        await _js.InvokeVoidAsync("RollerViewer3D.applyFinalPositioning");
        await _js.InvokeVoidAsync("RollerViewer3D.debugPositions");
    }

    // ====================== ANIMATION CONTROLS ======================
    public async Task SetSpinSpeedAsync(float speed = 0.004f)
    {
        await _js.InvokeVoidAsync("RollerViewer3D.setSpinSpeed", speed);
    }

    public async Task StartSpinAsync() => await _js.InvokeVoidAsync("RollerViewer3D.startSpin");
    public async Task StopSpinAsync() => await _js.InvokeVoidAsync("RollerViewer3D.stopSpin");

    public async Task<bool> ToggleSpinAsync()
    {
        return await _js.InvokeAsync<bool>("RollerViewer3D.toggleSpin");
    }

    // ====================== CAMERA CONTROLS ======================
    public async Task ResetCameraAsync() => await _js.InvokeVoidAsync("RollerViewer3D.resetCamera");

    public async Task SetCameraPresetAsync(string preset)
    {
        await _js.InvokeVoidAsync("RollerViewer3D.setCameraPreset", preset);
    }

    public async Task ZoomToFitAsync() => await _js.InvokeVoidAsync("RollerViewer3D.zoomToFit");
    public async Task ClearSceneAsync()
    {
        await _js.InvokeVoidAsync("RollerViewer3D.clearScene");
    }

    public async Task ReloadAsync(ElementReference container, ClientRollerSpecification roller, List<RollerShaft> shafts)
    {
        await ClearSceneAsync();
        await Task.Delay(50); // Small delay for cleanup
        await LoadRollerAsync(roller, shafts);
    }
    public async Task AddDimensionLinesAsync()
    {
        await _js.InvokeVoidAsync("RollerViewer3D.addDimensionLines");
    }

    public async Task RemoveDimensionLinesAsync()
    {
        await _js.InvokeVoidAsync("RollerViewer3D.removeDimensionLines");
    }

    public async Task EnableInteractivityAsync()
    {
        await _js.InvokeVoidAsync("RollerViewer3D.enableInteractivity");
    }
    // ====================== ADVANCED FEATURES ======================
    public async Task<bool> ToggleExplodedViewAsync()
    {
        try
        {
            return await _js.InvokeAsync<bool>("RollerViewer3D.toggleExplodedView");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error toggling exploded view: {ex.Message}");
            return false;
        }
    }

    public async Task SetExplodedFactorAsync(double factor)
    {
        await _js.InvokeVoidAsync("RollerViewer3D.explodeView", factor);
    }

    public async Task CaptureScreenshotAsync()
    {
        await _js.InvokeVoidAsync("RollerViewer3D.captureScreenshot");
    }

    public async Task AddMeasurementLabelsAsync()
    {
        await _js.InvokeVoidAsync("RollerViewer3D.addMeasurementLabels");
    }

    public async Task RemoveMeasurementLabelsAsync()
    {
        await _js.InvokeVoidAsync("RollerViewer3D.removeMeasurementLabels");
    }
    public async Task EnableAdvancedInteractivityAsync()
    {
        await _js.InvokeVoidAsync("RollerViewer3D.enableAdvancedInteractivity");
    }

    public async Task HideInfoPanelAsync()
    {
        await _js.InvokeVoidAsync("RollerViewer3D.hideInfoPanel");
    }

    // ====================== PART VISIBILITY ======================
    public async Task<List<RollerModelPartVisibility>> GetPartsListAsync()
    {
        return await _js.InvokeAsync<List<RollerModelPartVisibility>>("RollerViewer3D.getPartsList");
    }

    public async Task ToggleVisibilityAsync(string meshName, bool visible)
    {
        await _js.InvokeVoidAsync("RollerViewer3D.toggleVisibility", meshName, visible);
    }

    public async Task SetAllVisibilityAsync(bool visible)
    {
        await _js.InvokeVoidAsync("RollerViewer3D.setAllVisibility", visible);
    }

    public async Task SetOpacityAsync(string meshName, float opacity)
    {
        await _js.InvokeVoidAsync("RollerViewer3D.setOpacity", meshName, opacity);
    }

    public async Task ResetAllOpacitiesAsync()
    {
        await _js.InvokeVoidAsync("RollerViewer3D.resetAllOpacities");
    }
    public async ValueTask DisposeAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("RollerViewer3D.dispose");
        }
        catch { /* Ignore if already disposed */ }
    }
}