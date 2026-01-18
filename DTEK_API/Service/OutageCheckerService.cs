using System.Text.Json;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace DTEK_API.Service;

public class OutageCheckerService : IDisposable
    {
        private IWebDriver driver;
        
        public OutageCheckerService()
        {
            
            
        }
        
        public async Task<Dictionary<string, string>> CheckOutages(string city, string street, string house)
        {
            
            new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);
            var options = new ChromeOptions();
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            
            driver = new ChromeDriver(options);
            await Task.Delay(10000);
            
            driver.Navigate().GoToUrl("https://www.dtek-krem.com.ua/ua/shutdowns");
            await Task.Delay(15000);
            try
            {
                driver.FindElement(By.CssSelector(".modal__close")).Click();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            await Task.Delay(500);
            await FillField("city", city);
            await FillField("street", street);
            await FillField("house_num", house);
            await Task.Delay(3000);
            var container = driver.FindElement(By.Id("discon-fact"));
            var tablesContainer = container.FindElement(By.ClassName("discon-fact-tables"));
            var tableRoot = tablesContainer.FindElement(By.CssSelector("div.discon-fact-table.active"));
            var html = tableRoot.GetAttribute("innerHTML");
            var pattern = @"<td\s+class=""([^""]+)""></td>";
            var matches = Regex.Matches(html, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var sheduled = "cell-scheduled";
            var notSheduled = "cell-non-scheduled";
            var fisrtHalfSheduled = "cell-first-half";
            var secondHalfSheduled = "cell-second-half";
            var counter = 0;
            var result = new Dictionary<string, string>();
            foreach (Match match in matches)
            {
                string className = match.Groups[1].Value;
                result.Add($"{counter} : {counter + 1}", className);
                counter++;
            }
            
            return result;
        }
        
        private async Task FillField(string fieldId, string value)
        {
            var field = driver.FindElement(By.Id(fieldId));
            field.Clear();
            
            foreach (char c in value)
            {
                field.SendKeys(c.ToString());
                await Task.Delay(200);
            }
            
            await Task.Delay(500);
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var autocompleteItem = wait.Until(d => 
                    d.FindElement(By.CssSelector(".autocomplete-items div")));
                autocompleteItem.Click();
            }
            catch
            {
                // Якщо список не з'явився, просто натискаємо Enter
                field.SendKeys(Keys.Enter);
            }
            await Task.Delay(1000);
        }
        
        public void SaveToJson(Dictionary<string, string> data, string filename)
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            var json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filename, json);
            Console.WriteLine($"💾 Збережено у {filename}");
        }
        
        public void Dispose()
        {
            driver.Quit();
            driver.Dispose();
        }
    }