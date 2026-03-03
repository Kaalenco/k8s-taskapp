import React from 'react';
import { Routes, Route, NavLink } from 'react-router-dom';
import HomePage from './pages/HomePage';
import StatusPage from './pages/StatusPage';
import TasksPage from './pages/TasksPage';
import AddTaskPage from './pages/AddTaskPage';
import EditTaskPage from './pages/EditTaskPage';

function App() {
  return (
    <div className="layout">
      <nav className="sidebar">
        <div className="sidebar-header">
          <span className="sidebar-logo">&#9632;</span>
          <span className="sidebar-title">TaskApp</span>
        </div>
        <ul className="nav-links">
          <li><NavLink to="/" end>Home</NavLink></li>
          <li><NavLink to="/status">Status</NavLink></li>
          <li><NavLink to="/tasks">Tasks</NavLink></li>
          <li><NavLink to="/tasks/new">Add Task</NavLink></li>
        </ul>
        <div className="sidebar-footer">Kubernetes 101 Course</div>
      </nav>
      <main className="content">
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/status" element={<StatusPage />} />
          <Route path="/tasks" element={<TasksPage />} />
          <Route path="/tasks/new" element={<AddTaskPage />} />
          <Route path="/tasks/:id/edit" element={<EditTaskPage />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;
