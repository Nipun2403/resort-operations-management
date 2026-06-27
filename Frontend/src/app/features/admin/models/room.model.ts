export interface Room {
  id: number;
  roomNumber: string;
  roomTypeName: string;
  roomTypeId: number;
  basePrice: number;
  maxOccupancy: number;
  isAvailable: boolean;
  isActive: boolean;
}

export interface CreateRoomDTO {
  roomNumber: string;
  roomTypeId: number;
  isActive: boolean;
}

export interface UpdateRoomDTO {
  roomNumber?: string;
  roomTypeId?: number;
  isActive?: boolean;
}

export interface RoomStatus {
  roomId: number;
  roomNumber: string;
  roomTypeName: string;
  status: 'Occupied' | 'Available';
  currentBookingId: number | null;
  currentGuestName: string | null;
  nextCheckInDate: string | null;
}
