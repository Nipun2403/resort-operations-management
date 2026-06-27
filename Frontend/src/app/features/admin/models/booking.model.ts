export interface BookingRoom {
  id: number;
  bookingId: number;
  roomTypeId: number;
  roomId: number | null;
  roomNumber: string | null;
  lockedInPrice: number;
}

export interface Booking {
  id: number;
  guestCount: number;
  rooms: BookingRoom[];
  guestName: string;
  guestEmail: string;
  checkInDate: string; // "dd-MM-yyyy"
  checkOutDate: string; // "dd-MM-yyyy"
  bookingStatus: 'Booked' | 'CheckedIn' | 'CheckedOut' | 'Cancelled';
  userId: number | null;
  origin: 'WalkIn' | 'RegisteredUser' | 'Guest';
  bookedAt: string; // ISO 8601
  amenityIds: number[];
}
