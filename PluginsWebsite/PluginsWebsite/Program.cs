using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using PluginsWebsite.Components;
using PluginsWebsite.Components.Account;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using MudExtensions.Services;
using PluginsWebsite.Infrastracture.Data;
using PluginsWebsite.Client;
using PluginsWebsite.Core.Interfaces;
using PluginsWebsite.Infrastracture.Services;
using PluginsWebsite.Core.Seeders;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;

namespace PluginsWebsite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddMudServices();
            builder.Services.AddMudExtensions();
            builder.Services.AddHeadElementHelper();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();

            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Plugin API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            builder.Services.AddHttpClient();

            builder.Services.AddScoped<IPluginService, PluginService>();

            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddOAuth("GitHub", options =>
                {
                    options.ClientId = builder.Configuration["GitHub:ClientId"];
                    options.ClientSecret = builder.Configuration["GitHub:ClientSecret"];
                    options.CallbackPath = new PathString("/github-oauth");
                    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                    options.UserInformationEndpoint = "https://api.github.com/user";
                    options.Scope.Add("user:email");

                    options.SaveTokens = true;

                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");

                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            context.Properties.StoreTokens(context.Properties.GetTokens().ToList());

                            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);

                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);

                            response.EnsureSuccessStatusCode();

                            string content = await response.Content.ReadAsStringAsync();

                            var json = JsonDocument.Parse(content);

                            Console.WriteLine(json);

                            context.RunClaimActions(json.RootElement);

                        },
                    };
                }).AddIdentityCookies();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContextFactory<WebsiteContext>(options =>

            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options2 =>
            {
                options2.MigrationsAssembly("PluginsWebsite.Infrastracture");
            }));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<User>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<WebsiteContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            var app = builder.Build();

            using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<WebsiteContext>();

                context.Database.Migrate();

                foreach (var category in CategorySeeder.Categories)
                {
                    if (context.Categories.FirstOrDefault(x => x.Name == category.Name) != null)
                        continue;

                    context.Categories.Add(new Category()
                    {
                        Name = category.Name,
                        DisplayName = category.Name,
                    });
                }

                foreach (var loader in LoaderSeeder.Loaders)
                {
                    if (context.Loaders.FirstOrDefault(x => x.DisplayName == loader.Name) != null)
                        continue;

                    var group = context.Groups.FirstOrDefault(x => x.Name == loader.Name);

                    if (group == null)
                    {
                        group = new Group()
                        {
                            Name = loader.Name,
                            DisplayName = loader.Name,
                        };
                        context.Groups.Add(group);
                        context.SaveChanges();
                    }

                    var author = context.Authors.FirstOrDefault(x => x.GroupId == group.Id);

                    if (author == null)
                    {
                        author = new Author()
                        {
                            Type = Core.Enums.AuthorType.Group,
                            GroupId = group.Id,
                        };
                        context.Authors.Add(author);
                        context.SaveChanges();
                    }


                    context.Loaders.Add(new Loader()
                    {
                        Name = loader.Name,
                        DisplayName = loader.Name,
                        AuthorId = author.Id,
                    });
                }

                context.SaveChanges();            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                c.OAuthUseBasicAuthenticationWithAccessCodeGrant();

            });

            app.UseHttpsRedirection();

            app.UseHeadElementServerPrerendering();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(UserInfo).Assembly);

            app.MapControllers();

            app.Run();
        }
    }
}
