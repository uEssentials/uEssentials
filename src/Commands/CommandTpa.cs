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

using System;
using System.Collections.Generic;
using System.Linq;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Event;
using Essentials.Api.Task;
using Essentials.Api.Unturned;
using Essentials.Core;
using Essentials.Event.Handling;
using Essentials.I18n;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using EventType = Essentials.Api.Event.EventType;

namespace Essentials.Commands {

    // TODO: Improve
    // Multiple requests

    [CommandInfo(
        Name = "tpa",
        Usage = "[player/accept/deny/cancel]",
        AllowedSource = AllowedSource.PLAYER,
        MinArgs = 1
    )]
    public class CommandTpa : EssCommand {

        private static Dictionary<ulong, ulong> _requests = new Dictionary<ulong, ulong>();
        private static Dictionary<ulong, Task> _waitingToTeleport = new Dictionary<ulong, Task>();

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            bool lol;
            
            var player = src.ToPlayer();
            var senderId = player.CSteamId.m_SteamID;

            switch (args[0].ToLowerString) {
                case "accept":
                case "a": {
                    if (!player.HasPermission($"{Permission}.accept")) {
                        return CommandResult.NoPermission($"{Permission}.accept");
                    }

                    if (!_requests.ContainsValue(senderId)) {
                        return CommandResult.LangError("TPA_NONE");
                    }

                    var whoSentId = _requests.Keys.FirstOrDefault(k => _requests[k] == senderId);
                    var whoSent = UPlayer.From(new Steamworks.CSteamID(whoSentId));

                    if (whoSent == null) {
                        return CommandResult.LangError("TPA_NONE");
                    }

                    if (whoSent.Stance == EPlayerStance.DRIVING ||
                        whoSent.Stance == EPlayerStance.SITTING) {
                        EssLang.Send(whoSent, "CANNOT_TELEPORT_DRIVING");
                        return CommandResult.LangError("TPA_CANNOT_TELEPORT", whoSent.DisplayName);
                    }

                    EssLang.Send(src, "TPA_ACCEPTED_SENDER", whoSent.DisplayName);
                    EssLang.Send(whoSent, "TPA_ACCEPTED", src.DisplayName);
                    _requests.Remove(whoSentId);

                    var tpaSettings = EssCore.Instance.Config.Tpa;

                    if (tpaSettings.TeleportDelay > 0) {
                        var task = Task.Create()
                            .Id("Tpa Teleport")
                            .Action(() => {
                                _waitingToTeleport.Remove(player.CSteamId.m_SteamID);
                                if (whoSent.IsOnline && player.IsOnline) {
                                    whoSent.Teleport(player.Position + new Vector3(0f, 0.5f, 0f));
                                }
                            })
                            .Delay(TimeSpan.FromSeconds(tpaSettings.TeleportDelay))
                            .Submit();
                        _waitingToTeleport[player.CSteamId.m_SteamID] = task;
                    } else {
                        whoSent.Teleport(player.Position + new Vector3(0f, 0.5f, 0f));
                    }
                    break;
                }

                case "deny":
                case "d": {
                    if (!player.HasPermission($"{Permission}.deny")) {
                        return CommandResult.NoPermission($"{Permission}.deny");
                    }

                    if (!_requests.ContainsValue(senderId)) {
                        return CommandResult.LangError("TPA_NONE");
                    }

                    var whoSentId = _requests.Keys.FirstOrDefault(k => _requests[k] == senderId);
                    var whoSent = UPlayer.From(new Steamworks.CSteamID(whoSentId));

                    if (whoSent != null) {
                        EssLang.Send(whoSent, "TPA_DENIED", src.DisplayName);
                    }

                    EssLang.Send(src, "TPA_DENIED_SENDER", whoSent == null ? "Unknown" : whoSent.DisplayName);
                    _requests.Remove(whoSentId);
                    break;
                }

                case "cancel":
                case "c": {
                    if (!player.HasPermission($"{Permission}.cancel")) {
                        return CommandResult.NoPermission($"{Permission}.cancel");
                    }

                    if (!_requests.ContainsKey(senderId)) {
                        return CommandResult.LangError("TPA_NONE");
                    }

                    _requests.Remove(senderId);
                    EssLang.Send(src, "TPA_CANCELLED");
                    break;
                }

                // Send TPA to someone
                default: {
                    if (!player.HasPermission($"{Permission}.send")) {
                        return CommandResult.NoPermission($"{Permission}.send");
                    }

                    if (!args[0].IsValidPlayerIdentifier) {
                        return CommandResult.LangError("PLAYER_NOT_FOUND", args[0]);
                    }

                    var target = args[0].ToPlayer;

                    // Cancel the previous request if the player send a new request.
                    // We currently does not support multiple requests.
                    if (_requests.TryGetValue(senderId, out var value)) {
                        // Avoid 'flooding' requests to the same player
                        if (value == target.CSteamId.m_SteamID) {
                            return CommandResult.LangError("TPA_ALREADY_SENT", target.DisplayName);
                        }
                        _requests.Remove(senderId);
                    }

#if !DEV
                    if (target == player) {
                        return CommandResult.LangError("TPA_YOURSELF");
                    }
#endif

                    _requests.Add(senderId, target.CSteamId.m_SteamID);
                    EssLang.Send(src, "TPA_SENT_SENDER", target.DisplayName);
                    EssLang.Send(target, "TPA_SENT", src.DisplayName);

                    var tpaSettings = EssCore.Instance.Config.Tpa;

                    if (tpaSettings.ExpireDelay > 0) {
                        Task.Create()
                            .Id("Tpa Expire")
                            .Action(() => _requests.Remove(senderId))
                            .Delay(TimeSpan.FromSeconds(tpaSettings.ExpireDelay))
                            .Submit();
                    }
                    break;
                }
            }

            return CommandResult.Success();
        }

        protected override void OnUnregistered() {
            UEssentials.EventManager.Unregister<EssentialsEventHandler>("TpaPlayerDisconnect");
            UEssentials.EventManager.Unregister<EssentialsEventHandler>("TpaPlayerMove");
        }

        [SubscribeEvent(EventType.PLAYER_DISCONNECTED)]
        private void TpaPlayerDisconnect(UnturnedPlayer player) {
            var playerId = player.CSteamID.m_SteamID;

            if (_requests.ContainsKey(playerId)) {
                _requests.Remove(playerId);
            } else if (_requests.ContainsValue(playerId)) {
                var val = _requests.Keys.FirstOrDefault(k => _requests[k] == playerId);

                if (val != default(ulong)) {
                    _requests.Remove(val);
                }
            }
        }

        [SubscribeEvent(EventType.PLAYER_UPDATE_POSITION)]
        private void TpaPlayerMove(UnturnedPlayer player, Vector3 newPosition) {
            if (
                UEssentials.Config.Tpa.TeleportDelay > 0 &&
                UEssentials.Config.Tpa.CancelTeleportWhenMove &&
                _waitingToTeleport.TryGetValue(player.CSteamID.m_SteamID, out var task)
            ) {
                task.Cancel();
                _waitingToTeleport.Remove(player.CSteamID.m_SteamID);
                UPlayer.TryGet(player, p => EssLang.Send(p, "TELEPORT_CANCELLED_MOVED"));
            }
        }
    }

}
