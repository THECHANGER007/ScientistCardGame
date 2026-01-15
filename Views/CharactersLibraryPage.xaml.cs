using ScientistCardGame.Models;
using ScientistCardGame.Services;

namespace ScientistCardGame.Views
{
    public partial class CharactersLibraryPage : ContentPage
    {
        private List<Card> _allCharacters;

        public CharactersLibraryPage()
        {
            InitializeComponent();
            LoadCharacters();
        }

        private async void LoadCharacters()
        {
            var dbService = new DatabaseService(Path.Combine(FileSystem.AppDataDirectory, "scientistcards.db"));

            // Get ALL cards directly from database
            var allCards = await dbService.GetAllCardsAsync();

            // Filter to CHARACTER cards only and remove duplicates
            _allCharacters = allCards
                .Where(c => c.CardType == "CHARACTER")
                .GroupBy(c => c.Name)
                .Select(g => g.First())
                .OrderBy(c => c.Name)
                .ToList();

            DisplayCharacters(_allCharacters);
        }

        private void DisplayCharacters(List<Card> characters)
        {
            CharactersContainer.Clear();

            if (characters.Count == 0)
            {
                var noResultsLabel = new Label
                {
                    Text = "No characters found",
                    FontSize = 18,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                };
                CharactersContainer.Add(noResultsLabel);
                return;
            }

            foreach (var character in characters)
            {
                var characterCard = CreateCharacterCard(character);
                CharactersContainer.Add(characterCard);
            }
        }

        private Frame CreateCharacterCard(Card character)
        {
            Color borderColor = character.Tier switch
            {
                "LEGENDARY" => Colors.Gold,
                "MASTER" => Colors.Silver,
                "SCHOLAR" => Color.FromArgb("#CD7F32"),
                _ => Colors.Gray
            };

            Color bgColor = character.Field switch
            {
                "SCIENCE" => Color.FromArgb("#1e3a5f"),
                "PHILOSOPHY" => Color.FromArgb("#4a1e5f"),
                "SPIRITUALITY" => Color.FromArgb("#5f3a1e"),
                "HUMANITIES" => Color.FromArgb("#1e5f3a"),
                _ => Color.FromArgb("#1e293b")
            };

            var frame = new Frame
            {
                BorderColor = borderColor,
                BackgroundColor = bgColor,
                CornerRadius = 15,
                Padding = 15,
                Margin = new Thickness(0, 5)
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => OnCharacterTapped(character);
            frame.GestureRecognizers.Add(tapGesture);

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var leftStack = new VerticalStackLayout { Spacing = 8 };

            string tierBadge = character.Tier switch
            {
                "LEGENDARY" => "★★★",
                "MASTER" => "★★",
                "SCHOLAR" => "★",
                _ => ""
            };

            var nameLabel = new Label
            {
                Text = $"{tierBadge} {character.Name}",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            };
            leftStack.Add(nameLabel);

            var infoLabel = new Label
            {
                Text = $"{character.Tier} • {character.Field}",
                FontSize = 14,
                TextColor = Color.FromArgb("#94a3b8")
            };
            leftStack.Add(infoLabel);

            var schoolLabel = new Label
            {
                Text = $"🏛️ {character.School}",
                FontSize = 13,
                TextColor = Color.FromArgb("#cbd5e1")
            };
            leftStack.Add(schoolLabel);

            grid.Add(leftStack, 0, 0);

            var rightStack = new VerticalStackLayout
            {
                Spacing = 5,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center
            };

            var atkLabel = new Label
            {
                Text = $"⚔️ {character.ATK}",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#ef4444"),
                HorizontalTextAlignment = TextAlignment.End
            };
            rightStack.Add(atkLabel);

            var defLabel = new Label
            {
                Text = $"🛡️ {character.DEF}",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#3b82f6"),
                HorizontalTextAlignment = TextAlignment.End
            };
            rightStack.Add(defLabel);

            var tapHintLabel = new Label
            {
                Text = "Tap for details →",
                FontSize = 11,
                TextColor = Color.FromArgb("#64748b"),
                FontAttributes = FontAttributes.Italic,
                HorizontalTextAlignment = TextAlignment.End,
                Margin = new Thickness(0, 5, 0, 0)
            };
            rightStack.Add(tapHintLabel);

            grid.Add(rightStack, 1, 0);

            frame.Content = grid;
            return frame;
        }

        private async void OnCharacterTapped(Card character)
        {
            string biography = CharacterBiographies.GetBiography(character.Name);

            string details = biography + "\n\n";
            details += $"━━━━━━━━━━━━━━━━━━━━━━━\n";
            details += $"🎮 IN-GAME STATS:\n\n";
            details += $"🎖️ TIER: {character.Tier}\n";
            details += $"🌐 FIELD: {character.Field}\n";
            details += $"🏛️ SCHOOL: {character.School}\n";
            details += $"⚔️ ATK: {character.ATK}\n";
            details += $"🛡️ DEF: {character.DEF}\n\n";
            details += $"✨ SPECIAL EFFECT:\n{character.SpecialEffect}";

            await DisplayAlert(
                $"📚 {character.Name}",
                details,
                "Close"
            );
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
            {
                DisplayCharacters(_allCharacters);
                return;
            }

            var filtered = _allCharacters.Where(c =>
                c.Name.ToLower().Contains(searchText) ||
                c.Field.ToLower().Contains(searchText) ||
                c.School.ToLower().Contains(searchText) ||
                c.Tier.ToLower().Contains(searchText)
            ).ToList();

            DisplayCharacters(filtered);
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}