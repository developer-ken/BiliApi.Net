using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace BiliApi.Auth
{
    internal class FakeBrowser
    {
        IPlaywright pw;
        IBrowser browser;
        IBrowserContext context;

        public FakeBrowser()
        {
        }

        public async Task InitAsync()
        {
            if (pw == null)
                pw = await Playwright.CreateAsync();
            if (browser == null)
                browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true
                });
            if (context == null)
                context = await browser.NewContextAsync();
        }

        public async Task<IReadOnlyList<BrowserContextCookiesResult>> GetContextCookies(string url)
        {
            return await context.CookiesAsync(url);
        }

        public async Task<CookieCollection> GetContextCookies()
        {
            var cookies =  await context.CookiesAsync();
            CookieCollection result = new CookieCollection();
            foreach(BrowserContextCookiesResult cc in cookies)
            {
                System.Net.Cookie ccc = new System.Net.Cookie();
                ccc.Domain = cc.Domain;
                ccc.Name = cc.Name;
                ccc.Path = cc.Path;
                ccc.Value = cc.Value;
                ccc.HttpOnly = cc.HttpOnly;
                result.Add(ccc);
            }
            return result;
        }

        public async Task AddContextCookies(CookieCollection cookies)
        {
            List<Microsoft.Playwright.Cookie> cookieList = new List<Microsoft.Playwright.Cookie>();
            foreach(System.Net.Cookie c in cookies)
            {
                Microsoft.Playwright.Cookie cc = new Microsoft.Playwright.Cookie();
                cc.Value = c.Value;
                cc.Name = c.Name;
                cc.Path = c.Path;
                cc.Domain = c.Domain;
                cc.HttpOnly = c.HttpOnly;
                cookieList.Add(cc);
            }
            await context.AddCookiesAsync(cookieList);
        }

        public async Task AddContextCookies(List<Microsoft.Playwright.Cookie> cookies)
        {
            await context.AddCookiesAsync(cookies);
        }

        public async Task<string> GetPageAsync(string url)
        {
            var page = await context.NewPageAsync();
            await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });
            return await page.ContentAsync();
        }
    }
}
