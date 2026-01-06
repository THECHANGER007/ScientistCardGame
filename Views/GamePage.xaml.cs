using ScientistCardGame.Models;
using ScientistCardGame.Services;
using System;
using System.IO;
using System.Linq;

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
        private TurnTimerService _turnTimer; // ← ADD THIS
        private bool _isMultiplayerMode = false; // ← ADD THIS
        private bool _isInitialized = false;

        public GamePage()
        {
            InitializeComponent();
            
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Show loading message
            MessageLabel.Text = "🎮 Initializing game...";

            // Small delay to let UI show
            await Task.Delay(100);

            // Now initialize
            InitializeGame();
        }

        private async void InitializeGame()
        {
            try
            {
                MessageLabel.Text = "STEP 1: Starting...";
                await Task.Delay(500);

                // Stop old timer
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

                MessageLabel.Text = "STEP 6: Loading cards...";
                await Task.Delay(500);

                await _deckBuilderService.LoadAvailableCardsAsync();

                MessageLabel.Text = "STEP 7: Creating Player 1 deck...";
                await Task.Delay(500);

                var player1Deck = await _deckBuilderService.CreateStarterDeckAsync("Player 1");

                MessageLabel.Text = "STEP 8: Creating AI deck...";
                await Task.Delay(500);

                var player2Deck = await _deckBuilderService.CreateStarterDeckAsync("AI Opponent");

                MessageLabel.Text = "STEP 9: Starting game engine...";
                await Task.Delay(500);

                _gameEngine = new GameEngine();
                _gameEngine.StartNewGame("Player 1", "AI Opponent", player1Deck, player2Deck);

                MessageLabel.Text = "STEP 10: Creating effects engine...";
                await Task.Delay(500);

                var gameState = _gameEngine.GetGameState();
                _effectsEngine = new EffectsEngine(gameState);

                MessageLabel.Text = "STEP 11: Creating AI opponent...";
                await Task.Delay(500);

                _aiOpponent = new AIOpponent(_gameEngine, _effectsEngine);

                MessageLabel.Text = "STEP 12: Updating UI...";
                await Task.Delay(500);

                UpdateGameUI();

                ShowMessage("🎮 Game Started!");
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

            // Update Player Info
            PlayerNameLabel.Text = player.PlayerName;
            PlayerHPLabel.Text = player.HP.ToString();
            PlayerDeckCountLabel.Text = player.Deck.Count.ToString();
            PlayerGraveyardCountLabel.Text = player.Graveyard.Count.ToString();
            PlayerHandCountLabel.Text = $"Hand: {player.Hand.Count} | Summons: {player.SummonsThisTurn}/{player.MaxSummonsPerTurn}";

            // Update Opponent Info
            OpponentNameLabel.Text = opponent.PlayerName;
            OpponentHPLabel.Text = opponent.HP.ToString();
            OpponentDeckCountLabel.Text = opponent.Deck.Count.ToString();
            OpponentGraveyardCountLabel.Text = opponent.Graveyard.Count.ToString();
            OpponentHandCountLabel.Text = $"Hand: {opponent.Hand.Count}";

            // DNA: Update Turn Info with mode-specific text
            string turnText = $"Turn {gameState.CurrentTurn} - {gameState.CurrentPlayer.PlayerName}'s Turn";
            if (_isMultiplayerMode && gameState.CurrentPlayer.PlayerId == 2)
            {
                turnText += " 📱"; // Indicator for Player 2
            }
            TurnInfoLabel.Text = turnText;
            PhaseLabel.Text = $"📍 {gameState.CurrentPhase} Phase";
            // DNA: Timer disabled for now
            // if (_turnTimer != null && _turnTimer.IsEnabled && gameState.CurrentPlayer.PlayerId == 1)
            // {
            //     TimerLabel.IsVisible = true;
            //     TimerLabel.Text = $"⏱️ {_turnTimer.GetTimeString()}";
            // }
            // else
            // {
            TimerLabel.IsVisible = false;
            // }
            // DNA: Show active field bonuses
            var activeBonuses = GetActiveFieldBonuses(gameState.CurrentPlayer);
            if (!string.IsNullOrEmpty(activeBonuses))
            {
                MessageLabel.Text = activeBonuses;
            }

            // Update Hand
            UpdateHandDisplay(player);

            // Update Fields
            UpdateFieldDisplay(PlayerFieldContainer, player);
            UpdateFieldDisplay(OpponentFieldContainer, opponent);
            // Update Trap Zones
            UpdateTrapZoneDisplay(PlayerTrapZoneContainer, player);
            UpdateTrapZoneDisplay(OpponentTrapZoneContainer, opponent);

          
            // Check game over
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

        // DNA: Create visual card for hand display
        private Frame CreateHandCardView(Card card)
        {
            // DNA: Color-coded borders by FIELD and TYPE
            Color borderColor = Colors.Gray;
            Color backgroundColor = Color.FromArgb("#2C3E50");

            if (card.CardType == "CHARACTER")
            {
                borderColor = card.Field switch
                {
                    "SCIENCE" => Color.FromArgb("#3498DB"),      // Blue
                    "PHILOSOPHY" => Color.FromArgb("#9B59B6"),   // Purple
                    "SPIRITUALITY" => Color.FromArgb("#F39C12"), // Gold
                    "HUMANITIES" => Color.FromArgb("#27AE60"),   // Green
                    _ => Colors.Gray
                };
            }
            else if (card.CardType == "DISCOVERY")
            {
                borderColor = Color.FromArgb("#2ECC71");  // Bright Green
                backgroundColor = Color.FromArgb("#1E8449");
            }
            else if (card.CardType == "PARADOX")
            {
                borderColor = Color.FromArgb("#E91E63");  // Pink
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

            var stackLayout = new VerticalStackLayout
            {
                Spacing = 4
            };

            // DNA: Card Name with Tier Badge
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

            // DNA: Tier/Type indicator
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

            // DNA: Stats for CHARACTER cards
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

                // Field and School
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

            // DNA: Buttons
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

            // Get only CHARACTER cards for monster zone
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
                    var cardView = CreateFieldCardView(card); // FIXED: Removed second parameter
                    container.Add(cardView);
                }
            }
        }

        // DNA: Update trap zone display
        // DNA: Update trap zone display
        private void UpdateTrapZoneDisplay(VerticalStackLayout container, Player player)
        {
            container.Clear();

            // Get PARADOX cards (traps)
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
                // DNA: Determine if these are player's cards or opponent's
                bool isPlayerCard = (player.PlayerId == 1);

                foreach (var trap in trapCards)
                {
                    var trapView = CreateTrapCardView(trap, isPlayerCard); // FIXED: Changed 'card' to 'trap'
                    container.Add(trapView);
                }
            }
        }
        // DNA: Show full card details
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
        // DNA: Create face-down trap card view
        // DNA: Create face-down trap card view
        // DNA: Create visual card for trap zone display
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

            var stackLayout = new VerticalStackLayout
            {
                Spacing = 5
            };

            if (isPlayerCard && card.IsFaceDown)
            {
                // Player's trap - show name and buttons
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
                // Opponent's trap - face down
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
        // DNA: Activate trap card
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

            // Execute trap effect
            var result = _effectsEngine.ExecuteCardEffect(trap, gameState.Player1, gameState.Player2);

            // Remove from field and send to graveyard
            gameState.Player1.Field.Remove(trap);
            gameState.Player1.SendToGraveyard(trap);

            ShowMessage($"🔮 {result.Message}");
            UpdateGameUI();
        }

        // DNA: Create visual card for field display (with position, stats, buttons)
        private Frame CreateFieldCardView(Card card)
        {
            // DNA: Color-coded borders
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

            var stackLayout = new VerticalStackLayout
            {
                Spacing = 3
            };

            // DNA: Card Name with Tier Badge
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

            // DNA: Battle Position with Icon
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

            // DNA: Current Stats (with bonuses highlighted)
            string statText = card.BattlePosition == "ATTACK"
                ? $"⚔️ ATK: {card.CurrentATK}"
                : $"🛡️ DEF: {card.CurrentDEF}";

            // Show bonus if stats changed
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

            // DNA: Base Stats (smaller)
            var baseStatsLabel = new Label
            {
                Text = $"Base: {card.ATK}/{card.DEF}",
                FontSize = 8,
                TextColor = Color.FromArgb("#95A5A6"),
                HorizontalOptions = LayoutOptions.Center
            };
            stackLayout.Add(baseStatsLabel);

            // DNA: Field/School badges
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

            // DNA: Buttons
            var buttonStack = new HorizontalStackLayout
            {
                Spacing = 5,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };

            // Attack button
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

            // Switch position button
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

            // Effect button (only for MANUAL trigger cards)
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

        // ========== GAME ACTIONS ==========

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
                // DNA: Check summoning system
                var summonResult = await CheckSummonRequirements(card, gameState.CurrentPlayer);

                if (!summonResult.canSummon)
                {
                    await DisplayAlert("⚠️ Cannot Summon", summonResult.reason, "OK");
                    return;
                }

                // DNA: Apply summoning cost
                success = await ExecuteSummon(card, gameState.CurrentPlayer, summonResult);

                if (success)
                {
                    // DNA: Ask for battle position
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
                    // DNA: Apply graveyard effects to newly summoned card
                    _gameEngine.ApplyGraveyardEffects(gameState.CurrentPlayer);
                }
            }
            else if (card.CardType == "DISCOVERY")
            {
                var result = _effectsEngine.ExecuteCardEffect(card, gameState.CurrentPlayer, gameState.OpponentPlayer);
                gameState.CurrentPlayer.Hand.Remove(card);
                gameState.CurrentPlayer.SendToGraveyard(card);
                ShowMessage($"💚 {result.Message}");
                success = true;
            }
            else if (card.CardType == "PARADOX")
            {
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

            // DNA: Can only attack once per turn
            if (attackingCard.HasAttackedThisTurn)
            {
                await DisplayAlert("⚠️ Already Attacked",
                    $"{attackingCard.Name} has already attacked this turn!", "OK");
                return;
            }

            // DNA: Cannot attack on Turn 1
            if (gameState.CurrentTurn == 1)
            {
                await DisplayAlert("⚠️ Cannot Attack",
                    "You cannot attack on the first turn of the game!", "OK");
                return;
            }

            var opponentCharacters = opponent.Field.Where(c => c.CardType == "CHARACTER").ToList();

            if (opponentCharacters.Count == 0)
            {
                // Direct attack (no targets)
                bool confirmDirect = await DisplayAlert(
                    "⚔️ Direct Attack!",
                    $"{attackingCard.Name} will attack directly for {attackingCard.CurrentATK} damage!",
                    "✅ Attack",
                    "❌ Cancel"
                );

                if (!confirmDirect)
                    return;

                // DNA: Show bonus calculation
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
                // DNA: TARGET SELECTION!
                Card targetCard = await SelectAttackTarget(opponentCharacters, attackingCard);

                if (targetCard == null)
                    return; // Cancelled

                // DNA: Calculate and show bonuses BEFORE battle
                int baseATK = attackingCard.CurrentATK;
                int finalATK = _gameEngine.CalculateFinalATK(attackingCard, targetCard, gameState.CurrentPlayer);
                int atkBonus = finalATK - baseATK;

                string bonusInfo = "";
                if (atkBonus > 0)
                    bonusInfo = $" (+{atkBonus} bonus!)";
                else if (atkBonus < 0)
                    bonusInfo = $" ({atkBonus} penalty!)";

                ShowMessage($"⚔️ {attackingCard.Name}: {baseATK} ATK{bonusInfo} = {finalATK} total!");
                await Task.Delay(1000);

                // Execute battle with selected target
                var result = _gameEngine.ExecuteBattle(attackingCard, targetCard,
                    gameState.CurrentPlayer, opponent);
                ShowMessage($"⚔️ {result.Message}");
            }

            // Mark card as having attacked
            attackingCard.HasAttackedThisTurn = true;

            gameState.CheckVictoryCondition();
            UpdateGameUI();
        }
        // DNA: Let player select which card to attack
        // DNA: Let player select which card to attack
        private async Task<Card> SelectAttackTarget(List<Card> targets, Card attacker)
        {
            if (targets.Count == 0)
                return null;

            // DNA: Check Mandela protection ONCE at the start
            var gameState = _gameEngine.GetGameState();
            var validTargets = targets.ToList();

            if (gameState.OpponentPlayer.Graveyard.Any(c => c.Name == "Nelson Mandela"))
            {
                // Remove HUMANISM cards from target list
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
                // Only one valid target - confirm
                var target = validTargets[0];
                bool confirm = await DisplayAlert(
                    "⚔️ Confirm Attack",
                    $"{attacker.Name} ({attacker.CurrentATK} ATK)\n    VS\n{target.Name} ({(target.BattlePosition == "ATTACK" ? target.CurrentATK + " ATK" : target.CurrentDEF + " DEF")})",
                    "✅ Attack",
                    "❌ Cancel"
                );

                return confirm ? target : null;
            }

            // Multiple valid targets - show selection
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

            // Find selected target
            int selectedIndex = Array.IndexOf(targetOptions, choice);
            if (selectedIndex >= 0 && selectedIndex < validTargets.Count)
            {
                var target = validTargets[selectedIndex];

                // Confirm
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


        // DNA: Let player select which trap to activate
        private async Task<Card> SelectTrapToActivate(List<Card> traps)
        {
            if (traps.Count == 0)
                return null;

            if (traps.Count == 1)
            {
                // Only one trap - ask if they want to activate it
                bool activate = await DisplayAlert(
                    "🔮 Activate Trap?",
                    $"{traps[0].Name}\n\n✨ {traps[0].SpecialEffect}",
                    "✅ Activate",
                    "❌ Cancel"
                );

                return activate ? traps[0] : null;
            }

            // Multiple traps - let player choose
            string[] trapNames = traps.Select(t => $"{t.Name}").ToArray();

            string choice = await DisplayActionSheet(
                "🔮 Choose Trap to Activate",
                "❌ Cancel",
                null,
                trapNames
            );

            if (choice == "❌ Cancel" || choice == null)
                return null;

            // Find selected trap
            var selectedTrap = traps.FirstOrDefault(t => t.Name == choice);

            if (selectedTrap != null)
            {
                // Confirm activation
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

            // DNA: Check if effect already used
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

            // Execute effect
            var result = _effectsEngine.ExecuteCardEffect(card, gameState.CurrentPlayer, gameState.OpponentPlayer);

            // Mark as used
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
                var drawn = _gameEngine.ExecuteTurn();
                if (drawn.DrawnCard != null)
                {
                    ShowMessage($"📥 Drew: {drawn.DrawnCard.Name}");

                    // DNA: Check Prisoner's Dilemma effect
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

                // CRITICAL FIX: Always advance phase!
                gameState.NextPhase();
                UpdateGameUI();
                ShowMessage($"📋 {gameState.CurrentPhase} Phase");
            }
            else if (gameState.CurrentPhase == "MAIN")
            {
                // Move to Battle Phase
                gameState.NextPhase();
                UpdateGameUI();
                ShowMessage($"⚔️ {gameState.CurrentPhase} Phase - Attack with your cards!");
            }
            else if (gameState.CurrentPhase == "BATTLE")
            {
                // Move to End Phase
                gameState.NextPhase();
                UpdateGameUI();
                ShowMessage($"🔚 {gameState.CurrentPhase} Phase");
            }
            else if (gameState.CurrentPhase == "END")
            {
                // Turn is ending - should use End Turn button
                await DisplayAlert("ℹ️ End Turn", "Use the 'End Turn' button to finish your turn!", "OK");
            }
        }

        private async void OnEndTurnClicked(object sender, EventArgs e)
        {
            // DNA: Stop timer when turn ends
            if (_turnTimer != null) // ← FIX: Null check
            {
                _turnTimer.StopTimer();
            }

            _gameEngine.EndTurn();
            UpdateGameUI();

            var gameState = _gameEngine.GetGameState();

            // Check if it's AI's turn
            if (gameState.CurrentPlayer.PlayerId == 2)
            {
                // DNA: Check if AI turn should be skipped (Time Dilation)
                if (gameState.CurrentPlayer.SpecialEffect.Contains("[SKIP_NEXT_TURN]"))
                {
                    ShowMessage("⏱️ Time Dilation: AI's turn is SKIPPED!");
                    await Task.Delay(2000);

                    // Remove skip flag
                    gameState.CurrentPlayer.SpecialEffect =
                        gameState.CurrentPlayer.SpecialEffect.Replace(" [SKIP_NEXT_TURN]", "");

                    // End AI turn immediately
                    _gameEngine.EndTurn();
                    UpdateGameUI();
                    ShowMessage("🔄 Your turn begins!");
                    return;
                }

                // Disable buttons during AI turn
                NextPhaseButton.IsEnabled = false;
                EndTurnButton.IsEnabled = false;

                ShowMessage("🤖 AI Opponent is thinking...");
                await Task.Delay(1000);

                // DRAW PHASE
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

                // MAIN PHASE
                gameState.NextPhase();
                UpdateGameUI();
                ShowMessage("🤖 AI Main Phase - Playing cards...");
                await Task.Delay(1000);

                string playLog = await AIPlayCards();
                ShowMessage(playLog);
                await Task.Delay(1500);

                // BATTLE PHASE
                gameState.NextPhase();
                UpdateGameUI();
                ShowMessage("🤖 AI Battle Phase - Attacking...");
                await Task.Delay(1000);

                string battleLog = await AIBattle();
                ShowMessage(battleLog);
                await Task.Delay(1500);

                // END PHASE
                gameState.NextPhase();
                _gameEngine.EndTurn();
                UpdateGameUI();
                ShowMessage("🤖 AI ended turn. Your turn begins!");

                // Re-enable buttons
                NextPhaseButton.IsEnabled = true;
                EndTurnButton.IsEnabled = true;

                // DNA: Start timer for player's turn
                if (_turnTimer != null && _turnTimer.IsEnabled) // ← FIX: Null check
                {
                    _turnTimer.StartTimer();
                }
            }
            else
            {
                ShowMessage("🔄 Your turn begins!");
            }
        }

        // DNA: Timer settings
        private async void OnTimerSettingsClicked(object sender, EventArgs e)
        {
            if (_turnTimer == null) // ← FIX: Null check at start
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

        // AI helper methods
        private async Task<string> AIPlayCards()
        {
            var gameState = _gameEngine.GetGameState();
            var ai = gameState.Player2;
            string log = "";

            // DNA: Prioritize cards that create field synergy
            var charactersInHand = ai.Hand.Where(c => c.CardType == "CHARACTER").ToList();

            // Sort by synergy potential
            charactersInHand = charactersInHand.OrderByDescending(c =>
            {
                int synergy = 0;

                // +10 points if this card matches existing field
                synergy += ai.Field.Count(f => f.Field == c.Field) * 10;

                // +5 points for each matching school
                synergy += ai.Field.Count(f => f.School == c.School) * 5;

                // +20 points for LEGENDARY
                if (c.Tier == "LEGENDARY") synergy += 20;

                // +10 points for high ATK
                if (c.ATK >= 2500) synergy += 10;

                return synergy;
            }).Take(3).ToList();

            foreach (var card in charactersInHand)
            {
                if (ai.SummonsThisTurn >= 2) break;

                if (_gameEngine.PlayCharacterCard(card, ai))
                {
                    ai.SummonsThisTurn++;

                    // DNA: Smart position choice
                    card.BattlePosition = ChooseSmartPosition(card);
                    string posIcon = card.BattlePosition == "ATTACK" ? "⚔️" : "🛡️";

                    log += $"▶️ AI played {card.Name} ({posIcon} {card.BattlePosition})\n";
                    UpdateGameUI();
                    await Task.Delay(700);
                }
            }

            // DNA: AI uses card effects strategically
            var effectCards = ai.Field.Where(c =>
                c.CardType == "CHARACTER" &&
                c.EffectTrigger == "MANUAL" &&
                !c.EffectUsedThisTurn
            ).ToList();

            if (effectCards.Any() && _random.Next(100) < 40) // 40% chance to use effect
            {
                var effectCard = effectCards[_random.Next(effectCards.Count)];
                var result = _effectsEngine.ExecuteCardEffect(effectCard, ai, gameState.Player1);
                effectCard.EffectUsedThisTurn = true;

                log += $"🎯 AI activated {effectCard.Name}'s effect!\n";
                log += $"   {result.Message}\n";

                UpdateGameUI();
                await Task.Delay(700);
            }

            // DNA: Smart trap setting (Step 17D)
            var paradoxCards = ai.Hand.Where(c => c.CardType == "PARADOX").ToList();
            int aiAttackers = ai.Field.Count(c => c.CardType == "CHARACTER");

            if (paradoxCards.Any() && aiAttackers >= 2 && _random.Next(100) < 60) // 60% chance if we have 2+ attackers
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

            // DNA: Cannot attack on turn 1
            if (gameState.CurrentTurn == 1)
            {
                return "AI cannot attack on first turn\n";
            }

            var aiAttackers = ai.Field.Where(c => c.CardType == "CHARACTER").ToList();

            if (aiAttackers.Count == 0)
                return "AI has no attackers\n";

            foreach (var attacker in aiAttackers)
            {
                // DNA: Get player defenders FIRST (needed for trap redirect logic)
                var playerDefenders = player.Field.Where(c => c.CardType == "CHARACTER").ToList();
                // DNA: CHECK PLAYER TRAPS BEFORE AI ATTACKS!
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
                            var trapResult = _effectsEngine.ExecuteCardEffect(selectedTrap, player, ai);

                            player.Field.Remove(selectedTrap);
                            player.SendToGraveyard(selectedTrap);

                            log += $"🔮 YOU ACTIVATED: {selectedTrap.Name}!\n";
                            log += $"   {trapResult.Message}\n";

                            ShowMessage(log);
                            UpdateGameUI();
                            await Task.Delay(1500);

                            // DNA FIX: Check ALL trap effect flags

                            // 1. NEGATED ATTACK (Schrödinger's Cat)
                            if (trapResult.NegatedAttack)
                            {
                                log += $"🛡️ {attacker.Name}'s attack was NEGATED!\n";
                                ShowMessage(log);
                                await Task.Delay(1000);
                                attacker.HasAttackedThisTurn = true;
                                continue; // Skip this attack
                            }

                            // 2. REDIRECT TARGET (Heisenberg's Uncertainty)
                            if (trapResult.RedirectTarget && playerDefenders.Count > 1)
                            {
                                // Redirect to random card
                                var newTarget = playerDefenders[_random.Next(playerDefenders.Count)];
                                log += $"🎯 Attack redirected to {newTarget.Name}!\n";
                                ShowMessage(log);
                                await Task.Delay(1000);

                                // Execute battle with new target
                                var redirectResult = _gameEngine.ExecuteBattle(attacker, newTarget, ai, player);
                                log += $"⚔️ {redirectResult.Message}\n";
                                ShowMessage(log);
                                UpdateGameUI();
                                await Task.Delay(1000);

                                gameState.CheckVictoryCondition();
                                if (gameState.IsGameOver) break;

                                continue; // Skip normal attack flow
                            }

                            // 3. SKIP NEXT TURN (Time Dilation)
                            if (trapResult.SkipNextTurn)
                            {
                                gameState.CurrentPlayer.SpecialEffect += " [SKIP_NEXT_TURN]";
                                log += $"⏱️ AI's next turn will be SKIPPED!\n";
                                ShowMessage(log);
                                await Task.Delay(1000);
                                // Attack still continues
                            }
                        }
                    }
                }

              
                
                if (playerDefenders.Count == 0)
                {
                    // Direct attack
                    ShowMessage($"🎯 AI's {attacker.Name} targets YOU directly!");
                    UpdateGameUI();
                    await Task.Delay(1200);

                    var result = _gameEngine.ExecuteDirectAttack(attacker, ai, player);
                    log += $"⚔️ {attacker.Name} → {result.Damage} damage!\n";
                }
                else
                {
                  
                    // DNA: Smart target selection
                    var target = ChooseSmartTarget(playerDefenders, attacker);

                    // DNA: Calculate bonuses
                    int baseATK = attacker.CurrentATK;
                    int finalATK = _gameEngine.CalculateFinalATK(attacker, target, ai);
                    int atkBonus = finalATK - baseATK;

                    string bonusInfo = atkBonus > 0 ? $" +{atkBonus}" : "";
                    string targetPos = target.BattlePosition == "ATTACK" ? $"{target.CurrentATK} ATK" : $"{target.CurrentDEF} DEF";

                    ShowMessage($"🎯 AI's {attacker.Name} ({baseATK}{bonusInfo} = {finalATK} ATK) targeting {target.Name} ({targetPos})!");
                    UpdateGameUI();
                    await Task.Delay(1500);

                    var result = _gameEngine.ExecuteBattle(attacker, target, ai, player);
                    log += $"⚔️ {result.Message}\n";
                }

                UpdateGameUI();
                await Task.Delay(1000);

                gameState.CheckVictoryCondition();
                if (gameState.IsGameOver) break;
            }

            return log;
        }

        // ========== HELPERS ==========

        private void ShowMessage(string message)
        {
            MessageLabel.Text = message;

            // Auto-clear old messages after delay
            Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                if (MessageLabel.Text == message)
                    MessageLabel.Text = "";
                return false;
            });
        }

        private async void ShowGameOver(Player winner)
        {
            bool playerWon = winner.PlayerId == 1;

            // Record win/loss
            string statsText = "";
            if (_statsService != null) // ← FIX: Null check
            {
                if (playerWon)
                    await _statsService.RecordWin();
                else
                    await _statsService.RecordLoss();

                // Get updated stats
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
                // DNA: Clean up before restart
                if (_turnTimer != null) // ← FIX: Null check
                {
                    _turnTimer.StopTimer();
                }

                // Restart game
                InitializeGame();
            }
        }

        // DNA: Show statistics
        private async void OnStatsClicked(object sender, EventArgs e)
        {
            if (_statsService == null) // ← FIX: Null check at start
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


        // ========== DNA: SUMMONING SYSTEM (Alchemy & Ritual) ==========

        private async Task<(bool canSummon, string reason)> CheckSummonRequirements(Card card, Player player)
        {
            // Check summon limit
            if (player.SummonsThisTurn >= player.MaxSummonsPerTurn)
            {
                return (false, $"You can only summon {player.MaxSummonsPerTurn} cards per turn!");
            }

            // SCHOLAR: Free summon
            if (card.Tier == "SCHOLAR")
            {
                return (true, "");
            }

            // MASTER: Alchemy - requires HP sacrifice
            if (card.Tier == "MASTER")
            {
                if (player.HP <= 1000)
                {
                    return (false, "Not enough HP! Need at least 1001 HP to perform Alchemy!");
                }
                return (true, "");
            }

            // LEGENDARY: Ritual - requires tribute OR massive HP cost
            if (card.Tier == "LEGENDARY")
            {
                var tributeCandidates = player.Field.Where(c => c.CardType == "CHARACTER").ToList();

                if (tributeCandidates.Count >= 2)
                {
                    return (true, ""); // Can tribute
                }
                else if (player.HP > 3000)
                {
                    return (true, ""); // Can pay HP instead
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

            // SCHOLAR: Free summon
            if (card.Tier == "SCHOLAR")
            {
                success = _gameEngine.PlayCharacterCard(card, player);
            }
            // MASTER: Alchemy - HP cost
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
            // LEGENDARY: Ritual - Tribute or HP cost
            else if (card.Tier == "LEGENDARY")
            {
                var tributeCandidates = player.Field.Where(c => c.CardType == "CHARACTER").ToList();

                if (tributeCandidates.Count >= 2)
                {
                    // Option: Tribute 2 cards
                    bool useTribute = await DisplayAlert(
                        "🌟 Legendary Ritual",
                        $"Summon {card.Name} ({card.ATK}/{card.DEF})?\n\nChoose summoning method:",
                        "⚰️ Tribute 2 Cards",
                        "💔 Pay 3000 HP"
                    );

                    if (useTribute)
                    {
                        // Tribute first 2 cards
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
                        // Pay HP instead
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
                    // Only HP option available
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

            // DNA: AUTO-TRIGGER ON_SUMMON EFFECTS
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

        // DNA: Switch battle position
        private void OnSwitchPositionClicked(Card card)
        {
            card.BattlePosition = card.BattlePosition == "ATTACK" ? "DEFENSE" : "ATTACK";

            string newPosition = card.BattlePosition;
            string icon = newPosition == "ATTACK" ? "⚔️" : "🛡️";

            ShowMessage($"{icon} {card.Name} switched to {newPosition} mode!");
            UpdateGameUI();
        }

        // DNA: Click deck to draw (only in draw phase)...........
        private async void OnPlayerDeckTapped(object sender, EventArgs e)
        {
            // DNA: Check if game is initialized
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

            // Draw card
            var drawn = _gameEngine.ExecuteTurn();
            if (drawn.DrawnCard != null)
            {
                ShowMessage($"📥 Drew: {drawn.DrawnCard.Name}");
                gameState.NextPhase(); // Auto-advance to Main Phase
                UpdateGameUI();
            }
        }

        // DNA: View player graveyard (ALWAYS Player1)
        private async void OnPlayerGraveyardTapped(object sender, EventArgs e)
        {
            var gameState = _gameEngine.GetGameState();
            var graveyard = gameState.Player1.Graveyard; // ← FIXED: Always Player1

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
        // DNA: View opponent graveyard (ALWAYS Player2)
        private async void OnOpponentGraveyardTapped(object sender, EventArgs e)
        {
            var gameState = _gameEngine.GetGameState();
            var graveyard = gameState.Player2.Graveyard; // ← FIXED: Always Player2

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

        // DNA: Show full card details
        private async void ShowCardDetails(Card card)
        {
            string details = "";

            // Card Name and Type
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

            // Effect
            details += $"✨ EFFECT:\n{card.SpecialEffect}";

            await DisplayAlert($"📜 {card.Name}", details, "Close");
        }

        // DNA: Get active field/school bonuses for display
        private string GetActiveFieldBonuses(Player player)
        {
            List<string> bonuses = new List<string>();

            // Field bonuses
            if (player.CountFieldCards("SCIENCE") >= 2)
                bonuses.Add($"🔬 SCIENCE SYNERGY: +200 ATK ({player.CountFieldCards("SCIENCE")} cards)");

            if (player.CountFieldCards("PHILOSOPHY") >= 2)
                bonuses.Add($"💭 PHILOSOPHY SYNERGY: +200 ATK ({player.CountFieldCards("PHILOSOPHY")} cards)");

            if (player.CountFieldCards("SPIRITUALITY") >= 2)
                bonuses.Add($"🙏 SPIRITUALITY SYNERGY: +200 DEF ({player.CountFieldCards("SPIRITUALITY")} cards)");

            if (player.CountFieldCards("HUMANITIES") >= 2)
                bonuses.Add($"📚 HUMANITIES SYNERGY: +200 ATK/DEF ({player.CountFieldCards("HUMANITIES")} cards)");

            // Graveyard effects
            var graveyardEffects = _gameEngine.GetActiveGraveyardEffects(player);
            bonuses.AddRange(graveyardEffects);

            return bonuses.Count > 0 ? string.Join(" | ", bonuses) : "";
        }
        // DNA: AI chooses smart battle position
        private string ChooseSmartPosition(Card card)
        {
            // High ATK cards → Attack position
            if (card.ATK >= 2500)
                return "ATTACK";

            // High DEF, low ATK → Defense position
            if (card.DEF > card.ATK + 300)
                return "DEFENSE";

            // Balanced or offensive → Attack position
            if (card.ATK >= card.DEF)
                return "ATTACK";

            // Default: Defense for safety
            return "DEFENSE";
        }

        // DNA: AI chooses best target to attack
        private Card ChooseSmartTarget(List<Card> targets, Card attacker)
        {
            // Priority 1: Can we destroy a LEGENDARY?
            var legendaries = targets.Where(t => t.Tier == "LEGENDARY").ToList();
            if (legendaries.Any())
            {
                // Attack weakest legendary we can destroy
                var destroyable = legendaries.Where(t =>
                    (t.BattlePosition == "ATTACK" && attacker.CurrentATK > t.CurrentATK) ||
                    (t.BattlePosition == "DEFENSE" && attacker.CurrentATK > t.CurrentDEF)
                ).OrderBy(t => t.BattlePosition == "ATTACK" ? t.CurrentATK : t.CurrentDEF);

                if (destroyable.Any())
                    return destroyable.First();
            }

            // Priority 2: Attack highest ATK card we can destroy
            var attackMode = targets.Where(t => t.BattlePosition == "ATTACK").ToList();
            if (attackMode.Any())
            {
                var destroyable = attackMode.Where(t => attacker.CurrentATK > t.CurrentATK)
                    .OrderByDescending(t => t.CurrentATK);

                if (destroyable.Any())
                    return destroyable.First();
            }

            // Priority 3: Attack weakest DEF (easiest to destroy)
            var defenseMode = targets.Where(t => t.BattlePosition == "DEFENSE").ToList();
            if (defenseMode.Any())
            {
                return defenseMode.OrderBy(t => t.CurrentDEF).First();
            }

            // Fallback: Weakest overall
            return targets.OrderBy(t => t.BattlePosition == "ATTACK" ? t.CurrentATK : t.CurrentDEF).First();
        }
        // DNA: Show deck statistics
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

            // Field composition
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
        // DNA: Restart game
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
    }
}