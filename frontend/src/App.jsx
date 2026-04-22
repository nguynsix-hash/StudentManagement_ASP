import { useCallback, useEffect, useMemo, useState } from 'react'
import { fieldMeta, modules, navItems } from './modules'
import './App.css'

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? ''
const AUTH_USER_KEY = 'gym_admin_user'

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
    let msg = `Yeu cau that bai (${res.status})`
    if (data && typeof data === 'object') {
      if (data.message) {
        msg = data.message
      } else if (data.errors && typeof data.errors === 'object') {
        const firstError = Object.values(data.errors).flat().find(Boolean)
        if (typeof firstError === 'string') msg = firstError
      } else if (data.title) {
        msg = data.title
      }
    }
    throw new Error(msg)
  }

  return data
}
function fmt(v, t) {
  if (v === undefined || v === null || v === '') return '-'
  if (t === 'boolean') return v ? 'Có' : 'Không'
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
  if (meta.type === 'time') {
    const text = String(value)
    if (/^\d{2}:\d{2}:\d{2}$/.test(text)) return text.slice(0, 5)
    if (/^\d{2}:\d{2}$/.test(text)) return text
    return ''
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
  if (meta.type === 'time') {
    const text = String(value).trim()
    if (/^\d{2}:\d{2}$/.test(text)) return `${text}:00`
    if (/^\d{2}:\d{2}:\d{2}$/.test(text)) return text
    return undefined
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



function asItems(res) {
  if (Array.isArray(res)) return res
  if (res && typeof res === 'object' && Array.isArray(res.items)) return res.items
  return []
}

function readStoredUser() {
  try {
    const raw = localStorage.getItem(AUTH_USER_KEY)
    if (!raw) return null
    const user = JSON.parse(raw)
    if (!user || typeof user !== 'object') return null
    return user
  } catch {
    return null
  }
}


function App() {
  const [auth, setAuth] = useState(readStoredUser)
  const [loginForm, setLoginForm] = useState({ username: 'admin', password: '123456' })
  const [loginLoading, setLoginLoading] = useState(false)
  const [page, setPage] = useState('dashboard')
  const [dashboard, setDashboard] = useState(null)
  const [data, setData] = useState({})
  const [refs, setRefs] = useState({ roles: [], members: [], trainers: [], packages: [], subscriptions: [], schedules: [] })
  const [filters, setFilters] = useState(emptyFilters)
  const [loading, setLoading] = useState({ dashboard: false })
  const [msg, setMsg] = useState({ type: 'info', text: 'Chọn một module để bắt đầu quản lý dữ liệu.' })
  const [dialog, setDialog] = useState({ open: false, key: '', mode: 'create', record: null, values: {} })
  const [detailDialog, setDetailDialog] = useState({ open: false, key: '', record: null })
  const [passwordDialog, setPasswordDialog] = useState({
    open: false,
    key: '',
    record: null,
    values: { currentPassword: '', newPassword: '', confirmPassword: '' },
  })

  const mod = useMemo(() => (page === 'dashboard' ? null : modules[page]), [page])

  const optionsFrom = useCallback(
    (src, { activeOnly = false } = {}) => {
      if (src === 'roles') return refs.roles.map((r) => ({ value: String(r.id), label: `${r.name} (#${r.id})` }))
      if (src === 'members') {
        const list = activeOnly ? refs.members.filter((r) => r.isActive) : refs.members
        return list.map((r) => ({ value: String(r.id), label: `${r.fullName} (${r.memberCode})` }))
      }
      if (src === 'trainers') {
        const list = activeOnly ? refs.trainers.filter((r) => r.isActive) : refs.trainers
        return list.map((r) => ({ value: String(r.id), label: `${r.fullName} (${r.trainerCode})` }))
      }
      if (src === 'packages') {
        const list = activeOnly ? refs.packages.filter((r) => r.isActive) : refs.packages
        return list.map((r) => ({ value: String(r.id), label: `${r.name} (${r.packageCode})` }))
      }
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
        roles: asItems(roles),
        members: asItems(members),
        trainers: asItems(trainers),
        packages: asItems(packs),
        subscriptions: asItems(subs),
        schedules: asItems(sch),
      })
    } catch {
      // ignore hard failure for refs
    }
  }, [])

  const loadDashboard = useCallback(async () => {
    flag('dashboard', true)
    try {
      setDashboard(await callApi('/api/reports/dashboard'))
      notify('success', 'Đã làm mới tổng quan.')
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
        const currentFilters = f ?? filters[key] ?? {}
        const rows = await callApi(m.listPath(currentFilters))
        const list = asItems(rows)
        const normalized = m.clientFilter ? m.clientFilter(list, currentFilters) : list
        setData((p) => ({ ...p, [key]: normalized }))
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
      notify('warning', 'Nhap ten dang nhap va mat khau.')
      return
    }

    setLoginLoading(true)
    try {
      const loginRes = await callApi('/api/accounts/login', {
        method: 'POST',
        body: {
          username: loginForm.username.trim(),
          password: loginForm.password,
        },
      })

      const user = loginRes?.account ?? loginRes
      const normalizedRole = String(user?.roleName ?? '').trim().toLowerCase()
      if (normalizedRole !== 'admin') {
        notify('error', 'Tai khoan khong co quyen Admin.')
        return
      }
      if (!user) {
        notify('error', 'Phan hoi dang nhap khong hop le.')
        return
      }

      setAuth(user)
      localStorage.setItem(AUTH_USER_KEY, JSON.stringify(user))
      notify('success', `Chao mung ${user.fullName || user.username}`)
    } catch (e) {
      notify('error', `Dang nhap that bai: ${e.message}`)
    } finally {
      setLoginLoading(false)
    }
  }
  const logout = () => {
    setAuth(null)
    localStorage.removeItem(AUTH_USER_KEY)
    setPage('dashboard')
    notify('info', 'Đã đăng xuất.')
  }

  const openForm = (mode, record = null) => {
    const cfg = mode === 'create' ? mod?.create : mod?.update
    if (!cfg) return
    const values = {}
    cfg.fields.forEach((name) => {
      const meta = fieldMeta(name, page)
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
      const meta = fieldMeta(name, dialog.key)
      const parsed = toPayload(meta, dialog.values[name])
      if (parsed !== undefined) body[name] = parsed
    })

    if (dialog.key === 'schedules' && dialog.mode === 'create') {
      const trainer = refs.trainers.find((x) => x.id === Number(body.trainerId))
      if (!trainer || !trainer.isActive) {
        notify('error', 'Huấn luyện viên đang bị khóa. Hãy bật hoạt động trước khi tạo lịch.')
        return
      }
      if (body.memberId !== null && body.memberId !== undefined) {
        const member = refs.members.find((x) => x.id === Number(body.memberId))
        if (!member || !member.isActive) {
          notify('error', 'Hội viên đang bị khóa. Hãy bật hoạt động trước khi tạo lịch.')
          return
        }
      }
    }

    try {
      await callApi(cfg.path(dialog.record), { method: cfg.method, body })
      notify('success', 'Lưu thành công.')
      setDialog({ open: false, key: '', mode: 'create', record: null, values: {} })
      await Promise.all([loadModule(dialog.key), loadRefs()])
    } catch (e) {
      notify('error', e.message)
    }
  }

  const removeRow = async (row) => {
    if (!mod?.del) return
    if (!window.confirm('Bạn có chắc muốn xóa bản ghi này?')) return
    try {
      await callApi(mod.del.path(row), { method: mod.del.method })
      notify('success', 'Đã xóa.')
      await Promise.all([loadModule(page), loadRefs()])
    } catch (e) {
      notify('error', e.message)
    }
  }

  const openDetail = async (row) => {
    if (!mod?.detail) return
    try {
      const record = await callApi(mod.detail.path(row))
      setDetailDialog({ open: true, key: page, record })
    } catch (e) {
      notify('error', e.message)
    }
  }

  const openPasswordDialog = (row) => {
    if (!mod?.changePassword) return
    setPasswordDialog({
      open: true,
      key: page,
      record: row,
      values: { currentPassword: '', newPassword: '', confirmPassword: '' },
    })
  }

  const savePassword = async () => {
    if (!passwordDialog.record || !modules[passwordDialog.key]?.changePassword) return
    const { currentPassword, newPassword, confirmPassword } = passwordDialog.values

    if (!currentPassword || !newPassword || !confirmPassword) {
      notify('warning', 'Vui lòng nhập đủ thông tin đổi mật khẩu.')
      return
    }
    if (newPassword.length < 6) {
      notify('warning', 'Mật khẩu mới cần tối thiểu 6 ký tự.')
      return
    }
    if (newPassword !== confirmPassword) {
      notify('warning', 'Xác nhận mật khẩu mới không khớp.')
      return
    }

    try {
      const cfg = modules[passwordDialog.key].changePassword
      await callApi(cfg.path(passwordDialog.record), {
        method: cfg.method,
        body: { currentPassword, newPassword },
      })
      notify('success', 'Đổi mật khẩu thành công.')
      setPasswordDialog({
        open: false,
        key: '',
        record: null,
        values: { currentPassword: '', newPassword: '', confirmPassword: '' },
      })
    } catch (e) {
      notify('error', e.message)
    }
  }

  const toggleRow = async (row) => {
    if (!mod?.toggle) return
    try {
      await callApi(mod.toggle.path(row), { method: mod.toggle.method, body: mod.toggle.body(row) })
      notify('success', 'Đã cập nhật trạng thái.')
      await Promise.all([loadModule(page), loadRefs()])
    } catch (e) {
      notify('error', e.message)
    }
  }

  const subAction = async (kind, row) => {
    try {
      if (kind === 'status') {
        const x = await callApi(`/api/subscriptions/${row.id}/status`)
        notify('info', `Trạng thái: ${x.status}, kích hoạt=${x.isActive}`)
      }
      if (kind === 'extend') {
        const raw = window.prompt('Số ngày gia hạn thêm', '30')
        if (!raw) return
        const d = Number(raw)
        if (!Number.isFinite(d) || d <= 0) return
        await callApi(`/api/subscriptions/${row.id}/extend`, { method: 'POST', body: { extraDays: d } })
        notify('success', 'Đã gia hạn.')
      }
      if (kind === 'set') {
        const s = window.prompt('Giá trị trạng thái', row.status || 'Active')
        if (!s) return
        await callApi(`/api/subscriptions/${row.id}/status`, { method: 'PATCH', body: { status: s } })
        notify('success', 'Đã đổi trạng thái.')
      }
      if (kind === 'cancel') {
        if (!window.confirm(`Bạn có chắc muốn hủy gói đăng ký #${row.id}?`)) return
        await callApi(`/api/subscriptions/${row.id}/cancel`, { method: 'PATCH' })
        notify('success', 'Đã hủy gói đăng ký.')
      }
      await Promise.all([loadModule('subscriptions'), loadRefs()])
    } catch (e) {
      notify('error', e.message)
    }
  }

  const renderField = (name) => {
    const meta = fieldMeta(name, dialog.key)
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
      const onlyActive =
        dialog.mode === 'create' &&
        ((dialog.key === 'schedules' && (meta.from === 'trainers' || meta.from === 'members')) ||
          (dialog.key === 'subscriptions' && (meta.from === 'members' || meta.from === 'packages')))
      const opts = meta.options ?? optionsFrom(meta.from, { activeOnly: onlyActive })
      return (
        <label key={name} className="field">
          <span>{meta.label}</span>
          <select
            value={value ?? ''}
            onChange={(ev) => setDialog((p) => ({ ...p, values: { ...p.values, [name]: ev.target.value } }))}
          >
            <option value="">Chọn</option>
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

    const t = ['text', 'number', 'date', 'datetime-local', 'email', 'time', 'password'].includes(meta.type) ? meta.type : 'text'
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

  const renderDetailValue = (key, value, moduleKey) => {
    if (value === undefined || value === null || value === '') return '-'
    if (typeof value === 'object') return JSON.stringify(value)
    return fmt(value, fieldMeta(key, moduleKey).type)
  }

  const cards = [
    ['Hội viên đang hoạt động', dashboard?.activeMembers],
    ['Gói tập đang hoạt động', dashboard?.activePackages],
    ['Đăng ký gói đang hiệu lực', dashboard?.activeSubscriptions],
    ['Doanh thu tháng', dashboard ? Number(dashboard.monthlyRevenue ?? 0).toLocaleString('vi-VN') : '-'],
    ['Số buổi đã diễn ra', dashboard?.totalSessionsHeld],
    ['Số bản ghi điểm danh', dashboard?.totalAttendanceRecords],
  ]

  if (!auth) {
    return (
      <div className="loginShell">
        <div className="loginCard">
          <p className="kicker">GYM ADMIN</p>
          <h1>Đăng nhập quản trị</h1>
          <p className="loginHint">Đăng nhập bằng tài khoản có vai trò Admin để vào trang quản trị.</p>
          <label className="field">
            <span>Tên đăng nhập</span>
            <input
              value={loginForm.username}
              onChange={(e) => setLoginForm((p) => ({ ...p, username: e.target.value }))}
              placeholder="admin"
            />
          </label>
          <label className="field">
            <span>Mật khẩu</span>
            <input
              type="password"
              value={loginForm.password}
              onChange={(e) => setLoginForm((p) => ({ ...p, password: e.target.value }))}
              placeholder="******"
            />
          </label>
          <button onClick={login} disabled={loginLoading}>
            {loginLoading ? 'Đang đăng nhập...' : 'Đăng nhập'}
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
          <p>GYM ADMIN</p>
          <h1>Bảng điều khiển</h1>
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
            <span className="sectionTag">{page === 'dashboard' ? 'Tổng quan' : 'Quản trị'}</span>
            <h2>{page === 'dashboard' ? 'Tổng quan' : mod?.title}</h2>
            <p>{page === 'dashboard' ? 'Toàn cảnh hoạt động phòng gym' : 'Quản lý dữ liệu với thao tác CRUD'}</p>
          </div>
          <div className="headRight">
            <div className={`msg ${msg.type}`}>{msg.text}</div>
            <div className="userBox">
              <strong>{auth?.fullName || auth?.username}</strong>
              <span>{auth?.roleName}</span>
              <button className="ghost tiny" onClick={logout}>
                Đăng xuất
              </button>
            </div>
          </div>
        </header>

        {page === 'dashboard' ? (
          <section className="panel">
            <div className="tools">
              <button onClick={loadDashboard}>{loading.dashboard ? 'Đang tải...' : 'Làm mới'}</button>
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
              <h3>Gói đăng ký sắp hết hạn (7 ngày)</h3>
              <table>
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Hội viên</th>
                    <th>Gói tập</th>
                    <th>Hết hạn</th>
                    <th>Còn lại (ngày)</th>
                  </tr>
                </thead>
                <tbody>
                  {(dashboard?.expiringSubscriptions ?? []).length === 0 ? (
                    <tr>
                      <td colSpan={5}>Không có dữ liệu</td>
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
                {mod?.create ? <button onClick={() => openForm('create')}>+ Thêm mới</button> : null}
                <button className="ghost" onClick={() => loadModule(page)}>
                  {loading[page] ? 'Đang tải...' : 'Làm mới'}
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
                            <option value="">Tất cả</option>
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
                  <button
                    className="ghost"
                    onClick={() => {
                      loadModule(page, filters[page])
                    }}
                  >
                    Áp dụng
                  </button>
                </div>
              ) : null}
            </div>

            <div className="tableWrap">
              <table>
                <thead>
                  <tr>
                    {mod?.cols.map((c) => (
                      <th key={c}>{fieldMeta(c, page).label}</th>
                    ))}
                    <th>Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {(data[page] ?? []).length === 0 ? (
                    <tr>
                      <td colSpan={(mod?.cols.length ?? 0) + 1}>Không có dữ liệu</td>
                    </tr>
                  ) : (
                    (data[page] ?? []).map((r) => (
                      <tr key={r[mod.key]}>
                        {mod.cols.map((c) => (
                          <td key={`${r[mod.key]}-${c}`}>{fmt(r[c], fieldMeta(c, page).type)}</td>
                        ))}
                        <td className="actions">
                          {mod.detail ? (
                            <button className="tiny ghost" onClick={() => openDetail(r)}>
                              Chi tiết
                            </button>
                          ) : null}
                          {mod.update ? (
                            <button className="tiny" onClick={() => openForm('update', r)}>
                              Sửa
                            </button>
                          ) : null}
                          {mod.changePassword ? (
                            <button className="tiny ghost" onClick={() => openPasswordDialog(r)}>
                              Đổi mật khẩu
                            </button>
                          ) : null}
                          {mod.toggle ? (
                            <button className="tiny ghost" onClick={() => toggleRow(r)}>
                              Bật/Tắt
                            </button>
                          ) : null}
                          {page === 'subscriptions' ? (
                            <>
                              <button className="tiny ghost" onClick={() => subAction('status', r)}>
                                Trạng thái
                              </button>
                              <button className="tiny ghost" onClick={() => subAction('extend', r)}>
                                Gia hạn
                              </button>
                              <button className="tiny ghost" onClick={() => subAction('set', r)}>
                                Gán
                              </button>
                              <button className="tiny ghost" onClick={() => subAction('cancel', r)}>
                                Hủy
                              </button>
                            </>
                          ) : null}
                          {mod.del ? (
                            <button className="tiny danger" onClick={() => removeRow(r)}>
                              Xóa
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

      {detailDialog.open ? (
        <div className="modalBg" onClick={() => setDetailDialog((p) => ({ ...p, open: false }))}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <header>
              <h3>Chi tiết bản ghi</h3>
              <button className="ghost" onClick={() => setDetailDialog((p) => ({ ...p, open: false }))}>
                Đóng
              </button>
            </header>
            <div className="detailRows">
              {Object.entries(detailDialog.record ?? {}).map(([k, v]) => (
                <div key={k} className="detailRow">
                  <strong>{fieldMeta(k, detailDialog.key).label}</strong>
                  <span>{renderDetailValue(k, v, detailDialog.key)}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      ) : null}

      {passwordDialog.open ? (
        <div className="modalBg" onClick={() => setPasswordDialog((p) => ({ ...p, open: false }))}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <header>
              <h3>Đổi mật khẩu tài khoản</h3>
              <button className="ghost" onClick={() => setPasswordDialog((p) => ({ ...p, open: false }))}>
                Đóng
              </button>
            </header>
            <p className="loginHint">Tài khoản: {passwordDialog.record?.username}</p>
            <form
              onSubmit={(e) => {
                e.preventDefault()
                savePassword()
              }}
            >
              <div className="grid">
                <label className="field">
                  <span>Mật khẩu hiện tại</span>
                  <input
                    type="password"
                    value={passwordDialog.values.currentPassword}
                    onChange={(e) =>
                      setPasswordDialog((p) => ({ ...p, values: { ...p.values, currentPassword: e.target.value } }))
                    }
                  />
                </label>
                <label className="field">
                  <span>Mật khẩu mới</span>
                  <input
                    type="password"
                    value={passwordDialog.values.newPassword}
                    onChange={(e) =>
                      setPasswordDialog((p) => ({ ...p, values: { ...p.values, newPassword: e.target.value } }))
                    }
                  />
                </label>
                <label className="field">
                  <span>Xác nhận mật khẩu mới</span>
                  <input
                    type="password"
                    value={passwordDialog.values.confirmPassword}
                    onChange={(e) =>
                      setPasswordDialog((p) => ({ ...p, values: { ...p.values, confirmPassword: e.target.value } }))
                    }
                  />
                </label>
              </div>
              <div className="formActions">
                <button type="button" className="ghost" onClick={() => setPasswordDialog((p) => ({ ...p, open: false }))}>
                  Hủy
                </button>
                <button type="submit">Cập nhật mật khẩu</button>
              </div>
            </form>
          </div>
        </div>
      ) : null}

      {dialog.open ? (
        <div className="modalBg" onClick={() => setDialog((p) => ({ ...p, open: false }))}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <header>
              <h3>{dialog.mode === 'create' ? 'Tạo bản ghi' : 'Chỉnh sửa bản ghi'}</h3>
              <button className="ghost" onClick={() => setDialog((p) => ({ ...p, open: false }))}>
                Đóng
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
                  Hủy
                </button>
                <button type="submit">Lưu</button>
              </div>
            </form>
          </div>
        </div>
      ) : null}
    </div>
  )
}

export default App















