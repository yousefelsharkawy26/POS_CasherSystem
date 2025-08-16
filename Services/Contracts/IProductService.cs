using POS_ModernUI.Models;
using POS_ModernUI.Models.DTOs;
using POS_ModernUI.ViewModels.Windows;

namespace POS_ModernUI.Services.Contracts;

public interface IProductService
{
    // إضافة أو تحديث منتج جديد
    Task<Product> AddOrUpdateProductAsync(ProductInputModel input);

    // تحديث كمية المنتج عند الشراء
    Task UpdateProductQuantityOnPurchaseAsync(string productBarCode, int quantity, UnitTypes unitType);

    // جلب منتج بالباركود
    Task<Product> GetProductByBarcodeAsync(string barcode);

    // تحديث منتج موجود
    Task<bool> UpdateProductAsync(Product product);

    // دوال مساعدة للتحويل بين الوحدات
    int ConvertToBaseUnit(int quantity, UnitTypes unit);
}