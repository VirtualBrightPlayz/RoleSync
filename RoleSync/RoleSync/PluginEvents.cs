﻿using EXILED;
using EXILED.Extensions;
using System;
using System.Text;
using MEC;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Net.Sockets;

namespace RoleSync
{
    public class PluginEvents
    {
        private PluginMain plugin;

        public class DiscordData
        {
            public string command { get; set; }
            public string steamid { get; set; }
            public string[] players { get; set; }
        }

        public class ReturnData
        {
            public string command { get; set; }
            public string role { get; set; }
            public string id { get; set; }
            public bool shouldHide { get; set; }
        }

        private CoroutineHandle handle;

        public PluginEvents(PluginMain pluginMain)
        {
            this.plugin = pluginMain;
            handle = Timing.RunCoroutine(StreamCo(), Segment.Update);
        }

        ~PluginEvents()
        {
            Timing.KillCoroutines(handle);
        }

        private IEnumerator<float> StreamCo()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(0.1f);
                if (!plugin.client.Connected)
                {
                    Log.Info("Reconnecting...");
                    try
                    {
                        plugin.client.Connect(plugin.conf.ip, plugin.conf.port);
                    }
                    catch (SocketException) { }
                    yield return Timing.WaitForSeconds(0.5f);
                }
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
                        switch (rd.command)
                        {
                            case "rolesync":
                                ReferenceHub hub = Player.GetPlayer(rd.id.Contains("@") ? rd.id : rd.id + "@steam");
                                hub.serverRoles.SetGroup(ServerStatic.PermissionsHandler._groups[rd.role], false, disp: rd.shouldHide);
                                break;
                            case "playerlist":
                                List<string> players = new List<string>();
                                foreach (var plr in PlayerManager.players)
                                {
                                    if (plr != PlayerManager.localPlayer)
                                        players.Add(plr.GetPlayer().GetNickname() + " " + plr.GetPlayer().GetUserId());
                                }
                                var sendme = new DiscordData()
                                {
                                    command = "playerlist",
                                    players = players.ToArray()
                                };
                                string sendme2 = JsonConvert.SerializeObject(sendme);
                                byte[] arr = UTF8Encoding.UTF8.GetBytes(sendme2);
                                plugin.stream.Write(arr, 0, arr.Length);
                                plugin.stream.Flush();
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.ToString());
                    }
                }
            }
        }

        internal void RACmd(ref RACommandEvent ev)
        {
        }

        public void SendData(ReferenceHub hub)
        {
            try
            {
                var data = new DiscordData()
                {
                    command = "playerjoin",
                    steamid = hub.GetUserId()
                };
                string sendme = JsonConvert.SerializeObject(data);
                byte[] arr = UTF8Encoding.UTF8.GetBytes(sendme);
                plugin.stream.Write(arr, 0, arr.Length);
                plugin.stream.Flush();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        internal void ConsoleCmd(ConsoleCommandEvent ev)
        {
            if (ev.Command.ToLower() == "rankme")
            {
                SendData(ev.Player);
                ev.ReturnMessage = "OK.";
            }
        }

        internal void PlayerJoin(PlayerJoinEvent ev)
        {
            SendData(ev.Player);
        }
    }
}