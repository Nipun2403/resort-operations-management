export interface Amenity {
  id: number;
  name: string;
  description: string;
  price: number;
  isAvailable: boolean;
  imageUrl?: string;
}

export interface CreateAmenityDTO {
  name: string;
  description: string;
  price: number;
  imageUrl: string;
}

export interface UpdateAmenityDTO {
  name: string;
  description: string;
  price: number;
  isAvailable: boolean;
  imageUrl: string;
}
