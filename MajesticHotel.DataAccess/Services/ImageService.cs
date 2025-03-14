using MajesticHotel.Models;
using MajesticHotel_API.Services.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace MajesticHotel_API.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ImageService(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task UploadImagesAsync(List<IFormFile> files, string folderName, int EntityId)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            if (files != null)
            {
                if(!Directory.Exists(Path.Combine(wwwRootPath, @"images\" + folderName + "\\" + EntityId)))
                {
                    System.IO.Directory.CreateDirectory(Path.Combine(wwwRootPath, @"images\" + folderName + "\\" + EntityId));
                }
                foreach (var file in files)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string EntityPath = Path.Combine(wwwRootPath, @"images\" + folderName + "\\" + EntityId);
                    using (var fileStream = new FileStream(Path.Combine(EntityPath, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
            }
        }
        public void DeleteImages(string folerName, int EntityId)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            if (Directory.Exists(Path.Combine(wwwRootPath, @"images\" + folerName + "\\" + EntityId.ToString())))
            {
                System.IO.Directory.Delete(Path.Combine(wwwRootPath, @"images\" + folerName + "\\" + EntityId.ToString()), true);
            }
        }

        public List<string> GetImageUrls(string folderName, int EntityId)
        {
            var httpContext = _httpContextAccessor.HttpContext; // Get HttpContext

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            var EntityPath = Path.Combine(wwwRootPath, @"images\" + folderName + "\\" + EntityId);
            List<string> ImageUrls = new List<string>();
            if (Directory.Exists(EntityPath))
            {
                var EntityFiles = Directory.GetFiles(EntityPath);
                foreach (var File in EntityFiles)
                {
                    var ImageUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/images/{folderName}/{EntityId}/{Path.GetFileName(File)}";

                    //var ImageUrl = Path.Combine(@"\images\" + folderName + "\\" + EntityId + "\\", Path.GetFileName(File));
                    ImageUrls.Add(ImageUrl);
                }
            }
            return ImageUrls;
        }
    }
}
