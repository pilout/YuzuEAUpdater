﻿using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Cryptography;

public class Program
{
    static void Main(string[] args)
    {
        string latestReleaseUrl = "https://github.com/pilout/YuzuUpdater/releases/latest";
        Uri latestReleaseUri = new Uri(latestReleaseUrl);
        DateTime latestReleaseDate ;
        bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        string updaterFilePath = isLinux ? "YuzuEAUpdater" : "YuzuEAUpdater.exe");

        foreach(Process process in Process.GetProcessesByName(updaterFilePath.Replace(".exe","")))
        {
           process.Kill();
        }


        using (var client = new WebClient())
        {
            client.Headers.Add("User-Agent", "request");
            Stream stream = client.OpenRead(latestReleaseUri);
            StreamReader reader = new StreamReader(stream);
            string htmlContent = reader.ReadToEnd();

            string hashsStr = Regex.Match(htmlContent, @"\/pilout\/YuzuUpdater\/releases\/tag\/(.*)""").Groups[1].Value;
            hashsStr = hashsStr.Substring(0, hashsStr.IndexOf("\""));
            String[] hashs = hashsStr.Split('|');

            var needDownload = !File.Exists(updaterFilePath) || SHA256CheckSum(updaterFilePath) != (isLinux ? hashs[1] : hashs[0]);


            if (needDownload)
            {

                FileInfo fileInfo = new FileInfo(updaterFilePath);
                DateTime updaterCreatedDate = File.GetCreationTime(updaterFilePath);


                    Console.WriteLine("Update found, downloading...");
                    // /pilout/YuzuUpdater/releases/tag/1.9" />
                    
                    

                    string releasePackageUrl = "https://github.com/pilout/YuzuUpdater/releases/download/" + hashsStr + "/" + (isLinux ? "linux" : "windows" ) + "-x64.zip";
                    string zipFilePath = Path.Combine(Directory.GetCurrentDirectory(),"update00.zip");
                    client.Headers.Add("User-Agent", "request");
                    client.DownloadFile(releasePackageUrl, zipFilePath);
                    string extractPath = Path.Combine(Directory.GetCurrentDirectory());
                    FileStream streamzip = File.OpenRead(zipFilePath);
                    var zipFile = new ZipArchive(streamzip);

                    for (int i = 0; i < zipFile.Entries.Count; i++)
                    {
                        try
                        {
                            ZipArchiveEntry entry = zipFile.Entries[i];
                            string path = Path.Combine(extractPath, entry.FullName);
                            var ind = entry.FullName.LastIndexOf("/");
                            if (ind > 0)
                            {
                                string pathDirEntry = entry.FullName.Substring(0, ind);
                                Directory.CreateDirectory(Path.Combine(extractPath, pathDirEntry));
                                if(entry.FullName.EndsWith("/"))
                                    continue;
                            }


                            if (path == System.Reflection.Assembly.GetExecutingAssembly().Location)
                                continue;

                            entry.ExtractToFile(path, true);
                            Console.WriteLine("Extracting " + entry.FullName);
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.ReadLine();
                        }

                    }

                    streamzip.Close();
                    File.Delete(zipFilePath);
                    Console.WriteLine("Update done, restarting...");
                 

            }
            else
            {
                Console.WriteLine("No update found, restarting...");
            }

                if(isLinux)
                   Process.Start("chmod", "+x " + updaterFilePath);

        }

        var startInfo = new ProcessStartInfo(updaterFilePath);
        startInfo.UseShellExecute = true;
        Process.Start(startInfo);
        System.Threading.Thread.Sleep(5000);
        Environment.Exit(0);

    }


    public static string SHA256CheckSum(string filePath)
    {
        using (SHA256 SHA256 = SHA256Managed.Create())
        {
            using (FileStream fileStream = File.OpenRead(filePath))
                return Convert.ToBase64String(SHA256.ComputeHash(fileStream));
        }
    }
}