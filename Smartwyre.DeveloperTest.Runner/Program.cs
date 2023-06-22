using Moq;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;
using System;

namespace Smartwyre.DeveloperTest.Runner;

class Program
{
    static void Main(string[] args)
    {
        //Set up some data to run
        //Feel free to change the variables
        string RebateID = Guid.NewGuid().ToString();
        string ProductID = Guid.NewGuid().ToString();

        Rebate rebate = new Rebate
        {
            Amount = 10,
            Identifier = RebateID,
            Incentive = IncentiveType.FixedRateRebate,
            Percentage = 10
        };
        Product product = new Product
        {
            Identifier = ProductID,
            SupportedIncentives = SupportedIncentiveType.FixedRateRebate,
            Price = 10
        };
        CalculateRebateRequest request = new CalculateRebateRequest
        {
            RebateIdentifier = RebateID,
            ProductIdentifier = ProductID,
            Volume = 10
        };
        var _mockRebateDataStore = new Mock<IRebateDataStore>();
        _mockRebateDataStore.Setup(obj => obj.GetRebate(RebateID)).Returns(rebate);
        var _mockProductDataStore = new Mock<IProductDataStore>();
        _mockProductDataStore.Setup(obj => obj.GetProduct(ProductID)).Returns(product);

        //run
        var rebateService = new RebateService(_mockRebateDataStore.Object, _mockProductDataStore.Object);
        var result = rebateService.Calculate(request);

        Console.WriteLine($"Result: {result.Success}");
        Console.WriteLine($"Amount: {result.RebateAmount}");
    }
}
