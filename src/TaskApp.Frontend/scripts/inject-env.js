#!/usr/bin/env node
/**
 * Injects a window.env configuration block into build/index.html.
 * Usage: node scripts/inject-env.js <API_URL>
 * Example: node scripts/inject-env.js http://localhost:5000
 */

const fs = require('fs');
const path = require('path');

const apiUrl = process.argv[2] || '';
const indexPath = path.join(__dirname, '..', 'build', 'index.html');

if (!fs.existsSync(indexPath)) {
  console.error('build/index.html not found. Run the build first.');
  process.exit(1);
}

const script = `<script>window.env = { "API_URL": "${apiUrl}" };</script>`;
let html = fs.readFileSync(indexPath, 'utf8');

if (html.includes('window.env')) {
  // Replace existing injection so this script is idempotent
  html = html.replace(/<script>window\.env\s*=\s*\{[^<]*\};<\/script>/, script);
} else {
  html = html.replace('</head>', `${script}\n  </head>`);
}

fs.writeFileSync(indexPath, html, 'utf8');
console.log(`Injected window.env.API_URL = "${apiUrl}" into build/index.html`);
