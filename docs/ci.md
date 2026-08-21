# GitHub Actions — build, ACR, GitOps

Workflows:

- [`.github/workflows/build-and-release.yml`](../.github/workflows/build-and-release.yml) — **Build** (tests and shared ACR)
- [`.github/workflows/deploy.yml`](../.github/workflows/deploy.yml) — **Deploy** (GitOps / AKS only)

## What it does

| Event | Test | Build/push ACR | GitHub Environment | Bump GitOps `image.tag` |
| --- | --- | --- | --- | --- |
| Push to a feature branch | yes | no | — | no |
| Pull request | yes | no | — | no |
| Push to `main` | yes | shared ACR | `prod` | no |
| **Deploy → Run workflow** (choose env) | no | no | selected | yes (`apps/<env>/…/values.yaml`) |
| Push a `v*` tag | yes | shared ACR | `prod` | yes (`apps/prod`) |

A branch with an open PR may run tests twice (push + `pull_request`). That is expected.

Image tags: 7-character git SHA (GitHub short SHA), full SHA, and the git tag name when the run is a `v*` tag.

There is **one ACR**. A commit to `main` builds once. **Deploy** to `dev` or `prod` only changes which GitOps overlay Flux rolls. Use the `main` branch (or the commit that was built) for both.

To deploy:

1. GitHub → **email-consumer-service** → Actions → **Deploy** (not Build) → **Run workflow**
2. Branch: `main` (the commit whose image is already in ACR)
3. Environment input: `dev` or `prod`

That job does **not** rebuild. It checks that `email-consumer-service:<7-char-sha>` exists in the shared ACR, then updates GitOps.

The GitOps write updates [`apps/<env>/email-consumer-service/values.yaml`](https://github.com/brandon-parker-code/email-consumer-service-gitops) (`image.repository` and `image.tag`). Both overlays use the same `image.repository`.

## One-time GitHub setup

On **email-consumer-service**, create Environments **`prod`** and **`dev`**.

Federated credential subjects on the **shared** GHA identity (must match Terraform):

- `repo:brandon-parker-code/email-consumer-service:environment:prod`
- `repo:brandon-parker-code/email-consumer-service:environment:dev`
- `repo:brandon-parker-code/email-consumer-service:ref:refs/heads/main`

(GitHub also includes owner/repo numeric ids in the real `sub` claim; Terraform already encodes those.)

Both Environments use the **same** Azure variables (prod Terraform outputs):

- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`
- `ACR_LOGIN_SERVER`

And **secret**:

- `GITOPS_TOKEN` — fine-grained PAT with **Contents: Read and write** on `email-consumer-service-gitops`. Can be repo-level instead of duplicated per Environment.

`terraform apply` for prod must succeed before the first image push. Dev cluster apply can happen later; Deploy to `dev` only needs the image in ACR and GitOps `apps/dev` filled in.
