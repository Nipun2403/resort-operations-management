import { Signal, TemplateRef, WritableSignal } from '@angular/core';

export interface FilterOption {
  label: string;
  value: any;
}

export interface FilterDef {
  key: string;
  label: string;
  options: FilterOption[];
}

export interface ColumnDef {
  field: string;
  header: string;
  sortable: boolean;
  getValue: (row: any) => string;
  cellTemplate?: TemplateRef<any>;
}

export interface FormFieldDef {
  key: string;
  label: string;
  type:
    | 'text'
    | 'number'
    | 'email'
    | 'password'
    | 'textarea'
    | 'date'
    | 'url'
    | 'select'
    | 'toggle'
    | 'keyValueList'
    | 'imageUrlList';
  options?: FilterOption[];
  validators?: any[];
  showInAdd?: boolean;
  showInEdit?: boolean;
}

export interface CrudConfig<T> {
  entityName: string;
  entityNamePlural: string;
  columns: ColumnDef[];
  formFields: FormFieldDef[];
  filters: FilterDef[];
  supportsToggle: boolean;
  data: Signal<T[]>;
  loading: Signal<boolean>;
  error: WritableSignal<string | null>;
  totalCount: Signal<number>;
  pageIndex: Signal<number>;
  pageSize: Signal<number>;
}

export interface CrudModalData {
  editMode: boolean;
  entity: any | null;
  formFields: FormFieldDef[];
  supportsToggle: boolean;
}

export interface CrudModalResult {
  formValue: any;
  isActive: boolean;
  previousIsActive: boolean;
  entityId?: number;
}

