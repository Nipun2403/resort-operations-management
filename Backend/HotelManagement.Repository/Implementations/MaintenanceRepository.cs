using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;

namespace HotelManagement.Repository.Implementations;

public class MaintenanceRepository : GenericRepository<MaintenanceTask>, IMaintenanceRepository
{
    public MaintenanceRepository(ApplicationDbContext context) : base(context)
    {
    }
}
