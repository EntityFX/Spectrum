using System;
using System.Drawing;
using System.IO;
using EntityFX.Core.FileReader;
using EntityFX.Core.Platforms.Z48;

namespace EntityFX.Spectrum
{
    public class Program
    {
        static void Main()
        {
            var spectrum = new Z48Spectrum();
            //spectrum.LoadROM("C:\\Speccy\\Spectrum.rom");
            spectrum.LoadROM(@"C:\Users\SolopiyA\Downloads\zxmak.net.1.0.8.4\roms\48.rom");
            var bitmap = new Bitmap(256, 192);
            spectrum.Video.OutputBitmap = bitmap;
            int counter = 0;

            byte[] buffer = File.ReadAllBytes(@"C: \Users\SolopiyA\Downloads\zxmak.net.1.0.8.4\roms\MATCHD2.Z80");
            Format2 fileFormatz80 = new Format2();
            fileFormatz80.Read(buffer);
            spectrum.LoadRAMFromFile(fileFormatz80);

            while (true)
            {
                spectrum.Run();
                // spectrum.Video.Refresh();
                //Console.WriteLine(counter);
                //Console.Write(spectrum.ZilogZ80Cpu.Status.ToString());
                ++counter;
                //Console.SetCursorPosition(0, 0);
                //Console.Clear();
                if (counter % 100 == 0)
                {
                    bitmap.Save(String.Format("C:\\bmp\\{0}.bmp", counter));
                }
            }

        }
    }
}