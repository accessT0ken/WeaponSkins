using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.SchemaDefinitions;

using WeaponSkins.Configuration;

namespace WeaponSkins;

public partial class CommandService
{

    private ISwiftlyCore Core { get; init; }
    private ILogger Logger { get; init; }
    private MenuService MenuService { get; init; }
    private MainConfigModel Config { get; init; }

    public CommandService(ISwiftlyCore core,
        ILogger<CommandService> logger,
        MenuService menuService,
        IOptions<MainConfigModel> config)
    {
        Core = core;
        Logger = logger;
        MenuService = menuService;
        Config = config.Value;

        RegisterCommands();
    }
    public void RegisterCommands()
    {
        if (Config.EnableWsCommand)
            Core.Command.RegisterCommand("ws", CommandSkin);
    }

    private void CommandSkin(ICommandContext context)
    {
        if (!context.IsSentByPlayer)
        {
            context.Reply("This command can only be used by players.");
            return;
        }

        MenuService.OpenMainMenu(context.Sender!);
    }
}