#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Validate HelloAGENTS knowledge base (KB) structure and doc hygiene.

Scope: knowledge base only (helloagents/)
- Core structure existence
- Empty files detection
- Markdown relative link checks
- Lightweight sensitive info scan

Usage:
  python -X utf8 validate_kb.py [--path <project-root>]

Output:
  JSON report with issues grouped by severity.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, Iterable, List, Optional, Set, Tuple

sys.path.insert(0, str(Path(__file__).parent))
from utils import setup_encoding, script_error_handler, validate_base_path, get_workspace_path


SEVERITY_CRITICAL = "Critical"
SEVERITY_WARNING = "Warning"
SEVERITY_INFO = "Info"


@dataclass
class Finding:
    severity: str
    code: str
    path: str
    message: str
    detail: Optional[dict] = None

    def to_dict(self) -> dict:
        d = {
            "severity": self.severity,
            "code": self.code,
            "path": self.path,
            "message": self.message,
        }
        if self.detail is not None:
            d["detail"] = self.detail
        return d


def _read_text(path: Path) -> str:
    try:
        return path.read_text(encoding="utf-8")
    except Exception:
        return ""


def _is_effectively_empty(text: str) -> bool:
    return len(text.strip()) == 0


def _posix_relpath(path: Path, base: Path) -> str:
    try:
        return str(path.relative_to(base)).replace("\\", "/")
    except Exception:
        return str(path).replace("\\", "/")


_LINK_RE = re.compile(r"!?\[[^\]]*\]\(([^)]+)\)")


def _normalize_link_target(raw: str) -> Optional[str]:
    target = raw.strip().strip("\"").strip("'")
    if not target:
        return None

    # Ignore external / anchors.
    lower = target.lower()
    if lower.startswith("http://") or lower.startswith("https://"):
        return None
    if lower.startswith("mailto:") or lower.startswith("tel:"):
        return None
    if target.startswith("#"):
        return None

    # If template placeholders exist, we treat as non-actionable.
    if "{" in target or "}" in target:
        return "__TEMPLATE_PLACEHOLDER__"

    # Strip query/fragment.
    target = target.split("#", 1)[0]
    target = target.split("?", 1)[0]
    target = target.strip()
    if not target:
        return None

    # Ignore absolute paths (Windows or Unix).
    if re.match(r"^[a-zA-Z]:\\", target) or target.startswith("/"):
        return None

    return target


def _iter_markdown_files(root: Path) -> Iterable[Path]:
    for p in root.rglob("*.md"):
        # Skip dot folders.
        if any(part.startswith(".") for part in p.parts):
            continue
        yield p


_SENSITIVE_PATTERNS: List[Tuple[str, re.Pattern]] = [
    ("private_key_block", re.compile(r"BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY", re.IGNORECASE)),
    ("api_key_label", re.compile(r"\b(api[_-]?key|apikey)\b\s*[:=]", re.IGNORECASE)),
    ("token_label", re.compile(r"\b(token|access[_-]?token|refresh[_-]?token)\b\s*[:=]", re.IGNORECASE)),
    ("secret_label", re.compile(r"\b(secret|client[_-]?secret)\b\s*[:=]", re.IGNORECASE)),
    ("password_label", re.compile(r"\b(password|passwd|pwd)\b\s*[:=]", re.IGNORECASE)),
]


