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
        public static Task<List<RankedTeam>> GetRankings(WebProxy proxy = null)
        {
            return FetchPage("ranking/teams/", GetRankingslist, proxy);
        }
        private static List<RankedTeam> GetRankingslist(Task<HttpResponseMessage> response)
        {
            var content = response.Result.Content;
            string htmlContent = content.ReadAsStringAsync().Result;

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(htmlContent);

            HtmlNode document = html.DocumentNode;

            var RankedTeamList = new List<RankedTeam>();

            foreach(var team in document.QuerySelectorAll(".ranked-team"))
            {
                var rankedTeam = new RankedTeam();

                //id
                var id = team.QuerySelector(".more").FirstChild.Attributes["href"].Value.Split('/')[2];
                rankedTeam.Id = int.Parse(id);

                //name
                rankedTeam.Name = team.QuerySelector(".name").InnerText;

                //Rank
                var rank = team.QuerySelector(".position").InnerText.Replace("#", string.Empty);
                rankedTeam.Rank = int.Parse(rank);

                //points
                var points = team.QuerySelector(".points").InnerText;
                points = Regex.Replace(points, "[^0-9]", "");
                rankedTeam.Points = int.Parse(points);

                //change
                var change = team.QuerySelector(".change").InnerText;
                if(change == "-")
                {
                    rankedTeam.change = null;
                }
                else
                {
                    rankedTeam.change = int.Parse(change);
                }


                RankedTeamList.Add(rankedTeam); 
            }

            return RankedTeamList;
        }
    }
}
