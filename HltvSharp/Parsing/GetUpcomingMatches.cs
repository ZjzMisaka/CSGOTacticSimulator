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
using System.Threading.Tasks;

namespace HltvSharp.Parsing
{
    public static partial class HltvParser
    {
        public static Task<List<UpcomingMatch>> GetUpcomingMatches(WebProxy proxy = null)
        {
            return FetchPage("matches", ParseMatchesPage, proxy);
        }

        private static List<UpcomingMatch> ParseMatchesPage(Task<HttpResponseMessage> response)
        {
            var content = response.Result.Content;
            string htmlContent = content.ReadAsStringAsync().Result;

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(htmlContent);

            HtmlNode document = html.DocumentNode;

            var upcomingMatchNodes = document.QuerySelectorAll(".upcomingMatch");

            List<UpcomingMatch> upcomingMatches = new List<UpcomingMatch>();

            foreach (HtmlNode upcomingMatchNode in upcomingMatchNodes)
            {
                try
                {
                    UpcomingMatch model = new UpcomingMatch();

                    //Match ID
                    string matchPageUrl = upcomingMatchNode.FirstChild.Attributes["href"].Value;
                    model.Id = int.Parse(matchPageUrl.Split('/')[1]);

                    //Match date
                    long unixDateMilliseconds = long.Parse(upcomingMatchNode.Attributes["data-zonedgrouping-entry-unix"].Value);
                    model.Date = DateTimeFromUnixTimestampMillis(unixDateMilliseconds);

                    //Event ID and name
                    Event eventModel = new Event();

                    eventModel.Id = 0; //couldn't get the id 
                    if(upcomingMatchNode.QuerySelector(".matchEventName") != null)
                    {
                        eventModel.Name = upcomingMatchNode.QuerySelector(".matchEventName").InnerText;
                        model.Event = eventModel;
                    }
                    else
                    {
                        continue;
                    }
                    

                    //Number of stars
                    model.Stars = upcomingMatchNode.QuerySelectorAll(".stars i").Count();

                    
                    var resultScoreNode = upcomingMatchNode.QuerySelector(".result-score");

                    //Team 1 ID and name
                    Team team1Model = new Team();

                    team1Model.Id = 0; //couldn't get the id 
                    team1Model.Name = document.SelectNodes("//div[@class='matchTeam team1']")[0].ChildNodes[3].InnerText;
                    model.Team1 = team1Model;

                    //Team 2 ID and name
                    Team team2Model = new Team();

                    team2Model.Id = 0; //couldn't get the id 
                    team2Model.Name = document.SelectNodes("//div[@class='matchTeam team2']")[0].ChildNodes[3].InnerText;
                    model.Team2 = team2Model;

                    //Map and format
                    string mapText = upcomingMatchNode.QuerySelector(".matchMeta").InnerText;
                    if (mapText.Contains("bo"))
                        model.Format = mapText;
                    else
                    {
                        model.Format = "bo1";
                        model.Map = MapSlug.MapSlugs[mapText];
                    }

                    upcomingMatches.Add(model);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return upcomingMatches;
        }
    }
}
