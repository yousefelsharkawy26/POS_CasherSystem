using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using POS_ModernUI.DataAccess.UnitOfWork;
using POS_ModernUI.Models;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.Services.ImageServices;
using POS_ModernUI.Services.ImageServices.Interface;
using POS_ModernUI.ViewModels.Windows;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace POS_ModernUI.Views.Windows;

/// <summary>
/// Interaction logic for AddEditProductWindow.xaml
/// </summary>
public partial class AddEditProductWindow : IProductNavigationWindow
{
    #region Fields
    Product? _product;
    IImageRemoverService _imageRemover;
    ImageCompressor _imageCompressor;
    private readonly IUnitOfWork _unitOfWork;
    #endregion

    #region Props
    public AddEditProductViewModel ViewModel { get; }
    #endregion

    #region Constructors
    public AddEditProductWindow(AddEditProductViewModel vm,
                                IImageRemoverService imageRemover,
                                ImageCompressor imageCompressor,
                                IUnitOfWork unitOfWork)
    {
        ViewModel = vm;
        DataContext = this;
        _imageRemover = imageRemover;
        _imageCompressor = imageCompressor;
        _unitOfWork = unitOfWork;

        SystemThemeWatcher.Watch(this);

        InitializeComponent();
    }
    #endregion

    #region Navigation Props
    public void ShowWindow() => Show();
    public async void ShowWindow(Product product)
    {
        _product = product;

        await ViewModel.LoadProduct(product.ProductId);

        txtTitle.Text = "تعديل المنتج";
        Show();
    }
    public void CloseWindow() => Close();
    public INavigationView GetNavigation() => throw new NotImplementedException();

    public bool Navigate(Type pageType) => throw new NotImplementedException();

    public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => throw new NotImplementedException();

    public void SetServiceProvider(IServiceProvider serviceProvider) => throw new NotImplementedException();
    #endregion

    #region WindowActions
    OpenFileDialog? fd;
    private void UploadImageButton_Click(object sender, RoutedEventArgs e)
    {
        fd = new()
        {
            Title = "Select image",
            InitialDirectory = "",
            Filter = "Image Files (*.gif,*.jpg,*.jpeg,*.bmp,*.png)|*.gif;*.jpg;*.jpeg;*.bmp;*.png"
        };

        if (fd.ShowDialog() == false)
        {
            fd = null;
            return;
        }
        imgProduct.Source = new BitmapImage(new Uri(fd.FileName));
    }
    private async Task CreateNewImage(string destPath)
    {

        if (fd != null)
        {
            if (!Directory.Exists(destPath))
            {
                _ = Directory.CreateDirectory(destPath);
            }

            var newName = Guid.NewGuid().ToString();

            await _imageCompressor.SaveCompressedAsync(fd.FileName, destPath, newName);

            ViewModel.ImageUrl = @"\Images\" + newName + ".png";
        }

    }
    private async void SaveProductButton_Click(object sender, RoutedEventArgs e)
    {
        if (txtProductName.Text.Trim().IsNullOrEmpty())
            return;

        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var imagePath = path + @"\Images";

        await CreateNewImage(imagePath);

        if (_product == null)
        {

            bool result = await ViewModel.SaveNewProduct();
            if (!result)
            {
                this.Close();
                return;
            }
            imgProduct.Source = new BitmapImage(new Uri(imagePath + "\\defaultproduct.png"));
        }
        else
        {
            // Subscripe to lazy after remove it remove the old image
            if (File.Exists(path + _product.Image) && !_product.Image.Equals("\\Images\\DefaultProduct.png", StringComparison.OrdinalIgnoreCase))
                _imageRemover.AddPath(path + _product.Image);

            await ViewModel.UpdateCurrentProduct(_product);
            this.Close();
        }
    }
    private async void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        var txt = sender as Wpf.Ui.Controls.TextBox;

        if (e.Key == System.Windows.Input.Key.Enter)
        {
            // add logic Barcode reader here
            await ViewModel.SearchProductByBarcodeAsync(txt?.Text!);
        }
    }
    #endregion
}
