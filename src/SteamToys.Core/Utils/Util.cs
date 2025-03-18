using Serilog;

namespace SteamToys.Contact
{
    public static class Util
    {
        public static long GetSystemUnixTime()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            int hexLen = hex.Length;
            byte[] ret = new byte[hexLen / 2];
            for (int i = 0; i < hexLen; i += 2)
            {
                ret[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return ret;
        }

        public static string GenerateDeviceID() => "android:" + Guid.NewGuid().ToString();

        public static string FilterPhoneNumber(string phoneNumber, string itCode)
        {
            var format = phoneNumber.Replace("-", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty).Replace("+", string.Empty);
            format = format.Insert(itCode.Length, " ");
            return $"+{format}";
        }

        /// <summary>
        /// 写入maFile授权文件
        /// </summary>
        /// <param name="content"></param>
        /// <param name="steamId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task SaveToMaFileAsync(object content, ulong steamId, string path)
        {
            var accountContent = JsonConvert.SerializeObject(content);

            path = Path.Combine(path, "maFiles");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var filePath = Path.Combine(path, $"{steamId}.maFile");
            await File.WriteAllTextAsync(filePath, accountContent);
        }

        /// <summary>
        /// 写入绑定成功的Steam账号
        /// </summary>
        /// <param name="message"></param>
        public static async Task SaveToSucceedLogFileAsync(string message) => await File.AppendAllTextAsync(Path.Combine(Environment.CurrentDirectory, "succeed.txt"), message + "\n");

        public static string GetMailboxHost(string email, string prefix)
        {
            var sp = email.Split('@');
            if (sp.Length < 2 || string.IsNullOrWhiteSpace(sp[1])) throw new Exception("邮箱格式错误");
            if (!string.IsNullOrWhiteSpace(prefix)) prefix += ".";
            return $"{prefix}{sp[1]}";
        }

        /// <summary>
        /// 扩展方法，获得枚举的Description
        /// </summary>
        /// <param name="value">枚举值</param>
        /// <param name="nameInstead">当枚举值没有定义DescriptionAttribute，是否使用枚举名代替，默认是使用</param>
        /// <returns>枚举的Description</returns>
        public static string GetDescription(this Enum value, Boolean nameInstead = true)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name == null)
            {
                return null;
            }

            FieldInfo field = type.GetField(name);
            DescriptionAttribute attribute = System.Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            if (attribute == null && nameInstead == true)
            {
                return name;
            }
            return attribute?.Description;
        }

        /// <summary>
        /// 逐行读取，包含空行
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<string> SplitByLine(this string text)
        {
            List<string> lines = new List<string>();
            byte[] array = Encoding.UTF8.GetBytes(text);
            using (MemoryStream stream = new MemoryStream(array))
            {
                using (var sr = new StreamReader(stream))
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        lines.Add(line);
                        line = sr.ReadLine();
                    };
                }

            }
            return lines;
        }

        public static List<string> ReadSucceedDatas(string filePath)
        {
            List<string> lines = new List<string>();
            try
            {
                if (File.Exists(filePath))
                {
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            lines.Add(line);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while reading the file: " + e.Message);
            }

            return lines;
        }


        public static List<TimeSpan> GetIncreaseTimespans(int time)
        {
            var ts = new List<TimeSpan>();
            var sum = 0;
            for (int i = 1; i <= time; i++)
            {
                sum += i;
                if (sum >= 30) sum = 30;
                ts.Add(TimeSpan.FromSeconds(sum));
            }

            return ts;
        }

        public static string GetCookieValue(this CookieContainer cookieContainer, Uri uri, string name)
        {
            ArgumentNullException.ThrowIfNull(cookieContainer);
            ArgumentNullException.ThrowIfNull(uri);

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            CookieCollection cookies = cookieContainer.GetCookies(uri);

            return cookies.Count > 0 ? cookies.FirstOrDefault(cookie => cookie.Name == name)?.Value : null;
        }

        public static string ToBase64String(this IExtensible req)
        {
            ArgumentNullException.ThrowIfNull(req);
            var base64 = string.Empty;
            using (var st = new MemoryStream())
            {
                Serializer.Serialize(st, req);
                base64 = Convert.ToBase64String(st.ToArray());
            }

            return base64;
        }
    }
}
