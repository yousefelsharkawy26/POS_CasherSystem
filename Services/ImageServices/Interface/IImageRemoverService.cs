namespace POS_ModernUI.Services.ImageServices.Interface;
public interface IImageRemoverService
{
    void AddPath(string path);
    void LazyRemoveImagesFromPaths();
}
