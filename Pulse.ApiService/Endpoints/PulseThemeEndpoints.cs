using Microsoft.EntityFrameworkCore;
using Pulse.Models.PulseContext;
using Pulse.Models.UI;

public static class PulseThemeEndpoints
{
    public static void MapPulseThemeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/ui/pulsethemes")
                       .WithTags("PulseThemes");
        //.RequireAuthorization();  adjust to your permissions system

        group.MapGet("/", async (PulseDbContext db) =>
            Results.Ok(await db.PulseThemes.OrderBy(t => t.Name).ToListAsync()));

        group.MapGet("/active", async (PulseDbContext db) =>
        {
            var active = await db.PulseThemes.FirstOrDefaultAsync(t => t.IsActive);
            return active is null ? Results.NotFound() : Results.Ok(active);
        });

        group.MapGet("/{id:int}", async (int id, PulseDbContext db) =>
            await db.PulseThemes.FindAsync(id) is { } t ? Results.Ok(t) : Results.NotFound());

        group.MapPost("/", async (PulseTheme theme, PulseDbContext db) =>
        {
            theme.CreatedAt = DateTime.UtcNow;
            theme.UpdatedAt = DateTime.UtcNow;
            db.PulseThemes.Add(theme);
            await db.SaveChangesAsync();
            return Results.Created($"/ui/pulsethemes/{theme.Id}", theme);
        });

        group.MapPut("/{id:int}", async (int id, PulseTheme input, PulseDbContext db) =>
        {
            var existing = await db.PulseThemes.FindAsync(id);
            if (existing is null) return Results.NotFound();

            // map fields...
            existing.Name = input.Name;
            existing.Description = input.Description;
            existing.Mode = input.Mode;
            existing.PrimaryColor = input.PrimaryColor;
            // ... copy all other properties + Settings
            existing.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Results.Ok(existing);
        });

        group.MapPost("/{id:int}/activate", async (int id, PulseDbContext db) =>
        {
            await db.PulseThemes.ExecuteUpdateAsync(s => s.SetProperty(t => t.IsActive, false));
            var theme = await db.PulseThemes.FindAsync(id);
            if (theme is null) return Results.NotFound();

            theme.IsActive = true;
            await db.SaveChangesAsync();
            return Results.Ok(theme);
        });

        group.MapDelete("/{id:int}", async (int id, PulseDbContext db) =>
        {
            var theme = await db.PulseThemes.FindAsync(id);
            if (theme is null) return Results.NotFound();
            if (theme.IsSystem) return Results.BadRequest("Cannot delete system theme");

            db.PulseThemes.Remove(theme);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}