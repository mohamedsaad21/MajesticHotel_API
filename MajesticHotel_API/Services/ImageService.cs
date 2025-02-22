using MajesticHotel.Models;
using MajesticHotel_API.Services.IServices;
using Microsoft.AspNetCore.Hosting;

namespace MajesticHotel_API.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
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
                    string hotelPath = Path.Combine(wwwRootPath, @"images\" + folderName + "\\" + EntityId);
                    using (var fileStream = new FileStream(Path.Combine(hotelPath, fileName), FileMode.Create))
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

    }
}
