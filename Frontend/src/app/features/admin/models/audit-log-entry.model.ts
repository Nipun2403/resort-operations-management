export interface AuditLogEntry {
  id: number;
  entityName: string;
  action: string;
  recordId: { Id: number };
  oldValues: Record<string, any> | null;
  newValues: Record<string, any> | null;
  changedByEmail: string;
  changedByName: string;
  timestamp: string; // ISO 8601
}
