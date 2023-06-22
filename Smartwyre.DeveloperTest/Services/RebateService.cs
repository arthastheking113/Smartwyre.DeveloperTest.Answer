using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services;

public class RebateService : IRebateService
{
    private readonly IRebateDataStore _rebateDataStore;
    private readonly IProductDataStore _productDataStore;

    public RebateService(IRebateDataStore rebateDataStore, 
        IProductDataStore productDataStore)
    {
        _rebateDataStore = rebateDataStore;
        _productDataStore = productDataStore;
    }
    public CalculateRebateResult Calculate(CalculateRebateRequest request)
    {
        var result = new CalculateRebateResult();

        if (request is null) return result;

        Rebate rebate = _rebateDataStore.GetRebate(request.RebateIdentifier);

        if(rebate is null) return result;

        Product product = _productDataStore.GetProduct(request.ProductIdentifier);

        if (product is null) return result;

        switch (rebate.Incentive)
        {
            case IncentiveType.FixedCashAmount:

                result = CalculateFixedCashAmount(product, rebate);

                break;

            case IncentiveType.FixedRateRebate:

                result = CalculateFixedRateRebate(product, rebate, request.Volume);

                break;

            case IncentiveType.AmountPerUom:

                result = CalculateAmountPerUom(product, rebate, request.Volume);

                break;
        }

        if (result.Success) _rebateDataStore.StoreCalculationResult(rebate, result.RebateAmount);

        return result;
    }

    private CalculateRebateResult CalculateFixedCashAmount(Product product, Rebate rebate)
    {
        var result = new CalculateRebateResult();

        result.Success = product.IsValidIncentive(SupportedIncentiveType.FixedCashAmount) && rebate.Amount != 0;

        if (result.Success) result.RebateAmount = rebate.Amount;

        return result;
    }
    private CalculateRebateResult CalculateFixedRateRebate(Product product, Rebate rebate, decimal volume)
    {
        var result = new CalculateRebateResult();

        result.Success = product.IsValidIncentive(SupportedIncentiveType.FixedRateRebate) && rebate.Percentage != 0 && product.Price != 0 && volume != 0;

        if (result.Success) result.RebateAmount = product.Price * rebate.Percentage * volume;

        return result;
    }
    private CalculateRebateResult CalculateAmountPerUom(Product product, Rebate rebate, decimal volume)
    {
        var result = new CalculateRebateResult();

        result.Success = product.IsValidIncentive(SupportedIncentiveType.AmountPerUom) && rebate.Amount != 0 && volume != 0;

        if (result.Success) result.RebateAmount = rebate.Amount * volume;

        return result;
    }
}
