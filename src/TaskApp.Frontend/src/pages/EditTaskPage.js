import React, { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';

const API_URL = window.env?.API_URL || '';

export default function EditTaskPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [form, setForm] = useState({ title: '', description: '', status: 'todo' });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    fetch(`${API_URL}/api/tasks/${id}`)
      .then(r => {
        if (!r.ok) throw new Error(`HTTP ${r.status}`);
        return r.json();
      })
      .then(task => {
        setForm({
          title: task.title || '',
          description: task.description || '',
          status: task.status || 'todo',
        });
      })
      .catch(e => setError(e.message))
      .finally(() => setLoading(false));
  }, [id]);

  const set = (field) => (e) => setForm(prev => ({ ...prev, [field]: e.target.value }));

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.title.trim()) { setError('Title is required.'); return; }
    setSaving(true);
    setError(null);
    fetch(`${API_URL}/api/tasks/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(form),
    })
      .then(r => {
        if (!r.ok) throw new Error(`HTTP ${r.status}`);
      })
      .then(() => navigate('/tasks'))
      .catch(e => { setError(e.message); setSaving(false); });
  };

  const handleDelete = () => {
    if (!window.confirm('Permanently delete this task?')) return;
    fetch(`${API_URL}/api/tasks/${id}`, { method: 'DELETE' })
      .then(() => navigate('/tasks'))
      .catch(e => alert('Delete failed: ' + e.message));
  };

  if (loading) return <div style={{ padding: 40, color: '#6b7a99' }}>Loading task…</div>;

  return (
    <div>
      <div className="page-header">
        <h1>Edit Task</h1>
        <p>Update task details or remove it permanently.</p>
      </div>

      <div className="card">
        {error && <div className="form-error">{error}</div>}
        <form className="form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Title *</label>
            <input
              type="text"
              value={form.title}
              onChange={set('title')}
              autoFocus
            />
          </div>
          <div className="form-group">
            <label>Description</label>
            <textarea
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
              {saving ? 'Saving…' : 'Save Changes'}
            </button>
            <Link to="/tasks" className="btn btn-secondary">Cancel</Link>
            <button type="button" className="btn btn-danger" onClick={handleDelete}>
              Delete Task
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
