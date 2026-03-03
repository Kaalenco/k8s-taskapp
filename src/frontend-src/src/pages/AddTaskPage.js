import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';

const API_URL = window.env?.API_URL || '';

export default function AddTaskPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState({ title: '', description: '', status: 'todo' });
  const [error, setError] = useState(null);
  const [saving, setSaving] = useState(false);

  const set = (field) => (e) => setForm(prev => ({ ...prev, [field]: e.target.value }));

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.title.trim()) { setError('Title is required.'); return; }
    setSaving(true);
    setError(null);
    fetch(`${API_URL}/api/tasks`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(form),
    })
      .then(r => {
        if (!r.ok) throw new Error(`HTTP ${r.status}`);
        return r.json();
      })
      .then(() => navigate('/tasks'))
      .catch(e => { setError(e.message); setSaving(false); });
  };

  return (
    <div>
      <div className="page-header">
        <h1>Add Task</h1>
        <p>Create a new task in the database.</p>
      </div>

      <div className="card">
        {error && <div className="form-error">{error}</div>}
        <form className="form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Title *</label>
            <input
              type="text"
              placeholder="e.g. Deploy to production"
              value={form.title}
              onChange={set('title')}
              autoFocus
            />
          </div>
          <div className="form-group">
            <label>Description</label>
            <textarea
              placeholder="Optional details…"
              value={form.description}
              onChange={set('description')}
            />
          </div>
          <div className="form-group">
            <label>Status</label>
            <select value={form.status} onChange={set('status')}>
              <option value="todo">To Do</option>
              <option value="in-progress">In Progress</option>
              <option value="done">Done</option>
            </select>
          </div>
          <div className="form-row">
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? 'Saving…' : 'Create Task'}
            </button>
            <Link to="/tasks" className="btn btn-secondary">Cancel</Link>
          </div>
        </form>
      </div>
    </div>
  );
}
