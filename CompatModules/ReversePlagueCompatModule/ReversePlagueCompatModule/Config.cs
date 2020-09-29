using Exiled.API.Interfaces;

namespace ReversePlagueCompatModule
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
    }
}