import React, { useEffect, useState, useCallback } from 'react';
import { Link } from 'react-router-dom';

const API_URL = window.env?.API_URL || '';

function statusBadge(status) {
  const map = {
    'todo': 'badge-todo',
    'in-progress': 'badge-in-progress',
    'done': 'badge-done',
  };
  return <span className={`badge ${map[status] || 'badge-todo'}`}>{status || 'todo'}</span>;
}

export default function TasksPage() {
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const load = useCallback(() => {
    setLoading(true);
    setError(null);
    fetch(`${API_URL}/api/tasks`)
      .then(r => {
        if (!r.ok) throw new Error(`HTTP ${r.status}`);
        return r.json();
      })
      .then(data => setTasks(Array.isArray(data) ? data : []))
      .catch(e => setError(e.message))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => { load(); }, [load]);

  const handleDelete = (id) => {
    if (!window.confirm('Delete this task?')) return;
    fetch(`${API_URL}/api/tasks/${id}`, { method: 'DELETE' })
      .then(() => setTasks(prev => prev.filter(t => t.id !== id)))
      .catch(e => alert('Delete failed: ' + e.message));
  };

  return (
    <div>
      <div className="task-list-header">
        <div className="page-header" style={{ marginBottom: 0 }}>
          <h1>Tasks</h1>
          <p>All tasks from the backend database.</p>
        </div>
        <Link to="/tasks/new" className="btn btn-primary">+ New Task</Link>
      </div>

      {error && <div className="form-error">{error} — <button className="btn btn-sm btn-secondary" onClick={load}>Retry</button></div>}

      <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
        {loading ? (
          <div style={{ padding: 40, textAlign: 'center', color: '#6b7a99' }}>Loading…</div>
        ) : tasks.length === 0 ? (
          <div className="empty-state">
            <p>No tasks yet. Create your first one!</p>
            <Link to="/tasks/new" className="btn btn-primary">+ New Task</Link>
          </div>
        ) : (
          <table className="task-table">
            <thead>
              <tr>
                <th>Task</th>
                <th>Status</th>
                <th>Created</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {tasks.map(task => (
                <tr key={task.id}>
                  <td>
                    <div className="task-title">{task.title}</div>
                    {task.description && <div className="task-desc">{task.description}</div>}
                  </td>
                  <td>{statusBadge(task.status)}</td>
                  <td style={{ color: '#6b7a99', fontSize: 13, whiteSpace: 'nowrap' }}>
                    {task.createdAt ? new Date(task.createdAt).toLocaleDateString() : '—'}
                  </td>
                  <td>
                    <div className="task-actions">
                      <Link to={`/tasks/${task.id}/edit`} className="btn btn-sm btn-secondary">Edit</Link>
                      <button className="btn btn-sm btn-danger" onClick={() => handleDelete(task.id)}>Delete</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