def validate_kb(project_root: Path) -> dict:
    kb_root = get_workspace_path(str(project_root))

    findings: List[Finding] = []

    # Structure checks
    required_dirs = ["modules", "archive", "plan"]
    required_root_files = ["INDEX.md", "context.md", "CHANGELOG.md"]

    if not kb_root.exists():
        findings.append(
            Finding(
                severity=SEVERITY_CRITICAL,
                code="kb_missing",
                path=_posix_relpath(kb_root, project_root),
                message="知识库目录不存在（helloagents/）",
            )
        )
        return _finalize_report(project_root, kb_root, findings)

    for d in required_dirs:
        p = kb_root / d
        if not p.exists() or not p.is_dir():
            findings.append(
                Finding(
                    severity=SEVERITY_CRITICAL,
                    code="missing_dir",
                    path=_posix_relpath(p, project_root),
                    message=f"缺少必需目录: {d}/",
                )
            )

    for f in required_root_files:
        p = kb_root / f
        if not p.exists() or not p.is_file():
            findings.append(
                Finding(
                    severity=SEVERITY_CRITICAL,
                    code="missing_file",
                    path=_posix_relpath(p, project_root),
                    message=f"缺少必需文件: {f}",
                )
            )
        else:
            txt = _read_text(p)
            if _is_effectively_empty(txt):
                findings.append(
                    Finding(
                        severity=SEVERITY_CRITICAL,
                        code="empty_file",
                        path=_posix_relpath(p, project_root),
                        message=f"核心文件为空: {f}",
                    )
                )

    # Non-empty checks for key index files
    for p in [kb_root / "modules" / "_index.md", kb_root / "archive" / "_index.md"]:
        if not p.exists():
            findings.append(
                Finding(
                    severity=SEVERITY_WARNING,
                    code="missing_index",
                    path=_posix_relpath(p, project_root),
                    message="建议存在的索引文件缺失",
                )
            )
        else:
            txt = _read_text(p)
            if _is_effectively_empty(txt):
                findings.append(
                    Finding(
                        severity=SEVERITY_WARNING,
                        code="empty_index",
                        path=_posix_relpath(p, project_root),
                        message="索引文件为空",
                    )
                )

    # Empty file scan (all md)
    for md in _iter_markdown_files(kb_root):
        txt = _read_text(md)
        if _is_effectively_empty(txt):
            findings.append(
                Finding(
                    severity=SEVERITY_WARNING,
                    code="empty_md",
                    path=_posix_relpath(md, project_root),
                    message="Markdown 文件为空",
                )
            )

    # Link checks (relative)
    dead_links: List[dict] = []
    placeholder_links: List[dict] = []

    for md in _iter_markdown_files(kb_root):
        txt = _read_text(md)
        if not txt:
            continue

        for m in _LINK_RE.finditer(txt):
            raw_target = m.group(1)
            target = _normalize_link_target(raw_target)
            if target is None:
                continue
            if target == "__TEMPLATE_PLACEHOLDER__":
                placeholder_links.append(
                    {
                        "from": _posix_relpath(md, project_root),
                        "to": raw_target,
                    }
                )
                continue

            from_dir = md.parent
            to_path = (from_dir / target).resolve()

            # Prevent resolving outside the KB root (but do not fail hard).
            try:
                to_path.relative_to(kb_root.resolve())
            except Exception:
                findings.append(
                    Finding(
                        severity=SEVERITY_WARNING,
                        code="link_outside_kb",
                        path=_posix_relpath(md, project_root),
                        message="相对链接指向知识库目录之外（建议避免）",
                        detail={"to": raw_target},
                    )
                )
                continue

            if not to_path.exists():
                dead_links.append(
                    {
                        "from": _posix_relpath(md, project_root),
                        "to": raw_target,
                    }
                )

    if dead_links:
        findings.append(
            Finding(
                severity=SEVERITY_WARNING,
                code="dead_links",
                path=_posix_relpath(kb_root, project_root),
                message=f"发现 {len(dead_links)} 个相对链接不存在",
                detail={"links": dead_links[:100]},
            )
        )

    if placeholder_links:
        findings.append(
            Finding(
                severity=SEVERITY_INFO,
                code="template_links",
                path=_posix_relpath(kb_root, project_root),
                message=f"检测到 {len(placeholder_links)} 个模板占位符链接（可忽略）",
                detail={"links": placeholder_links[:50]},
            )
        )

    # Placeholder markers
    placeholder_hits = 0
    for md in _iter_markdown_files(kb_root):
        txt = _read_text(md)
        if "(待补充)" in txt:
            placeholder_hits += txt.count("(待补充)")

    if placeholder_hits:
        findings.append(
            Finding(
                severity=SEVERITY_INFO,
                code="placeholders",
                path=_posix_relpath(kb_root, project_root),
                message=f"检测到 (待补充) 占位符 {placeholder_hits} 处",
            )
        )

    # Sensitive info scan (lightweight)
    for md in _iter_markdown_files(kb_root):
        txt = _read_text(md)
        if not txt:
            continue

        for name, pat in _SENSITIVE_PATTERNS:
            if pat.search(txt):
                findings.append(
                    Finding(
                        severity=SEVERITY_WARNING,
                        code=f"sensitive_{name}",
                        path=_posix_relpath(md, project_root),
                        message="疑似包含敏感信息标记（请人工确认是否为占位或示例）",
                    )
                )
                break

    return _finalize_report(project_root, kb_root, findings)


def _finalize_report(project_root: Path, kb_root: Path, findings: List[Finding]) -> dict:
    by_sev: Dict[str, int] = {SEVERITY_CRITICAL: 0, SEVERITY_WARNING: 0, SEVERITY_INFO: 0}
    for f in findings:
        by_sev[f.severity] = by_sev.get(f.severity, 0) + 1

    ok = by_sev.get(SEVERITY_CRITICAL, 0) == 0
    return {
        "ok": ok,
        "project_root": str(project_root),
        "kb_root": str(kb_root),
        "summary": {
            "critical": by_sev.get(SEVERITY_CRITICAL, 0),
            "warning": by_sev.get(SEVERITY_WARNING, 0),
            "info": by_sev.get(SEVERITY_INFO, 0),
        },
        "findings": [f.to_dict() for f in findings],
    }


@script_error_handler
def main() -> None:
    setup_encoding()

    parser = argparse.ArgumentParser(description="Validate HelloAGENTS knowledge base")
    parser.add_argument("--path", default=None, help="Project root (default: cwd)")
    args = parser.parse_args()

    project_root = validate_base_path(args.path)
    report = validate_kb(project_root)

    print(json.dumps(report, ensure_ascii=False, indent=2))
    sys.exit(0 if report.get("ok") else 1)


if __name__ == "__main__":
    main()
