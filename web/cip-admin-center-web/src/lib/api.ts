// Minimal typed client for the CIP Admin Center REST API (routes are prefixed with /api/v1).
const base = "/cip-api";

export type Customer = { id: string; name: string };

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${base}${path}`, {
    ...init,
    headers: { "Content-Type": "application/json", ...init?.headers },
  });
  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new Error(problem?.detail ?? problem?.title ?? `Request failed (${response.status})`);
  }
  return (await response.json()) as T;
}

export async function getHealth(): Promise<string> {
  const response = await fetch(`${base}/health`);
  return response.text();
}

export const createCustomer = (name: string) =>
  request<Customer>("/api/v1/customers", { method: "POST", body: JSON.stringify({ name }) });

export const getCustomer = (id: string) => request<Customer>(`/api/v1/customers/${id}`);
