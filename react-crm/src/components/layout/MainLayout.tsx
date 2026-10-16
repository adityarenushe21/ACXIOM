import React from 'react';
import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../../features/auth/AuthContext';
import { 
  LayoutDashboard, 
  Users, 
  UserCheck, 
  TrendingUp, 
  CalendarClock, 
  BarChart3, 
  LogOut, 
  ShieldCheck,
  Server
} from 'lucide-react';

export const MainLayout: React.FC = () => {
  const { user, logout, isDemoMode } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const isAdmin = user?.roles.includes('Admin');
  const isManager = user?.roles.includes('Manager');

  return (
    <div className="layout-root">
      {/* Sidebar */}
      <aside className="sidebar">
        <div className="sidebar-brand">
          <div className="brand-badge">CRM</div>
          <div>
            <h1 className="brand-title">AcxiomCRM</h1>
            <span className="brand-subtitle">ASP.NET + React</span>
          </div>
        </div>

        <nav className="nav-menu">
          <NavLink to="/dashboard" className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}>
            <LayoutDashboard size={18} />
            <span>Dashboard</span>
          </NavLink>

          <NavLink to="/customers" className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}>
            <Users size={18} />
            <span>Customers</span>
          </NavLink>

          <NavLink to="/leads" className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}>
            <UserCheck size={18} />
            <span>Leads</span>
          </NavLink>

          <NavLink to="/opportunities" className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}>
            <TrendingUp size={18} />
            <span>Opportunities</span>
          </NavLink>

          <NavLink to="/followups" className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}>
            <CalendarClock size={18} />
            <span>Follow-Ups</span>
          </NavLink>

          {(isAdmin || isManager) && (
            <NavLink to="/reports" className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}>
              <BarChart3 size={18} />
              <span>Reports</span>
            </NavLink>
          )}

          {isAdmin && (
            <NavLink to="/admin" className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}>
              <ShieldCheck size={18} />
              <span>User Admin</span>
            </NavLink>
          )}
        </nav>

        <div className="sidebar-footer">
          <div className="user-info">
            <div className="user-avatar">{user?.firstName?.[0] || 'U'}</div>
            <div className="user-details">
              <span className="user-name">{user?.fullName}</span>
              <span className="user-role">{user?.roles?.[0]}</span>
            </div>
          </div>
          <button onClick={handleLogout} className="btn-logout" title="Sign Out">
            <LogOut size={16} />
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <div className="main-wrapper">
        <header className="topbar">
          <div className="backend-indicator">
            <Server size={15} />
            <span>API Target: <strong>http://localhost:5000</strong></span>
            <span className={`status-pill ${isDemoMode ? 'demo' : 'connected'}`}>
              {isDemoMode ? 'Preview / Standalone Mode' : 'Connected to Backend'}
            </span>
          </div>

          <div className="topbar-actions">
            <span className="role-tag">Role: <strong>{user?.roles?.[0]}</strong></span>
          </div>
        </header>

        <main className="content-body">
          <Outlet />
        </main>
      </div>
    </div>
  );
};
