export interface PaginatedResponse<T> {
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  data: T[];
}