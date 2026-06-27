export interface RoomType {
  id: number;
  name: string;
  description: string | null;
  basePrice: number;
  maxOccupancy: number;
  imageUrls: string[];
  squareFootage: number | null;
  bedConfiguration: Record<string, number> | null;
  isActive: boolean;
}

export interface CreateRoomTypeDTO {
  name: string;
  description?: string;
  basePrice: number;
  maxOccupancy: number;
  imageUrls?: string[];
  squareFootage?: number;
  bedConfiguration?: Record<string, number>;
}

export interface UpdateRoomTypeDTO {
  name?: string;
  description?: string;
  basePrice?: number;
  maxOccupancy?: number;
  imageUrls?: string[];
  squareFootage?: number;
  bedConfiguration?: Record<string, number>;
  isActive?: boolean;
}
