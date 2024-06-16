using Microsoft.Extensions.Options;

namespace Identity.API.Options;

public class JwtOptionsConfiguration : IConfigureOptions<JwtOptions>
{
    private const string Section = "Jwt";
    private readonly IConfiguration _configuration;

    public JwtOptionsConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(JwtOptions options)
    {
        _configuration.GetSection(Section).Bind(options);
    }
}
