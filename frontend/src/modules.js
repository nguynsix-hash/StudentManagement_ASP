export const navItems = [
  ['dashboard', 'Dashboard'],
  ['roles', 'Roles'],
  ['accounts', 'Accounts'],
  ['members', 'Members'],
  ['trainers', 'Trainers'],
  ['packages', 'Packages'],
  ['subscriptions', 'Subscriptions'],
  ['payments', 'Payments'],
  ['schedules', 'Schedules'],
  ['attendances', 'Attendances'],
]

const statusOpts = [
  { value: 'Present', label: 'Present' },
  { value: 'Absent', label: 'Absent' },
  { value: 'Late', label: 'Late' },
]

export const modules = {
  roles: {
    title: 'Role Management',
    listPath: () => '/api/roles',
    key: 'id',
    cols: ['id', 'name', 'description'],
    create: { method: 'POST', path: () => '/api/roles', fields: ['name', 'description'] },
    update: { method: 'PUT', path: (r) => `/api/roles/${r.id}`, fields: ['name', 'description'] },
    del: { method: 'DELETE', path: (r) => `/api/roles/${r.id}` },
  },
  accounts: {
    title: 'Account Management',
    listPath: () => '/api/accounts',
    key: 'id',
    cols: ['id', 'username', 'fullName', 'roleName', 'email', 'phone', 'isActive', 'createdAt'],
    create: {
      method: 'POST',
      path: () => '/api/accounts/register',
      fields: ['username', 'password', 'fullName', 'email', 'phone', 'roleId'],
    },
    update: {
      method: 'PUT',
      path: (r) => `/api/accounts/${r.id}`,
      fields: ['fullName', 'email', 'phone', 'roleId', 'isActive'],
    },
    del: { method: 'DELETE', path: (r) => `/api/accounts/${r.id}` },
  },
  members: {
    title: 'Member Management',
    listPath: (f) => (f.keyword ? `/api/members/search?keyword=${encodeURIComponent(f.keyword)}` : '/api/members'),
    filters: [{ key: 'keyword', type: 'text', label: 'Search' }],
    key: 'id',
    cols: ['id', 'memberCode', 'fullName', 'gender', 'phone', 'email', 'isActive', 'createdAt'],
    create: {
      method: 'POST',
      path: () => '/api/members',
      fields: ['memberCode', 'fullName', 'dateOfBirth', 'gender', 'phone', 'email', 'address'],
    },
    update: {
      method: 'PUT',
      path: (r) => `/api/members/${r.id}`,
      fields: ['memberCode', 'fullName', 'dateOfBirth', 'gender', 'phone', 'email', 'address', 'isActive'],
    },
    del: { method: 'DELETE', path: (r) => `/api/members/${r.id}` },
    toggle: { method: 'PATCH', path: (r) => `/api/members/${r.id}/status`, body: (r) => ({ isActive: !r.isActive }) },
  },
  trainers: {
    title: 'Trainer Management',
    listPath: () => '/api/trainers',
    key: 'id',
    cols: ['id', 'trainerCode', 'fullName', 'specialty', 'phone', 'email', 'isActive', 'createdAt'],
    create: { method: 'POST', path: () => '/api/trainers', fields: ['trainerCode', 'fullName', 'specialty', 'phone', 'email'] },
    update: {
      method: 'PUT',
      path: (r) => `/api/trainers/${r.id}`,
      fields: ['trainerCode', 'fullName', 'specialty', 'phone', 'email', 'isActive'],
    },
    del: { method: 'DELETE', path: (r) => `/api/trainers/${r.id}` },
    toggle: { method: 'PATCH', path: (r) => `/api/trainers/${r.id}/status`, body: (r) => ({ isActive: !r.isActive }) },
  },
  packages: {
    title: 'Package Management',
    listPath: () => '/api/membershippackages',
    key: 'id',
    cols: ['id', 'packageCode', 'name', 'durationDays', 'price', 'isActive', 'createdAt'],
    create: {
      method: 'POST',
      path: () => '/api/membershippackages',
      fields: ['packageCode', 'name', 'description', 'durationDays', 'price'],
    },
    update: {
      method: 'PUT',
      path: (r) => `/api/membershippackages/${r.id}`,
      fields: ['packageCode', 'name', 'description', 'durationDays', 'price', 'isActive'],
    },
    del: { method: 'DELETE', path: (r) => `/api/membershippackages/${r.id}` },
    toggle: { method: 'PATCH', path: (r) => `/api/membershippackages/${r.id}/status`, body: (r) => ({ isActive: !r.isActive }) },
  },
  subscriptions: {
    title: 'Subscription Management',
    listPath: (f) => (f.memberId ? `/api/subscriptions/member/${f.memberId}` : '/api/subscriptions'),
    filters: [{ key: 'memberId', type: 'select', label: 'Member', from: 'members' }],
    key: 'id',
    cols: ['id', 'memberName', 'packageName', 'startDate', 'endDate', 'status', 'createdAt'],
    create: {
      method: 'POST',
      path: () => '/api/subscriptions',
      fields: ['memberId', 'membershipPackageId', 'startDate'],
    },
  },
  payments: {
    title: 'Payment Management',
    listPath: (f) => (f.subscriptionId ? `/api/payments/subscription/${f.subscriptionId}` : '/api/payments'),
    filters: [{ key: 'subscriptionId', type: 'select', label: 'Subscription', from: 'subscriptions' }],
    key: 'id',
    cols: ['id', 'subscriptionId', 'memberName', 'amount', 'paymentMethod', 'status', 'paymentDate'],
    create: {
      method: 'POST',
      path: () => '/api/payments',
      fields: ['subscriptionId', 'amount', 'paymentDate', 'paymentMethod', 'status', 'note'],
    },
    update: {
      method: 'PUT',
      path: (r) => `/api/payments/${r.id}`,
      fields: ['amount', 'paymentDate', 'paymentMethod', 'status', 'note'],
    },
    del: { method: 'DELETE', path: (r) => `/api/payments/${r.id}` },
  },
  schedules: {
    title: 'Schedule Management',
    listPath: (f) => (f.trainerId ? `/api/schedules/trainer/${f.trainerId}` : f.memberId ? `/api/schedules/member/${f.memberId}` : '/api/schedules'),
    filters: [
      { key: 'trainerId', type: 'select', label: 'Trainer', from: 'trainers' },
      { key: 'memberId', type: 'select', label: 'Member', from: 'members' },
    ],
    key: 'id',
    cols: ['id', 'title', 'scheduleDate', 'startTime', 'endTime', 'trainerName', 'memberName'],
    create: {
      method: 'POST',
      path: () => '/api/schedules',
      fields: ['title', 'scheduleDate', 'startTime', 'endTime', 'trainerId', 'memberId', 'notes'],
    },
    update: {
      method: 'PUT',
      path: (r) => `/api/schedules/${r.id}`,
      fields: ['title', 'scheduleDate', 'startTime', 'endTime', 'trainerId', 'memberId', 'notes'],
    },
    del: { method: 'DELETE', path: (r) => `/api/schedules/${r.id}` },
  },
  attendances: {
    title: 'Attendance Management',
    listPath: (f) => {
      const q = new URLSearchParams()
      if (f.scheduleId) q.append('scheduleId', f.scheduleId)
      if (f.memberId) q.append('memberId', f.memberId)
      return `/api/attendances${q.toString() ? `?${q.toString()}` : ''}`
    },
    filters: [
      { key: 'scheduleId', type: 'select', label: 'Schedule', from: 'schedules' },
      { key: 'memberId', type: 'select', label: 'Member', from: 'members' },
    ],
    key: 'id',
    cols: ['id', 'scheduleTitle', 'memberName', 'status', 'recordedAt', 'note'],
    create: {
      method: 'POST',
      path: () => '/api/attendances/mark',
      fields: ['scheduleId', 'memberId', 'status', 'note'],
    },
    update: {
      method: 'PUT',
      path: (r) => `/api/attendances/${r.id}`,
      fields: ['status', 'note'],
    },
    del: { method: 'DELETE', path: (r) => `/api/attendances/${r.id}` },
  },
}

