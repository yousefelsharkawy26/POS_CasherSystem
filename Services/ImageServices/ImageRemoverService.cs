using System.IO;
using POS_ModernUI.Database.Repository.IRepository;
using POS_ModernUI.Services.ImageServices.Interface;

namespace POS_ModernUI.Services.ImageServices;
public class ImageRemoverService : IImageRemoverService
{
    private readonly IUnitOfWork _unitOfWork;
    public ImageRemoverService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public void AddPath(string path)
    {
        _unitOfWork.ImagePaths.Add(new() {  Path = path });
        _unitOfWork.Save();
    }

    public void LazyRemoveImagesFromPaths()
    {
        var lst = _unitOfWork.ImagePaths.GetAll().ToList();
        foreach (var path in lst)
        {
            try
            {
                File.Delete(path.Path);

                _unitOfWork.ImagePaths.Delete(path);
                _unitOfWork.Save();
            }
            catch
            {
            }
        }
    }
}
