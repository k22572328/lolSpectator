namespace LolSpector.Core.Models;

// Raw shapes returned by the Riot API. Only the fields lolSpector actually
// uses are modeled — System.Text.Json (case-insensitive) ignores the rest.

public class RiotAccountDto
{
    public string Puuid { get; set; } = "";
    public string GameName { get; set; } = "";
    public string TagLine { get; set; } = "";
}

public class LeagueEntryDto
{
    public string QueueType { get; set; } = "";
    public string Tier { get; set; } = "";
    public string Rank { get; set; } = "";
    public int LeaguePoints { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
}

public class MatchDto
{
    public MatchInfoDto Info { get; set; } = new();
}

public class MatchInfoDto
{
    public long GameDuration { get; set; }
    public long GameStartTimestamp { get; set; }
    public int QueueId { get; set; }
    public List<ParticipantDto> Participants { get; set; } = [];
    public List<TeamDto> Teams { get; set; } = [];
}

public class ParticipantDto
{
    public string Puuid { get; set; } = "";
    public int ChampionId { get; set; }
    public string ChampionName { get; set; } = "";
    public string TeamPosition { get; set; } = "";
    public int TeamId { get; set; }
    public bool Win { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int TotalMinionsKilled { get; set; }
    public int NeutralMinionsKilled { get; set; }
    public int GoldEarned { get; set; }
    public int TotalDamageDealtToChampions { get; set; }
    public int VisionScore { get; set; }
}

public class TeamDto
{
    public int TeamId { get; set; }
    public TeamObjectivesDto Objectives { get; set; } = new();
}

public class TeamObjectivesDto
{
    public ObjectiveDto Champion { get; set; } = new();
}

public class ObjectiveDto
{
    public int Kills { get; set; }
}
