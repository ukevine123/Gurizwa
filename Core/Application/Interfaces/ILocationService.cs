using Domain.Entities;

namespace Application.Interfaces;

public interface ILocationService
{
    Task<List<Province>> GetAllLocationsAsync();
}