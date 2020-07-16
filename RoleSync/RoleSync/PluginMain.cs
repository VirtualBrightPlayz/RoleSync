using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using HarmonyLib;
using Exiled.Events;
using Exiled.API.Features;
using Exiled.Events;

namespace RoleSync
{
    public class PluginMain : Plugin<Config>
    {
        public override string Name => "DiscordRoleSync";
        public override string Author => "VirtualBrightPlayz";
        public override Version Version => new Version(1, 4, 0);
        public PluginEvents PLEV;
        public ConfigObject conf;
        public TcpClient client;
        public NetworkStream stream;
        public Harmony inst;

        public override void OnDisabled()
        {
            base.OnDisabled();
            inst.UnpatchAll();
            Exiled.Events.Handlers.Player.Joined -= PLEV.PlayerJoin;
            PLEV = null;
            client.Close();
            client.Dispose();
            client = null;
        }

        public override void OnEnabled()
        {
            base.OnEnabled();
            LoadConfig();
            client = new TcpClient();
            client.Connect(conf.ip, conf.port);
            stream = client.GetStream();
            PLEV = new PluginEvents(this);
            Exiled.Events.Handlers.Player.Joined += PLEV.PlayerJoin;
            inst = new Harmony("virtual.scpsl.rolesync");
            inst.PatchAll();
        }

        public void LoadConfig()
        {
            string pluginDir = Path.Combine(Paths.Configs, "RoleSync");
            if (!Directory.Exists(pluginDir))
                Directory.CreateDirectory(pluginDir);
            string path = Path.Combine(pluginDir, "config" + ServerStatic.ServerPort + ".json");
            if (!File.Exists(path))
                File.WriteAllText(path, JsonConvert.SerializeObject(new ConfigObject()));
            conf = JsonConvert.DeserializeObject<ConfigObject>(File.ReadAllText(path));
        }

        public override void OnReloaded()
        {
            base.OnReloaded();
            LoadConfig();
        }
    }
}
