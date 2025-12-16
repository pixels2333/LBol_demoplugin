namespace NetworkPlugin.Network.MidGameJoin.Result;

public class CatchUpResult : BaseResult
{
    public int EventsApplied { get; set; }

    public static CatchUpResult Success(int eventsApplied) => new() { IsSuccess = true, EventsApplied = eventsApplied };
    public static CatchUpResult Failed(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
}
