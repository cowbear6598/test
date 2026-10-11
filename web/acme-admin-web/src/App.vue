<script setup>
import { onMounted, reactive, ref } from 'vue'
import { api } from './api.js'

const summary = ref(null)
const orders = ref([])
const customers = ref([])
const stock = ref([])
const notifications = ref([])
const error = ref('')
const message = ref('')
const form = reactive({ customerId: 1, sku: 'WIDGET-001', quantity: 1 })

async function loadAll() {
  error.value = ''
  try {
    const [s, o, c, st, n] = await Promise.all([
      api.summary(), api.orders(), api.customers(), api.stock(), api.notifications(),
    ])
    summary.value = s.data
    orders.value = o.data
    customers.value = c.data
    stock.value = st.data
    notifications.value = n.data
  } catch (e) {
    error.value = `Failed to load: ${e.message}`
  }
}

async function submitOrder() {
  message.value = ''
  error.value = ''
  try {
    const { status, data } = await api.createOrder({ ...form })
    message.value = status === 409
      ? `Order #${data.id} rejected: ${data.rejectReason}`
      : `Order #${data.id} created, total ${data.total}`
    await loadAll()
  } catch (e) {
    error.value = `Create failed: ${e.message}`
  }
}

onMounted(loadAll)
</script>

<template>
  <main>
    <header class="topbar">
      <h1>Acme Admin</h1>
      <span class="version">v0.5.0-beta.1</span>
    </header>
    <p v-if="error" class="error">{{ error }}</p>
    <p v-if="message" class="message">{{ message }}</p>
    <button @click="loadAll">Refresh</button>

    <section v-if="summary">
      <h2>Summary <small>(report-service)</small></h2>
      <ul>
        <li>Orders: {{ summary.total_orders }} ({{ summary.rejected_orders }} rejected)</li>
        <li>Revenue: {{ summary.total_revenue }}</li>
        <li v-for="(v, tier) in summary.revenue_by_tier" :key="tier">{{ tier }}: {{ v }}</li>
        <li>Low stock: {{ summary.low_stock_skus.join(', ') || 'none' }}</li>
      </ul>
    </section>

    <section>
      <h2>New order <small>(orders-api)</small></h2>
      <form @submit.prevent="submitOrder">
        <select v-model.number="form.customerId">
          <option v-for="c in customers" :key="c.id" :value="c.id">{{ c.name }} ({{ c.tier }})</option>
        </select>
        <select v-model="form.sku">
          <option v-for="s in stock" :key="s.sku" :value="s.sku">{{ s.sku }} - {{ s.name }}</option>
        </select>
        <input v-model.number="form.quantity" type="number" min="1" />
        <button type="submit">Place order</button>
      </form>
    </section>

    <section>
      <h2>Orders <small>(orders-api)</small></h2>
      <table>
        <tr><th>#</th><th>Customer</th><th>SKU</th><th>Qty</th><th>Unit</th><th>Disc.</th><th>Total</th><th>Status</th></tr>
        <tr v-for="o in orders" :key="o.id">
          <td>{{ o.id }}</td><td>{{ o.customerName }}</td><td>{{ o.sku }}</td><td>{{ o.quantity }}</td>
          <td class="num">{{ o.unitPrice }}</td><td class="num">{{ o.discountRate }}</td><td class="num">{{ o.total }}</td>
          <td><span class="badge" :data-status="o.status">{{ o.status }}</span><span v-if="o.rejectReason"> ({{ o.rejectReason }})</span></td>
        </tr>
      </table>
    </section>

    <section>
      <h2>Stock <small>(inventory-service, called directly)</small></h2>
      <table>
        <tr><th>SKU</th><th>Name</th><th>Available</th><th>Reserved</th></tr>
        <tr v-for="s in stock" :key="s.sku">
          <td>{{ s.sku }}</td><td>{{ s.name }}</td><td>{{ s.quantity }}</td><td>{{ s.reserved }}</td>
        </tr>
      </table>
    </section>

    <section>
      <h2>Notifications <small>(notification-service)</small></h2>
      <table>
        <tr><th>#</th><th>Channel</th><th>To</th><th>Subject</th></tr>
        <tr v-for="n in notifications" :key="n.id">
          <td>{{ n.id }}</td><td>{{ n.channel }}</td><td>{{ n.to }}</td><td>{{ n.subject }}</td>
        </tr>
      </table>
    </section>
  </main>
</template>

<style>
body { font-family: 'Inter', system-ui, sans-serif; margin: 0; color: #1f2933; background: #f5f7fa; }
main { max-width: 1100px; margin: 0 auto; padding: 1.5rem; }
.topbar { display: flex; align-items: baseline; gap: 1rem; border-bottom: 2px solid #3e4c59; }
.version { color: #7b8794; font-size: 0.85rem; }
.num { text-align: right; font-variant-numeric: tabular-nums; }
.badge { padding: 2px 6px; border-radius: 4px; background: #e4e7eb; }
.badge[data-status='Rejected'] { background: #fde2e2; color: #b00020; }
small { color: #888; font-weight: normal; font-size: 0.7em; }
table { border-collapse: collapse; margin-bottom: 1rem; }
th, td { border: 1px solid #ccc; padding: 4px 8px; text-align: left; }
form > * { margin-right: 0.5rem; }
.error { color: #b00020; }
.message { color: #1b5e20; }
</style>
