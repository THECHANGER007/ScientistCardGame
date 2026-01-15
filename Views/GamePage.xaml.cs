using ScientistCardGame.Models;
using ScientistCardGame.Services;
using System;
using System.IO;
using System.Linq;
using Plugin.Maui.Audio;  // ← ADD THIS

namespace ScientistCardGame.Views
{
    public partial class GamePage : ContentPage
    {
        private GameEngine _gameEngine;
        private EffectsEngine _effectsEngine;
        private AIOpponent _aiOpponent;
        private DatabaseService _databaseService;
        private DeckBuilderService _deckBuilderService;
        private Card _selectedCard;
        private Random _random = new Random();
        private StatsService _statsService;
        private TurnTimerService _turnTimer;
        private bool _isMultiplayerMode = false;
        private bool _isInitialized = false;
        private AudioService _audioService;  // ← ADD THIS
        private bool _gameOverShown = false;
        public enum AIDifficulty { EASY, NORMAL, HARD }
        private AIDifficulty _aiDifficulty = AIDifficulty.NORMAL; // Default


        public GamePage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            MessageLabel.Text = "🎮 Initializing game...";
            await Task.Delay(100);
            InitializeGame();
        }

        private async void InitializeGame()
        {
            try
            {
                _gameOverShown = false;

                MessageLabel.Text = "STEP 1: Starting...";
                await Task.Delay(500);

                if (_turnTimer != null)
                {
                    _turnTimer.StopTimer();
                }

                MessageLabel.Text = "STEP 2: Creating database service...";
                await Task.Delay(500);

                _databaseService = new DatabaseService(Path.Combine(FileSystem.AppDataDirectory, "scientistcards.db"));

                MessageLabel.Text = "STEP 3: Creating deck builder...";
                await Task.Delay(500);

                _deckBuilderService = new DeckBuilderService(_databaseService);

                MessageLabel.Text = "STEP 4: Creating stats service...";
                await Task.Delay(500);

                _statsService = new StatsService();

                MessageLabel.Text = "STEP 5: Creating timer...";
                await Task.Delay(500);

                _turnTimer = new TurnTimerService(60);
                _turnTimer.IsEnabled = false;

                // ← ADD AUDIO SERVICE HERE
                MessageLabel.Text = "STEP 5.5: Creating audio service...";
                await Task.Delay(500);

                _audioService = new AudioService(AudioManager.Current);
                await _audioService.PlayBackgroundMusicAsync("background_music.mp3");

                MessageLabel.Text = "STEP 6: Checking database...";
                await Task.Delay(500);

                bool isEmpty = await _databaseService.IsDatabaseEmptyAsync();
                isEmpty = true;

                if (isEmpty)
                {
                    MessageLabel.Text = "📥 Importing cards from Excel... Please wait!";
                    await Task.Delay(500);

                    var importer = new DataImporter(_databaseService);
                    try
                    {
                        // Copy CHARACTER cards to temp
                        using var stream1 = await FileSystem.OpenAppPackageFileAsync("Data/Scientists_Cards_100_With_Effects_U.xlsx");
                        var tempPath1 = Path.Combine(FileSystem.CacheDirectory, "temp_characters.xlsx");
                        using (var fileStream1 = File.Create(tempPath1))
                        {
                            await stream1.CopyToAsync(fileStream1);
                        }

                        // Copy SPECIAL cards to temp
                        using var stream2 = await FileSystem.OpenAppPackageFileAsync("Data/Special_Cards_20.xlsx");
                        var tempPath2 = Path.Combine(FileSystem.CacheDirectory, "temp_special.xlsx");
                        using (var fileStream2 = File.Create(tempPath2))
                        {
                            await stream2.CopyToAsync(fileStream2);
                        }

                        // Import BOTH files
                        await importer.ImportAllCardsAsync(tempPath1, tempPath2);

                        // Clean up
                        File.Delete(tempPath1);
                        File.Delete(tempPath2);
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("❌ Import Error", $"Could not find Excel file!\n\n{ex.Message}", "OK");
                        return;
                    }

                    MessageLabel.Text = "✅ Cards imported successfully!";
                    await Task.Delay(1000);
                }

                MessageLabel.Text = "STEP 7: Loading cards...";
                await Task.Delay(500);

                await _deckBuilderService.LoadAvailableCardsAsync();

                MessageLabel.Text = "STEP 8: Creating Player 1 deck...";
                await Task.Delay(500);

                var player1Deck = await _deckBuilderService.CreateStarterDeckAsync("Player 1");

                MessageLabel.Text = "STEP 9: Creating AI deck...";
                await Task.Delay(500);

                var player2Deck = await _deckBuilderService.CreateStarterDeckAsync("AI Opponent");

                MessageLabel.Text = "STEP 10: Starting game engine...";
                await Task.Delay(500);

                _gameEngine = new GameEngine();
                _gameEngine.StartNewGame("Player 1", "AI Opponent", player1Deck, player2Deck);

                MessageLabel.Text = "STEP 11: Creating effects engine...";
                await Task.Delay(500);

                var gameState = _gameEngine.GetGameState();
                _effectsEngine = new EffectsEngine(gameState);

                MessageLabel.Text = "STEP 12: Choose AI difficulty...";
                await Task.Delay(500);

                // ✨ ASK PLAYER TO CHOOSE DIFFICULTY
                string difficultyChoice = await DisplayActionSheet(
                    "🤖 SELECT AI DIFFICULTY",
                    null,
                    null,
                    "😊 EASY - AI makes mistakes",
                    "⚔️ NORMAL - Balanced challenge",
                    "💀 HARD - AI plays perfectly"
                );

                if (difficultyChoice?.Contains("EASY") == true)
                    _aiDifficulty = AIDifficulty.EASY;
                else if (difficultyChoice?.Contains("HARD") == true)
                    _aiDifficulty = AIDifficulty.HARD;
                else
                    _aiDifficulty = AIDifficulty.NORMAL;

                string difficultyName = _aiDifficulty.ToString();
                MessageLabel.Text = $"STEP 12: Creating {difficultyName} AI opponent...";
                await Task.Delay(500);

                _aiOpponent = new AIOpponent(_gameEngine, _effectsEngine);

                MessageLabel.Text = "STEP 13: Updating UI...";
                await Task.Delay(500);

                UpdateGameUI();

                ShowMessage("🎮 Game Started!");

                // TEST ANIMATION - Remove this later
                MessageLabel.Text = "Testing animations...";
                await Task.Delay(1000);

                var testCard = _gameEngine.GetGameState().CurrentPlayer.Field.FirstOrDefault();
                if (testCard != null)
                {
                    await DisplayAlert("Animation Test", "Watch the first card get highlighted!", "OK");
                    await HighlightTargetCard(testCard);
                    await Task.Delay(1000);
                    await DisplayAlert("Animation Test", "Now watch it get crushed!", "OK");
                    await CrushCardAnimation(testCard);
                }


                _isInitialized = true;
            }
            catch (Exception ex)
            {
                MessageLabel.Text = $"❌ ERROR: {ex.Message}";
                await DisplayAlert("ERROR", ex.Message + "\n\n" + ex.StackTrace, "OK");
            }
        }

        private void UpdateGameUI()
        {
            var gameState = _gameEngine.GetGameState();
            var player = gameState.Player1;
            var opponent = gameState.Player2;

            PlayerNameLabel.Text = player.PlayerName;
            PlayerHPLabel.Text = player.HP.ToString();
            PlayerDeckCountLabel.Text = player.Deck.Count.ToString();
            PlayerGraveyardCountLabel.Text = player.Graveyard.Count.ToString();
            PlayerHandCountLabel.Text = $"Hand: {player.Hand.Count} | Summons: {player.SummonsThisTurn}/{player.MaxSummonsPerTurn}";

            OpponentNameLabel.Text = opponent.PlayerName;
            OpponentHPLabel.Text = opponent.HP.ToString();
            OpponentDeckCountLabel.Text = opponent.Deck.Count.ToString();
            OpponentGraveyardCountLabel.Text = opponent.Graveyard.Count.ToString();
            OpponentHandCountLabel.Text = $"Hand: {opponent.Hand.Count}";

            string turnText = $"Turn {gameState.CurrentTurn} - {gameState.CurrentPlayer.PlayerName}'s Turn";
            if (_isMultiplayerMode && gameState.CurrentPlayer.PlayerId == 2)
            {
                turnText += " 📱";
            }
            TurnInfoLabel.Text = turnText;
            PhaseLabel.Text = $"📍 {gameState.CurrentPhase} Phase";
            TimerLabel.IsVisible = false;

            var activeBonuses = GetActiveFieldBonuses(gameState.CurrentPlayer);
            if (!string.IsNullOrEmpty(activeBonuses))
            {
                MessageLabel.Text = activeBonuses;
            }

            UpdateHandDisplay(player);
            UpdateFieldDisplay(PlayerFieldContainer, player);
            UpdateFieldDisplay(OpponentFieldContainer, opponent);
            UpdateTrapZoneDisplay(PlayerTrapZoneContainer, player);
            UpdateTrapZoneDisplay(OpponentTrapZoneContainer, opponent);

            if (gameState.IsGameOver)
            {
                ShowGameOver(gameState.Winner);
            }
        }

        private void UpdateHandDisplay(Player player)
        {
            PlayerHandContainer.Clear();

            foreach (var card in player.Hand)
            {
                var cardView = CreateHandCardView(card);
                PlayerHandContainer.Add(cardView);
            }
        }

