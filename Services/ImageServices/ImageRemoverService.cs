using System.IO;
using POS_ModernUI.DataAccess.UnitOfWork;
using POS_ModernUI.Services.ImageServices.Interface;

namespace POS_ModernUI.Services.ImageServices;
public class ImageRemoverService : IImageRemoverService
{
    private readonly IUnitOfWork _unitOfWork;
    public ImageRemoverService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task AddPath(string path)
    {
        await _unitOfWork.ImagePaths.AddAsync(new() {  Path = path });
        await _unitOfWork.SaveAsync();
    }

    public async Task LazyRemoveImagesFromPaths()
    {
        var lst = (await _unitOfWork.ImagePaths.GetAllAsync()).ToList();
        foreach (var path in lst)
        {
            try
            {
                File.Delete(path.Path);

                await _unitOfWork.ImagePaths.DeleteAsync(path);
                await _unitOfWork.SaveAsync();
            }
            catch
            {
            }
        }
    }
}
