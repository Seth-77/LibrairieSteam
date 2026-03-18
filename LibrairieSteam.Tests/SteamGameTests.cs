using LibrairieSteam.Models;
using Xunit;

namespace LibrairieSteam.Tests;

public class SteamGameTests
{
    [Fact]
    public void PlaytimeFormatted_WithHoursAndMinutes_ReturnsCorrectFormat()
    {
        var game = new SteamGame { PlaytimeForever = 125 }; // 2h 5min

        Assert.Equal("2h 5min", game.PlaytimeFormatted);
    }

    [Fact]
    public void PlaytimeFormatted_WithOnlyMinutes_ReturnsMinutesOnly()
    {
        var game = new SteamGame { PlaytimeForever = 45 };

        Assert.Equal("45min", game.PlaytimeFormatted);
    }

    [Theory]
    [InlineData(0, "0min")]
    [InlineData(60, "1h 0min")]
    [InlineData(61, "1h 1min")]
    [InlineData(1440, "24h 0min")]
    public void PlaytimeFormatted_VariousValues(int minutes, string expected)
    {
        var game = new SteamGame { PlaytimeForever = minutes };
        Assert.Equal(expected, game.PlaytimeFormatted);
    }

    [Fact]
    public void Playtime2WeeksFormatted_WhenZero_ReturnsEmpty()
    {
        var game = new SteamGame { Playtime2Weeks = 0 };
        Assert.Equal("", game.Playtime2WeeksFormatted);
    }

    [Fact]
    public void Playtime2WeeksFormatted_WithValue_IncludesRécemment()
    {
        var game = new SteamGame { Playtime2Weeks = 90 }; // 1h 30min
        Assert.Equal("1h 30min récemment", game.Playtime2WeeksFormatted);
    }
}