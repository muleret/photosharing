using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoSharing.Models
{
    public class BlobViewModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}