using System.Security.Claims;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

public class PhotoUpdateDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public string? Date { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public string? LocationLabel { get; set; }
    public string? Technique { get; set; }
    public string? Quote { get; set; }
    public string? InventoryNumber { get; set; }
    public string? OriginalFormat { get; set; }
    public string? License { get; set; }
    public string? Digitization { get; set; }
    public string? Tags { get; set; }
}

public class PhotoUploadDto
{
    public IFormFile File { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string Date { get; set; } = string.Empty;
    public string? Lat { get; set; }
    public string? Lng { get; set; }
    public string? LocationLabel { get; set; }
    public string? Technique { get; set; }
    public string? Quote { get; set; }
    public string? InventoryNumber { get; set; }
    public string? OriginalFormat { get; set; }
    public string? License { get; set; }
    public string? Digitization { get; set; }
    public string? Tags { get; set; } // comma-separated tag names
}

[ApiController]
[Route("api/[controller]")]
public class PhotosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public PhotosController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetPhotoDetails(int id)
    {
        var photo = await _db.Photos
        .Include(p => p.Category)
        .Include(p => p.Author)
        .Include(p => p.Tags)
        .FirstOrDefaultAsync(p => p.Id == id);

        if(photo == null) return NotFound();
        
        return Ok(photo);
    }

    [HttpGet]
    public async Task<IActionResult> GetPhotos(
        [FromQuery] string? q,
        [FromQuery] int? categoryId,
        [FromQuery] string? dateFrom,
        [FromQuery] string? dateTo,
        [FromQuery] string? tag,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
      var query = _db.Photos
          .Include(p => p.Category)
          .Include(p => p.Author)
          .Include(p => p.Tags)
          .AsQueryable();

      if (!string.IsNullOrEmpty(q))
      {
          query = query.Where(p =>
              p.Title.Contains(q) ||
              (p.Description != null && p.Description.Contains(q)) ||
              (p.LocationLabel != null && p.LocationLabel.Contains(q)));
      }

      if (categoryId.HasValue)
      {
          query = query.Where(p => p.CategoryId == categoryId.Value);
      }

      if (!string.IsNullOrEmpty(tag))
      {
          query = query.Where(p => p.Tags.Any(t => t.Name == tag));
      }

      if (!string.IsNullOrEmpty(dateFrom))
      {
          query = query.Where(p => p.Date.CompareTo(dateFrom) >= 0);
      }

      if (!string.IsNullOrEmpty(dateTo))
      {
          // Pad year-only/month-only dates so all dates within the period are included
          // e.g. "1960" → "1960-12-31" includes "1960", "1960-05", "1960-05-15"
          var dateToEnd = dateTo.Length == 4 ? dateTo + "-12-31"
                        : dateTo.Length == 7 ? dateTo + "-31"
                        : dateTo;
          query = query.Where(p => p.Date.CompareTo(dateToEnd) <= 0);
      }

      query = sortBy switch
      {
          "date" => sortDir == "asc" ? query.OrderBy(p => p.Date) : query.OrderByDescending(p => p.Date),
          "createdAt" => sortDir == "asc" ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
          _ => query.OrderByDescending(p => p.CreatedAt) 
      };

      var totalCount = await query.CountAsync();

      var items = await query
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .ToListAsync();

      return Ok(new
      {
          items,
          page,
          pageSize,
          totalCount,
          totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
      });
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetUserPhotos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var query = _db.Photos
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Include(p => p.Tags)
            .Where(p => p.AuthorId == userId)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            items,
            page,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddPhoto([FromForm] PhotoUploadDto dto)
    {
        if (dto.File == null || dto.File.Length == 0)
            return BadRequest("Plik jest wymagany");

        // Save file to wwwroot/uploads
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(dto.File.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await dto.File.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/{fileName}";

        double? lat = null, lng = null;
        if (!string.IsNullOrEmpty(dto.Lat) && double.TryParse(dto.Lat, System.Globalization.CultureInfo.InvariantCulture, out var parsedLat))
            lat = parsedLat;
        if (!string.IsNullOrEmpty(dto.Lng) && double.TryParse(dto.Lng, System.Globalization.CultureInfo.InvariantCulture, out var parsedLng))
            lng = parsedLng;

        var photo = new Photo
        {
            Title = dto.Title,
            Description = dto.Description,
            ImagePath = relativePath,
            ThumbnailPath = relativePath,
            CategoryId = dto.CategoryId,
            Date = dto.Date,
            Lat = lat,
            Lng = lng,
            LocationLabel = dto.LocationLabel,
            Technique = dto.Technique,
            Quote = dto.Quote,
            InventoryNumber = dto.InventoryNumber,
            OriginalFormat = dto.OriginalFormat,
            License = dto.License,
            Digitization = dto.Digitization,
            AuthorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)
        };

        // Handle tags (comma-separated)
        if (!string.IsNullOrWhiteSpace(dto.Tags))
        {
            var tagNames = dto.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var name in tagNames)
            {
                var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Name == name);
                if (tag == null)
                {
                    tag = new Tag { Name = name };
                    _db.Tags.Add(tag);
                }
                photo.Tags.Add(tag);
            }
        }

        _db.Photos.Add(photo);
        await _db.SaveChangesAsync();

        // Reload with navigation properties
        await _db.Entry(photo).Reference(p => p.Category).LoadAsync();
        await _db.Entry(photo).Reference(p => p.Author).LoadAsync();
        await _db.Entry(photo).Collection(p => p.Tags).LoadAsync();

        return Created($"/api/photos/{photo.Id}", photo);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> EditPhoto(int id, [FromBody] PhotoUpdateDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("admin");

        var existing = await _db.Photos.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == id);
        if (existing == null) return NotFound();
        if (existing.AuthorId != userId && !isAdmin) return Forbid();

        if (dto.Title != null) existing.Title = dto.Title;
        existing.Description = dto.Description;
        if (dto.CategoryId.HasValue) existing.CategoryId = dto.CategoryId.Value;
        if (dto.Date != null) existing.Date = dto.Date;
        existing.Lat = dto.Lat;
        existing.Lng = dto.Lng;
        existing.LocationLabel = dto.LocationLabel;
        existing.Technique = dto.Technique;
        existing.Quote = dto.Quote;
        existing.InventoryNumber = dto.InventoryNumber;
        existing.OriginalFormat = dto.OriginalFormat;
        existing.License = dto.License;
        existing.Digitization = dto.Digitization;

        if (dto.Tags != null)
        {
            existing.Tags.Clear();
            var tagNames = dto.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var name in tagNames)
            {
                var t = await _db.Tags.FirstOrDefaultAsync(x => x.Name == name) ?? new Tag { Name = name };
                if (t.Id == 0) _db.Tags.Add(t);
                existing.Tags.Add(t);
            }
        }

        await _db.SaveChangesAsync();
        await _db.Entry(existing).Reference(p => p.Category).LoadAsync();
        await _db.Entry(existing).Reference(p => p.Author).LoadAsync();
        return Ok(existing);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("admin");

        var photo = await _db.Photos.FindAsync(id);
        if (photo == null) return NotFound();
        if (photo.AuthorId != userId && !isAdmin) return Forbid();

        // Delete physical file from disk
        if (!string.IsNullOrEmpty(photo.ImagePath))
        {
            var filePath = Path.Combine(_env.WebRootPath, photo.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        _db.Photos.Remove(photo);
        await _db.SaveChangesAsync();
        return Ok();
    }

}
