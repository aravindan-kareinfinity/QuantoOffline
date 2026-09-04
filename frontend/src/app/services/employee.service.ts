import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../environments/environment';
import { ActionRequest } from '../models/action-request';
import { Employee, EmployeeSearchReq } from '../models/employee';

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/EmployeeService`;

  /** POST EmployeeService/Search — same as demo. */
  async search(req: Partial<EmployeeSearchReq> = {}): Promise<Employee[]> {
    const payload = Object.assign(new EmployeeSearchReq(), req);
    const rows = await firstValueFrom(
      this.http.post<Employee[]>(`${this.baseUrl}/Search`, new ActionRequest(payload)),
    );
    return Array.isArray(rows) ? rows : [];
  }

  async getById(id: number): Promise<Employee | null> {
    if (!Number.isFinite(id) || id <= 0) return null;
    const rows = await this.search({ id });
    return rows.find((row) => row.id === id) ?? rows[0] ?? null;
  }

  /** POST EmployeeService/Save — same as demo HR Employee. */
  async save(employee: Employee): Promise<Employee> {
    return firstValueFrom(
      this.http.post<Employee>(`${this.baseUrl}/Save`, new ActionRequest(employee)),
    );
  }
}
