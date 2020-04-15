using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Vintagestory.ServerMods
{
    class LookingForGroup : ModSystem
    {
        ICoreServerAPI serverApi;
        List<string> lfgList;
        public override void StartServerSide(ICoreServerAPI api)
        {
            serverApi = api;

            api.Event.GameWorldSave += OnSaveGameSaving;
            api.Event.SaveGameLoaded += OnSaveGameLoading;

            api.RegisterCommand("lfg", "Access the lfg list.", "[list|join|leave]", OnLfg);
        }

        private void OnLfg(IServerPlayer player, int groupId, CmdArgs args)
        {
            string cmd = args.PopWord();
            switch (cmd)
            {
                case "list":
                    if (lfgList.Count == 0)
                    {
                        player.SendMessage(groupId, "Noone is looking for a group.", EnumChatType.CommandSuccess);
                    }
                    else
                    {
                        string response = "Players looking for group:";
                        lfgList.ForEach((playerUid) =>
                        {
                            response += "\n" + serverApi.World.PlayerByUid(playerUid).PlayerName;
                        });

                        player.SendMessage(groupId, response, EnumChatType.CommandSuccess);
                    }
                    break;

                case "join":
                    if (lfgList.Contains(player.PlayerUID))
                    {
                        player.SendMessage(groupId, "You're already in the list!", EnumChatType.CommandError);
                    }
                    else
                    {
                        lfgList.Add(player.PlayerUID);
                        player.SendMessage(groupId, "Successfully joined.", EnumChatType.CommandSuccess);
                    }
                    break;

                case "leave":
                    if (!lfgList.Remove(player.PlayerUID))
                    {
                        player.SendMessage(groupId, "You're not in the list!", EnumChatType.CommandError);
                    }
                    else
                    {
                        player.SendMessage(groupId, "Successfully left.", EnumChatType.CommandSuccess);
                    }
                    break;

                case null:
                default:
                    player.SendMessage(groupId, "/lfg [list|join|leave]", EnumChatType.CommandError);
                    return;
            }
        }

        private void OnSaveGameLoading()
        {
            byte[] data = serverApi.WorldManager.SaveGame.GetData("lfg");

            lfgList = data == null ? new List<string>() : SerializerUtil.Deserialize<List<string>>(data);
        }
        private void OnSaveGameSaving()
        {
            serverApi.WorldManager.SaveGame.StoreData("lfg", SerializerUtil.Serialize(lfgList));
        }
    }
}
