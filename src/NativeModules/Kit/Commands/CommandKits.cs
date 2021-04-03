#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2018  leonardosnt
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/
#endregion

using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Core;
using Essentials.I18n;
using System.Linq;

namespace Essentials.NativeModules.Kit.Commands {

    [CommandInfo(
        Name = "kits",
        Description = "View available kits"
    )]
    public class CommandKits : EssCommand {

        public override CommandResult OnExecute(ICommandSource source, ICommandArgs parameters) {
            var kitConfig = EssCore.Instance.Config.Kit;
            var hasEconomyProvider = UEssentials.EconomyProvider.IsPresent;

            var kits = KitModule.Instance.KitManager.Kits.Where(k => k.CanUse(source)).Select(k => {
                if (!hasEconomyProvider || !kitConfig.ShowCost || (k.Cost <= 0 && !kitConfig.ShowCostIfZero)) {
                    return k.Name;
                }
                return string.Format(kitConfig.CostFormat, k.Name, k.Cost, UEssentials.EconomyProvider.Value.CurrencySymbol);
            }).ToList();


            if (kits.Count == 0) {
                // Prevent confusion when there are no kits defined
                if (KitModule.Instance.KitManager.Count == 0) {
                    return CommandResult.LangError("KIT_NONE_DEFINED");
                }
                EssLang.SendNoBuffer(source, "KIT_NONE");
            } else {
                EssLang.SendNoBuffer(source, "KIT_LIST", string.Join(", ", kits.ToArray()));
            }

            return CommandResult.Success();
        }

    }

}