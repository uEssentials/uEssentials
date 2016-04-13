using Essentials.Api.Unturned;

namespace Essentials.Core.Economy
{
    public interface IEconomyProvider
    {
        string Currency { get; }

        decimal Withdraw( UPlayer player, decimal amount );

        decimal Deposit( UPlayer player, decimal amount );

        decimal GetBalance( UPlayer player );

        bool Has( UPlayer player, decimal amount );
    }
}