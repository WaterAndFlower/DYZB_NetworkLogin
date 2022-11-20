using System;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace ZBCollegeNetWorkLogin;

public partial class MainPage : ContentPage {
    string getURL;
    public Tuple<string, string> GetLocalIP() {
        string addressIP = "\n", lastIP = "";
        foreach (var _ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
            if (_ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                Debug.WriteLine(_ip.ToString());
                lastIP = _ip.ToString();
                addressIP += _ip.ToString() + '\n';
            }
        }
        return new Tuple<string, string>(addressIP, lastIP);
    }
    void SetInitData() {
        var str = Preferences.Default.Get("Account", "");
        if (str == "") {
        } else {
            mAccount.Text = str;
            mSaveData.IsChecked = true;
        }
        str = Preferences.Default.Get("Password", "");
        if (str == "") {
        } else {
            mPassword.Text = str;
        }
    }
    public MainPage() {
        InitializeComponent();
        mIPShow.Text = "您目前IP地址（电脑）：" + GetLocalIP().Item1 + "可以试一试上面的IP地址";
        mIP.Text = GetLocalIP().Item2;
        SetInitData();
    }

    private async void mButton_Clicked(object sender, EventArgs e) {
        if (mSaveData.IsChecked) {
            Preferences.Default.Set("Account", mAccount.Text.ToString());
            Preferences.Default.Set("Password", mPassword.Text.ToString());
        } else {
            Preferences.Default.Remove("Account");
            Preferences.Default.Remove("Password");
        }
        bool select = await DisplayAlert("状态", $"正在登录网络：\n账户是：{mAccount.Text}\nIP目的地是：{mIP.Text}", "确定", "取消");
        if (select) {
            Login login = new Login("");
            getURL = $"http://172.31.254.2:801/eportal/?c=Portal&a=login&callback=dr1003&login_method=1&user_account=,b,{mAccount.Text}@cmcc&user_password={mPassword.Text}&wlan_user_ip={mIP.Text}&wlan_user_ipv6=&wlan_user_mac=000000000000";
            Debug.WriteLine("Get地址" + getURL);
            //开始Get请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(getURL);
            request.ContentType = "text/html, application/xhtml+xml, */*";
            request.Method = "GET";
            HttpWebResponse response = null;
            try {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch {
                await DisplayAlert("错误", "这个网络没有登录入口，检查电脑是否在NNUZB上", "确定");
                return;
            }
            Stream rs = response.GetResponseStream();
            StreamReader sr = new StreamReader(rs, Encoding.UTF8);
            var result = sr.ReadToEnd();
            sr.Close();
            rs.Close();
            Debug.WriteLine("请求的结果" + result);
            var msg = "";
            switch (result) {
                case "dr1003({\"result\":\"1\",\"msg\":\"\\u8ba4\\u8bc1\\u6210\\u529f\"})":
                msg = "登录成功！";
                break;
                case "dr1003({\"result\":\"0\",\"msg\":\"\",\"ret_code\":2})":
                msg = "您已经登录了。";
                break;
                case "dr1003({\"result\":\"0\",\"msg\":\"aW51c2UsIGxvZ2luIGFnYWluLCBwYyBPTG5vIDEgPj0gMQ==\",\"ret_code\":1})":
                msg = "其他设备已经登录，请再次尝试一次。";
                break;
                case "dr1003({\"result\":\"0\",\"msg\":\"bGRhcCBhdXRoIGVycm9y\",\"ret_code\":1})":
                msg = "密码似乎错误。";
                break;
                default:
                msg = "登录失败，有可能是欠费停机了。或者IP地址填入错误了，或者是账号输入错误了。";
                break;
            }
            await DisplayAlert("结果", msg, "确定");
        }
    }
}

