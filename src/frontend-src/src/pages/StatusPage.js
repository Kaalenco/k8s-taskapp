import React, { useState, useCallback } from 'react';

const API_URL = window.env?.API_URL || '';

export default function StatusPage() {
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(false);

  const ping = useCallback(() => {
    setLoading(true);
    setResult(null);
    const start = Date.now();
    fetch(`${API_URL}/api/tasks`)
      .then(async r => {
        const elapsed = Date.now() - start;
        let body = null;
        try { body = await r.json(); } catch (_) {}
        setResult({ ok: r.ok, status: r.status, elapsed, body });
      })
      .catch(err => {
        setResult({ ok: false, status: 'N/A', elapsed: Date.now() - start, error: err.message });
      })
      .finally(() => setLoading(false));
  }, []);

  // Run on first render
  React.useEffect(() => { ping(); }, [ping]);

  return (
    <div>
      <div className="page-header">
        <h1>Backend Status</h1>
        <p>Live connection check to the backend API service.</p>
      </div>

      <div className="card">
        <div className="status-grid">
          <div className="status-metric">
            <div className="label">API Endpoint</div>
            <div className="value small">{API_URL || '(same origin)'}/api/tasks</div>
          </div>
          <div className="status-metric">
            <div className="label">HTTP Status</div>
            <div className="value">{result ? result.status : '—'}</div>
          </div>
          <div className="status-metric">
            <div className="label">Response Time</div>
            <div className="value">{result ? `${result.elapsed} ms` : '—'}</div>
          </div>
          <div className="status-metric">
            <div className="label">Result</div>
            <div className="value">
              {result == null
                ? <span className="badge badge-todo">—</span>
                : result.ok
                  ? <span className="badge badge-ok">OK</span>
                  : <span className="badge badge-err">FAIL</span>}
            </div>
          </div>
        </div>

        {result?.body != null && (
          <>
            <div style={{ fontSize: 13, fontWeight: 600, color: '#6b7a99', marginBottom: 8 }}>
              Response body (first 10 items)
            </div>
            <pre className="json-block">
              {JSON.stringify(Array.isArray(result.body) ? result.body.slice(0, 10) : result.body, null, 2)}
            </pre>
          </>
        )}

        {result?.error && (
          <div className="form-error" style={{ marginTop: 12 }}>
            Error: {result.error}
          </div>
        )}

        <div style={{ marginTop: 20 }}>
          <button className="btn btn-primary" onClick={ping} disabled={loading}>
            {loading ? 'Checking…' : 'Refresh'}
          </button>
        </div>
      </div>
    </div>
  );
}
