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
using Rocket.Unturned.Player;

namespace Essentials.Compatibility.Hooks {

    public class AviEconomyHook : Hook, IEconomyProvider {

        public string CurrencySymbol => UEssentials.Config.Economy.UconomyCurrency;

        private Type _bankType;
        private MethodInfo _getBalanceMethod;
        private MethodInfo _performPaymentMethod;
        private MethodInfo _processPurchaseMethod;

        public AviEconomyHook() : base("avi_economy") { }

        public override void OnLoad() {

            Logger.Log("Loading AviEconomy hook...");

            IRocketPlugin economyPlugin = R.Plugins.GetPlugins().FirstOrDefault(c => c.Name.EqualsIgnoreCase("AviEconomy"));

            if (economyPlugin == null) { throw new Exception("AviEconomy Plugin couldn't be loaded..."); }

            this._bankType = economyPlugin.GetType().Assembly.GetType("com.aviadmini.rocketmod.AviEconomy.Bank");

            if (this._bankType == null) { throw new Exception("AviEconomy Bank type couldn't be loaded..."); }

            this._getBalanceMethod = ReflectUtil.GetMethod(this._bankType, "GetBalance", BindingFlags.Static | BindingFlags.Public,
                new[] {typeof(string)});

            if (this._bankType == null) { throw new Exception("AviEconomy GetBalance method couldn't be loaded..."); }

            this._performPaymentMethod = ReflectUtil.GetMethod(this._bankType, "PerformPayment", BindingFlags.Static | BindingFlags.Public,
                new[] {typeof(IRocketPlayer), typeof(string), typeof(decimal), typeof(string)});

            if (this._bankType == null) { throw new Exception("AviEconomy PerformPayment method couldn't be loaded..."); }

            this._processPurchaseMethod = ReflectUtil.GetMethod(this._bankType, "ProcessPurchaseFromServer",
                BindingFlags.Static | BindingFlags.Public,
                new[] {typeof(UnturnedPlayer), typeof(decimal), typeof(string), typeof(bool)});

            if (this._bankType == null) { throw new Exception("AviEconomy ProcessPurchase method couldn't be loaded..."); }

            Logger.Log("AviEconomy hook loaded.");

        }

        public override void OnUnload() { }

        public override bool CanBeLoaded() => R.Plugins.GetPlugins().Any(c => c.Name.EqualsIgnoreCase("AviEconomy"));

        public decimal Withdraw(UPlayer player, decimal amount) {

            this._processPurchaseMethod.Invoke(this._bankType, new object[] {player.RocketPlayer, amount, null, true});

            return this.GetBalance(player);
        }

        public decimal Deposit(UPlayer player, decimal amount) {
            this._performPaymentMethod.Invoke(this._bankType,
                new object[] {new ConsolePlayer(), player.Id, amount, null});
            return this.GetBalance(player);
        }

        public decimal GetBalance(UPlayer player) => (decimal) this._getBalanceMethod.Invoke(this._bankType, new object[] {player.Id});

        public bool Has(UPlayer player, decimal amount) => this.GetBalance(player) - amount >= 0;

    }

}