export interface MenuItem {
  id: number;
  name: string;
  price: number;
  category: string;
  isAvailable: boolean;
  imageUrl?: string;
  description?: string;
}

export interface CreateMenuItemDTO {
  name: string;
  price: number;
  category: string;
  isAvailable: boolean;
  description?: string;
  imageUrl: string;
}

export interface UpdateMenuItemDTO {
  name: string;
  price: number;
  category: string;
  isAvailable: boolean;
  description?: string;
  imageUrl: string;
}
