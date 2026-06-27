export interface Receipt {
  id: number;
  bookingId: number;
  amountPaid: number;
  paymentMethod: string;
  transactionId: string;
  paidAt: string; // ISO 8601
}
