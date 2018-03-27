#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2018  leonardosnt and contributors
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

using System;
using System.Linq;
using System.Reflection;

using Essentials.Api;
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.Common.Util;
using Essentials.Economy;

using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Player;

namespace Essentials.Compatibility.Hooks {

    public class AviEconomyHook : Hook, IEconomyProvider {

        public string CurrencySymbol => UEssentials.Config.Economy.UconomyCurrency;

        private Type _bankType;
        private MethodInfo _getBalanceMethod;
        private MethodInfo _performPaymentMethod;

        public AviEconomyHook() : base("avi_economy") { }

        public override void OnLoad() {
            UEssentials.Logger.LogInfo("Loading AviEconomy hook...");

            IRocketPlugin economyPlugin = R.Plugins.GetPlugins().FirstOrDefault(c => c.Name.EqualsIgnoreCase("AviEconomy"));

            if (economyPlugin == null)
                throw new Exception("AviEconomy Plugin couldn't be loaded...");

            this._bankType = economyPlugin.GetType().Assembly.GetType("com.aviadmini.rocketmod.AviEconomy.Bank");

            if (this._bankType == null)
              throw new Exception("AviEconomy Bank type couldn't be loaded...");

            this._getBalanceMethod = ReflectUtil.GetMethod(this._bankType, "GetBalance", BindingFlags.Static | BindingFlags.Public,
                new[] {typeof(string)});

            if (this._getBalanceMethod == null)
                throw new Exception("AviEconomy GetBalance method couldn't be loaded...");

            this._performPaymentMethod = ReflectUtil.GetMethod(this._bankType, "PayAsServer", BindingFlags.Static | BindingFlags.Public,
                new[] {typeof(IRocketPlayer), typeof(decimal), typeof(bool), typeof(decimal).MakeByRefType(), typeof(string)});

            if (this._performPaymentMethod == null)
                throw new Exception("AviEconomy PerformPayment method couldn't be loaded...");

            UEssentials.Logger.LogInfo("AviEconomy hook loaded.");
        }

        public override void OnUnload() { }

        public override bool CanBeLoaded() => R.Plugins.GetPlugins().Any(c => c.Name.EqualsIgnoreCase("AviEconomy"));

        public decimal Withdraw(UPlayer player, decimal amount) {
            // args[3] = out decimal pFinalBalance
            var args = new object[] { player.RocketPlayer, -amount, false, null, null };
            this._performPaymentMethod.Invoke(this._bankType, args);
            return (decimal) args[3];
        }

        public decimal Deposit(UPlayer player, decimal amount) {
            // args[3] = out decimal pFinalBalance
            var args = new object[] { player.RocketPlayer, amount, false, null, null };
            this._performPaymentMethod.Invoke(this._bankType, args);
            return (decimal) args[3];
        }

        public decimal GetBalance(UPlayer player) => (decimal) this._getBalanceMethod.Invoke(this._bankType, new object[] {player.Id});

        public bool Has(UPlayer player, decimal amount) => this.GetBalance(player) - amount >= 0;

    }

}