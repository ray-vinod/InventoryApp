using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.RefreshServices;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Components
{
    public partial class UploadFile
    {

        IList<IBrowserFile> selectedFile;
        private List<FileInfo> fileInfos;

        [Inject]
        private AlertService AlertService { get; set; }

        [Inject]
        private IWebHostEnvironment Env { get; set; }

        [Inject]
        public IndexRefreshService IndexRefreshService { get; set; }

        private record FileInfo(string Name, long Size, string Url);


        protected override void OnInitialized()
        {
            fileInfos = new List<FileInfo>();
        }

        private async Task OnFileChange(InputFileChangeEventArgs e)
        {
            if (e.FileCount > 0)
            {
                if (e.FileCount > 10)
                {
                    e = null;
                    AlertService.AddMessage(new Alert("You can choose maximum numbers of file is 10.",
                        AlertType.Error));
                    return;
                }

                var files = e.GetMultipleFiles();
                selectedFile = files.ToList();
                var format = "image/png";
                foreach (var file in files)
                {
                    var resizedImage = await file.RequestImageFileAsync(format, 100, 100);
                    var buffer = new byte[resizedImage.Size];
                    await resizedImage.OpenReadStream().ReadAsync(buffer);
                    var data = $"data:{format};base64,{Convert.ToBase64String(buffer)}";
                    fileInfos.Add(new FileInfo(file.Name, file.Size, data));
                }
            }
        }

        private async Task RemoveImage(FileInfo img)
        {
            var result = fileInfos.Remove(img);
            if (result)
            {
                var f = selectedFile.Where(x => x.Name == img.Name).FirstOrDefault();
                selectedFile.Remove(f);
                await InvokeAsync(() => StateHasChanged());
            }
        }

        private async Task Upload()
        {
            var format = "image/png";
            foreach (var file in selectedFile)
            {
                var resizedImageFile = await file.RequestImageFileAsync(format, 500, 400);
                var buffer = new byte[resizedImageFile.Size];
                Stream stream = resizedImageFile.OpenReadStream();
                var fileName = file.Name;

                //Rename image file and upload
                var path = $"{Env.WebRootPath}\\Images\\CarouselImages\\{file.Name}";
                FileStream fs = File.Create(path);
                await stream.CopyToAsync(fs);
                fs.Close();
                fileInfos.Clear();

                IndexRefreshService.CallPageRefresh();

                await InvokeAsync(() => StateHasChanged());
                AlertService.AddMessage(new Alert("File Uploaded Successfully!", AlertType.Success));
            }

            IndexRefreshService.CallPageRefresh();

        }
    }
}