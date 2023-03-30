using System.Text;
using static System.Buffers.Binary.BinaryPrimitives;
using System;
using System.Collections.Generic;
using System.IO;

namespace HoneyScoop.FileHandling.FileTypes.encoding

// TODO: Write the header and footer signatures using Regex e.g. \x00\x00 instead of 00 and \x47\x49\x46\x08 instead of GIF8
// Also look at this: https://en.wikipedia.org/wiki/GIF#File_format
// (Can use regex with the header to match either GIF87a or GIF89a and do some more research on the footer cause 0x0000 is likely to come up a lot so if that is the only viable to use footer that's an issue)


namespace HoneyScoop.FileHandling.FileTypes
{
    internal class FileTypeGif : IFileType
    {
        public string Header => "GIF"; // GIF signature
        public string Footer => ";\0"; // GIF terminator

        private const int HeaderSize = 3;
        private const int FooterSize = 2;

        private const byte Gif87aVersion = 0x37;
        private const byte Gif89aVersion = 0x39;

        private readonly ref struct LogicalScreenDescriptor
        {
            internal readonly ushort Width;
            internal readonly ushort Height;
            internal readonly byte Flags;
            internal readonly byte BackgroundColorIndex;
            internal readonly byte PixelAspectRatio;

            internal LogicalScreenDescriptor(ReadOnlySpan<byte> data)
            {
                Width = BitConverter.ToUInt16(data.Slice(0, 2));
                Height = BitConverter.ToUInt16(data.Slice(2, 2));
                Flags = data[4];
                BackgroundColorIndex = data[5];
                PixelAspectRatio = data[6];
            }
        }

        private readonly ref struct ImageDescriptor
        {
            private readonly ushort Left;
            internal readonly ushort Top;
            internal readonly ushort Width;
            internal readonly ushort Height;
            internal readonly byte Flags;

            internal ImageDescriptor(ReadOnlySpan<byte> data)
            {
                Left = BitConverter.ToUInt16(data.Slice(0, 2));
                Top = BitConverter.ToUInt16(data.Slice(2, 2));
                Width = BitConverter.ToUInt16(data.Slice(4, 2));
                Height = BitConverter.ToUInt16(data.Slice(6, 2));
                Flags = data[8];
            }
        }

        private readonly ref struct ColorTable
        {
            internal readonly byte[] Data;

            internal ColorTable(ReadOnlySpan<byte> data, int size)
            {
                Data = new byte[size * 3]; // 3 bytes per color (RGB)
                data.Slice(0, Data.Length).CopyTo(Data);
            }

            internal byte[] GetColor(int index)
            {
                if (index < Data.Length / 3)
                {
                    int offset = index * 3;
                    return new byte[] { Data[offset], Data[offset + 1], Data[offset + 2] };
                }
                return null;
            }
        }

        private enum BlockType : byte
        {
            Extension = 0x21,
            Image = 0x2C,
            Terminator = 0x3B
        }

        private enum ExtensionLabel : byte
        {
            GraphicsControl = 0xF9,
            Comment = 0xFE,
            Application = 0xFF,
            PlainText = 0x01
        }

        private enum GraphicsControlDisposalMethod : byte
        {
            Unspecified = 0,
            DoNotDispose = 1,
            RestoreBackground = 2,
            RestorePrevious = 3
        }

        private const byte GraphicsControlTransparencyFlag = 0x01;

        private readonly ref struct GraphicsControlExtension
        {
            internal readonly byte BlockSize;
            internal readonly byte Flags;
            internal readonly ushort DelayTime;
            internal readonly byte TransparentColorIndex;
            internal readonly GraphicsControlDisposalMethod DisposalMethod;

            internal GraphicsControlExtension(ReadOnlySpan<byte> data)
            {
                BlockSize = data[0];
                Flags = data[1];
                DelayTime = BitConverter.ToUInt16(data.Slice(2, 2));
                TransparentColorIndex = data[4];
                DisposalMethod = (GraphicsControlDisposalMethod)((Flags >> 2) & 0x07);
            }

	        private readonly struct ImageData
        {
            internal readonly byte[] LZWMinimumCodeSize;
            internal readonly byte[] Data;

            internal ImageData(ReadOnlySpan<byte> data)
            {
                // The LZWMinimumCodeSize is always the first byte of the Image Data block
                LZWMinimumCodeSize = new byte[] { data[0] };

                // The rest of the data is the compressed image data
                Data = new byte[data.Length - 1];
                data.Slice(1).CopyTo(Data);
            }
        }

        public bool IsValidFileType(ReadOnlySpan<byte> fileData)
        {
            if (fileData.Length < HeaderSize + FooterSize)
            {
                return false;
            }

            // Check header signature
            if (!fileData.Slice(0, HeaderSize).SequenceEqual(Header))
            {
                return false;
            }

            // Check footer terminator
            if (!fileData.Slice(fileData.Length - FooterSize, FooterSize).SequenceEqual(Footer))
            {
                return false;
            }

            // Check version
            byte versionByte = fileData[3];
            if (versionByte != Gif87aVersion && versionByte != Gif89aVersion)
            {
                return false;
            }

            return true;
        }

        public FileTypeInfo GetFileType(ReadOnlySpan<byte> fileData)
        {
            if (!IsValidFileType(fileData))
            {
                return null;
            }

            // Determine GIF version
            byte versionByte = fileData[3];
            string version = versionByte == Gif87aVersion ? "87a" : "89a";

            // Parse Logical Screen Descriptor
            LogicalScreenDescriptor logicalScreenDescriptor = new LogicalScreenDescriptor(fileData.Slice(6, 7));

            // Determine color table size (based on Flags field of Logical Screen Descriptor)
            int colorTableSize = 1 << ((logicalScreenDescriptor.Flags & 0x07) + 1);

            // Parse Global Color Table (if present)
            ColorTable globalColorTable = null;
            if ((logicalScreenDescriptor.Flags & 0x80) == 0x80)
            {
                globalColorTable = new ColorTable(fileData.Slice(13, colorTableSize), colorTableSize);
            }

            // Search for blocks until the terminator is found
            int position = 13 + colorTableSize * 3;
            while (fileData[position] != (byte)BlockType.Terminator)
            {
                switch ((BlockType)fileData[position])
                {
                    case BlockType.Extension:
                        position = ParseExtensionBlock(fileData, position, globalColorTable);
                        break;

                    case BlockType.Image:
                        position = ParseImageBlock(fileData, position, globalColorTable);
                        break;

                    default:
                        // Invalid block type, so we can't continue parsing
                        return null;
                }
            }

            return new FileTypeInfo("GIF", "Graphics Interchange Format", version);
        }