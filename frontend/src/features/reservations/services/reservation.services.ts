import type { ApiResponse } from '@/lib/api/api.types';
import http from '@/lib/api/http';
import type { CreateReservationRequestDto, ReleaseReservationRequestDto, ReservationDto } from '../types/reservation.api.types';

export const listReservations = async (warehouseId?: string, status?: string) => {
  const response = await http.get<ApiResponse<ReservationDto[]>>('/Reservation', {
    params: {
      warehouseId: warehouseId || undefined,
      status: status || undefined
    }
  });
  return response.data.data;
};

export const createReservation = async (request: CreateReservationRequestDto) => {
  const response = await http.post<ApiResponse<ReservationDto>>('/Reservation', request);
  return response.data.data;
};

export const releaseReservation = async (stockReservationId: string, request: ReleaseReservationRequestDto) => {
  const response = await http.post<ApiResponse<ReservationDto>>(`/Reservation/${stockReservationId}/release`, request);
  return response.data.data;
};
