import { Pipe, PipeTransform, inject } from '@angular/core';
import {TranslationService} from '../../core/services';

@Pipe({
  name: 'translate',
  pure: false,
  standalone: true
})
export class TranslatePipe implements PipeTransform {
  private translationService = inject(TranslationService);

  transform(key: string): string {
    if (!key) {
      return '';
    }

    return this.translationService.translate(key);
  }
}
