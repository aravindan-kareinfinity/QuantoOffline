import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FormScreenHeaderComponent } from './form-screen-header.component';

describe('FormScreenHeaderComponent', () => {
  let component: FormScreenHeaderComponent;
  let fixture: ComponentFixture<FormScreenHeaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FormScreenHeaderComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(FormScreenHeaderComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('backLabel', 'Companies');
    fixture.componentRef.setInput('title', 'Edit company');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
