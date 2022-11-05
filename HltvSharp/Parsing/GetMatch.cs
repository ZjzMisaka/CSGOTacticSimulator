using Fizzler.Systems.HtmlAgilityPack;
using HltvSharp.Models;
using HltvSharp.Models.Enums;
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
        public static Task<FullMatch> GetMatch(int id, WebProxy proxy = null)
        {
            return FetchPage($"matches/{id}/-", (response) => ParseMatchPage(response, id), proxy);
        }

        private static FullMatch ParseMatchPage(Task<HttpResponseMessage> response, int id = 0)
        {
            var content = response.Result.Content;
            string htmlContent = content.ReadAsStringAsync().Result;

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(htmlContent);

            HtmlNode document = html.DocumentNode;

            FullMatch model = new FullMatch();

            model.Id = id;
            
            //Match date
            long date = long.Parse(document.QuerySelector(".timeAndEvent .date").Attributes["data-unix"].Value);
            model.Date = DateTimeFromUnixTimestampMillis(date);

            //Match format
            try
            {
                string preformattedText = document.QuerySelector(".preformatted-text").InnerText;
                model.Format = preformattedText.Split('\n').First();
                model.AdditionalInfo = preformattedText.Substring(preformattedText.IndexOf('\n') + 1);
            }
            catch (Exception)
            {
                model.Format = "Best of 1";
            }


           
            //Team 1
            Team team1 = new Team();
            team1.Id = int.Parse(document.QuerySelectorAll(".team a").First().Attributes["href"].Value.Replace("/team/", string.Empty).Split('/').First());
            team1.Name = document.QuerySelectorAll(".team img.logo").First().Attributes["title"].Value;
            model.Team1 = team1;

            //Team 2
            Team team2 = new Team();
            team2.Id = int.Parse(document.QuerySelectorAll(".team a").First().Attributes["href"].Value.Replace("/team/", string.Empty).Split('/').First());
            team2.Name = document.QuerySelectorAll(".team img.logo").Last().Attributes["title"].Value;
            model.Team2 = team2;

            //Winning team
            if(document.QuerySelector(".team1-gradient > div") != null)
            {
                if (document.QuerySelector(".team1-gradient > div").HasClass("won"))
                    model.WinningTeam = team1;
            }

            if (document.QuerySelector(".team2-gradient > div") != null)
            {
                if (document.QuerySelector(".team2-gradient > div").HasClass("won"))
                    model.WinningTeam = team2;
            }
                

            //Event
            Event matchEvent = new Event();
            matchEvent.Name = document.QuerySelector(".timeAndEvent .event a").Attributes["title"].Value;
            matchEvent.Id = int.Parse(document.QuerySelector(".timeAndEvent .event a").Attributes["href"].Value.Split('/')[1]);
            model.Event = matchEvent;

            //Maps
            var mapHolderNodes = document.QuerySelectorAll(".mapholder");

            List<MapResult> mapResults = new List<MapResult>();
            foreach (var mapHolderNode in mapHolderNodes)
            {
                MapResult mapResult = new MapResult();
                mapResult.Name = mapHolderNode.QuerySelector(".mapname").InnerText;
                if(mapHolderNode.QuerySelector(".results") != null)
                {
                    var resultsNode = mapHolderNode.QuerySelector(".results");
                    if (resultsNode.QuerySelectorAll(".results-team-score") != null)
                    {
                        var scoreNodes = resultsNode.QuerySelectorAll(".results-team-score").ToList();
                        if (scoreNodes.Count > 0 && scoreNodes[0].InnerText != "-")
                        {
                            mapResult.Team1Score = int.Parse(scoreNodes[0].InnerText);
                            mapResult.Team2Score = int.Parse(scoreNodes[1].InnerText);
                            if (mapHolderNode.QuerySelector(".results-stats") != null)
                                mapResult.StatsId = int.Parse(mapHolderNode.QuerySelector(".results-stats").Attributes["href"].Value.Split('/')[3]);
                        }

                        mapResults.Add(mapResult);
                    }
                }
               
                
            }
            model.Maps = mapResults.ToArray();

            //Demos
            var demoNodes = document.QuerySelectorAll(".stream-box").Where(node => node.Attributes["data-stream-embed"] == null);
            List<Demo> demos = new List<Demo>();
            foreach (var demoNode in demoNodes)
            {
                if (demoNode.QuerySelector("a") == null || !demoNode.QuerySelector("a").Attributes.Contains("href"))
                    continue;

                Demo demo = new Demo();
               if(demoNode.FirstChild.Name == "a")
                {
                    string demoDownloadUrl = "https://www.hltv.org" + demoNode.FirstChild.Attributes["href"].Value;
                    demo.Name = demoNode.QuerySelector("a").InnerText;
                    demo.Url = GetDemoDirectDownloadUrl(demoDownloadUrl);
                    demos.Add(demo);
                }
            }
            model.Demos = demos.ToArray();

            //Veto
            var vetoNodes = document.QuerySelectorAll(".veto-box .padding > div");
            List<Veto> vetos = new List<Veto>();
            foreach (var vetoNode in vetoNodes)
            {
                Veto veto = new Veto();
                string cleanVeto = Regex.Replace(vetoNode.InnerText.Trim(), @"^\d.", "").Trim();

                bool containsPicked = vetoNode.InnerText.ToLower().Contains("picked");
                bool containsRemoved = vetoNode.InnerText.ToLower().Contains("removed");

                string teamName = "";
                string mapName = "";
                string action = containsPicked ? "picked" : "removed";

                if (containsPicked)
                {
                    teamName = cleanVeto.Split(" picked ".ToCharArray())[0];
                    mapName = cleanVeto.Split(" picked ".ToCharArray())[1];
                }
                if (containsRemoved)
                {
                    teamName = cleanVeto.Split(" removed ".ToCharArray())[0];
                    mapName = cleanVeto.Split(" removed ".ToCharArray())[1];
                }

                if (mapName == "" || teamName == "")
                {
                    mapName = cleanVeto.Split(" ".ToCharArray())[0];
                    action = "other";
                }

                if (teamName != "")
                    veto.Team = team1.Name == teamName ? team1 : team2;
                veto.Map = mapName;
                veto.Action = action;
                vetos.Add(veto);
            }
            model.Vetos = vetos.ToArray();

            //Team 1 players
            var team1PlayersHolderNode = document.QuerySelectorAll("div.players").First();
            var team1PlayersFlagAllignNodes = team1PlayersHolderNode.QuerySelectorAll(".flagAlign");
            List<Player> team1Players = new List<Player>();
            foreach (var flagAllignNode in team1PlayersFlagAllignNodes)
            {
                Player player = new Player();
                player.Name = flagAllignNode.QuerySelector(".text-ellipsis").InnerText;
                if (flagAllignNode.ParentNode.Attributes["href"] != null && flagAllignNode.ParentNode.Attributes["href"].Value != null)
                    player.Id = int.Parse(flagAllignNode.ParentNode.Attributes["href"].Value.Split('/')[1]);
                team1Players.Add(player);
            }
            model.Team1Players = team1Players.ToArray();

            //Team 2 players
            var team2PlayersHolderNode = document.QuerySelectorAll("div.players").Last();
            var team2PlayersFlagAllignNodes = team2PlayersHolderNode.QuerySelectorAll(".flagAlign");
            List<Player> team2Players = new List<Player>();
            foreach (var flagAllignNode in team2PlayersFlagAllignNodes)
            {
                Player player = new Player();
                player.Name = flagAllignNode.QuerySelector(".text-ellipsis").InnerText;
                if (flagAllignNode.ParentNode.Attributes["href"] != null && flagAllignNode.ParentNode.Attributes["href"].Value != null)
                    player.Id = int.Parse(flagAllignNode.ParentNode.Attributes["href"].Value.Split('/')[1]);
                team2Players.Add(player);
            }
            model.Team2Players = team2Players.ToArray();

            //playermatchstats


            HtmlNode table = null;
            HtmlNode table2 = null;
            var textnode = document.SelectNodes("//div[@class='headline']");
            if(textnode != null)
            {
                var up = textnode.Where(test => test.InnerText.Contains("Match stats"));

                if (up != null)
                {
                    var rec = up.First().ParentNode.NextSibling.NextSibling.NextSibling.NextSibling.ChildNodes[1];
                    if (rec.Name == "table")
                    {
                        table = rec;
                    }
                    var rec2 = up.First().ParentNode.NextSibling.NextSibling.NextSibling.NextSibling.ChildNodes[7];
                    if (rec2.Name == "table")
                    {
                        table2 = rec2;
                    }

                }

                if (table != null)
                {
                    var rowlist = new List<MatchStat>();
                    foreach (var row in table.QuerySelectorAll("tr").Skip(1))
                    {
                        var stat = new MatchStat();

                        //playerID
                        var pid = row.ChildNodes["td"].ChildNodes["div"].ChildNodes["a"].Attributes["href"].Value.Split('/')[2];
                        stat.PlayerID = int.Parse(pid);



                        //PlayerName
                        stat.PlayerName = row.ChildNodes["td"].ChildNodes["div"].ChildNodes["a"].ChildNodes["div"].InnerText;

                        //K-D
                        stat.KD = row.QuerySelector(".kd").InnerText;

                        // +/-
                        var pm = row.QuerySelector(".plus-minus").InnerText;
                        stat.plusminus = int.Parse(pm);

                        //ADR
                        var adr = row.QuerySelector(".adr").InnerText;
                        stat.ADR = decimal.Parse(adr.Replace(".", ","));

                        //Kast%
                        var kast = row.QuerySelector(".kast").InnerText;
                        stat.KastProcentage = decimal.Parse(kast.Replace("%", string.Empty).Replace(".", ","));

                        //Rating
                        var r = row.QuerySelector(".rating").InnerText;
                        stat.Rating = decimal.Parse(r.Replace(".", ","));


                        rowlist.Add(stat);
                    }

                    model.Team1PlayerStats = rowlist;
                }

                if (table2 != null)
                {
                    var rowlist2 = new List<MatchStat>();
                    foreach (var row in table2.QuerySelectorAll("tr").Skip(1))
                    {
                        var stat = new MatchStat();

                        //playerID
                        var pid = row.ChildNodes["td"].ChildNodes["div"].ChildNodes["a"].Attributes["href"].Value.Split('/')[2];
                        stat.PlayerID = int.Parse(pid);



                        //PlayerName
                        stat.PlayerName = row.ChildNodes["td"].ChildNodes["div"].ChildNodes["a"].ChildNodes["div"].InnerText;

                        //K-D
                        stat.KD = row.QuerySelector(".kd").InnerText;

                        // +/-
                        var pm = row.QuerySelector(".plus-minus").InnerText;
                        stat.plusminus = int.Parse(pm);

                        //ADR
                        var adr = row.QuerySelector(".adr").InnerText;
                        stat.ADR = decimal.Parse(adr.Replace(".", ","));

                        //Kast%
                        var kast = row.QuerySelector(".kast").InnerText;
                        stat.KastProcentage = decimal.Parse(kast.Replace("%", string.Empty).Replace(".", ","));

                        //Rating
                        var r = row.QuerySelector(".rating").InnerText;
                        stat.Rating = decimal.Parse(r.Replace(".", ","));


                        rowlist2.Add(stat);
                    }
                    model.Team2PlayerStats = rowlist2;
                }

            }


            return model;
        }

        //https://stackoverflow.com/a/47806360
        static string GetDemoDirectDownloadUrl(string demoDownloadUrl)
        {
            var request = (HttpWebRequest)WebRequest.Create(demoDownloadUrl);
            request.Method = "HEAD";
            request.AllowAutoRedirect = false;

            string location = "";

            try
            {
                using (var reg = request.GetResponse() as HttpWebResponse)
                {
                   location = reg.Headers["Location"];
                }
            }
            catch (WebException e)
            {
                location = e.Response.Headers["Location"];
            }

            return location;
        }
    }
}
