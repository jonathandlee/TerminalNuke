using BepInEx;
using HarmonyLib;
using System.Reflection;
using TerminalApi;
using TerminalApi.Events;
using static TerminalApi.Events.Events;
using static TerminalApi.TerminalApi;
using UnityEngine;
using Unity.Netcode;
using GameNetcodeStuff;
using System.Threading.Tasks;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace LethalCompanyTemplate
{
    [BepInPlugin(TerminalNuke.PluginInfo.PLUGIN_GUID, TerminalNuke.PluginInfo.PLUGIN_NAME, TerminalNuke.PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        // Because player list is really annoying to get, 
        private List<PlayerControllerB> players = new List<PlayerControllerB>();
        
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {TerminalNuke.PluginInfo.PLUGIN_GUID} is loaded!");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            InitialCommands();
            TerminalParsedSentence += TextSubmitted;
            TerminalBeginUsing += OnBeginUsing;
        }

        private void InitialCommands()
        {
            TerminalNode node = CreateTerminalNode("You're not on a moon. The player list is not confirmed.\n", true);
            TerminalKeyword verbKeyword = CreateTerminalKeyword("check", true);
            TerminalKeyword nounKeyword = CreateTerminalKeyword("players");
            verbKeyword.AddCompatibleNoun(nounKeyword, node);
            nounKeyword.defaultVerb = verbKeyword;
            AddTerminalKeyword(verbKeyword);
            AddTerminalKeyword(nounKeyword);

            TerminalNode node2 = CreateTerminalNode("Wait til you land before you start slaying, queen!", true);
            TerminalKeyword nounKeyword2 = CreateTerminalKeyword("kill", false, node2);
            AddTerminalKeyword(nounKeyword2);
        }

        private List<PlayerControllerB> GetPlayers()
        {
            List<PlayerControllerB> playerControllerBs = new List<PlayerControllerB>();
            foreach (PlayerControllerB p in RoundManager.Instance.playersManager.allPlayerScripts)
            {
                if (p.isPlayerControlled)
                {
                    playerControllerBs.Add(p);
                }
            }
            return playerControllerBs;
        }

        private string PlayerString(List<PlayerControllerB> playerlist)
        {
            string playerString = "";
            foreach (PlayerControllerB p in playerlist)
            {
                if (p.isPlayerControlled)
                {
                    playerString += p.playerUsername + "\n ";
                }
            }
            return playerString;
        }



        private async void TextSubmitted(object sender, TerminalParseSentenceEventArgs e)
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            List<PlayerControllerB> playerControllerBs = GetPlayers();
            Vector3 pos = player.gameplayCamera.transform.position;
            Logger.LogMessage($"Text submitted: {e.SubmittedText} Node Returned: {e.ReturnedNode}");
            Logger.LogMessage($"User: {sender.ToString()}");




            // From what I can tell, the AddCommand method of the terminalAPI only allows for text display, so we simply add commands to the terminalAPI and then use the TextSubmitted event to handle the logic of the commands
            if (e.SubmittedText.Equals("hello"))
            {
                Vector3 explosionPos = player.gameplayCamera.transform.position;
                await Task.Delay(2000);
                Logger.LogMessage("Kill Attempted");
                //player.KillPlayer(pos * 2, true, CauseOfDeath.Blast, 0);
                // I need to spawn in a landmine object

                foreach (PlayerControllerB p in RoundManager.Instance.playersManager.allPlayerScripts)
                {
                    if (p.isPlayerControlled) 
                    {
                        Logger.LogMessage("Player Position Check");
                        Logger.LogMessage(p.gameplayCamera.transform.position);
                        Logger.LogMessage(p.name);
                    }
                }
            }

            


            if (e.SubmittedText.Equals("players"))
            {
                Logger.LogMessage("Players gotten");
                foreach (PlayerControllerB p in players)
                {
                    Logger.LogMessage(p.playerUsername);
                }

            }
             
            
            if (e.SubmittedText.Substring(0,4).Equals("kill") && e.SubmittedText.Length > 5)
            {
                string text = e.SubmittedText.Substring(5);
                KillPlayer(text);
            }
            Logger.LogMessage($"Player Position: {pos}");
            Logger.LogMessage($"Player Info: {player.name}");
        }



        private void OnBeginUsing(object sender, TerminalEventArgs e)
        {
            players = GetPlayers();
            UpdateKeywordCompatibleNoun("check", "players", CreateTerminalNode($"The List of Players is: \n {PlayerString(players)}.", true));
        }
        private async void KillPlayer(string playerName)
        {
            await Task.Delay(2000);
            foreach (PlayerControllerB p in players)
            {
              
                Logger.LogMessage(p.playerUsername);
                Logger.LogMessage(p.name);
                Logger.LogMessage(p.playerUsername.Equals(playerName));
                if (p.playerUsername.Equals(playerName))
                {
                    Logger.LogMessage("Player Found");
                    p.KillPlayer(p.gameplayCamera.transform.position * 2, true, CauseOfDeath.Blast, 1);
                }



                
            }

            // Implementation of the KillPlayer logic
            // This might involve finding the player's character or controller and performing the kill action
        }

        









    }
}