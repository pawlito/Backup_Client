using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NASClientTCP
{
    interface IhashGenerator
    {
        string GetChecksumBuffered(Stream stream);
    }
}
