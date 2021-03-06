﻿
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using SEOAutomation.Base.Service.GoogleAdword;
using SEOAutomation.Base.Models.Common;
using SEOAutomation.GoogleAdword.Services;
using Gecko;


namespace SEOAutomation.Winform
{
    public partial class BrowerGecko : Form
    {
        private IGoogleAdwordService _googleAdwordService;
        public bool isClick = false;
        Timer clickLinkTimer = new Timer();
        int intInterval = 20;
        int clickLimit = 0;
        List<AdwordConfig> lstAdwordConfigs = null;
        private AdwordConfig objAdwordConfig = null;
        private int numberURL = 0;
        int countClickOnSite = 0;
        private string googleURL = "https://www.google.com/search?q=";
        private bool isFindingURL = false;
        private int page = 0;
        string IPPublic = "";
        public BrowerGecko()
        {
            InitializeComponent();
            _googleAdwordService = new GoogleAdwordService();
            //Xpcom.Initialize(@"E:\Sample\Gecko33\xulrunner-sdk\bin");
            Xpcom.Initialize(@"F:\Sample\Gecko33\xulrunner-sdk\bin");
            nsIBrowserHistory historyMan = Xpcom.GetService<nsIBrowserHistory>(Gecko.Contracts.NavHistoryService);
            historyMan = Xpcom.QueryInterface<nsIBrowserHistory>(historyMan);
            historyMan.RemoveAllPages();
            // https://developer.mozilla.org/en-US/docs/Mozilla/Tech/XPCOM/Reference/Interface/imgICache
            nsICookieManager CookieMan;
            CookieMan = Xpcom.GetService<nsICookieManager>("@mozilla.org/cookiemanager;1");
            CookieMan = Xpcom.QueryInterface<nsICookieManager>(CookieMan);
            CookieMan.RemoveAll();
            IPPublic = getPublicIP();
            WriteLog(IPPublic.ToString());
        }
        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }
        private void clickLinkTimer_Tick(object sender, EventArgs e)
        {
            //clickLinkTimer.Stop();
            Timer timer = (Timer)sender;
            timer.Stop();


            Random rd = new Random();
            if (countClickOnSite <= clickLimit)

            {

                GeckoElementCollection cv = geckoBrower.Document.GetElementsByTagName("a");


                string[] arrInterval = objAdwordConfig.IntervalClick.Split(',');
                int rdFrom = -1;
                int rdTo = -1;
                if (arrInterval.Length > 0)
                {
                    rdFrom = int.Parse(arrInterval[0]);
                    rdTo = int.Parse(arrInterval[1]);
                }
                if (cv != null && cv.Length > 0)
                {
                    int randomClick = rd.Next(0, cv.Length - 1);

                    if (!StripHTML(cv[randomClick].InnerHtml).Equals("Xem hướng nhà theo tuổi") &&
                        !String.IsNullOrEmpty(StripHTML(cv[randomClick].InnerHtml)))
                    {
                        string strHref = cv[randomClick].GetAttribute("href");
                        WriteLog(strHref);
                        cv[randomClick].Click();
                        // MessageBox.Show(countClickOnSite.ToString());
                        countClickOnSite = countClickOnSite + 1;


                    }


                }
                else
                {
                    //MessageBox.Show("href : " + geckoBrower.Url.ToString());
                    WriteLog("Khong co the a : " + geckoBrower.Url.ToString());
                }
                timer.Interval = (1 * rd.Next(rdFrom, rdTo) * 1000);
                timer.Start();
            }
            else
            {
                // MessageBox.Show("Số lần click on site " + countClickOnSite.ToString());
                viewNextURL();
                ViewLink();

            }

        }
        private void Brower_Load(object sender, EventArgs e)
        {
            Random random = new Random();
            lstAdwordConfigs = _googleAdwordService.GetAdwordConfigs().OrderBy(arg => random.Next(int.MaxValue)).ToList();
            ViewLink();
        }

