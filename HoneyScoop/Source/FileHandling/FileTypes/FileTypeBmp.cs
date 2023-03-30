using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyScoop.Source.FileHandling.FileTypes
{
    internal class FileTypeBmp
    {

        public string Header => @"\x42\x4D"; //  BMP signature
        public string Footer => @""; // No Footer Found


    }
}
