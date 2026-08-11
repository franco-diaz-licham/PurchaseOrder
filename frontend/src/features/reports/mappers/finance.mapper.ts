import type { WarehouseCommittedValueDto } from '../types/finance.api.types';
import type { WarehouseCommittedValue } from '../types/finance.types';

export const toWarehouseCommittedValue = (dto: WarehouseCommittedValueDto): WarehouseCommittedValue => ({
  warehouseId: dto.warehouseId,
  warehouseCode: dto.warehouseCode,
  warehouseName: dto.warehouseName,
  warehouseDisplayName: `${dto.warehouseCode} - ${dto.warehouseName}`,
  committedValue: dto.committedValue
});

export const toWarehouseCommittedValues = (dtos: WarehouseCommittedValueDto[]): WarehouseCommittedValue[] => dtos.map(toWarehouseCommittedValue);
