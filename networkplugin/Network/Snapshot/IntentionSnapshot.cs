using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot
{
    /// <summary>
    /// 敌人意图快照（降级实现：仅保留可序列化的数据结构，避免 dynamic 依赖）。
    /// </summary>
    public class IntentionSnapshot
    {
        public string IntentionType { get; set; } = string.Empty;
        public string IntentionName { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Description { get; set; } = string.Empty;

        public Dictionary<string, List<Dictionary<string, object>>> EnemyIntentions { get; set; } =
            new Dictionary<string, List<Dictionary<string, object>>>();

        public IntentionSnapshot()
        {
        }

        public IntentionSnapshot(string intentionType, string intentionName, int value)
        {
            IntentionType = intentionType;
            IntentionName = intentionName;
            Value = value;
        }

        public IntentionSnapshot(object battleController)
        {
            EnemyIntentions = new Dictionary<string, List<Dictionary<string, object>>>();
        }
    }
}

