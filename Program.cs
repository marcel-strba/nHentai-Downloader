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
            if (ext.EndsWith(".jpg"))
                return ".jpg";
            if (ext.EndsWith(".png"))
                return ".png";
            if (ext.EndsWith(".gif"))
                return ".gif";
            if (ext.EndsWith(".webp"))
                return ".webp";
            else
                return null;
        }
        // A method that returns the page number based on the page count string
        static int PageCount(string pageMatch)
        {
            string pageString = Regex.Replace(pageMatch, "[^0-9]", "");
            return Convert.ToInt32(pageString);
        }
        // A method that returns the gallery title based on the title string
        static string GetTitle(string nameMatch)
        {
            return nameMatch.Substring(9, nameMatch.Length - 11);
        }
        // A method that downloads all images in the gallery into the output folder and handles
        // failed image downloads.
        static bool DownloadGallery(string imageURL, string outputPath, WebClient webClient)
        {
            try
            {
                webClient.DownloadFile(imageURL, outputPath);
                return true;
            }
            catch(Exception)
            {
                Console.WriteLine("ERROR: Could not download image! What would you like to do?\n" +
                    "Enter - Ignore image\n" +
                    "R - Retry\n" +
                    "Q - Quit");
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch(key.Key.ToString())
                {
                    case "R":
                        DownloadGallery(imageURL, outputPath, webClient);
                        return true;
                    case "Q":
                        return false;
                    default:
                        return true;
                }
            }
        }
        static void Main(string[] args)
        {
            string executablePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            WebClient webClient = new WebClient();
            Regex pagePattern = new Regex("class=\"name\">[\\0-9]*<");
            Regex imagePattern = new Regex("https://i.nhentai.net/galleries/([a-zA-Z0-9\\/\\.]*)?", RegexOptions.IgnoreCase);
            Regex namePattern = new Regex("content=([a-zA-Z0-9\\ \\\"\\-\\]]*)", RegexOptions.IgnoreCase);

            const string galleryTemplate = "https://nhentai.net/g/";
            string imageURLBase, imageURL, extension, doujinID, doujinURL;

            // Getting user input and generating a gallery URL
            if (args.Length == 0)
            {
                Console.WriteLine("Enter doujin ID: ");
                doujinID = Console.ReadLine();
                doujinURL = galleryTemplate + doujinID + "/";
            }
            else
            {
                Console.WriteLine("\n");
                doujinID = args[0];
                doujinURL = galleryTemplate + doujinID + "/";
            }

            // Checking if the user input is valid
            try
            {
                int.TryParse(doujinID, out int temp);
            }
            catch (Exception)
            {
                Console.WriteLine("ERROR: Invalid input!");
                goto EndPoint;
            }

            // Downloading the gallery HTML and finding the name and page count
            string galleryHTML = webClient.DownloadString(doujinURL);
            MatchCollection name = namePattern.Matches(galleryHTML);
            string title = GetTitle(name[1].Value);
            MatchCollection pageMatch = pagePattern.Matches(galleryHTML);

            // Outputing the gallery title and page count
            Console.WriteLine("Title: " + title);
            Console.WriteLine($"{PageCount(pageMatch[0].Value)} pages");

            // User confirmation before downloading the gallery
            Console.WriteLine("Continue? (Press Q to cancel.)");
            ConsoleKeyInfo input =Console.ReadKey(true);
            if (input.Key.ToString() == "Q")
                goto EndPoint;

            // Creating a directory with the doujin title and ID in brackets as name.
            System.IO.Directory.CreateDirectory($"{executablePath}/{title}({doujinID})/");

            // Getting the base of the image URL and image extension.
            MatchCollection imageMatch = imagePattern.Matches(webClient.DownloadString(doujinURL + (1) + "/"));
            imageURLBase = imageMatch[0].Value;
            extension = GetExtension(imageURLBase);

            // Finding and downloading all the images from the gallery
            for (int currentPage = 1; currentPage <= PageCount(pageMatch[0].Value); currentPage++)
            {
                imageURL = Regex.Replace(imageURLBase, "[0-9]\\.", $"{currentPage}.");
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"Progress: {currentPage}/{PageCount(pageMatch[0].Value)}");
                string outputPath = $"{executablePath}\\{title}({doujinID})\\{currentPage}{extension}";
                if (!DownloadGallery(imageURL, outputPath, webClient))
                    break;
            }
            
            EndPoint:
            Console.WriteLine("Done! Press any key to quit.");
            Console.ReadKey();
        }
    }
}
