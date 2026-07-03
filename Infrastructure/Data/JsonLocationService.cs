using Application.Interfaces;
using Domain.Entities;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting; // Needed to find wwwroot

namespace Infrastructure.Services;

public class JsonLocationService : ILocationService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public JsonLocationService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<List<Province>> GetAllLocationsAsync()
    {
        // 1. Locate the file in wwwroot/Data
        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Data", "RwandaHierarchicalLocations.json");

        if (!File.Exists(filePath))
        {
            return new List<Province>();
        }

        // 2. Read and Deserialize the JSON
        using var jsonStream = File.OpenRead(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        
        var data = await JsonSerializer.DeserializeAsync<List<Province>>(jsonStream, options);

        return data ?? new List<Province>();
    }
}