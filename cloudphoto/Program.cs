using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using System.Collections;
using System.Collections.Generic;
using Amazon.S3.Transfer; 
using System.IO;
using System.Linq;
using System.Text;

namespace cloudphoto
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length > 6 || args.Length == 0)
            {
                Console.WriteLine("Wrong arguments count");
                return ;
            }
            
            AmazonS3Config configsS3 = new AmazonS3Config { 
                ServiceURL = "https://s3.yandexcloud.net"
            };
            AmazonS3Client client = new AmazonS3Client(configsS3);
            const string bucketName = "kfu-itis-spr22-cloudphoto";
            
            
            var command = CommandParser.ParseCommand(args[0]);
            if (command == null)
            {
                await Console.Error.WriteLineAsync("unknown command");
                return ;
            }

            if (TryExecuteCommand(client, bucketName, command, args))
                Console.WriteLine("Command executed successfully");
            else 
                Console.WriteLine("Command execution failed");
        }

        private static bool TryExecuteCommand(AmazonS3Client client, string bucketName, 
                CommandParser.CommandType? command, string[] args)
        {
            string albumName;
            string pathName;

            switch (args[1])
            {
                case "--album" when args[3] == "--path":
                    albumName = args[2];
                    pathName = args[4];
                    break;
                case "--path" when args[3] == "--album":
                    pathName = args[2];
                    albumName = args[4];
                    break;
                default:
                    Console.Error.WriteLine("Wrong options");
                    return false;
            }

            if (command == CommandParser.CommandType.Upload)
            {
                if (!CloudManager.UploadFiles(client, bucketName, pathName, albumName))
                    return false;
            }
            else if (command == CommandParser.CommandType.Download)
            {
                if (!CloudManager.DownloadFiles(client, bucketName, pathName, albumName))
                    return false;
            }

            foreach (var arg in args)
            {
                Console.WriteLine(arg);
            }
            return true;
        }
    }

    public static class CommandParser
    {
        public enum CommandType
        {
            Upload,
            Download,
            ListAlbums,
            ListPhotos,
            GenerateSite
        }

        public static CommandType? ParseCommand(string cmdString)
        {
            if (Enum.TryParse(cmdString, out CommandType command))
                return command;
            return null;
        }
    }
}