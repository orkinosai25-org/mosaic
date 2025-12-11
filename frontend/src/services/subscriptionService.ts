import type { Subscription, Plan } from '../types';

const API_BASE_URL = '/api';

export const subscriptionService = {
  /**
   * Get current subscription for a user
   */
  async getCurrentSubscription(userEmail: string): Promise<Subscription> {
    const response = await fetch(`${API_BASE_URL}/subscription/current?userEmail=${encodeURIComponent(userEmail)}`);
    if (!response.ok) {
      throw new Error('Failed to fetch subscription');
    }
    return response.json();
  },

  /**
   * Get available subscription plans
   */
  async getPlans(): Promise<Plan[]> {
    const response = await fetch(`${API_BASE_URL}/subscription/plans`);
    if (!response.ok) {
      throw new Error('Failed to fetch plans');
    }
    return response.json();
  },

  /**
   * Create a checkout session for new subscription
   */
  async createCheckoutSession(
    userEmail: string,
    tier: string,
    billingInterval: string,
    successUrl: string,
    cancelUrl: string
  ): Promise<string> {
    const response = await fetch(`${API_BASE_URL}/subscription/checkout?userEmail=${encodeURIComponent(userEmail)}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        tier,
        billingInterval,
        successUrl,
        cancelUrl,
      }),
    });
    
    if (!response.ok) {
      throw new Error('Failed to create checkout session');
    }
    
    const data = await response.json();
    return data.sessionUrl;
  },

  /**
   * Update subscription (upgrade/downgrade)
   */
  async updateSubscription(
    userEmail: string,
    newTier: string,
    billingInterval: string
  ): Promise<Subscription> {
    const response = await fetch(`${API_BASE_URL}/subscription/update?userEmail=${encodeURIComponent(userEmail)}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        newTier,
        billingInterval,
      }),
    });
    
    if (!response.ok) {
      throw new Error('Failed to update subscription');
    }
    
    return response.json();
  },

  /**
   * Cancel subscription
   */
  async cancelSubscription(userEmail: string, cancelAtPeriodEnd: boolean = true): Promise<void> {
    const response = await fetch(
      `${API_BASE_URL}/subscription/cancel?userEmail=${encodeURIComponent(userEmail)}&cancelAtPeriodEnd=${cancelAtPeriodEnd}`,
      {
        method: 'DELETE',
      }
    );
    
    if (!response.ok) {
      throw new Error('Failed to cancel subscription');
    }
  },

  /**
   * Create billing portal session
   */
  async createBillingPortalSession(userEmail: string, returnUrl: string): Promise<string> {
    const response = await fetch(`${API_BASE_URL}/subscription/billing-portal?userEmail=${encodeURIComponent(userEmail)}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        returnUrl,
      }),
    });
    
    if (!response.ok) {
      throw new Error('Failed to create billing portal session');
    }
    
    const data = await response.json();
    return data.portalUrl;
  },
};
