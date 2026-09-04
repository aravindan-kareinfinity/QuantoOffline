import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ButtonComponent } from './button.component';

describe('ButtonComponent', () => {
  let component: ButtonComponent;
  let fixture: ComponentFixture<ButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ButtonComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should disable when loading to prevent double clicks', () => {
    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    expect(button.disabled).toBeTrue();
    expect(button.getAttribute('aria-busy')).toBe('true');
  });

  it('should emit clicked when enabled', () => {
    const spy = jasmine.createSpy('clicked');
    component.clicked.subscribe(spy);
    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    button.click();
    expect(spy).toHaveBeenCalled();
  });

  it('should not emit clicked when disabled', () => {
    fixture.componentRef.setInput('disabled', true);
    fixture.detectChanges();
    const spy = jasmine.createSpy('clicked');
    component.clicked.subscribe(spy);
    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    button.click();
    expect(spy).not.toHaveBeenCalled();
  });
});
