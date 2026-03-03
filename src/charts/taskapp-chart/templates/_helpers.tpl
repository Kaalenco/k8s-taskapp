{{/*
Helm Template Helpers
Session 16: Helm Package Manager

This file contains reusable template functions (helpers).

What are Template Helpers?
- Reusable functions that generate common values
- Avoid repeating the same logic in multiple templates
- Make templates cleaner and more maintainable
- Named with underscore prefix to indicate they're partials

Helper Functions Defined:
- taskapp.name: Chart name
- taskapp.fullname: Full resource name (release + chart)
- taskapp.chart: Chart name and version
- taskapp.labels: Common labels for all resources
- taskapp.selectorLabels: Labels for selecting pods

Helm Template Syntax:
{{ ... }}       - Action (execute function)
{{- ... }}      - Action with left whitespace trim
{{ ... -}}      - Action with right whitespace trim
{{/* ... */}}   - Comment (not rendered)
{{ define "name" }} ... {{ end }}  - Define template
{{ include "name" . }}  - Include template
{{ .Values.key }}  - Access value from values.yaml
{{ .Release.Name }}  - Access release name
{{ .Chart.Name }}  - Access chart name

Sprig Functions (built-in):
default, quote, upper, lower, trim, trunc, replace, etc.
See: http://masterminds.github.io/sprig/
*/}}

{{/*
Expand the name of the chart.
Returns: Chart name (e.g., "taskapp")
Usage: {{ include "taskapp.name" . }}
*/}}
{{- define "taskapp.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
Returns: Release name + chart name (e.g., "my-taskapp" or "taskapp")
Usage: {{ include "taskapp.fullname" . }}

This is used for resource names to ensure uniqueness.
If release name contains chart name, don't duplicate it.
Truncate to 63 chars (Kubernetes label limit).
*/}}
{{- define "taskapp.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
Returns: Chart name and version (e.g., "taskapp-1.0.0")
Usage: {{ include "taskapp.chart" . }}
*/}}
{{- define "taskapp.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels applied to all resources.
These labels help identify resources and enable querying.
Returns: Multi-line YAML labels
Usage:
  labels:
    {{- include "taskapp.labels" . | nindent 4 }}

Labels included:
- helm.sh/chart: Chart name and version
- app.kubernetes.io/name: Application name
- app.kubernetes.io/instance: Release name
- app.kubernetes.io/version: App version
- app.kubernetes.io/managed-by: Helm
*/}}
{{- define "taskapp.labels" -}}
helm.sh/chart: {{ include "taskapp.chart" . }}
{{ include "taskapp.selectorLabels" . -}}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels for matching pods to services/deployments.
These labels must be consistent across related resources.
Returns: Multi-line YAML labels
Usage:
  selector:
    matchLabels:
      {{- include "taskapp.selectorLabels" . | nindent 6 }}

Labels included:
- app.kubernetes.io/name: Application name
- app.kubernetes.io/instance: Release name
*/}}
{{- define "taskapp.selectorLabels" -}}
app.kubernetes.io/name: {{ include "taskapp.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Component-specific labels.
Adds a "component" label to identify tier (database, backend, frontend).
Returns: Multi-line YAML labels with component added
Usage:
  labels:
    {{- include "taskapp.componentLabels" (dict "component" "backend" "context" .) | nindent 4 }}

Parameters:
- component: Component name (database, backend, frontend)
- context: Root context (.)
*/}}
{{- define "taskapp.componentLabels" -}}
{{ include "taskapp.labels" .context }}
app.kubernetes.io/component: {{ .component }}
{{- end }}

{{/*
Component-specific selector labels.
Returns: Multi-line YAML labels with component added
Usage:
  selector:
    matchLabels:
      {{- include "taskapp.componentSelectorLabels" (dict "component" "backend" "context" .) | nindent 6 }}
*/}}
{{- define "taskapp.componentSelectorLabels" -}}
{{ include "taskapp.selectorLabels" .context }}
app.kubernetes.io/component: {{ .component }}
{{- end }}

{{/*
Database host helper.
Returns appropriate database host based on configuration.
If internal database enabled, returns service name.
If external database, returns configured host.
Usage: {{ include "taskapp.databaseHost" . }}
*/}}
{{- define "taskapp.databaseHost" -}}
{{- if .Values.database.enabled }}
{{- printf "%s-mysql" (include "taskapp.fullname" .) }}
{{- else }}
{{- required "database.externalHost is required when database.enabled is false" .Values.database.externalHost }}
{{- end }}
{{- end }}

{{/*
Service account name helper.
Returns the service account name to use.
If not specified, uses default service account.
Usage: {{ include "taskapp.serviceAccountName" . }}
*/}}
{{- define "taskapp.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "taskapp.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
Image pull secrets helper.
Generates imagePullSecrets list if specified.
Usage:
  {{- include "taskapp.imagePullSecrets" . | nindent 6 }}
*/}}
{{- define "taskapp.imagePullSecrets" -}}
{{- if .Values.imagePullSecrets }}
imagePullSecrets:
{{- range .Values.imagePullSecrets }}
  - name: {{ . }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Examples of Using Helpers:

1. In deployment.yaml:
   metadata:
     name: {{ include "taskapp.fullname" . }}-backend
     labels:
       {{- include "taskapp.componentLabels" (dict "component" "backend" "context" .) | nindent 4 }}

2. In service.yaml:
   metadata:
     name: {{ include "taskapp.fullname" . }}-backend
   spec:
     selector:
       {{- include "taskapp.componentSelectorLabels" (dict "component" "backend" "context" .) | nindent 6 }}

3. In configmap.yaml:
   data:
     DB_HOST: {{ include "taskapp.databaseHost" . | quote }}

Benefits of Helpers:
- DRY (Don't Repeat Yourself): Write once, use everywhere
- Consistency: Same logic everywhere
- Maintainability: Update in one place
- Readability: Templates are cleaner
- Testing: Easier to test individual functions
*/}}
