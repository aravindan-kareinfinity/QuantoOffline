/** Persisted POS counter session — survives navigation until closed or logout. */
export class PosSessionState {
  isOpen = false;
  openedAt = '';
  openingBalance = 0;
  locationid = 0;
  counterid = 0;
  cashId = 0;
}
