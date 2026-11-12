using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using WService.Authorization;

//builder - middleware
var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("AuthLimiter", httpContext => {
        var IPAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "UnKnown";

        // [ JUST ] I Want from you to clear next section:
        //GetFixedWindowLimiter: [ Time Manager about rate limiting - Core Section ]

        //[ Fixed Approach ] Many requests can be made within a very short period around the allowed limit, and this situation is called a (border explosion). so the solution about time management of windowing is Sliding Window

        //GetSlidingWindowLimiter: Same [ GetFixedWindowLimiter ] Idea but completely prevents burst attacks [ Recommended Way ]!

        //The [ Sliding Window ] does not revert all permissions at once, but rather releases the permissions consumed in the segment that exited the time window [ gradually ] for the same request! also

        //The user can submit a new request as soon as one or more permits are returned, and does not need to wait until all previous requests are completed or the counter returns to its original state completely.

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: IPAddress,
            factory: _ => new SlidingWindowRateLimiterOptions {
                // attempt time to use endpoint
                PermitLimit = 3,
                // If rate limit happen, the service it will waiting 1m then it will enable to use endpoint again
                //so in 1m, the user can do request 3 time just!
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 2,//implementing sliding window idea to the 1m / 2
                //If server have too far request, don't let his requests go through the queue of expectations, but reject them immediately, same [ ClockSkew= TimeSpan.Zero ] Idea
                QueueLimit = 0
            }
        );
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

// that's enable redirect user into https if try to use http, it will not enable HTTPS! where https must configured
app.UseHttpsRedirection();
app.UseCors("StudentCORSPolicy");

app.UseRateLimiter();

//Use that be Handle Every Request , and based on this signal [  StatusCodes.Status429TooManyRequests ] we handle endpoints that implementing rate limiting , so you can any signal to refer target endpoint

app.Use(async (context,next) => {
    await next(); // [ Step 1 ] that mean we enable request to be execute needed endpoints
    
    //[ Step 2 ] we will do check if rate limiting happen, if true show needed message just
    //context be as: HttpContext [ Main Http Request to the server ]
    if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests){
        await context.Response.WriteAsync("something went wrong.");
    }

    if (context.Response.StatusCode == StatusCodes.Status403Forbidden){

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var IPAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path.ToString();
        app.Logger.LogWarning("Forbidden access. UserId={UserId}, Path={Path}, IP={IP}",userId,path,IPAddress);
    }
});

app.MapControllers();
app.Run();
