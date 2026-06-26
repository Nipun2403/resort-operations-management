using HotelManagement.DAL.Entities;
namespace HotelManagement.Repository.Interfaces;
public interface IHousekeepingRepository : IGenericRepository<Housekeeping>
{
    Task<IEnumerable<Housekeeping>> GetActiveTasksForRoomAsync(int roomId);
}
