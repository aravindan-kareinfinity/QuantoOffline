import { TokenErrorBody, UsersLoginRes } from '../models/users';

function pickString(raw: Record<string, unknown>, ...keys: string[]): string {
  for (const key of keys) {
    const v = raw[key];
    if (v != null && v !== '') return String(v);
  }
  return '';
}

function toRecord(body: string | Record<string, unknown>): Record<string, unknown> {
  if (typeof body === 'string') {
    const trimmed = body?.trim() ?? '';
    if (!trimmed) {
      throw {
        status: 400,
        error: {
          error: 'empty_response',
          error_description: 'Token endpoint returned an empty body.',
        },
      };
    }
    try {
      return JSON.parse(trimmed) as Record<string, unknown>;
    } catch {
      const isHtml = trimmed.startsWith('<!') || trimmed.startsWith('<html');
      throw {
        status: 400,
        error: {
          error: 'invalid_response',
          error_description: isHtml
            ? 'Token endpoint returned HTML instead of JSON. Check baseUrl and CORS.'
            : 'Token endpoint did not return JSON.',
        },
      };
    }
  }
  return body;
}

/** Maps OAuth / InfyPOS token JSON into {@link UsersLoginRes}. */
export function parseLoginResponseBody(body: string | Record<string, unknown>): UsersLoginRes {
  const raw = toRecord(body);

  const errCode = raw['error'];
  if (typeof errCode === 'string' && errCode) {
    throw { status: 400, error: raw as unknown as TokenErrorBody };
  }

  const res = new UsersLoginRes();
  res.access_token = pickString(raw, 'access_token', 'Access_token', 'accessToken');
  res.token_type = pickString(raw, 'token_type', 'Token_type', 'tokenType') || undefined;
  const expires = raw['expires_in'] ?? raw['Expires_in'] ?? raw['expiresIn'];
  if (expires != null) res.expires_in = Number(expires);
  res.refresh_token = pickString(raw, 'refresh_token', 'Refresh_token', 'refreshToken') || undefined;
  res.userName = pickString(raw, 'userName', 'UserName', 'username');
  res.username = pickString(raw, 'username', 'Username', 'userName');
  res.userId = pickString(raw, 'userId', 'UserId');
  res.sessionId = pickString(raw, 'sessionId', 'SessionId');
  res.employeeId = pickString(raw, 'employeeId', 'EmployeeId', 'employeeid');
  res.Title = pickString(raw, 'Title');
  res.Designation = pickString(raw, 'Designation');
  res.OrganizationID = pickString(raw, 'OrganizationID', 'organizationId');
  res.OrganizationCode = pickString(raw, 'OrganizationCode', 'organizationCode');
  res.CompanyName = pickString(raw, 'CompanyName', 'companyName');
  res.LandingScreen = pickString(raw, 'LandingScreen', 'landingScreen');
  res.Passcode = pickString(raw, 'Passcode', 'passcode');

  if (!res.access_token.trim()) {
    throw {
      status: 400,
      error: {
        error: 'invalid_response',
        error_description: 'Login succeeded but no access token was returned.',
      },
    };
  }

  return res;
}
