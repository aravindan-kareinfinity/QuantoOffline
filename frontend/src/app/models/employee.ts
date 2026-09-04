/**
 * Minimal InfyPOS Employee for `EmployeeService/Save` / `Search`.
 * Full HR payload is not required — name (+ optional company/location) is enough.
 */
export class Employee {
  id = 0;
  name = '';
  title = 0;
  emailid = '';
  contactno = '';
  desingnationid = 0;
  refempid = '';
  companyid = 0;
  roleid = 0;
  isactive = true;
  version = 1;
  locationid = 0;
  floorid = 0;
  departmentid = 0;
  sectionid = 0;
  allowsystemaccess = false;
  ErrorMessage?: string;
  errormessage?: string;
}

/** POST EmployeeService/Search criteria — filter by id when editing a linked user. */
export class EmployeeSearchReq {
  id = 0;
  name = '';
  companyid = 0;
  locationid = 0;
  isactive = true;
}
