#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Generate a write plan for `upgradewiki.py --write`.

Goal:
- Upgrade `helloagents/` to the latest KB structure (INDEX.md/context.md/modules/archive)
- Preserve ALL existing information (no deletes)

This script only generates a JSON plan. File IO is executed by `upgradewiki.py`.

Usage:
  python -X utf8 generate_upgrade_plan.py --out plan.json [--path <project-root>]
"""

from __future__ import annotations

import argparse
import json
import re
from dataclasses import dataclass
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Optional, Tuple

# Reuse HelloAGENTS helper utilities (path resolution + templates dir)
from utils import (
    get_workspace_path,
    get_plan_path,
    get_archive_path,
    get_year_month,
    get_template_loader,
    validate_base_path,
)


@dataclass(frozen=True)
class HistoryEntry:
    timestamp: str
    feature: str
    kind: str
    status: str
    rel_path: str  # e.g. helloagents/history/2026-01/202601090851_xxx/


def _read_text_or_empty(path: Path) -> str:
    if not path.exists() or not path.is_file():
        return ""
    return path.read_text(encoding="utf-8")


def _finalize_doc(md: str) -> str:
    """Normalize placeholder tokens so the upgraded KB contains no template residue."""
    # Normalize template placeholders to a stable literal.
    # User asked to "fill all"; when info is unknown, we use "无" rather than leaving placeholders.
    md = md.replace("(待补充)", "无")
    md = re.sub(r"\{[^}]+\}", "无", md)

    # Avoid repeating excessive spaces.
    md = re.sub(r"[ \t]+$", "", md, flags=re.MULTILINE)

    if not md.endswith("\n"):
        md += "\n"
    return md


def _strip_first_heading(md: str) -> str:
    # Remove the first H1 heading line to embed in other docs.
    lines = md.splitlines()
    if lines and lines[0].lstrip().startswith("#"):
        # Drop the first heading line and following blank line if present.
        lines = lines[1:]
        if lines and not lines[0].strip():
            lines = lines[1:]
    return "\n".join(lines).strip() + ("\n" if md.endswith("\n") else "")


def _normalize_tasks_statuses(md: str) -> str:
    # Convert common GitHub checkbox variants to HelloAGENTS validator-friendly ones.
    # - [x] => - [√]
    # - [X] => - [X]
    md = re.sub(r"\[(x)\]", "[√]", md)
    md = re.sub(r"\[( )\]", "[ ]", md)
    return md


def _parse_history_index(history_index_md: str) -> List[HistoryEntry]:
    # Expected table header in Chinese; be lenient and parse any markdown table rows.
    entries: List[HistoryEntry] = []
    lines = history_index_md.splitlines()

    in_table = False
    for line in lines:
        if line.strip().startswith("|"):
            # Detect separator row
            if re.match(r"^\|\s*-{2,}", line.strip()):
                in_table = True
                continue
            if not in_table:
                continue

            cols = [c.strip() for c in line.strip().strip("|").split("|")]
            if len(cols) < 5:
                continue

            timestamp = cols[0]
            feature = cols[1]
            kind = cols[2]
            status = cols[3]
            path_cell = cols[4]

            # Extract path inside backticks
            m = re.search(r"`([^`]+)`", path_cell)
            rel_path = m.group(1) if m else path_cell.strip()
            rel_path = rel_path.strip().rstrip("/")

            if not re.fullmatch(r"\d{12}", timestamp):
                continue
            if not feature:
                continue

            entries.append(
                HistoryEntry(
                    timestamp=timestamp,
                    feature=feature,
                    kind=kind or "未知",
                    status=status or "未知",
                    rel_path=rel_path,
                )
            )

    return entries


def _guess_pkg_type(kind: str) -> str:
    # Keep it simple: docs-only entries are overview; most are implementation.
    if "文档" in kind:
        return "overview"
    return "implementation"


def _infer_modules_for_entry(feature: str, kind: str) -> str:
    """Infer affected modules for archive index based on feature naming."""
    f = (feature or "").lower()

    # Specific keywords
    if "networkplayer" in f:
        return "networkplayer"
    if "trade" in f or "tradepanel" in f:
        return "networkplugin"
    if "resurrect" in f:
        return "networkplugin"
    if "event" in f or "dialog" in f:
        return "networkplugin"
    if "turn" in f:
        return "networkplugin"
    if "midgame" in f or "join" in f:
        return "networkplugin"
    if "networkmanager" in f:
        return "networkplugin"
    if "nat" in f or "upnp" in f or "stun" in f:
        return "networkplugin"
    if "server" in f or "relay" in f:
        return "networkplugin"
    if "carduse" in f or "remote" in f:
        return "networkplugin"

    # Kind hints
    if "文档" in kind:
        return "(docs)"

    return "networkplugin"


def _render_index(project_name: str, last_updated: str, module_count: int, pending_plans: int) -> str:
    loader = get_template_loader()
    tmpl = loader.load("INDEX.md") or "# {project_name} 知识库\n"
    content = tmpl
    content = content.replace("{project_name}", project_name)

    # Fill YAML block placeholders (best-effort)
    content = content.replace("{YYYY-MM-DD HH:MM}", last_updated)
    content = content.replace("{数量}", str(module_count), 1)  # modules
    content = content.replace("{数量}", str(pending_plans), 1)  # plans

    # Avoid leaving raw placeholders when replacement count mismatches.
    content = re.sub(r"\{YYYY-MM-DD HH:MM\}", last_updated, content)

    # Avoid dead placeholder links: keep the example as inline code.
    content = content.replace("[modules/{模块名}.md](modules/{模块名}.md)", "`modules/<模块名>.md`")
    content = content.replace("modules/{模块名}.md", "modules/<模块名>.md")

    # Keep generic archive/plan guidance without being normalized into "无".
    content = content.replace("archive/{YYYY-MM}/{方案包}/proposal.md", "archive/<YYYY-MM>/<方案包>/proposal.md")
    content = content.replace("plan/{方案包}/*", "plan/<方案包>/*")

    return _finalize_doc(content)


def _render_context(project_name: str, overview: str, arch: str, tech: str) -> str:
    loader = get_template_loader()
    tmpl = loader.load("context.md") or "# 项目上下文\n"

    # Fill with real data where we can.
    content = tmpl
    content = content.replace("{项目名称}", project_name)
    content = content.replace("{一句话描述}", "LBoL 联机/同步 Mod（Harmony Patch + LiteNetLib）")
    content = content.replace("{Web应用/CLI工具/库/服务/...}", "LBoL Mod")
    content = content.replace("{开发中/维护中/稳定}", "开发中")

    content = content.replace("{主要编程语言}", "C#")
    content = content.replace("{使用的框架}", "Harmony + LiteNetLib")
    content = content.replace("{npm/pip/cargo/...}", ".NET SDK")
    content = content.replace("{webpack/vite/gradle/...}", "dotnet build")

    # Minimal dependencies row - keep placeholders removed.
    content = content.replace("| {依赖名} | {版本} | {用途说明} |", "| Harmony | (repo) | Patch 框架 |\n| LiteNetLib | (repo) | 网络传输 |")

    # Core functionality and boundaries.
    content = content.replace("- {功能1}", "- 联机同步：网络消息/事件通道（GameEvent）")
    content = content.replace("- {功能2}", "- Harmony 补丁：战斗/事件/交易等关键交互同步")

    content = content.replace("- {做什么}", "- 让 LBoL 支持多人联机并同步关键状态")
    content = content.replace("- {不做什么}", "- 不承诺兼容所有第三方 Mod；不做云存档/账号体系")

    # Conventions: reuse existing project.md snippets.
    content = content.replace("{驼峰/下划线/...}", "与现有代码一致")
    content = content.replace("{规则}", "与现有目录结构一致")

    content = content.replace("{格式说明}", "以日志为主（必要时 JSON 结构）")
    content = content.replace("{级别说明}", "Info/Warn/Error")

    content = content.replace("{框架名}", "(未统一)；可用构建验证代替")
    content = content.replace("{百分比}", "(未设定)")
    content = content.replace("{路径}", "(无固定路径)")

    content = content.replace("{策略}", "(未约束)")
    content = content.replace("{格式}", "(未约束)")

    # Constraints: include ADR pointers derived from arch.md.
    constraints_table = (
        "| 约束 | 原因 | 决策来源 |\n"
        "|------|------|----------|\n"
        "| 远端队友目标出牌：目标端结算 + 快照广播 | 动画一致且避免重复结算 | archive/2026-01/202601090851_remote_target_card/proposal.md#D001 |\n"
        "| ServerCore 双模式（Host/Relay） | 复用核心并统一消息链路 | archive/2026-01/202601091556_unify_server_core_two_modes/proposal.md#D001 |\n"
    )
    content = re.sub(r"\| \{约束描述\} \| \{简要原因\} \| \[\{方案包名\}#D\{NNN\}\].*?\|", constraints_table, content)

    # Fill technical debt table with concrete items.
    debt_table = (
        "| 债务描述 | 优先级 | 来源 | 建议处理时机 |\n"
        "|---------|--------|------|-------------|\n"
        "| 缺少自动化联机/同步回归测试（主要依赖手动联机验证） | P1 | 项目约定 | 修改网络/补丁逻辑后立即执行 |\n"
        "| 文档与代码一致性需持续维护（新增事件/载荷时需同步 modules/protocol.md） | P2 | 知识库升级 | 每次新增消息类型时 |\n"
    )
    content = re.sub(
        r"\| 债务描述 \| 优先级 \| 来源 \| 建议处理时机 \|[\s\S]*?\| \(待补充\) \| P0/P1/P2 \| \(待补充\) \| \(待补充\) \|",
        debt_table,
        content,
        flags=re.MULTILINE,
    )

    # Append a short appendix with migrated raw docs (so nothing is lost).
    appendix_parts: List[str] = []
    if tech.strip():
        appendix_parts.append("## 附录A：旧版 project.md（原文保留）\n\n" + tech.strip() + "\n")
    if overview.strip():
        appendix_parts.append("## 附录B：旧版 wiki/overview.md（原文保留）\n\n" + overview.strip() + "\n")
    if arch.strip():
        appendix_parts.append("## 附录C：旧版 wiki/arch.md（原文保留）\n\n" + arch.strip() + "\n")

    if appendix_parts:
        content = content.rstrip() + "\n\n---\n\n" + "\n\n".join(appendix_parts)

    return _finalize_doc(content)


def _render_modules_index() -> str:
    loader = get_template_loader()
    tmpl = loader.load("modules/_index.md") or "# 模块索引\n"

    rows = [
        "| 模块 | 职责 | 状态 | 文档 |",
        "|------|------|------|------|",
        "| networkplugin | 联机同步与 UI/补丁主模块 | 🚧 | [networkplugin.md](./networkplugin.md) |",
        "| networkplayer | 玩家模型/DTO/兼容层 | ✅ | [networkplayer.md](./networkplayer.md) |",
        "| protocol | 网络事件/载荷/数据模型 | ✅ | [protocol.md](./protocol.md) |",
    ]

    dep = (
        "networkplugin → protocol\n"
        "networkplugin → networkplayer\n"
        "networkplayer → protocol\n"
    )

    content = tmpl
    content = re.sub(r"\| \{模块名\} \| \{一句话职责\} \| ✅/🚧/📝 \| \[\{模块名\}\.md\].*?\|", "\n".join(rows), content)
    content = content.replace("模块A → 模块B → 模块C\n      ↘ 模块D", dep)
    return _finalize_doc(content)


def _render_module_doc(module_name: str, purpose: str, raw_md: str, deps: List[str], rdeps: List[str]) -> str:
    """Render a module doc using the module template but with all fields filled."""
    loader = get_template_loader()
    tmpl = loader.load("modules/module.md") or "# {模块名}\n\n## 职责\n\n{详细职责描述}\n"

    content = tmpl.replace("{模块名}", module_name)

    preserved = raw_md.strip() or "(无)"

    # 职责
    content = content.replace(
        "{详细职责描述}",
        purpose.strip() + "\n\n---\n\n以下内容从旧版 wiki 迁移，保持原文：\n\n" + preserved,
    )

    # Interface: keep minimal but concrete.
    content = content.replace(
        "| {名称} | {参数列表} | {返回类型} | {功能说明} |",
        "| (见文档原文) | - | - | 模块接口以代码为准，文档记录关键约定 |",
    )
    content = content.replace(
        "| {字段名} | {类型} | {用途说明} |",
        "| - | - | - |",
    )

    # Behavior section: keep one scenario pointing readers to the migrated content.
    content = content.replace("{场景名称}", "核心场景")
    content = content.replace("{前置条件}", "需要联机连接（Host/Relay）")
    content = content.replace("{预期行为}", "按模块约定发送/接收事件并保证一致性")
    content = content.replace("{预期结果}", "本地与远端状态收敛一致")

    dep_str = ", ".join(deps) if deps else "无"
    rdep_str = ", ".join(rdeps) if rdeps else "无"
    content = content.replace("依赖: {依赖模块列表}", f"依赖: {dep_str}")
    content = content.replace("被依赖: {被依赖模块列表}", f"被依赖: {rdep_str}")

    return _finalize_doc(content)


def _render_protocol(api_md: str, data_md: str) -> str:
    merged = []
    if api_md.strip():
        merged.append("## 来源：wiki/api.md\n\n" + _strip_first_heading(api_md).strip())
    if data_md.strip():
        merged.append("## 来源：wiki/data.md\n\n" + _strip_first_heading(data_md).strip())
    body = "\n\n".join([m for m in merged if m.strip()])
    return _render_module_doc(
        module_name="protocol",
        purpose="定义网络事件/载荷/数据模型的文档约定（以代码实现为准）。",
        raw_md=body,
        deps=[],
        rdeps=["networkplugin", "networkplayer"],
    )


def _render_archive_index(entries: List[HistoryEntry]) -> str:
    loader = get_template_loader()
    tmpl = loader.load("archive/_index.md") or "# 方案归档索引\n"

    # Build a simple current-year table.
    rows = [
        "| 时间戳 | 名称 | 类型 | 涉及模块 | 决策 | 结果 |",
        "|--------|------|------|---------|------|------|",
    ]

    for e in entries:
        affected = _infer_modules_for_entry(e.feature, e.kind)
        rows.append(f"| {e.timestamp} | {e.feature} | {e.kind} | {affected} | {e.feature}#D001 | {e.status} |")

    content = tmpl
    content = re.sub(r"\| \{YYYYMMDDHHMM\} \| \{feature\} .*?\|", "\n".join(rows), content)

    # Build monthly bullet list.
    by_month: Dict[str, List[HistoryEntry]] = {}
    for e in entries:
        by_month.setdefault(get_year_month(e.timestamp), []).append(e)

    month_blocks: List[str] = []
    for month in sorted(by_month.keys(), reverse=True):
        items = by_month[month]
        items.sort(key=lambda x: x.timestamp)
        lines = [f"### {month}"]
        for e in items:
            pkg = f"{e.timestamp}_{e.feature}"
            lines.append(f"- [{pkg}](./{month}/{pkg}/) - {e.kind} / {e.status}")
        month_blocks.append("\n".join(lines))

    # Replace the example YYYY-MM section.
    content = re.sub(r"### YYYY-MM[\s\S]*?$", "\n\n".join(month_blocks) + "\n", content, flags=re.MULTILINE)

    # Replace year links hint to avoid dead links.
    content = content.replace("> 历史年份: [2024](_index-2024.md) | [2023](_index-2023.md) | ...", "> 历史年份: 2026（当前）")

    # Remove the template example bullet if it still exists.
    content = re.sub(r"\n- \[YYYYMMDDHHMM_feature\]\(\./YYYY-MM/.*?\) - .*?\n", "\n", content)

    return _finalize_doc(content)


def _render_archive_package_proposal(entry: HistoryEntry, why_md: str, how_md: str) -> str:
    loader = get_template_loader()
    tmpl = loader.load("plan/proposal.md") or "# 变更提案: {feature}\n"

    date = f"{entry.timestamp[:4]}-{entry.timestamp[4:6]}-{entry.timestamp[6:8]}"
    pkg_type = _guess_pkg_type(entry.kind)

    repl = {
        "{feature}": entry.feature,
        "{YYYY-MM-DD}": date,
        "{pkg_type}": pkg_type,
    }

    content = tmpl
    for k, v in repl.items():
        content = content.replace(k, v)

    # Fill meta fields that are not placeholders
    content = content.replace("类型: 新功能/修复/重构/优化", f"类型: {entry.kind}")
    content = content.replace("优先级: P0/P1/P2/P3", "优先级: P2")
    content = content.replace("状态: 草稿", f"状态: 归档({entry.status})")

    affected = _infer_modules_for_entry(entry.feature, entry.kind)

    # Inject migrated content into key sections.
    why_body = _strip_first_heading(why_md).strip()
    how_body = _strip_first_heading(how_md).strip()

    if not why_body:
        why_body = "(无)"
    if not how_body:
        how_body = "(无)"

    content = content.replace("{为什么需要这个变更}", why_body)
    content = content.replace("{要达成什么目标}", f"完成 {entry.feature} 的方案归档，并保证资料在新版知识库结构中可追溯。")

    content = content.replace("{简要描述实现方式}", how_body)

    # Impact scope
    content = content.replace("- {模块1}: {影响说明}", f"- {affected}: 方案/实现/文档更新")
    content = content.replace("预计变更文件: {数量}", "预计变更文件: 已完成（归档）")

    # Risks
    content = content.replace("| {风险} | 高/中/低 | {措施} |", "| 资料迁移遗漏 | 低 | 原始文件保留 + 生成新版归档 |")

    # Decision section
    content = content.replace("{决策标题}", "采用现有方案并按新版模板归档")
    content = content.replace("{为什么需要这个决策}", "需要将历史方案迁移到统一结构，便于检索与后续维护。")
    content = content.replace("A: {方案A}", "A: 直接归档（推荐）")
    content = content.replace("B: {方案B}", "B: 重写方案后归档")
    content = content.replace("{优点}", "成本低")
    content = content.replace("{缺点}", "可能保留历史表述风格")
    content = content.replace("{详细理由}", "保留原文以避免信息丢失，同时补齐索引与结构。")
    content = content.replace("{对哪些模块有影响}", affected)

    # Make acceptance reflect archived status.
    content = content.replace("- [ ] {标准1}", "- [√] 资料已迁移并可追溯")
    content = content.replace("- [ ] {标准2}", "- [√] 归档包包含 proposal.md + tasks.md")

    return _finalize_doc(content)


def _render_archive_package_tasks(entry: HistoryEntry, task_md: str) -> str:
    loader = get_template_loader()
    tmpl = loader.load("plan/tasks.md") or "# 任务清单: {feature}\n"

    date = f"{entry.timestamp[:4]}-{entry.timestamp[4:6]}-{entry.timestamp[6:8]}"
    pkg_name = f"{entry.timestamp}_{entry.feature}"

    content = tmpl
    content = content.replace("{feature}", entry.feature)
    content = content.replace("helloagents/plan/{YYYYMMDDHHMM}_{feature}/", f"helloagents/archive/{get_year_month(entry.timestamp)}/{pkg_name}/")

    # Inject the original tasks list below "任务列表" to preserve details.
    task_body = _strip_first_heading(_normalize_tasks_statuses(task_md)).strip()
    if not task_body:
        task_body = "(无任务明细)"

    # Replace the placeholder tasks list block.
    # Use a function replacement so backslashes in markdown are not treated as regex escapes.
    replacement = "## 任务列表\n\n" + task_body + "\n\n---\n\n## 执行备注"
    content = re.sub(
        r"## 任务列表[\s\S]*?## 执行备注",
        lambda _m: replacement,
        content,
        flags=re.MULTILINE,
    )

    # Update execution status header quickly (archived):
    content = re.sub(r"总任务: X", "总任务: (已归档)", content)
    content = re.sub(r"已完成: 0", f"已完成: (参考原任务列表)", content)
    content = re.sub(r"完成率: 0%", "完成率: (参考原任务列表)", content)

    if not content.endswith("\n"):
        content += "\n"

    return _finalize_doc(content)


def generate_plan(project_root: Path) -> Dict:
    kb_root = get_workspace_path(str(project_root))

    # Source docs
    overview = _read_text_or_empty(kb_root / "wiki" / "overview.md")
    api_md = _read_text_or_empty(kb_root / "wiki" / "api.md")
    arch_md = _read_text_or_empty(kb_root / "wiki" / "arch.md")
    data_md = _read_text_or_empty(kb_root / "wiki" / "data.md")

    wiki_networkplugin = _read_text_or_empty(kb_root / "wiki" / "modules" / "networkplugin.md")
    wiki_networkplayer = _read_text_or_empty(kb_root / "wiki" / "modules" / "networkplayer.md")

    project_md = _read_text_or_empty(kb_root / "project.md")
    history_index_md = _read_text_or_empty(kb_root / "history" / "index.md")

    entries = _parse_history_index(history_index_md)

    # Compute counts
    project_name = project_root.name
    last_updated = "2026-01-22 00:00"

    pending_plans = 0
    plan_dir = get_plan_path(str(project_root))
    if plan_dir.exists() and plan_dir.is_dir():
        for child in plan_dir.iterdir():
            if child.is_dir() and not child.name.startswith("."):
                pending_plans += 1

    module_count = 3

    operations: List[Dict] = []

    # Ensure top-level directories exist.
    operations.append({"action": "mkdir", "path": "modules"})
    operations.append({"action": "mkdir", "path": "archive"})

    # Root files
    operations.append({
        "action": "write",
        "path": "INDEX.md",
        "content": _render_index(project_name, last_updated, module_count, pending_plans),
    })
    operations.append({
        "action": "write",
        "path": "context.md",
        "content": _render_context(project_name, overview, arch_md, project_md),
    })

    # Modules
    operations.append({
        "action": "write",
        "path": "modules/_index.md",
        "content": _render_modules_index(),
    })

    operations.append({
        "action": "write",
        "path": "modules/networkplugin.md",
        "content": _render_module_doc(
            module_name="networkplugin",
            purpose="提供联机同步与 UI 扩展补丁（HarmonyPatch + LiteNetLib）。",
            raw_md=wiki_networkplugin,
            deps=["protocol", "networkplayer"],
            rdeps=[],
        ),
    })
    operations.append({
        "action": "write",
        "path": "modules/networkplayer.md",
        "content": _render_module_doc(
            module_name="networkplayer",
            purpose="定义知识库层面的 NetworkPlayer 分层约定与兼容层说明（DTO/运行时模型）。",
            raw_md=wiki_networkplayer,
            deps=["protocol"],
            rdeps=["networkplugin"],
        ),
    })
    operations.append({
        "action": "write",
        "path": "modules/protocol.md",
        "content": _render_protocol(api_md, data_md),
    })

    # Archive index
    operations.append({
        "action": "write",
        "path": "archive/_index.md",
        "content": _render_archive_index(entries),
    })

    # Archive packages
    for e in entries:
        ym = get_year_month(e.timestamp)
        pkg = f"{e.timestamp}_{e.feature}"
        base = f"archive/{ym}/{pkg}"
        operations.append({"action": "mkdir", "path": f"archive/{ym}"})
        operations.append({"action": "mkdir", "path": base})

        pkg_dir = project_root / e.rel_path
        why_md = _read_text_or_empty(pkg_dir / "why.md")
        how_md = _read_text_or_empty(pkg_dir / "how.md")
        task_md = _read_text_or_empty(pkg_dir / "task.md")

        operations.append({
            "action": "write",
            "path": f"{base}/proposal.md",
            "content": _render_archive_package_proposal(e, why_md, how_md),
        })
        operations.append({
            "action": "write",
            "path": f"{base}/tasks.md",
            "content": _render_archive_package_tasks(e, task_md),
        })

    return {"operations": operations}


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate HelloAGENTS KB upgrade write plan")
    parser.add_argument("--out", required=True, help="Output plan JSON file")
    parser.add_argument("--path", default=None, help="Project root (default: cwd)")
    args = parser.parse_args()

    base = validate_base_path(args.path)
    plan = generate_plan(base)

    out_path = Path(args.out)
    out_path.write_text(json.dumps(plan, ensure_ascii=False, indent=2), encoding="utf-8")

    print(json.dumps({
        "ok": True,
        "out": str(out_path),
        "operations": len(plan.get("operations", [])),
    }, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
