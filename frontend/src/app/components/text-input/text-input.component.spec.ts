import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Component } from '@angular/core';

import { TextInputComponent } from './text-input.component';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, TextInputComponent],
  template: `<app-text-input label="Email" [formControl]="control" />`,
})
class HostComponent {
  control = new FormControl('hello');
}

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, TextInputComponent],
  template: `<app-text-input label="Email" [formControl]="control" />`,
})
class RequiredHostComponent {
  control = new FormControl('', Validators.required);
}

describe('TextInputComponent', () => {
  let fixture: ComponentFixture<HostComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HostComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
  });

  it('should create and bind via ControlValueAccessor', () => {
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    expect(input).toBeTruthy();
    expect(input.value).toBe('hello');
  });

  it('should update the form control on input', () => {
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    input.value = 'world';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    expect(fixture.componentInstance.control.value).toBe('world');
  });

  it('should show manual error message', () => {
    const alone = TestBed.createComponent(TextInputComponent);
    alone.componentRef.setInput('error', 'Required');
    alone.detectChanges();
    expect(alone.nativeElement.textContent).toContain('Required');
  });

  it('should show validation error from bound form control', () => {
    const host = TestBed.createComponent(RequiredHostComponent);
    host.detectChanges();

    const input = host.nativeElement.querySelector('input') as HTMLInputElement;
    input.dispatchEvent(new Event('blur'));
    host.componentInstance.control.markAsTouched();
    host.detectChanges();

    expect(host.nativeElement.textContent).toContain('This field is required.');
  });
});
