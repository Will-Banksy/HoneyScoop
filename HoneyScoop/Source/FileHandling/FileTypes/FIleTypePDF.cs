using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyScoop.Source.FileHandling.FileTypes
{
    internal class FIleTypePdf
    {

        public string Header => @"\x25\x50\x44\x46\x2D"; // PDF signature
        public string Footer => @"(\x0A\x25\x25\x45\x4f\x46) | (\x0A\x25\x25\x45\x4F\x46\x0A) | (\x0D\x0A\x25\x25\x45\x4F\x46\x0D\x0A) | (\x0A\x25\x25\x45\x4F\x46\x0A)";


    }
}
