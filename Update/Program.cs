using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using SevenZipLib;

namespace Update
{
        class Program
        {
                static string downloadDir = null;
                /// <summary>
                /// 入口点
                /// </summary>
                /// <param name="args">arg0:下载地址,arg1:存放目录</param>
                static void Main(string[] args)
                {
                        DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                        downloadDir = di.Parent.FullName;
                        if (args.Length != 2)
                        {
                                Console.WriteLine("更新失败");
                        }
                        else
                        {
                                WebClient client = new WebClient();
                                client.DownloadProgressChanged += client_DownloadProgressChanged;
                                client.DownloadFileCompleted += client_DownloadFileCompleted;
                                Uri address = new Uri(args[0]);
                                downloadDir = args[1];
                                client.DownloadFileAsync(address, downloadDir + @"\EDS.rar");
                        }

                        Console.ReadLine();
                }

                static void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
                {
                        System.Threading.Thread.Sleep(2000);
                        Console.WriteLine("下载完成!还剩10秒");
                        deleteOldFiles();
                        Console.WriteLine("删除旧程序!");
                        extractNewFiles();
                        Console.WriteLine("更新完成");
                }

                private static void extractNewFiles()
                {
                        DirectoryInfo dir = new DirectoryInfo(downloadDir);
                        FileInfo fi = dir.GetFiles("*.rar")[0];
                        using (SevenZipArchive archive = new SevenZipArchive(fi.FullName))
                        {
                                archive.ExtractAll(downloadDir);
                        }
                        fi.Delete();
                }

                private static void deleteOldFiles()
                {
                        System.Threading.Thread.Sleep(10000);
                        DirectoryInfo di = new DirectoryInfo(downloadDir);
                        foreach (FileInfo file in di.GetFiles())
                        {
                                if (file.Extension != ".rar")
                                {
                                        file.Delete();
                                }
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                                if (dir.Name != "Update")
                                {
                                        dir.Delete(true);
                                }
                        }
                }

                static void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
                {
                        if (e.ProgressPercentage % 5 == 0)
                        {
                                Console.WriteLine(string.Format("下载：{0}%", e.ProgressPercentage));
                        }
                }
        }
}
