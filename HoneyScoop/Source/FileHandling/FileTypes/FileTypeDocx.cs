namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypeDocx
{ 
    public string Header => @"\x50\x4B\x03\x04\x14\x00\x06\x00"; // Docx signature
    public string Footer => @"\x50\x4B\x05\x06"; // Docx footer, same as zip (EOCD)

}