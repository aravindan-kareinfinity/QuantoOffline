import { Component, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';

import { ButtonComponent } from '../../components/button/button.component';
import { TextInputComponent } from '../../components/text-input/text-input.component';
import { StorageService } from '../../services/storage.service';
import { LoginFormModel } from '../../models/users';
import { UsersService } from '../../services/users.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, ButtonComponent, TextInputComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly router = inject(Router);
  private readonly users = inject(UsersService);
  private readonly storage = inject(StorageService);

  protected readonly isSubmitting = signal(false);
  protected readonly loginError = signal<string | null>(null);

  protected draft = new LoginFormModel();

  protected async login(form: NgForm): Promise<void> {
    for (const control of Object.values(form.controls)) {
      control.markAsTouched();
    }
    if (form.invalid) return;

    this.isSubmitting.set(true);
    this.loginError.set(null);
    try {
      const res = await this.users.login({
        username: this.draft.username.trim(),
        password: this.draft.password,
      });
      this.storage.set(res);
      await this.router.navigate(['/dashboard']);
    } catch {
      this.loginError.set('Login failed.');
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
