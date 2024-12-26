using PeaktrailsApp.Data.Entities;
using PeaktrailsBackend.Data.Entities;

public class Trail
{
    public int TrailId { get; set; }
    public string Name { get; set; }
    public decimal Length { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Difficulty { get; set; }
    public string Description { get; set; }
    public decimal? Elevation { get; set; }
    public decimal? TotalAscent { get; set; }
    public decimal? TotalDescent { get; set; }
    public string Location { get; set; }
    public string GPXLocation { get; set; }
    public string GPXContent { get; set; }

    // Navigatie-eigenschap naar de foto's (one-to-many relatie)

    public List<Photo> Photos { get; set; } = new List<Photo>();

    // Relatie met FavoriteTrail
    public List<FavoriteTrail> FavoriteTrails { get; set; } = new List<FavoriteTrail>();
}
