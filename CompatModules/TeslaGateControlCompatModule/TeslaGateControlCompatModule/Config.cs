using Exiled.API.Interfaces;

namespace TeslaGateControlCompatModule
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
    }
}