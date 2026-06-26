using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAnalyticsFunctionsForMultiRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION CalculateRevPAR(startDate timestamp with time zone, endDate timestamp with time zone)
                RETURNS numeric
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    total_revenue numeric;
                    total_rooms integer;
                BEGIN
                    SELECT COUNT(*) INTO total_rooms FROM ""Rooms"" WHERE ""IsActive"" = true;
                    
                    IF total_rooms = 0 THEN
                        RETURN 0;
                    END IF;

                    SELECT COALESCE(SUM(GREATEST(EXTRACT(DAY FROM (b.""CheckOutDate"" - b.""CheckInDate"")), 1) * rt.""BasePrice""), 0)
                    INTO total_revenue
                    FROM ""Bookings"" b
                    JOIN ""BookingRooms"" br ON b.""Id"" = br.""BookingId""
                    JOIN ""RoomTypes"" rt ON br.""RoomTypeId"" = rt.""Id""
                    WHERE b.""BookingStatus"" IN ('Booked', 'CheckedIn', 'CheckedOut')
                      AND b.""CheckInDate"" < endDate AND b.""CheckOutDate"" > startDate;
                      
                    RETURN total_revenue / total_rooms;
                END;
                $$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION CalculateOccupancyRate(startDate timestamp with time zone, endDate timestamp with time zone)
                RETURNS double precision
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    occupied_rooms integer;
                    total_rooms integer;
                BEGIN
                    SELECT COUNT(*) INTO total_rooms FROM ""Rooms"" WHERE ""IsActive"" = true;
                    
                    IF total_rooms = 0 THEN
                        RETURN 0;
                    END IF;

                    SELECT COUNT(DISTINCT br.""RoomId"")
                    INTO occupied_rooms
                    FROM ""Bookings"" b
                    JOIN ""BookingRooms"" br ON b.""Id"" = br.""BookingId""
                    WHERE b.""BookingStatus"" IN ('Booked', 'CheckedIn', 'CheckedOut')
                      AND b.""CheckInDate"" < endDate AND b.""CheckOutDate"" > startDate
                      AND br.""RoomId"" IS NOT NULL;
                      
                    RETURN (occupied_rooms::double precision / total_rooms::double precision) * 100.0;
                END;
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
