using AutoMapper;
using HotelManagement.DAL.Entities;
using HotelManagement.BLL.DTOs;
using System.Text.Json;

namespace HotelManagement.BLL.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Entity to DTO
        CreateMap<Room, RoomDTO>()
            .ForMember(dest => dest.BasePrice, opt => opt.MapFrom(src => src.RoomType != null ? src.RoomType.BasePrice : 0))
            .ForMember(dest => dest.MaxOccupancy, opt => opt.MapFrom(src => src.RoomType != null ? src.RoomType.MaxOccupancy : 0))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsActive)); ;
        // RoomTypeName is automatically mapped from RoomType.Name by AutoMapper's flattening convention.

        CreateMap<Booking, BookingDTO>()
            .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.BookingRooms))
            .ForMember(dest => dest.AmenityIds, opt => opt.MapFrom(src => src.BookingAmenities != null ? src.BookingAmenities.Select(ba => ba.AmenityId).ToList() : new List<int>()))
            .ForMember(dest => dest.BookedAt, opt => opt.MapFrom(src => src.BookedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")))
            .ReverseMap()
            .ForMember(dest => dest.BookedAt, opt => opt.Ignore());

        CreateMap<BookingRoom, BookingRoomDTO>()
            .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room != null ? src.Room.RoomNumber : null))
            .ReverseMap();

        CreateMap<Housekeeping, HousekeepingDTO>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")))
            .ForMember(dest => dest.StartedAt, opt => opt.MapFrom(src => src.StartedAt.HasValue ? src.StartedAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null))
            .ForMember(dest => dest.FinishedAt, opt => opt.MapFrom(src => src.FinishedAt.HasValue ? src.FinishedAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null))
            .ReverseMap()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.FinishedAt, opt => opt.Ignore());

        CreateMap<Feedback, FeedbackDTO>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")))
            .ReverseMap()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        CreateMap<CreateFeedbackDTO, Feedback>();
        CreateMap<AuditLog, AuditLogDTO>().ReverseMap();

        CreateMap<MaintenanceTask, MaintenanceTaskDTO>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")))
            .ForMember(dest => dest.StartedAt, opt => opt.MapFrom(src => src.StartedAt.HasValue ? src.StartedAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null))
            .ForMember(dest => dest.FinishedAt, opt => opt.MapFrom(src => src.FinishedAt.HasValue ? src.FinishedAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null))
            .ReverseMap()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.FinishedAt, opt => opt.Ignore());

        CreateMap<RoomType, RoomTypeDTO>()
            .ForMember(dest => dest.MaxOccupancy, opt => opt.MapFrom(src => src.MaxOccupancy))
            .ForMember(dest => dest.BedConfiguration, opt => opt.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.BedConfigurationJson)
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, int>>(src.BedConfigurationJson, (JsonSerializerOptions?)null)))
            .ReverseMap()
            .ForMember(dest => dest.BedConfigurationJson, opt => opt.MapFrom(src =>
                src.BedConfiguration == null
                ? null
                : JsonSerializer.Serialize(src.BedConfiguration, (JsonSerializerOptions?)null)));

        CreateMap<CreateRoomTypeDTO, RoomType>()
            .ForMember(dest => dest.BedConfigurationJson, opt => opt.MapFrom(src =>
                src.BedConfiguration == null
                ? null
                : JsonSerializer.Serialize(src.BedConfiguration, (JsonSerializerOptions?)null)));

        CreateMap<UpdateRoomTypeDTO, RoomType>()
            .ForMember(dest => dest.BedConfigurationJson, opt => opt.MapFrom(src =>
                src.BedConfiguration == null
                ? null
                : JsonSerializer.Serialize(src.BedConfiguration, (JsonSerializerOptions?)null)));

        CreateMap<MenuItem, MenuItemDTO>().ReverseMap();
        CreateMap<Amenity, AmenityDTO>().ReverseMap();

        CreateMap<FoodOrder, FoodOrderDTO>()
            .ForMember(dest => dest.GeneratedAt, opt => opt.MapFrom(src => src.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")))
            .ForMember(dest => dest.FinishedAt, opt => opt.MapFrom(src => src.FinishedAt.HasValue ? src.FinishedAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null))
            .ReverseMap()
            .ForMember(dest => dest.GeneratedAt, opt => opt.Ignore())
            .ForMember(dest => dest.FinishedAt, opt => opt.Ignore());
        CreateMap<FoodOrderItem, FoodOrderItemDTO>()
            .ForMember(dest => dest.MenuItemName, opt => opt.MapFrom(src => src.MenuItem != null ? src.MenuItem.Name : string.Empty))
            .ReverseMap();

        CreateMap<Receipt, ReceiptDTO>()
            .ForMember(dest => dest.PaidAt, opt => opt.MapFrom(src => src.PaidAt.ToString("yyyy-MM-ddTHH:mm:ssZ")));

        CreateMap<User, StaffResponseDTO>();
    }
}
