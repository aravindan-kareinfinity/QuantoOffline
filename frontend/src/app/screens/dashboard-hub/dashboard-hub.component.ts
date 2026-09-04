import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-dashboard-hub',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './dashboard-hub.component.html',
  styleUrl: './dashboard-hub.component.scss',
  host: { class: 'block h-full min-h-0' },
})
export class DashboardHubComponent {}
