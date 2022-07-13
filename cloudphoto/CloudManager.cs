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

        public static async Task<bool> PrintAlbumsList(AmazonS3Client client, string bucketName)
        {
            var utility = new TransferUtility(client);  
            
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
            var files = Directory
                .EnumerateFiles(localDirectoryPath, "*.jpg")
                .Select(ConvertToUnixTypePath);
            foreach (var filePath in files)
            {
                var fileName = filePath.Split('/').Last();
                if (!UploadFileToCloud(client, filePath, bucketName, cloudAlbumName + '/' + fileName))
                    return false;
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

        private static bool UploadFileToCloud(AmazonS3Client client, string localFilePath, string bucketName, string fileNameInCloud)  
        {
            var utility = new TransferUtility(client);  
            var request = new TransferUtilityUploadRequest();  

            request.BucketName = bucketName;
            request.Key = fileNameInCloud; //file name up in S3  
            request.FilePath = localFilePath;
            utility.Upload(request); //commensing the transfer 
            return true; //indicate that the file was sent  
        }
    }
}