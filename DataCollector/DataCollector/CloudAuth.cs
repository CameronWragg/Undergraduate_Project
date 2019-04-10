using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Storage.v1;
using Google.Cloud.Storage.V1;

namespace DataCollector
{
    internal static class CloudAuth
    {
        private static string serviceAccountFile = "CMP3060M-db0fd67d6de4.json";
        public static object AuthImplicit(string projectId)
        {
            var credential = GoogleCredential.FromFile(serviceAccountFile);
            var storage = StorageClient.Create(credential);
            string bucketName = projectId + "-capturetest";
            try
            {
                storage.CreateBucket(projectId, bucketName);
            }
            catch (Google.GoogleApiException e)
            when (e.Error.Code == 409)
            {
                ErrorHandler.AddMessage(bucketName + " exists.");
            }

            var buckets = storage.ListBuckets(projectId);
            foreach (var bucket in buckets)
            {
                ErrorHandler.AddMessage(bucket.Name);
            }
            return null;
        }
    }
}
