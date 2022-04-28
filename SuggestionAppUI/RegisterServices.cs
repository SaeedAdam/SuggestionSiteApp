namespace SuggestionAppUI;

public static class RegisterServices
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddMemoryCache();

        builder.Services.AddSingleton<IDbConnection, DbConnection>();
        builder.Services.AddSingleton<ICategoryData, MongoCategoryData>();
        builder.Services.AddTransient<IStatusData, MongoStatusData>();
        builder.Services.AddTransient<IUserData, MongoUserData>();
        builder.Services.AddTransient<ISuggestionData, MongoSuggestionData>();
    }
}