export interface User {
  id: string;
  name: string;
  email: string;
  avatarUrl?: string;
  isAuthenticated: boolean;
}

export interface Site {
  id: string;
  name: string;
  domain: string;
  status: 'active' | 'staging' | 'inactive';
  createdAt: string;
}

export interface UsageMetrics {
  sites: number;
  storage: number; // in GB
  bandwidth: number; // in GB
  visitors: number;
}

export interface NavigationItem {
  key: string;
  text: string;
  icon: string;
  path: string;
}
