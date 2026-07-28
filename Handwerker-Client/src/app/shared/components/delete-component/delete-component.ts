import {Component, ElementRef, EventEmitter, Output, ViewChild, ChangeDetectionStrategy} from '@angular/core';
import {TranslatePipe} from '../../pipes/translate.pipe';

@Component({
  selector: 'app-delete-component',
  imports: [
    TranslatePipe
  ],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './delete-component.html'
})
export class DeleteComponent {
  @ViewChild('dialog') dialog!: ElementRef<HTMLDialogElement>;
  @Output() deleteConfirmed = new EventEmitter<string | null>();
  private currentId: string | null = null;

  open(id: string) {
    this.currentId = id;
    this.dialog.nativeElement.showModal();
  }
  close() {
    this.dialog.nativeElement.close();
  }

  delete() {
    this.dialog.nativeElement.close();
    this.deleteConfirmed.emit(this.currentId);
  }

}
