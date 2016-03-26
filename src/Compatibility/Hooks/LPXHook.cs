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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Essentials.Api;
using Essentials.Common;
using Essentials.Common.Reflect;
using Rocket.API;
using Rocket.Core;
using Essentials.Core.Permission;
using Rocket.Core.Assets;

namespace Essentials.Compatibility.Hooks
{
    internal class LPXHook : Hook
    {
        private IRocketPermissionsProvider _defaultProvider;

        public LPXHook() : base( "lpx" ) {}

        public override void OnLoad()
        {
            EssProvider.Logger.LogInfo( "Hooking with LPX..." );

            var lpx = R.Plugins.GetPlugins().First( c => c.Name.Equals( "LPX" ) );
            var sqlPerm = lpx.GetType().Assembly.GetType( "LIGHT.SQLPermission" );

            var sqlPermInst = new EssentialsPermissionsProvider( 
                (IRocketPermissionsProvider) Activator.CreateInstance( sqlPerm ) 
            );

            _defaultProvider = R.Permissions;
            R.Permissions = sqlPermInst;

            EssProvider.Logger.LogInfo( "Successfully hooked with LPX." );
        }

        public override void OnUnload()
        {
            R.Permissions = _defaultProvider;
        }

        public override bool CanBeLoaded()
        {
            var lpx = R.Plugins.GetPlugins().First( c => c.Name.Equals( "LPX" ) );

            if ( lpx == null )
            {
                return false;
            }

            var lpxBaseType = lpx.GetType().BaseType;
            var configProp = lpxBaseType?.GetProperty( "Configuration" )?.GetValue( lpx, new object[0] );
            var configInst = configProp?.GetType().GetProperty( "Instance" )?.GetValue( configProp, new object[0] );
            var lpxEnabledField = configInst?.GetType().GetField( "LPXEnabled" )?.GetValue( configInst );

            if ( lpxEnabledField is bool && !(bool) lpxEnabledField )
            {
                return false;
            }

            return true;
        }
    }
}