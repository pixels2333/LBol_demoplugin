using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot
{
    /// <summary>
    /// 敌人意图快照（降级实现：仅保留可序列化的数据结构，避免 dynamic 依赖）。
    /// </summary>
    public class IntentionSnapshot
    {
        /// <summary>意图类型标识</summary>
        public string IntentionType { get; set; } = string.Empty;
        /// <summary>意图名称</summary>
        public string IntentionName { get; set; } = string.Empty;
        /// <summary>意图数值（如伤害量）</summary>
        public int Value { get; set; }
        /// <summary>意图描述文本</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>所有敌人的意图数据，键为敌人ID，值为意图属性字典列表</summary>
        public Dictionary<string, List<Dictionary<string, object>>> EnemyIntentions { get; set; } =
            new Dictionary<string, List<Dictionary<string, object>>>();

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public IntentionSnapshot()
        {
        }

        /// <summary>
        /// 使用指定意图信息初始化快照
        /// </summary>
        /// <param name="intentionType">意图类型</param>
        /// <param name="intentionName">意图名称</param>
        /// <param name="value">意图数值</param>
        public IntentionSnapshot(string intentionType, string intentionName, int value)
        {
            IntentionType = intentionType;
            IntentionName = intentionName;
            Value = value;
        }

        /// <summary>
        /// 从战斗控制器对象初始化意图快照
        /// </summary>
        /// <param name="battleController">战斗控制器实例</param>
        public IntentionSnapshot(object battleController)
        {
            EnemyIntentions = new Dictionary<string, List<Dictionary<string, object>>>();
        }
    }
}
