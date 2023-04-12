namespace HoneyScoop.FileHandling.FileTypes
{
    internal class FileTypeBmp
    {

        public string Header => @"\x42\x4D"; //  BMP signature
        public string Footer => @""; // No Footer Found


    }
}
