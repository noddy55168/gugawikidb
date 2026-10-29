"""發布前確認 site 與本機生成清單一致；不讀取 DB、不重建或修改網站。"""
from pathlib import Path
import hashlib
import json
import re


def verify(root):
    """限制公開檔案種類、版本、路徑及大小，任何缺漏或雜湊不符即停止發布。"""
    if root.is_symlink():
        raise ValueError('site must not be a link')
    entries = list(root.rglob('*'))
    if any(p.is_symlink() for p in entries):
        raise ValueError('Links are not allowed')
    release = json.loads((root / 'release.json').read_text(encoding='utf-8'))
    if release['schemaVersion'] != 'endfield.pages.v1' or release['base'] != '/gugawikidb/':
        raise ValueError('Wrong release schema or repository base')
    files = release['files']
    actual = {p.relative_to(root).as_posix() for p in entries if p.is_file()}
    if actual != set(files) | {'release.json'}:
        raise ValueError('Unexpected or missing files')
    allowed = re.compile(r'(?:index\.html|og\.png|\.nojekyll|assets/[A-Za-z0-9_-]+\.(?:js|css)|static-data/manifest\.json|static-data/data/[a-f0-9]{24}/[a-f0-9]{64}\.json|media/[A-Za-z0-9_./-]+\.png)')
    total = (root / 'release.json').stat().st_size
    for name, item in files.items():
        if not allowed.fullmatch(name) or any(p in ('', '.', '..') for p in name.split('/')):
            raise ValueError('Disallowed file: ' + name)
        path = root / name
        if path.stat().st_nlink != 1:
            raise ValueError('Hard links are not allowed: ' + name)
        content = path.read_bytes()
        if len(content) != item['bytes'] or hashlib.sha256(content).hexdigest() != item['sha256']:
            raise ValueError('Hash or size mismatch: ' + name)
        total += len(content)
    if total >= 1_000_000_000:
        raise ValueError('Website must remain below the local 1 GB guardrail')
    manifest = json.loads((root / 'static-data/manifest.json').read_text(encoding='utf-8'))
    if manifest['releaseId'] != release['dataReleaseId']:
        raise ValueError('Data release mismatch')
    print(f"Verified {len(actual)} files, {total} bytes, data {release['dataReleaseId']}")


if __name__ == '__main__':
    verify(Path(__file__).resolve().parents[1] / 'site')
