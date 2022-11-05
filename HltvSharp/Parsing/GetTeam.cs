using Fizzler.Systems.HtmlAgilityPack;
using HltvSharp.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace HltvSharp.Parsing
{
    public static partial class HltvParser
    {

        public static Task<Team> GetTeam(int teamid, WebProxy proxy = null)
        {
            return FetchPage($"team/{teamid}/-", (response) => GetInfoParse(response, teamid), proxy);
        }

        private static Team GetInfoParse(Task<HttpResponseMessage> response, int id = 0)
        {

            //load html
            var content = response.Result.Content;
            string htmlContent = content.ReadAsStringAsync().Result;

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(htmlContent);

            HtmlNode document = html.DocumentNode;

            //start of actual code
            var team = new Team();

            //name
            team.Name = document.QuerySelector(".profile-team-name").InnerText;

            //country
            team.Country = document.QuerySelector(".team-country").InnerText;

            //id
            team.Id = int.Parse(document.SelectNodes("//link[@rel='canonical']")[0].Attributes["href"].Value.Split('/')[4]);

            //Team stats
            var profileteamstats = document.QuerySelectorAll(".profile-team-stat").ToArray();

            //WorldRanking
            if (int.TryParse(profileteamstats[0].ChildNodes["span"].InnerText.Replace("#", string.Empty), out var rank))
            {
                team.WorldRank = rank;
            }


            //AveragePlayerAge
            if (profileteamstats.ElementAtOrDefault(2) != null)
            {
                if (profileteamstats[2].InnerText.Contains("Average player age"))
                {
                    team.AveragePlayerAge = double.Parse(profileteamstats[2].ChildNodes["span"].InnerText.Replace(".", ","));
                }
            }

            //winrate
            if (double.TryParse(document.SelectNodes("//div[@class='highlighted-stat']")[1].ChildNodes["div"].InnerText.Replace("%", String.Empty).Replace(".", ","), out var winrate))
            {
                team.winRateProcentage = winrate;
            }

            //Coach
            if (profileteamstats.ElementAtOrDefault(3) != null)
            {
                if (profileteamstats[3].InnerText.Contains("Coach"))
                {
                    var Coach = new Coach();

                    //id
                    Coach.id = int.Parse(profileteamstats[3].ChildNodes["a"].Attributes["href"].Value.Split("/".ToCharArray())[2]);

                    //country
                    Coach.Country = profileteamstats[3].ChildNodes["a"].ChildNodes["img"].Attributes["title"].Value;

                    //firstname
                    Coach.Name = profileteamstats[3].ChildNodes["a"].InnerText;

                    team.Coach = Coach;
                }
            }




            team.Players = GetPlayers(document);

            team.RecentMatches = GetRecentMatches(document);

            team.UpcomingMatches = GetUpcomingMatches(document);

            return team;
        }

        private static List<Player> GetPlayers(HtmlNode document)
        {
            var PlayerList = new List<Player>();

            if (!document.InnerHtml.Contains("table-container players-table"))
            {
                return null;
            }

            var table = document.SelectNodes("//table[@class='table-container players-table']")[0];
            var ht = new HtmlDocument();
            ht.LoadHtml(table.InnerHtml);

            table = ht.DocumentNode;

            var tbody = table.SelectNodes("//tbody")[0];

            var tbodyhtml = new HtmlDocument();
            tbodyhtml.LoadHtml(tbody.InnerHtml);

            HtmlNode tb = tbodyhtml.DocumentNode;

            foreach (var PlayerCellFE in tb.SelectNodes("//tr"))
            {
                var Player = new Player();

                var htm = new HtmlDocument();
                htm.LoadHtml(PlayerCellFE.InnerHtml);

                var PlayerCell = htm.DocumentNode;
                //id
                Player.Id = int.Parse(PlayerCell.ChildNodes["td"].ChildNodes["a"].Attributes["href"].Value.Split('/')[2]);

                //name
                var name = "";


                name = PlayerCell.SelectNodes("//img")[0].Attributes["title"].Value;





                if (name == null || name == "")
                {
                    throw new Exception("Player name was null");
                }
                Player.Name = name;

                //Player image
                var imgurl = "";


                imgurl = PlayerCell.SelectNodes("//img")[0].Attributes["src"].Value;


                if (imgurl == null || imgurl == "")
                {
                    throw new Exception("Player Image url was null");
                }
                Player.playerImgUrl = imgurl;

                //Country
                Player.Country = PlayerCell.SelectNodes("//img[@class='gtSmartphone-only flag']")[0].Attributes["title"].Value;

                //status
                Player.status = PlayerCell.QuerySelector(".player-status").InnerText;

                //Time on Team
                Player.timeOnTeam = PlayerCell.SelectNodes("//td")[2].ChildNodes["div"].InnerText;

                //Maps played
                Player.mapsPlayed = int.Parse(PlayerCell.SelectNodes("//td")[3].ChildNodes["div"].InnerText);

                //Rating
                if (PlayerCell.SelectNodes("//td")[4].ChildNodes["div"].InnerText != "-")
                {
                    Player.rating = double.Parse(PlayerCell.SelectNodes("//td")[4].ChildNodes["div"].InnerText.Replace(".", ","));
                }


                PlayerList.Add(Player);
            }


            return PlayerList;
        }

        private static List<Match> GetUpcomingMatches(HtmlNode document)
        {

            if (!document.InnerHtml.Contains("table-container match-table"))
            {
                return null;
            }
            var MatchList = new List<Match>();


            //get the table
            HtmlNode table = null;
            var textnode = document.SelectNodes("//h2[@class='standard-headline']");
            var up = textnode.Where(test => test.InnerText.Contains("Upcoming matches"));
            if (up != null)
            {
                var rec = up.First().NextSibling.NextSibling;
                if (rec.Name == "table")
                {
                    table = rec;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            foreach (var teamrow in table.QuerySelectorAll(".team-row"))
                {
                    var Match = new Match();

                    if (teamrow.QuerySelector(".matchpage-button-cell") == null)
                    {
                        var id2 = teamrow.QuerySelector(".stats-button-cell").ChildNodes["a"].Attributes["href"].Value.Split('/')[2];
                        Match.id = int.Parse(id2);
                    }
                    else
                    {
                        var id2 = teamrow.QuerySelector(".matchpage-button-cell").ChildNodes["a"].Attributes["href"].Value.Split('/')[2];
                        Match.id = int.Parse(id2);
                    }



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
                    if (teamcell[0].ChildNodes[5].ChildNodes["a"] != null)
                    {
                        if (teamcell[0].ChildNodes[5].ChildNodes["a"].Attributes["href"] != null)
                        {
                            int.TryParse(teamcell[0].ChildNodes[5].ChildNodes["a"].Attributes["href"].Value.Split('/')[2], out var id);
                            Match.team2id = id;
                        }
                    }


                    //team 2 icon url


                    if (teamcell[0].ChildNodes[5].ChildNodes["span"].ChildNodes["a"] == null)
                    {
                        Match.team2iconurl = teamcell[0].ChildNodes[5].ChildNodes["span"].ChildNodes["img"].Attributes["src"].Value;
                    }
                    else
                    {
                        Match.team2iconurl = teamcell[0].ChildNodes[5].ChildNodes["span"].ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;
                    }





                    MatchList.Add(Match);
                }



            
            return MatchList;
        }




        private static List<Match> GetRecentMatches(HtmlNode document)
        {

            if (!document.InnerHtml.Contains("table-container match-table"))
            {
                return null;
            }
            var MatchList = new List<Match>();


            //get the table
            HtmlNode table = null;
            var textnode = document.SelectNodes("//h2[@class='standard-headline']");
            var up = textnode.Where(test => test.InnerText.Contains("Recent results"));
            if (up != null)
            {
                var rec = up.First().NextSibling.NextSibling;
                if (rec.Name == "table")
                {
                    table = rec;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }


            foreach (var teamrow in table.QuerySelectorAll(".team-row"))
            {
                var Match = new Match();

                var id = teamrow.QuerySelector(".stats-button-cell").ChildNodes["a"].Attributes["href"].Value.Split('/')[2];
                Match.id = int.Parse(id);

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




            return MatchList;
        }



    }
}
