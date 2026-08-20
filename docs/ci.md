# GitHub Actions — build, ACR, GitOps

Workflows:

- [`.github/workflows/build-and-release.yml`](../.github/workflows/build-and-release.yml) — **Build** (tests and ACR)
- [`.github/workflows/deploy.yml`](../.github/workflows/deploy.yml) — **Deploy** (GitOps / AKS only)

## What it does

| Event | Test | Build/push ACR | Bump GitOps `image.tag` (Flux deploys) |
| --- | --- | --- | --- |
| Push to a feature branch | yes | no | no |
| Pull request | yes | no | no |
| Push to `main` | yes | yes | no |
| **Deploy → Run workflow** | no | no | yes (existing ACR image only) |
| Push a `v*` tag | yes | yes | yes |

A branch with an open PR may run tests twice (push + `pull_request`). That is expected.

Image tags: 7-character git SHA (GitHub short SHA), full SHA, and the git tag name when the run is a `v*` tag.

AKS only changes after GitOps `image.tag` is updated. Flux then pulls that image automatically. A commit to `main` builds and pushes to ACR; it does **not** deploy.

To deploy: GitHub → **email-consumer-service** → Actions → **Deploy** (not Build) → **Run workflow** → choose `main` (the commit whose image is already in ACR) → Run. That job does **not** rebuild. It checks that `email-consumer-service:<7-char-sha>` exists in ACR, then updates GitOps. If the image is missing, the job fails.

The GitOps write updates [`apps/prod/email-consumer-service/values.yaml`](https://github.com/brandon-parker-code/email-consumer-service-gitops/blob/main/apps/prod/email-consumer-service/values.yaml) (`image.repository` and `image.tag`).

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

`terraform apply` in [platform-terraform](https://github.com/brandon-parker-code/platform-terraform) must succeed before the first image push. Until then, tests still run on PRs and feature branches.
