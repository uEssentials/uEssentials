using Rocket.API;

namespace Essentials.src.Api.Configuration
{
    public class MyConfig : IRocketPluginConfiguration
    {
        public bool Enabled;
        public void LoadDefaults()
        {
            Enabled = true;
        }
    }
}
