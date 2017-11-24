using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using PhotoSharing.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
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

            List<BlobViewModel> blobs = new List<BlobViewModel>();

            foreach (var blob in container.ListBlobs())
            {
                string fileNameFull = blob.Uri.AbsoluteUri;

                if (fileNameFull.Contains(".thumb"))
                    continue;

                string fileName = Path.GetFileName(fileNameFull);

                var fileNameThumbFull = Path.Combine(Path.GetDirectoryName(fileNameFull),
                    Path.GetFileNameWithoutExtension(fileNameFull) + ".thumb" + Path.GetExtension(fileName));

                blobs.Add(new BlobViewModel
                {
                    Name = Path.GetFileName(blob.Uri.AbsolutePath),
                    Url = blob.Uri.AbsoluteUri,
                    ThumbnailUrl = fileNameThumbFull
                });
            }

            ViewBag.Blobs = blobs;
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
                    // x.jpg => x.thumb.jpg
                    var fileName = Path.GetFileName(file.FileName);
                    string fileNameFull = Path.GetTempFileName() + Path.GetExtension(fileName);
                    file.SaveAs(fileNameFull);
                    var fileNameThumbFull = Path.Combine(Path.GetDirectoryName(fileNameFull),
                        Path.GetFileNameWithoutExtension(fileName) + ".thumb" + Path.GetExtension(fileName));
                    var fileNameThumb = Path.GetFileName(fileNameThumbFull);

                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var container = blobClient.GetContainerReference("images1");

                    Image image = Image.FromFile(fileNameFull);
                    double ratio = image.Width / (image.Height * 1.0);
                    int theight = 100;
                    Image thumb = image.GetThumbnailImage((int)(ratio * theight), theight, () => false, IntPtr.Zero);
                    thumb.Save(fileNameThumbFull);

                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
                    CloudBlockBlob blockBlobThumb = container.GetBlockBlobReference(fileNameThumb);

                    // Create or overwrite the "myblob" blob with contents from a local file.
                    blockBlob.UploadFromStream(file.InputStream);
                    blockBlobThumb.UploadFromFile(fileNameThumbFull);
                }
            }

            return RedirectToAction("Index");
        }
    }
}