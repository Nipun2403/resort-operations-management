export interface MenuItem {
  id: number;
  name: string;
  price: number;
  category: string;
  isAvailable: boolean;
  image?: string;
  description?: string;
}

export interface CreateMenuItemDTO {
  name: string;
  price: number;
  category: string;
  isAvailable: boolean;
}

export interface UpdateMenuItemDTO {
  name: string;
  price: number;
  category: string;
  isAvailable: boolean;
}
