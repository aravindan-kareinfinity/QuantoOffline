import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  host: { class: 'block min-h-0' },
})
export class DashboardComponent {}
