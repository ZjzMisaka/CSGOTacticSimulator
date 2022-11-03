using Fizzler.Systems.HtmlAgilityPack;
using HltvSharp.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HltvSharp.Parsing
{
    public static partial class HltvParser
    {
        public static Task<Player> GetPlayer(int playerid, WebProxy proxy = null)
        {
            return FetchPage($"player/{playerid}/-", (response) => GetPlayerParse(response, playerid), proxy);
        }
        private static Player GetPlayerParse(Task<HttpResponseMessage> response, int id = 0)
        {
            //load html
            var content = response.Result.Content;
            string htmlContent = content.ReadAsStringAsync().Result;

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(htmlContent);

            HtmlNode document = html.DocumentNode;


            var player = new Player();

            //id
            player.Id = int.Parse(document.SelectNodes("//meta[@property='og:url']/@content")[0].GetAttributeValue("content", String.Empty).Split('/')[3]);

            //name
            var nick = document.QuerySelector(".playerNickname").InnerText;
            var realname = document.QuerySelector(".playerRealname").InnerText;
            player.Name = $"{realname.Split(' ')[1]} '{nick}' {realname.Split(' ')[2]}"; //split ei toimi oikein

            //PlayerImageUrl
            player.playerImgUrl = document.QuerySelector(".bodyshot-img").Attributes["src"].Value;

            //Country
            player.Country = document.QuerySelector(".flag").Attributes["title"].Value;

            //age
            var age = document.QuerySelector(".playerInfo").QuerySelector(".playerAge").SelectNodes("//span[@itemprop='text']")[0].InnerText;
            age = Regex.Replace(age, "[^0-9]", "");
            player.age = int.Parse(age);

            //current team
            player.currentTeam = document.QuerySelector(".playerInfo").QuerySelector(".playerTeam").QuerySelector(".listRight").ChildNodes[1].InnerText;

            player.teams = GetTeams(document);

            player.upcomingMatches = GetUpcomingPlayerMatches(document);

            player.recentMatches = GetRecentPlayerMatches(document);

            return player;
        }
        
        private static List<Team> GetTeams(HtmlNode document)
        {
            List<Team> teams = new List<Team>();

            var table = document.QuerySelector(".team-breakdown");
            if(table == null) { return null; }

            var teamlist = table.QuerySelectorAll(".past-team");

            foreach (var team in teamlist)
            {
                var NewTeam = new Team();

                NewTeam.Name = team.QuerySelector(".team-name-cell").InnerText.Replace("\n", string.Empty);

                NewTeam.Id = int.Parse(team.QuerySelector(".team-name-cell").ChildNodes["a"].Attributes["href"].Value.Split('/')[2]);

                var time = new TimePeriod();
                var from = long.Parse(team.QuerySelector(".time-period-cell").ChildNodes[0].Attributes["data-unix"].Value);
                var to = long.Parse(team.QuerySelector(".time-period-cell").ChildNodes[2].Attributes["data-unix"].Value);
                time.from = DateTimeFromUnixTimestampMillis(from);
                time.to = DateTimeFromUnixTimestampMillis(to);
                NewTeam.timePeriod = time;


                teams.Add(NewTeam);
            }

            return teams;
        }

        private static List<Models.Match> GetUpcomingPlayerMatches(HtmlNode document)
        {

            var MatchList = new List<Models.Match>();

            var table = document.SelectNodes("//table[@class='table-container match-table']")[0];
            var ht = new HtmlDocument();
            ht.LoadHtml(table.InnerHtml);

            table = ht.DocumentNode;

            for (var i = 0; i < 10; i++)
            {
                try
                {
                    if (table.SelectNodes("//tbody")[i] == null) { continue; }
                }
                catch
                {
                    continue;
                }

                var tbody = table.SelectNodes("//tbody")[i];



                var tbodyhtml = new HtmlDocument();
                tbodyhtml.LoadHtml(tbody.InnerHtml);

                HtmlNode tb = tbodyhtml.DocumentNode;

                foreach (var teamrow in tb.QuerySelectorAll(".team-row"))
                {
                    var Match = new Models.Match();

                    //Date
                    var date = long.Parse(teamrow.ChildNodes["td"].ChildNodes["span"].Attributes["data-unix"].Value);
                    Match.date = DateTimeFromUnixTimestampMillis(date);

                    var K = new HtmlDocument();
                    K.LoadHtml(teamrow.InnerHtml);

                    var s = K.DocumentNode;

                    var teamcell = s.SelectNodes("//td[@class='team-center-cell']");

                    //Team 1 name
                    Match.team1name = teamcell[0].ChildNodes["div"].ChildNodes["a"].InnerText;

                    //team 1 id 
                    Match.team1id = int.Parse(teamcell[0].ChildNodes["div"].ChildNodes["a"].Attributes["href"].Value.Split('/')[2]);

                    //team 1 icon url
                    Match.team1iconurl = teamcell[0].ChildNodes["div"].ChildNodes["span"].ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;

                    //team 2 name
                    Match.team2name = teamcell[0].ChildNodes[5].ChildNodes[1].InnerText;

                    //team 2 id
                    if(teamcell[0].ChildNodes[5].ChildNodes["a"] != null)
                    {
                        if(teamcell[0].ChildNodes[5].ChildNodes["a"].Attributes["href"] != null)
                        {
                            Match.team2id = int.Parse(teamcell[0].ChildNodes[5].ChildNodes["a"].Attributes["href"].Value.Split('/')[2]);
                        }
                    }
                    

                    //team 2 icon url
                    if(teamcell[0].ChildNodes[5].ChildNodes["span"].ChildNodes["img"] != null)
                    {
                        Match.team2iconurl = teamcell[0].ChildNodes[5].ChildNodes["span"].ChildNodes["img"].Attributes["src"].Value;
                    }
                    else
                    {
                        Match.team2iconurl = teamcell[0].ChildNodes[5].ChildNodes["span"].FirstChild.ChildNodes["img"].Attributes["src"].Value;
                    }
                    




                    MatchList.Add(Match);
                }



            }
            return MatchList;
        }




        private static List<Models.Match> GetRecentPlayerMatches(HtmlNode document)
        {

            var MatchList = new List<Models.Match>();

            var table = document.SelectNodes("//table[@class='table-container match-table']")[1];

            var ht = new HtmlDocument();
            ht.LoadHtml(table.InnerHtml);

            table = ht.DocumentNode;

            for (var i = 0; i < 10; i++)
            {
                try
                {
                    if (table.SelectNodes("//tbody")[i] == null) { continue; }
                }
                catch
                {
                    continue;
                }

                var tbody = table.SelectNodes("//tbody")[i];



                var tbodyhtml = new HtmlDocument();
                tbodyhtml.LoadHtml(tbody.InnerHtml);

                HtmlNode tb = tbodyhtml.DocumentNode;

                foreach (var teamrow in tb.QuerySelectorAll(".team-row"))
                {
                    var Match = new Models.Match();

                    //Date
                    var date = long.Parse(teamrow.ChildNodes["td"].ChildNodes["span"].Attributes["data-unix"].Value);
                    Match.date = DateTimeFromUnixTimestampMillis(date);

                    var K = new HtmlDocument();
                    K.LoadHtml(teamrow.InnerHtml);

                    var s = K.DocumentNode;

                    var teamcell = s.SelectNodes("//td[@class='team-center-cell']");

                    //id
                    Match.id = int.Parse(teamrow.QuerySelector(".stats-button-cell").FirstChild.Attributes["href"].Value.Split('/')[3]);

                    //Team 1 name
                    Match.team1name = teamcell[0].ChildNodes["div"].ChildNodes["a"].InnerText;

                    //team 1 id 
                    Match.team1id = int.Parse(teamcell[0].ChildNodes["div"].ChildNodes["a"].Attributes["href"].Value.Split('/')[2]);

                    //team 1 icon url
                    Match.team1iconurl = teamcell[0].ChildNodes["div"].ChildNodes["span"].ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;

                    //team 2 name
                    Match.team2name = teamcell[0].ChildNodes[5].ChildNodes["a"].InnerText;

                    //team 2 id
                    Match.team2id = int.Parse(teamcell[0].ChildNodes[5].ChildNodes["a"].Attributes["href"].Value.Split('/')[2]);

                    //team 2 icon url
                    Match.team2iconurl = teamcell[0].ChildNodes[5].ChildNodes["span"].ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;


                    //team 1 score
                    Match.team1Score = int.Parse(teamcell[0].ChildNodes[3].ChildNodes[0].InnerText);

                    //team 2 score
                    Match.team2Score = int.Parse(teamcell[0].ChildNodes[3].ChildNodes[2].InnerText);


                    MatchList.Add(Match);
                }



            }
            return MatchList;
        }

    }
}
