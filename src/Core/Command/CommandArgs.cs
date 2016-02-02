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

using System;
using System.Linq;
using Essentials.Api.Command;
using Essentials.Api.Unturned;
// ReSharper disable ConvertPropertyToExpressionBody

namespace Essentials.Core.Command
{
    ///<summary>
    /// Default implementation of ICommandArgs
    /// Author: Leonardosc
    ///</summary>
    internal class CommandArgs : ICommandArgs
    {
        public CommandArgs( string[] rawArgs )
        {
            RawArguments = rawArgs;

            var arguments = new ICommandArgument[ Length ];

            for ( var i = 0; i < rawArgs.Length; i++ )
            {
                arguments[i] = new CommandArgument( i, rawArgs[i] );
            }

            Arguments = arguments;
        }

        public string[] RawArguments { get; }

        public ICommandArgument[] Arguments { get; }

        public int Length => RawArguments.Length;

        public bool IsEmpty => Length == 0;

        public ICommandArgument this[ int argumentIndex ] =>
            Arguments[argumentIndex];

        public string GetJoinedArguments( int initialIndex )
        {
            return string.Join( " ", RawArguments.Skip( initialIndex ).ToArray() );
        }
    }

    ///<summary>
    /// Default implementation of ICommandArgument
    /// Author: Leonardosc
    ///</summary>
    public class CommandArgument : ICommandArgument
    {
        public int Index { get; }

        public string RawValue { get; }

        public int ToInt => int.Parse( RawValue );

        public bool ToBool => bool.Parse( RawValue );

        public double ToDouble => double.Parse( RawValue );

        public UPlayer ToPlayer => UPlayer.From( RawValue );

        public string ToLowerString => ToString().ToLower();

        public string ToUpperString => ToString().ToLower();

        public uint ToUint => uint.Parse( RawValue );

        public short ToShort => short.Parse( RawValue );

        public ushort ToUshort => ushort.Parse( RawValue );

        public bool IsBool
        {
            get
            {
                bool unused;
                return bool.TryParse( RawValue, out unused );
            }
        }

        public bool IsString
        {
            get
            {
                return !IsBool && !IsDouble && !IsInt;
            }
        }

        public bool IsValidPlayerName
        {
            get
            {
                return UPlayer.From( RawValue ) != null;
            }
        }

        public bool IsInt
        {
            get
            {
                int unused;
                return int.TryParse( RawValue, out unused );
            }
        }

        public bool IsDouble
        {
            get
            {
                double unused;
                return double.TryParse( RawValue, out unused );
            }
        }

        public bool IsUint
        {
            get
            {
                uint unused;
                return uint.TryParse( RawValue, out unused );
            }
        }

        public bool IsShort
        {
            get
            {
                short unused;
                return short.TryParse( RawValue, out unused );
            }
        }

        public bool IsUshort
        {
            get
            {
                ushort unused;
                return ushort.TryParse( RawValue, out unused );
            }
        }

        public CommandArgument( int index, string rawValue )
        {
            Index = index;
            RawValue = rawValue;
        }

        public bool Is( int other )
        {
            if ( !IsInt )
                return false;

            return ToInt == other;
        }

        public bool Is( double other )
        {
            if ( !IsDouble )
                return false;

            return Math.Abs( ToDouble - other ) < 0.05;
        }

        public bool Is( string other, bool ignoreCase = true )
        {
            if ( !IsString )
                return false;

            return string.Compare( 
                ToString(),
                other, 
                StringComparison.OrdinalIgnoreCase 
            ) == 0;
        }

        public bool Is( UPlayer other )
        {
            if ( !IsValidPlayerName )
                return false;

            return ToPlayer == other;
        }

        public bool IsOneOf( string[] others, bool ignoreCase = true )
        {
            return others.Any( other => Is( other, ignoreCase ) );
        }

        public override string ToString()
        {
            return RawValue;
        }
    }
}
