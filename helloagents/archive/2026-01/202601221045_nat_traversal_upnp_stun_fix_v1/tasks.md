# 任务清单: nat_traversal_upnp_stun_fix_v1

目录: `helloagents/plan/无_nat_traversal_upnp_stun_fix_v1/`

---

## 任务状态符号说明

| 符号 | 状态 | 说明 |
|------|------|------|
| `[ ]` | pending | 待执行 |
| `[√]` | completed | 已完成 |
| `[X]` | failed | 执行失败 |
| `[-]` | skipped | 已跳过 |
| `[?]` | uncertain | 待确认 |

---

## 执行状态
```yaml
总任务: (已归档)
已完成: (参考原任务列表)
完成率: (参考原任务列表)
```

---

## 任务列表

> 状态符号：
> - [ ] 待执行
> - [√] 已完成
> - [X] 执行失败
> - [-] 已跳过

## 任务清单

- [ ] 明确端口来源：端口固定且由用户输入；所有本地绑定/上报逻辑以该端口为准（失败需给出清晰错误）。
- [ ] NatInfo 序列化：实现 `IPEndPoint` 的 `JsonConverter`，并通过属性级标注，确保 `RelayServer` 的 `JsonSerializer.Deserialize<NatTraversal.NatInfo>` 可成功反序列化。
- [ ] Token：修复 `GenerateConnectionToken/ValidateConnectionToken`，改为 TTL 校验；补充单元测试（至少覆盖：过期/格式错误/正常）。
- [ ] STUN（可选增强）：实现最小 STUN Binding 探测，替换 `DetectNatType` 中的模拟响应；`DetectNatTypeMultiple` 保持并发。
- [ ] UPnP（低优先级，可选）：按“不支持”为主路径实现（失败即回退），必要时再评估 `Open.NAT` 依赖。
- [ ] 异步一致性：处理 `DetectLocalConnectivity` 内部异步赋值导致信息不一致的问题（建议新增 async 版本或同步等待带超时）。
- [ ] 日志与报告：补充关键日志（成功/失败原因），确保 `GenerateNatReport` 输出有意义。
- [ ] 运行验证：编译 networkplugin；在 VPN/LAN 直连环境下可正常上报/解析；在无 UPnP/无公网环境下不崩溃。

## 回归风险

- 引入新依赖可能导致 Unity 装载失败（缺少依赖 dll 或 AOT 兼容问题）。
- 修改 `NatInfo` 序列化形状可能影响已有协议兼容。

## 测试与验证建议

- 编译：`dotnet build`（或通过现有构建脚本）。
- 逻辑测试：token 解析；端点序列化/反序列化（服务端缓存 NAT info）。
- 运行时：主要在 VPN/LAN 直连环境验证；UPnP/STUN 仅在需要时再做环境验证。

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
