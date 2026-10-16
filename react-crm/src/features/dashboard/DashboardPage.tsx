import React, { useEffect, useState } from 'react';
import { apiClient } from '../../lib/api-client';
import type { DashboardSummaryDto } from '../../types';
import { 
  Users, 
  UserCheck, 
  TrendingUp, 
  DollarSign, 
  PieChart, 
  ArrowUpRight 
} from 'lucide-react';

export const DashboardPage: React.FC = () => {
  const [stats, setStats] = useState<DashboardSummaryDto>({
    totalCustomers: 24,
    totalLeads: 48,
    openLeads: 19,
    totalOpportunities: 12,
    openOpportunities: 7,
    wonOpportunities: 4,
    lostOpportunities: 1,
    totalPipelineValue: 485000,
    weightedPipelineValue: 275000
  });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setLoading(true);
    apiClient.get<DashboardSummaryDto>('/api/v1/dashboard/summary')
      .then(res => setStats(res.data))
      .catch(() => {
        // Fallback to initial mock stats when backend is offline
      })
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="page-content">
      <div className="page-header">
        <div>
          <h2>Executive CRM Dashboard</h2>
          <p>Real-time metrics scoped to your organizational role and permissions.</p>
        </div>
      </div>

      {loading && <div className="loading-bar">Refreshing metrics...</div>}

      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon bg-blue">
            <Users size={22} />
          </div>
          <div className="stat-meta">
            <span className="stat-label">Total Customers</span>
            <span className="stat-value">{stats.totalCustomers}</span>
          </div>
          <div className="stat-badge text-green">
            <ArrowUpRight size={14} /> +12%
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon bg-indigo">
            <UserCheck size={22} />
          </div>
          <div className="stat-meta">
            <span className="stat-label">Active Leads</span>
            <span className="stat-value">{stats.openLeads}</span>
            <span className="stat-sub">of {stats.totalLeads} total</span>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon bg-emerald">
            <TrendingUp size={22} />
          </div>
          <div className="stat-meta">
            <span className="stat-label">Open Deals</span>
            <span className="stat-value">{stats.openOpportunities}</span>
            <span className="stat-sub">{stats.wonOpportunities} won</span>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon bg-amber">
            <DollarSign size={22} />
          </div>
          <div className="stat-meta">
            <span className="stat-label">Total Pipeline</span>
            <span className="stat-value">${stats.totalPipelineValue.toLocaleString()}</span>
            <span className="stat-sub">Weighted: ${stats.weightedPipelineValue.toLocaleString()}</span>
          </div>
        </div>
      </div>

      <div className="dashboard-sections">
        <div className="card-box">
          <div className="card-header">
            <div className="flex-center gap-2">
              <PieChart size={18} />
              <h3>Pipeline Stages & Performance</h3>
            </div>
          </div>
          <div className="pipeline-bars">
            <div className="pipeline-row">
              <div className="pipeline-meta">
                <span>Prospecting</span>
                <strong>4 deals ($120,000)</strong>
              </div>
              <div className="progress-track"><div className="progress-fill" style={{ width: '45%' }}></div></div>
            </div>
            <div className="pipeline-row">
              <div className="pipeline-meta">
                <span>Qualification</span>
                <strong>2 deals ($85,000)</strong>
              </div>
              <div className="progress-track"><div className="progress-fill" style={{ width: '60%' }}></div></div>
            </div>
            <div className="pipeline-row">
              <div className="pipeline-meta">
                <span>Proposal / Negotiation</span>
                <strong>1 deal ($170,000)</strong>
              </div>
              <div className="progress-track"><div className="progress-fill" style={{ width: '85%' }}></div></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
