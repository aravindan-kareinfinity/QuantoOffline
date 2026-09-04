/**
 * InfyPOS `ActionRequest<T>` body — `{ Item, securitytoken }`.
 * Use for TaxService / CompanyService / OrganisationService / QuantoLiteApi endpoints.
 */
export class ActionRequest<T> {
  Item: T;
  securitytoken = 'String content';

  constructor(item: T) {
    this.Item = item;
  }
}
