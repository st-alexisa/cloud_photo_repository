using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer; 
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Amazon.S3.Model;

namespace cloudphoto
{
    public static class CloudManager
    {

        public static async Task<bool> PrintPhotosList(AmazonS3Client client, string bucketName, string albumName)
        {
            var response = await client.ListObjectsAsync(bucketName, albumName);
            if (response.S3Objects.Select(x => x.Key[..x.Key.IndexOf('/')]).Distinct().All(x => x != albumName))
            {
                Console.WriteLine("no specified album");
                return true;
            }
            Console.WriteLine("Photos in " + albumName + ":");
            foreach (var fileKey in response.S3Objects
                .Select(x => x.Key[(x.Key.IndexOf('/') + 1)..])
                .OrderBy(x => x))
            {
                Console.WriteLine(fileKey);
            }
            return true;
        }
        
        public static async Task<bool> PrintAlbumsList(AmazonS3Client client, string bucketName)
        {
            var response = await client.ListObjectsAsync(bucketName);
            Console.WriteLine("Albums:");
            foreach (var fileKey in response.S3Objects
                .Select(x => x.Key[..x.Key.IndexOf('/')])
                .Distinct()
                .OrderBy(x => x))
            {
                Console.WriteLine(fileKey);
            }
            return true;
        }
        
        public static bool UploadFiles(AmazonS3Client client, string bucketName, string localDirectoryPath, string cloudAlbumName)
        {
            var utility = new TransferUtility(client);
            
            var files = Directory
                .EnumerateFiles(localDirectoryPath, "*.jpg")
                .Select(ConvertToUnixTypePath);
            foreach (var filePath in files)
            {
                var fileName = filePath.Split('/').Last();
                utility.Upload(filePath, bucketName, cloudAlbumName + '/' + fileName);
            }
            return true;
        }
        
        public static bool DownloadFiles(AmazonS3Client client, string bucketName, string localDirectoryPath,
            string cloudAlbumName)
        {
            var utility = new TransferUtility(client);  
            
            var response = client.ListObjectsAsync(bucketName, cloudAlbumName);
            foreach (var obj in response.Result.S3Objects)
            {
                var filePath = localDirectoryPath + '/' + obj.Key.Split('/').Last();
                utility.Download(filePath, bucketName, obj.Key);
            }
            return true;
        }

        private static string ConvertToUnixTypePath(string path)
        {
            var builder = new StringBuilder();
            foreach (var c in path)
                builder.Append(c != '\\' ? c : '/');
            return builder.ToString();
        }
    }
}