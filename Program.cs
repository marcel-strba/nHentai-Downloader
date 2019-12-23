/*
 * This is a simple CLI program that downloads an entire doujinshi/gallery
 * from the website nhentai.net based on the gallery ID which the user
 * inputs once the program is started.
*/

using System;
using System.Net;
using System.Text.RegularExpressions;

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
                return null;
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
            Regex namePattern = new Regex("content=([a-zA-Z0-9\\ \\\"\\-\\]]*)", RegexOptions.IgnoreCase);

            const string galleryTemplate = "https://nhentai.net/g/";
            string imageHTML, imageURL, imageExtension;

            // Getting user input and generating a gallery URL
            Console.WriteLine("Enter a doujin ID: ");
            string doujinID = Console.ReadLine();
            string doujinURL = galleryTemplate + doujinID + "/";

            // Checking if the user input is valid
            try
            {
                int.TryParse(doujinID, out int temp);
            }
            catch (Exception)
            {
                Console.WriteLine("Error: Invalid input!");
                goto EndPoint;
            }

            // Downloading the gallery HTML and finding the name and page count
            string galleryHTML = webClient.DownloadString(doujinURL);
            MatchCollection name = namePattern.Matches(galleryHTML);
            MatchCollection pageMatch = pagePattern.Matches(galleryHTML);

            // Outputing the gallery title and page count
            Console.WriteLine("Title: " + GetTitle(name[1].Value));
            Console.WriteLine(pageMatch[0].Value);

            // User confirmation before downloading the gallery
            Console.WriteLine("Continue? (Press Q to cancel.)");
            ConsoleKeyInfo input =Console.ReadKey(true);
            if (input.Key.ToString() == "Q")
                goto EndPoint;

            // Creating a directory with the gallery ID as the name
            System.IO.Directory.CreateDirectory("./" + doujinID + "/");

            // Finding and downloading all the images from the gallery
            for (int currentPage = 1; currentPage <= PageCount(pageMatch[0].Value); currentPage++)
            {
                imageHTML = webClient.DownloadString(doujinURL + (currentPage) + "/");
                MatchCollection imageMatch = imagePattern.Matches(imageHTML);
                imageURL = imageMatch[0].Value;
                imageExtension = GetExtension(imageURL);
                Console.WriteLine("Downloading: " + imageURL);
                try
                {
                    webClient.DownloadFile(imageURL, "./" + doujinID + "/" + currentPage + imageExtension);
                }
                catch (Exception)
                {
                    try
                    {
                        Console.WriteLine("Error: Couldn't download image. Retrying...");
                        webClient.DownloadFile(imageURL, "./" + doujinID + "/" + currentPage + imageExtension);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error: Couldn't download image. Please check your internet connection or if nhentai is down.");
                        break;
                    }
                }
            }

            EndPoint:
            Console.WriteLine("Done! Press any key to quit.");
            Console.ReadKey();
        }
    }
}
