import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { purchaseOrderKeys } from '@/features/purchase-orders/queries/purchaseOrder.queries';
import { toCreateReservationRequestDto, toReleaseReservationRequestDto, toReservation, toReservations } from '../mappers/reservation.mapper';
import { createReservation, listReservations, releaseReservation } from '../services/reservation.services';
import type { CreateReservationCommand, ReleaseReservationCommand } from '../types/reservation.types';

export const reservationKeys = {
  all: ['reservations'] as const,
  list: (warehouseId?: string, status?: string) => ['reservations', warehouseId ?? 'all', status ?? 'all'] as const
};

export const useReservationsQuery = (warehouseId?: string, status?: string, enabled = true) =>
  useQuery({
    queryKey: reservationKeys.list(warehouseId, status),
    queryFn: async () => toReservations(await listReservations(warehouseId, status)),
    enabled
  });

export const useCreateReservationMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (command: CreateReservationCommand) => toReservation(await createReservation(toCreateReservationRequestDto(command))),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: reservationKeys.all });
      await queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.all });
      await queryClient.invalidateQueries({ queryKey: ['finance'] });
      await queryClient.invalidateQueries({ queryKey: ['audit-log'] });
    }
  });
};

export const useReleaseReservationMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (command: ReleaseReservationCommand) => toReservation(await releaseReservation(command.stockReservationId, toReleaseReservationRequestDto(command))),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: reservationKeys.all });
      await queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.all });
      await queryClient.invalidateQueries({ queryKey: ['finance'] });
      await queryClient.invalidateQueries({ queryKey: ['audit-log'] });
    }
  });
};
