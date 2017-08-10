using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NASClientTCP
{
    class Helpers
    {
        public static string BuildMessage(List<string> items)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string item in items)
            {
                builder.Append(item.ToString()).AppendLine();
            }
            return builder.ToString();
        }
    }
}
