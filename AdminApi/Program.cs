global using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<IConfiguration>(builder.Configuration);

var appSettings = new Core.Models.AdminAppSettings();

var config = builder.Configuration.GetSection("Config");
config.Bind(appSettings);
builder.Services.Configure<Core.Models.AdminAppSettings>(config); //注入 Core.Models.AdminAppSettings

var commonConfig = builder.Configuration.GetSection("Config:Common");
commonConfig.Bind(commonConfig);
builder.Services.Configure<Core.Models.AdminAppSettings.CommonClass>(commonConfig); //注入 Core.Models.AdminAppSettings.CommonClass

Core.Helpers.AuthHelper.SetAuthCookieName("ibysjd");

Core.Constants.Env.SetAdminCommonSettings(appSettings.Common);
Su.Wu.SetLogRoot(appSettings.Common.LogDirectory!);

// 加載 secrets.json
builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);
var secretSettings = new Core.Models.SecretSettings();
// 綁定整個 Config 區段
config.Bind(secretSettings);
builder.Services.Configure<Core.Models.SecretSettings>(builder.Configuration.GetSection("Config"));
//注入 DBC 設定
string pgDbc = secretSettings.Secrets.ConnectionStrings!.DefaultConnectionString!;
Core.Ef.CBCTContext.SetDbc(pgDbc);
Su.PgSql.AddDbc(Core.Constants.DbIds.CBCT, pgDbc);
Su.PgSql.DefaultDbId = Core.Constants.DbIds.CBCT;
Su.PgSqlCache.AddMonitoredDb(Su.PgSql.DefaultDbId);
Su.PgSqlCache.StartUpdateTableCache();
System.Threading.Thread.Sleep(200); //等待建立暫存

//注入 AWS SES 的設定
Core.Helpers.EmailHelper._senderInfo = secretSettings.SenderInfo;
Core.Helpers.EmailHelper._serverInfo = secretSettings.AWS_SES;

//允許這些網址 Cross.(Develope mode 開啟)
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://*.bike.idv.tw",
                              "https://*.bike.idv.tw",
                              "http://localhost",
                              "https://localhost",
                              "http://192.168.101.102",
                              "https://192.168.101.102",
                              "http://localhost:5100",
                              "https://localhost:5100")
                            .SetIsOriginAllowedToAllowWildcardSubdomains()
                            .AllowCredentials() // Cookie
                            .AllowAnyMethod() // DELETE method
                            .AllowAnyHeader();
                      });
});

//這裡會注入一個 IHttpContextAccessor 
builder.Services.AddHttpContextAccessor();

builder.Services
    .AddControllers(options =>
    {
        //授權檢查
        options.Filters.Add(typeof(AdminApi.CheckAuthorizationFilter));
    })
    .AddNewtonsoftJson(options =>
    {
        // 使用自製的 CamelCaseContractResolver
        options.SerializerSettings.ContractResolver = new Su.CamelCaseContractResolver();

        //處理循環引用
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// 註冊 Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CBCT Admin API", Version = "v1" });

    //多個 Project, 每一個 xmldocument 都要載入
    List<string> xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
    foreach (string fileName in xmlFiles)
    {
        string xmlFilePath = Path.Combine(AppContext.BaseDirectory, fileName);
        if (File.Exists(xmlFilePath))
        {
            options.IncludeXmlComments(xmlFilePath, includeControllerXmlComments: true);
        }
    }

    //防止 SchemaId 的錯誤 
    options.CustomSchemaIds(type => type.ToString());
});

//注入 DbContext
builder.Services.AddDbContext<Core.Ef.CBCTContext>(options => options.UseNpgsql(secretSettings.Secrets.ConnectionStrings.DefaultConnectionString!));
builder.Services.AddMvcCore().AddApiExplorer();
builder.Services.AddDistributedMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseHttpsRedirection();
}

InitSu(app, app.Environment);

app.UseStaticFiles();

//配置上傳目錄的静態文件服務
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.GetFullPath(appSettings.Common.UploadDirectory!)),
    RequestPath = "/upload",
    OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 60 * 60 * 24 * 365; //一年
        ctx.Context.Response.Headers[HeaderNames.CacheControl] =
            "public,max-age=" + durationInSeconds;
    }
});

// 系統產生的文件供後台下載必須經過授權，所以不要直接掛上 download 路徑

////配置下载目錄的静態文件服務
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(Path.GetFullPath(appSettings.Common.DownloadDirectory!)),
//    RequestPath = "/download",
//    OnPrepareResponse = ctx =>
//    {
//        const int durationInSeconds = 60 * 60 * 24 * 365; //一年
//        ctx.Context.Response.Headers[HeaderNames.CacheControl] =
//            "public,max-age=" + durationInSeconds;
//    }
//});

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(MyAllowSpecificOrigins);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//註冊 middleware
app.UseMiddleware(typeof(Core.Middlewares.ErrorHandleMiddleware)); // 錯誤處理
app.UseMiddleware(typeof(Core.Middlewares.FileHandingMiddleware)); // 上傳檔案檢查

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//});
app.MapControllers();

app.Run();

void InitSu(IApplicationBuilder app, IWebHostEnvironment env)
{
    Su.CurrentContext.Configure(app);//讓 Su.CurrentContext 可以使用.
    Su.Mail.IsSendWithGmail = true;

    //不可更改，更改後會造成舊的 cookie 無法解密
    Su.Encryption.SetCookieEncryptKeyAndIv(secretSettings.Secrets.CookieKey, secretSettings.Secrets.CookieIv);
}
