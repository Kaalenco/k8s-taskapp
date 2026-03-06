# Guidelines for Multiple Targeted Deployments

## Problem

The current workflow rebuilds and redeploys all three images on every push to `trunk`,
even when only one component changed. This wastes build minutes and introduces unnecessary
risk — a frontend change should never cause a backend redeploy.

---

## Repository Component Map

| Component | Source paths | Image built | Affects |
|-----------|-------------|-------------|---------|
| Shared data layer | `src/TaskApp.Api.Data/**` | — | Backend + Migrations |
| Migration runner | `src/TaskApp.Db/**` | `taskapp-init` | Migrations Job |
| Backend API | `src/backend/**` | `taskapp-backend` | Backend Deployment |
| Frontend | `src/frontend-src/**` | `taskapp-frontend` | Frontend Deployment |
| Helm chart | `src/charts/taskapp-chart/**` | — | All (re-deploy only) |

**Key dependency:** `TaskApp.Api.Data` is shared. Any change there must trigger
both the backend and the migration runner image rebuilds.

---

## Recommended Strategy: Single Workflow with Change Detection

Keep one workflow file but add a `detect-changes` job at the start. Each subsequent
job checks the detection output before running. This preserves job coordination
(correct `needs` ordering, shared secrets, single deploy step) while skipping
unnecessary work.

### Step 1 — Detect what changed

Use the `dorny/paths-filter` action:

```yaml
jobs:
  detect-changes:
    runs-on: ubuntu-latest
    outputs:
      migrations: ${{ steps.filter.outputs.migrations }}
      backend:    ${{ steps.filter.outputs.backend }}
      frontend:   ${{ steps.filter.outputs.frontend }}
      chart:      ${{ steps.filter.outputs.chart }}
    steps:
    - uses: actions/checkout@v4
    - uses: dorny/paths-filter@v3
      id: filter
      with:
        filters: |
          migrations:
            - 'src/TaskApp.Api.Data/**'
            - 'src/TaskApp.Db/**'
          backend:
            - 'src/TaskApp.Api.Data/**'
            - 'src/backend/**'
          frontend:
            - 'src/frontend-src/**'
          chart:
            - 'src/charts/taskapp-chart/**'
```

Note that both `migrations` and `backend` include `src/TaskApp.Api.Data/**` because
the shared library affects both images.

### Step 2 — Conditionally run build jobs

Add an `if` condition to each build job:

```yaml
  build-backend:
    needs: detect-changes
    if: needs.detect-changes.outputs.backend == 'true'
    ...

  build-backend-init:
    needs: detect-changes
    if: needs.detect-changes.outputs.migrations == 'true'
    ...

  build-frontend:
    needs: detect-changes
    if: needs.detect-changes.outputs.frontend == 'true'
    ...
```

### Step 3 — Handle the deploy job

The deploy job must run when **any** component changed:

```yaml
  deploy:
    needs: [detect-changes, build-backend, build-backend-init, build-frontend]
    if: |
      always() &&
      !failure() && !cancelled() &&
      (
        needs.detect-changes.outputs.backend    == 'true' ||
        needs.detect-changes.outputs.migrations == 'true' ||
        needs.detect-changes.outputs.frontend   == 'true' ||
        needs.detect-changes.outputs.chart      == 'true'
      )
```

The `always()` is required because skipped build jobs would otherwise cause `deploy`
to be skipped too. The `!failure() && !cancelled()` guard ensures a failed build still
blocks the deploy.

### Step 4 — Resolve image tags in the deploy step

When a build job is skipped, its `outputs.image-version` is empty. The deploy step
must fall back to the last published tag (e.g. `trunk`) for unchanged components:

```yaml
    - name: Deploy with Helm
      run: |
        MIGRATIONS_TAG="${{ needs.build-backend-init.outputs.image-version }}"
        BACKEND_TAG="${{ needs.build-backend.outputs.image-version }}"
        FRONTEND_TAG="${{ needs.build-frontend.outputs.image-version }}"

        helm upgrade taskapp ${{ env.HELM_CHART_PATH }} \
          --install \
          --namespace ${{ env.KUBERNETES_NAMESPACE }} \
          --set migrations.image="${{ env.DOCKER_REGISTRY }}/${{ env.BACKEND_INIT_NAME }}:${MIGRATIONS_TAG:-trunk}" \
          --set backend.image="${{ env.DOCKER_REGISTRY }}/${{ env.BACKEND_IMAGE_NAME }}:${BACKEND_TAG:-trunk}" \
          --set frontend.image.repository="${{ env.DOCKER_REGISTRY }}/${{ env.FRONTEND_IMAGE_NAME }}" \
          --set frontend.image.tag="${FRONTEND_TAG:-trunk}" \
          --wait \
          --timeout 5m
```

The `${VAR:-trunk}` syntax substitutes `trunk` when the variable is empty (i.e. the
build was skipped). Replace `trunk` with whatever your stable fallback tag is.

---

## Deployment Order Constraints

These constraints must be respected regardless of which components changed:

```
MySQL ready
    └── Migrations Job (init container waits for MySQL)
            └── Backend (depends on schema existing)
                    └── Frontend (independent, but needs backend for full function)
```

The init container in the migrations Job already handles the MySQL readiness wait
at runtime. The backend's readiness probe (`/ready`) handles the schema dependency —
the backend will not become Ready until the database is accessible.

---

## Helm Chart-Only Changes

When only `src/charts/taskapp-chart/**` changes (e.g. resource limits, ingress config),
no images need rebuilding. The deploy job runs with all three build jobs skipped,
using the fallback `trunk` tags. The `chart` output from `detect-changes` ensures
the deploy job still triggers.

---

## Workflow Dispatch (Manual Trigger)

For `workflow_dispatch`, all change detection outputs will be `false` (no diff).
Override this by forcing all components to deploy on manual trigger:

```yaml
  detect-changes:
    outputs:
      migrations: ${{ steps.filter.outputs.migrations || github.event_name == 'workflow_dispatch' }}
      backend:    ${{ steps.filter.outputs.backend    || github.event_name == 'workflow_dispatch' }}
      frontend:   ${{ steps.filter.outputs.frontend   || github.event_name == 'workflow_dispatch' }}
      chart:      ${{ steps.filter.outputs.chart      || github.event_name == 'workflow_dispatch' }}
```

---

## Alternative: Multiple Workflow Files

If the coordination complexity above is undesirable, the workflow can be split into
separate files per component. This is simpler but loses cross-component coordination:

| File | Trigger paths |
|------|--------------|
| `deploy-migrations.yml` | `src/TaskApp.Api.Data/**`, `src/TaskApp.Db/**` |
| `deploy-backend.yml` | `src/TaskApp.Api.Data/**`, `src/backend/**` |
| `deploy-frontend.yml` | `src/frontend-src/**` |
| `deploy-chart.yml` | `src/charts/taskapp-chart/**` |

Each file uses `on.push.paths` to filter its trigger. The main drawback is that a
change to `src/TaskApp.Api.Data/` fires two independent workflows simultaneously
(migrations + backend), with no guarantee of ordering between them. The single-workflow
approach with `needs` ordering is strongly preferred for production use.
