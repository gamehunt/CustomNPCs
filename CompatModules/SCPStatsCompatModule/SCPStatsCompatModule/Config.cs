using Exiled.API.Interfaces;

namespace SCPStatsCompatModule
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
    }
}