using Exiled.API.Interfaces;

namespace DICompatModule
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
    }
}