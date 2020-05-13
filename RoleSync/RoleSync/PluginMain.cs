using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using EXILED;
using EXILED.Extensions;

namespace RoleSync
{
    public class PluginMain : Plugin
    {
        public override string getName => "DiscordRoleSync";
        public PluginEvents PLEV;
        public ConfigObject conf;
        public TcpClient client;
        public NetworkStream stream => client.GetStream();

        public override void OnDisable()
        {
            Events.PlayerJoinEvent -= PLEV.PlayerJoin;
            PLEV = null;
            client.Close();
            client.Dispose();
            client = null;
        }

        public override void OnEnable()
        {
            LoadConfig();
            client = new TcpClient();
            client.Connect("127.0.0.1", 6000);
            PLEV = new PluginEvents(this);
            Events.PlayerJoinEvent += PLEV.PlayerJoin;
        }

        public void LoadConfig()
        {
            
        }

        public override void OnReload()
        {

        }
    }
}
