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

using Essentials.I18n;
using System.Linq;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "kits",
        Description = "View available kits"
    )]
    public class CommandKits : EssCommand
    {
        public override void OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            var kits = ( 
                from kit in EssProvider.KitManager.Kits
                where kit.CanUse( source )
                select kit.Name 
            ).ToList();

            if ( kits.Count == 0 )
                EssLang.KIT_NONE.SendTo( source );
            else
                EssLang.KIT_LIST.SendTo( source, string.Join( ", ", kits.ToArray() ) );
        }
    }
}
