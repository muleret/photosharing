using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using System.Drawing;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        CloudStorageAccount storageAccount;

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");

            var blobClient = storageAccount.CreateCloudBlobClient();
            var queueClient = storageAccount.CreateCloudQueueClient();
            var tqueue = queueClient.GetQueueReference("thumbnailqueue");
            CloudQueueMessage msg = null;

            try
            {
                //this.RunAsync(this.cancellationTokenSource.Token).Wait();
                while (true)
                {
                    try
                    {
                        msg = tqueue.GetMessage();

                        if (msg != null)
                        {
                            ProcessQueueMessage(msg, tqueue, blobClient);
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(500);
                        }
                    }
                    catch (StorageException e)
                    {
                        if (msg != null && msg.DequeueCount > 5)
                        {
                            tqueue.DeleteMessage(msg);
                        }

                        System.Threading.Thread.Sleep(5000);
                    }
                }
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        private void ProcessQueueMessage(CloudQueueMessage msg, CloudQueue tqueue, CloudBlobClient blobClient)
        {
            var container = blobClient.GetContainerReference("images1");
            var imageBlob = container.GetBlobReference(Path.GetFileName(msg.AsString));

            string fileName = Path.GetFileName(msg.AsString); // imageName.jpg
            string fileNameFull = Path.GetTempFileName(); // C:\temp\xyz.tmp
            File.Move(fileNameFull, fileNameFull + Path.GetExtension(msg.AsString)); // C:\temp\xyz.tmp.jpg
            fileNameFull = fileNameFull + Path.GetExtension(msg.AsString); // C:\temp\xyz.tmp.jpg

            // // C:\temp\imageName.thumb.jpg
            var fileNameThumbFull = Path.Combine(Path.GetDirectoryName(fileNameFull),
                Path.GetFileNameWithoutExtension(fileName) + ".thumb" + Path.GetExtension(fileName));
            var fileNameThumb = Path.GetFileName(fileNameThumbFull); // imageName.thumb.jpg

            CloudBlockBlob blockBlobThumb = container.GetBlockBlobReference(fileNameThumb);

            imageBlob.DownloadToFile(fileNameFull, FileMode.Truncate);

            Image image = Image.FromFile(fileNameFull);
            double ratio = image.Width / (image.Height * 1.0);
            int theight = 100;
            Image thumb = image.GetThumbnailImage((int)(ratio * theight), theight, () => false, IntPtr.Zero);
            thumb.Save(fileNameThumbFull);

            blockBlobThumb.UploadFromFile(fileNameThumbFull);

            tqueue.DeleteMessage(msg);
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
