using Scheduled.Stock.Purchase.Domain.ValueObjects;

namespace Scheduled.Stock.Purchase.Domain.Tests;

public class CpfTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    public void Should_Create_Valid_Cpf(string validCpf)
    {
        var result = Cpf.Create(validCpf);

        Assert.True(result.IsSuccess);
        Assert.Equal("52998224725", result.Value?.Number);
    }

    [Fact]
    public void Should_Return_Error_When_Cpf_Is_Whitespace()
    {
        var result = Cpf.Create("   ");

        Assert.True(result.IsFailure);
        Assert.Equal("CPF cannot be empty", result.Error.Message);
    }

    [Fact]
    public void Should_Return_Error_When_Cpf_Is_Null()
    {
        var result = Cpf.Create(null!);

        Assert.True(result.IsFailure);
        Assert.Equal("CPF cannot be empty", result.Error.Message);
    }

    [Theory]
    [InlineData("123")] // Too short
    [InlineData("123456789012")] // Too long
    [InlineData("123.456.789")] // Non-digits making it 9 digits
    [InlineData("123.456.78")] // 8 digits
    public void Should_Return_Error_For_Invalid_Length(string invalidCpf)
    {
        var result = Cpf.Create(invalidCpf);

        Assert.True(result.IsFailure);
        Assert.Equal("CPF must have 11 digits", result.Error.Message);
    }

    [Theory]
    [InlineData("11111111111")]
    [InlineData("22222222222")]
    [InlineData("00000000000")]
    public void Should_Return_Error_For_Repeated_Digits(string invalidCpf)
    {
        var result = Cpf.Create(invalidCpf);

        Assert.True(result.IsFailure);
        Assert.Equal("CPF cannot have all digits equal", result.Error.Message);
    }

    [Theory]
    [InlineData("12345678900")] // Invalid checksum
    [InlineData("52998224726")] // Valid first 9, but wrong last digit
    public void Should_Return_Error_For_Invalid_Checksum(string invalidCpf)
    {
        var result = Cpf.Create(invalidCpf);

        Assert.True(result.IsFailure);
        Assert.Equal("CPF is invalid", result.Error.Message);
    }

    [Fact]
    public void Should_Remove_Mask_From_Cpf()
    {
        var result = Cpf.Create("529.982.247-25");

        Assert.True(result.IsSuccess);
        Assert.Equal("52998224725", result.Value?.Number);
    }

    [Fact]
    public void Should_Format_Cpf_Correctly_In_ToString()
    {
        var result = Cpf.Create("52998224725");

        Assert.True(result.IsSuccess);
        Assert.Equal("529.982.247-25", result.Value?.ToString());
    }

    [Fact]
    public void Should_Consider_Cpfs_Equal_When_Values_Are_The_Same()
    {
        var cpf1 = Cpf.Create("52998224725").Value;
        var cpf2 = Cpf.Create("529.982.247-25").Value;

        Assert.Equal(cpf1, cpf2);
    }

    [Fact]
    public void Should_Consider_Cpfs_Different_When_Values_Are_Different()
    {
        var cpf1 = Cpf.Create("52998224725").Value;
        var cpf2 = Cpf.Create("12345678909").Value;

        Assert.NotEqual(cpf1, cpf2);
    }
}
