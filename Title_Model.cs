using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDBImport
{
    public class Title_Model
    {
        public int TConst { get; set; }
        public string TitleType { get; set; }
        public string PrimaryTitle { get; set; }
        public string OriginalTitle { get; set; }
        public bool IsAdult { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public int? RuntimeMinutes { get; set; }

        public Title_Model(string[] titlesInfo)
        {
            TConst = int.Parse(titlesInfo[0].Substring(2));
            TitleType = titlesInfo[1];
            PrimaryTitle = titlesInfo[2];
            OriginalTitle = titlesInfo[3];
            IsAdult = titlesInfo[4] == "1";
            StartYear = ParseNullableInt(titlesInfo[5]);
            EndYear = ParseNullableInt(titlesInfo[6]);
            RuntimeMinutes = ParseNullableInt(titlesInfo[7]);
        }

        private int? ParseNullableInt(string value)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return null;
        }

        public override string ToString()
        {
            return TConst + " - " + PrimaryTitle;
        }
    }
}