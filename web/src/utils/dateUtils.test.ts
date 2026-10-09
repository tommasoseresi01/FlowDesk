import {describe, expect, test} from 'vitest';
import {
  API_DATE_FORMAT,
  formatDate,
  formatDateFromUtc,
  parseDate
} from '@app/utils/dateUtils';

describe('dateUtils', () => {
  test('parseDate reads the API format', () => {
    expect(parseDate('2026-10-08T09:05:30')).toEqual(
      new Date(2026, 9, 8, 9, 5, 30)
    );
  });

  test('parseDate returns null for a missing value', () => {
    expect(parseDate(null)).toBeNull();
    expect(parseDate(undefined)).toBeNull();
    expect(parseDate('')).toBeNull();
  });

  test('formatDate and parseDate are symmetric on the API format', () => {
    const date = new Date(2026, 0, 31, 23, 59, 1);
    expect(parseDate(formatDate(date, API_DATE_FORMAT))).toEqual(date);
  });

  test('formatDateFromUtc shifts a UTC value to the local time zone', () => {
    const utcValue = new Date(2026, 9, 8, 12, 0, 0);
    const expected = new Date(
      utcValue.getTime() - utcValue.getTimezoneOffset() * 60000
    );

    expect(formatDateFromUtc(utcValue, 'dd/MM/yyyy HH:mm')).toBe(
      formatDate(expected, 'dd/MM/yyyy HH:mm')
    );
  });
});
