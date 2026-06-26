using HotelManagement.DAL.Entities;

namespace HotelManagement.Repository.Interfaces;

public interface IRoomTypeRepository : IGenericRepository<RoomType>
{
    Task<IEnumerable<RoomType>> GetRoomTypesAsync(bool includeRetired = false);
}
