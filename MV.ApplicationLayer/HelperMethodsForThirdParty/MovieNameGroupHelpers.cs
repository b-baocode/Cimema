using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.HelperMethodsForThirdParty
{
    public static class MovieNameGroupHelpers
    {
        public static string GetGroupNameForMovie(string movieTitle)
        {
            var safeTitle = movieTitle.ToLower().Replace(":", "").Replace(" ", "-");
            return $"movie-{safeTitle}";
        }
    }
}
