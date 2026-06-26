export interface AnalyticsDashboardDTO {
  occupancyRate: number;
  averageDailyRate: number;
  revPAR: number;
  totalRevenue: number;
  grossTurnover: number;
  averageLengthOfStay: number;
  cancellationRate: number;
  guestSatisfactionScore: number;
  averageHousekeepingTurnaroundMinutes: number;
  nonRoomExpenditure: {
    totalFoodSpend: number;
    totalAmenitySpend: number;
    highestSpendCategory: string;
  };
}
