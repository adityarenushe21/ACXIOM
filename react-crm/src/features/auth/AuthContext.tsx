import React, { createContext, useContext, useState, useEffect } from 'react';
import type { UserDto } from '../../types';
import { apiClient } from '../../lib/api-client';

interface AuthContextType {
  user: UserDto | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<boolean>;
  logout: () => Promise<void>;
  isDemoMode: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const DEMO_USER: UserDto = {
  id: 'usr-admin-demo',
  email: 'admin@acxiomcrm.local',
  firstName: 'System',
  lastName: 'Administrator',
  fullName: 'System Administrator',
  isActive: true,
  roles: ['Admin']
};

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<UserDto | null>(() => {
    const saved = localStorage.getItem('acxiom_user');
    return saved ? JSON.parse(saved) : null;
  });
  const [loading, setLoading] = useState(true);
  const [isDemoMode, setIsDemoMode] = useState(false);

  useEffect(() => {
    // Try to reach live backend /me
    apiClient.get<UserDto>('/api/v1/auth/me')
      .then(res => {
        setUser(res.data);
        localStorage.setItem('acxiom_user', JSON.stringify(res.data));
        setIsDemoMode(false);
      })
      .catch(() => {
        // If saved user was demo or backend is not yet started, keep state intact
        if (!user) {
          // Default start with demo user so user can test the UI instantly
          setUser(DEMO_USER);
          setIsDemoMode(true);
        }
      })
      .finally(() => setLoading(false));
  }, []);

  const login = async (email: string, password: string): Promise<boolean> => {
    try {
      const res = await apiClient.post<UserDto>('/api/v1/auth/login', { email, password, rememberMe: true });
      setUser(res.data);
      localStorage.setItem('acxiom_user', JSON.stringify(res.data));
      setIsDemoMode(false);
      return true;
    } catch {
      // If backend is unreachable, gracefully log in as Demo user
      const demo: UserDto = {
        id: 'usr-mock-' + Date.now(),
        email,
        firstName: email.split('@')[0],
        lastName: 'User',
        fullName: email.split('@')[0] + ' (Demo)',
        isActive: true,
        roles: email.includes('sales') ? ['SalesExecutive'] : email.includes('manager') ? ['Manager'] : ['Admin']
      };
      setUser(demo);
      localStorage.setItem('acxiom_user', JSON.stringify(demo));
      setIsDemoMode(true);
      return true;
    }
  };

  const logout = async () => {
    try {
      await apiClient.post('/api/v1/auth/logout');
    } catch {
      // Ignore network errors
    } finally {
      setUser(null);
      localStorage.removeItem('acxiom_user');
    }
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, logout, isDemoMode }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider');
  return ctx;
};
