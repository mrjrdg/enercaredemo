using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Identity.Web;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);


string signInMagicLinkPolicy = builder.Configuration
    .GetSection("AzureAdB2CConfiguration")["MagicLinksPolicyId"];

string signUpMagicLinkPolicy = builder.Configuration
    .GetSection("AzureAdB2CConfiguration")["InvitationPolicyId"];

builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(o =>
{
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Strict;
    o.Cookie.HttpOnly = true;
})
.AddOpenIdConnect(signInMagicLinkPolicy, GetOpenIdOptions(signInMagicLinkPolicy))
.AddOpenIdConnect(signUpMagicLinkPolicy, GetOpenIdOptions(signUpMagicLinkPolicy));

builder.Services.AddHttpClient();

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

app.UseAuthorization();
app.UseAuthentication();

app.MapFallbackToController("Index", "FallBack");

app.Run();


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


       options.Scope.Clear();
       // The OFFLINE_ACCESS scope is to tell the auth server (B2C) that we also want a REFRESH_TOKEN
       options.Scope.Add("https://EnercareTest.onmicrosoft.com/api/api.read");
       options.Scope.Add("offline_access");

       options.ClientId = clientId;
       options.SignedOutRedirectUri = "/";
       options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
       options.ResponseMode = OpenIdConnectResponseMode.FormPost;
       options.ClientSecret = "pLn8Q~e.v5AsCwIZT4btt1f~uCdLq11C56NH5cTx";
       options.SignedOutCallbackPath = "/signout/" + policy;
       options.SaveTokens = true;
       options.ClaimsIssuer = "B2C";

       //The SIGNIN SCHEME must be COOKIES for the sign in
       options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

       if (policy == magicLinkPolicy)
       {
           options.MetadataAddress = $"{b2cDomain}/{domain}/{magicLinkPolicy}/v2.0/" +
               $".well-known/openid-configuration";

           options.CallbackPath = new PathString("/signin-oidc-link");
       }
       else if (policy == invite)
       {
           options.MetadataAddress = $"{b2cDomain}/{domain}/{invite}/v2.0/" +
               $".well-known/openid-configuration";

           options.CallbackPath = new PathString("/signup-oidc-invite");
       }


       options.Events = new OpenIdConnectEvents
       {

           // handle the logout redirection
           OnRedirectToIdentityProviderForSignOut = (context) =>
           {
               var logoutUri = $"https://{builder.Configuration["b2c:Domain"]}/v2/logout?client_id={builder.Configuration["b2c:ClientId"]}";

               var postLogoutUri = context.Properties.RedirectUri;
               if (!string.IsNullOrEmpty(postLogoutUri))
               {
                   if (postLogoutUri.StartsWith("/"))
                   {
                       // transform to absolute
                       var request = context.Request;
                       postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                   }
                   logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
               }
               context.Response.Redirect(logoutUri);
               context.HandleResponse();

               return Task.CompletedTask;
           },

           OnAccessDenied = context =>
           {
               // to redirect the user to the homepage if the access is denied
               context.HandleResponse();
               context.Response.Redirect("/");
               return Task.FromResult(0);
           },

           OnRedirectToIdentityProvider = context =>
           {
               //context.ProtocolMessage.SetParameter("audience", builder.Configuration["b2c:ApiAudience"]);
               if (context.Properties.Items.ContainsKey("id_token_hint"))
               {
                   // AZURE B2C will trade the ID_TOKEN_HINT for the real ID_TOKEN
                   var tokenHint = context.Properties.Items["id_token_hint"];
                   context.ProtocolMessage.IdTokenHint = tokenHint;
               }

               return Task.FromResult(0);
           },


           // Find the tokens in the PROTOCOL_MESSAGE property of the CONTEXT
           OnAuthorizationCodeReceived = context =>
           {
               return Task.FromResult(0);
           },


           // This is in this event that you can find the expireAt
           OnTicketReceived = context =>
           {
               return Task.FromResult(0);
           },
       };
   };

