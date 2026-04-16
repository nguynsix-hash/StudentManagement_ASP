import { useCallback, useEffect, useMemo, useState } from 'react'
import { fieldMeta, modules, navItems } from './modules'
import './App.css'

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? ''
const AUTH_KEY = 'gym_admin_auth'

function api(path) {
  return `${API_BASE}${path}`
}

async function callApi(path, { method = 'GET', body } = {}) {
  const res = await fetch(api(path), {
    method,
    headers: {
      Accept: 'application/json',
      ...(body ? { 'Content-Type': 'application/json' } : {}),
    },
    body: body ? JSON.stringify(body) : undefined,
  })
  const txt = await res.text()
  let data = null
  if (txt) {
    try {
      data = JSON.parse(txt)
    } catch {
      data = txt
    }
  }
  if (!res.ok) {
    const msg = data && typeof data === 'object' && data.message ? data.message : `Request failed (${res.status})`
    throw new Error(msg)
  }
  return data
}

function fmt(v, t) {
  if (v === undefined || v === null || v === '') return '-'
  if (t === 'boolean') return v ? 'Yes' : 'No'
  if (t === 'currency') return Number(v).toLocaleString('vi-VN')
  if (t === 'date') {
    const d = new Date(v)
    return Number.isNaN(d.getTime()) ? String(v) : d.toLocaleDateString('vi-VN')
  }
  if (t === 'datetime') {
    const d = new Date(v)
    return Number.isNaN(d.getTime()) ? String(v) : d.toLocaleString('vi-VN')
  }
  return String(v)
}

function toInput(meta, value) {
  if (meta.type === 'checkbox') return Boolean(value)
  if (value === undefined || value === null) return ''
  if (meta.type === 'date') {
    const d = new Date(value)
    return Number.isNaN(d.getTime()) ? '' : d.toISOString().slice(0, 10)
  }
  if (meta.type === 'datetime-local') {
    const d = new Date(value)
    if (Number.isNaN(d.getTime())) return ''
    const offset = d.getTimezoneOffset()
    return new Date(d.getTime() - offset * 60000).toISOString().slice(0, 16)
  }
  return String(value)
}

function toPayload(meta, value) {
  if (meta.type === 'checkbox') return Boolean(value)
  if (value === '' || value === undefined || value === null) {
    if (meta.nullable) return null
    return undefined
  }
  if (meta.number) {
    const n = Number(value)
    return Number.isFinite(n) ? n : undefined
  }
  if (meta.type === 'datetime-local') {
    const d = new Date(value)
    return Number.isNaN(d.getTime()) ? undefined : d.toISOString()
  }
  return value
}

function emptyFilters() {
  const out = {}
  Object.keys(modules).forEach((k) => {
    out[k] = {}
    ;(modules[k].filters ?? []).forEach((f) => {
      out[k][f.key] = ''
    })
  })
  return out
}

function readStoredAuth() {
  try {
    const raw = localStorage.getItem(AUTH_KEY)
    if (!raw) return null
    return JSON.parse(raw)
  } catch {
    return null
  }
}

