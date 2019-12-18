/*
 * This is a simple CLI program that downloads an entire doujinshi/gallery
 * from the website nhentai.net based on the gallery ID which the user
 * inputs once the program is started.
*/

using System;
using System.Text.RegularExpressions;
using System.Net;

namespace nHentai_Downloader
{
    class Program
    {
        // A method that returns the extension that the image has based on the URL
        static string GetExtension(string ext)
        {
            if (ext.EndsWith(".jpeg"))
                return ".jpeg";
            else if (ext.EndsWith(".jpg"))
                return ".jpg";
            else if (ext.EndsWith(".png"))
                return ".png";
            else if (ext.EndsWith(".gif"))
                return ".gif";
            else if (ext.EndsWith(".webm"))
                return ".webm";
            else
                return "INVALID";
        }
        // A method that returns the page number based on the page count string
        static int PageCount(string pageMatch)
        {
            string[] pageString = pageMatch.Split(' ');
            return Convert.ToInt32(pageString[0]);
        }
        // A method that returns the gallery title based on the title string
        static string GetTitle(string nameMatch)
        {
            int titleLength = nameMatch.Length;
            return nameMatch.Substring(9, titleLength - 11);
        }
        static void Main(string[] args)
        {
            WebClient webClient = new WebClient();
            Regex pagePattern = new Regex("[\\0-9]* pages");
            Regex imagePattern = new Regex("https://i.nhentai.net/galleries/([a-zA-Z0-9\\/\\.]*)?", RegexOptions.IgnoreCase);
            Regex namePattern = new Regex("content=([a-zA-Z0-9\\ \\\"\\-]*)", RegexOptions.IgnoreCase);

            string galleryTemplate = "https://nhentai.net/g/";
            string imageHTML;

            // Getting user input and generating a gallery URL.
            Console.WriteLine("Enter a doujin ID: ");
            string doujinID = Console.ReadLine();
            string doujinURL = galleryTemplate + doujinID + "/";

            // Downloading the gallery HTML and finding the name and page count
            string galleryHTML = webClient.DownloadString(doujinURL);
            MatchCollection name = namePattern.Matches(galleryHTML);
            MatchCollection pageMatch = pagePattern.Matches(galleryHTML);

            // Outputing the gallery title and page count
            Console.WriteLine("Title: " + GetTitle(name[1].Value));
            Console.WriteLine(pageMatch[0].Value);

            // Creating a directory with the gallery ID as the name
            System.IO.Directory.CreateDirectory("./" + doujinID + "/");

            // Finding and downloading all the images from the gallery
            for (int i = 0; i < PageCount(pageMatch[0].Value); i++)
            {
                imageHTML = webClient.DownloadString(doujinURL + (i + 1) + "/");
                MatchCollection imageMatch = imagePattern.Matches(imageHTML);
                Console.WriteLine("Downloading: " + imageMatch[0].Value);
                try
                {
                    webClient.DownloadFile(imageMatch[0].Value, "./" + doujinID + "/" + i + GetExtension(imageMatch[0].Value));
                }
                catch(Exception x)
                {
                    try
                    {
                        Console.WriteLine("Error: Couldn't download image. Retrying...");
                        webClient.DownloadFile(imageMatch[0].Value, "./" + doujinID + "/" + i + GetExtension(imageMatch[0].Value));
                    }
                    catch(Exception y)
                    {
                        Console.WriteLine("Error: Couldn't download image. Please check your internet connection or if nhentai is down.");
                        break;
                    }
                }
            }

            Console.WriteLine("Done!");
            Console.ReadKey();
        }
    }
}
