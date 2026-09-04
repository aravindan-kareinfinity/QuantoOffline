import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfirmDialogComponent } from './confirm-dialog.component';
import { ConfirmDialogService } from '../../services/confirm-dialog.service';

describe('ConfirmDialogComponent', () => {
  let component: ConfirmDialogComponent;
  let fixture: ComponentFixture<ConfirmDialogComponent>;
  let dialog: ConfirmDialogService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfirmDialogComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmDialogComponent);
    component = fixture.componentInstance;
    dialog = TestBed.inject(ConfirmDialogService);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('renders nothing until a confirmation is requested', () => {
    expect(fixture.nativeElement.querySelector('[role="alertdialog"]')).toBeNull();
  });

  it('resolves true when confirmed', async () => {
    const result = dialog.confirm({ message: 'Delete this row?' });
    fixture.detectChanges();

    const confirmBtn = fixture.nativeElement.querySelectorAll('button')[1] as HTMLButtonElement;
    confirmBtn.click();

    expect(await result).toBeTrue();
  });

  it('resolves false when cancelled', async () => {
    const result = dialog.confirm({ message: 'Delete this row?' });
    fixture.detectChanges();

    const cancelBtn = fixture.nativeElement.querySelectorAll('button')[0] as HTMLButtonElement;
    cancelBtn.click();

    expect(await result).toBeFalse();
  });
});
