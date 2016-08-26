#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  leonardosnt
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

using Essentials.Api;
using Essentials.Api.Unturned;

namespace Essentials.Economy {

    public class ExpEconomyProvider : IEconomyProvider {

        public string CurrencySymbol => UEssentials.Config.Economy.XpCurrency;

        public decimal Withdraw(UPlayer player, decimal amount) {
            return (player.Experience -= (uint) amount);
        }

        public decimal Deposit(UPlayer player, decimal amount) {
            return (player.Experience += (uint) amount);
        }

        public decimal GetBalance(UPlayer player) {
            return player.Experience;
        }

        public bool Has(UPlayer player, decimal amount) {
            return (player.Experience - amount) >= 0;
        }

    }

}