using Essentials.Api;
using Essentials.Api.Unturned;

namespace Essentials.Core.Economy
{
    public class  ExpEconomyProvider : IEconomyProvider
    {
        public string Currency => EssProvider.Config.Economy.XpCurrency;

        public decimal Withdraw( UPlayer player, decimal amount )
        {
            return (player.Experience -= (uint) amount);
        }

        public decimal Deposit( UPlayer player, decimal amount )
        {
            return (player.Experience += (uint) amount);
        }

        public decimal GetBalance( UPlayer player )
        {
            return player.Experience;
        }

        public bool Has( UPlayer player, decimal amount )
        {
            return (player.Experience - amount) >= 0;
        }
    }
}