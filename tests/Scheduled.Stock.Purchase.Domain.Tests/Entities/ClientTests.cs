using Scheduled.Stock.Purchase.Domain.Entities;
using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Shouldly;

namespace Scheduled.Stock.Purchase.Domain.Tests.Entities;

public class ClientTests
{
    [Fact]
    public void Should_Create_Client_When_Data_Is_Valid()
    {
        // Arrange
        var cpfResult = Cpf.Create("52998224725");
        cpfResult.IsSuccess.ShouldBeTrue();

        var cpf = cpfResult.Value!;
        var fullName = "Maria Silva";
        var email = "maria.silva@example.com";

        // Act
        var result = Client.Create(fullName, email, cpf);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var client = result.Value;
        client.ShouldNotBeNull();
        client.FullName.ShouldBe(fullName);
        client.Email.ShouldBe(email);
        client.Cpf.ShouldBe(cpf);
        (DateTime.UtcNow - client.CreatedAt).ShouldBeLessThan(TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Fail_When_FullName_Is_Invalid(string? fullName)
    {
        // Arrange
        var cpfResult = Cpf.Create("52998224725");
        cpfResult.IsSuccess.ShouldBeTrue();

        var cpf = cpfResult.Value!;
        var email = "maria.silva@example.com";

        // Act
        var result = Client.Create(fullName, email, cpf);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ClientErrors.InvalidFullName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    public void Should_Fail_When_Email_Is_Invalid(string? email)
    {
        // Arrange
        var cpfResult = Cpf.Create("52998224725");
        cpfResult.IsSuccess.ShouldBeTrue();

        var cpf = cpfResult.Value!;
        var fullName = "Maria Silva";

        // Act
        var result = Client.Create(fullName, email, cpf);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ClientErrors.InvalidEmail);
    }

    [Fact]
    public void Should_Fail_When_Cpf_Is_Null()
    {
        // Arrange
        var fullName = "Maria Silva";
        var email = "maria.silva@example.com";

        // Act
        var result = Client.Create(fullName, email, null);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ClientErrors.InvalidCpf);
    }

    [Fact]
    public void Should_Generate_Different_Ids_For_Different_Clients()
    {
        // Arrange
        var cpfResult = Cpf.Create("52998224725");
        cpfResult.IsSuccess.ShouldBeTrue();

        var cpf = cpfResult.Value!;
        var fullName = "Maria Silva";
        var email = "maria.silva@example.com";

        // Act
        var client1 = Client.Create(fullName, email, cpf).Value!;
        var client2 = Client.Create(fullName, email, cpf).Value!;

        // Assert
        client1.Id.ShouldNotBe(client2.Id);
    }
}
