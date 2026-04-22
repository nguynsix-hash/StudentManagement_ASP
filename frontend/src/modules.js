export const navItems = [
  ['dashboard', 'Tổng quan'],
  ['roles', 'Vai trò'],
  ['accounts', 'Tài khoản'],
  ['members', 'Hội viên'],
  ['trainers', 'Huấn luyện viên'],
  ['packages', 'Gói tập'],
  ['subscriptions', 'Đăng ký gói'],
  ['payments', 'Thanh toán'],
  ['schedules', 'Lịch tập'],
  ['attendances', 'Điểm danh'],
]

const attendanceStatusOpts = [
  { value: 'Present', label: 'Có mặt' },
  { value: 'Absent', label: 'Vắng' },
  { value: 'Late', label: 'Đi muộn' },
]

const subscriptionStatusOpts = [
  { value: 'Active', label: 'Đang hoạt động' },
  { value: 'Expired', label: 'Hết hạn' },
  { value: 'Cancelled', label: 'Đã hủy' },
]

const paymentStatusOpts = [
  { value: 'Paid', label: 'Đã thanh toán' },
  { value: 'Pending', label: 'Chờ thanh toán' },
  { value: 'Refunded', label: 'Đã hoàn tiền' },
  { value: 'Cancelled', label: 'Đã hủy' },
]

const paymentMethodOpts = [
  { value: 'Cash', label: 'Tiền mặt' },
  { value: 'Card', label: 'Thẻ' },
  { value: 'Transfer', label: 'Chuyển khoản' },
  { value: 'E-Wallet', label: 'Ví điện tử' },
]

