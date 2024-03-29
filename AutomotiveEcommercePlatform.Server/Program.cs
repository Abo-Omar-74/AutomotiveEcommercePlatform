using System.Text;
using AutomotiveEcommercePlatform.Server.Configurations;
using AutomotiveEcommercePlatform.Server.Data;
using AutomotiveEcommercePlatform.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReactApp1.Server.Data;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddAuthorization();


builder.Services.AddDefaultIdentity < ApplicationUser>()
    .AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();



// builder.Services.AddControllers();
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adding Jwt Configs 

builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));


var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));


// Enable Cors 

builder.Services.AddCors();

// Adding IAuthService

builder.Services.AddScoped<IAuthService, AuthService>();

// Adding ICarService
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(jwt =>
{
    var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtConfig:Secret").Value);

    jwt.SaveToken = true;

    jwt.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key) , 
        ValidateIssuer = false, // for dev 
        ValidateAudience = false, // for dev
        RequireExpirationTime = false, // for dev -- needs to be updated when refresh tokens is added 
        ValidateLifetime = true
    };
});



var app = builder.Build();


app.UseDefaultFiles();
app.UseStaticFiles();
app.MapIdentityApi<ApplicationUser>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// enable cors 

app.UseCors(c => c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
