import { Directive, Input } from '@angular/core';
import { AbstractControl, NG_VALIDATORS, ValidationErrors, Validator } from '@angular/forms';

export const GSTIN_PATTERN = /^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$/i;
export const PAN_PATTERN = /^[A-Z]{5}[0-9]{4}[A-Z]{1}$/i;
export const IFSC_PATTERN = /^[A-Z]{4}0[A-Z0-9]{6}$/i;
export const NUMERIC_PATTERN = /^\d+$/;
export const PHONE_PATTERN = /^(?:\+?91)?[6-9]\d{9}$/;

/**
 * One reusable, field-agnostic pattern validator — pass any regex as input
 * instead of writing a new directive per field (GSTIN, phone, PAN, ...).
 * Skips validation when the control is empty so fields stay optional.
 */
@Directive({
  selector: '[appOptionalPattern][ngModel]',
  providers: [
    { provide: NG_VALIDATORS, useExisting: OptionalPatternValidatorDirective, multi: true },
  ],
  standalone: true,
})
export class OptionalPatternValidatorDirective implements Validator {
  @Input('appOptionalPattern') pattern: string | RegExp = '';

  validate(control: AbstractControl): ValidationErrors | null {
    const value = String(control.value ?? '').trim();
    if (!value || !this.pattern) return null;
    const re = typeof this.pattern === 'string' ? new RegExp(this.pattern) : this.pattern;
    return re.test(value) ? null : { format: true };
  }
}

/** Import on screens that use template-driven validation helpers. */
export const FORM_VALIDATOR_DIRECTIVES = [OptionalPatternValidatorDirective] as const;
