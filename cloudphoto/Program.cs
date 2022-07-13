using System;
using System.Threading.Tasks;
using Amazon.S3;

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

            if (await TryExecuteCommand(client, bucketName, command, args))
                Console.WriteLine("Command executed successfully");
            else 
                Console.WriteLine("Command execution failed");
        }

        private static async Task<bool> TryExecuteCommand(AmazonS3Client client, string bucketName, 
                CommandParser.CommandType? command, string[] args)
        {
            if (!CommandParser.TryParseArguments(args, out var albumName, out var pathName))
                return false;

            if (command == CommandParser.CommandType.Upload && args.Length == 5
                            && pathName != null && albumName != null)
                return CloudManager.UploadFiles(client, bucketName, pathName, albumName);
            if (command == CommandParser.CommandType.Download && args.Length == 5
                            && pathName != null && albumName != null)
                return CloudManager.DownloadFiles(client, bucketName, pathName, albumName);
            if (command == CommandParser.CommandType.ListAlbums && args.Length == 1)
                return await CloudManager.PrintAlbumsList(client, bucketName);
            if (command == CommandParser.CommandType.ListPhotos && args.Length == 3 && albumName != null) 
                return await CloudManager.PrintPhotosList(client, bucketName, albumName);
            Console.WriteLine("Wrong arguments");
            return false;
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

        public static bool TryParseArguments(string[] args, out string albumName, out string pathName)
        {
            albumName = null;
            pathName = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--path" && i + 1 < args.Length)
                    pathName = args[++i];
                else if (args[i] == "--album" && i + 1 < args.Length)
                    albumName = args[++i];
            }
            return true;
        }

        public static CommandType? ParseCommand(string cmdString)
        {
            if (Enum.TryParse(cmdString, out CommandType command))
                return command;
            return null;
        }
    }
}