using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using WService.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

//builder - middleware
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//Add Authorization button to enable show data when i use corrected token

builder.Services.AddSwaggerGen(options => {
    //[ Step 1 ] JWT - Bearer Plan
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        //Name : about token, we send it in header section        
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter Bearer {your JWT token}"
    });

    //[ Step 2 ] - configurations about implementation JWT token requirement in every endpoint controller that have [Authorize]
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddCors(options => {
    options.AddPolicy("StudentCORSPolicy", policy => {
        policy
        .WithOrigins("http://localhost:5256", "https://localhost:7065")
        .AllowAnyHeader() // if any api request that have header [ may be content Token ] , as a server accept this request without any problem!
        .AllowAnyMethod(); // accept any http method like GET,POST,...., as a server accept this request without any problem!
    });
});

//[API Security Layer - needed to validate api request before show response]
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(option => {
        option.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "WService",
            ValidAudience = "StudentAPIUser",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("asc-ll78a#!78%!AA$$7ara3b3m99999a99a-su")),
            ClockSkew= TimeSpan.Zero // What meaning it will adding any additional time like [ 5m ] up of the defined expired token time
        };
    });


builder.Services.AddAuthorization(options =>{
    options.AddPolicy("StudentOwner", policy => {
       policy.Requirements.Add(new StudentOwnerRequirement());
    });
});

//IAuthorizationHandler: implementing guard in our system
builder.Services.AddSingleton<IAuthorizationHandler, StudentOwnerHandler>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

// that's enable redirect user into https if try to use http, it will not enable HTTPS! where https must configured
app.UseHttpsRedirection();
app.UseCors("StudentCORSPolicy");

//middleware that handle JWT processes and validations
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
