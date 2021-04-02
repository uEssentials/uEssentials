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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using SDG.Unturned;
using Essentials.I18n;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Rocket.Core;

namespace Essentials.Commands
{

    [CommandInfo(
        Name = "sudo",
        Description = "Make player or console execute a command",
        Usage = "[player | * | *console*] [command]"
    )]
    public class CommandSudo : EssCommand
    {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            if (args.Length < 2)
            {
                return CommandResult.ShowUsage();
            }

            string name;

            if (args[0].Equals(name = "*console*"))
            {
                #region TestingStuff
                // I was testing some stuff
                /*if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = args.Join(1);
                    process.StartInfo = startInfo;
                    process.Start();
                }
                else
                {
                    ProcessStartInfo procStartInfo = new ProcessStartInfo("/bin/bash", args.Join(1));
                    procStartInfo.RedirectStandardOutput = true;
                    procStartInfo.UseShellExecute = false;
                    procStartInfo.CreateNoWindow = true;

                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo = procStartInfo;
                    proc.Start();
                }*/
                #endregion
                // Execute by console
                R.Commands.Execute(null, args.Join(1));

            }
            else if (args[0].Equals("*"))
            {
                UServer.Players.ForEach(p => {
                    ChatManager.instance.askChat(p.CSteamId, (byte)EChatMode.GLOBAL, args.Join(1));
                });

                name = "Everyone";
            }
            else
            {
                if (!args[0].IsValidPlayerIdentifier)
                {
                    return CommandResult.LangError("PLAYER_NOT_FOUND", args[0]);
                }

                var targetPlayer = args[0].ToPlayer;

                ChatManager.instance.askChat(targetPlayer.CSteamId, (byte)EChatMode.GLOBAL, args.Join(1));

                name = targetPlayer.CharacterName;
            }

            EssLang.Send(src, "SUDO_EXECUTED", name, args.Join(1));

            return CommandResult.Success();
        }

    }

}