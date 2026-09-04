/**
 * Successful POST /token response: OAuth2 bearer fields plus InfyPOS custom claims.
 */
export class UsersLoginRes {
  access_token = '';
  token_type?: string;
  expires_in?: number;
  refresh_token?: string;
  userId?: string;
  userName?: string;
  username?: string;
  sessionId?: string;
  employeeId?: string;
  Title?: string;
  Designation?: string;
  OrganizationID?: string;
  OrganizationCode?: string;
  CompanyName?: string;
  LandingScreen?: string;
  Passcode?: string;
}

export class UsersLoginReq {
  username = '';
  password = '';
}

/** OAuth2 error JSON returned with 400 from /token */
export class TokenErrorBody {
  error = '';
  error_description?: string;
}

/** Tenant user — `QuantoLiteApi/Users/*`. */
export class SystemUser {
  id = 0;
  username = '';
  employeename = '';
  employeeid = 0;
  companyid = 0;
  locationid = 0;
  roleid = 0;
  role = 0;
  rolename = '';
  organizationid?: number;
  organizationcode?: string;
  isactive = true;
  email = '';
  mobileno = '';
  version?: number;
  accesskey = '';
  landingpage = '';
  permissions: UserPermission[] = [];
  password?: string;
  createdby = 0;
  modifiedby = 0;
  createdon = '';
  modifiedon = '';
  ErrorMessage?: string;
}

export class UserPermission {
  code = '';
  view = false;
  create = false;
  modify = false;
  delete = false;
  print = false;
}

export class SystemUserSearchReq {
  id = 0;
  username = '';
  locationid = 0;
  inactive = false;
}

export class UserFormModel {
  employeename = '';
  email = '';
  username = '';
  password = '';
  confirmPassword = '';
  companyid: number | null = null;
  locationid: number | null = null;
  /** Role from USERROLE — configuration still pending. */
  roleid: number | null = null;
  isactive = true;
  /** Linked InfyPOS employee id from EmployeeService/Save. */
  employeeid: number | null = null;
}

export class LoginFormModel {
  username = '';
  password = '';
}
