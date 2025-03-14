﻿using Microsoft.AspNetCore.Http;

namespace MajesticHotel_API.Services.IServices
{
    public interface IImageService
    {
        Task UploadImagesAsync(List<IFormFile> files, string folderName, int EntityId);
        List<string> GetImageUrls(string folderName, int EntityId);
        void DeleteImages(string folerName, int EntityId);
    }
}
