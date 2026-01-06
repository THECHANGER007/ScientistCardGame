using ScientistCardGame.Models;
using ScientistCardGame.Services;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ScientistCardGame.Views
{
    public partial class DeckBuilderTestPage : ContentPage
    {
        private DatabaseService _databaseService;
        private DeckBuilderService _deckBuilderService;
        private Deck _currentDeck;

        public DeckBuilderTestPage()
        {
            InitializeComponent();
            InitializeServices();
            LoadAvailableCards();
        }

        private void InitializeServices()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "scientistcards.db");
            _databaseService = new DatabaseService(dbPath);
            _deckBuilderService = new DeckBuilderService(_databaseService);
        }

        private async void LoadAvailableCards()
        {
            await _deckBuilderService.LoadAvailableCardsAsync();
            ShowAllCards();
        }

        private async void OnCreateStarterDeckClicked(object sender, EventArgs e)
        {
            _currentDeck = await _deckBuilderService.CreateStarterDeckAsync("Starter Deck");
            UpdateDeckDisplay();
            await DisplayAlert("✅ Success", "Starter deck created with 30 balanced cards!", "OK");
        }

        private async void OnCreateScienceDeckClicked(object sender, EventArgs e)
        {
            _currentDeck = await _deckBuilderService.CreateScienceDeckAsync();
            UpdateDeckDisplay();
            await DisplayAlert("✅ Success", "Science-focused deck created!", "OK");
        }

        private void OnCreateEmptyDeckClicked(object sender, EventArgs e)
        {
            _currentDeck = _deckBuilderService.CreateNewDeck("My Custom Deck");
            UpdateDeckDisplay();
            ShowAllCards();
        }

        private void OnShowAllCardsClicked(object sender, EventArgs e)
        {
            ShowAllCards();
        }

        private void ShowAllCards()
        {
            var cards = _deckBuilderService.GetAllAvailableCards();
            DisplayCards(cards, "All Cards");
        }

        private void OnShowLegendaryClicked(object sender, EventArgs e)
        {
            var cards = _deckBuilderService.GetCardsByTier("LEGENDARY");
            DisplayCards(cards, "LEGENDARY Cards");
        }

        private void OnShowScienceClicked(object sender, EventArgs e)
        {
            var cards = _deckBuilderService.GetCardsByField("SCIENCE");
            DisplayCards(cards, "SCIENCE Field Cards");
        }

        private void OnShowDiscoveryClicked(object sender, EventArgs e)
        {
            var cards = _deckBuilderService.GetDiscoveryCards();
            DisplayCards(cards, "DISCOVERY Cards");
        }

        private void OnShowParadoxClicked(object sender, EventArgs e)
        {
            var cards = _deckBuilderService.GetParadoxCards();
            DisplayCards(cards, "PARADOX Cards");
        }

        private void DisplayCards(System.Collections.Generic.List<Card> cards, string title)
        {
            CardListContainer.Clear();

            var titleLabel = new Label
            {
                Text = $"📚 {title} ({cards.Count} cards)",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.DarkBlue,
                Margin = new Thickness(0, 10, 0, 10)
            };
            CardListContainer.Add(titleLabel);

            foreach (var card in cards.Take(20))
            {
                var cardFrame = CreateCardView(card);
                CardListContainer.Add(cardFrame);
            }
        }

        private Frame CreateCardView(Card card)
        {
            var frame = new Frame
            {
                BorderColor = GetCardBorderColor(card),
                CornerRadius = 10,
                Padding = 10,
                Margin = new Thickness(0, 5),
                BackgroundColor = GetCardBackgroundColor(card)
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var infoStack = new VerticalStackLayout { Spacing = 3 };

            infoStack.Add(new Label
            {
                Text = card.Name,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold
            });

            if (card.CardType == "CHARACTER")
            {
                infoStack.Add(new Label
                {
                    Text = $"{card.Tier} | {card.Field} | {card.School}",
                    FontSize = 11,
                    TextColor = Colors.Gray
                });
                infoStack.Add(new Label
                {
                    Text = $"⚔️ ATK: {card.ATK}  🛡️ DEF: {card.DEF}",
                    FontSize = 12,
                    TextColor = Colors.DarkRed
                });
            }
            else
            {
                infoStack.Add(new Label
                {
                    Text = $"{card.CardType} - {card.EffectType}",
                    FontSize = 11,
                    TextColor = Colors.Gray
                });
            }

            infoStack.Add(new Label
            {
                Text = card.SpecialEffect,
                FontSize = 10,
                TextColor = Colors.DarkGreen,
                MaximumWidthRequest = 400
            });

            grid.Add(infoStack, 0, 0);

            if (_currentDeck != null)
            {
                var addButton = new Button
                {
                    Text = "➕",
                    WidthRequest = 50,
                    HeightRequest = 50,
                    CornerRadius = 25,
                    BackgroundColor = Colors.Green,
                    TextColor = Colors.White,
                    FontSize = 20
                };

                int currentCount = _currentDeck.GetCardCount(card.Name);
                if (currentCount > 0)
                {
                    addButton.Text = $"{currentCount}";
                    addButton.BackgroundColor = Colors.Orange;
                }

                addButton.Clicked += (s, e) => AddCardToDeck(card, addButton);
                grid.Add(addButton, 1, 0);
            }

            frame.Content = grid;
            return frame;
        }

        private async void AddCardToDeck(Card card, Button button)
        {
            if (_currentDeck == null)
            {
                await DisplayAlert("⚠️ No Deck", "Create a deck first!", "OK");
                return;
            }

            bool success = _currentDeck.AddCard(card);

            if (success)
            {
                int count = _currentDeck.GetCardCount(card.Name);
                button.Text = $"{count}";
                button.BackgroundColor = Colors.Orange;

                UpdateDeckDisplay();
            }
            else
            {
                if (_currentDeck.Cards.Count >= 30)
                {
                    await DisplayAlert("⚠️ Deck Full", "Deck already has 30 cards!", "OK");
                }
                else
                {
                    await DisplayAlert("⚠️ Limit Reached", $"Maximum 4 copies of '{card.Name}' allowed!", "OK");
                }
            }
        }

        private void UpdateDeckDisplay()
        {
            if (_currentDeck == null)
            {
                DeckNameLabel.Text = "No Deck Created";
                DeckStatsLabel.Text = "Create a deck to see statistics";
                ValidationLabel.Text = "";
                CurrentDeckLabel.Text = "Deck is empty";
                return;
            }

            DeckNameLabel.Text = $"📦 {_currentDeck.DeckName}";

            var stats = _deckBuilderService.GetDeckStatistics(_currentDeck);
            DeckStatsLabel.Text = stats.ToString();

            var validation = _deckBuilderService.ValidateDeck(_currentDeck);
            ValidationLabel.Text = validation.errorMessage;
            ValidationLabel.TextColor = validation.isValid ? Colors.Green : Colors.Red;

            var groupedCards = _currentDeck.Cards
                .GroupBy(c => c.Name)
                .OrderBy(g => g.Key);

            StringBuilder sb = new StringBuilder();
            foreach (var group in groupedCards)
            {
                sb.AppendLine($"{group.Key} x{group.Count()}");
            }

            CurrentDeckLabel.Text = sb.ToString();
        }

        private Color GetCardBorderColor(Card card)
        {
            if (card.CardType == "DISCOVERY") return Colors.Green;
            if (card.CardType == "PARADOX") return Colors.Pink;

            return card.Tier switch
            {
                "LEGENDARY" => Colors.Gold,
                "MASTER" => Colors.Silver,
                "SCHOLAR" => Color.FromArgb("#CD7F32"),
                _ => Colors.Gray
            };
        }

        private Color GetCardBackgroundColor(Card card)
        {
            if (card.CardType == "DISCOVERY") return Color.FromArgb("#F0FFF0");
            if (card.CardType == "PARADOX") return Color.FromArgb("#FFF0F5");

            return card.Tier switch
            {
                "LEGENDARY" => Color.FromArgb("#FFFACD"),
                "MASTER" => Color.FromArgb("#F5F5F5"),
                "SCHOLAR" => Color.FromArgb("#FFF8DC"),
                _ => Colors.White
            };
        }
    }
}