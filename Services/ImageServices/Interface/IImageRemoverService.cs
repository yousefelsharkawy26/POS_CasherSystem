namespace POS_ModernUI.Services.ImageServices.Interface;
public interface IImageRemoverService
{
    Task AddPath(string path);
    Task LazyRemoveImagesFromPaths();
}
