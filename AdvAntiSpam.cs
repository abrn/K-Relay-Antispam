using Lib_K_Relay;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Enmity
{
    public class AdvAntiSpam : IPlugin
    {
        private Properties.Settings Config = Properties.Settings.Default;

        private bool enabled = true;

        public string GetAuthor()
        { return "Enmity"; }

        public string GetName()
        { return "Advanced Anti-Spam"; }

        public string GetDescription()
        { return "Flexible plugin to let you block all bots from spamming the chat."; }

        public string[] GetCommands()
        { return new string[] { "/antispam enable", "/antispam disable", "/antispam settings", "/antispam add" }; }

        StringCollection FilterList { get; set; }

        public void Initialize(Proxy proxy)
        {
            proxy.ClientConnected += OnClientConnect;
            proxy.ClientDisconnected += OnClientDisconnect;
            proxy.HookCommand("antispam", EnableDisable);
            proxy.HookCommand("say", SayCommand);
            proxy.HookPacket<TextPacket>(ProcessMessage);
        }

        private void EnableDisable(Client client, string command, string[] args)
        {
            if (args.Length == 0)
            {
                client.SendToClient(PluginUtils.CreateOryxNotification("Adv Anti-Spam", "Incorrect usage, see K Relay plugins tab for command help."));
                return;
            }
            switch(args[0])
            {
                case "enable":
                    client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, 8453888, "Anti-Spam Enabled"));
                    enabled = true;
                    break;
                case "disable":
                    client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, 16728064, "Anti-Spam Disabled"));
                    enabled = false;
                    break;
                case "settings":
                    PluginUtils.ShowGenericSettingsGUI(Config, "Adv Anti-Spam Settings");
                    Config.Save();
                    break;
                case "add":
                    client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, 49151, "Filter added!"));
                    string FullArgs = "";
                    for (int i = 1; i < args.Length; i++ )
                    {
                        FullArgs = FullArgs + " " + args[i];
                    }
                    Config.Filter.Add(FullArgs.Trim());
                    Config.Save();
                    Config.Reload();
                    break;
            }
        }

        private void OnClientConnect(Client client)
        { Config.Save(); }

        private void OnClientDisconnect(Client client)
        { Config.Save(); }

        public void SayCommand(Client client, string command, string[] args)
        {
            string FullArgs = "";
            for (int i = 0; i < args.Length; i++)
            {
                FullArgs = FullArgs + " " + args[i];
            }
            client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, 16744448, FullArgs));
        }

        public void ProcessMessage(Client client, TextPacket packet)
        {
            if (!enabled) return;

            foreach (string Filter in Config.Filter)
            {
                if (packet.Text.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (Config.IgnoreBots == true)
                    {
                        var Ignore = Packet.Create(PacketType.EDITACCOUNTLIST) as EditAccountListPacket;
                        Ignore.Add = true;
                        Ignore.AccountListId = 1;
                        Ignore.ObjectId = packet.ObjectId;
                        client.SendToServer(Ignore);
                    }
                    packet.Send = false;
                }
            }
        }
    }
}
