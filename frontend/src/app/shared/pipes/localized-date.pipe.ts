import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'localizedDate',
  standalone: true
})
export class LocalizedDatePipe implements PipeTransform {
  transform(value: string | Date | null | undefined, locale = 'th-TH'): string {
    if (!value) return '-';
    const date = typeof value === 'string' ? new Date(value) : value;
    if (isNaN(date.getTime())) return '-';
    return new Intl.DateTimeFormat(locale, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(date);
  }
}
