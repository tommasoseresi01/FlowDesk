import {format, parse} from 'date-fns';

// Formato delle date scambiate con l'API (senza offset, espresse in UTC).
export const API_DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:ss";

export function parseDate(
  date: string | null | undefined,
  formatToUse: string = API_DATE_FORMAT
): Date | null {
  if (!date) {
    return null;
  }
  return parse(date, formatToUse, new Date());
}

export function formatDate(date: Date, formatToUse: string): string {
  return format(date, formatToUse);
}

// Per date arrivate dall'API in UTC: le sposta sul fuso del browser prima di formattarle.
export function formatDateFromUtc(date: Date, formatToUse: string): string {
  const localDate = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
  return format(localDate, formatToUse);
}
