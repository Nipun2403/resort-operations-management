export interface AvailableRoomType {
  roomTypeId: number;
  name: string;
  basePrice: number;
  maxOccupancy: number;
  description: string | null;
  imageUrls: string[] | null;
  squareFootage: number | null;
  bedConfiguration: Record<string, number> | null;
  availableCount: number;
}
