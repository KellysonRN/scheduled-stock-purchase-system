using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Shouldly;

namespace Scheduled.Stock.Purchase.Domain.Tests;

public class CpfTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    public void Should_Create_Valid_Cpf(string validCpf)
    {
        var result = Cpf.Create(validCpf);

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Number.ShouldBe("52998224725");
    }

    [Fact]
    public void Should_Return_Error_When_Cpf_Is_Whitespace()
    {
        var result = Cpf.Create("   ");

        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldBe("CPF cannot be empty");
    }

    [Fact]
    public void Should_Return_Error_When_Cpf_Is_Null()
    {
        var result = Cpf.Create(null!);

        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldBe("CPF cannot be empty");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789012")]
    [InlineData("123.456.789")]
    [InlineData("123.456.78")]
    public void Should_Return_Error_For_Invalid_Length(string invalidCpf)
    {
        var result = Cpf.Create(invalidCpf);

        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldBe("CPF must have 11 digits");
    }

    [Theory]
    [InlineData("11111111111")]
    [InlineData("22222222222")]
    [InlineData("00000000000")]
    public void Should_Return_Error_For_Repeated_Digits(string invalidCpf)
    {
        var result = Cpf.Create(invalidCpf);

        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldBe("CPF cannot have all digits equal");
    }

    [Theory]
    [InlineData("12345678900")]
    [InlineData("52998224726")]
    public void Should_Return_Error_For_Invalid_Checksum(string invalidCpf)
    {
        var result = Cpf.Create(invalidCpf);

        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldBe("CPF is invalid");
    }

    [Fact]
    public void Should_Remove_Mask_From_Cpf()
    {
        var result = Cpf.Create("529.982.247-25");

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Number.ShouldBe("52998224725");
    }

    [Fact]
    public void Should_Format_Cpf_Correctly_In_ToString()
    {
        var result = Cpf.Create("52998224725");

        result.IsSuccess.ShouldBeTrue();
        result.Value!.ToString().ShouldBe("529.982.247-25");
    }

    [Fact]
    public void Should_Consider_Cpfs_Equal_When_Values_Are_The_Same()
    {
        var cpf1 = Cpf.Create("52998224725").Value;
        var cpf2 = Cpf.Create("529.982.247-25").Value;

        cpf1.ShouldBe(cpf2);
    }

    [Fact]
    public void Should_Consider_Cpfs_Different_When_Values_Are_Different()
    {
        var cpf1 = Cpf.Create("52998224725").Value;
        var cpf2 = Cpf.Create("12345678909").Value;

        cpf1.ShouldNotBe(cpf2);
    }
}
