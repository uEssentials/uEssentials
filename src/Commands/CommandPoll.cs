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

        public override void OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            if ( parameters.IsEmpty )
            {
                ShowUsage( source );
            }
            else
            {
                switch (parameters[0].ToString().ToLower())
                {
                    case "start":
                        if ( source.HasPermission( "essentials.commands.poll.start" ) )
                        {
                            if ( parameters.Length < 4 )
                            {
                                source.SendMessage( "/poll start [name] [duration] [description]" );
                                return;
                            }

                            var pollName = parameters[1].ToString();

                            lock ( Polls )
                            {
                                if ( Polls.ContainsKey( pollName ) )
                                {
                                    EssLang.POLL_NAME_IN_USE.SendTo( source );
                                    return;
                                }
                            }

                            var pollDescription = parameters.Join( 3 );

                            if ( parameters[2].IsInt )
                            {
                                var poll = new Poll( 
                                    pollName, 
                                    pollDescription,
                                    parameters[2].ToInt 
                                );

                                poll.Start();
                            }
                            else
                            {
                                EssLang.INVALID_NUMBER.SendTo( source, parameters[2] );
                            }
                        }
                        else
                        {
                            EssLang.COMMAND_NO_PERMISSION.SendTo( source );
                        }
                        break;

                    case "stop":
                        if ( source.HasPermission( "essentials.commands.poll.stop" ) )
                        {
                            if ( parameters.Length < 2 )
                            {
                                source.SendMessage( "/poll stop [name]" );
                                return;
                            }

                            var pollName = parameters[1].ToString();

                            if ( !PollExists( pollName, source ) ) return;

                            lock ( Polls )
                            {
                                Polls[pollName].Stop();
                            }
                        }
                        else
                        {
                            EssLang.COMMAND_NO_PERMISSION.SendTo( source );
                        }
                        break;

                    case "list":
                        if ( source.HasPermission( "essentials.commands.poll.info" ) )
                        {
                            lock ( Polls )
                            {
                                if ( !Polls.Any() )
                                {
                                    EssLang.POLL_NONE.SendTo( source );
                                    return;
                                }

                                lock ( Polls )
                                {
                                    EssLang.POLL_LIST.SendTo( source );

                                    foreach ( var poll in Polls.Values )
                                    {
                                        EssLang.POLL_LIST_ENTRY.SendTo( 
                                            source,
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
                            EssLang.COMMAND_NO_PERMISSION.SendTo( source );
                        }                        
                        break;

                    case "info":
                        if ( source.HasPermission( "essentials.commands.poll.info" ) )
                        {
                            lock ( Polls )
                            {
                                if ( !Polls.Any() )
                                {
                                    EssLang.POLL_NONE.SendTo( source );
                                    return;
                                }

                                if ( parameters.Length < 2 )
                                {
                                    source.SendMessage( "Use /poll info [poll_name]");
                                }
                                else
                                {
                                    var pollName = parameters[1].ToString();

                                    if ( !PollExists( pollName, source ) ) return;

                                    var poll = Polls[pollName];

                                    EssLang.POLL_INFO.SendTo( source, pollName );

                                    EssLang.POLL_LIST_ENTRY.SendTo( 
                                        source, 
                                        pollName, 
                                        poll.Description, 
                                        poll.YesVotes, 
                                        poll.NoVotes 
                                    );
                                }
                            }
                        }
                        else
                        {
                            EssLang.COMMAND_NO_PERMISSION.SendTo( source );
                        }
                        break;

                    default:
                        ShowUsage( source );
                        break;
                }
            }
        }

        /// <summary>
        /// An struct who represent an Poll
        /// </summary>
        public struct Poll
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

                lock ( Polls ) Polls.Add( Name, this );

                var thiz = this;

                if ( Duration > 0 )
                {
                    Tasks.New( task =>
                    {
                        lock ( Polls )
                        {
                            Console.WriteLine( "dsdsdsd" );
                            if ( !Polls.ContainsKey( thiz.Name ) ) return;

                            thiz.Stop();
                        }
                    }).Delay( Duration * 1000 ).Go();   
                }

                if ( !EssProvider.Config.EnablePollRunningMessage ) return;

                var interval = EssProvider.Config.PollRunningMessageCooldown * 1000;

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
