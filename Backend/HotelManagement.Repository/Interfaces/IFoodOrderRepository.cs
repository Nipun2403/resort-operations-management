using HotelManagement.DAL.Entities;

namespace HotelManagement.Repository.Interfaces;

public interface IFoodOrderRepository : IGenericRepository<FoodOrder>
{
    Task<IEnumerable<FoodOrder>> GetActiveOrdersWithDetailsAsync();
    Task<IEnumerable<FoodOrder>> GetAllOrdersWithDetailsAsync();
}
