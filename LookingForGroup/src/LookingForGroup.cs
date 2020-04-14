using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Vintagestory.ServerMods
{
    class LookingForGroup : ModSystem
    {
        ICoreServerAPI serverApi;
        public override void StartServerSide(ICoreServerAPI api)
        {
            serverApi = api;

            api.RegisterCommand("lfg", "List or join the lfg list", "[list|join|leave]", OnLfg);
        }

        private void OnLfg(IServerPlayer player, int groupId, CmdArgs args)
        {
            if (args.Length < 1)
            {
                player.SendMessage(groupId, "/lfg [list|join|leave]", EnumChatType.CommandError);
                return;
            }

            byte[] data = serverApi.WorldManager.GetData("lfg");

            List<string> players = data == null ? new List<string>() : SerializerUtil.Deserialize<List<string>>(data);

            string cmd = args.PopSingle();

            switch (cmd)
            {
                case "list":
                    if (players.Count == 0)
                    {
                        player.SendMessage(groupId, "Noone is looking for group!", EnumChatType.Notification);
                        break;
                    }

                    string lfgList = "Players looking for group:";

                    players.ForEach((playerUid) =>
                    {
                        lfgList += "\n" + serverApi.World.PlayerByUid(playerUid).PlayerName;
                    });

                    player.SendMessage(groupId, lfgList, EnumChatType.Notification);
                    break;

                case "join":
                    if (players.Contains(player.PlayerUID))
                    {
                        player.SendMessage(groupId, "You're already in this list!", EnumChatType.Notification);
                    }
                    else
                    {
                        players.Add(player.PlayerUID);
                        data = SerializerUtil.Serialize(players);

                        serverApi.WorldManager.StoreData("lfg", data);

                        player.SendMessage(groupId, "Successfully joined!", EnumChatType.Notification);
                    }
                    break;

                case "leave":
                    if (!players.Remove(player.PlayerUID))
                    {
                        player.SendMessage(groupId, "You're not in the list!", EnumChatType.Notification);
                    }
                    else
                    {
                        data = SerializerUtil.Serialize(players);

                        serverApi.WorldManager.StoreData("lfg", data);

                        player.SendMessage(groupId, "Successfully left!", EnumChatType.Notification);
                    }
                    break;

                default:
                    player.SendMessage(groupId, "/lfg [list|join|leave]", EnumChatType.CommandError);
                    break;
            }
        }
    }
}
