using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelMetricFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION CalculateAverageHousekeepingTurnaroundTime()
                RETURNS double precision
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    avg_minutes double precision;
                BEGIN
                    SELECT COALESCE(AVG(EXTRACT(EPOCH FROM (""FinishedAt"" - ""StartedAt"")) / 60.0), 0)
                    INTO avg_minutes
                    FROM ""HousekeepingTasks""
                    WHERE ""Status"" = 'Completed' AND ""StartedAt"" IS NOT NULL AND ""FinishedAt"" IS NOT NULL;
                    
                    RETURN avg_minutes;
                END;
                $$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION CalculateGuestHappinessIndex()
                RETURNS double precision
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    avg_rating double precision;
                BEGIN
                    SELECT COALESCE(AVG(""Rating""), 0)
                    INTO avg_rating
                    FROM ""Feedbacks"";
                    
                    IF avg_rating = 0 THEN
                        RETURN 0;
                    END IF;
                    
                    RETURN (avg_rating / 5.0) * 100.0;
                END;
                $$;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION CalculateRevPAR(startDate timestamp with time zone, endDate timestamp with time zone)
                RETURNS numeric
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    total_revenue numeric;
                    total_rooms integer;
                BEGIN
                    SELECT COUNT(*) INTO total_rooms FROM ""Rooms"";
                    
                    IF total_rooms = 0 THEN
                        RETURN 0;
                    END IF;

                    SELECT COALESCE(SUM(GREATEST(EXTRACT(DAY FROM (""CheckOutDate"" - ""CheckInDate"")), 1) * rt.""BasePrice""), 0)
                    INTO total_revenue
                    FROM ""Bookings"" b
                    JOIN ""Rooms"" r ON b.""RoomId"" = r.""Id""
                    JOIN ""RoomTypes"" rt ON r.""RoomTypeId"" = rt.""Id""
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
                    SELECT COUNT(*) INTO total_rooms FROM ""Rooms"";
                    
                    IF total_rooms = 0 THEN
                        RETURN 0;
                    END IF;

                    SELECT COUNT(DISTINCT ""RoomId"")
                    INTO occupied_rooms
                    FROM ""Bookings""
                    WHERE ""BookingStatus"" IN ('Booked', 'CheckedIn', 'CheckedOut')
                      AND ""CheckInDate"" < endDate AND ""CheckOutDate"" > startDate;
                      
                    RETURN (occupied_rooms::double precision / total_rooms::double precision) * 100.0;
                END;
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS CalculateAverageHousekeepingTurnaroundTime;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS CalculateGuestHappinessIndex;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS CalculateRevPAR;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS CalculateOccupancyRate;");
        }
    }
}
