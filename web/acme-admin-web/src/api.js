// Thin fetch wrapper. Note the mix of casing: .NET services return camelCase,
// Go/Python/Node services return snake_case. The UI just deals with it.
async function request(path, options = {}) {
  const resp = await fetch(path, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  })
  const text = await resp.text()
  const data = text ? JSON.parse(text) : null
  if (!resp.ok && resp.status !== 409) {
    throw new Error(data?.error || `${resp.status} ${resp.statusText}`)
  }
  return { status: resp.status, data }
}

export const api = {
  orders: () => request('/svc/orders/api/orders'),
  createOrder: (body) => request('/svc/orders/api/orders', { method: 'POST', body: JSON.stringify(body) }),
  customers: () => request('/svc/customers/api/customers'),
  stock: () => request('/svc/inventory/stock'),
  summary: () => request('/svc/reports/reports/summary'),
  notifications: () => request('/svc/notifications/notifications'),
}
