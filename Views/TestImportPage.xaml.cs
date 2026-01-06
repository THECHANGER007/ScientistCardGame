using ScientistCardGame.Services;
using System;
using System.IO;
using System.Text;
using System.Linq;

namespace ScientistCardGame.Views
{
    public partial class TestImportPage : ContentPage
    {
        private DatabaseService _databaseService;
        private DataImporter _dataImporter;

        public TestImportPage()
        {
            InitializeComponent();
            InitializeServices();
        }

        private void InitializeServices()
        {
            // Create database in app's data folder
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "scientistcards.db");
            _databaseService = new DatabaseService(dbPath);
            _dataImporter = new DataImporter(_databaseService);
        }

        private async void OnImportClicked(object sender, EventArgs e)
        {
            try
            {
                StatusLabel.Text = "⏳ Importing... Please wait...";
                StatusLabel.TextColor = Colors.Orange;

                // Get paths to Excel files in Data folder
                string scientistsFile = Path.Combine(AppContext.BaseDirectory, "Data", "Scientists_Cards_100_With_Effects.xlsx");
                string specialFile = Path.Combine(AppContext.BaseDirectory, "Data", "Special_Cards_20.xlsx");

                // Check if files exist
                if (!File.Exists(scientistsFile))
                {
                    StatusLabel.Text = $"❌ ERROR: Scientists file not found at: {scientistsFile}";
                    StatusLabel.TextColor = Colors.Red;
                    return;
                }

                if (!File.Exists(specialFile))
                {
                    StatusLabel.Text = $"❌ ERROR: Special cards file not found at: {specialFile}";
                    StatusLabel.TextColor = Colors.Red;
                    return;
                }

                // Import all cards
                bool success = await _dataImporter.ImportAllCardsAsync(scientistsFile, specialFile);

                if (success)
                {
                    // Get summary
                    string summary = await _dataImporter.GetImportSummaryAsync();

                    StatusLabel.Text = "✅ Import Successful!";
                    StatusLabel.TextColor = Colors.Green;

                    ResultsLabel.Text = summary;
                    ResultsFrame.IsVisible = true;
                }
                else
                {
                    StatusLabel.Text = "❌ Import Failed!";
                    StatusLabel.TextColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"❌ ERROR: {ex.Message}";
                StatusLabel.TextColor = Colors.Red;
            }
        }

        private async void OnShowLegendaryClicked(object sender, EventArgs e)
        {
            try
            {
                var cards = await _databaseService.GetCardsByTierAsync("LEGENDARY");

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Found {cards.Count} LEGENDARY cards:\n");

                foreach (var card in cards.OrderBy(c => c.Name))
                {
                    sb.AppendLine($"🌟 {card.Name}");
                    sb.AppendLine($"   Field: {card.Field} | School: {card.School}");
                    sb.AppendLine($"   ATK: {card.ATK} | DEF: {card.DEF}");
                    sb.AppendLine($"   Effect: {card.SpecialEffect}");
                    sb.AppendLine();
                }

                QueryResultsLabel.Text = sb.ToString();
                QueryFrame.IsVisible = true;
            }
            catch (Exception ex)
            {
                QueryResultsLabel.Text = $"Error: {ex.Message}";
                QueryFrame.IsVisible = true;
            }
        }

        private async void OnShowScienceClicked(object sender, EventArgs e)
        {
            try
            {
                var cards = await _databaseService.GetCardsByFieldAsync("SCIENCE");

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Found {cards.Count} SCIENCE field cards:\n");

                foreach (var card in cards.OrderByDescending(c => c.ATK).Take(10))
                {
                    sb.AppendLine($"🔬 {card.Name} ({card.Tier})");
                    sb.AppendLine($"   School: {card.School} | ATK: {card.ATK} | DEF: {card.DEF}");
                    sb.AppendLine();
                }

                QueryResultsLabel.Text = sb.ToString();
                QueryFrame.IsVisible = true;
            }
            catch (Exception ex)
            {
                QueryResultsLabel.Text = $"Error: {ex.Message}";
                QueryFrame.IsVisible = true;
            }
        }

        private async void OnShowDiscoveryClicked(object sender, EventArgs e)
        {
            try
            {
                var cards = await _databaseService.GetAllDiscoveryCardsAsync();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Found {cards.Count} DISCOVERY cards:\n");

                foreach (var card in cards)
                {
                    sb.AppendLine($"💚 {card.Name}");
                    sb.AppendLine($"   Type: {card.EffectType}");
                    sb.AppendLine($"   Effect: {card.SpecialEffect}");
                    sb.AppendLine();
                }

                QueryResultsLabel.Text = sb.ToString();
                QueryFrame.IsVisible = true;
            }
            catch (Exception ex)
            {
                QueryResultsLabel.Text = $"Error: {ex.Message}";
                QueryFrame.IsVisible = true;
            }
        }

        private async void OnShowParadoxClicked(object sender, EventArgs e)
        {
            try
            {
                var cards = await _databaseService.GetAllParadoxCardsAsync();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Found {cards.Count} PARADOX cards:\n");

                foreach (var card in cards)
                {
                    sb.AppendLine($"💗 {card.Name}");
                    sb.AppendLine($"   Type: {card.EffectType}");
                    sb.AppendLine($"   Effect: {card.SpecialEffect}");
                    sb.AppendLine();
                }

                QueryResultsLabel.Text = sb.ToString();
                QueryFrame.IsVisible = true;
            }
            catch (Exception ex)
            {
                QueryResultsLabel.Text = $"Error: {ex.Message}";
                QueryFrame.IsVisible = true;
            }
        }
    }
}