export function fieldMeta(key) {
  const map = {
    id: { type: 'number', label: 'ID' },
    name: { type: 'text', label: 'Name' },
    description: { type: 'text', label: 'Description' },
    username: { type: 'text', label: 'Username' },
    password: { type: 'text', label: 'Password' },
    fullName: { type: 'text', label: 'Full Name' },
    roleId: { type: 'select', label: 'Role', from: 'roles', number: true },
    memberId: { type: 'select', label: 'Member', from: 'members', number: true, nullable: true },
    trainerId: { type: 'select', label: 'Trainer', from: 'trainers', number: true },
    membershipPackageId: { type: 'select', label: 'Package', from: 'packages', number: true },
    subscriptionId: { type: 'select', label: 'Subscription', from: 'subscriptions', number: true },
    scheduleId: { type: 'select', label: 'Schedule', from: 'schedules', number: true },
    memberCode: { type: 'text', label: 'Member Code' },
    trainerCode: { type: 'text', label: 'Trainer Code' },
    packageCode: { type: 'text', label: 'Package Code' },
    dateOfBirth: { type: 'date', label: 'Date Of Birth' },
    gender: { type: 'text', label: 'Gender' },
    phone: { type: 'text', label: 'Phone' },
    email: { type: 'email', label: 'Email' },
    address: { type: 'text', label: 'Address' },
    specialty: { type: 'text', label: 'Specialty' },
    durationDays: { type: 'number', label: 'Duration Days' },
    price: { type: 'number', label: 'Price' },
    startDate: { type: 'date', label: 'Start Date' },
    endDate: { type: 'date', label: 'End Date' },
    status: { type: 'select', label: 'Status', options: statusOpts },
    amount: { type: 'number', label: 'Amount' },
    paymentDate: { type: 'datetime-local', label: 'Payment Date' },
    paymentMethod: { type: 'text', label: 'Payment Method' },
    note: { type: 'textarea', label: 'Note' },
    title: { type: 'text', label: 'Title' },
    scheduleDate: { type: 'date', label: 'Schedule Date' },
    startTime: { type: 'text', label: 'Start Time' },
    endTime: { type: 'text', label: 'End Time' },
    notes: { type: 'textarea', label: 'Notes' },
    isActive: { type: 'checkbox', label: 'Active' },
    createdAt: { type: 'datetime', label: 'Created At' },
    roleName: { type: 'text', label: 'Role' },
    memberName: { type: 'text', label: 'Member' },
    packageName: { type: 'text', label: 'Package' },
    scheduleTitle: { type: 'text', label: 'Schedule' },
    recordedAt: { type: 'datetime', label: 'Recorded At' },
  }
  return map[key] ?? { type: 'text', label: key }
}
