﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Layer2Converter
{
    class Program
    {
        const string INPUT_PATH = @"..\..\..\..\images\resized\";
        const string OUTPUT_PATH = @"..\..\..\..\basic\";
        const int BANK_SIZE = 16 * 1024;

        static void Main(string[] args)
        {
            Convert320x256("bridge.png");
            Convert256x256("watch.png");
        }

        static void Convert320x256(string inFile)
        {
            // Make some temporary data structure to hold the palette entries and pixel bytes.
            var pal = new Dictionary<Color, ushort>();
            var palindex = new Dictionary<Color, byte>();
            var pixels = new List<byte>();
            var palBytes = new List<byte>();
            // Create some output paths and filenames for the pixel banks and palette.
            string outPixels = Path.Combine(OUTPUT_PATH, Path.GetFileNameWithoutExtension(inFile));
            string outPal = Path.ChangeExtension(Path.Combine(OUTPUT_PATH, inFile), "pal");
            // Now open our inut file, using the .NET Bitmap abstraction.
            using (var img = Bitmap.FromFile(Path.Combine(INPUT_PATH, inFile), true) as Bitmap)
            {
                // .NET has a bunch of abstraction to deal with multiframe images like anigifs
                FrameDimension dimension = new FrameDimension(img.FrameDimensionsList[0]);
                int frameCount = img.GetFrameCount(dimension);
                for (int i = 0; i < frameCount; i++)
                {
                    byte palind = 0;
                    img.SelectActiveFrame(dimension, i);
                    // A 320x256 layer 2 image is 80K in size, with 10x 8K stripes going left to right across the page.
                    // Within each stripe, the X axis of the oroginal image goes from top to bottom,
                    // and the Y axis goes from left to right. So it looks like the image is flopped horizonally, then rotated 90 anticlockwise.
                    // We need two loops to process the pixels. The outer loop should be the original image X axis,
                    for (int x = 0; x < img.Width; x++)
                    {
                        // and the inner loop should be the original image Y axis.
                        for (int y = 0; y < img.Height; y++)
                        {
                            // Inside these two loops, we can get each pixel in turn.
                            // In .NET, these coloura is ARGB888.
                            var col = img.GetPixel(x, y);
                            // To make maximum use of Next 9bit colours, so we want to convert each one to RGB333.
                            // For each channel, take the top 3 (most significant) bits.
                            int r = col.R >> 5;
                            int g = col.G >> 5;
                            int b = col.B >> 5;
                            // Blue is a special case, because two of its bits are in one palette byte (in %RRRGGGBB format),
                            // and the other bit is in the other palette byte (in %xxxxxxxB format).
                            int blo = b >> 1;
                            int bhi = b & 1;
                            // Calculate the palette low byte (%RRRGGGBB)
                            int lo = (r << 5) + (g << 2) + blo;
                            // Calculate the palette high byte (%%xxxxxxxB)
                            int hi = bhi;
                            // Turn this into a 16bit value, for matching purposes
                            ushort val = (ushort)((hi << 8) + lo);
                            // We only want to maintain one copy of each ARGB8888 colour in our lookup list
                            if (!pal.ContainsKey(col))
                            {
                                // If we didn't already have it in our lookup list, add it with the ARGB8888 colour as the key and the 16bit value as the value.
                                pal.Add(col, val);
                                // Also add it to our list of palette indices, with the ARGB8888 colour as the key and the palette index as the value.
                                palindex.Add(col, palind);
                                // Also add the RGB333 colour to our list of raw palette bytes, in little-endian format.
                                // This is the raw data that will defined in the Next.
                                palBytes.Add(Convert.ToByte(lo));
                                palBytes.Add(Convert.ToByte(hi));
                                // Palette indices started at 0, so bump up the number for the next entry
                                palind++;
                            }
                            // Now we have a palette entry for the colour, we can canculate it's palette index (whether it was a new entry or not).
                            var colind = palindex[col];
                            // And then add a pixel for this palette entry. Pixels are always lookup entries into the palette.
                            pixels.Add(colind);
                        }
                    }
                    // Only process the first frame because we're not interested in anigifs, etc
                    break;
                }
            }
            // NextZXOS APIs can read an write files up to Int32.Max size, but NextBASIC can only LOAD one 16K BANK at a time,
            // so split up the 80K of pixels into five separate 16K files.
            var pixelBytes = pixels.ToArray();
            for (int i = 0; i < 5; i++)
            {
                string fn = outPixels + (i + 1) + ".bin";
                var pixelBank = new byte[BANK_SIZE];
                Array.Copy(pixelBytes, BANK_SIZE * i, pixelBank, 0, BANK_SIZE);
                File.WriteAllBytes(fn, pixelBank);
            }

            // 9 bit palettes are always 256 pairs of bytes, one for each palette entry.
            // We can load this from a single file into a single bank.
            var palByteArray = new byte[512];
            Array.Copy(palBytes.ToArray(), palByteArray, palBytes.Count);
            File.WriteAllBytes(outPal, palByteArray);
        }

        static void Convert256x256(string inFile)
        {
            // Make some temporary data structure to hold the palette entries and pixel bytes.
            var pal = new Dictionary<Color, ushort>();
            var palindex = new Dictionary<Color, byte>();
            var pixels = new List<byte>();
            var palBytes = new List<byte>();
            // Create some output paths and filenames for the pixel banks and palette.
            string outPixels = Path.Combine(OUTPUT_PATH, Path.GetFileNameWithoutExtension(inFile));
            string outPal = Path.ChangeExtension(Path.Combine(OUTPUT_PATH, inFile), "pal");
            // Now open our inut file, using the .NET Bitmap abstraction.
            using (var img = Bitmap.FromFile(Path.Combine(INPUT_PATH, inFile), true) as Bitmap)
            {
                // .NET has a bunch of abstraction to deal with multiframe images like anigifs
                FrameDimension dimension = new FrameDimension(img.FrameDimensionsList[0]);
                int frameCount = img.GetFrameCount(dimension);
                for (int i = 0; i < frameCount; i++)
                {
                    byte palind = 0;
                    img.SelectActiveFrame(dimension, i);
                    // A 256x192 layer 2 image is 48K in size, with 6x 8K stripes going top down across the page.
                    // Within each stripe, the X axis of the oroginal image goes from left to right, and the Y axis goes from top to bottom .
                    // We need two loops to process the pixels. The outer loop should be the original image Y axis,
                    for (int y = 0; y < img.Height; y++)
                    {
                        // and the inner loop should be the original image X axis.
                        for (int x = 0; x < img.Width; x++)
                        {
                            // Inside these two loops, we can get each pixel in turn.
                            // In .NET, these coloura is ARGB888.
                            var col = img.GetPixel(x, y);
                            // To make maximum use of Next 9bit colours, so we want to convert each one to RGB333.
                            // For each channel, take the top 3 (most significant) bits.
                            int r = col.R >> 5;
                            int g = col.G >> 5;
                            int b = col.B >> 5;
                            // Blue is a special case, because two of its bits are in one palette byte (in %RRRGGGBB format),
                            // and the other bit is in the other palette byte (in %xxxxxxxB format).
                            int blo = b >> 1;
                            int bhi = b & 1;
                            // Calculate the palette low byte (%RRRGGGBB)
                            int lo = (r << 5) + (g << 2) + blo;
                            // Calculate the palette high byte (%%xxxxxxxB)
                            int hi = bhi;
                            // Turn this into a 16bit value, for matching purposes
                            ushort val = (ushort)((hi << 8) + lo);
                            // We only want to maintain one copy of each ARGB8888 colour in our lookup list
                            if (!pal.ContainsKey(col))
                            {
                                // If we didn't already have it in our lookup list, add it with the ARGB8888 colour as the key and the 16bit value as the value.
                                pal.Add(col, val);
                                // Also add it to our list of palette indices, with the ARGB8888 colour as the key and the palette index as the value.
                                palindex.Add(col, palind);
                                // Also add the RGB333 colour to our list of raw palette bytes, in little-endian format.
                                // This is the raw data that will defined in the Next.
                                palBytes.Add(Convert.ToByte(lo));
                                palBytes.Add(Convert.ToByte(hi));
                                // Palette indices started at 0, so bump up the number for the next entry
                                palind++;
                            }
                            // Now we have a palette entry for the colour, we can canculate it's palette index (whether it was a new entry or not).
                            var colind = palindex[col];
                            // And then add a pixel for this palette entry. Pixels are always lookup entries into the palette.
                            pixels.Add(colind);
                        }
                    }
                    // Only process the first frame because we're not interested in anigifs, etc
                    break;
                }
            }
            // NextZXOS APIs can read an write files up to Int32.Max size, but NextBASIC can only LOAD one 16K BANK at a time,
            // so split up the 48K of pixels into three separate 16K files.
            var pixelBytes = pixels.ToArray();
            for (int i = 0; i < 3; i++)
            {
                string fn = outPixels + (i + 1) + ".bin";
                var pixelBank = new byte[BANK_SIZE];
                Array.Copy(pixelBytes, BANK_SIZE * i, pixelBank, 0, BANK_SIZE);
                File.WriteAllBytes(fn, pixelBank);
            }

            // 9 bit palettes are always 256 pairs of bytes, one for each palette entry.
            // We can load this from a single file into a single bank.
            var palByteArray = new byte[512];
            Array.Copy(palBytes.ToArray(), palByteArray, palBytes.Count);
            File.WriteAllBytes(outPal, palByteArray);
        }

    }
}
