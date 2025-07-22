using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using POS_ModernUI.Helpers;
using POS_ModernUI.Models;
using POS_ModernUI.Services.Contracts;
using POS_ModernUI.Services.ImageServices;
using POS_ModernUI.Services.ImageServices.Interface;
using POS_ModernUI.ViewModels.Windows;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace POS_ModernUI.Views.Windows
{
    /// <summary>
    /// Interaction logic for AddEditProductWindow.xaml
    /// </summary>
    public partial class AddEditProductWindow : IProductNavigationWindow
    {
        Product _product;
        IImageRemoverService _imageRemover;
        ImageCompressor _imageCompressor;
        public AddEditProductViewModel ViewModel { get; }
        public AddEditProductWindow(AddEditProductViewModel vm,
                                    IImageRemoverService imageRemover,
                                    ImageCompressor imageCompressor)
        {
            ViewModel = vm;
            DataContext = this;
            _imageRemover = imageRemover;
            _imageCompressor = imageCompressor;

            SystemThemeWatcher.Watch(this);

            InitializeComponent();
        }



        public void ShowWindow() => Show();
        public void ShowWindow(Product product)
        {
            _product = product;
            ViewModel.SelectedUnit = product.UnitName;
            ViewModel.ProductUnitPrice = product.UnitPrice;
            ViewModel.ProductQuantity = product.QuantityInStock;
            ViewModel.ProductBarCode = product.Barcode;
            ViewModel.ProductName = product.Name;
            ViewModel.ImageUrl = product.Image;
            txtTitle.Text = "تعديل المنتج";
            Show();
        }

        public void CloseWindow() => Close();
        
        public INavigationView GetNavigation()
        {
            throw new NotImplementedException();
        }

        public bool Navigate(Type pageType)
        {
            throw new NotImplementedException();
        }

        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider)
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

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

            if(int.TryParse(txtQuantity.Text, out int quantity))
                if (quantity <= 0)
                    return;
            if (decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice))
                if (unitPrice <= 0)
                    return;

            string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var imagePath = path + @"\Images";

            await CreateNewImage(imagePath);

            if (_product == null)
            {

                bool result = ViewModel.SaveNewProduct();
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

                ViewModel.UpdateCurrentProduct(_product);
                this.Close();
            }
        }

    }
}
