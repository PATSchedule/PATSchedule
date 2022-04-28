using PATShared;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

const int EXIT_SUCCESS = 0;
const int EXIT_FAILURE = 1;

try
{
    // russo sparche fix
    Console.OutputEncoding = Encoding.UTF8;
    Console.InputEncoding = Encoding.UTF8;

    await Console.Out.WriteLineAsync("-- test тест --");

    var path = @"D:\Garbage\Downloads\2554file.pdf";
    using var ms = new MemoryStream(await File.ReadAllBytesAsync(path));
    var p = new PdfConverter(new System.Net.Http.HttpClient());
    await Console.Out.WriteLineAsync(await p.Convert(ms));

    return EXIT_SUCCESS;
}
catch (Exception exc)
{
    await Console.Error.WriteLineAsync("-- Exception:");
    await Console.Error.WriteLineAsync(exc.ToString());
    await Console.Error.WriteLineAsync("-- Exception end.");

    if (Debugger.IsAttached)
    {
        throw;
    }

    return EXIT_FAILURE;
}
