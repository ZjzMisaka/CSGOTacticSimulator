using CSGOTacticSimulator.Global;
using CSGOTacticSimulator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSGOTacticSimulator.Helper
{
    
    static public class CommandHelper
    {
        static public List<string> commands = new List<string>();
        static public List<string> GetCommands(string commandsText)
        {
            commands = new List<string>(commandsText.Split('\n'));
            for (int i = 0; i < commands.Count; ++i)
            {
                commands[i].Trim();
            }
            return commands;
        }

        static public Command AnalysisCommand(string cmd)
        {
            if(cmd.Length > 0 && cmd[0] == '-')
            {
                return Command.BadOrNotCommand;
            }

            if (cmd.Contains("set camp"))
            {
                return Command.SetCamp;
            }
            else if (cmd.Contains("create team"))
            {
                return Command.CreateTeam;
            }
            else if (cmd.Contains("create character"))
            {
                return Command.CreateCharacter;
            }
            else if (cmd.Contains("create comment"))
            {
                return Command.CreateComment;
            }
            else if (cmd.Contains("give character"))
            {
                if (cmd.Contains("weapon"))
                {
                    return Command.GiveCharacterWeapon;
                }
                else if (cmd.Contains("grenade"))
                {
                    return Command.GiveCharacterGrenade;
                }
                else if (cmd.Contains("props"))
                {
                    return Command.GiveCharacterProps;
                }
                else
                {
                    return Command.BadOrNotCommand;
                }
            }
            else if (cmd.Contains("set character"))
            {
                if (cmd.Contains("status"))
                {
                    return Command.SetCharacterStatus;
                }
                else if (cmd.Contains("vertical position"))
                {
                    return Command.SetCharacterVerticalPosition;
                }
                else
                {
                    return Command.BadOrNotCommand;
                }
            }
            else if (cmd.Contains("action character"))
            {
                if (cmd.Contains("move"))
                {
                    return Command.ActionCharacterMove;
                }
                else if (cmd.Contains("throw"))
                {
                    return Command.ActionCharacterThrow;
                }
                else if (cmd.Contains("shoot"))
                {
                    return Command.ActionCharacterShoot;
                }
                else if (cmd.Contains("do"))
                {
                    return Command.ActionCharacterDo;
                }
                else if (cmd.Contains("wait until"))
                {
                    return Command.ActionCharacterWaitUntil;
                }
                else if (cmd.Contains("wait for"))
                {
                    return Command.ActionCharacterWaitFor;
                }
                else
                {
                    return Command.BadOrNotCommand;
                }
            }
            else
            {
                return Command.BadOrNotCommand;
            }
        }
    }
}
