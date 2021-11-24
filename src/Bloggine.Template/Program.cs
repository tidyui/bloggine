using Bloggine;

var app = Blog.Create(args, options =>
{
    // options.CacheMaxAge = 86400;
    // options.DataAssetPath = "Uploads";
    // options.DataPath = "Data";
    // options.Headline = "Just another markdown blog";
    // options.Title = "Bloggine";
    // options.Theme = "Default";
    options.UseFileSystemWatcher = true;
});

app.Run();