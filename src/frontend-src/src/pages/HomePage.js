import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

const API_URL = window.env?.API_URL || '';

export default function HomePage() {
  const [status, setStatus] = useState('checking'); // 'checking' | 'ok' | 'err'

  useEffect(() => {
    const controller = new AbortController();
    fetch(`${API_URL}/api/tasks`, { signal: controller.signal })
      .then(r => setStatus(r.ok ? 'ok' : 'err'))
      .catch(() => setStatus('err'));
    return () => controller.abort();
  }, []);

  const statusLabel = {
    checking: 'Checking backend…',
    ok: 'Backend: Connected ✓',
    err: 'Backend: Unavailable ✗',
  };

  return (
    <div>
      <div className="card home-hero">
        <h1>TaskApp</h1>
        <p>A simple task manager deployed on Kubernetes — Session 15 &amp; 16 sample application.</p>

        <div className={`home-status ${status}`}>
          {statusLabel[status]}
        </div>

        <div className="home-actions">
          <Link to="/tasks" className="btn btn-primary">View Tasks</Link>
          <Link to="/tasks/new" className="btn btn-secondary">Add Task</Link>
          <Link to="/status" className="btn btn-secondary">Backend Status</Link>
        </div>
      </div>

      <div className="card">
        <h3 style={{ marginBottom: 14, fontSize: 15, color: '#1a2744' }}>About this app</h3>
        <p style={{ fontSize: 14, color: '#6b7a99', lineHeight: 1.6 }}>
          This application demonstrates a three-tier architecture running in Kubernetes:
        </p>
        <ul style={{ marginTop: 12, paddingLeft: 20, fontSize: 14, color: '#6b7a99', lineHeight: 2 }}>
          <li><strong style={{ color: '#1a2744' }}>Frontend</strong> — This React app, served by nginx</li>
          <li><strong style={{ color: '#1a2744' }}>Backend</strong> — Node.js REST API at <code style={{ background: '#f0f2f8', padding: '1px 6px', borderRadius: 4 }}>/api/tasks</code></li>
          <li><strong style={{ color: '#1a2744' }}>Database</strong> — MySQL for persistent task storage</li>
        </ul>
      </div>
    </div>
  );
}
