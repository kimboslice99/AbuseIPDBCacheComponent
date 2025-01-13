using System;
using System.Windows.Forms;
using System.Configuration;
using System.Reflection;
using System.IO;

namespace AbuseIPDBCacheComponent_ConfigurationTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            if(!CurrentUserHasWriteAccess())
            {
                MessageBox.Show("Write access denied. Cannot proceed, please try running as administrator.");
                this.Close();
            }
            InitializeComponent();
            // Get the path to the configuration file
            string dllConfigPath = Path.Combine(
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location
                ),
                "AbuseIPDBCacheComponent.dll"
            );

            // Open the configuration file
            Configuration config = ConfigurationManager.OpenExeConfiguration(dllConfigPath);

            // Fetch the current values
            string tlsVersion = config.AppSettings.Settings["HttpClientProtocol"]?.Value;
            string enableLogging = config.AppSettings.Settings["EnableLogging"]?.Value;
            string cacheTime = config.AppSettings.Settings["CacheTimeHours"]?.Value;
            string apiKey = config.AppSettings.Settings["ApiKey"]?.Value;
            string confidenceScore = config.AppSettings.Settings["MinConfidenceScore"]?.Value;
            string maxAgeInDays = config.AppSettings.Settings["MaxAgeInDays"]?.Value;


            // Display the values in the controls
            tlsComboBox.SelectedItem = tlsVersion;
            checkBox1.Checked = bool.TryParse(enableLogging, out bool isLoggingEnabled) && isLoggingEnabled;
            cacheTimeBox.Text = cacheTime;
            abuseipdbApiKey.Text = apiKey;
            minConfidenceScore.Text = confidenceScore;
            maxAge.Text = maxAgeInDays;
        }

        private void submit_Click(object sender, EventArgs e)
        {
            // Get the path to the configuration file
            string dllConfigPath = Path.Combine(
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location
                ),
                "AbuseIPDBCacheComponent.dll"
            );

            // Get the selected values from the controls
            string selectedTlsVersion = tlsComboBox.SelectedItem?.ToString();
            bool isLoggingEnabled = checkBox1.Checked;
            string cacheTimeHours = cacheTimeBox.Text;
            string apiKey = abuseipdbApiKey.Text;
            string maxAgeInDays = maxAge.Text;
            string confidenceScore = minConfidenceScore.Text;

            if(string.IsNullOrEmpty( apiKey ))
            {
                MessageBox.Show("No API key entered. Component will not function without an API key from AbuseIPDB.");
            }

            // Open the configuration file
            Configuration config = ConfigurationManager.OpenExeConfiguration(dllConfigPath);

            // Update the AppSettings section
            if (config.AppSettings.Settings["HttpClientProtocol"] != null)
            {
                config.AppSettings.Settings["HttpClientProtocol"].Value = selectedTlsVersion;
            }
            else
            {
                config.AppSettings.Settings.Add("HttpClientProtocol", selectedTlsVersion);
            }

            if (config.AppSettings.Settings["EnableLogging"] != null)
            {
                config.AppSettings.Settings["EnableLogging"].Value = isLoggingEnabled.ToString();
            }
            else
            {
                config.AppSettings.Settings.Add("EnableLogging", isLoggingEnabled.ToString());
            }

            if (config.AppSettings.Settings["CacheTimeHours"] != null)
            {
                config.AppSettings.Settings["CacheTimeHours"].Value = cacheTimeHours;
            }
            else
            {
                config.AppSettings.Settings.Add("CacheTimeHours", cacheTimeHours);
            }

            if (config.AppSettings.Settings["ApiKey"] != null)
            {
                config.AppSettings.Settings["ApiKey"].Value = apiKey;
            }
            else
            {
                config.AppSettings.Settings.Add("ApiKey", apiKey);
            }

            if (config.AppSettings.Settings["MaxAgeInDays"] != null)
            {
                config.AppSettings.Settings["MaxAgeInDays"].Value = maxAgeInDays;
            }
            else
            {
                config.AppSettings.Settings.Add("MaxAgeInDays", maxAgeInDays);
            }

            if (config.AppSettings.Settings["MinConfidenceScore"] != null)
            {
                config.AppSettings.Settings["MinConfidenceScore"].Value = confidenceScore;
            }
            else
            {
                config.AppSettings.Settings.Add("MinConfidenceScore", confidenceScore);
            }

            // Save the configuration
            config.Save(ConfigurationSaveMode.Modified);
            // Display a message to confirm the update
            MessageBox.Show("Configuration updated successfully!");
            // were done here
            this.Close();
        }

        private void digit_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check if the entered character is a digit or a control character (like backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                // If it's not, set Handled to true to cancel the key press
                e.Handled = true;
            }
        }

        private void digitOrDot_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check if the entered character is a digit or a control character (like backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                // If it's not, set Handled to true to cancel the key press
                e.Handled = true;
            }
        }

        private bool CurrentUserHasWriteAccess()
        {
            string testFile = Path.Combine(
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location
                ),
                "test.file"
            );
            try
            {
                StreamWriter stream = File.AppendText(testFile);
                stream.WriteLine("test");
                stream.Flush();
                stream.Close();
                File.Delete(testFile);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
