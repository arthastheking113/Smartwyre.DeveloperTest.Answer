using Moq;
using NUnit.Framework;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;
using System;


namespace Smartwyre.DeveloperTest.Tests;

public class RebateServiceTests
{
    RebateService rebateService;
    private Mock<IRebateDataStore> _mockRebateDataStore;
    private Mock<IProductDataStore> _mockProductDataStore;
    private string TestRebateID = Guid.NewGuid().ToString();
    private string TestProductID = Guid.NewGuid().ToString();

    [SetUp]
    public void SetUp()
    {
        _mockRebateDataStore = new Mock<IRebateDataStore>();
        _mockProductDataStore = new Mock<IProductDataStore>();
        rebateService = new RebateService(_mockRebateDataStore.Object, _mockProductDataStore.Object);
    }

    [Test]
    public void Calculate_ShouldReturnImmediatelyWhenRequestIsNull()
    {
        //arrange
        CalculateRebateRequest nullRequest = null;

        //act
        var actual = rebateService.Calculate(nullRequest);

        //assert
        Assert.IsFalse(actual.Success);
        _mockRebateDataStore.Verify(c => c.GetRebate(It.IsAny<string>()), Times.Never);
        _mockProductDataStore.Verify(c => c.GetProduct(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Calculate_ShouldReturnImmediatelyWhenRebateIsNull()
    {
        //arrange
        Rebate nullRebate = null;
        CalculateRebateRequest request = new CalculateRebateRequest
        {
             RebateIdentifier = TestRebateID,
        };
        _mockRebateDataStore = new Mock<IRebateDataStore>();
        _mockRebateDataStore.Setup(obj => obj.GetRebate(TestRebateID)).Returns(nullRebate);

        rebateService = new RebateService(_mockRebateDataStore.Object, _mockProductDataStore.Object);

        //act
        var actual = rebateService.Calculate(request);

        //assert
        Assert.IsFalse(actual.Success);
        _mockRebateDataStore.Verify(c => c.GetRebate(request.RebateIdentifier), Times.Once);
        _mockProductDataStore.Verify(c => c.GetProduct(request.ProductIdentifier), Times.Never);
    }
    
    [Test]
    public void Calculate_ShouldReturnImmediatelyWhenProductIsNull()
    {
        //arrange
        Rebate rebate = new Rebate();
        Product nullProduct = null;
        CalculateRebateRequest request = new CalculateRebateRequest
        {
             RebateIdentifier = TestRebateID,
        };
        _mockRebateDataStore = new Mock<IRebateDataStore>();
        _mockRebateDataStore.Setup(obj => obj.GetRebate(TestRebateID)).Returns(rebate);
        _mockProductDataStore = new Mock<IProductDataStore>();
        _mockProductDataStore.Setup(obj => obj.GetProduct(TestProductID)).Returns(nullProduct);

        rebateService = new RebateService(_mockRebateDataStore.Object, _mockProductDataStore.Object);

        //act
        var actual = rebateService.Calculate(request);

        //assert
        Assert.IsFalse(actual.Success);
        _mockRebateDataStore.Verify(c => c.GetRebate(request.RebateIdentifier), Times.Once);
        _mockProductDataStore.Verify(c => c.GetProduct(request.ProductIdentifier), Times.Once);
    }

    [Test]
    public void Calculate_ShouldCalculateCorrectValueForFixedCashAmountWhenValid()
    {
        //arrange
        Rebate rebate = new Rebate 
        {
            Amount = 1,
            Identifier = TestRebateID,
            Incentive = IncentiveType.FixedCashAmount
        };
        Product product = new Product 
        {
            Identifier = TestProductID,
            SupportedIncentives = SupportedIncentiveType.FixedCashAmount
        };
        CalculateRebateRequest request = new CalculateRebateRequest
        {
            RebateIdentifier = TestRebateID,
            ProductIdentifier = TestProductID,
        };
        _mockRebateDataStore = new Mock<IRebateDataStore>();
        _mockRebateDataStore.Setup(obj => obj.GetRebate(TestRebateID)).Returns(rebate);
        _mockProductDataStore = new Mock<IProductDataStore>();
        _mockProductDataStore.Setup(obj => obj.GetProduct(TestProductID)).Returns(product);

        rebateService = new RebateService(_mockRebateDataStore.Object, _mockProductDataStore.Object);

        //act
        var actual = rebateService.Calculate(request);

        //assert
        Assert.IsTrue(actual.Success);
        Assert.AreEqual(rebate.Amount, actual.RebateAmount);
        _mockRebateDataStore.Verify(c => c.GetRebate(request.RebateIdentifier), Times.Once);
        _mockProductDataStore.Verify(c => c.GetProduct(request.ProductIdentifier), Times.Once);
        _mockRebateDataStore.Verify(c => c.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>()),Times.Once);
    }
    [Test]
    public void Calculate_ShouldCalculateCorrectValueForFixedCashAmountWhenNotValidIncentive()
    {
        //arrange
        Rebate rebate = new Rebate 
        {
            Amount = 1,
            Identifier = TestRebateID,
            Incentive = IncentiveType.FixedCashAmount
        };
        Product product = new Product 
        {
            Identifier = TestProductID,
            SupportedIncentives = SupportedIncentiveType.FixedRateRebate
        };
        CalculateRebateRequest request = new CalculateRebateRequest
        {
            RebateIdentifier = TestRebateID,
            ProductIdentifier = TestProductID,
        };
        _mockRebateDataStore = new Mock<IRebateDataStore>();
        _mockRebateDataStore.Setup(obj => obj.GetRebate(TestRebateID)).Returns(rebate);
        _mockProductDataStore = new Mock<IProductDataStore>();
        _mockProductDataStore.Setup(obj => obj.GetProduct(TestProductID)).Returns(product);

        rebateService = new RebateService(_mockRebateDataStore.Object, _mockProductDataStore.Object);

        //act
        var actual = rebateService.Calculate(request);

        //assert
        Assert.IsFalse(actual.Success);
        Assert.AreEqual(0m, actual.RebateAmount);
        _mockRebateDataStore.Verify(c => c.GetRebate(request.RebateIdentifier), Times.Once);
        _mockProductDataStore.Verify(c => c.GetProduct(request.ProductIdentifier), Times.Once);
        _mockRebateDataStore.Verify(c => c.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>()),Times.Never);
    }
    
    [Test]
    public void Calculate_ShouldCalculateCorrectValueForFixedCashAmountWhenNotValidRebateAmount()
    {
        //arrange
        Rebate rebate = new Rebate 
        {
            Amount = 0,
            Identifier = TestRebateID,
            Incentive = IncentiveType.FixedCashAmount
        };
        Product product = new Product 
        {
            Identifier = TestProductID,
            SupportedIncentives = SupportedIncentiveType.FixedCashAmount
        };
        CalculateRebateRequest request = new CalculateRebateRequest
        {
            RebateIdentifier = TestRebateID,
            ProductIdentifier = TestProductID,
        };
        _mockRebateDataStore = new Mock<IRebateDataStore>();
        _mockRebateDataStore.Setup(obj => obj.GetRebate(TestRebateID)).Returns(rebate);
        _mockProductDataStore = new Mock<IProductDataStore>();
        _mockProductDataStore.Setup(obj => obj.GetProduct(TestProductID)).Returns(product);

        rebateService = new RebateService(_mockRebateDataStore.Object, _mockProductDataStore.Object);

        //act
        var actual = rebateService.Calculate(request);

        //assert
        Assert.IsFalse(actual.Success);
        Assert.AreEqual(0m, actual.RebateAmount);
        _mockRebateDataStore.Verify(c => c.GetRebate(request.RebateIdentifier), Times.Once);
        _mockProductDataStore.Verify(c => c.GetProduct(request.ProductIdentifier), Times.Once);
        _mockRebateDataStore.Verify(c => c.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>()),Times.Never);
    }
    
