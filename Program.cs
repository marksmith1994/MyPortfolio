using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MyPortfolio.Services;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient<GitHubService>();
builder.Services.AddHttpClient<StravaService>();
builder.Services.AddHttpClient<NasaService>();
builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddHttpClient<SpaceXService>();
builder.Services.AddScoped<GitHubService>();
builder.Services.AddScoped<StravaService>();
builder.Services.AddScoped<NasaService>();
builder.Services.AddScoped<WeatherService>();
builder.Services.AddScoped<SpaceXService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
