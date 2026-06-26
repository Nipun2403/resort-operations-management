using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;

namespace HotelManagement.Repository.Implementations;

public class AmenityRepository : GenericRepository<Amenity>, IAmenityRepository
{
    public AmenityRepository(ApplicationDbContext context) : base(context)
    {
    }
}
