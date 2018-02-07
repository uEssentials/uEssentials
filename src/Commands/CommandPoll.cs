#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2017  leonardosnt
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

using System.Collections.Generic;
using System.Linq;
using Essentials.I18n;
using System;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Task;
using Essentials.Common;

namespace Essentials.Commands {

    [CommandInfo(
        Name = "poll",
        Description = "Start/Stop a poll",
        Usage = "[start | stop | list | info]"
    )]
    public class CommandPoll : EssCommand {

        /// <summary>
        /// List of current polls
        /// </summary>
        public static Dictionary<string, Poll> Polls = new Dictionary<string, Poll>();

        /// <summary>
        /// Check a poll with passed name exists, if not, will send POLL_NOT_EXIST message.
        /// </summary>
        internal static readonly Func<string, ICommandSource, bool> PollExists = (pollName, source) => {
            lock (Polls) {
                if (Polls.ContainsKey(pollName)) return true;
                EssLang.Send(source, "POLL_NOT_EXIST");
                return false;
            }
        };

        private static readonly string[] _validInputs = { "start", "stop", "list", "info" };

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            if (args.IsEmpty || _validInputs.None(i => i.EqualsIgnoreCase(args[0].ToString())) ) {
                return CommandResult.ShowUsage();
            }
            if (!src.HasPermission($"{Permission}.{args[0]}")) {
                return CommandResult.NoPermission($"{Permission}.{args[0]}");
            }
            switch (args[0].ToString().ToLower()) {
                case "start": {
                    if (args.Length < 4) {
                        return CommandResult.InvalidArgs("/poll start [name] [duration] [description]");
                    }

                    var pollName = args[1].ToString();

                    lock (Polls) {
                        if (Polls.ContainsKey(pollName)) {
                            return CommandResult.LangError("POLL_NAME_IN_USE");
                        }
                    }

                    var pollDescription = args.Join(3);

                    if (args[2].IsInt) {
                        var poll = new Poll(
                            pollName,
                            pollDescription,
                            args[2].ToInt
                            );

                        poll.Start();
                    } else {
                        return CommandResult.LangError("INVALID_NUMBER", args[2]);
                    }
                    break;
                }

                case "stop": {
                    if (args.Length < 2) {
                        return CommandResult.InvalidArgs("/poll stop [name]");
                    }

                    var pollName = args[1].ToString();

                    if (!PollExists(pollName, src)) {
                        return CommandResult.Empty();
                    }

                    lock (Polls) {
                        Polls[pollName].Stop();
                    }
                    break;
                }

                case "list": {
                    lock (Polls) {
                        if (!Polls.Any()) {
                            return CommandResult.LangError("POLL_NONE");
                        }

                        EssLang.Send(src, "POLL_LIST");

                        foreach (var poll in Polls.Values) {
                            EssLang.Send(src,
                                "POLL_LIST_ENTRY",
                                poll.Name,
                                poll.Description,
                                poll.YesVotes,
                                poll.NoVotes
                             );
                        }
                    }
                    break;
                }

                case "info": {
                    lock (Polls) {
                        if (!Polls.Any()) {
                            return CommandResult.LangError("POLL_NONE");
                        }

                        if (args.Length < 2) {
                            return CommandResult.InvalidArgs("Use /poll info [poll_name]");
                        }

                        var pollName = args[1].ToString();

                        if (!PollExists(pollName, src)) {
                            return CommandResult.Empty();
                        }

                        var poll = Polls[pollName];

                        EssLang.Send(src, "POLL_INFO", pollName);

                        EssLang.Send(
                            src,
                            "POLL_LIST_ENTRY",
                            pollName,
                            poll.Description,
                            poll.YesVotes,
                            poll.NoVotes
                         );
                    }
                    break;
                }
            }

            return CommandResult.Success();
        }

        public class Poll {

            /// <summary>
            /// List of player who voted
            /// </summary>
            public List<string> Voted { get; }

            /// <summary>
            /// The poll's name
            /// </summary>
            public string Name;

            /// <summary>
            /// The poll's name
            /// </summary>
            public string Description;

            /// <summary>
            /// Yes vote count
            /// </summary>
            public int YesVotes { get; set; }

            /// <summary>
            /// No vote count
            /// </summary>
            public int NoVotes { get; set; }

            /// <summary>
            /// The poll's duration in seconds <para />
            /// Use -1 for infinite
            /// </summary>
            public int Duration { get; set; }

            public Poll(string name, string description, int duration) {
                Voted = new List<string>();
                Name = name;
                Description = description;
                YesVotes = NoVotes = 0;
                Duration = duration;
            }

            /// <summary>
            /// Start the poll
            /// </summary>
            public void Start() {
                EssLang.Broadcast("POLL_STARTED", Name, Description);

                var thiz = this;

                lock (Polls) Polls.Add(Name, thiz);

                if (Duration > 0) {
                    Task.Create()
                        .Id("Poll Stop")
                        .Delay(TimeSpan.FromSeconds(Duration))
                        .Action(() => {
                            lock (Polls) {
                                if (!Polls.ContainsKey(thiz.Name)) return;
                                thiz.Stop();
                            }
                        })
                        .Submit();
                }

                if (!UEssentials.Config.EnablePollRunningMessage) return;

                var interval = UEssentials.Config.PollRunningMessageCooldown*1000;

                Task.Create()
                    .Id("Poll Running Message")
                    .Interval(interval)
                    .UseIntervalAsDelay()
                    .Action(task => {
                        lock (Polls) {
                            if (!Polls.ContainsKey(thiz.Name)) {
                                task.Cancel();
                                return;
                            }

                            EssLang.Broadcast("POLL_RUNNING", thiz.Name, thiz.Description,
                                thiz.YesVotes, thiz.NoVotes);
                        }
                    })
                    .Submit();
            }

            /// <summary>
            /// Stop the poll
            /// </summary>
            public void Stop() {
                EssLang.Broadcast("POLL_STOPPED", Name, Description, YesVotes, NoVotes);

                lock (Polls) Polls.Remove(Name);
            }

        }

    }

}