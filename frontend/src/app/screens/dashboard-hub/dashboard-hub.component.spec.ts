import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { DashboardHubComponent } from './dashboard-hub.component';

describe('DashboardHubComponent', () => {
  let fixture: ComponentFixture<DashboardHubComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardHubComponent],
      providers: [
        provideRouter([
          {
            path: '',
            component: DashboardHubComponent,
            children: [{ path: '', component: DashboardHubComponent }],
          },
        ]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardHubComponent);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });
});