        private Frame CreateHandCardView(Card card)
        {
            Color borderColor = Colors.Gray;
            Color backgroundColor = Color.FromArgb("#2C3E50");

            if (card.CardType == "CHARACTER")
            {
                borderColor = card.Field switch
                {
                    "SCIENCE" => Color.FromArgb("#3498DB"),
                    "PHILOSOPHY" => Color.FromArgb("#9B59B6"),
                    "SPIRITUALITY" => Color.FromArgb("#F39C12"),
                    "HUMANITIES" => Color.FromArgb("#27AE60"),
                    _ => Colors.Gray
                };
            }
            else if (card.CardType == "DISCOVERY")
            {
                borderColor = Color.FromArgb("#2ECC71");
                backgroundColor = Color.FromArgb("#1E8449");
            }
            else if (card.CardType == "PARADOX")
            {
                borderColor = Color.FromArgb("#E91E63");
                backgroundColor = Color.FromArgb("#880E4F");
            }

            var frame = new Frame
            {
                BorderColor = borderColor,
                BackgroundColor = backgroundColor,
                CornerRadius = 12,
                Padding = 8,
                Margin = 3,
                WidthRequest = 140,
                HeightRequest = 200,
                HasShadow = true
            };
            // ✨ ADD THIS LINE:
            frame.BindingContext = card;

            var stackLayout = new VerticalStackLayout
            {
                Spacing = 4
            };

            string tierBadge = card.Tier switch
            {
                "LEGENDARY" => "★",
                "MASTER" => "◆",
                "SCHOLAR" => "●",
                _ => ""
            };

            string cardTypeIcon = card.CardType switch
            {
                "DISCOVERY" => "🔬",
                "PARADOX" => "🔮",
                _ => ""
            };

            var nameLabel = new Label
            {
                Text = $"{tierBadge} {cardTypeIcon} {card.Name}".Trim(),
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                LineBreakMode = LineBreakMode.WordWrap,
                MaxLines = 2,
                HorizontalOptions = LayoutOptions.Center
            };
            stackLayout.Add(nameLabel);

            string typeText = card.CardType == "CHARACTER"
                ? $"{card.Tier}"
                : $"{card.CardType}";

            var tierLabel = new Label
            {
                Text = typeText,
                FontSize = 9,
                TextColor = Color.FromArgb("#BDC3C7"),
                HorizontalOptions = LayoutOptions.Center
            };
            stackLayout.Add(tierLabel);

            if (card.CardType == "CHARACTER")
            {
                var statsLabel = new Label
                {
                    Text = $"⚔️ {card.ATK}  🛡️ {card.DEF}",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#ECF0F1"),
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 5, 0, 5)
                };
                stackLayout.Add(statsLabel);

                var fieldLabel = new Label
                {
                    Text = $"🌐 {card.Field}",
                    FontSize = 9,
                    TextColor = Color.FromArgb("#95A5A6"),
                    HorizontalOptions = LayoutOptions.Center
                };
                stackLayout.Add(fieldLabel);

                var schoolLabel = new Label
                {
                    Text = $"📚 {card.School}",
                    FontSize = 8,
                    TextColor = Color.FromArgb("#7F8C8D"),
                    HorizontalOptions = LayoutOptions.Center
                };
                stackLayout.Add(schoolLabel);
            }

            var buttonStack = new HorizontalStackLayout
            {
                Spacing = 5,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };

            var playButton = new Button
            {
                Text = "▶",
                FontSize = 12,
                Padding = 5,
                BackgroundColor = Color.FromArgb("#27AE60"),
                TextColor = Colors.White,
                CornerRadius = 5,
                WidthRequest = 50
            };
            playButton.Clicked += (s, e) => OnPlayCardClicked(card);
            buttonStack.Add(playButton);

            var infoButton = new Button
            {
                Text = "ℹ️",
                FontSize = 12,
                Padding = 5,
                BackgroundColor = Color.FromArgb("#3498DB"),
                TextColor = Colors.White,
                CornerRadius = 5,
                WidthRequest = 50
            };
            infoButton.Clicked += (s, e) => OnCardDetailsClicked(card);
            buttonStack.Add(infoButton);

            stackLayout.Add(buttonStack);
            frame.Content = stackLayout;

