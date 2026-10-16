import React, { useState } from 'react';
import { useAuth } from '../AuthContext';
import { useNavigate } from 'react-router-dom';
import { Lock, Mail, ShieldAlert, CheckCircle2 } from 'lucide-react';

export const LoginPage: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('admin@acxiomcrm.local');
  const [password, setPassword] = useState('CrmDev@2024Secure!');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      await login(email, password);
      navigate('/dashboard');
    } catch {
      setError('Invalid login credentials.');
    } finally {
      setLoading(false);
    }
  };

  const handleQuickFill = (role: 'Admin' | 'Manager' | 'SalesExecutive') => {
    if (role === 'Admin') setEmail('admin@acxiomcrm.local');
    if (role === 'Manager') setEmail('manager@acxiomcrm.local');
    if (role === 'SalesExecutive') setEmail('sales@acxiomcrm.local');
    setPassword('CrmDev@2024Secure!');
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <div className="login-header">
          <div className="brand-logo">
            <Lock className="logo-icon" />
          </div>
          <h2>AcxiomCRM</h2>
          <p>Enterprise ASP.NET Core & React Client</p>
        </div>

        {error && (
          <div className="error-alert">
            <ShieldAlert size={18} />
            <span>{error}</span>
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Work Email</label>
            <div className="input-with-icon">
              <Mail className="input-icon" size={18} />
              <input
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="name@company.com"
              />
            </div>
          </div>

          <div className="form-group">
            <label>Password</label>
            <div className="input-with-icon">
              <Lock className="input-icon" size={18} />
              <input
                type="password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••••••"
              />
            </div>
          </div>

          <button type="submit" disabled={loading} className="btn-primary w-full">
            {loading ? 'Authenticating...' : 'Sign In'}
          </button>
        </form>

        <div className="quick-roles">
          <p className="quick-title">Quick-fill Demo Roles:</p>
          <div className="role-buttons">
            <button type="button" onClick={() => handleQuickFill('Admin')} className="btn-chip">
              <CheckCircle2 size={13} /> Admin
            </button>
            <button type="button" onClick={() => handleQuickFill('Manager')} className="btn-chip">
              <CheckCircle2 size={13} /> Manager
            </button>
            <button type="button" onClick={() => handleQuickFill('SalesExecutive')} className="btn-chip">
              <CheckCircle2 size={13} /> Sales Exec
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
