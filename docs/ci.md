# GitHub Actions — build, ACR, GitOps

Workflow: [`.github/workflows/build-and-release.yml`](../.github/workflows/build-and-release.yml)

## What it does

| Event | Test | Build/push ACR | Bump GitOps `image.tag` |
| --- | --- | --- | --- |
| Pull request to `main` | yes | no | no |
| Push to `main`, tags `v*`, or workflow_dispatch | yes | yes | yes |

Image tags: 7-character git SHA (GitHub short SHA), full SHA, and the git tag name when the run is a `v*` tag.

The GitOps write updates [`apps/prod/email-consumer-service/values.yaml`](https://github.com/brandon-parker-code/email-consumer-service-gitops/blob/main/apps/prod/email-consumer-service/values.yaml) (`image.repository` and `image.tag`). Flux (next phase) reconciles that file.

## One-time GitHub setup

On **email-consumer-service**:

1. Create Environment **`prod`** (Settings → Environments). The Azure federated credential subject is `repo:brandon-parker-code/email-consumer-service:environment:prod`.
2. Environment **`prod`** (or repository) **variables** — names must match exactly; do not prefix with `GITHUB_`:
   - `AZURE_CLIENT_ID` — Terraform output `AZURE_CLIENT_ID`
   - `AZURE_TENANT_ID` — Terraform output `AZURE_TENANT_ID`
   - `AZURE_SUBSCRIPTION_ID` — Terraform output `AZURE_SUBSCRIPTION_ID`
   - `ACR_LOGIN_SERVER` — Terraform output `ACR_LOGIN_SERVER` (example: `acrecsprodxxxx.azurecr.io`)
3. Environment **`prod`** (or repository) **secret**:
   - `GITOPS_TOKEN` — fine-grained PAT with **Contents: Read and write** on `email-consumer-service-gitops`. Must be a **secret**, not a variable.

`terraform apply` must succeed before the first image push. Until then, tests still run on PRs.
