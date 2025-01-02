using LoginFacebook.Model;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Text.Json;
using Keys = OpenQA.Selenium.Keys;

namespace Form1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void LoginClick(object sender, EventArgs e)
        {
            try
            {
                string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\", "Data", "FacebookAccount.json"));
                string jsonFilePath = projectDirectory;

                string jsonContent = File.ReadAllText(jsonFilePath);
                var facebookAccounts = JsonSerializer.Deserialize<List<FacebookAccount>>(jsonContent);

                if (facebookAccounts == null || facebookAccounts.Count == 0)
                {
                    MessageBox.Show("Không có tài khoản nào trong danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var tasks = facebookAccounts.Select(account => Task.Run(() => LoginFacebook(account.UserName, account.Password,account.Key)));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoginFacebook(string userName, string password,string Key)
        {
            ChromeDriver localDriver = null;
            try
            {
                var options = new ChromeOptions();
                options.AddArgument("--start-maximized");

                localDriver = new ChromeDriver(options);

                localDriver.Navigate().GoToUrl("https://www.facebook.com/");
                localDriver.FindElement(By.Id("email")).SendKeys(userName);
                localDriver.FindElement(By.Id("pass")).SendKeys(password);
                localDriver.FindElement(By.Name("login")).Click();

                Thread.Sleep(5000);

                localDriver.ExecuteScript("window.open('https://2fa.live/', '_blank');");
                localDriver.SwitchTo().Window(localDriver.WindowHandles.Last());
                var inputField = localDriver.FindElement(By.Id("listToken"));
                inputField.SendKeys(Key);
                localDriver.FindElement(By.Id("submit")).Click();

                Thread.Sleep(3000);
                localDriver.FindElement(By.Id("copy_btn")).Click();

                string code = GetClipboardText();

                localDriver.SwitchTo().Window(localDriver.WindowHandles.First());

                var inputElement = localDriver.FindElements(By.CssSelector("input")).FirstOrDefault();
                if (inputElement != null && !string.IsNullOrEmpty(inputElement.GetAttribute("id")))
                {
                    string inputId = inputElement.GetAttribute("id");
                    localDriver.FindElement(By.Id(inputId)).SendKeys(code);
                    var continueButton = localDriver.FindElements(By.XPath("//span[contains(text(), 'Continue')]")).FirstOrDefault();
                    if (continueButton != null)
                    {
                        continueButton.Click();
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy nút 'Continue' trên trang!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    var twoFactorField = localDriver.FindElement(By.Id("approvals_code"));
                    twoFactorField.SendKeys(code);
                    Thread.Sleep(1000);
                    localDriver.FindElement(By.Id("approvals_code")).SendKeys(Keys.Enter);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi với tài khoản {userName}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
               
            }
        }
        private string GetClipboardText()
        {
            string clipboardText = string.Empty;
            Thread thread = new Thread(() =>
            {
                clipboardText = Clipboard.GetText();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (!string.IsNullOrEmpty(clipboardText) && clipboardText.Contains("|"))
            {
                clipboardText = clipboardText.Split('|').Last();
            }

            return clipboardText;
        }
    }
}
