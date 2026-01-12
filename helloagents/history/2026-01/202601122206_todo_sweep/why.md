# 变更提案：TODO 注释清理与落地（多轮补充）
目录：`helloagents/plan/202601122206_todo_sweep/`

## 背景
- `networkplugin` 内存在多处 `TODO` 注释：一部分是“缺失实现/占位逻辑”（可通过编译与静态检查验证），另一部分需要进入游戏联机环境或依赖外部网络环境（UPnP/STUN）才能验证与落地。

## 目标
- 扫描并分类 `TODO` 注释。
- 将“无需手动验证即可安全落地”的 TODO 直接实现，并通过 `dotnet build` 验证可编译。
- 将“需要手动验证/外部依赖/大规模重构决策”的 TODO 统一记录并显式跳过（不在本批实现）。

## 非目标
- 不在本批完成需要真实联机/回放/场景 UI 才能验证的逻辑（仅记录）。
- 不引入新的第三方网络库来实现 UPnP/STUN。

## 相关方案
- `helloagents/history/2026-01/202601121639_midgamejoin_todo/` 已归档（MidGameJoin TODO 落地方案）。
