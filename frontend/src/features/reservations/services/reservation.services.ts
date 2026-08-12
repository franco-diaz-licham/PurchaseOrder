import type { ApiResponse } from '@/lib/api/api.types';
import http from '@/lib/api/http';
import type { CreateReservationRequestDto, ReleaseReservationRequestDto, ReservationResponseDto } from '../types/reservation.api.types';

export const listReservations = async () => {
  const response = await http.get<ApiResponse<ReservationResponseDto[]>>('/reservation');
  return response.data.data;
};

export const createReservation = async (request: CreateReservationRequestDto) => {
  const response = await http.post<ApiResponse<ReservationResponseDto>>('/reservation', request);
  return response.data.data;
};

export const releaseReservation = async (stockReservationId: string, request: ReleaseReservationRequestDto) => {
  const response = await http.post<ApiResponse<ReservationResponseDto>>(`/reservation/${stockReservationId}/release`, request);
  return response.data.data;
};
