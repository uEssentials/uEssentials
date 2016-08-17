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

using System.Linq;
using Essentials.Api.Command;
using Essentials.Api.Unturned;

namespace Essentials.Core.Command {

    ///<summary>
    /// Default implementation of ICommandArgs
    ///</summary>
    internal class CommandArgs : ICommandArgs {

        public string[] RawArguments { get; }
        public ICommandArgument[] Arguments { get; }
        public int Length => RawArguments.Length;
        public bool IsEmpty => Length == 0;

        public CommandArgs(string[] rawArgs) {
            /*RawArguments = new string[0];

            if ( rawArgs.Length < 0 )
                return;

            var args = new List<string>();
            var argBuilder = new StringBuilder();
            var inQuot = false;

            var chars = rawArgs.ToCharArray();
            var charLen = chars.Length;

            for ( var i = 0; i < charLen; i++ )
            {
                var ch = chars[i];

                if ( (ch == '\'' || ch == '"') )
                {
                    if ( i != 0 && chars[i - 1] == '\\' )
                    {
                        argBuilder.Length = argBuilder.Length - 1;
                        goto append;
                    }

                    inQuot = !inQuot;
                    goto appendAll;
                }

                if ( ch == ' ' )
                {
                    if ( inQuot )
                        goto append;
                    goto appendAll;
                }

                append:
                argBuilder.Append( ch );
                if ( i == (charLen - 1) )
                    goto appendAll;
                continue;

                appendAll:
                if ( argBuilder.Length > 0 )
                {
                    args.Add( argBuilder.ToString() );
                    argBuilder.Length = 0;
                }
            }*/

            RawArguments = rawArgs;
            var arguments = new ICommandArgument[Length];

            for (var i = 0; i < RawArguments.Length; i++) {
                arguments[i] = new CommandArgument(i, RawArguments[i]);
            }

            Arguments = arguments;
        }

        public ICommandArgument this[int argumentIndex] {
            get { return Arguments[argumentIndex]; }
        }

        public string Join(int initialIndex) {
            return string.Join(" ", Arguments.Skip(initialIndex)
                .Select(arg => arg.ToString())
                .ToArray());
        }

        public string Join(int startIndex, int endIndex, string separator) {
            return string.Join(separator, Arguments.Skip(startIndex)
                .Select(arg => arg.ToString())
                .ToArray());
        }

    }

    ///<summary>
    /// Default implementation of ICommandArgument
    ///</summary>
    internal class CommandArgument : ICommandArgument {

        internal CommandArgument(int index, string rawValue) {
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

        public UPlayer ToPlayer {
            get {
                ulong id;
                if (ulong.TryParse(RawValue, out id)) {
                    var player = UPlayer.From(id);
                    if (player != null) {
                        return player;
                    }
                }
                return UPlayer.From(RawValue);
            }
        }

        public bool IsBool {
            get {
                bool unused;
                return bool.TryParse(RawValue, out unused);
            }
        }

        public bool IsString {
            get {
                var c = RawValue[0];
                return c != '-' && (c < '0' || c > '9');
            }
        }

        public bool IsValidPlayerIdentifier {
            get {
                // Steam 64 id
                ulong id;
                if (ulong.TryParse(RawValue, out id)) {
                    return UPlayer.From(id) != null;
                }
                // Player name
                return UPlayer.From(RawValue) != null;
            }
        }

        public bool IsLong {
            get {
                long unused;
                return long.TryParse(RawValue, out unused);
            }
        }

        public bool IsULong {
            get {
                ulong unused;
                return ulong.TryParse(RawValue, out unused);
            }
        }

        public bool IsInt {
            get {
                int unused;
                return int.TryParse(RawValue, out unused);
            }
        }

        public bool IsDouble {
            get {
                double unused;
                return double.TryParse(RawValue, out unused);
            }
        }

        public bool IsFloat {
            get {
                float unused;
                return float.TryParse(RawValue, out unused);
            }
        }

        public bool IsUInt {
            get {
                uint unused;
                return uint.TryParse(RawValue, out unused);
            }
        }

        public bool IsShort {
            get {
                short unused;
                return short.TryParse(RawValue, out unused);
            }
        }

        public bool IsUShort {
            get {
                ushort unused;
                return ushort.TryParse(RawValue, out unused);
            }
        }

        public override string ToString() {
            return RawValue;
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (obj is string) {
                return this.RawValue == (string) obj;
            }
            if (obj is ICommandArgument) {
                return Equals((ICommandArgument) obj);
            }
            return false;

        }

        public bool Equals(ICommandArgument other) {
            return other.RawValue == this.RawValue;
        }

        public override int GetHashCode() {
            return RawValue.GetHashCode();
        }

    }

}