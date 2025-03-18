namespace SteamToys.Service.Sms;

public class SmsService : ISmsService
{
    private readonly ILogger _logger;
    private readonly IEnumerable<ISmsProvider> _smsProviders;
    private static readonly SemaphoreLocker _locker = new SemaphoreLocker();
    private bool _isLock;
    private ConcurrentQueue<DestroyPhoneNumberItem> _destroyQueue = new ConcurrentQueue<DestroyPhoneNumberItem>();

    public SmsService(ILoggerFactory loggerFactory, IEnumerable<ISmsProvider> smsProviders)
    {
        _logger = loggerFactory.CreateLogger<SmsService>();
        _smsProviders = smsProviders;
        StartDestroyPhoneNumberWork();
    }

    private void StartDestroyPhoneNumberWork()
    {
        // 启动线程处理
        _ = Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                DestroyPhoneNumberItem? item = null;
                try
                {
                   var success =  _destroyQueue.TryDequeue(out item);
                    if (!success)
                    {
                        await Task.Delay(3000);
                        continue;
                    }

                    if (item == null) continue;

                    // OnlineSim 需要等2min
                    if (item.Platform == SmsPlatform.OnlineSim)
                    {
                        // 校验时间是否符合销毁时间
                        var time = (DateTime.Now - item.GenerateTime).TotalSeconds;
                        if (time < 2 * 60)
                        {
                            _destroyQueue.Enqueue(item);
                            continue;
                        }
                    }

                    // FiveSim 需要等1min
                    if (item.Platform == SmsPlatform.FiveSim)
                    {
                        var time = (DateTime.Now - item.GenerateTime).TotalSeconds;
                        if (time < 1 * 60)
                        {
                            _destroyQueue.Enqueue(item);
                            continue;
                        }
                    }

                    // _logger.LogInformation($"开始进行销毁：{JsonConvert.SerializeObject(item)}");
                    var provider = GetSmsProvider(item.Platform);
                    var result = await provider.DestroyPhoneNumberAsync(item.Id);
                    if (!result && item.RetryCount <= 10)
                    {
                        item.RetryCount++;
                        _destroyQueue.Enqueue(item);
                    }

                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    if (item != null)
                    {
                        item.RetryCount++;
                        _destroyQueue.Enqueue(item);
                    }
                    _logger.LogError(ex,"销毁电话号码异常");
                }
            }
        });
    }

    public async Task<GetPhoneNumber> GetPhoneNumberAsync(SmsPlatform platform, string service, string country)
    {
        var provider = GetSmsProvider(platform);
        var result = new GetPhoneNumber();

        // 加锁，防止并发
        await _locker.LockAsync(async () =>
        {
            var resp = await WaitAndRetryForGetPhoneNumberAsync().ExecuteAsync(async () =>
            {
                var responese = await provider.GetPhoneNumberAsync(service, country);
                return responese;
            });

            resp.Adapt(result);
        });

        result.Platform = platform;
        result.GenerateTime = DateTime.Now;
        return result;
    }

    public async Task<GetPhoneNumberStatus> GetPhoneNumberStatusAsync(GetPhoneNumberStatusRequest request)
    {
        var provider = GetSmsProvider(request.Platform);
        var result = new GetPhoneNumberStatus();
        var resp = await WaitAndRetryForGetStatusAsync(request.WaitTime).ExecuteAsync(async () =>
        {
            var responese = await provider.GetPhoneNumberStatusAsync(request.Id);
            return responese;
        });
        resp.Adapt(result);
        return result;
    }

    public async Task DestroyPhoneNumberAsync(DestroyPhoneNumberItem destroy)
    {
        if (string.IsNullOrWhiteSpace(destroy.Id)) throw new ArgumentNullException(nameof(destroy));
        _destroyQueue.Enqueue(destroy);
        await Task.CompletedTask;
    }


    ISmsProvider GetSmsProvider(SmsPlatform platform)
    {
        var provider = _smsProviders.FirstOrDefault(x => x.Platform == platform);
        if (provider == null) throw new ArgumentException("can not find a match sms provider!");
        return provider;
    }

    AsyncRetryPolicy<GetPhoneNumberResponse> WaitAndRetryForGetPhoneNumberAsync(
    [CallerMemberName] string memberName = "",
    [CallerFilePath] string sourceFilePath = "",
    [CallerLineNumber] int sourceLineNumber = 0) => Policy
        .HandleResult<GetPhoneNumberResponse>(r => r.IsRetry || string.IsNullOrWhiteSpace(r.PhoneNumber))
        .WaitAndRetryAsync(3, retryTimes => TimeSpan.FromSeconds(1), (res, _, i, _) =>
    {
        _logger.LogError($"获取手机号码-第 {i} 次重试，response：{JsonConvert.SerializeObject(res)}，MemberName：{memberName}，FilePath：{sourceFilePath}，LineNumber：{sourceLineNumber}");
    });

    AsyncRetryPolicy<GetPhoneNumberStatusResponse> WaitAndRetryForGetStatusAsync(
    int waitTime,
    [CallerMemberName] string memberName = "",
    [CallerFilePath] string sourceFilePath = "",
    [CallerLineNumber] int sourceLineNumber = 0) => Policy
        .HandleResult<GetPhoneNumberStatusResponse>(r => r.IsRetry)
        .WaitAndRetryAsync(Util.GetIncreaseTimespans(waitTime), (res, sleep, i, _) =>
        {
            _logger.LogError($"获取手机号码状态-第 {i} 次重试 {sleep.TotalSeconds}s，response：{JsonConvert.SerializeObject(res)}，MemberName：{memberName}，FilePath：{sourceFilePath}，LineNumber：{sourceLineNumber}");
        });
}
