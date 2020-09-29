using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace ReversePlagueCompatModule
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
    }
}