            return frame;
        }

        private void UpdateFieldDisplay(HorizontalStackLayout container, Player player)
        {
            container.Clear();

            var characterCards = player.Field.Where(c => c.CardType == "CHARACTER").ToList();

            if (characterCards.Count == 0)
            {
                container.Add(new Label
                {
                    Text = "No monsters on field",
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = 20
                });
            }
            else
            {
                foreach (var card in characterCards)
                {
                    var cardView = CreateFieldCardView(card);
                    container.Add(cardView);
                }
            }
        }

        private void UpdateTrapZoneDisplay(VerticalStackLayout container, Player player)
        {
            container.Clear();

            var trapCards = player.Field.Where(c => c.CardType == "PARADOX").ToList();

            if (trapCards.Count == 0)
            {
                container.Add(new Label
                {
                    Text = "No traps set",
                    FontSize = 10,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = 10
                });
            }
            else
            {
                bool isPlayerCard = (player.PlayerId == 1);

                foreach (var trap in trapCards)
                {
                    var trapView = CreateTrapCardView(trap, isPlayerCard);
                    container.Add(trapView);
                }
            }
        }

        private async void OnCardDetailsClicked(Card card)
        {
            string details = $"📜 {card.Name}\n\n";

            if (card.CardType == "CHARACTER")
            {
                details += $"Tier: {card.Tier}\n";
                details += $"Field: {card.Field}\n";
                details += $"School: {card.School}\n";
                details += $"ATK: {card.ATK} | DEF: {card.DEF}\n\n";
            }
            else
            {
                details += $"Type: {card.CardType}\n\n";
            }

            details += $"Effect:\n{card.SpecialEffect}";

            await DisplayAlert($"📋 {card.Name}", details, "Close");
        }

        private Frame CreateTrapCardView(Card card, bool isPlayerCard)
        {
            var frame = new Frame
            {
                BorderColor = Color.FromArgb("#E91E63"),
                BackgroundColor = Color.FromArgb("#880E4F"),
                CornerRadius = 10,
                Padding = 8,
                Margin = 3,
                WidthRequest = 120,
                HasShadow = true
            };

            frame.BindingContext = card;

            var stackLayout = new VerticalStackLayout
            {
                Spacing = 5
            };

            if (isPlayerCard && card.IsFaceDown)
            {
                var nameLabel = new Label
                {
                    Text = $"🔮 {card.Name}",
                    FontSize = 10,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    LineBreakMode = LineBreakMode.WordWrap,
                    MaxLines = 2,
                    HorizontalOptions = LayoutOptions.Center
                };
                stackLayout.Add(nameLabel);

                var buttonStack = new HorizontalStackLayout
                {
                    Spacing = 5,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                var viewButton = new Button
                {
                    Text = "👁️",
                    FontSize = 10,
                    Padding = 5,
                    BackgroundColor = Color.FromArgb("#3498DB"),
                    TextColor = Colors.White,
                    CornerRadius = 5
                };
                viewButton.Clicked += (s, e) => OnCardDetailsClicked(card);
                buttonStack.Add(viewButton);

                var activateButton = new Button
                {
                    Text = "⚡",
                    FontSize = 10,
                    Padding = 5,
                    BackgroundColor = Color.FromArgb("#E74C3C"),
                    TextColor = Colors.White,
                    CornerRadius = 5
                };
                activateButton.Clicked += (s, e) => OnActivateTrapClicked(card);
                buttonStack.Add(activateButton);

                stackLayout.Add(buttonStack);
            }
            else
            {
                var faceDownLabel = new Label
                {
                    Text = "🎴\nTRAP",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#E91E63"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 20)
                };
                stackLayout.Add(faceDownLabel);
            }

            frame.Content = stackLayout;
            return frame;
        }

        private async void OnActivateTrapClicked(Card trap)
        {
            var gameState = _gameEngine.GetGameState();

            bool confirm = await DisplayAlert(
                "🔮 Activate Trap",
                $"Activate {trap.Name}?\n\n✨ Effect: {trap.SpecialEffect}",
                "✅ Activate",
                "❌ Cancel"
            );

            if (!confirm)
                return;

            // ← ADD SOUND HERE
            await _audioService.PlaySoundEffectAsync("trap_set");

            var result = _effectsEngine.ExecuteCardEffect(trap, gameState.Player1, gameState.Player2);

            gameState.Player1.Field.Remove(trap);
            gameState.Player1.SendToGraveyard(trap);

            ShowMessage($"🔮 {result.Message}");
            UpdateGameUI();
        }

        private Frame CreateFieldCardView(Card card)
        {
            Color borderColor = card.Field switch
            {
                "SCIENCE" => Color.FromArgb("#3498DB"),
                "PHILOSOPHY" => Color.FromArgb("#9B59B6"),
                "SPIRITUALITY" => Color.FromArgb("#F39C12"),
                "HUMANITIES" => Color.FromArgb("#27AE60"),
                _ => Colors.Gray
            };

            var frame = new Frame
            {
                BorderColor = borderColor,
                BackgroundColor = Color.FromArgb("#34495E"),
                CornerRadius = 10,
                Padding = 8,
                Margin = 3,
                WidthRequest = 150,
                HasShadow = true
            };

            frame.BindingContext = card;

            var stackLayout = new VerticalStackLayout
            {
                Spacing = 3
            };

            string tierBadge = card.Tier switch
            {
                "LEGENDARY" => "★★★",
                "MASTER" => "★★",
                "SCHOLAR" => "★",
                _ => ""
            };

            var nameLabel = new Label
            {
                Text = $"{tierBadge} {card.Name}",
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                LineBreakMode = LineBreakMode.WordWrap,
                MaxLines = 2,
                HorizontalOptions = LayoutOptions.Center
            };
            stackLayout.Add(nameLabel);

            string positionIcon = card.BattlePosition == "ATTACK" ? "⚔️" : "🛡️";
            string positionText = card.BattlePosition == "ATTACK" ? "ATTACK" : "DEFENSE";
            Color positionColor = card.BattlePosition == "ATTACK"
                ? Color.FromArgb("#E74C3C")
                : Color.FromArgb("#3498DB");

            var positionLabel = new Label
            {
                Text = $"{positionIcon} {positionText}",
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = positionColor,
                HorizontalOptions = LayoutOptions.Center,
                BackgroundColor = Color.FromArgb("#2C3E50"),
                Padding = new Thickness(5, 2)
            };
            stackLayout.Add(positionLabel);

            string statText = card.BattlePosition == "ATTACK"
                ? $"⚔️ ATK: {card.CurrentATK}"
                : $"🛡️ DEF: {card.CurrentDEF}";

            bool hasBonus = (card.BattlePosition == "ATTACK" && card.CurrentATK != card.ATK) ||
                           (card.BattlePosition == "DEFENSE" && card.CurrentDEF != card.DEF);

            if (hasBonus)
            {
                int bonus = card.BattlePosition == "ATTACK"
                    ? card.CurrentATK - card.ATK
                    : card.CurrentDEF - card.DEF;
                statText += $" (+{bonus})";
            }

            var statsLabel = new Label
            {
                Text = statText,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = hasBonus ? Color.FromArgb("#2ECC71") : Color.FromArgb("#ECF0F1"),
                HorizontalOptions = LayoutOptions.Center
            };
            stackLayout.Add(statsLabel);

            var baseStatsLabel = new Label
            {
                Text = $"Base: {card.ATK}/{card.DEF}",
                FontSize = 8,
                TextColor = Color.FromArgb("#95A5A6"),
                HorizontalOptions = LayoutOptions.Center
            };
            stackLayout.Add(baseStatsLabel);

            var fieldSchoolStack = new HorizontalStackLayout
            {
                Spacing = 5,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 3, 0, 0)
            };

            string fieldIcon = card.Field switch
            {
                "SCIENCE" => "🔬",
                "PHILOSOPHY" => "💭",
                "SPIRITUALITY" => "🙏",
                "HUMANITIES" => "📚",
                _ => "🌐"
            };

            var fieldBadge = new Label
            {
                Text = fieldIcon,
                FontSize = 14,
                BackgroundColor = Color.FromArgb("#2C3E50"),
                Padding = new Thickness(5, 2)
            };
            fieldSchoolStack.Add(fieldBadge);

            stackLayout.Add(fieldSchoolStack);

            var buttonStack = new HorizontalStackLayout
            {
                Spacing = 5,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };

            var attackButton = new Button
            {
                Text = "⚔️",
                FontSize = 10,
                Padding = 5,
                BackgroundColor = Color.FromArgb("#E74C3C"),
                TextColor = Colors.White,
                CornerRadius = 5
            };
            attackButton.Clicked += (s, e) => OnAttackClicked(card);
            buttonStack.Add(attackButton);

            var switchButton = new Button
            {
                Text = "🔄",
                FontSize = 10,
                Padding = 5,
                BackgroundColor = Color.FromArgb("#3498DB"),
                TextColor = Colors.White,
                CornerRadius = 5
            };
            switchButton.Clicked += (s, e) => OnSwitchPositionClicked(card);
            buttonStack.Add(switchButton);

            if (card.EffectTrigger == "MANUAL")
            {
                var effectButton = new Button
                {
                    Text = "✨",
                    FontSize = 10,
                    Padding = 5,
                    BackgroundColor = Color.FromArgb("#9B59B6"),
                    TextColor = Colors.White,
                    CornerRadius = 5
                };
                effectButton.Clicked += (s, e) => OnActivateEffectClicked(card);
                buttonStack.Add(effectButton);
            }

            stackLayout.Add(buttonStack);
            frame.Content = stackLayout;

            return frame;
        }

        private async void OnPlayCardClicked(Card card)
        {
            var gameState = _gameEngine.GetGameState();

            if (gameState.CurrentPhase != "MAIN")
            {
                await DisplayAlert("⚠️ Wrong Phase", "You can only play cards during Main Phase!", "OK");
                return;
            }

            bool success = false;

            if (card.CardType == "CHARACTER")
            {
                var summonResult = await CheckSummonRequirements(card, gameState.CurrentPlayer);

                if (!summonResult.canSummon)
                {
                    await DisplayAlert("⚠️ Cannot Summon", summonResult.reason, "OK");
                    return;
                }

                success = await ExecuteSummon(card, gameState.CurrentPlayer, summonResult);

                if (success)
                {
                    // ← ADD SOUND HERE
                    await _audioService.PlaySoundEffectAsync("summon");

                    bool attackMode = await DisplayAlert(
                        "⚔️ Choose Battle Position",
                        $"Summon {card.Name} in which position?\n\n⚔️ ATTACK: Can attack, uses ATK when defending\n🛡️ DEFENSE: Cannot attack, uses DEF when defending",
                        "⚔️ Attack Mode",
                        "🛡️ Defense Mode"
                    );

                    card.BattlePosition = attackMode ? "ATTACK" : "DEFENSE";

                    string positionIcon = attackMode ? "⚔️" : "🛡️";
                    string positionText = attackMode ? "ATTACK" : "DEFENSE";

                    gameState.CurrentPlayer.SummonsThisTurn++;
                    ShowMessage($"✅ {positionIcon} Summoned {card.Name} in {positionText} mode! ({gameState.CurrentPlayer.SummonsThisTurn}/{gameState.CurrentPlayer.MaxSummonsPerTurn})");
                    _gameEngine.ApplyGraveyardEffects(gameState.CurrentPlayer);
                }
            }
            else if (card.CardType == "DISCOVERY")
            {
                // ← ADD SOUND HERE
                await _audioService.PlaySoundEffectAsync("discovery");

                var result = _effectsEngine.ExecuteCardEffect(card, gameState.CurrentPlayer, gameState.OpponentPlayer);
                gameState.CurrentPlayer.Hand.Remove(card);
                gameState.CurrentPlayer.SendToGraveyard(card);
                ShowMessage($"💚 {result.Message}");
                success = true;
            }
            else if (card.CardType == "PARADOX")
            {
                // ← ADD SOUND HERE
                await _audioService.PlaySoundEffectAsync("trap_set");

                success = _gameEngine.SetParadoxCard(card, gameState.CurrentPlayer);
                if (success)
                    ShowMessage($"🔮 Set {card.Name} face-down!");
            }

            if (success)
            {
                UpdateGameUI();
            }
        }

        private async void OnAttackClicked(Card attackingCard)
        {
            var gameState = _gameEngine.GetGameState();
            var opponent = gameState.OpponentPlayer;

            if (attackingCard.HasAttackedThisTurn)
            {
                await DisplayAlert("⚠️ Already Attacked",
                    $"{attackingCard.Name} has already attacked this turn!", "OK");
                return;
            }

            if (gameState.CurrentTurn == 1)
            {
                await DisplayAlert("⚠️ Cannot Attack",
                    "You cannot attack on the first turn of the game!", "OK");
                return;
            }

            var opponentCharacters = opponent.Field.Where(c => c.CardType == "CHARACTER").ToList();

            if (opponentCharacters.Count == 0)
            {
                // DIRECT ATTACK
                bool confirmDirect = await DisplayAlert(
                    "⚔️ Direct Attack!",
                    $"{attackingCard.Name} will attack directly for {attackingCard.CurrentATK} damage!",
                    "✅ Attack",
                    "❌ Cancel"
                );

                if (!confirmDirect)
                    return;

                await _audioService.PlaySoundEffectAsync("attack");
                await ShowAttackIndicator(attackingCard, null);

                int baseATK = attackingCard.CurrentATK;
                int finalATK = _gameEngine.CalculateFinalATK(attackingCard, null, gameState.CurrentPlayer);
                int atkBonus = finalATK - baseATK;
                string bonusInfo = "";
                if (atkBonus > 0)
                    bonusInfo = $" (+{atkBonus} from synergy!)";

                ShowMessage($"⚔️ {attackingCard.Name}: {baseATK} ATK{bonusInfo} = {finalATK} direct damage!");
                await Task.Delay(1000);

                var result = _gameEngine.ExecuteDirectAttack(attackingCard, gameState.CurrentPlayer, opponent);
                ShowMessage($"🔥 {result.Message}");
            }
            else
            {
                // ATTACK OPPONENT'S CARD
                Card targetCard = await SelectAttackTarget(opponentCharacters, attackingCard);
                if (targetCard == null)
                    return;

                // ✨ NEW: AI TRAP ACTIVATION CHECK
                var aiTraps = opponent.Field.Where(c => c.CardType == "PARADOX").ToList();
                if (aiTraps.Any())
                {
                    ShowMessage($"⚠️ Your {attackingCard.Name} declares attack on {targetCard.Name}!");
                    UpdateGameUI();
                    await Task.Delay(1000);

                    // AI decides if it should activate trap
                    bool shouldActivate = false;
                    Card trapToActivate = null;

                    // SMART DECISION: Activate based on difficulty
                    if (attackingCard.CurrentATK > targetCard.CurrentATK && targetCard.Tier == "LEGENDARY")
                    {
                        // Protecting legendary card
                        shouldActivate = _aiDifficulty switch
                        {
                            AIDifficulty.EASY => _random.Next(100) < 40,    // 40% chance (often forgets)
                            AIDifficulty.NORMAL => _random.Next(100) < 70,  // 70% chance
                            AIDifficulty.HARD => true,                      // Always protects
                            _ => true
                        };
                        trapToActivate = aiTraps.FirstOrDefault();
                    }
                    else if (attackingCard.CurrentATK >= 3000)
                    {
                        // Strong attack incoming
                        int activateChance = _aiDifficulty switch
                        {
                            AIDifficulty.EASY => 20,    // 20% chance (rarely reacts)
                            AIDifficulty.NORMAL => 60,  // 60% chance
                            AIDifficulty.HARD => 90,    // 90% chance (almost always)
                            _ => 60
                        };
                        shouldActivate = _random.Next(100) < activateChance;
                        trapToActivate = aiTraps.FirstOrDefault();
                    }
                    else
                    {
                        // Normal situation
                        int randomChance = _aiDifficulty switch
                        {
                            AIDifficulty.EASY => 10,    // 10% chance (rarely uses)
                            AIDifficulty.NORMAL => 30,  // 30% chance
                            AIDifficulty.HARD => 50,    // 50% chance (uses often)
                            _ => 30
                        };
                        shouldActivate = _random.Next(100) < randomChance;
                        trapToActivate = aiTraps.FirstOrDefault();
                    }

                    if (shouldActivate && trapToActivate != null)
                    {
                        await _audioService.PlaySoundEffectAsync("trap_set");

                        ShowMessage($"🔮 AI ACTIVATED TRAP: {trapToActivate.Name}!");
                        await Task.Delay(1500);

                        var trapResult = _effectsEngine.ExecuteCardEffect(trapToActivate, opponent, gameState.CurrentPlayer);

                        opponent.Field.Remove(trapToActivate);
                        opponent.SendToGraveyard(trapToActivate);

                        ShowMessage($"🔮 {trapResult.Message}");
                        UpdateGameUI();
                        await Task.Delay(1500);

                        // If trap negated attack, stop here
                        if (trapResult.NegatedAttack)
                        {
                            ShowMessage($"🛡️ Your attack was NEGATED by {trapToActivate.Name}!");
                            attackingCard.HasAttackedThisTurn = true;
                            UpdateGameUI();
                            return;
                        }

                        // If trap redirected, choose new target
                        if (trapResult.RedirectTarget && opponentCharacters.Count > 1)
                        {
                            targetCard = opponentCharacters[_random.Next(opponentCharacters.Count)];
                            ShowMessage($"🎯 Attack redirected to {targetCard.Name}!");
                            await Task.Delay(1000);
                        }
                    }
                }

                // ✨ NEW: HIGHLIGHT TARGET CARD
                await HighlightTargetCard(targetCard);
                await Task.Delay(500);

                await _audioService.PlaySoundEffectAsync("attack");

                int baseATK = attackingCard.CurrentATK;
                int finalATK = _gameEngine.CalculateFinalATK(attackingCard, targetCard, gameState.CurrentPlayer);
                int atkBonus = finalATK - baseATK;
                string bonusInfo = "";
                if (atkBonus > 0)
                    bonusInfo = $" (+{atkBonus} bonus!)";
                else if (atkBonus < 0)
                    bonusInfo = $" ({atkBonus} penalty!)";

                ShowMessage($"⚔️ {attackingCard.Name}: {baseATK} ATK{bonusInfo} = {finalATK} total!");
                await Task.Delay(800);

                await ShowAttackIndicator(attackingCard, targetCard);

                var result = _gameEngine.ExecuteBattle(attackingCard, targetCard,
    gameState.CurrentPlayer, opponent);

                // ✨ CRUSH ANIMATION - Handle all destruction cases
                if (result.Message.Contains("MUTUAL DESTRUCTION"))
                {
                    // Both cards destroyed - animate both
                    var task1 = CrushCardAnimation(attackingCard);
                    var task2 = CrushCardAnimation(targetCard);
                    await Task.WhenAll(task1, task2);
                    await _audioService.PlaySoundEffectAsync("destroy");
                }
                else if (result.DestroyedCard != null)
                {
                    // Single card destroyed
                    await CrushCardAnimation(result.DestroyedCard);
                    await _audioService.PlaySoundEffectAsync("destroy");
                }

                ShowMessage($"⚔️ {result.Message}");
            }

            attackingCard.HasAttackedThisTurn = true;
            gameState.CheckVictoryCondition();
            UpdateGameUI();
        }

        private async Task<Card> SelectAttackTarget(List<Card> targets, Card attacker)
        {
            if (targets.Count == 0)
                return null;

            var gameState = _gameEngine.GetGameState();
            var validTargets = targets.ToList();

            if (gameState.OpponentPlayer.Graveyard.Any(c => c.Name == "Nelson Mandela"))
            {
                validTargets = validTargets.Where(t => t.School != "HUMANISM").ToList();

                if (validTargets.Count == 0)
                {
                    await DisplayAlert("🛡️ Mandela's Protection",
                        "All opponent cards are HUMANISM and protected by Nelson Mandela's graveyard effect!",
                        "OK");
                    return null;
                }
            }

            if (validTargets.Count == 1)
            {
                var target = validTargets[0];
                bool confirm = await DisplayAlert(
                    "⚔️ Confirm Attack",
                    $"{attacker.Name} ({attacker.CurrentATK} ATK)\n    VS\n{target.Name} ({(target.BattlePosition == "ATTACK" ? target.CurrentATK + " ATK" : target.CurrentDEF + " DEF")})",
                    "✅ Attack",
                    "❌ Cancel"
                );

                return confirm ? target : null;
            }

            var targetOptions = validTargets.Select(t =>
                $"{t.Name} ({t.BattlePosition}) - " +
                (t.BattlePosition == "ATTACK" ? $"{t.CurrentATK} ATK" : $"{t.CurrentDEF} DEF")
            ).ToArray();

            string choice = await DisplayActionSheet(
                $"⚔️ {attacker.Name} - Choose Target",
                "❌ Cancel",
                null,
                targetOptions
            );

            if (choice == "❌ Cancel" || choice == null)
                return null;

            int selectedIndex = Array.IndexOf(targetOptions, choice);
            if (selectedIndex >= 0 && selectedIndex < validTargets.Count)
            {
                var target = validTargets[selectedIndex];

                bool confirm = await DisplayAlert(
                    "⚔️ Confirm Attack",
                    $"{attacker.Name} ({attacker.CurrentATK} ATK)\n    VS\n{target.Name} ({(target.BattlePosition == "ATTACK" ? target.CurrentATK + " ATK" : target.CurrentDEF + " DEF")})",
                    "✅ Attack",
                    "❌ Cancel"
                );

                return confirm ? target : null;
            }

            return null;
        }

        private async Task<Card> SelectTrapToActivate(List<Card> traps)
        {
            if (traps.Count == 0)
                return null;

            if (traps.Count == 1)
            {
                bool activate = await DisplayAlert(
                    "🔮 Activate Trap?",
                    $"{traps[0].Name}\n\n✨ {traps[0].SpecialEffect}",
                    "✅ Activate",
                    "❌ Cancel"
                );

                return activate ? traps[0] : null;
            }

            string[] trapNames = traps.Select(t => $"{t.Name}").ToArray();

            string choice = await DisplayActionSheet(
                "🔮 Choose Trap to Activate",
                "❌ Cancel",
                null,
                trapNames
            );

            if (choice == "❌ Cancel" || choice == null)
                return null;

            var selectedTrap = traps.FirstOrDefault(t => t.Name == choice);

            if (selectedTrap != null)
            {
                bool confirm = await DisplayAlert(
                    "🔮 Confirm Activation",
                    $"{selectedTrap.Name}\n\n✨ {selectedTrap.SpecialEffect}",
                    "✅ Activate",
                    "❌ Cancel"
                );

                return confirm ? selectedTrap : null;
            }

            return null;
        }

        private async void OnActivateEffectClicked(Card card)
        {
            var gameState = _gameEngine.GetGameState();

            if (card.EffectFrequency == "ONCE_PER_TURN" && card.EffectUsedThisTurn)
            {
                await DisplayAlert("⚠️ Effect Already Used",
                    $"{card.Name}'s effect can only be used once per turn!", "OK");
                return;
            }

            if (card.EffectFrequency == "ONCE_PER_DUEL" && card.EffectUsedThisDuel)
            {
                await DisplayAlert("⚠️ Effect Already Used",
                    $"{card.Name}'s effect can only be used once per duel!", "OK");
                return;
            }

            var result = _effectsEngine.ExecuteCardEffect(card, gameState.CurrentPlayer, gameState.OpponentPlayer);

            card.EffectUsedThisTurn = true;
            card.TimesEffectUsed++;

            if (card.EffectFrequency == "ONCE_PER_DUEL")
                card.EffectUsedThisDuel = true;

            ShowMessage($"✨ {result.Message}");

            _gameEngine.ApplyGraveyardEffects(gameState.CurrentPlayer);

            UpdateGameUI();
        }

        private async void OnNextPhaseClicked(object sender, EventArgs e)
        {
            var gameState = _gameEngine.GetGameState();

            if (gameState.CurrentPhase == "DRAW")
            {
                // ← ADD SOUND HERE
                await _audioService.PlaySoundEffectAsync("phase_change");

                ShowPhaseAnnouncement("DRAW PHASE", "Draw your card!");
                await Task.Delay(1500);

                var drawn = _gameEngine.ExecuteTurn();
                if (drawn.DrawnCard != null)
                {
                    // ← ADD SOUND HERE
                    await _audioService.PlaySoundEffectAsync("draw");

                    ShowMessage($"📥 Drew: {drawn.DrawnCard.Name}");

                    if (gameState.CurrentPlayer.SpecialEffect.Contains("[PRISONER_DILEMMA]"))
                    {
                        if (gameState.CurrentPlayer.Hand.Count > 0)
                        {
                            var discard = gameState.CurrentPlayer.Hand.First();
                            gameState.CurrentPlayer.Hand.Remove(discard);
                            gameState.CurrentPlayer.SendToGraveyard(discard);
                            ShowMessage($"🎲 Prisoner's Dilemma: Discarded {discard.Name}!");
                            await Task.Delay(1000);
                        }
                    }
                }

                gameState.NextPhase();
                UpdateGameUI();
            }
            else if (gameState.CurrentPhase == "MAIN")
            {
                // ← ADD SOUND HERE
                await _audioService.PlaySoundEffectAsync("phase_change");

                ShowPhaseAnnouncement("MAIN PHASE", "Summon monsters and set traps!");
                await Task.Delay(1500);

                gameState.NextPhase();
                UpdateGameUI();
            }
            else if (gameState.CurrentPhase == "BATTLE")
            {
                // ← ADD SOUND HERE
                await _audioService.PlaySoundEffectAsync("phase_change");

                ShowPhaseAnnouncement("BATTLE PHASE", "Declare your attacks!");
                await Task.Delay(1500);

                gameState.NextPhase();
                UpdateGameUI();
            }
            else if (gameState.CurrentPhase == "END")
            {
                await DisplayAlert("ℹ️ End Turn", "Use the 'End Turn' button to finish your turn!", "OK");
            }
        }

        private async void OnEndTurnClicked(object sender, EventArgs e)
        {
            if (_turnTimer != null)
            {
                _turnTimer.StopTimer();
            }

            _gameEngine.EndTurn();
            UpdateGameUI();

            var gameState = _gameEngine.GetGameState();

            if (gameState.CurrentPlayer.PlayerId == 2)
            {
                if (gameState.CurrentPlayer.SpecialEffect.Contains("[SKIP_NEXT_TURN]"))
                {
                    ShowMessage("⏱️ Time Dilation: AI's turn is SKIPPED!");
                    await Task.Delay(2000);

                    gameState.CurrentPlayer.SpecialEffect =
                        gameState.CurrentPlayer.SpecialEffect.Replace(" [SKIP_NEXT_TURN]", "");

                    _gameEngine.EndTurn();
                    UpdateGameUI();

                    ShowPhaseAnnouncement("YOUR TURN", "Get ready to draw!");
                    await Task.Delay(1500);

                    gameState.NextPhase();
                    UpdateGameUI();
                    ShowPhaseAnnouncement("DRAW PHASE", "Draw your card!");

                    return;
                }

                NextPhaseButton.IsEnabled = false;
                EndTurnButton.IsEnabled = false;

                ShowPhaseAnnouncement("OPPONENT'S TURN", "AI is thinking...");
                await Task.Delay(1500);

                ShowMessage("🤖 AI Opponent is thinking...");
                await Task.Delay(1000);

                ShowPhaseAnnouncement("AI DRAW PHASE", "Opponent is drawing...");
                await Task.Delay(1500);

                gameState.NextPhase();
                UpdateGameUI();
                ShowMessage("🤖 AI Draw Phase...");
                await Task.Delay(800);

                var drawn = _gameEngine.ExecuteTurn();
                if (drawn.DrawnCard != null)
                {
                    ShowMessage($"🤖 AI drew {drawn.DrawnCard.Name}");
                    await Task.Delay(1000);
                }

                ShowPhaseAnnouncement("AI MAIN PHASE", "Opponent is playing cards...");
                await Task.Delay(1500);

                gameState.NextPhase();
                UpdateGameUI();
                ShowMessage("🤖 AI Main Phase - Playing cards...");
                await Task.Delay(1000);

                string playLog = await AIPlayCards();
                ShowMessage(playLog);
                await Task.Delay(1500);

                ShowPhaseAnnouncement("AI BATTLE PHASE", "Opponent is attacking!");
                await Task.Delay(1500);

                gameState.NextPhase();
                UpdateGameUI();
                ShowMessage("🤖 AI Battle Phase - Attacking...");
                await Task.Delay(1000);

                string battleLog = await AIBattle();
                ShowMessage(battleLog);
                await Task.Delay(1500);

                gameState.NextPhase();
                _gameEngine.EndTurn();
                UpdateGameUI();
                ShowMessage("🤖 AI ended turn. Your turn begins!");

                NextPhaseButton.IsEnabled = true;
                EndTurnButton.IsEnabled = true;

                await Task.Delay(1000);

                ShowPhaseAnnouncement("YOUR TURN", "Get ready to draw!");
                await Task.Delay(1500);

                ShowMessage("🎮 Your turn! Draw Phase starting...");
                await Task.Delay(500);

                gameState.NextPhase();
                UpdateGameUI();

                ShowPhaseAnnouncement("DRAW PHASE", "Draw your card!");

                if (_turnTimer != null && _turnTimer.IsEnabled)
                {
                    _turnTimer.StartTimer();
                }
            }
            else
            {
                ShowMessage("🔄 Your turn begins!");
            }
        }

        private async void OnTimerSettingsClicked(object sender, EventArgs e)
        {
            if (_turnTimer == null)
            {
                await DisplayAlert("⏱️ Timer Not Available", "Timer is currently disabled", "OK");
                return;
            }

            string currentStatus = _turnTimer.IsEnabled ? "ENABLED" : "DISABLED";
            string currentTime = _turnTimer.TimeLimit == 999999 ? "UNLIMITED" : $"{_turnTimer.TimeLimit} seconds";

            string choice = await DisplayActionSheet(
                $"⏱️ Turn Timer Settings\n\nStatus: {currentStatus}\nTime Limit: {currentTime}",
                "Cancel",
                null,
                "⏱️ 30 Seconds",
                "⏱️ 60 Seconds (Default)",
                "⏱️ 90 Seconds",
                "♾️ Unlimited (No Timer)",
                _turnTimer.IsEnabled ? "❌ Disable Timer" : "✅ Enable Timer"
            );

            if (choice == null || choice == "Cancel")
                return;

            switch (choice)
            {
                case "⏱️ 30 Seconds":
                    _turnTimer.SetTimeLimit(30);
                    _turnTimer.IsEnabled = true;
                    await DisplayAlert("⏱️ Timer Set", "Turn timer: 30 seconds", "OK");
                    break;

                case "⏱️ 60 Seconds (Default)":
                    _turnTimer.SetTimeLimit(60);
                    _turnTimer.IsEnabled = true;
                    await DisplayAlert("⏱️ Timer Set", "Turn timer: 60 seconds", "OK");
                    break;

                case "⏱️ 90 Seconds":
                    _turnTimer.SetTimeLimit(90);
                    _turnTimer.IsEnabled = true;
                    await DisplayAlert("⏱️ Timer Set", "Turn timer: 90 seconds", "OK");
                    break;

                case "♾️ Unlimited (No Timer)":
                    _turnTimer.IsEnabled = false;
                    TimerLabel.IsVisible = false;
                    await DisplayAlert("⏱️ Timer Disabled", "No time limit on turns", "OK");
                    break;

                case "❌ Disable Timer":
                    _turnTimer.IsEnabled = false;
                    _turnTimer.StopTimer();
                    TimerLabel.IsVisible = false;
                    await DisplayAlert("⏱️ Timer Disabled", "Turn timer is now OFF", "OK");
                    break;

                case "✅ Enable Timer":
                    _turnTimer.IsEnabled = true;
                    await DisplayAlert("⏱️ Timer Enabled", $"Turn timer is now ON ({_turnTimer.TimeLimit}s)", "OK");
                    break;
            }

            UpdateGameUI();
        }

        private async Task<string> AIPlayCards()
        {
            var gameState = _gameEngine.GetGameState();
            var ai = gameState.Player2;
            string log = "";

            var charactersInHand = ai.Hand.Where(c => c.CardType == "CHARACTER").ToList();

            charactersInHand = charactersInHand.OrderByDescending(c =>
            {
                int synergy = 0;
                synergy += ai.Field.Count(f => f.Field == c.Field) * 10;
                synergy += ai.Field.Count(f => f.School == c.School) * 5;
                if (c.Tier == "LEGENDARY") synergy += 20;
                if (c.ATK >= 2500) synergy += 10;
                return synergy;
            }).Take(3).ToList();

            foreach (var card in charactersInHand)
            {
                if (ai.SummonsThisTurn >= 2) break;

                if (_gameEngine.PlayCharacterCard(card, ai))
                {
                    ai.SummonsThisTurn++;
                    card.BattlePosition = ChooseSmartPosition(card);
                    string posIcon = card.BattlePosition == "ATTACK" ? "⚔️" : "🛡️";

                    log += $"▶️ AI played {card.Name} ({posIcon} {card.BattlePosition})\n";
                    UpdateGameUI();
                    await Task.Delay(700);
                }
            }

            // ✨ NEW: SMART POSITION SWITCHING
            var playerAttackers = gameState.Player1.Field.Where(c => c.CardType == "CHARACTER").ToList();
            if (playerAttackers.Any())
            {
                int strongestPlayerATK = playerAttackers.Max(c => c.CurrentATK);

                // Check each AI card and switch weak ones to defense
                foreach (var aiCard in ai.Field.Where(c => c.CardType == "CHARACTER").ToList())
                {
                    // If AI card is weak compared to player's strongest card
                    if (aiCard.BattlePosition == "ATTACK" && aiCard.CurrentATK < strongestPlayerATK)
                    {
                        // Switch to defense if DEF is better than ATK
                        if (aiCard.CurrentDEF > aiCard.CurrentATK)
                        {
                            aiCard.BattlePosition = "DEFENSE";
                            log += $"🛡️ AI switched {aiCard.Name} to DEFENSE mode (outmatched by your {strongestPlayerATK} ATK card)\n";
                            UpdateGameUI();
                            await Task.Delay(500);
                        }
                    }
                }
            }

            var effectCards = ai.Field.Where(c =>
                c.CardType == "CHARACTER" &&
                c.EffectTrigger == "MANUAL" &&
                !c.EffectUsedThisTurn
            ).ToList();

            if (effectCards.Any() && _random.Next(100) < 40)
            {
                var effectCard = effectCards[_random.Next(effectCards.Count)];
                var result = _effectsEngine.ExecuteCardEffect(effectCard, ai, gameState.Player1);
                effectCard.EffectUsedThisTurn = true;

                log += $"🎯 AI activated {effectCard.Name}'s effect!\n";
                log += $"   {result.Message}\n";

                UpdateGameUI();
                await Task.Delay(700);
            }

            // ✨ SMART DISCOVERY CARD USAGE
            var discoveryCards = ai.Hand.Where(c => c.CardType == "DISCOVERY").ToList();
            if (discoveryCards.Any())
            {
                Card bestDiscovery = null;

                // Prioritize based on situation
                if (ai.Hand.Count <= 2)
                {
                    // Low hand? Use draw cards first!
                    bestDiscovery = discoveryCards.FirstOrDefault(d =>
                        d.SpecialEffect.Contains("draw", StringComparison.OrdinalIgnoreCase));
                }
                else if (ai.Field.Count(c => c.CardType == "CHARACTER") >= 2)
                {
                    // Have monsters? Use boost cards!
                    bestDiscovery = discoveryCards.FirstOrDefault(d =>
                        d.SpecialEffect.Contains("ATK", StringComparison.OrdinalIgnoreCase) ||
                        d.SpecialEffect.Contains("DEF", StringComparison.OrdinalIgnoreCase));
                }

                // If no specific card found, pick based on difficulty
                if (bestDiscovery == null)
                {
                    int useChance = _aiDifficulty switch
                    {
                        AIDifficulty.EASY => 30,    // 30% chance (rarely uses cards)
                        AIDifficulty.NORMAL => 60,  // 60% chance
                        AIDifficulty.HARD => 90,    // 90% chance (almost always uses)
                        _ => 60
                    };

                    if (_random.Next(100) < useChance)
                    {
                        bestDiscovery = discoveryCards[_random.Next(discoveryCards.Count)];
                    }
                }

                if (bestDiscovery != null)
                {
                    await _audioService.PlaySoundEffectAsync("discovery");

                    var result = _effectsEngine.ExecuteCardEffect(bestDiscovery, ai, gameState.Player1);
                    ai.Hand.Remove(bestDiscovery);
                    ai.SendToGraveyard(bestDiscovery);

                    log += $"🔬 AI strategically used {bestDiscovery.Name}!\n";
                    log += $"   {result.Message}\n";

                    UpdateGameUI();
                    await Task.Delay(700);
                }
            }

            // PLAY PARADOX CARDS
            var paradoxCards = ai.Hand.Where(c => c.CardType == "PARADOX").ToList();
            int aiAttackers = ai.Field.Count(c => c.CardType == "CHARACTER");

            if (paradoxCards.Any() && aiAttackers >= 2 && _random.Next(100) < 60)
            {
                var paradox = paradoxCards.First();
                if (_gameEngine.SetParadoxCard(paradox, ai))
                {
                    log += $"🔮 AI set {paradox.Name} face-down (protecting {aiAttackers} monsters)\n";
                    UpdateGameUI();
                    await Task.Delay(400);
                }
            }

            if (string.IsNullOrEmpty(log))
                log = "AI has no cards to play\n";

            return log;
        }

        private async Task<string> AIBattle()
        {
            var gameState = _gameEngine.GetGameState();
            var ai = gameState.Player2;
            var player = gameState.Player1;
            string log = "";

            if (gameState.CurrentTurn == 1)
            {
                return "AI cannot attack on first turn\n";
            }

            var aiAttackers = ai.Field.Where(c => c.CardType == "CHARACTER").ToList();

            if (aiAttackers.Count == 0)
                return "AI has no attackers\n";

            foreach (var attacker in aiAttackers)
            {
                var playerDefenders = player.Field.Where(c => c.CardType == "CHARACTER").ToList();
                var playerTraps = player.Field.Where(c => c.CardType == "PARADOX").ToList();

                if (playerTraps.Count > 0)
                {
                    ShowMessage($"⚠️ AI's {attacker.Name} declares attack!");
                    UpdateGameUI();
                    await Task.Delay(1000);

                    bool wantsToActivate = await DisplayAlert(
                        "🔮 TRAP ACTIVATION?",
                        $"⚠️ AI's {attacker.Name} ({attacker.CurrentATK} ATK) is attacking!\n\n🔮 You have {playerTraps.Count} trap(s) set!\n\nActivate a trap?",
                        "🔮 YES - Activate Trap",
                        "❌ NO - Let Attack Continue"
                    );

                    if (wantsToActivate)
                    {
                        var selectedTrap = await SelectTrapToActivate(playerTraps);

                        if (selectedTrap != null)
                        {
                            // ← ADD SOUND HERE
                            await _audioService.PlaySoundEffectAsync("trap_set");

                            var trapResult = _effectsEngine.ExecuteCardEffect(selectedTrap, player, ai);

                            player.Field.Remove(selectedTrap);
                            player.SendToGraveyard(selectedTrap);

                            log += $"🔮 YOU ACTIVATED: {selectedTrap.Name}!\n";
                            log += $"   {trapResult.Message}\n";

                            ShowMessage(log);
                            UpdateGameUI();
                            await Task.Delay(1500);

                            if (trapResult.NegatedAttack)
                            {
                                log += $"🛡️ {attacker.Name}'s attack was NEGATED!\n";
                                ShowMessage(log);
                                await Task.Delay(1000);
                                attacker.HasAttackedThisTurn = true;
                                continue;
                            }

                            if (trapResult.RedirectTarget && playerDefenders.Count > 1)
                            {
                                var newTarget = playerDefenders[_random.Next(playerDefenders.Count)];
                                log += $"🎯 Attack redirected to {newTarget.Name}!\n";
                                ShowMessage(log);
                                await Task.Delay(1000);

                                var redirectResult = _gameEngine.ExecuteBattle(attacker, newTarget, ai, player);
                                log += $"⚔️ {redirectResult.Message}\n";

                                if (redirectResult.DestroyedCard != null)
                                {
                                    await _audioService.PlaySoundEffectAsync("destroy");
                                }

                                ShowMessage(log);
                                UpdateGameUI();
                                await Task.Delay(1000);

                                gameState.CheckVictoryCondition();
                                if (gameState.IsGameOver) break;

                                continue;
                            }

                            if (trapResult.SkipNextTurn)
                            {
                                gameState.CurrentPlayer.SpecialEffect += " [SKIP_NEXT_TURN]";
                                log += $"⏱️ AI's next turn will be SKIPPED!\n";
                                ShowMessage(log);
                                await Task.Delay(1000);
                            }
                        }
                    }
                }

                if (playerDefenders.Count == 0)
                {
                    // ← ADD SOUND HERE
                    await _audioService.PlaySoundEffectAsync("attack");

                    ShowMessage($"🎯 AI's {attacker.Name} targets YOU directly!");
                    UpdateGameUI();
                    await Task.Delay(1200);

                    await ShowAttackIndicator(attacker, null);

                    var result = _gameEngine.ExecuteDirectAttack(attacker, ai, player);
                    log += $"⚔️ {attacker.Name} → {result.Damage} damage!\n";
                }
                else
                {
                    var target = ChooseSmartTarget(playerDefenders, attacker);

                    // ✨ NEW: CALCULATE IF ATTACK WILL WIN
                    int attackerFinalATK = _gameEngine.CalculateFinalATK(attacker, target, ai);
                    int targetValue = target.BattlePosition == "ATTACK"
                        ? target.CurrentATK
                        : target.CurrentDEF;

                    // ✨ SMART DECISION: Don't attack if we'll lose! (Based on difficulty)
                    bool willLose = attackerFinalATK <= targetValue && target.BattlePosition == "ATTACK";

                    if (willLose)
                    {
                        // Difficulty affects decision making
                        bool attackAnyway = _aiDifficulty switch
                        {
                            AIDifficulty.EASY => _random.Next(100) < 60,    // 60% chance to attack anyway (dumb)
                            AIDifficulty.NORMAL => _random.Next(100) < 20,  // 20% chance (sometimes mistakes)
                            AIDifficulty.HARD => false,                     // Never attacks when losing
                            _ => false
                        };

                        if (!attackAnyway)
                        {
                            log += $"🤔 AI's {attacker.Name} ({attackerFinalATK} ATK) decided NOT to attack {target.Name} ({targetValue} ATK) - too risky!\n";
                            continue; // Skip this attack
                        }
                        else
                        {
                            log += $"😵 AI's {attacker.Name} recklessly attacks {target.Name} (bad decision)!\n";
                            // Continue with attack (AI makes mistake)
                        }
                    }

                    // Attack is safe or beneficial - proceed!
                    await _audioService.PlaySoundEffectAsync("attack");

                    int baseATK = attacker.CurrentATK;
                    int finalATK = _gameEngine.CalculateFinalATK(attacker, target, ai);
                    int atkBonus = finalATK - baseATK;

                    string bonusInfo = atkBonus > 0 ? $" +{atkBonus}" : "";
                    string targetPos = target.BattlePosition == "ATTACK" ? $"{target.CurrentATK} ATK" : $"{target.CurrentDEF} DEF";

                    ShowMessage($"🎯 AI's {attacker.Name} ({baseATK}{bonusInfo} = {finalATK} ATK) targeting {target.Name} ({targetPos})!");
                    UpdateGameUI();
                    await Task.Delay(1500);

                    await ShowAttackIndicator(attacker, target);

                    var result = _gameEngine.ExecuteBattle(attacker, target, ai, player);

                    // ✨ CRUSH ANIMATION - Handle all destruction cases
                    if (result.Message.Contains("MUTUAL DESTRUCTION"))
                    {
                        // Both cards destroyed - animate both
                        var task1 = CrushCardAnimation(attacker);
                        var task2 = CrushCardAnimation(target);
                        await Task.WhenAll(task1, task2);
                        await _audioService.PlaySoundEffectAsync("destroy");
                    }
                    else if (result.DestroyedCard != null)
                    {
                        // Single card destroyed - CRUSH IT!
                        await CrushCardAnimation(result.DestroyedCard);
                        await _audioService.PlaySoundEffectAsync("destroy");
                    }

                    log += $"⚔️ {result.Message}\n";
                }

                UpdateGameUI();
                await Task.Delay(1000);

                gameState.CheckVictoryCondition();
                if (gameState.IsGameOver) break;
            }

            return log;
        }

        private void ShowMessage(string message)
        {
            MessageLabel.Text = message;

            Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                if (MessageLabel.Text == message)
                    MessageLabel.Text = "";
                return false;
            });
        }

        private async void ShowGameOver(Player winner)
        {
            // ✨ PREVENT MULTIPLE CALLS
            if (_gameOverShown) return;
            _gameOverShown = true;

            bool playerWon = winner.PlayerId == 1;

            // ← ADD SOUND HERE
            if (playerWon)
                await _audioService.PlaySoundEffectAsync("victory");
            else
                await _audioService.PlaySoundEffectAsync("defeat");

            string statsText = "";
            if (_statsService != null)
            {
                if (playerWon)
                    await _statsService.RecordWin();
                else
                    await _statsService.RecordLoss();

                var stats = _statsService.GetStats();
                string winRate = _statsService.GetWinRate().ToString("F1");
                statsText = $"\n\n📊 YOUR STATS:\nWins: {stats.TotalWins} | Losses: {stats.TotalLosses}\nWin Rate: {winRate}%";
            }

            string message = playerWon
                ? $"🎉 VICTORY! 🎉\n\nYou defeated the AI opponent!{statsText}"
                : $"💀 DEFEAT 💀\n\nThe AI opponent has won!{statsText}";

            bool playAgain = await DisplayAlert(
                playerWon ? "🏆 YOU WIN!" : "☠️ GAME OVER",
                message,
                "🔄 Play Again",
                "❌ Exit"
            );

            if (playAgain)
            {
                if (_turnTimer != null)
                {
                    _turnTimer.StopTimer();
                }

                InitializeGame();
            }
        }

        private async void OnStatsClicked(object sender, EventArgs e)
        {
            if (_statsService == null)
            {
                await DisplayAlert("📊 Stats Not Available", "Statistics tracking is currently disabled", "OK");
                return;
            }

            string stats = _statsService.GetStatsDisplay();

            bool resetStats = await DisplayAlert(
                "📊 Your Statistics",
                stats,
                "🗑️ Reset Stats",
                "✅ Close"
            );

            if (resetStats)
            {
                bool confirmReset = await DisplayAlert(
                    "⚠️ Reset Statistics?",
                    "This will delete all your win/loss records. Are you sure?",
                    "Yes, Reset",
                    "Cancel"
                );

                if (confirmReset)
                {
                    await _statsService.ResetStats();
                    await DisplayAlert("✅ Reset Complete", "Your statistics have been reset!", "OK");
                }
            }
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

        private async Task<(bool canSummon, string reason)> CheckSummonRequirements(Card card, Player player)
        {
            if (player.SummonsThisTurn >= player.MaxSummonsPerTurn)
            {
                return (false, $"You can only summon {player.MaxSummonsPerTurn} cards per turn!");
            }

            if (card.Tier == "SCHOLAR")
            {
                return (true, "");
            }

            if (card.Tier == "MASTER")
            {
                if (player.HP <= 1000)
                {
                    return (false, "Not enough HP! Need at least 1001 HP to perform Alchemy!");
                }
                return (true, "");
            }

            if (card.Tier == "LEGENDARY")
            {
                var tributeCandidates = player.Field.Where(c => c.CardType == "CHARACTER").ToList();

                if (tributeCandidates.Count >= 2)
                {
                    return (true, "");
                }
                else if (player.HP > 3000)
                {
                    return (true, "");
                }
                else
                {
                    return (false, "Ritual requires 2 cards to tribute OR 3000 HP to sacrifice!");
                }
            }

            return (true, "");
        }

        private async Task<bool> ExecuteSummon(Card card, Player player, (bool canSummon, string reason) requirements)
        {
            var gameState = _gameEngine.GetGameState();
            bool success = false;

            if (card.Tier == "SCHOLAR")
            {
                success = _gameEngine.PlayCharacterCard(card, player);
            }
            else if (card.Tier == "MASTER")
            {
                bool confirmed = await DisplayAlert(
                    "⚗️ Alchemy Ritual",
                    $"Summon {card.Name} ({card.ATK}/{card.DEF})?\n\n💔 Cost: 1000 HP\n💚 Your HP: {player.HP}",
                    "✅ Summon",
                    "❌ Cancel"
                );
                if (!confirmed)
                    return false;

                player.TakeDamage(1000);
                ShowMessage($"⚗️ Alchemy! Sacrificed 1000 HP to summon {card.Name}!");
                success = _gameEngine.PlayCharacterCard(card, player);
            }
            else if (card.Tier == "LEGENDARY")
            {
                var tributeCandidates = player.Field.Where(c => c.CardType == "CHARACTER").ToList();

                if (tributeCandidates.Count >= 2)
                {
                    bool useTribute = await DisplayAlert(
                        "🌟 Legendary Ritual",
                        $"Summon {card.Name} ({card.ATK}/{card.DEF})?\n\nChoose summoning method:",
                        "⚰️ Tribute 2 Cards",
                        "💔 Pay 3000 HP"
                    );

                    if (useTribute)
                    {
                        var tribute1 = tributeCandidates[0];
                        var tribute2 = tributeCandidates[1];

                        bool confirmTribute = await DisplayAlert(
                            "⚰️ Confirm Tribute",
                            $"Tribute:\n• {tribute1.Name}\n• {tribute2.Name}\n\nto summon {card.Name}?",
                            "✅ Yes",
                            "❌ No"
                        );

                        if (!confirmTribute)
                            return false;

                        player.SendToGraveyard(tribute1);
                        player.SendToGraveyard(tribute2);
                        ShowMessage($"⚰️ Ritual! Tributed {tribute1.Name} and {tribute2.Name}!");
                    }
                    else
                    {
                        if (player.HP <= 3000)
                        {
                            await DisplayAlert("⚠️ Not Enough HP", "You need more than 3000 HP!", "OK");
                            return false;
                        }

                        player.TakeDamage(3000);
                        ShowMessage($"💔 Ritual! Sacrificed 3000 HP to summon {card.Name}!");
                    }
                }
                else if (player.HP > 3000)
                {
                    bool confirmed = await DisplayAlert(
                        "🌟 Legendary Ritual",
                        $"Summon {card.Name} ({card.ATK}/{card.DEF})?\n\n💔 Cost: 3000 HP\n💚 Your HP: {player.HP}",
                        "✅ Summon",
                        "❌ Cancel"
                    );

                    if (!confirmed)
                        return false;

                    player.TakeDamage(3000);
                    ShowMessage($"💔 Ritual! Sacrificed 3000 HP to summon {card.Name}!");
                }
                else
                {
                    return false;
                }

                success = _gameEngine.PlayCharacterCard(card, player);
            }

            if (success && card.EffectTrigger == "ON_SUMMON")
            {
                await Task.Delay(300);

                var result = _effectsEngine.ExecuteCardEffect(card, player, gameState.OpponentPlayer);
                card.EffectUsedThisTurn = true;

                await DisplayAlert("✨ Summon Effect!", result.Message, "OK");

                _gameEngine.ApplyGraveyardEffects(player);
                UpdateGameUI();
            }

            return success;
        }

        private void OnSwitchPositionClicked(Card card)
        {
            card.BattlePosition = card.BattlePosition == "ATTACK" ? "DEFENSE" : "ATTACK";

            string newPosition = card.BattlePosition;
            string icon = newPosition == "ATTACK" ? "⚔️" : "🛡️";

            ShowMessage($"{icon} {card.Name} switched to {newPosition} mode!");
            UpdateGameUI();
        }

        private async void OnPlayerDeckTapped(object sender, EventArgs e)
        {
            if (!_isInitialized || _gameEngine == null)
            {
                await DisplayAlert("⏳ Please Wait", "Game is still initializing...", "OK");
                return;
            }
            var gameState = _gameEngine.GetGameState();

            if (gameState.CurrentPhase != "DRAW")
            {
                await DisplayAlert("ℹ️ Info",
                    $"Your deck has {gameState.CurrentPlayer.Deck.Count} cards remaining.", "OK");
                return;
            }

            var drawn = _gameEngine.ExecuteTurn();
            if (drawn.DrawnCard != null)
            {
                // ← ADD SOUND HERE
                await _audioService.PlaySoundEffectAsync("draw");

                ShowMessage($"📥 Drew: {drawn.DrawnCard.Name}");
                gameState.NextPhase();
                UpdateGameUI();
            }
        }

        private async void OnPlayerGraveyardTapped(object sender, EventArgs e)
        {
            var gameState = _gameEngine.GetGameState();
            var graveyard = gameState.Player1.Graveyard;

            if (graveyard.Count == 0)
            {
                await DisplayAlert("⚰️ Graveyard", "Your graveyard is empty.", "OK");
                return;
            }

            string graveyardList = "⚰️ YOUR GRAVEYARD:\n\n";
            foreach (var card in graveyard)
            {
                if (card.CardType == "CHARACTER")
                    graveyardList += $"• {card.Name} ({card.Tier}) - {card.ATK}/{card.DEF}\n";
                else
                    graveyardList += $"• {card.Name} ({card.CardType})\n";
            }

            graveyardList += $"\n💀 Total: {graveyard.Count} cards";

            await DisplayAlert("⚰️ Your Graveyard", graveyardList, "Close");
        }

        private async void OnOpponentGraveyardTapped(object sender, EventArgs e)
        {
            var gameState = _gameEngine.GetGameState();
            var graveyard = gameState.Player2.Graveyard;

            if (graveyard.Count == 0)
            {
                await DisplayAlert("⚰️ Graveyard", "Opponent's graveyard is empty.", "OK");
                return;
            }

            string graveyardList = "⚰️ OPPONENT'S GRAVEYARD:\n\n";
            foreach (var card in graveyard)
            {
                if (card.CardType == "CHARACTER")
                    graveyardList += $"• {card.Name} ({card.Tier}) - {card.ATK}/{card.DEF}\n";
                else
                    graveyardList += $"• {card.Name} ({card.CardType})\n";
            }

            graveyardList += $"\n💀 Total: {graveyard.Count} cards";

            await DisplayAlert("⚰️ Opponent's Graveyard", graveyardList, "Close");
        }

        private async void ShowCardDetails(Card card)
        {
            string details = "";

            details += $"📜 {card.Name}\n";
            details += $"━━━━━━━━━━━━━━━━━\n\n";

            if (card.CardType == "CHARACTER")
            {
                details += $"🎖️ Tier: {card.Tier}\n";
                details += $"🌐 Field: {card.Field}\n";
                details += $"🏛️ School: {card.School}\n";
                details += $"⚔️ ATK: {card.ATK}\n";
                details += $"🛡️ DEF: {card.DEF}\n\n";
            }
            else
            {
                details += $"📦 Type: {card.CardType}\n";
                details += $"⚡ Effect Type: {card.EffectType}\n\n";
            }

            details += $"✨ EFFECT:\n{card.SpecialEffect}";

            await DisplayAlert($"📜 {card.Name}", details, "Close");
        }

        private string GetActiveFieldBonuses(Player player)
        {
            List<string> bonuses = new List<string>();

            if (player.CountFieldCards("SCIENCE") >= 2)
                bonuses.Add($"🔬 SCIENCE SYNERGY: +200 ATK ({player.CountFieldCards("SCIENCE")} cards)");

            if (player.CountFieldCards("PHILOSOPHY") >= 2)
                bonuses.Add($"💭 PHILOSOPHY SYNERGY: +200 ATK ({player.CountFieldCards("PHILOSOPHY")} cards)");

            if (player.CountFieldCards("SPIRITUALITY") >= 2)
                bonuses.Add($"🙏 SPIRITUALITY SYNERGY: +200 DEF ({player.CountFieldCards("SPIRITUALITY")} cards)");

            if (player.CountFieldCards("HUMANITIES") >= 2)
                bonuses.Add($"📚 HUMANITIES SYNERGY: +200 ATK/DEF ({player.CountFieldCards("HUMANITIES")} cards)");

            var graveyardEffects = _gameEngine.GetActiveGraveyardEffects(player);
            bonuses.AddRange(graveyardEffects);

            return bonuses.Count > 0 ? string.Join(" | ", bonuses) : "";
        }

        private string ChooseSmartPosition(Card card)
        {
            if (card.ATK >= 2500)
                return "ATTACK";

            if (card.DEF > card.ATK + 300)
                return "DEFENSE";

            if (card.ATK >= card.DEF)
                return "ATTACK";

            return "DEFENSE";
        }

        private Card ChooseSmartTarget(List<Card> targets, Card attacker)
        {
            var legendaries = targets.Where(t => t.Tier == "LEGENDARY").ToList();
            if (legendaries.Any())
            {
                var destroyable = legendaries.Where(t =>
                    (t.BattlePosition == "ATTACK" && attacker.CurrentATK > t.CurrentATK) ||
                    (t.BattlePosition == "DEFENSE" && attacker.CurrentATK > t.CurrentDEF)
                ).OrderBy(t => t.BattlePosition == "ATTACK" ? t.CurrentATK : t.CurrentDEF);

                if (destroyable.Any())
                    return destroyable.First();
            }

            var attackMode = targets.Where(t => t.BattlePosition == "ATTACK").ToList();
            if (attackMode.Any())
            {
                var destroyable = attackMode.Where(t => attacker.CurrentATK > t.CurrentATK)
                    .OrderByDescending(t => t.CurrentATK);

                if (destroyable.Any())
                    return destroyable.First();
            }

            var defenseMode = targets.Where(t => t.BattlePosition == "DEFENSE").ToList();
            if (defenseMode.Any())
            {
                return defenseMode.OrderBy(t => t.CurrentDEF).First();
            }

            return targets.OrderBy(t => t.BattlePosition == "ATTACK" ? t.CurrentATK : t.CurrentDEF).First();
        }

        private async void OnDeckStatsRequested()
        {
            var gameState = _gameEngine.GetGameState();
            var player = gameState.Player1;

            var stats = $"📊 YOUR DECK STATISTICS:\n\n";
            stats += $"🎴 Total Cards: {player.Deck.Count + player.Hand.Count + player.Field.Count + player.Graveyard.Count}\n";
            stats += $"📦 Deck: {player.Deck.Count}\n";
            stats += $"🤚 Hand: {player.Hand.Count}\n";
            stats += $"⚔️ Field: {player.Field.Count(c => c.CardType == "CHARACTER")}\n";
            stats += $"🔮 Traps: {player.Field.Count(c => c.CardType == "PARADOX")}\n";
            stats += $"⚰️ Graveyard: {player.Graveyard.Count}\n\n";

            if (player.Field.Any())
            {
                stats += $"🌐 FIELD COMPOSITION:\n";
                stats += $"🔬 SCIENCE: {player.CountFieldCards("SCIENCE")}\n";
                stats += $"💭 PHILOSOPHY: {player.CountFieldCards("PHILOSOPHY")}\n";
                stats += $"🙏 SPIRITUALITY: {player.CountFieldCards("SPIRITUALITY")}\n";
                stats += $"📚 HUMANITIES: {player.CountFieldCards("HUMANITIES")}\n";
            }

            await DisplayAlert("📊 Deck Stats", stats, "Close");
        }

        private async void OnRestartClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "🔄 Restart Game?",
                "Are you sure? Current progress will be lost!",
                "✅ Yes, Restart",
                "❌ Cancel"
            );

            if (confirm)
            {
                InitializeGame();
            }
        }

        private async void ShowPhaseAnnouncement(string phaseName, string subtitle)
        {
            PhaseAnnouncementLabel.Text = phaseName;
            PhaseSubtitleLabel.Text = subtitle;
            PhaseAnnouncementOverlay.IsVisible = true;

            await Task.Delay(1500);

            PhaseAnnouncementOverlay.IsVisible = false;
        }

        private async Task ShowAttackIndicator(Card attacker, Card target = null)
        {
            AttackIndicatorOverlay.IsVisible = true;

            string message = target != null
                ? $"⚔️ {attacker.Name} → {target.Name}!"
                : $"⚔️ {attacker.Name} → DIRECT ATTACK!";

            ShowMessage(message);

            await Task.Delay(1200);

            AttackIndicatorOverlay.IsVisible = false;
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Stop background music when leaving game
            _audioService?.StopBackgroundMusic();
        }

        // ✨ METHOD 1: HIGHLIGHT TARGET CARD
        private async Task HighlightTargetCard(Card targetCard)
        {
            Frame cardFrame = FindCardFrame(targetCard);
            if (cardFrame == null) return;

            var originalBorderColor = cardFrame.BorderColor;
            var originalBackgroundColor = cardFrame.BackgroundColor;

            // Flash red 3 times to show target
            for (int i = 0; i < 3; i++)
            {
                cardFrame.BorderColor = Colors.Red;
                cardFrame.BackgroundColor = Color.FromArgb("#FF0000").WithAlpha(0.3f);
                await Task.Delay(150);

                cardFrame.BorderColor = originalBorderColor;
                cardFrame.BackgroundColor = originalBackgroundColor;
                await Task.Delay(150);
            }
        }

        // ✨ METHOD 2: FIND CARD FRAME IN UI
        private Frame FindCardFrame(Card card)
        {
            // Search through entire page recursively
            return FindCardFrameRecursive(this.Content, card);
        }

        private Frame FindCardFrameRecursive(IView element, Card card)
        {
            if (element == null) return null;

            // Check if this element is the card frame we're looking for
            if (element is Frame frame && frame.BindingContext == card)
                return frame;

            // Check if element has children (Layout, Grid, StackLayout, etc.)
            if (element is Layout layout)
            {
                foreach (var child in layout.Children)
                {
                    var result = FindCardFrameRecursive(child, card);
                    if (result != null) return result;
                }
            }

            // Check ScrollView
            if (element is ScrollView scrollView)
            {
                return FindCardFrameRecursive(scrollView.Content, card);
            }

            return null;
        }

        // ✨ METHOD 3: CRUSH CARD ANIMATION
        private async Task CrushCardAnimation(Card card)
        {
            Frame cardFrame = FindCardFrame(card);
            if (cardFrame == null) return;

            try
            {
                // Shake effect
                await cardFrame.RotateTo(-15, 50);
                await cardFrame.RotateTo(15, 50);
                await cardFrame.RotateTo(-15, 50);
                await cardFrame.RotateTo(0, 50);

                // Simultaneous: fade, shrink, red flash
                var fadeTask = cardFrame.FadeTo(0.3, 200);
                var scaleTask = cardFrame.ScaleTo(0.7, 200);
                await Task.WhenAll(fadeTask, scaleTask);

                // Flash red (destruction)
                cardFrame.BackgroundColor = Colors.Red.WithAlpha(0.8f);
                await Task.Delay(150);

                // Final destruction - fade out and spin
                var finalFade = cardFrame.FadeTo(0, 400);
                var finalScale = cardFrame.ScaleTo(0.1, 400);
                var finalRotate = cardFrame.RotateTo(180, 400);
                await Task.WhenAll(finalFade, finalScale, finalRotate);

                // Reset for next render
                cardFrame.Opacity = 1;
                cardFrame.Scale = 1;
                cardFrame.Rotation = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Crush animation error: {ex.Message}");
            }
        }
    }
}