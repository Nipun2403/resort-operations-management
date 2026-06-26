using HotelManagement.DAL.Entities;

namespace HotelManagement.Repository.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetActiveStaffAsync(bool includeFired = false);
}
