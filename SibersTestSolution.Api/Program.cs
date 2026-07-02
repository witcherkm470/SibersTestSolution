using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using SibersTestSolution.Api.Validation;
using SibersTestSolution.Application.DI;
using SibersTestSolution.Application.Validation.Employees;
using SibersTestSolution.Infrastructure.DI;
using SibersTestSolution.Infrastructure.Database;
using SibersTestSolution.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);
var sharedSettingsPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "appsettings.json"));

builder.Configuration.AddJsonFile(sharedSettingsPath, optional: false, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateEmployeeRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EmployeeQueryParametersValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    foreach (var xmlPath in Directory.EnumerateFiles(AppContext.BaseDirectory, "SibersTestSolution.*.xml"))
    {
        options.IncludeXmlComments(xmlPath);
    }
});
builder.Services.AddSibersTestSolutionInfrastructure(builder.Configuration);
builder.Services.AddSibersTestSolutionApplication();
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 3;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<SibersTestSolutionDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

await app.Services.SeedIdentityAsync();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
