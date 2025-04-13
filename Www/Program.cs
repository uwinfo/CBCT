using Core.Helpers;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Su;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<IConfiguration>(builder.Configuration);

var appSettings = new Core.Models.WwwSettings();

var config = builder.Configuration.GetSection("Config");
config.Bind(appSettings);
builder.Services.Configure<Core.Models.WwwSettings>(config); //注入 Core.Models.WwwSettings

var commonConfig = builder.Configuration.GetSection("Config:Common");
commonConfig.Bind(commonConfig);
builder.Services.Configure<Core.Models.WwwSettings.CommonClass>(commonConfig); //注入 Core.Models.WwwSettings.CommonClass

//Core.Helpers.AuthHelper.SetAuthCookieName("sdhroe");

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
//Su.PgSqlCache.AddMonitoredDb(Su.PgSql.DefaultDbId);
//Su.PgSqlCache.StartUpdateTableCache();
System.Threading.Thread.Sleep(200); //等待建立暫存

Su.Wu.InitialSetting("sdhroe", appSettings.Common.LogDirectory!);

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
                              "http://localhost:7200",
                              "https://localhost:7200")
                            .SetIsOriginAllowedToAllowWildcardSubdomains()
                            .AllowCredentials() // Cookie
                            .AllowAnyMethod() // DELETE method
                            .AllowAnyHeader();
                      });
});

//這裡會注入一個 IHttpContextAccessor 
builder.Services.AddScoped<AuthHelper>();
builder.Services.AddScoped<HttpContextWrapper>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HttpContextHelper>();

builder.Services
    .AddControllers(options =>
    {
        //授權檢查
        //options.Filters.Add(typeof(CheckAuthorizationFilter));  //前台不需做授權檢查
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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CBCT Web API", Version = "v1" });

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

//不要注入 DbContext, 在 Controller 不可使用 DbContext，以保証必需使用 helper 才能存取資料庫
//builder.Services.AddDbContext<Core.Ef.CBCTContext>(options => options.UseNpgsql(secretSettings.Secrets.ConnectionStrings.DefaultConnectionString!));

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

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(MyAllowSpecificOrigins);

app.UseRouting();

//註冊 middleware
app.UseMiddleware(typeof(Core.Middlewares.ErrorHandleMiddleware)); // 錯誤處理
app.UseMiddleware(typeof(Core.Middlewares.FileHandingMiddleware)); // 上傳檔案檢查
app.UseMiddleware(typeof(Core.Middlewares.ComputerUidMiddleware)); // 新增 ComputerId
app.UseMiddleware(typeof(Core.Middlewares.SessionMiddleware)); // 新增 SessionMiddleware

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//});
app.MapControllers();

app.Run();

void InitSu(IApplicationBuilder app, IWebHostEnvironment env)
{
    //Su.CurrentContext.Configure(app);//讓 Su.CurrentContext 可以使用.
    Su.Mail.IsSendWithGmail = true;

    //不可更改，更改後會造成舊的 cookie 無法解密
    Su.Encryption.SetCookieEncryptKeyAndIv(secretSettings.Secrets.CookieKey, secretSettings.Secrets.CookieIv);
}