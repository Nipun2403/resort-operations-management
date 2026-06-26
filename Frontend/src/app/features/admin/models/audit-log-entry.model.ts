export interface AuditLogEntry {
  id: number;
  entityName: string;
  action: string;
  recordId: { Id: number };
  oldValues: Record<string, any>;
  newValues: Record<string, any>;
  changedByEmail: string;
  changedByName: string;
  timestamp: string; // ISO 8601
}
