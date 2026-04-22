using ConnectDB.Data;
using ConnectDB.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace ConnectDB;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var jwtSection = builder.Configuration.GetSection("Jwt");
        var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var jwtIssuer = jwtSection["Issuer"] ?? "ConnectDB";
        var jwtAudience = jwtSection["Audience"] ?? "ConnectDBClient";
        var jwtSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var useInMemoryDb = builder.Configuration.GetValue<bool>("UseInMemoryDatabase") ||
            string.Equals(
                Environment.GetEnvironmentVariable("USE_INMEMORY_DB"),
                "true",
                StringComparison.OrdinalIgnoreCase);

        builder.Services.AddControllers();
        if (useInMemoryDb)
        {
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("ConnectDBInMemory"));
        }
        else
        {
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        }

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = jwtSigningKey,
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

        builder.Services.AddAuthorization();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Gym Management API",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Nhap token theo dang: Bearer {token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred." });
            });
        });

        if (useInMemoryDb)
        {
            SeedInMemoryData(app);
        }

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseCors();
        if (!app.Environment.IsProduction())
        {
            app.UseHttpsRedirection();
        }
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

    private static void SeedInMemoryData(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role { Name = "Admin", Description = "System administrator" },
                new Role { Name = "Receptionist", Description = "Front desk staff" },
                new Role { Name = "Trainer", Description = "Trainer role" },
                new Role { Name = "Member", Description = "Gym member role" });
            context.SaveChanges();
        }

        if (!context.Trainers.Any())
        {
            context.Trainers.Add(new Trainer
            {
                TrainerCode = "TR001",
                FullName = "Demo Trainer",
                Specialty = "General Fitness",
                Phone = "0900000001",
                Email = "trainer.demo@gym.local",
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();
        }

        if (!context.Members.Any())
        {
            context.Members.Add(new Member
            {
                MemberCode = "MB001",
                FullName = "Demo Member",
                DateOfBirth = new DateTime(2000, 1, 1),
                Gender = "Male",
                Phone = "0900000002",
                Email = "member.demo@gym.local",
                Address = "Demo Address",
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();
        }

        if (!context.MembershipPackages.Any())
        {
            context.MembershipPackages.Add(new MembershipPackage
            {
                PackageCode = "PK001",
                Name = "Demo Package",
                Description = "Package for API demo",
                DurationDays = 30,
                Price = 500000,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();
        }

        var trainer = context.Trainers.First();
        var member = context.Members.First();
        var membershipPackage = context.MembershipPackages.First();

        if (!context.Subscriptions.Any())
        {
            context.Subscriptions.Add(new Subscription
            {
                MemberId = member.Id,
                MembershipPackageId = membershipPackage.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(membershipPackage.DurationDays),
                Status = "Active",
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();
        }

        var subscription = context.Subscriptions.First();

        if (!context.Schedules.Any())
        {
            context.Schedules.Add(new Schedule
            {
                Title = "Demo Session",
                ScheduleDate = DateTime.Today,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(9, 0, 0),
                TrainerId = trainer.Id,
                MemberId = member.Id,
                Notes = "Sample schedule for testing"
            });
            context.SaveChanges();
        }

        var schedule = context.Schedules.First();

        if (!context.Attendances.Any())
        {
            context.Attendances.Add(new Attendance
            {
                ScheduleId = schedule.Id,
                MemberId = member.Id,
                Status = "Present",
                Note = "Seeded attendance",
                RecordedAt = DateTime.Now
            });
            context.SaveChanges();
        }

        if (!context.Payments.Any())
        {
            context.Payments.Add(new Payment
            {
                SubscriptionId = subscription.Id,
                Amount = membershipPackage.Price,
                PaymentDate = DateTime.Now,
                PaymentMethod = "Cash",
                Status = "Paid",
                Note = "Seeded payment"
            });
            context.SaveChanges();
        }

        if (!context.Accounts.Any())
        {
            var adminRoleId = context.Roles
                .Where(r => r.Name == "Admin")
                .Select(r => (int?)r.Id)
                .FirstOrDefault();
            if (!adminRoleId.HasValue)
            {
                return;
            }

            context.Accounts.Add(new Account
            {
                Username = "admin",
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                FullName = "Demo Admin",
                Email = "admin.demo@gym.local",
                Phone = "0900000003",
                RoleId = adminRoleId.Value,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();
        }
    }
}
