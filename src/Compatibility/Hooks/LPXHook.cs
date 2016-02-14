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
using System.Collections.Generic;
using System.Linq;
using Essentials.Api;
using Rocket.API;
using Rocket.API.Serialisation;
using Rocket.Core;

namespace Essentials.Compatibility.Hooks
{
    internal class LPXHook : Hook
    {
        private IRocketPermissionsProvider _defaultProvider;

        public LPXHook() : base( "lpx" ) {}

        public override void OnLoad()
        {
            var logger = EssProvider.Logger;

            logger.LogInfo( "Hooking with LPX..." );
            var lpx = R.Plugins.GetPlugins().First( c => c.Name.Equals( "LPX" ) );
            var sqlPerm = lpx.GetType().Assembly.GetType( "LIGHT.SQLPermission" );

            var sqlPermInst = new WrappedLPXSQLPermission( 
                (IRocketPermissionsProvider) Activator.CreateInstance( sqlPerm ) 
            );

            _defaultProvider = R.Permissions;

            R.Permissions = sqlPermInst;
            logger.LogInfo( "Successfully hooked with LPX." );
        }

        public override void OnUnload()
        {
            R.Permissions = _defaultProvider;
        }

        public override bool CanBeLoaded()
        {
            return R.Plugins.GetPlugins().Any( c => c.Name.Equals( "LPX" ) );
        }
    }

    internal class WrappedLPXSQLPermission : IRocketPermissionsProvider
    {
        private readonly IRocketPermissionsProvider _lpxSqlPermission;

        public WrappedLPXSQLPermission( IRocketPermissionsProvider lpxSqlPermisssions )
        {
            _lpxSqlPermission = lpxSqlPermisssions;
        }

        public bool HasPermission( IRocketPlayer player, string requestedPermission, bool defaultReturnValue = false )
        {
            return _lpxSqlPermission.HasPermission( player, requestedPermission, defaultReturnValue );
        }

        public bool HasPermission( IRocketPlayer player, string requestedPermission, out uint? cooldownLeft, bool defaultReturnValue = false )
        {
            var essCommand = EssProvider.CommandManager.GetByName( requestedPermission );

            if ( essCommand != null )
            {
                return _lpxSqlPermission.HasPermission( player, essCommand.Permission, out cooldownLeft, defaultReturnValue );
            }

            if ( !EssProvider.CommandManager.HasWithName( requestedPermission ) )
            {
                cooldownLeft = 0;
                return true;
            }

            return _lpxSqlPermission.HasPermission( player, requestedPermission, out cooldownLeft, defaultReturnValue );
        }

        public List<RocketPermissionsGroup> GetGroups( IRocketPlayer player, bool includeParentGroups )
        {
            return _lpxSqlPermission.GetGroups( player, includeParentGroups );
        }

        public List<Permission> GetPermissions( IRocketPlayer player )
        {
            return _lpxSqlPermission.GetPermissions( player );
        }

        public bool SetGroup( IRocketPlayer player, string groupID )
        {
            return _lpxSqlPermission.SetGroup( player, groupID );
        }

        public void Reload() {}
    }
}