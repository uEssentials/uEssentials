﻿#region License

/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2017  leonardosnt and contributors
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
using Rocket.Core.Logging;

namespace Essentials.Compatibility.Hooks {

    public class AviEconomyHook : Hook, IEconomyProvider {

        public string CurrencySymbol => UEssentials.Config.Economy.UconomyCurrency;

        private Type _bankType;
        private MethodInfo _getBalanceMethod;
        private MethodInfo _performPaymentMethod;
        private MethodInfo _processPurchaseMethod;

        public AviEconomyHook() : base("avi_economy") { }

        public override void OnLoad() {

            UEssentials.Logger.LogInfo("Loading AviEconomy hook...");

            IRocketPlugin economyPlugin = R.Plugins.GetPlugins().FirstOrDefault(c => c.Name.EqualsIgnoreCase("AviEconomy"));

            if (economyPlugin == null) { throw new Exception("AviEconomy Plugin couldn't be loaded..."); }

            _bankType = economyPlugin.GetType().Assembly.GetType("com.aviadmini.rocketmod.AviEconomy.Bank");

            if (_bankType == null) { throw new Exception("AviEconomy Bank type couldn't be loaded..."); }

            _getBalanceMethod = ReflectUtil.GetMethod(_bankType, "GetBalance", BindingFlags.Static | BindingFlags.Public,
                new[] {typeof(string)});

            if (_getBalanceMethod == null) { throw new Exception("AviEconomy GetBalance method couldn't be loaded..."); }

            _performPaymentMethod = ReflectUtil.GetMethod(_bankType, "PerformPayment", BindingFlags.Static | BindingFlags.Public,
                new[] {typeof(IRocketPlayer), typeof(string), typeof(decimal), typeof(string)});

            if (_performPaymentMethod == null) { throw new Exception("AviEconomy PerformPayment method couldn't be loaded..."); }

            _processPurchaseMethod = ReflectUtil.GetMethod(_bankType, "ProcessPurchaseFromServer", BindingFlags.Static | BindingFlags.Public,
                new[] {typeof(IRocketPlayer), typeof(decimal), typeof(string), typeof(bool)});

            if (_processPurchaseMethod == null) { throw new Exception("AviEconomy ProcessPurchase method couldn't be loaded..."); }

            UEssentials.Logger.LogInfo("AviEconomy hook loaded.");

        }

        public override void OnUnload() { }

        public override bool CanBeLoaded() => R.Plugins.GetPlugins().Any(c => c.Name.EqualsIgnoreCase("AviEconomy"));

        public decimal Withdraw(UPlayer player, decimal amount) {
            _processPurchaseMethod.Invoke(_bankType, new object[] {player.RocketPlayer, amount, null, true});
            return GetBalance(player);
        }

        public decimal Deposit(UPlayer player, decimal amount) {
            _performPaymentMethod.Invoke(_bankType, new object[] {new ConsolePlayer(), player.Id, amount, null});
            return GetBalance(player);
        }

        public decimal GetBalance(UPlayer player) => (decimal) _getBalanceMethod.Invoke(_bankType, new object[] {player.Id});

        public bool Has(UPlayer player, decimal amount) => GetBalance(player) - amount >= 0;

    }

}