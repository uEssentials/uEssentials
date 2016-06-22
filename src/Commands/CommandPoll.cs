/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  Leonardosc
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

using System.Collections.Generic;
using System.Linq;
using Essentials.I18n;
using System;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Task;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "poll",
        Description = "Start/Stop an poll",
        Usage = "[start/stop/list/info]"
    )]
    public class CommandPoll : EssCommand
    {
        /// <summary>
        /// List of current polls
        /// </summary>
        public static Dictionary<string, Poll> Polls = new Dictionary<string, Poll>();

        /// <summary>
        /// Check an poll with passed name exists, if not, will send POLL_NOT_EXIST message.
        /// </summary>
        internal static readonly Func<string, ICommandSource, bool> PollExists = (pollName, source)=>
        {
            lock ( Polls )
            {
                if ( Polls.ContainsKey( pollName ) ) return true;
                EssLang.POLL_NOT_EXIST.SendTo( source );
                return false;
            }
        };

        public override CommandResult OnExecute( ICommandSource src, ICommandArgs args )
        {
            if ( args.IsEmpty )
            {
                return CommandResult.ShowUsage();
            }
            else
            {
                switch (args[0].ToString().ToLower())
                {
                    case "start":
                        if ( src.HasPermission( "essentials.command.poll.start" ) )
                        {
                            if ( args.Length < 4 )
                            {
                                return CommandResult.InvalidArgs( "/poll start [name] [duration] [description]" );
                            }

                            var pollName = args[1].ToString();

                            lock ( Polls )
                            {
                                if ( Polls.ContainsKey( pollName ) )
                                {
                                    return CommandResult.Lang( EssLang.POLL_NAME_IN_USE );
                                }
                            }

                            var pollDescription = args.Join( 3 );

                            if ( args[2].IsInt )
                            {
                                var poll = new Poll(
                                    pollName,
                                    pollDescription,
                                    args[2].ToInt
                                );

                                poll.Start();
                            }
                            else
                            {
                                return CommandResult.Lang( EssLang.INVALID_NUMBER, args[2] );
                            }
                        }
                        else
                        {
                            return CommandResult.Lang( EssLang.COMMAND_NO_PERMISSION );
                        }
                        break;

                    case "stop":
                        if ( src.HasPermission( "essentials.command.poll.stop" ) )
                        {
                            if ( args.Length < 2 )
                            {
                                return CommandResult.InvalidArgs( "/poll stop [name]" );
                            }

                            var pollName = args[1].ToString();

                            if ( !PollExists( pollName, src ) )
                            {
                                return CommandResult.Empty();
                            }

                            lock ( Polls )
                            {
                                Polls[pollName].Stop();
                            }
                        }
                        else
                        {
                            return CommandResult.Lang( EssLang.COMMAND_NO_PERMISSION );
                        }
                        break;

                    case "list":
                        if ( src.HasPermission( "essentials.command.poll.info" ) )
                        {
                            lock ( Polls )
                            {
                                if ( !Polls.Any() )
                                {
                                    return CommandResult.Lang( EssLang.POLL_NONE );
                                }

                                lock ( Polls )
                                {
                                    EssLang.POLL_LIST.SendTo( src );

                                    foreach ( var poll in Polls.Values )
                                    {
                                        EssLang.POLL_LIST_ENTRY.SendTo(
                                            src,
                                            poll.Name,
                                            poll.Description,
                                            poll.YesVotes,
                                            poll.NoVotes
                                        );
                                    }
                                }
                            }
                        }
                        else
                        {
                            return CommandResult.Lang( EssLang.COMMAND_NO_PERMISSION );
                        }
                        break;

                    case "info":
                        if ( src.HasPermission( "essentials.command.poll.info" ) )
                        {
                            lock ( Polls )
                            {
                                if ( !Polls.Any() )
                                {
                                    return CommandResult.Lang( EssLang.POLL_NONE );
                                }

                                if ( args.Length < 2 )
                                {
                                    return CommandResult.InvalidArgs( "Use /poll info [poll_name]" );
                                }

                                var pollName = args[1].ToString();

                                if ( !PollExists( pollName, src ) )
                                {
                                    return CommandResult.Empty();
                                }

                                var poll = Polls[pollName];

                                EssLang.POLL_INFO.SendTo( src, pollName );

                                EssLang.POLL_LIST_ENTRY.SendTo(
                                    src,
                                    pollName,
                                    poll.Description,
                                    poll.YesVotes,
                                    poll.NoVotes
                                );
                            }
                        }
                        else
                        {
                            return CommandResult.Lang( EssLang.COMMAND_NO_PERMISSION );
                        }
                        break;

                    default:
                        return CommandResult.ShowUsage();
                }
            }

            return CommandResult.Success();
        }

        public class Poll
        {
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

            public Poll( string name, string description, int duration )
            {
                Voted = new List<string>();
                Name = name;
                Description = description;
                YesVotes = NoVotes = 0;
                Duration = duration;
            }

            /// <summary>
            /// Start the poll
            /// </summary>
            public void Start()
            {
                EssLang.POLL_STARTED.Broadcast( Name, Description );

                var thiz = this;

                lock ( Polls ) Polls.Add( Name, thiz );

                if ( Duration > 0 )
                {
                    Tasks.New( task =>
                    {
                        lock ( Polls )
                        {
                            if ( !Polls.ContainsKey( thiz.Name ) ) return;

                            thiz.Stop();
                        }
                    }).Delay( Duration * 1000 ).Go();
                }

                if ( !UEssentials.Config.EnablePollRunningMessage ) return;

                var interval = UEssentials.Config.PollRunningMessageCooldown * 1000;

                Tasks.New( task =>
                {
                    lock ( Polls )
                    {
                        if ( !Polls.ContainsKey( thiz.Name ) )
                        {
                            task.Cancel();
                            return;
                        }

                        EssLang.POLL_RUNNING.Broadcast(
                            thiz.Name,
                            thiz.Description,
                            thiz.YesVotes,
                            thiz.NoVotes
                        );
                    }

                }).Interval( interval ).Delay( interval ).Go();
            }

            /// <summary>
            /// Stop the poll
            /// </summary>
            public void Stop()
            {
                EssLang.POLL_STOPPED.Broadcast( Name, Description, YesVotes, NoVotes );

                lock ( Polls ) Polls.Remove( Name );
            }
        }
    }
}
