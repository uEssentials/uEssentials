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

using System.Linq;
using Essentials.Api.Command;
using Essentials.Api.Unturned;

namespace Essentials.Core.Command
{
    ///<summary>
    /// Default implementation of ICommandArgs
    ///</summary>
    internal class CommandArgs : ICommandArgs
    {
        public string[] RawArguments { get; }
        public ICommandArgument[] Arguments { get; }
        public int Length => RawArguments.Length;
        public bool IsEmpty => Length == 0;

        public CommandArgs(string[] rawArgs)
        {
            RawArguments = rawArgs;
            var arguments = new ICommandArgument[Length];

            for (var i = 0; i < RawArguments.Length; i++)
            {
                arguments[i] = new CommandArgument(i, RawArguments[i]);
            }

            Arguments = arguments;
        }

        public ICommandArgument this[int argumentIndex]
        {
            get => Arguments[argumentIndex];
        }

        public string Join(int initialIndex)
        {
            return string.Join(" ", Arguments.Skip(initialIndex)
                .Select(arg => arg.ToString())
                .ToArray());
        }

        public string Join(int startIndex, int endIndex, string separator)
        {
            return string.Join(separator, Arguments.Skip(startIndex)
                .Select(arg => arg.ToString())
                .ToArray());
        }
    }

    ///<summary>
    /// Default implementation of ICommandArgument
    ///</summary>
    internal class CommandArgument : ICommandArgument
    {
        internal CommandArgument(int index, string rawValue)
        {
            Index = index;
            RawValue = rawValue;
        }

        public int Index { get; }

        public string RawValue { get; }

        public ulong ToULong => ulong.Parse(RawValue);

        public long ToLong => long.Parse(RawValue);

        public int ToInt => int.Parse(RawValue);

        public bool ToBool => bool.Parse(RawValue);

        public double ToDouble => double.Parse(RawValue);

        public float ToFloat => float.Parse(RawValue);

        public string ToLowerString => ToString().ToLowerInvariant();

        public string ToUpperString => ToString().ToUpperInvariant();

        public uint ToUInt => uint.Parse(RawValue);

        public short ToShort => short.Parse(RawValue);

        public ushort ToUShort => ushort.Parse(RawValue);

        public UPlayer ToPlayer
        {
            get
            {
                if (ulong.TryParse(RawValue, out var id))
                {
                    var player = UPlayer.From(id);
                    if (player != null)
                    {
                        return player;
                    }
                }

                return UPlayer.From(RawValue);
            }
        }

        public bool IsBool
        {
            get { return bool.TryParse(RawValue, out _); }
        }

        public bool IsString
        {
            get
            {
                var c = RawValue[0];
                return c != '-' && (c < '0' || c > '9');
            }
        }

        public bool IsValidPlayerIdentifier
        {
            get
            {
                // Steam 64 id
                if (ulong.TryParse(RawValue, out var id))
                {
                    return UPlayer.From(id) != null;
                }

                // Player name
                return UPlayer.From(RawValue) != null;
            }
        }

        public bool IsLong
        {
            get { return long.TryParse(RawValue, out _); }
        }

        public bool IsULong
        {
            get { return ulong.TryParse(RawValue, out _); }
        }

        public bool IsInt
        {
            get { return int.TryParse(RawValue, out _); }
        }

        public bool IsDouble
        {
            get { return double.TryParse(RawValue, out _); }
        }

        public bool IsFloat
        {
            get { return float.TryParse(RawValue, out _); }
        }

        public bool IsUInt
        {
            get { return uint.TryParse(RawValue, out _); }
        }

        public bool IsShort
        {
            get { return short.TryParse(RawValue, out _); }
        }

        public bool IsUShort
        {
            get { return ushort.TryParse(RawValue, out _); }
        }

        public override string ToString()
        {
            return RawValue;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is string)
            {
                return this.RawValue == (string) obj;
            }

            if (obj is ICommandArgument)
            {
                return Equals((ICommandArgument) obj);
            }

            return false;
        }

        public bool Equals(ICommandArgument other)
        {
            return other.RawValue == this.RawValue;
        }

        public override int GetHashCode()
        {
            return RawValue.GetHashCode();
        }
    }
}