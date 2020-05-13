using EXILED;
using EXILED.Extensions;
using System;
using System.Text;
using MEC;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RoleSync
{
    public class PluginEvents
    {
        private PluginMain plugin;

        public class DiscordData
        {
            public string command { get; set; }
            public ulong steamid { get; set; }
        }

        public class ReturnData
        {
            public string role { get; set; }
            public ulong id { get; set; }
        }

        private CoroutineHandle handle;

        public PluginEvents(PluginMain pluginMain)
        {
            this.plugin = pluginMain;
            handle = Timing.RunCoroutine(StreamCo());
        }

        private IEnumerator<float> StreamCo()
        {
            yield return Timing.WaitForSeconds(0.1f);
            if (plugin.stream.DataAvailable)
            {
                List<byte> list = new List<byte>();
                int data = plugin.stream.ReadByte();
                while (data != -1)
                {
                    list.Add((byte)data);
                    data = plugin.stream.ReadByte();
                    if (data != -1 && UTF8Encoding.UTF8.GetString(new byte[] { (byte)data }).Contains(";"))
                    {
                        break;
                    }
                }
                string str = UTF8Encoding.UTF8.GetString(list.ToArray());
                try
                {
                    ReturnData rd = JsonConvert.DeserializeObject<ReturnData>(str);
                    ReferenceHub hub = Player.GetPlayer(rd.id + "@steam");
                    hub.SetRank(ServerStatic.PermissionsHandler._groups[rd.role]);
                }
                catch (Exception)
                { }
            }
        }

        ~PluginEvents()
        {
            Timing.KillCoroutines(handle);
        }

        internal void PlayerJoin(PlayerJoinEvent ev)
        {
            try
            {
                var data = new DiscordData()
                {
                    command = "",
                    steamid = ulong.Parse(ev.Player.GetUserId().Substring(0, 16))
                };
                string sendme = JsonConvert.SerializeObject(data);
                byte[] arr = UTF8Encoding.UTF8.GetBytes(sendme);
                plugin.stream.Write(arr, 0, arr.Length);
                plugin.stream.Flush();
                //ev.Player.SetRank(ServerStatic.PermissionsHandler._groups[]);
            }
        catch (Exception) { }
        }
    }
}