export const modules = {
  roles: {
    title: 'Quản lý vai trò',
    listPath: () => '/api/roles',
    key: 'id',
    cols: ['id', 'name', 'description'],
    create: { method: 'POST', path: () => '/api/roles', fields: ['name', 'description'] },
    update: { method: 'PUT', path: (r) => `/api/roles/${r.id}`, fields: ['name', 'description'] },
    del: { method: 'DELETE', path: (r) => `/api/roles/${r.id}` },
  },
  accounts: {
    title: 'Quản lý tài khoản',
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
    title: 'Quản lý hội viên',
    listPath: (f) => (f.keyword ? `/api/members/search?keyword=${encodeURIComponent(f.keyword)}` : '/api/members'),
    filters: [{ key: 'keyword', type: 'text', label: 'Tìm kiếm' }],
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
    title: 'Quản lý huấn luyện viên',
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
    title: 'Quản lý gói tập',
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
    title: 'Quản lý đăng ký gói',
    listPath: (f) => (f.memberId ? `/api/subscriptions/member/${f.memberId}` : '/api/subscriptions'),
    filters: [{ key: 'memberId', type: 'select', label: 'Hội viên', from: 'members' }],
    key: 'id',
    cols: ['id', 'memberName', 'packageName', 'startDate', 'endDate', 'status', 'createdAt'],
    create: {
      method: 'POST',
      path: () => '/api/subscriptions',
      fields: ['memberId', 'membershipPackageId', 'startDate'],
    },
  },
  payments: {
    title: 'Quản lý thanh toán',
    listPath: (f) => (f.subscriptionId ? `/api/payments/subscription/${f.subscriptionId}` : '/api/payments'),
    filters: [{ key: 'subscriptionId', type: 'select', label: 'Đăng ký gói', from: 'subscriptions' }],
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
    title: 'Quản lý lịch tập',
    listPath: (f) => (f.trainerId ? `/api/schedules/trainer/${f.trainerId}` : f.memberId ? `/api/schedules/member/${f.memberId}` : '/api/schedules'),
    clientFilter: (rows, f) =>
      rows.filter(
        (r) =>
          (!f.trainerId || String(r.trainerId) === String(f.trainerId)) &&
          (!f.memberId || String(r.memberId ?? '') === String(f.memberId)),
      ),
    filters: [
      { key: 'trainerId', type: 'select', label: 'Huấn luyện viên', from: 'trainers' },
      { key: 'memberId', type: 'select', label: 'Hội viên', from: 'members' },
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
    title: 'Quản lý điểm danh',
    listPath: (f) => {
      const q = new URLSearchParams()
      if (f.scheduleId) q.append('scheduleId', f.scheduleId)
      if (f.memberId) q.append('memberId', f.memberId)
      return `/api/attendances${q.toString() ? `?${q.toString()}` : ''}`
    },
    filters: [
      { key: 'scheduleId', type: 'select', label: 'Lịch tập', from: 'schedules' },
      { key: 'memberId', type: 'select', label: 'Hội viên', from: 'members' },
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

export function fieldMeta(key, moduleKey) {
  const statusOptionsByModule = {
    attendances: attendanceStatusOpts,
    payments: paymentStatusOpts,
    subscriptions: subscriptionStatusOpts,
  }

  const map = {
    id: { type: 'number', label: 'ID' },
    name: { type: 'text', label: 'Tên' },
    description: { type: 'text', label: 'Mô tả' },
    username: { type: 'text', label: 'Tên đăng nhập' },
    password: { type: 'text', label: 'Mật khẩu' },
    fullName: { type: 'text', label: 'Họ và tên' },
    roleId: { type: 'select', label: 'Vai trò', from: 'roles', number: true },
    memberId: { type: 'select', label: 'Hội viên', from: 'members', number: true, nullable: true },
    trainerId: { type: 'select', label: 'Huấn luyện viên', from: 'trainers', number: true },
    membershipPackageId: { type: 'select', label: 'Gói tập', from: 'packages', number: true },
    subscriptionId: { type: 'select', label: 'Đăng ký gói', from: 'subscriptions', number: true },
    scheduleId: { type: 'select', label: 'Lịch tập', from: 'schedules', number: true },
    memberCode: { type: 'text', label: 'Mã hội viên' },
    trainerCode: { type: 'text', label: 'Mã huấn luyện viên' },
    packageCode: { type: 'text', label: 'Mã gói tập' },
    dateOfBirth: { type: 'date', label: 'Ngày sinh' },
    gender: { type: 'text', label: 'Giới tính' },
    phone: { type: 'text', label: 'Số điện thoại' },
    email: { type: 'email', label: 'Email' },
    address: { type: 'text', label: 'Địa chỉ' },
    specialty: { type: 'text', label: 'Chuyên môn' },
    durationDays: { type: 'number', label: 'Số ngày' },
    price: { type: 'number', label: 'Giá' },
    startDate: { type: 'date', label: 'Ngày bắt đầu' },
    endDate: { type: 'date', label: 'Ngày kết thúc' },
    status: { type: 'select', label: 'Trạng thái', options: statusOptionsByModule[moduleKey] ?? [] },
    amount: { type: 'number', label: 'Số tiền' },
    paymentDate: { type: 'datetime-local', label: 'Ngày thanh toán' },
    paymentMethod: {
      type: moduleKey === 'payments' ? 'select' : 'text',
      label: 'Phương thức thanh toán',
      options: moduleKey === 'payments' ? paymentMethodOpts : undefined,
    },
    note: { type: 'textarea', label: 'Ghi chú' },
    title: { type: 'text', label: 'Tiêu đề' },
    scheduleDate: { type: 'date', label: 'Ngày tập' },
    startTime: { type: 'time', label: 'Giờ bắt đầu' },
    endTime: { type: 'time', label: 'Giờ kết thúc' },
    notes: { type: 'textarea', label: 'Ghi chú thêm' },
    isActive: { type: 'checkbox', label: 'Đang hoạt động' },
    createdAt: { type: 'datetime', label: 'Ngày tạo' },
    roleName: { type: 'text', label: 'Vai trò' },
    memberName: { type: 'text', label: 'Hội viên' },
    packageName: { type: 'text', label: 'Gói tập' },
    scheduleTitle: { type: 'text', label: 'Lịch tập' },
    recordedAt: { type: 'datetime', label: 'Thời gian ghi nhận' },
  }
  return map[key] ?? { type: 'text', label: key }
}
