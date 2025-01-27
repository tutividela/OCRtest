﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using IronOcr;
using static OCRtest.ImageFunctions;
using static OCRtest.ServerFunctions;


namespace OCRtest
{
    class Program
    {
        static void Main()
        {
            Installation.LicenseKey = "IRONOCR.DEVTEAM.IRO211001.1586.56158.100112-9D7ACE374A-CTZUR5OSM7EAP-LOU76DHZ54CP-NO6PNMKW2L6Q-E4FEEQ2IKFOE-EFACSUJ5FCYS-SK6QO2-LIGVSQPV6M2HUA-PROFESSIONAL.SUB-PZBACF.RENEW.SUPPORT.01.OCT.2022 ";
            
            var Ocr = new IronTesseract();
            bool hit;

            string cutImagesPath = "C:/inetpub/wwwroot/Fotos/cut_images/";
            string regEx = "([0-9]+-[0-9]+-[2-9][0-9])|([C,c,P,p][0-9]+-[0-9]+-[2-9][0-9])";

            string sourceFiles = "C:/inetpub/wwwroot/Fotos/FotosOriginales/";
            

            string carpetaConUbicacion = "";
            string carpetaSinUbicacion = "";

            ManageLibraries(ref carpetaConUbicacion, ref carpetaSinUbicacion);
            

            string[] imagePathArray = Directory.GetFiles(sourceFiles);
            imagePathArray = imagePathArray.Where(w => w != imagePathArray[0]).ToArray();
            ManageDirectory(cutImagesPath);

            foreach (var imagePath in imagePathArray)
            {
                hit = false;

                Console.WriteLine("Trying {0} ...", Path.GetFileName(imagePath));

                Bitmap b = new Bitmap(imagePath);
                Rectangle r = new Rectangle(0, 2176, 3968, 800);
                Bitmap croppedImage = CropImage(b, r);
                string cutPath = cutImagesPath + "cut_" + Path.GetFileNameWithoutExtension(imagePath) + ".png";
                croppedImage.Save(cutPath);

                using (var input = new OcrInput(cutPath))
                {

                    OcrResult result = Ocr.Read(input);
                    foreach (var line in result.Lines)
                    {
                        MatchCollection mc = Regex.Matches(line.Text, regEx);
                        foreach (Match m in mc)
                        {
                            if (m.Success)
                            {
                                hit = SaveImageWithLocation(imagePath, carpetaConUbicacion, m);
                            }
                        }
                        if (hit) break;
                    }
                    if (!hit)
                    {
                        foreach (var line in result.Lines)
                        {
                            if (Regex.Match(line.Text, "[0-9]").Success)
                            {
                                string linePath =
                                        cutImagesPath
                                        + "line_"
                                        + line.LineNumber
                                        + "_"
                                        + Path.GetFileNameWithoutExtension(imagePath)
                                        + ".png";
                                line.ToBitmap(input).Save(linePath);
                                MatchCollection mc = Regex.Matches(Ocr.Read(linePath).Text, regEx);

                                foreach (Match m in mc)
                                {
                                    if (m.Success)
                                    {

                                        hit = SaveImageWithLocation(imagePath, carpetaConUbicacion, m);
                                    }

                                }
                                if (hit) break;
                            }
                        }
                        if (!hit)
                        {
                            Console.WriteLine("MISS");
                            SaveImageWithNoLocation(imagePath, carpetaSinUbicacion);
                        }

                    }

                }
            }
            ManageDirectory(cutImagesPath);

        }
    }
}
