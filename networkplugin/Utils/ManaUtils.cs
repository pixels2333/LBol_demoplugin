using System;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Units;

namespace NetworkPlugin.Utils
{
    /// <summary>
    /// LBoL法力系统工具类 - 用于处理法力相关的计算和转换
    /// LBoL使用多色法力系统（红、蓝、绿、白）
    /// </summary>
    public static class ManaUtils
    {
        /// <summary>
        /// 将ManaGroup转换为数组格式 [红, 蓝, 绿, 白]
        /// </summary>
        /// <param name="manaGroup">法力组</param>
        /// <returns>法力数组</returns>
        public static int[] ManaGroupToArray(ManaGroup manaGroup)
        {
            if (manaGroup == null)
            {
                return [0, 0, 0, 0];
            }

            return
            [
                manaGroup.Red,
                manaGroup.Blue,
                manaGroup.Green,
                manaGroup.White
            ];
        } // 将法力组转换为数组格式 [红, 蓝, 绿, 白]，便于网络传输和计算

        /// <summary>
        /// 将数组转换为ManaGroup
        /// </summary>
        /// <param name="manaArray">法力数组 [红, 蓝, 绿, 白]</param>
        /// <returns>ManaGroup实例</returns>
        public static ManaGroup ArrayToManaGroup(int[] manaArray)
        {
            if (manaArray == null || manaArray.Length < 4)
            {
                return new ManaGroup(0, 0, 0, 0);
            }

            return new ManaGroup(
                manaArray[0], // Red
                manaArray[1], // Blue
                manaArray[2], // Green
                manaArray[3]  // White
            );
        } // 将数组格式 [红, 蓝, 绿, 白] 转换为法力组对象，用于游戏逻辑处理

        /// <summary>
        /// 获取ManaGroup的字符串表示
        /// </summary>
        /// <param name="manaGroup">法力组</param>
        /// <returns>字符串表示</returns>
        public static string ManaGroupToString(ManaGroup manaGroup)
        {
            if (manaGroup == null)
            {
                return "R0B0G0W0";
            }

            return $"R{manaGroup.Red}B{manaGroup.Blue}G{manaGroup.Green}W{manaGroup.White}";
        } // 将法力组转换为紧凑的字符串表示，格式为R红B蓝G绿W白

        /// <summary>
        /// 从字符串解析ManaGroup
        /// </summary>
        /// <param name="manaString">法力字符串</param>
        /// <returns>ManaGroup实例</returns>
        public static ManaGroup StringToManaGroup(string manaString)
        {
            // 解析格式如 "R2B1G3W0"
            try
            {
                if (string.IsNullOrEmpty(manaString))
                {
                    return new ManaGroup(0, 0, 0, 0);
                }

                var parts = manaString.Split(['R', 'B', 'G', 'W'], StringSplitOptions.RemoveEmptyEntries);

                int red = 0, blue = 0, green = 0, white = 0;

                if (parts.Length > 0)
                {
                    int.TryParse(parts[0], out red);
                }

                if (parts.Length > 1)
                {
                    int.TryParse(parts[1], out blue);
                }

                if (parts.Length > 2)
                {
                    int.TryParse(parts[2], out green);
                }

                if (parts.Length > 3)
                {
                    int.TryParse(parts[3], out white);
                }

                return new ManaGroup(red, blue, green, white);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[ManaUtils] Error parsing mana string '{manaString}': {ex.Message}");
                return new ManaGroup(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// 计算两个ManaGroup的差值
        /// </summary>
        /// <param name="from">起始法力</param>
        /// <param name="to">目标法力</param>
        /// <returns>差值ManaGroup</returns>
        public static ManaGroup CalculateManaDifference(ManaGroup from, ManaGroup to)
        {
            if (from == null && to == null)
            {
                return new ManaGroup(0, 0, 0, 0);
            }

            if (from == null)
            {
                return new ManaGroup(to.Red, to.Blue, to.Green, to.White);
            }

            if (to == null)
            {
                return new ManaGroup(-from.Red, -from.Blue, -from.Green, -from.White);
            }

            return new ManaGroup(
                to.Red - from.Red,
                to.Blue - from.Blue,
                to.Green - from.Green,
                to.White - from.White
            );
        }

        /// <summary>
        /// 检查是否有足够的法力来支付消耗
        /// </summary>
        /// <param name="available">可用法力</param>
        /// <param name="cost">法力消耗</param>
        /// <returns>如果法力足够返回true</returns>
        public static bool CanAffordMana(ManaGroup available, ManaGroup cost)
        {
            if (available == null || cost == null)
            {
                return false;
            }

            return available.Red >= cost.Red &&
                   available.Blue >= cost.Blue &&
                   available.Green >= cost.Green &&
                   available.White >= cost.White;
        }

        /// <summary>
        /// 获取总法力值
        /// </summary>
        /// <param name="manaGroup">法力组</param>
        /// <returns>总法力值</returns>
        public static int GetTotalMana(ManaGroup manaGroup)
        {
            return manaGroup?.Total ?? 0;
        }

        /// <summary>
        /// 创建空的ManaGroup
        /// </summary>
        /// <returns>空的ManaGroup</returns>
        public static ManaGroup GetEmptyManaGroup()
        {
            return new ManaGroup(0, 0, 0, 0);
        }

        /// <summary>
        /// 克隆ManaGroup
        /// </summary>
        /// <param name="original">原始ManaGroup</param>
        /// <returns>克隆的ManaGroup</returns>
        public static ManaGroup CloneManaGroup(ManaGroup original)
        {
            if (original == null)
            {
                return null;
            }

            return new ManaGroup(original.Red, original.Blue, original.Green, original.White);
        }
    }
}