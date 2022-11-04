using HltvSharp.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HltvSharp
{
    public class Search
    {
        public async Task<List<TeamSearchItem>> Teams(string SearchQuery)
        {
            var Results = await GetSearchResults(SearchQuery);

            var Teamlist = new List<TeamSearchItem>();

            var TeamArray = Results["teams"];
            if(TeamArray == null) { return null; }
            
            foreach(var Team in TeamArray)
            {
                var TeamItem = new TeamSearchItem();

                TeamItem.Name = (string)Team["name"];
                TeamItem.Id = (int)Team["id"];
                TeamItem.flagUrl = (string)Team["flagUrl"];
                TeamItem.teamLogoDarkUrl = (string)Team["teamLogoNight"];
                TeamItem.teamLogoLightUrl = (string)Team["teamLogoDay"];
                TeamItem.webLocation = (string)Team["location"];

                var PlayerList = new List<TeamSearchPlayerItem>();

                var PlayerArray = Team["players"] as JArray;

                foreach(var Player in PlayerArray)
                {
                    var PlayerItem = new TeamSearchPlayerItem();
                    PlayerItem.firstName = (string)Player["firstName"];
                    PlayerItem.lastName = (string)Player["lastName"];
                    PlayerItem.nickName = (string)Player["nickName"];
                    PlayerItem.flagUrl = (string)Player["flagUrl"];
                    PlayerItem.webLocation = (string)Player["location"];

                    PlayerList.Add(PlayerItem);
                }
                
                TeamItem.PlayerList = PlayerList;

                Teamlist.Add(TeamItem);
            }

            return Teamlist;
        }

        public async Task<List<PlayerSearchItem>> Players(string SearchQuery)
        {
            var Results = await GetSearchResults(SearchQuery);
            if (Results == null)
            {
                return null;
            }

            var PlayerList = new List<PlayerSearchItem>();

            var PlayerArray = Results["players"];
            if(PlayerArray == null) { return null; }

            foreach(var Player in PlayerArray)
            {
                var PlayerItem = new PlayerSearchItem();

                PlayerItem.firstName = (string)Player["firstName"];
                PlayerItem.lastName = (string)Player["firstName"];
                PlayerItem.nickName = (string)Player["nickName"];
                PlayerItem.flagUrl = (string)Player["flagUrl"];
                PlayerItem.webLocation = (string)Player["location"];
                PlayerItem.id = (int)Player["id"];
                PlayerItem.pictureUrl = (string)Player["pictureUrl"];

                var PlayerTeamItem = Player["team"];

                var PlayerTeam = new PlayerTeamItem();

               if(PlayerTeamItem != null)
                {
                    PlayerTeam.name = (string)PlayerTeamItem["name"];
                    PlayerTeam.teamLogoDarkUrl = (string)PlayerTeamItem["teamLogoNight"];
                    PlayerTeam.teamLogoLightUrl = (string)PlayerTeamItem["teamLogoDay"];
                    PlayerTeam.webLocation = (string)PlayerTeamItem["location"];
                }
                else
                {
                    PlayerTeam = null;
                }

                PlayerItem.team = PlayerTeam;

                PlayerList.Add(PlayerItem);
            }


            return PlayerList;
        }

        public async Task<List<EventsSearchItem>> Events(string SearchQuery)
        {
            var Results = await GetSearchResults(SearchQuery);

            var EventList = new List<EventsSearchItem>();

            var EventArray = Results["events"];
            if (EventArray == null) { return null; }

            foreach (var Event in EventArray)
            {
                var EventItem = new EventsSearchItem();

                
                

                EventItem.id = (int)Event["id"];
                EventItem.name = (string)Event["name"];
                EventItem.flagUrl = (string)Event["flagUrl"];
                EventItem.flagUrl = (string)Event["flagUrl"];
                EventItem.eventLogoUrl = (string)Event["eventLogo"];
                EventItem.webLocation = (string)Event["location"];
                EventItem.physicalLocation = (string)Event["physicalLocation"];
                EventItem.prizePool = (string)Event["prizePool"];
                EventItem.eventType = (string)Event["eventType"];

                EventList.Add(EventItem);
            }


            return EventList;
        }

        public async Task<AllSearchItem> All(string SearchQuery)
        {
            var item = new AllSearchItem();

            var Results = await GetSearchResults(SearchQuery);


            //teams
            var Teamlist = new List<TeamSearchItem>();

            var TeamArray = Results["teams"];
            if (TeamArray == null) { return null; }

            foreach (var Team in TeamArray)
            {
                var TeamItem = new TeamSearchItem();

                TeamItem.Name = (string)Team["name"];
                TeamItem.Id = (int)Team["id"];
                TeamItem.flagUrl = (string)Team["flagUrl"];
                TeamItem.teamLogoDarkUrl = (string)Team["teamLogoNight"];
                TeamItem.teamLogoLightUrl = (string)Team["teamLogoDay"];
                TeamItem.webLocation = (string)Team["location"];

                var TeamPlayerList = new List<TeamSearchPlayerItem>();

                var TeamPlayerArray = Team["players"] as JArray;

                foreach (var Player in TeamPlayerArray)
                {
                    var PlayerItem = new TeamSearchPlayerItem();
                    PlayerItem.firstName = (string)Player["firstName"];
                    PlayerItem.lastName = (string)Player["lastName"];
                    PlayerItem.nickName = (string)Player["nickName"];
                    PlayerItem.flagUrl = (string)Player["flagUrl"];
                    PlayerItem.webLocation = (string)Player["location"];

                    TeamPlayerList.Add(PlayerItem);
                }

                TeamItem.PlayerList = TeamPlayerList;

                Teamlist.Add(TeamItem);
            }

            item.Teams = Teamlist;


            //players
            var PlayerList = new List<PlayerSearchItem>();

            var PlayerArray = Results["players"];
            if (PlayerArray == null) { return null; }

            foreach (var Player in PlayerArray)
            {
                var PlayerItem = new PlayerSearchItem();

                PlayerItem.firstName = (string)Player["firstName"];
                PlayerItem.lastName = (string)Player["firstName"];
                PlayerItem.nickName = (string)Player["nickName"];
                PlayerItem.flagUrl = (string)Player["flagUrl"];
                PlayerItem.webLocation = (string)Player["location"];
                PlayerItem.id = (int)Player["id"];
                PlayerItem.pictureUrl = (string)Player["pictureUrl"];

                var PlayerTeamItem = Player["team"];

                var PlayerTeam = new PlayerTeamItem();

                if (PlayerTeamItem != null)
                {
                    PlayerTeam.name = (string)PlayerTeamItem["name"];
                    PlayerTeam.teamLogoDarkUrl = (string)PlayerTeamItem["teamLogoNight"];
                    PlayerTeam.teamLogoLightUrl = (string)PlayerTeamItem["teamLogoDay"];
                    PlayerTeam.webLocation = (string)PlayerTeamItem["location"];
                }
                else
                {
                    PlayerTeam = null;
                }

                PlayerItem.team = PlayerTeam;

                PlayerList.Add(PlayerItem);
            }

            item.Players = PlayerList;


            //events
            var EventList = new List<EventsSearchItem>();

            var EventArray = Results["events"];
            if (EventArray == null) { return null; }

            foreach (var Event in EventArray)
            {
                var EventItem = new EventsSearchItem();




                EventItem.id = (int)Event["id"];
                EventItem.name = (string)Event["name"];
                EventItem.flagUrl = (string)Event["flagUrl"];
                EventItem.flagUrl = (string)Event["flagUrl"];
                EventItem.eventLogoUrl = (string)Event["eventLogo"];
                EventItem.webLocation = (string)Event["location"];
                EventItem.physicalLocation = (string)Event["physicalLocation"];
                EventItem.prizePool = (string)Event["prizePool"];
                EventItem.eventType = (string)Event["eventType"];

                EventList.Add(EventItem);
            }

            item.Events = EventList;



            return item;
        }

        public async Task<JToken> RawJson(string SearchQuery)
        {
            var Results = await GetSearchResults(SearchQuery);

            return Results;
        }

        private async Task<JToken> GetSearchResults(string SearchQuery)
        {
            var url = "https://www.hltv.org/search?term=" + SearchQuery;

            var client = new HttpClient();

            string content = "";
            try
            {
                content = await client.GetStringAsync(url);
            }
            catch
            {
                return null;
            }
            JArray results = JArray.Parse(content);
            var result = results[0];

            if(results == null)
            {
                throw new Exception("Error Getting results");
            }

            return result;
        }
    }
}
