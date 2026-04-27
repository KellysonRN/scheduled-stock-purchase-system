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
        Assert.Equal("52998224725", result.Value.Numero);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("11111111111")]
    [InlineData("12345678900")]
    public void Should_Return_Failure_For_Invalid_Cpf(string invalidCpf)
    {
        var result = Cpf.Create(invalidCpf);

        Assert.True(result.IsFailure);
        Assert.Null(result.Value);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public void Should_Return_Error_When_Cpf_Is_Null_Or_Empty()
    {
        var result = Cpf.Create("");

        Assert.True(result.IsFailure);
        Assert.Equal("CPF cannot be empty", result.Error);
    }

    [Fact]
    public void Should_Remove_Mask_From_Cpf()
    {
        var result = Cpf.Create("529.982.247-25");

        Assert.True(result.IsSuccess);
        Assert.Equal("52998224725", result.Value.Numero);
    }

    [Fact]
    public void Should_Format_Cpf_Correctly_In_ToString()
    {
        var result = Cpf.Create("52998224725");

        Assert.True(result.IsSuccess);
        Assert.Equal("529.982.247-25", result.Value.ToString());
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
