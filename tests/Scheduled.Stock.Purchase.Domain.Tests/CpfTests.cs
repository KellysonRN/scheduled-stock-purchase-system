namespace Scheduled.Stock.Purchase.Domain.Tests;

public class CpfTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    public void Should_Create_Valid_Cpf(string validCpf)
    {
        var cpf = new Cpf(validCpf);

        Assert.Equal("52998224725", cpf.Numero);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("123")]
    [InlineData("11111111111")] // repeated digits
    [InlineData("12345678900")] // invalid
    public void Should_Throw_Exception_For_Invalid_Cpf(string invalidCpf)
    {
        Assert.Throws<ArgumentException>(() => new Cpf(invalidCpf));
    }

    [Fact]
    public void Should_Remove_Mask_From_Cpf()
    {
        var cpf = new Cpf("529.982.247-25");

        Assert.Equal("52998224725", cpf.Numero);
    }

    [Fact]
    public void Should_Format_Cpf_Correctly_In_ToString()
    {
        var cpf = new Cpf("52998224725");

        Assert.Equal("529.982.247-25", cpf.ToString());
    }

    [Fact]
    public void Should_Consider_Cpfs_Equal_When_Values_Are_The_Same()
    {
        var cpf1 = new Cpf("52998224725");
        var cpf2 = new Cpf("529.982.247-25");

        Assert.True(cpf1 == cpf2);
        Assert.Equal(cpf1, cpf2);
    }

    [Fact]
    public void Should_Consider_Cpfs_Different_When_Values_Are_Different()
    {
        var cpf1 = new Cpf("52998224725");
        var cpf2 = new Cpf("12345678909");

        Assert.False(cpf1 == cpf2);
        Assert.NotEqual(cpf1, cpf2);
    }
}
