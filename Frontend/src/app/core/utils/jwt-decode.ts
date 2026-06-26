export interface JwtPayload {
  exp: number;
  role: string;
  firstName: string;
  lastName: string;
  [key: string]: unknown;
}

export function jwtDecode(token: string): JwtPayload | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) {
      return null;
    }
    const payload = parts[1];
    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    const parsed = JSON.parse(jsonPayload);
    
    const firstName = parsed.firstName || parsed.given_name || '';
    const lastName = parsed.lastName || parsed.family_name || '';
    
    return {
      ...parsed,
      exp: parsed.exp ? Number(parsed.exp) : 0,
      role: parsed.role || '',
      firstName,
      lastName
    } as JwtPayload;
  } catch (e) {
    return null;
  }
}
