*See [searchlight](https://github.com/Will-Banksy/searchlight) for an advancement on methods implemented here (but for a lesser variety of file types)*

# HoneyScoop

# Documentation

HoneyScoop is a command-line tool developed in C# that aims to improve file
type aware extraction methods for digital forensics investigators. These methods
will provide a faster and more efficient workflow for file carving and recognition
from raw data. HoneyScoop supports various file formats including jpg, png, gif,
mp4, mp3, wav, xlsx, pdf, docx, pptx, and zip.

HoneyScoop accepts the following command line arguments:

- - i, --input_file: (Required) Specifies the input file to conduct file
    reconstruction on.
- - v, --verbose: (Optional) Sets the output to verbose messages, providing
    more detailed information during the processing.
- - q, --quiet: (Optional) Sets whether to view output or not, providing a
    quieter mode of operation.
- - O, --no_organise: (Optional) Disables organizing the carved files by type. By
    default, files are organized into subdirectories by type.
- - T, --timestamp: (Optional) Enables timestamping the output directories,
    providing timestamps for the directories created during processing.
- - o, --output: (Optional) Specifies the output directory path. By default, the
    current directory is used as the output directory.
- - c, --types: (Optional) Specifies the types of files to process, separated by
    commas. Supported file types can be selected from jpg, png, gif, mp4, mp3,
    wav, xlsx, pdf, docx, pptx, and zip.

Example usage:

![Screenshot from 2023-05-05 20-23-21](https://user-images.githubusercontent.com/100227246/236551311-2c9bf2af-f8b9-4507-b5f8-e2b5713fe18e.png)

This command will run HoneyScoop with the input file "input.raw", output the
reconstructed files to the specified "output_directory" path, process only jpg,
png, and mp4 file types, and display verbose messages during processing.
HoneyScoop will also create directories for each supported file type in the output
directory (unless disabled using the -O, --no_organise option), and carve and
recognize files of those types from the input file. The reconstructed files will be
saved in the respective directories based on their file types.

When it comes to error handling, HoneyScoop performs basic error handling for
invalid or missing command line arguments. It checks if the input file exists and if
the specified file types are supported. If any errors are encountered, appropriate
error messages will be displayed, and the tool will exit gracefully.

In conclusion, HoneyScoop is a powerful and efficient command-line tool for file
carving and recognition in digital forensics investigations. It improves file type
aware extraction methods by independently looking for file type specific
properties, providing faster and more efficient processing. With support for
various file formats and options for output organization and verbosity,
HoneyScoop is a valuable tool for digital forensics investigators, which can be
easily combined with OpenForensics.

# Install

1. Open a terminal session
2. Type in: git clone https://github.com/Will-Banksy/HoneyScoop
3. Change into the directory
4. run ./HoneyScoop.exe -h, which will show help screen with the usable commands
