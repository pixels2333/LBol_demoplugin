using System;
using System.Reflection;
using LBoL.Core;
using LBoL.Core.Units;
using Moq;

namespace NetworkPlugin.Tests
{
    public static class GameMockFactory
    {
        public static GameRunController CreateMockGameRun(PlayerUnit? player = null, int money = 0)
        {

            var gameRun = (GameRunController)Activator.CreateInstance(typeof(GameRunController), true)!;

            if (player != null)
            {
                SetPrivateFieldOrProperty(gameRun, "Player", player);
            }

            SetPrivateFieldOrProperty(gameRun, "Money", money);

            return gameRun;
        }

        public static PlayerUnit CreateMockPlayer(string id = "mock_player_id", string name = "Mock Player", int maxHp = 100, int hp = 100)
        {
            var mock = new Mock<PlayerUnit>();
            var player = mock.Object;

            SetPrivateFieldOrProperty(player, "Id", id);
            SetPrivateFieldOrProperty(player, "MaxHp", maxHp);
            SetPrivateFieldOrProperty(player, "Hp", hp);

            mock.Setup(p => p.Name).Returns(name);

            return player;
        }

        public static void SetPrivateFieldOrProperty(object instance, string name, object value)
        {
            if (instance == null) return;

            var type = instance.GetType();
            while (type != null)
            {

                var backingFieldName = $"<{name}>k__BackingField";
                var backingField = type.GetField(backingFieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (backingField != null)
                {
                    backingField.SetValue(instance, value);
                    return;
                }

                var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            ?? type.GetField($"_{char.ToLower(name[0])}{name.Substring(1)}", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(instance, value);
                    return;
                }

                var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(instance, value);
                    return;
                }

                type = type.BaseType;
            }

            throw new InvalidOperationException($"Could not find field or property '{name}' on type {instance.GetType().FullName}");
        }
    }
}
