using LBoL.Base;
using NetworkPlugin.Utils;
using Xunit;

namespace NetworkPlugin.Tests;

public class ManaUtilsTests
{
    [Fact]
    public void ManaGroupToArray_WithValidGroup_ReturnsCorrectArray()
    {
        var group = new ManaGroup { Red = 1, Blue = 2, Green = 3, White = 4 };
        var arr = ManaUtils.ManaGroupToArray(group);

        Assert.NotNull(arr);
        Assert.Equal(4, arr.Length);
        Assert.Equal(1, arr[0]);
        Assert.Equal(2, arr[1]);
        Assert.Equal(3, arr[2]);
        Assert.Equal(4, arr[3]);
    }

    [Fact]
    public void ArrayToManaGroup_WithValidArray_ReturnsCorrectGroup()
    {
        var arr = new[] { 5, 6, 7, 8 };
        var group = ManaUtils.ArrayToManaGroup(arr);

        Assert.Equal(5, group.Red);
        Assert.Equal(6, group.Blue);
        Assert.Equal(7, group.Green);
        Assert.Equal(8, group.White);
    }

    [Fact]
    public void ArrayToManaGroup_WithNullOrShortArray_ReturnsEmpty()
    {
        var empty1 = ManaUtils.ArrayToManaGroup(null);
        var empty2 = ManaUtils.ArrayToManaGroup(new[] { 1, 2, 3 });

        Assert.Equal(0, empty1.Red);
        Assert.Equal(0, empty1.Blue);
        Assert.Equal(0, empty2.Red);
        Assert.Equal(0, empty2.Blue);
    }

    [Fact]
    public void ManaGroupToString_ReturnsCorrectFormat()
    {
        var group = new ManaGroup { Red = 2, Blue = 3, Green = 0, White = 5 };
        var str = ManaUtils.ManaGroupToString(group);

        Assert.Equal("R2B3G0W5", str);
    }

    [Fact]
    public void StringToManaGroup_WithValidString_ParsesCorrectly()
    {
        var group = ManaUtils.StringToManaGroup("R2B3G0W5");

        Assert.Equal(2, group.Red);
        Assert.Equal(3, group.Blue);
        Assert.Equal(0, group.Green);
        Assert.Equal(5, group.White);
    }

    [Fact]
    public void StringToManaGroup_WithNullOrEmpty_ReturnsEmpty()
    {
        var group1 = ManaUtils.StringToManaGroup(null);
        var group2 = ManaUtils.StringToManaGroup(string.Empty);

        Assert.Equal(0, group1.Red);
        Assert.Equal(0, group1.Blue);
        Assert.Equal(0, group2.Red);
        Assert.Equal(0, group2.Blue);
    }

    [Fact]
    public void CalculateManaDifference_ReturnsCorrectDifference()
    {
        var from = new ManaGroup { Red = 2, Blue = 5, Green = 1, White = 4 };
        var to = new ManaGroup { Red = 5, Blue = 2, Green = 1, White = 6 };

        var diff = ManaUtils.CalculateManaDifference(from, to);

        Assert.Equal(3, diff.Red);
        Assert.Equal(-3, diff.Blue);
        Assert.Equal(0, diff.Green);
        Assert.Equal(2, diff.White);
    }

    [Fact]
    public void CanAffordMana_ReturnsTrueWhenAffordable()
    {
        var available = new ManaGroup { Red = 5, Blue = 5, Green = 5, White = 5 };
        var cost1 = new ManaGroup { Red = 2, Blue = 3, Green = 5, White = 0 };
        var cost2 = new ManaGroup { Red = 6, Blue = 1, Green = 1, White = 1 };

        Assert.True(ManaUtils.CanAffordMana(available, cost1));
        Assert.False(ManaUtils.CanAffordMana(available, cost2));
    }

    [Fact]
    public void GetTotalMana_ReturnsSumOfAllComponents()
    {
        var group = new ManaGroup
        {
            Any = 1,
            White = 2,
            Blue = 3,
            Black = 4,
            Red = 5,
            Green = 6,
            Colorless = 7,
            Philosophy = 8,
            Hybrid = 9
        };

        var total = ManaUtils.GetTotalMana(group);

        Assert.Equal(45, total);
    }

    [Fact]
    public void CloneManaGroup_ReturnsDuplicate()
    {
        var original = new ManaGroup
        {
            Any = 1,
            White = 2,
            Blue = 3,
            Black = 4,
            Red = 5,
            Green = 6,
            Colorless = 7,
            Philosophy = 8,
            Hybrid = 9
        };

        var clone = ManaUtils.CloneManaGroup(original);

        Assert.Equal(original.Any, clone.Any);
        Assert.Equal(original.White, clone.White);
        Assert.Equal(original.Blue, clone.Blue);
        Assert.Equal(original.Black, clone.Black);
        Assert.Equal(original.Red, clone.Red);
        Assert.Equal(original.Green, clone.Green);
        Assert.Equal(original.Colorless, clone.Colorless);
        Assert.Equal(original.Philosophy, clone.Philosophy);
        Assert.Equal(original.Hybrid, clone.Hybrid);
    }

    [Fact]
    public void GetEmptyManaGroup_ReturnsEmpty()
    {
        var group = ManaUtils.GetEmptyManaGroup();
        Assert.Equal(0, group.Any);
        Assert.Equal(0, group.Red);
        Assert.Equal(0, group.Blue);
    }
}
