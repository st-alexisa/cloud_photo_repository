using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer; 
using System.IO;
using System.Linq;
using System.Text;

namespace cloudphoto
{
    public static class CloudManager
    {
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