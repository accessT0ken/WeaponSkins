using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared.Players;

using WeaponSkins.Econ;
using WeaponSkins.Shared;

namespace WeaponSkins.Database;

public class DatabaseSynchronizeService
{
    private DatabaseService DatabaseService { get; init; }
    private DataService DataService { get; init; }
    private ILogger<DatabaseSynchronizeService> Logger { get; init; }
    private EconService EconService { get; init; }

    public DatabaseSynchronizeService(DatabaseService databaseService, DataService dataService, ILogger<DatabaseSynchronizeService> logger, EconService econService)
    {
        DatabaseService = databaseService;
        DataService = dataService;
        Logger = logger;
        EconService = econService;
    }

    public void Synchronize()
    {
        SynchronizeAsync();
    }

    public Task SynchronizeAsync()
    {
        return Task.Run(async () =>
        {
            Logger.LogInformation("[WeaponSkins] Starting database synchronization...");

            try
            {
                var skins = await DatabaseService.GetAllSkinsAsync();
                var skinList = skins.ToList();
                skinList.ForEach(skin => DataService.WeaponDataService.StoreSkin(skin));
                Logger.LogInformation("[WeaponSkins] Loaded {Count} weapon skins from database.", skinList.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[WeaponSkins] Failed to load weapon skins from database.");
            }

            try
            {
                var knives = await DatabaseService.GetAllKnifesAsync();
                var knifeList = knives.ToList();
                knifeList.ForEach(knife => DataService.KnifeDataService.StoreKnife(knife));
                Logger.LogInformation("[WeaponSkins] Loaded {Count} knife skins from database.", knifeList.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[WeaponSkins] Failed to load knife skins from database.");
            }

            try
            {
                var gloves = await DatabaseService.GetAllGlovesAsync();
                var gloveList = gloves.ToList();
                gloveList.ForEach(glove => DataService.GloveDataService.StoreGlove(glove));
                Logger.LogInformation("[WeaponSkins] Loaded {Count} glove skins from database.", gloveList.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[WeaponSkins] Failed to load glove skins from database.");
            }

            try
            {
                if (await DatabaseService.HasLegacyAgentsTableAsync())
                {
                    var legacyAgents = await DatabaseService.GetAllLegacyAgentsAsync();
                    int count = 0;
                    foreach (var legacy in legacyAgents)
                    {
                        var steamId = ulong.Parse(legacy.SteamID);
                        if (!string.IsNullOrEmpty(legacy.AgentCT))
                        {
                            var agent = EconService.Agents.Values.FirstOrDefault(a =>
                                a.ModelPath.Contains(legacy.AgentCT, StringComparison.OrdinalIgnoreCase) ||
                                a.Name.Equals(legacy.AgentCT, StringComparison.OrdinalIgnoreCase));
                            if (agent != null)
                            {
                                DataService.AgentDataService.SetAgent(steamId, Team.CT, agent.Index);
                                count++;
                            }
                        }
                        if (!string.IsNullOrEmpty(legacy.AgentT))
                        {
                            var agent = EconService.Agents.Values.FirstOrDefault(a =>
                                a.ModelPath.Contains(legacy.AgentT, StringComparison.OrdinalIgnoreCase) ||
                                a.Name.Equals(legacy.AgentT, StringComparison.OrdinalIgnoreCase));
                            if (agent != null)
                            {
                                DataService.AgentDataService.SetAgent(steamId, Team.T, agent.Index);
                                count++;
                            }
                        }
                    }
                    Logger.LogInformation("[WeaponSkins] Loaded {Count} agents from database (legacy format).", count);
                }
                else
                {
                    var agents = await DatabaseService.GetAllAgentsAsync();
                    var agentList = agents.ToList();
                    agentList.ForEach(agent => DataService.AgentDataService.SetAgent(agent.SteamID, agent.Team, agent.AgentIndex));
                    Logger.LogInformation("[WeaponSkins] Loaded {Count} agents from database.", agentList.Count);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[WeaponSkins] Failed to load agents from database.");
            }

            try
            {
                var musicKits = await DatabaseService.GetAllMusicKitsAsync();
                var mkList = musicKits.ToList();
                mkList.ForEach(mk => DataService.MusicKitDataService.SetMusicKit(mk.SteamID, mk.MusicKitIndex));
                Logger.LogInformation("[WeaponSkins] Loaded {Count} music kits from database.", mkList.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[WeaponSkins] Failed to load music kits from database.");
            }

            Logger.LogInformation("[WeaponSkins] Database synchronization complete.");
        });
    }
}