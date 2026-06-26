namespace HotelManagement.BLL.DTOs;

public class AnalyticsDashboardDTO
{
    public decimal OccupancyRate { get; set; }
    public decimal AverageDailyRate { get; set; }
    public decimal RevPAR { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal GrossTurnover { get; set; }
    public double AverageLengthOfStay { get; set; }
    public decimal CancellationRate { get; set; }
    public double GuestSatisfactionScore { get; set; }
    public double AverageHousekeepingTurnaroundMinutes { get; set; }

    public ExpenditureBreakdownDTO NonRoomExpenditure { get; set; } = new();
}

public class ExpenditureBreakdownDTO
{
    public decimal TotalFoodSpend { get; set; }
    public decimal TotalAmenitySpend { get; set; }
    public string HighestSpendCategory { get; set; } = string.Empty;
}
