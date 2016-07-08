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

using Essentials.Common;
using Essentials.Common.Reflect;
using Rocket.Core;
using System.Linq;
using System.Reflection;
using Essentials.Api;
using Essentials.Api.Unturned;
using Essentials.Economy;

namespace Essentials.Compatibility.Hooks {

    public class UconomyHook : Hook, IEconomyProvider {

        public string Currency => UEssentials.Config.Economy.UconomyCurrency;

        private MethodAccessor<decimal> _getBalanceMethod;
        private MethodAccessor<decimal> _increaseBalanceMethod;

        public UconomyHook() : base("economy") {}

        public override void OnLoad() {
            var uconomyPlugin = R.Plugins.GetPlugins().FirstOrDefault(c => c.Name.EqualsIgnoreCase("uconomy"));
            var uconomyType = uconomyPlugin.GetType().Assembly.GetType("fr34kyn01535.Uconomy.Uconomy");
            var uconomyInstance =
                uconomyType.GetField("Instance", BindingFlags.Static | BindingFlags.Public).GetValue(uconomyPlugin);

            var databaseInstance = uconomyInstance.GetType().GetField("Database").GetValue(uconomyInstance);
            _getBalanceMethod = AccessorFactory.AccessMethod<decimal>(databaseInstance, "GetBalance");
            _increaseBalanceMethod = AccessorFactory.AccessMethod<decimal>(databaseInstance, "IncreaseBalance");
        }

        public override void OnUnload() {}

        public override bool CanBeLoaded() {
            return R.Plugins.GetPlugins().Any(c => c.Name.EqualsIgnoreCase("uconomy"));
        }

        public decimal Withdraw(UPlayer player, decimal amount) {
            return _increaseBalanceMethod.Invoke(player.CSteamId.m_SteamID.ToString(), -amount);
        }

        public decimal Deposit(UPlayer player, decimal amount) {
            return _increaseBalanceMethod.Invoke(player.CSteamId.m_SteamID.ToString(), amount);
        }

        public decimal GetBalance(UPlayer player) {
            return _getBalanceMethod.Invoke(player.CSteamId.m_SteamID.ToString());
        }

        public bool Has(UPlayer player, decimal amount) {
            return (GetBalance(player) - amount) >= 0;
        }

    }

}