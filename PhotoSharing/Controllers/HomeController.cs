using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotoSharing.Controllers
{
    public class HomeController : Controller
    {
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                   CloudConfigurationManager.GetSetting("StorageConnectionString"));

        public ActionResult Index()
        {
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("images1");

            List<string> blobs = new List<string>();

            foreach(var blob in container.ListBlobs())
            {
                blobs.Add(Path.GetFileName(blob.Uri.AbsolutePath));
            }

            ViewBag.Blobs = blobs.ToArray();
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file1)
        {
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);

                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var container = blobClient.GetContainerReference("images1");

                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

                    // Create or overwrite the "myblob" blob with contents from a local file.
                    blockBlob.UploadFromStream(file.InputStream);
                }
            }

            return RedirectToAction("Index");
        }
    }
}