export interface BillingFolio {
  bookingId: number;
  guestName: string;
  nightsStayed: number;
  roomBasePrice: number;
  roomTotal: number;
  foodTotal: number;
  amenityTotal: number;
  totalBill: number;
  paymentStatus: string;
  servicePaymentStatus: string;
  foodItems: string[];
  amenityItems: string[];
}
