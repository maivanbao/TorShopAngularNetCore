using AngularNetCoreIMS.WebApi.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IO;

namespace AngularNetCoreIMS.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
			//Allow domain header
			//services.AddCors(options =>
			//{
			//	options.AddPolicy("AngularNetCoreIMSClient",
			//		builder =>
			//		{
			//			builder.WithOrigins("http://localhost:4200")
			//				   .AllowAnyHeader()
			//				   .AllowCredentials()
			//				   .AllowAnyMethod();
			//		});
			//});
			services.AddCors();

			// Enable the use of an  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]   attribute on methods and classes to protect.
			services.AddAuthentication().AddJwtBearer(cfg =>
              {
                  cfg.RequireHttpsMetadata = false;
                  cfg.SaveToken = true;

                  cfg.TokenValidationParameters = new TokenValidationParameters()
                  {
                      IssuerSigningKey = TokenAuthOption.Key,
                      ValidAudience = TokenAuthOption.Audience,
                      ValidIssuer = TokenAuthOption.Issuer,
                    // When receiving a token, check that we've signed it.
                    ValidateIssuerSigningKey = true,
                    // When receiving a token, check that it is still valid.
                    ValidateLifetime = true,
                    // This defines the maximum allowable clock skew - i.e. provides a tolerance on the token expiry time
                    // when validating the lifetime. As we're creating the tokens locally and validating them on the same
                    // machines which should have synchronised time, this can be set to zero. and default value will be 5minutes
                    ClockSkew = TimeSpan.FromMinutes(0)
                  };
              });
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            #region logger

            //     loggerFactory.AddSerilog();
            //    loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //     loggerFactory.AddDebug();

            #endregion logger

            #region static files

            app.UseStaticFiles();

            #endregion static files

				#region Handle Exception

				app.UseExceptionHandler(appBuilder =>
				{
					appBuilder.Use(async (context, next) =>
					{
						var error = context.Features[typeof(IExceptionHandlerFeature)] as IExceptionHandlerFeature;

						//when authorization has failed, should retrun a json message to client
						if (error != null && error.Error is SecurityTokenExpiredException)
						{
							context.Response.StatusCode = 401;
							context.Response.ContentType = "application/json";

							await context.Response.WriteAsync(JsonConvert.SerializeObject(new RequestResult
							{
								State = RequestState.NotAuth,
								Msg = "token expired"
							}));
						}
						//when orther error, retrun a error message json to client
						else if (error != null && error.Error != null)
						{
							context.Response.StatusCode = 500;
							context.Response.ContentType = "application/json";
							await context.Response.WriteAsync(JsonConvert.SerializeObject(new RequestResult
							{
								State = RequestState.Failed,
								Msg = error.Error.Message
							}));
						}
						//when no error, do next.
						else await next();
					});
				});

				#endregion Handle Exception

			//app.UseCors("AngularNetCoreIMSClient");
			app.UseCors(builder => builder
				.WithOrigins("http://localhost:4200")
				.AllowAnyMethod()
				.AllowAnyHeader()
				.AllowCredentials());

			app.UseAuthentication();

            #region route

            app.UseMvc(routes =>
            {
                routes.MapSpaFallbackRoute("spa-fallback", new { controller = "home", action = "index" });
            });

			#endregion route


		}
    }
}