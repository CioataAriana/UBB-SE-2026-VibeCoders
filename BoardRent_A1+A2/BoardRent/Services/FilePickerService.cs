using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace BoardRent.Services
{
    public class FilePickerService : IFilePickerService
    {
        public async Task<string> PickImageFileAsync()
        {
            var picker = new FileOpenPicker();
            // App._window must be accessible. If it's internal/private, make it public static in App.xaml.cs
            var windowHandle = WindowNative.GetWindowHandle(App._window);
            InitializeWithWindow.Initialize(picker, windowHandle);

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();
            return file?.Path;
        }
    }
}