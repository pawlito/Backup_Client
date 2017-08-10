using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NASClientTCP
{
    class CompareItems
    {
        public CompareItems()
        {

        }
        public Boolean CompareStrings(string str1, string str2)
        {
            Boolean returnCode = true;
            if (string.Compare(str1, str2) != 0)
            {
                returnCode = false;
            }

            return returnCode;
        }

        public int CompareDates(string date1, string date2)
        {
            DateTime d1, d2;
            try
            {
               DateTime.TryParse(date1, out d1);
               DateTime.TryParse(date2, out d2);
                if (d2 < d1)
                {
                    return 1;
                }
                else if (d1 == d2)
                {
                    return 0;
                }
                else
                    return -1;
            }
            catch
            {
                return -1;
            }
            return -1;
        }

    }
}
