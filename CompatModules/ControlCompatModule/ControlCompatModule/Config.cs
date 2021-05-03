using Exiled.API.Interfaces;

namespace ControlCompatModule
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
    }
}