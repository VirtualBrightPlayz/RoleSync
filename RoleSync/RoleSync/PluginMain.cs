using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using EXILED;
using EXILED.Extensions;
using Newtonsoft.Json;
using Harmony;

namespace RoleSync
{
    public class PluginMain : Plugin
    {
        public override string getName => "DiscordRoleSync";
        public PluginEvents PLEV;
        public ConfigObject conf;
        public TcpClient client;
        public NetworkStream stream;
        public HarmonyInstance inst;

        public override void OnDisable()
        {
            inst.UnpatchAll();
            Events.PlayerJoinEvent -= PLEV.PlayerJoin;
            Events.ConsoleCommandEvent -= PLEV.ConsoleCmd;
            Events.RemoteAdminCommandEvent -= PLEV.RACmd;
            PLEV = null;
            client.Close();
            client.Dispose();
            client = null;
        }

        public override void OnEnable()
        {
            LoadConfig();
            client = new TcpClient();
            client.Connect(conf.ip, conf.port);
            stream = client.GetStream();
            PLEV = new PluginEvents(this);
            Events.PlayerJoinEvent += PLEV.PlayerJoin;
            Events.ConsoleCommandEvent += PLEV.ConsoleCmd;
            Events.RemoteAdminCommandEvent += PLEV.RACmd;
            inst = HarmonyInstance.Create("virtual.scpsl.rolesync");
            inst.PatchAll();
        }

        public void LoadConfig()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string pluginDir = Path.Combine(appData, "Plugins", "RoleSync");
            if (!Directory.Exists(pluginDir))
                Directory.CreateDirectory(pluginDir);
            string path = Path.Combine(pluginDir, "config" + ServerStatic.ServerPort + ".json");
            if (!File.Exists(path))
                File.WriteAllText(path, JsonConvert.SerializeObject(new ConfigObject()));
            conf = JsonConvert.DeserializeObject<ConfigObject>(File.ReadAllText(path));
        }

        public override void OnReload()
        {
            LoadConfig();
        }
    }
}
