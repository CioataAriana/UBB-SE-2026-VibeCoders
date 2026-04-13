namespace BoardRent.Services
{
    using System;
    using System.Threading.Tasks;
    using Windows.Storage.Pickers;
    using WinRT.Interop;

    public class FilePickerService : IFilePickerService
    {
        public async Task<string> PickImageFileAsync()
        {
            // Verificăm dacă suntem în context de aplicație sau de test
            if (App.Window == null)
            {
                return null;
            }

            var picker = new FileOpenPicker();
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, windowHandle);

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();
            return file?.Path;
        }
    }
}