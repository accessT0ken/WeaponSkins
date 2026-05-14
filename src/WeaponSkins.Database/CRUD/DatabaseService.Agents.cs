using FreeSql.DataAnnotations;

using SwiftlyS2.Shared.Players;

namespace WeaponSkins.Database;

[Table(Name = "wp_player_agents", DisableSyncStructure = true)]
public record LegacyAgentModel
{
    [Column(Name = "steamid")] public required string SteamID { get; set; }
    [Column(Name = "agent_ct")] public string? AgentCT { get; set; }
    [Column(Name = "agent_t")] public string? AgentT { get; set; }
}

public partial class DatabaseService
{
    public async Task<bool> HasLegacyAgentsTableAsync()
    {
        try
        {
            var result = await fsql.Ado.QueryAsync<string>(
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'wp_player_agents' AND COLUMN_NAME = 'agent_ct'");
            return result.Any();
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<LegacyAgentModel>> GetAllLegacyAgentsAsync()
    {
        return await fsql.Ado.QueryAsync<LegacyAgentModel>(
            "SELECT steamid as SteamID, agent_ct as AgentCT, agent_t as AgentT FROM wp_player_agents");
    }

    public async Task StoreAgentsAsync(IEnumerable<(ulong SteamID, Team Team, int AgentIndex)> agents)
    {
        await fsql.InsertOrUpdate<AgentModel>()
            .SetSource(agents.Select(a => AgentModel.FromDataModel(a.SteamID, a.Team, a.AgentIndex)))
            .ExecuteAffrowsAsync();
    }

    public async Task<int?> GetAgentAsync(ulong steamId, Team team)
    {
        var model = await fsql.Select<AgentModel>()
            .Where(a => a.SteamID == steamId.ToString() && a.Team == (short)team)
            .ToOneAsync();
        return model?.AgentIndex;
    }

    public async Task<IEnumerable<(ulong SteamID, Team Team, int AgentIndex)>> GetAgentsAsync(ulong steamId)
    {
        var models = await fsql.Select<AgentModel>()
            .Where(a => a.SteamID == steamId.ToString())
            .ToListAsync();
        return models.Select(m => (ulong.Parse(m.SteamID), (Team)m.Team, m.AgentIndex));
    }

    public async Task<IEnumerable<(ulong SteamID, Team Team, int AgentIndex)>> GetAllAgentsAsync()
    {
        var models = await fsql.Select<AgentModel>().ToListAsync();
        return models.Select(m => (ulong.Parse(m.SteamID), (Team)m.Team, m.AgentIndex));
    }

    public async Task RemoveAgentAsync(ulong steamId, Team team)
    {
        await fsql.Delete<AgentModel>()
            .Where(a => a.SteamID == steamId.ToString() && a.Team == (short)team)
            .ExecuteAffrowsAsync();
    }

    public async Task RemoveAgentsAsync(ulong steamId)
    {
        await fsql.Delete<AgentModel>()
            .Where(a => a.SteamID == steamId.ToString())
            .ExecuteAffrowsAsync();
    }
}
