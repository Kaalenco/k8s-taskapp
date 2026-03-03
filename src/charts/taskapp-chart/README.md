# TaskApp Helm Chart

A production-ready Helm chart for deploying a three-tier task management application.

## Components

- **MySQL Database** - Persistent data storage
- **Backend API** - RESTful API service (Node.js)
- **Frontend Web App** - Nginx serving static content
- **Ingress** - HTTPS external access with TLS

## Prerequisites

- Kubernetes 1.20+
- Helm 3.0+
- Ingress controller installed (nginx, traefik, etc.)
- TLS certificate (for HTTPS)

## Installation

### Quick Start

```bash
# Create namespace
kubectl create namespace taskapp

# Create TLS certificate (self-signed for dev)
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout tls.key -out tls.crt \
  -subj "/CN=taskapp.local/O=TaskApp"

kubectl create secret tls taskapp-tls \
  --cert=tls.crt --key=tls.key \
  -n taskapp

# Install chart
helm install my-taskapp ./taskapp-chart

# Check status
kubectl get pods -n taskapp -w
```

### Custom Configuration

```bash
# Create custom values file
cat > my-values.yaml <<EOF
backend:
  replicaCount: 3

database:
  persistence:
    size: 5Gi

ingress:
  hosts:
    - host: taskapp.example.com
EOF

# Install with custom values
helm install my-taskapp ./taskapp-chart -f my-values.yaml
```

### Using External Database

```bash
# Disable internal MySQL, use Azure SQL instead
helm install my-taskapp ./taskapp-chart \
  --set database.enabled=false \
  --set backend.config.dbHost=myserver.database.windows.net \
  --set backend.config.dbPort=1433
```

## Configuration

See [values.yaml](values.yaml) for all configuration options.

### Key Configuration Options

| Parameter | Description | Default |
|-----------|-------------|---------|
| `global.namespace` | Kubernetes namespace | `taskapp` |
| `database.enabled` | Deploy MySQL database | `true` |
| `database.persistence.size` | Database storage size | `1Gi` |
| `backend.replicaCount` | Backend pod replicas | `2` |
| `backend.autoscaling.enabled` | Enable HPA for backend | `true` |
| `backend.autoscaling.maxReplicas` | Max backend replicas | `10` |
| `frontend.replicaCount` | Frontend pod replicas | `2` |
| `ingress.enabled` | Enable Ingress | `true` |
| `ingress.hosts[0].host` | Application hostname | `taskapp.local` |
| `ingress.tls.enabled` | Enable TLS/HTTPS | `true` |

## Upgrading

```bash
# Update values
helm upgrade my-taskapp ./taskapp-chart -f my-values.yaml

# Or update specific value
helm upgrade my-taskapp ./taskapp-chart --set backend.replicaCount=5
```

## Rollback

```bash
# View release history
helm history my-taskapp

# Rollback to previous version
helm rollback my-taskapp

# Rollback to specific revision
helm rollback my-taskapp 2
```

## Uninstallation

```bash
# Delete release
helm uninstall my-taskapp

# Delete namespace (if desired)
kubectl delete namespace taskapp
```

## Environment-Specific Deployments

### Development

```yaml
# values-dev.yaml
database:
  persistence:
    size: 500Mi
backend:
  replicaCount: 1
  autoscaling:
    enabled: false
frontend:
  replicaCount: 1
ingress:
  hosts:
    - host: taskapp-dev.local
```

```bash
helm install taskapp-dev ./taskapp-chart -f values-dev.yaml
```

### Production

```yaml
# values-prod.yaml
database:
  persistence:
    size: 20Gi
  resources:
    requests:
      memory: "512Mi"
      cpu: "500m"
    limits:
      memory: "1Gi"
      cpu: "1000m"
backend:
  replicaCount: 5
  autoscaling:
    enabled: true
    maxReplicas: 20
frontend:
  replicaCount: 3
ingress:
  hosts:
    - host: taskapp.example.com
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-prod
```

```bash
helm install taskapp-prod ./taskapp-chart -f values-prod.yaml
```

## Troubleshooting

### Pods Not Starting

```bash
# Check pod status
kubectl get pods -n taskapp

# View pod logs
kubectl logs -n taskapp -l app.kubernetes.io/name=taskapp

# Describe pod for events
kubectl describe pod -n taskapp <pod-name>
```

### Ingress Not Working

```bash
# Check ingress status
kubectl get ingress -n taskapp

# Verify ingress controller is running
kubectl get pods -n ingress-nginx

# Check ingress controller logs
kubectl logs -n ingress-nginx deployment/ingress-nginx-controller
```

### Database Connection Issues

```bash
# Test database connectivity
kubectl run test --image=mysql:8.0 --rm -it --restart=Never -- \
  mysql -h my-taskapp-mysql.taskapp.svc.cluster.local -u taskuser -p

# Check database logs
kubectl logs -n taskapp -l app.kubernetes.io/component=database
```

## Chart Development

### Linting

```bash
helm lint ./taskapp-chart
```

### Dry Run

```bash
helm install test-release ./taskapp-chart --dry-run --debug
```

### Template Rendering

```bash
helm template test-release ./taskapp-chart
```

### Packaging

```bash
# Package chart
helm package ./taskapp-chart

# Creates: taskapp-1.0.0.tgz
```

## License

MIT

## Maintainer

Kubernetes 101 Course
