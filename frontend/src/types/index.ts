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

export interface TierLimits {
  maxWebsites: number;
  maxStorageBytes: number;
  maxBandwidthBytes: number;
  maxCustomDomains: number;
  hasAds: boolean;
  hasBranding: boolean;
}

export interface Subscription {
  id: number;
  tier: string;
  status: string;
  billingInterval: string;
  priceAmount: number;
  currency: string;
  currentPeriodStart: string;
  currentPeriodEnd: string;
  cancelAtPeriodEnd: boolean;
  canceledAt?: string;
  limits: TierLimits;
}

export interface Plan {
  tier: string;
  name: string;
  description: string;
  monthlyPrice: number;
  yearlyPrice: number;
  monthlyPriceId: string;
  yearlyPriceId: string;
  limits: TierLimits;
}

