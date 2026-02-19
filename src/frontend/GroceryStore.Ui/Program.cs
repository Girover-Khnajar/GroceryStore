using GroceryStore.Ui.Services;
using GroceryStore.Ui.Components;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents( );
//builder.Services.AddRazorComponents( )
//    .AddInteractiveServerComponents( ); // Interactive SSR (Server)
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection(ApiOptions.SectionName));
builder.Services.AddHttpClient<IApiClient,ApiClient>( );
var app = builder.Build( );

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment( ))
{
    app.UseExceptionHandler("/Error",createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts( );
}
app.UseStatusCodePagesWithReExecute("/not-found",createScopeForStatusCodePages: true);
app.UseHttpsRedirection( );

app.UseAntiforgery( );

app.MapStaticAssets( );
app.MapRazorComponents<App>( );
//app.MapRazorComponents<App>( )
//   .AddInteractiveServerRenderMode( );
app.Run( );
