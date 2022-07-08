using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Identity.Web;
using EnercareDemo2.Service;

var builder = WebApplication.CreateBuilder(args);


/*builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();*/


builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(microsoftIdentityOptions =>
    {
        builder.Configuration
        .GetSection("AzureAd")
        .Bind(microsoftIdentityOptions);

        microsoftIdentityOptions.CallbackPath = "/auth/signin-oidc";
        microsoftIdentityOptions.ResponseType = OpenIdConnectResponseType.Code;
        microsoftIdentityOptions.ResponseMode = OpenIdConnectResponseMode.FormPost;
        microsoftIdentityOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        microsoftIdentityOptions.Scope.Add("https://EnercareTest.onmicrosoft.com/api/api.read");
        microsoftIdentityOptions.Scope.Add("offline_access");
        microsoftIdentityOptions.Scope.Add("openid");
        microsoftIdentityOptions.SaveTokens = true;

    }, cookieOption =>
    {
        cookieOption.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        cookieOption.Cookie.SameSite = SameSiteMode.Strict;
        cookieOption.Cookie.HttpOnly = true;
    })
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

string signInMagicLinkPolicy = builder.Configuration
    .GetSection("AzureAdB2CConfiguration")["MagicLinksPolicyId"];

string signUpMagicLinkPolicy = builder.Configuration
    .GetSection("AzureAdB2CConfiguration")["InvitationPolicyId"];

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(signInMagicLinkPolicy, GetOpenIdOptions(signInMagicLinkPolicy))
    .AddOpenIdConnect(signUpMagicLinkPolicy, GetOpenIdOptions(signUpMagicLinkPolicy));

builder.Services.AddScoped<IdTokenHintService>();
builder.Services.AddScoped<MailDeliveryService>();
builder.Services.AddScoped<TokenSigningCertificateService>();
builder.Services.AddScoped<SignUpMagicLinkService>();
builder.Services.AddScoped<UserMagicLinkInvitationHandler>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html"); ;

app.Run();


/*
 * why does the cookie is chunk in 3 ?
 https://hajekj.net/2017/03/20/cookie-size-and-cookie-authentication-in-asp-net-core/
 */
Action<OpenIdConnectOptions> GetOpenIdOptions(string policy)
   => options =>
   {
       var clientId = builder.Configuration
           .GetSection("AzureAdB2CConfiguration")["ClientId"];

       var b2cDomain = builder.Configuration
           .GetSection("AzureAdB2CConfiguration")["Instance"];

       var domain = builder.Configuration
           .GetSection("AzureAdB2CConfiguration")["Domain"];

       var magicLinkPolicy = builder.Configuration
           .GetSection("AzureAdB2CConfiguration")["MagicLinksPolicyId"];

       var invite = builder.Configuration
           .GetSection("AzureAdB2CConfiguration")["InvitationPolicyId"];


       options.Scope.Add("https://EnercareTest.onmicrosoft.com/api/api.read");
       options.Scope.Add("offline_access");
       options.Scope.Add("openid");
       options.ClientId = clientId;
       options.SignedOutRedirectUri = "/";
       options.ResponseType = OpenIdConnectResponseType.CodeIdTokenToken;
       options.ClientSecret = "pLn8Q~e.v5AsCwIZT4btt1f~uCdLq11C56NH5cTx";
       options.SignedOutCallbackPath = "/signout/" + policy;
       options.SaveTokens = true;

       options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

       if (policy == magicLinkPolicy)
       {
           options.MetadataAddress = $"{b2cDomain}/{domain}/{magicLinkPolicy}/v2.0/" +
               $".well-known/openid-configuration";

           options.CallbackPath = "/auth/signin-oidc-link"; // since we are redirecting to an SPA make sur that the url contain a path that will be redirect to a controller even if the action do not exist.
       }
       else if (policy == invite)
       {
           options.MetadataAddress = $"{b2cDomain}/{domain}/{invite}/v2.0/" +
               $".well-known/openid-configuration";

           options.CallbackPath = "/auth/signup-oidc-invite"; // since we are redirecting to an SPA make sur that the url contain a path that will be redirect to a controller even if the action do not exist.
       }


       options.Events = new OpenIdConnectEvents
       {
           OnRedirectToIdentityProvider = context =>
           {
               if (context.Properties.Items.ContainsKey("id_token_hint"))
               {
                   var tokenHint = context.Properties.Items["id_token_hint"];
                   context.ProtocolMessage.IdTokenHint = tokenHint;
               }

               return Task.FromResult(0);
           },
       };
   };


