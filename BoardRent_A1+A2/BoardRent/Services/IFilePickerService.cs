using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading.Tasks;

namespace BoardRent.Services
{
    public interface IFilePickerService
    {
        Task<string> PickImageFileAsync();
    }
}