        private void ViewLink()
        {
            if (numberURL <= lstAdwordConfigs.Count - 1)
            {

                if (lstAdwordConfigs != null && lstAdwordConfigs.Count > 0)
                {
                    objAdwordConfig = lstAdwordConfigs[numberURL];
                }


                string[] arrKeyWord = objAdwordConfig.KeyWord.Split(',');
                string[] arrclickLimit = objAdwordConfig.LinkQuantityClick.Split(',');
                if (arrclickLimit.Length > 0)
                {
                    Random rdClickLimit = new Random();
                    clickLimit = rdClickLimit.Next(int.Parse(arrclickLimit[0]), int.Parse(arrclickLimit[1]));
                    //MessageBox.Show("số lần click tối đa : " + clickLimit.ToString());
                }
                if (arrKeyWord.Length > 0)
                {
                    int i = 0;
                    string strQuery = HttpUtility.UrlEncode(arrKeyWord[i]);
                    // geckoBrower.Dispose();
                    geckoBrower.Navigate(googleURL + strQuery);



                }
                
            }
            else
            {
                clickLinkTimer.Stop();
                clickLinkTimer.Enabled = false;
                clickLinkTimer.Dispose();
                MessageBox.Show("Done Number URL : " + numberURL);


                //Loop khi reset IP

                System.Threading.Thread.Sleep(60000);
                IPPublic = getPublicIP();
                WriteLog(IPPublic.ToString());
                if (!IPPublic.Equals(IPPublic.ToString()))
                {
                   
                    MessageBox.Show(IPPublic);
                   
                }

                numberURL = 0;
                nsIBrowserHistory historyMan = Xpcom.GetService<nsIBrowserHistory>(Gecko.Contracts.NavHistoryService);
                historyMan = Xpcom.QueryInterface<nsIBrowserHistory>(historyMan);
                historyMan.RemoveAllPages();
                //clear cache

                nsICookieManager CookieMan;
                CookieMan = Xpcom.GetService<nsICookieManager>("@mozilla.org/cookiemanager;1");
                CookieMan = Xpcom.QueryInterface<nsICookieManager>(CookieMan);
                CookieMan.RemoveAll();
                loop_ViewLinkWithIP();

            }
        }

        private void geckoBrower_DocumentCompleted(object sender, Gecko.Events.GeckoDocumentCompletedEventArgs e)
        {

            if (!isFindingURL)
            {
                if (geckoBrower.Url.Host.Equals("www.google.com") || geckoBrower.Url.Host.Equals("www.google.co.uk") || geckoBrower.Url.Host.Equals("www.google.co.jp"))
                {


                    GeckoElementCollection cv = geckoBrower.Document.GetElementsByTagName("a");
                    foreach (var item in cv)
                    {
                        //Doanh nghiệp xanh - Diễn đàn cộng đồng doanh nghiệp Việt Nam
                        if (StripHTML(item.InnerHtml).Equals(objAdwordConfig.TextLink))
                        {
                            isFindingURL = true;
                            System.Threading.Thread.Sleep(20000);
                            WriteLog(StripHTML(item.InnerHtml));
                            item.Click();
                            //Tim thay URL can view thi goi timer de click tren trang 
                            clickLinkTimer = new Timer();
                            clickLinkTimer.Interval = (1 * 20 * 1000);
                            clickLinkTimer.Tick += new EventHandler(clickLinkTimer_Tick);
                            clickLinkTimer.Start();
                        }

                    }
                    if (cv.Length > 0 && !isFindingURL)
                    {
                        page = page + 1;
                        string googleUri = geckoBrower.Url.ToString();
                        int paraStart = 10 * page;
                        //System.Threading.Thread.Sleep(10000);
                        geckoBrower.Navigate(googleUri + "&start=" + paraStart.ToString());
                    }
                }
            }
        }

        private void viewNextURL()
        {
            numberURL = numberURL + 1;
            countClickOnSite = 0;
            isFindingURL = false;
            page = 0;
            clickLimit = 0;
        }
        private void loop_ViewLinkWithIP()
        {
            numberURL = 0;
            countClickOnSite = 0;
            isFindingURL = false;
            page = 0;
            clickLimit = 0;
            ViewLink();
        }
        private string getPublicIP()
        {
            string url = "http://checkip.dyndns.org";
            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            string response = sr.ReadToEnd().Trim();
            string[] a = response.Split(':');
            string a2 = a[1].Substring(1);
            string[] a3 = a2.Split('<');
            string a4 = a3[0];
            return a4;
        }
        private void WriteLog(string strLog)
        {
            File.AppendAllText(@"D:\Project\SEOAutomation\Log\LogViewLink.txt", strLog + Environment.NewLine);
        }


    }
}