    [Test]
    public void Calculate_ShouldCalculateCorrectValueForFixedRateRebateWhenValid()
    {
        //arrange
        Rebate rebate = new Rebate 
        {
            Amount = 1,
            Identifier = TestRebateID,
            Incentive = IncentiveType.FixedRateRebate,
            Percentage = 10
        };
        Product product = new Product 
        {
            Identifier = TestProductID,
            SupportedIncentives = SupportedIncentiveType.FixedRateRebate,
            Price = 10
        };
        CalculateRebateRequest request = new CalculateRebateRequest
        {
            RebateIdentifier = TestRebateID,
            ProductIdentifier = TestProductID,
            Volume = 1
        };
        _mockRebateDataStore = new Mock<IRebateDataStore>();
        _mockRebateDataStore.Setup(obj => obj.GetRebate(TestRebateID)).Returns(rebate);
        _mockProductDataStore = new Mock<IProductDataStore>();
        _mockProductDataStore.Setup(obj => obj.GetProduct(TestProductID)).Returns(product);

        rebateService = new RebateService(_mockRebateDataStore.Object, _mockProductDataStore.Object);

        var expectedAmount = product.Price * rebate.Percentage * request.Volume;

        //act
        var actual = rebateService.Calculate(request);

        //assert
        Assert.IsTrue(actual.Success);
        Assert.AreEqual(expectedAmount, actual.RebateAmount);
        _mockRebateDataStore.Verify(c => c.GetRebate(request.RebateIdentifier), Times.Once);
        _mockProductDataStore.Verify(c => c.GetProduct(request.ProductIdentifier), Times.Once);
        _mockRebateDataStore.Verify(c => c.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>()),Times.Once);
    }
    [Test]
    public void Calculate_ShouldCalculateCorrectValueForFixedRateRebateWhenNotValidIncentive()
    {
        //arrange
        Rebate rebate = new Rebate 
        {
            Amount = 1,
            Identifier = TestRebateID,
            Incentive = IncentiveType.FixedRateRebate,
            Percentage = 10
        };
        Product product = new Product 
        {
            Identifier = TestProductID,
            SupportedIncentives = SupportedIncentiveType.FixedCashAmount,
            Price = 10
        };
        CalculateRebateRequest request = new CalculateRebateRequest
        {
            RebateIdentifier = TestRebateID,
            ProductIdentifier = TestProductID,
            Volume = 1
        };
        _mockRebateDataStore = new Mock<IRebateDataStore>();
        _mockRebateDataStore.Setup(obj => obj.GetRebate(TestRebateID)).Returns(rebate);
        _mockProductDataStore = new Mock<IProductDataStore>();
        _mockProductDataStore.Setup(obj => obj.GetProduct(TestProductID)).Returns(product);

        rebateService = new RebateService(_mockRebateDataStore.Object, _mockProductDataStore.Object);

        //act
        var actual = rebateService.Calculate(request);

        //assert
        Assert.IsFalse(actual.Success);
        Assert.AreEqual(0m, actual.RebateAmount);
        _mockRebateDataStore.Verify(c => c.GetRebate(request.RebateIdentifier), Times.Once);
        _mockProductDataStore.Verify(c => c.GetProduct(request.ProductIdentifier), Times.Once);
        _mockRebateDataStore.Verify(c => c.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>()),Times.Never);
    }
    [Test]
    public void Calculate_ShouldCalculateCorrectValueForFixedRateRebateWhenNotValidRebatePriceIncentiveAndVolume()
    {
        //arrange
        Rebate rebate = new Rebate 
        {
            Amount = 0,
            Identifier = TestRebateID,
            Incentive = IncentiveType.FixedRateRebate
        };
        Product product = new Product 
        {
            Identifier = TestProductID,
            SupportedIncentives = SupportedIncentiveType.FixedRateRebate
        };
        CalculateRebateRequest request = new CalculateRebateRequest
        {
            RebateIdentifier = TestRebateID,
            ProductIdentifier = TestProductID,
        };
        _mockRebateDataStore = new Mock<IRebateDataStore>();
        _mockRebateDataStore.Setup(obj => obj.GetRebate(TestRebateID)).Returns(rebate);
        _mockProductDataStore = new Mock<IProductDataStore>();
        _mockProductDataStore.Setup(obj => obj.GetProduct(TestProductID)).Returns(product);

        rebateService = new RebateService(_mockRebateDataStore.Object, _mockProductDataStore.Object);

        //act
        var actual = rebateService.Calculate(request);

        //assert
        Assert.IsFalse(actual.Success);
        Assert.AreEqual(0m, actual.RebateAmount);
        _mockRebateDataStore.Verify(c => c.GetRebate(request.RebateIdentifier), Times.Once);
        _mockProductDataStore.Verify(c => c.GetProduct(request.ProductIdentifier), Times.Once);
        _mockRebateDataStore.Verify(c => c.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>()),Times.Never);
    }
    
    [Test]
    public void Calculate_ShouldCalculateCorrectValueForAmountPerUomWhenValid()
    {
        //arrange
        Rebate rebate = new Rebate 
        {
            Amount = 1,
            Identifier = TestRebateID,
            Incentive = IncentiveType.AmountPerUom,
            Percentage = 10
        };
        Product product = new Product 
        {
            Identifier = TestProductID,
            SupportedIncentives = SupportedIncentiveType.AmountPerUom,
            Price = 10
        };
        CalculateRebateRequest request = new CalculateRebateRequest
        {
            RebateIdentifier = TestRebateID,
            ProductIdentifier = TestProductID,
            Volume = 1
        };
        _mockRebateDataStore = new Mock<IRebateDataStore>();
        _mockRebateDataStore.Setup(obj => obj.GetRebate(TestRebateID)).Returns(rebate);
        _mockProductDataStore = new Mock<IProductDataStore>();
        _mockProductDataStore.Setup(obj => obj.GetProduct(TestProductID)).Returns(product);

        rebateService = new RebateService(_mockRebateDataStore.Object, _mockProductDataStore.Object);

        var expectedAmount = rebate.Amount * request.Volume;

        //act
        var actual = rebateService.Calculate(request);

        //assert
        Assert.IsTrue(actual.Success);
        Assert.AreEqual(expectedAmount, actual.RebateAmount);
        _mockRebateDataStore.Verify(c => c.GetRebate(request.RebateIdentifier), Times.Once);
        _mockProductDataStore.Verify(c => c.GetProduct(request.ProductIdentifier), Times.Once);
        _mockRebateDataStore.Verify(c => c.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>()),Times.Once);
    }
    [Test]
    public void Calculate_ShouldCalculateCorrectValueForAmountPerUomWhenNotValidIncentive()
    {
        //arrange
        Rebate rebate = new Rebate 
        {
            Amount = 1,
            Identifier = TestRebateID,
            Incentive = IncentiveType.AmountPerUom,
            Percentage = 10
        };
        Product product = new Product 
        {
            Identifier = TestProductID,
            SupportedIncentives = SupportedIncentiveType.FixedCashAmount,
            Price = 10
        };
        CalculateRebateRequest request = new CalculateRebateRequest
        {
            RebateIdentifier = TestRebateID,
            ProductIdentifier = TestProductID,
            Volume = 1
        };
        _mockRebateDataStore = new Mock<IRebateDataStore>();
        _mockRebateDataStore.Setup(obj => obj.GetRebate(TestRebateID)).Returns(rebate);
        _mockProductDataStore = new Mock<IProductDataStore>();
        _mockProductDataStore.Setup(obj => obj.GetProduct(TestProductID)).Returns(product);

        rebateService = new RebateService(_mockRebateDataStore.Object, _mockProductDataStore.Object);

        //act
        var actual = rebateService.Calculate(request);

        //assert
        Assert.IsFalse(actual.Success);
        Assert.AreEqual(0m, actual.RebateAmount);
        _mockRebateDataStore.Verify(c => c.GetRebate(request.RebateIdentifier), Times.Once);
        _mockProductDataStore.Verify(c => c.GetProduct(request.ProductIdentifier), Times.Once);
        _mockRebateDataStore.Verify(c => c.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>()),Times.Never);
    }
    [Test]
    public void Calculate_ShouldCalculateCorrectValueAmountPerUomWhenNotValidRebateAmount()
    {
        //arrange
        Rebate rebate = new Rebate 
        {
            Amount = 0,
            Identifier = TestRebateID,
            Incentive = IncentiveType.AmountPerUom
        };
        Product product = new Product 
        {
            Identifier = TestProductID,
            SupportedIncentives = SupportedIncentiveType.AmountPerUom
        };
        CalculateRebateRequest request = new CalculateRebateRequest
        {
            RebateIdentifier = TestRebateID,
            ProductIdentifier = TestProductID,
        };
        _mockRebateDataStore = new Mock<IRebateDataStore>();
        _mockRebateDataStore.Setup(obj => obj.GetRebate(TestRebateID)).Returns(rebate);
        _mockProductDataStore = new Mock<IProductDataStore>();
        _mockProductDataStore.Setup(obj => obj.GetProduct(TestProductID)).Returns(product);

        rebateService = new RebateService(_mockRebateDataStore.Object, _mockProductDataStore.Object);

        //act
        var actual = rebateService.Calculate(request);

        //assert
        Assert.IsFalse(actual.Success);
        Assert.AreEqual(0m, actual.RebateAmount);
        _mockRebateDataStore.Verify(c => c.GetRebate(request.RebateIdentifier), Times.Once);
        _mockProductDataStore.Verify(c => c.GetProduct(request.ProductIdentifier), Times.Once);
        _mockRebateDataStore.Verify(c => c.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>()),Times.Never);
    }

}
