namespace LolSpector.Core.Models;

public class PlayerResult
{
    public string RiotId { get; set; } = "";
    public string Puuid { get; set; } = "";
    public List<LeagueEntryDto> LeagueEntries { get; set; } = [];

    /// <summary>Most recent matches first, across both ranked queues.</summary>
    public List<MatchParticipantSample> SoloSamples { get; set; } = [];
    public List<MatchParticipantSample> FlexSamples { get; set; } = [];
}
