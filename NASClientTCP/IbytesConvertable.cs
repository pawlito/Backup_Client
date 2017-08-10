using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NASClientTCP
{
    interface IbytesConvertable
    {
        byte[] GetBytesFromString(string str);
        string GetString(byte[] bytes);
    }
}
