using POS_ModernUI.Models;
using POS_ModernUI.DataAccess.UnitOfWork;
using System.Threading.Tasks;

namespace POS_ModernUI.Services;
public class ProductServices
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductServices(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // 1. إضافة أو تعديل منتج مع وحداته
    public async Task SaveOrUpdateProductAsync(Product product, List<ProductUnit> units)
    {
        // حفظ أو تحديث بيانات المنتج
        if (product.ProductId > 0)
        {
            await _unitOfWork.Products.UpdateAsync(product);
        }
        else
        {
            await _unitOfWork.Products.AddAsync(product);
        }

        // حذف الوحدات القديمة (لو تعديل)
        var oldUnits = (await _unitOfWork.ProductUnits.GetAllAsync(pu => pu.ProductId == product.ProductId)).ToList();
        foreach (var old in oldUnits)
        {
            await _unitOfWork.ProductUnits.DeleteAsync(old);
        }

        // إضافة الوحدات الجديدة
        foreach (var unit in units)
        {
            unit.ProductId = product.ProductId;
            await _unitOfWork.ProductUnits.AddAsync(unit);
        }

        await _unitOfWork.SaveAsync();
    }

    // 2. حساب معامل التحويل من أي وحدة لأصغر وحدة
    public async Task<int> GetConversionFactorToSmallestUnit(int productId, int unitId, Dictionary<int, ProductUnit>? unitsDict = null)
    {
        int factor = 1;
        int? currentUnitId = unitId;

        // تحميل الوحدات مرة واحدة لو لم ترسل
        if (unitsDict == null)
        {
            unitsDict = (await _unitOfWork.ProductUnits
                .GetAllAsync(pu => pu.ProductId == productId))
                .ToDictionary(u => u.UnitId);
        }

        while (true)
        {
            if (!unitsDict.TryGetValue(currentUnitId.Value, out var unit) || unit.ParentProductUnitId == null)
                break;
            factor *= unit.QuantityPerParent ?? 1;
            currentUnitId = unit.ParentProductUnitId;
        }
        return factor;
    }

    // 3. تحويل كميات متعددة إلى أصغر وحدة
    public async Task<int> ConvertToSmallestUnit(int productId, List<(int UnitId, int Quantity)> quantities)
    {
        var unitsDict = (await _unitOfWork.ProductUnits
            .GetAllAsync(pu => pu.ProductId == productId))
            .ToDictionary(u => u.UnitId);

        int total = 0;
        foreach (var q in quantities)
        {
            int factor = await GetConversionFactorToSmallestUnit(productId, q.UnitId, unitsDict);
            total += q.Quantity * factor;
        }
        return total;
    }

    // 4. خصم من المخزون عند البيع
    public async Task<bool> SellProductAsync(int productId, int unitId, int quantitySold)
    {
        int factor = await GetConversionFactorToSmallestUnit(productId, unitId);
        int piecesToDeduct = quantitySold * factor;

        var product = await _unitOfWork.Products.GetAsync(p => p.ProductId == productId);
        if (product == null || product.QuantityInStock < piecesToDeduct)
            return false;

        product.QuantityInStock -= piecesToDeduct;
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveAsync();
        
        return true;
    }

    // دالة الشراء: تضيف أو تحدث المنتج وتعدل الكمية
    public async Task PurchaseProductAsync(Product product, int purchasedQuantity, int purchasedUnitId, List<ProductUnit> units)
    {
        // تحقق هل المنتج موجود
        var dbProduct = await _unitOfWork.Products.GetAsync(p => p.Name == product.Name);

        if (dbProduct == null)
        {
            // منتج جديد: أضف المنتج مع وحداته
            product.UnitShares = units;
            // احسب الكمية بالوحدة الأساسية (أصغر وحدة)
            int baseQuantity = ConvertToSmallestUnit(units, purchasedUnitId, purchasedQuantity);
            product.QuantityInStock = baseQuantity;
            await _unitOfWork.Products.AddAsync(product);
        }
        else
        {
            // منتج موجود: أضف الكمية الجديدة بعد التحويل
            var productUnits = (await _unitOfWork.ProductUnits.GetAllAsync(pu => pu.ProductId == dbProduct.ProductId)).ToList();
            int baseQuantity = ConvertToSmallestUnit(productUnits, purchasedUnitId, purchasedQuantity);
            dbProduct.QuantityInStock += baseQuantity;
            await _unitOfWork.Products.UpdateAsync(dbProduct);
        }
        
        await _unitOfWork.SaveAsync();
    }

    // تحويل أي كمية من وحدة إلى أصغر وحدة للمنتج
    public int ConvertToSmallestUnit(IEnumerable<ProductUnit> units, int fromUnitId, int quantity)
    {
        // بناء قاموس للوحدات حسب UnitId
        var unitsDict = units.ToDictionary(u => u.UnitId);

        int factor = 1;
        int? currentUnitId = fromUnitId;
        while (true)
        {
            if (!unitsDict.TryGetValue(currentUnitId.Value, out var unit) || unit.ParentProductUnitId == null)
                break;
            factor *= unit.QuantityPerParent ?? 1;
            currentUnitId = unit.ParentProductUnitId;
        }
        return quantity * factor;
    }
}
