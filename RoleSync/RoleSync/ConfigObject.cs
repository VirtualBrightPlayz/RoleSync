using Exiled.API.Interfaces;
using System.Collections.Generic;

namespace RoleSync
{
    public class ConfigObject
    {
        public int port { get; set; } = 6000;
        public string ip { get; set; } = "127.0.0.1";
        public string name { get; set; } = "server name";
    }
}