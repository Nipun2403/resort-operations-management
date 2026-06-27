export interface Amenity {
  id: number;
  name: string;
  description: string;
  price: number;
  isAvailable: boolean;
}

export interface CreateAmenityDTO {
  name: string;
  description: string;
  price: number;
}

export interface UpdateAmenityDTO {
  name: string;
  description: string;
  price: number;
  isAvailable: boolean;
}
