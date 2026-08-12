import { UilBox, UilChartPie, UilClipboardNotes, UilHistory, UilLayerGroup } from '@iconscout/react-unicons';
import { NavLink, Outlet } from 'react-router-dom';
import { cn } from '@/lib/cn';

const navItems = [
  { label: 'Purchase Orders', to: '/purchase-orders', icon: UilClipboardNotes },
  { label: 'Inventory Items', to: '/inventory-items', icon: UilBox },
  { label: 'Finance Reports', to: '/finance', icon: UilChartPie },
  { label: 'Audit Log', to: '/audit-log', icon: UilHistory }
];

export const WorkspaceLayout = () => (
  <div className="min-h-screen bg-background">
    <aside className="fixed inset-y-0 left-0 hidden w-64 border-r bg-slate-950 text-white lg:block">
      <div className="flex h-16 items-center gap-3 border-b border-white/10 px-5">
        <UilLayerGroup className="h-6 w-6 text-teal-300" />
        <div>
          <div className="text-sm font-semibold">PurchaseOrderApp</div>
          <div className="text-xs text-slate-400">Stock reservations</div>
        </div>
      </div>
      <nav className="grid gap-1 p-3">
        {navItems.map((item) => {
          const Icon = item.icon;
          return (
            <NavLink className={({ isActive }) => cn('flex items-center gap-3 rounded-md px-3 py-2.5 text-sm font-medium text-slate-300 hover:bg-white/10 hover:text-white', isActive && 'bg-white/10 text-white')} key={item.to} to={item.to}>
              <Icon className="h-5 w-5" />
              {item.label}
            </NavLink>
          );
        })}
      </nav>
    </aside>
    <div className="lg:pl-64">
      <header className="sticky top-0 z-10 flex h-14 items-center gap-2 overflow-x-auto border-b bg-card px-4 lg:hidden">
        {navItems.map((item) => (
          <NavLink className={({ isActive }) => cn('whitespace-nowrap rounded-md px-2 py-1.5 text-xs font-semibold', isActive ? 'bg-primary text-white' : 'text-secondary')} key={item.to} to={item.to}>
            {item.label}
          </NavLink>
        ))}
      </header>
      <Outlet />
    </div>
  </div>
);
