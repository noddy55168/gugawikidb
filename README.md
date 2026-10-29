# Guga database

Public, read-only static edition of a practice wiki website built with Vue.
The development project also has a C# backend; this repository contains the generated static website only.

## First publication

1. Review and commit the prepared files to `main`, then push to GitHub.
2. In **Settings → Pages → Build and deployment**, set **Source** to **GitHub Actions**.
3. Open **Actions → Publish Guga database → Run workflow**, select `main`, and run it.
4. Wait for both `package` and `deploy` to pass. Open the URL shown by the deployment.

Expected project URL: https://noddy55168.github.io/gugawikidb/
Example detail: https://noddy55168.github.io/gugawikidb/#/operators/4
These URLs become available only after a successful publication.

## Manual updates

Pushing commits alone does **not** publish the website. Only `workflow_dispatch` triggers publication.
The workflow checks the file list and SHA256 values, then uploads **only `site/`**.
It does not build Vue or connect to a database. No personal access token is required by the workflow.

Generate and verify a complete new package in the development project with base `/gugawikidb/`.
Replace the previous `site/` contents as one reviewed release, including removal of obsolete files;
do not mix new JavaScript with an old data manifest. Commit, push, and run the workflow manually.
Keep `.gitattributes`: generated files must retain their exact bytes for hash verification.

Before publication, the package can also be checked locally with `python tools/verify-release.py`.
`site/release.json` records the data version, base, build timestamp, and file hashes.
For rollback, restore a complete previously verified `site/` package in a new commit and publish it manually.

## Content boundary

This is a reading snapshot: accounts, editing, and administration are unavailable.
Database files, credentials, research exports, and private development documents do not belong in this repository.
Game names, artwork, and other game assets belong to their respective rights holders.
