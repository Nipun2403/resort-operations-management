using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repository.Implementations;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<IEnumerable<User>> GetActiveStaffAsync(bool includeFired = false)
    {
        return await _context.Users
            .Where(u => (includeFired || u.IsActive) && u.Role != "RegisteredUser")
            .OrderBy(u => u.Role)
            .ThenBy(u => u.LastName)
            .ToListAsync();
    }
}
