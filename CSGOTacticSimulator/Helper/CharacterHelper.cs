using CSGOTacticSimulator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGOTacticSimulator.Helper
{
    static public class CharacterHelper
    {
        static private List<Character> characters = new List<Character>();
        static private Dictionary<string, int> characterNameDic = new Dictionary<string, int>();

        static public List<Character> GetCharacters()
        {
            return characters;
        }
        static public void AddCharacter(Character character)
        {
            characters.Add(character);
        }
        static public Character GetCharacter(int number)
        {
            return characters[number];
        }
        static public Character GetCharacter(long steamId)
        {
            foreach (Character character in characters)
            {
                if (character.SteamId == steamId)
                {
                    return character;
                }
            }
            return null;
        }

        static public bool AddIntoNameDic(string name, int number)
        {
            if (!characterNameDic.ContainsKey(name))
            {
                characterNameDic.Add(name, number);
                return true;
            }
            else
            {
                return false;
            }
        }
        static public Character GetCharacter(string str)
        {
            int number;
            if(!int.TryParse(str, out number))
            {
                number = characterNameDic[str];
            }
            return GetCharacter(number);
        }

        static public void ClearCharacters()
        {
            characters.Clear();
            characterNameDic.Clear();
        }
    }
}
