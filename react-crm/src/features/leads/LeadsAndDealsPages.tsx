import React from 'react';
import { Plus, PhoneCall } from 'lucide-react';

export const LeadsPage: React.FC = () => {
  const leads = [
    { id: '1', code: 'LEAD-101', name: 'Starlight Retail', contact: 'Alice M.', value: 45000, status: 'Qualified', priority: 'High' },
    { id: '2', code: 'LEAD-102', name: 'Nexus Innovations', contact: 'Bob K.', value: 92000, status: 'Contacted', priority: 'Urgent' },
    { id: '3', code: 'LEAD-103', name: 'Apex Media Corp', contact: 'Carla S.', value: 28000, status: 'New', priority: 'Medium' }
  ];

  return (
    <div className="page-content">
      <div className="page-header">
        <div>
          <h2>Leads Pipeline</h2>
          <p>Manage potential clients, track source channels, and convert to opportunities.</p>
        </div>
        <button className="btn-primary"><Plus size={16} /> New Lead</button>
      </div>

      <div className="card-box p-0">
        <table className="data-table">
          <thead>
            <tr>
              <th>Lead Account</th>
              <th>Code</th>
              <th>Contact</th>
              <th>Expected Value</th>
              <th>Priority</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {leads.map((l) => (
              <tr key={l.id}>
                <td className="font-semibold">{l.name}</td>
                <td><span className="code-badge">{l.code}</span></td>
                <td>{l.contact}</td>
                <td>${l.value.toLocaleString()}</td>
                <td><span className={`priority-badge ${l.priority.toLowerCase()}`}>{l.priority}</span></td>
                <td><span className="status-badge active">{l.status}</span></td>
                <td>
                  <button className="btn-chip"><PhoneCall size={12} /> Contact</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export const OpportunitiesPage: React.FC = () => {
  const deals = [
    { id: '1', name: 'Cloud Migration ERP', customer: 'Acme Global', amount: 150000, stage: 'Proposal', prob: 70 },
    { id: '2', name: 'Cybersecurity Audit', customer: 'Zenith Logistics', amount: 80000, stage: 'Negotiation', prob: 90 },
    { id: '3', name: 'CRM Integration', customer: 'Starlight Retail', amount: 45000, stage: 'Qualification', prob: 40 }
  ];

  return (
    <div className="page-content">
      <div className="page-header">
        <div>
          <h2>Sales Opportunities</h2>
          <p>Track weighted deal forecasting and transition stages to Closed Won.</p>
        </div>
        <button className="btn-primary"><Plus size={16} /> New Opportunity</button>
      </div>

      <div className="card-box p-0">
        <table className="data-table">
          <thead>
            <tr>
              <th>Deal Name</th>
              <th>Customer</th>
              <th>Deal Value</th>
              <th>Probability</th>
              <th>Weighted Value</th>
              <th>Stage</th>
            </tr>
          </thead>
          <tbody>
            {deals.map((d) => (
              <tr key={d.id}>
                <td className="font-semibold">{d.name}</td>
                <td>{d.customer}</td>
                <td>${d.amount.toLocaleString()}</td>
                <td>{d.prob}%</td>
                <td className="text-green font-semibold">${((d.amount * d.prob) / 100).toLocaleString()}</td>
                <td><span className="status-badge active">{d.stage}</span></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export const FollowUpsPage: React.FC = () => {
  const followUps = [
    { id: '1', subject: 'Product Demo & Proposal Review', target: 'Acme Global', date: 'Tomorrow at 10:00 AM', type: 'Call', status: 'Pending' },
    { id: '2', subject: 'Contract Redlining Discussion', target: 'Zenith Logistics', date: 'Oct 22 at 2:30 PM', type: 'Meeting', status: 'Pending' },
    { id: '3', subject: 'Initial Discovery Intro Call', target: 'Starlight Retail', date: 'Oct 18 at 11:00 AM', type: 'Call', status: 'Completed' }
  ];

  return (
    <div className="page-content">
      <div className="page-header">
        <div>
          <h2>Scheduled Follow-Ups</h2>
          <p>Calendar events, task reminders, and prospect engagement touchpoints.</p>
        </div>
        <button className="btn-primary"><Plus size={16} /> Schedule Follow-Up</button>
      </div>

      <div className="card-box p-0">
        <table className="data-table">
          <thead>
            <tr>
              <th>Subject</th>
              <th>Associated Account</th>
              <th>Scheduled Time</th>
              <th>Channel</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {followUps.map((f) => (
              <tr key={f.id}>
                <td className="font-semibold">{f.subject}</td>
                <td>{f.target}</td>
                <td>{f.date}</td>
                <td><span className="code-badge">{f.type}</span></td>
                <td><span className={`status-badge ${f.status === 'Completed' ? 'active' : 'pending'}`}>{f.status}</span></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
