using Scheduled.Stock.Purchase.Shared;
using Shouldly;

namespace Scheduled.Stock.Purchase.Shared.Tests;

public class ResultTests
{
    [Fact]
    public void NonGeneric_Success_and_Failure()
    {
        var ok = Result.Success();
        ok.IsSuccess.ShouldBeTrue();
        ok.IsFailure.ShouldBeFalse();

        var err = Result.Failure(new Error("E.Test", "Test error"));
        err.IsSuccess.ShouldBeFalse();
        err.IsFailure.ShouldBeTrue();
        err.Error.Code.ShouldBe("E.Test");
    }

    [Fact]
    public void Generic_Success_Value_and_Implicit_Conversions()
    {
        Result<string> r = "hello";
        r.IsSuccess.ShouldBeTrue();
        r.Value.ShouldBe("hello");

        Result<string> fromError = new Error("E.G", "g error");
        fromError.IsFailure.ShouldBeTrue();
        fromError.Error.Code.ShouldBe("E.G");
    }
}