function App() {
  const [auth, setAuth] = useState(readStoredAuth)
  const [loginForm, setLoginForm] = useState({ username: 'admin', password: '123456' })
  const [loginLoading, setLoginLoading] = useState(false)
  const [page, setPage] = useState('dashboard')
  const [dashboard, setDashboard] = useState(null)
  const [data, setData] = useState({})
  const [refs, setRefs] = useState({ roles: [], members: [], trainers: [], packages: [], subscriptions: [], schedules: [] })
  const [filters, setFilters] = useState(emptyFilters)
  const [loading, setLoading] = useState({ dashboard: false })
  const [msg, setMsg] = useState({ type: 'info', text: 'Open a module and start managing records.' })
  const [dialog, setDialog] = useState({ open: false, key: '', mode: 'create', record: null, values: {} })

  const mod = useMemo(() => (page === 'dashboard' ? null : modules[page]), [page])

  const optionsFrom = useCallback(
    (src) => {
      if (src === 'roles') return refs.roles.map((r) => ({ value: String(r.id), label: `${r.name} (#${r.id})` }))
      if (src === 'members') return refs.members.map((r) => ({ value: String(r.id), label: `${r.fullName} (${r.memberCode})` }))
      if (src === 'trainers') return refs.trainers.map((r) => ({ value: String(r.id), label: `${r.fullName} (${r.trainerCode})` }))
      if (src === 'packages') return refs.packages.map((r) => ({ value: String(r.id), label: `${r.name} (${r.packageCode})` }))
      if (src === 'subscriptions') return refs.subscriptions.map((r) => ({ value: String(r.id), label: `#${r.id} ${r.memberName}` }))
      if (src === 'schedules') return refs.schedules.map((r) => ({ value: String(r.id), label: `#${r.id} ${r.title}` }))
      return []
    },
    [refs],
  )

  const flag = (k, v) => setLoading((p) => ({ ...p, [k]: v }))

  const notify = (type, text) => setMsg({ type, text })

  const loadRefs = useCallback(async () => {
    try {
      const [roles, members, trainers, packs, subs, sch] = await Promise.all([
        callApi('/api/roles'),
        callApi('/api/members'),
        callApi('/api/trainers'),
        callApi('/api/membershippackages'),
        callApi('/api/subscriptions'),
        callApi('/api/schedules'),
      ])
      setRefs({
        roles: Array.isArray(roles) ? roles : [],
        members: Array.isArray(members) ? members : [],
        trainers: Array.isArray(trainers) ? trainers : [],
        packages: Array.isArray(packs) ? packs : [],
        subscriptions: Array.isArray(subs) ? subs : [],
        schedules: Array.isArray(sch) ? sch : [],
      })
    } catch {
      // ignore hard failure for refs
    }
  }, [])

  const loadDashboard = useCallback(async () => {
    flag('dashboard', true)
    try {
      setDashboard(await callApi('/api/reports/dashboard'))
      notify('success', 'Dashboard refreshed.')
    } catch (e) {
      notify('error', e.message)
    } finally {
      flag('dashboard', false)
    }
  }, [])

  const loadModule = useCallback(
    async (key, f) => {
      const m = modules[key]
      if (!m) return
      flag(key, true)
      try {
        const rows = await callApi(m.listPath(f ?? filters[key] ?? {}))
        setData((p) => ({ ...p, [key]: Array.isArray(rows) ? rows : [] }))
      } catch (e) {
        notify('error', e.message)
      } finally {
        flag(key, false)
      }
    },
    [filters],
  )

  useEffect(() => {
    if (!auth) return
    loadRefs()
    loadDashboard()
  }, [auth, loadRefs, loadDashboard])

  useEffect(() => {
    if (!auth) return
    if (page === 'dashboard') return
    loadModule(page)
  }, [auth, page, loadModule])

  const login = async () => {
    if (!loginForm.username.trim() || !loginForm.password.trim()) {
      notify('warning', 'Nhap username va password.')
      return
    }

    setLoginLoading(true)
    try {
      const user = await callApi('/api/accounts/login', {
        method: 'POST',
        body: {
          username: loginForm.username.trim(),
          password: loginForm.password,
        },
      })

      const normalizedRole = String(user?.roleName ?? '').trim().toLowerCase()
      if (normalizedRole !== 'admin') {
        notify('error', 'Tai khoan khong co quyen Admin.')
        return
      }

      setAuth(user)
      localStorage.setItem(AUTH_KEY, JSON.stringify(user))
      notify('success', `Welcome ${user.fullName || user.username}`)
    } catch (e) {
      notify('error', `Dang nhap that bai: ${e.message}`)
    } finally {
      setLoginLoading(false)
    }
  }

  const logout = () => {
    setAuth(null)
    localStorage.removeItem(AUTH_KEY)
    setPage('dashboard')
    notify('info', 'Da dang xuat.')
  }

  const openForm = (mode, record = null) => {
    const cfg = mode === 'create' ? mod?.create : mod?.update
    if (!cfg) return
    const values = {}
    cfg.fields.forEach((name) => {
      const meta = fieldMeta(name)
      values[name] = toInput(meta, record ? record[name] : '')
    })
    setDialog({ open: true, key: page, mode, record, values })
  }

  const saveForm = async () => {
    const m = modules[dialog.key]
    if (!m) return
    const cfg = dialog.mode === 'create' ? m.create : m.update
    if (!cfg) return

    const body = {}
    cfg.fields.forEach((name) => {
      const meta = fieldMeta(name)
      const parsed = toPayload(meta, dialog.values[name])
      if (parsed !== undefined) body[name] = parsed
    })

    try {
      await callApi(cfg.path(dialog.record), { method: cfg.method, body })
      notify('success', 'Saved successfully.')
      setDialog({ open: false, key: '', mode: 'create', record: null, values: {} })
      await Promise.all([loadModule(dialog.key), loadRefs()])
    } catch (e) {
      notify('error', e.message)
    }
  }

  const removeRow = async (row) => {
    if (!mod?.del) return
    if (!window.confirm('Delete this record?')) return
    try {
      await callApi(mod.del.path(row), { method: mod.del.method })
      notify('success', 'Deleted.')
      await Promise.all([loadModule(page), loadRefs()])
    } catch (e) {
      notify('error', e.message)
    }
  }

  const toggleRow = async (row) => {
    if (!mod?.toggle) return
    try {
      await callApi(mod.toggle.path(row), { method: mod.toggle.method, body: mod.toggle.body(row) })
      notify('success', 'Status updated.')
      await Promise.all([loadModule(page), loadRefs()])
    } catch (e) {
      notify('error', e.message)
    }
  }

  const subAction = async (kind, row) => {
    try {
      if (kind === 'status') {
        const x = await callApi(`/api/subscriptions/${row.id}/status`)
        notify('info', `Status: ${x.status}, active=${x.isActive}`)
      }
      if (kind === 'extend') {
        const raw = window.prompt('Extra days', '30')
        if (!raw) return
        const d = Number(raw)
        if (!Number.isFinite(d) || d <= 0) return
        await callApi(`/api/subscriptions/${row.id}/extend`, { method: 'POST', body: { extraDays: d } })
        notify('success', 'Extended.')
      }
      if (kind === 'set') {
        const s = window.prompt('Status value', row.status || 'Active')
        if (!s) return
        await callApi(`/api/subscriptions/${row.id}/status`, { method: 'PATCH', body: { status: s } })
        notify('success', 'Status changed.')
      }
      if (kind === 'cancel') {
        if (!window.confirm(`Cancel subscription #${row.id}?`)) return
        await callApi(`/api/subscriptions/${row.id}/cancel`, { method: 'PATCH' })
        notify('success', 'Cancelled.')
      }
      await Promise.all([loadModule('subscriptions'), loadRefs()])
    } catch (e) {
      notify('error', e.message)
    }
  }

  const renderField = (name) => {
    const meta = fieldMeta(name)
    const value = dialog.values[name]

    if (meta.type === 'checkbox') {
      return (
        <label key={name} className="check">
          <input
            type="checkbox"
            checked={Boolean(value)}
            onChange={(ev) => setDialog((p) => ({ ...p, values: { ...p.values, [name]: ev.target.checked } }))}
          />
          <span>{meta.label}</span>
        </label>
      )
    }

    if (meta.type === 'select') {
      const opts = meta.options ?? optionsFrom(meta.from)
      return (
        <label key={name} className="field">
          <span>{meta.label}</span>
          <select
            value={value ?? ''}
            onChange={(ev) => setDialog((p) => ({ ...p, values: { ...p.values, [name]: ev.target.value } }))}
          >
            <option value="">Select</option>
            {opts.map((o) => (
              <option key={`${name}-${o.value}`} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        </label>
      )
    }

    if (meta.type === 'textarea') {
      return (
        <label key={name} className="field">
          <span>{meta.label}</span>
          <textarea
            value={value ?? ''}
            onChange={(ev) => setDialog((p) => ({ ...p, values: { ...p.values, [name]: ev.target.value } }))}
          />
        </label>
      )
    }

    const t = ['text', 'number', 'date', 'datetime-local', 'email'].includes(meta.type) ? meta.type : 'text'
    return (
      <label key={name} className="field">
        <span>{meta.label}</span>
        <input
          type={t}
          value={value ?? ''}
          onChange={(ev) => setDialog((p) => ({ ...p, values: { ...p.values, [name]: ev.target.value } }))}
        />
      </label>
    )
  }

  const cards = [
    ['Active Members', dashboard?.activeMembers],
    ['Active Packages', dashboard?.activePackages],
    ['Active Subscriptions', dashboard?.activeSubscriptions],
    ['Monthly Revenue', dashboard ? Number(dashboard.monthlyRevenue ?? 0).toLocaleString('vi-VN') : '-'],
    ['Sessions Held', dashboard?.totalSessionsHeld],
    ['Attendance Records', dashboard?.totalAttendanceRecords],
  ]

  if (!auth) {
    return (
      <div className="loginShell">
        <div className="loginCard">
          <p className="kicker">Gym Admin</p>
          <h1>Admin Sign In</h1>
          <p className="loginHint">Dang nhap tai khoan co role Admin de vao trang quan tri.</p>
          <label className="field">
            <span>Username</span>
            <input
              value={loginForm.username}
              onChange={(e) => setLoginForm((p) => ({ ...p, username: e.target.value }))}
              placeholder="admin"
            />
          </label>
          <label className="field">
            <span>Password</span>
            <input
              type="password"
              value={loginForm.password}
              onChange={(e) => setLoginForm((p) => ({ ...p, password: e.target.value }))}
              placeholder="******"
            />
          </label>
          <button onClick={login} disabled={loginLoading}>
            {loginLoading ? 'Signing in...' : 'Sign In'}
          </button>
          <div className={`msg ${msg.type}`}>{msg.text}</div>
        </div>
      </div>
    )
  }

  return (
    <div className="admin">
      <aside className="side">
        <div className="brand">
          <p>Gym Admin</p>
          <h1>Control Panel</h1>
        </div>
        <nav className="nav-menu">
          {navItems.map(([id, label]) => (
            <button key={id} className={id === page ? 'active' : ''} onClick={() => setPage(id)}>
              {label}
            </button>
          ))}
        </nav>
      </aside>

      <main className="main">
        <header className="head">
          <div className="headText">
            <span className="sectionTag">{page === 'dashboard' ? 'Overview' : 'Operations'}</span>
            <h2>{page === 'dashboard' ? 'Dashboard' : mod?.title}</h2>
            <p>{page === 'dashboard' ? 'Business overview' : 'Manage records with CRUD interface'}</p>
          </div>
          <div className="headRight">
            <div className={`msg ${msg.type}`}>{msg.text}</div>
            <div className="userBox">
              <strong>{auth.fullName || auth.username}</strong>
              <span>{auth.roleName}</span>
              <button className="ghost tiny" onClick={logout}>
                Logout
              </button>
            </div>
          </div>
        </header>

        {page === 'dashboard' ? (
          <section className="panel">
            <div className="tools">
              <button onClick={loadDashboard}>{loading.dashboard ? 'Loading...' : 'Refresh'}</button>
            </div>
            <div className="stats">
              {cards.map(([k, v]) => (
                <article key={k}>
                  <span>{k}</span>
                  <strong>{v ?? '-'}</strong>
                </article>
              ))}
            </div>
            <div className="tableWrap">
              <h3>Expiring Subscriptions (7 days)</h3>
              <table>
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Member</th>
                    <th>Package</th>
                    <th>End</th>
                    <th>Days</th>
                  </tr>
                </thead>
                <tbody>
                  {(dashboard?.expiringSubscriptions ?? []).length === 0 ? (
                    <tr>
                      <td colSpan={5}>No data</td>
                    </tr>
                  ) : (
                    dashboard.expiringSubscriptions.map((x) => (
                      <tr key={x.subscriptionId}>
                        <td>#{x.subscriptionId}</td>
                        <td>{x.memberName}</td>
                        <td>{x.packageName}</td>
                        <td>{fmt(x.endDate, 'date')}</td>
                        <td>{x.daysRemaining}</td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </section>
        ) : (
          <section className="panel">
            <div className="tools">
              <div className="leftTools">
                {mod?.create ? <button onClick={() => openForm('create')}>+ New</button> : null}
                <button className="ghost" onClick={() => loadModule(page)}>
                  {loading[page] ? 'Loading...' : 'Refresh'}
                </button>
              </div>
              {(mod?.filters ?? []).length > 0 ? (
                <div className="filters">
                  {(mod?.filters ?? []).map((f) => {
                    const value = filters[page]?.[f.key] ?? ''
                    if (f.type === 'select') {
                      const opts = optionsFrom(f.from)
                      return (
                        <label key={f.key}>
                          <span>{f.label}</span>
                          <select
                            value={value}
                            onChange={(ev) =>
                              setFilters((p) => ({ ...p, [page]: { ...p[page], [f.key]: ev.target.value } }))
                            }
                          >
                            <option value="">All</option>
                            {opts.map((o) => (
                              <option key={`${f.key}-${o.value}`} value={o.value}>
                                {o.label}
                              </option>
                            ))}
                          </select>
                        </label>
                      )
                    }
                    return (
                      <label key={f.key}>
                        <span>{f.label}</span>
                        <input
                          value={value}
                          onChange={(ev) => setFilters((p) => ({ ...p, [page]: { ...p[page], [f.key]: ev.target.value } }))}
                        />
                      </label>
                    )
                  })}
                  <button className="ghost" onClick={() => loadModule(page)}>
                    Apply
                  </button>
                </div>
              ) : null}
            </div>

            <div className="tableWrap">
              <table>
                <thead>
                  <tr>
                    {mod?.cols.map((c) => (
                      <th key={c}>{fieldMeta(c).label}</th>
                    ))}
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {(data[page] ?? []).length === 0 ? (
                    <tr>
                      <td colSpan={(mod?.cols.length ?? 0) + 1}>No records</td>
                    </tr>
                  ) : (
                    (data[page] ?? []).map((r) => (
                      <tr key={r[mod.key]}>
                        {mod.cols.map((c) => (
                          <td key={`${r[mod.key]}-${c}`}>{fmt(r[c], fieldMeta(c).type)}</td>
                        ))}
                        <td className="actions">
                          {mod.update ? (
                            <button className="tiny" onClick={() => openForm('update', r)}>
                              Edit
                            </button>
                          ) : null}
                          {mod.toggle ? (
                            <button className="tiny ghost" onClick={() => toggleRow(r)}>
                              Toggle
                            </button>
                          ) : null}
                          {page === 'subscriptions' ? (
                            <>
                              <button className="tiny ghost" onClick={() => subAction('status', r)}>
                                Status
                              </button>
                              <button className="tiny ghost" onClick={() => subAction('extend', r)}>
                                Extend
                              </button>
                              <button className="tiny ghost" onClick={() => subAction('set', r)}>
                                Set
                              </button>
                              <button className="tiny ghost" onClick={() => subAction('cancel', r)}>
                                Cancel
                              </button>
                            </>
                          ) : null}
                          {mod.del ? (
                            <button className="tiny danger" onClick={() => removeRow(r)}>
                              Delete
                            </button>
                          ) : null}
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </section>
        )}
      </main>

      {dialog.open ? (
        <div className="modalBg" onClick={() => setDialog((p) => ({ ...p, open: false }))}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <header>
              <h3>{dialog.mode === 'create' ? 'Create Record' : 'Edit Record'}</h3>
              <button className="ghost" onClick={() => setDialog((p) => ({ ...p, open: false }))}>
                Close
              </button>
            </header>
            <form
              onSubmit={(e) => {
                e.preventDefault()
                saveForm()
              }}
            >
              <div className="grid">
                {(dialog.mode === 'create' ? modules[dialog.key]?.create?.fields : modules[dialog.key]?.update?.fields)?.map((f) =>
                  renderField(f),
                )}
              </div>
              <div className="formActions">
                <button type="button" className="ghost" onClick={() => setDialog((p) => ({ ...p, open: false }))}>
                  Cancel
                </button>
                <button type="submit">Save</button>
              </div>
            </form>
          </div>
        </div>
      ) : null}
    </div>
  )
}

export default App

