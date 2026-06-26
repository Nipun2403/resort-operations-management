using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;

namespace HotelManagement.Repository.Implementations;

public class ReceiptRepository : GenericRepository<Receipt>, IReceiptRepository
{
    public ReceiptRepository(ApplicationDbContext context) : base(context)
    {
    }
}
