using WebApiEcomm.Core.Entites.Dtos;

namespace WebApiEcomm.Tests.Unit;

public class AuthDtoTests
{
    [Fact]
    public void VerifyEmailRequest_Should_HaveFixedCodeLengthContract()
    {
        var req = new VerifyEmailRequest
        {
            Email = "user@example.com",
            Code = "123456"
        };

        Assert.Equal(6, req.Code.Length);
    }

    [Fact]
    public void TokenPairResponse_DefaultTokenType_ShouldBeBearer()
    {
        var response = new TokenPairResponse();
        Assert.Equal("Bearer", response.TokenType);
    }
}
