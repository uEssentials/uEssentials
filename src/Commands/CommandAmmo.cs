using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.I18n;
using Rocket.Unturned.Items;
using SDG.Unturned;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "ammo",
        Aliases = new string[0],
        Usage = "[amount]",
        Description = "Get equipped weapon ammo.",
        AllowedSource = AllowedSource.PLAYER
    )]
    public class CommandAmmo : EssCommand
    {
        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            var player = src.ToPlayer();
            var itemAsset = player.Equipment.asset;
            if (!(itemAsset is ItemGunAsset gunAsset))
                return CommandResult.LangError("AMMO_NOT_GUN");

            if (!(UnturnedItems.GetItemAssetById(gunAsset.getMagazineID()) is ItemMagazineAsset magAsset))
                return CommandResult.LangError("AMMO_FAILED");

            switch (args.Length)
            {
                case 0:
                    return SpawnMag(player, magAsset);
                default:
                    return !ushort.TryParse(args[0].ToString(), out var amount)
                        ? CommandResult.ShowUsage()
                        : SpawnMag(player, magAsset, amount);
            }
        }

        private CommandResult SpawnMag(UPlayer player, ItemAsset magAsset, ushort amount = 1)
        {
            player.GiveItem(new Item(magAsset.id, true), amount, true);
            EssLang.Send(player, "AMMO_SUCCESS", amount, magAsset.itemName, magAsset.id);
            return CommandResult.Success();
        }
    }
}