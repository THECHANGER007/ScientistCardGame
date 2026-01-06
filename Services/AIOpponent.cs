using ScientistCardGame.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ScientistCardGame.Services
{
    public class AIOpponent
    {
        private GameEngine _gameEngine;
        private EffectsEngine _effectsEngine;
        private Random _random;

        public AIOpponent(GameEngine gameEngine, EffectsEngine effectsEngine)
        {
            _gameEngine = gameEngine;
            _effectsEngine = effectsEngine;
            _random = new Random();
        }

        // Execute AI turn automatically
        public async Task<string> ExecuteAITurnAsync()
        {
            var gameState = _gameEngine.GetGameState();
            string log = "";

            // DRAW PHASE
            if (gameState.CurrentPhase == "DRAW")
            {
                var drawn = _gameEngine.ExecuteTurn();
                if (drawn.DrawnCard != null)
                    log += $"🤖 AI drew {drawn.DrawnCard.Name}\n";

                await Task.Delay(500); // Pause so player can see
                gameState.NextPhase();
            }

            // MAIN PHASE - Play cards
            if (gameState.CurrentPhase == "MAIN")
            {
                log += await PlayCardsFromHand();
                await Task.Delay(800);
                gameState.NextPhase();
            }

            // BATTLE PHASE - Attack!
            if (gameState.CurrentPhase == "BATTLE")
            {
                log += await ExecuteAttacks();
                await Task.Delay(800);
                gameState.NextPhase();
            }

            // END PHASE - End turn
            if (gameState.CurrentPhase == "END")
            {
                _gameEngine.EndTurn();
                log += "🤖 AI ended turn\n";
            }

            return log;
        }

        // Play cards from hand
        private async Task<string> PlayCardsFromHand()
        {
            var gameState = _gameEngine.GetGameState();
            var ai = gameState.Player2;
            string log = "";

            // Play up to 3 CHARACTER cards
            var charactersInHand = ai.Hand.Where(c => c.CardType == "CHARACTER").ToList();
            int cardsPlayed = 0;

            foreach (var card in charactersInHand.Take(3))
            {
                if (_gameEngine.PlayCharacterCard(card, ai))
                {
                    log += $"🤖 AI played {card.Name} ({card.ATK}/{card.DEF})\n";
                    cardsPlayed++;
                    await Task.Delay(300);
                }
            }

            // Maybe play a DISCOVERY card (30% chance)
            var discoveryCards = ai.Hand.Where(c => c.CardType == "DISCOVERY").ToList();
            if (discoveryCards.Any() && _random.Next(100) < 30)
            {
                var discovery = discoveryCards.First();
                var result = _effectsEngine.ExecuteCardEffect(discovery, ai, gameState.Player1);
                ai.Hand.Remove(discovery);
                ai.SendToGraveyard(discovery);
                log += $"🤖 AI activated {discovery.Name}!\n";
                log += $"   {result.Message}\n";
                await Task.Delay(400);
            }

            // Maybe set a PARADOX card (20% chance)
            var paradoxCards = ai.Hand.Where(c => c.CardType == "PARADOX").ToList();
            if (paradoxCards.Any() && _random.Next(100) < 20)
            {
                var paradox = paradoxCards.First();
                if (_gameEngine.SetParadoxCard(paradox, ai))
                {
                    log += $"🤖 AI set {paradox.Name} face-down\n";
                    await Task.Delay(300);
                }
            }

            if (cardsPlayed == 0)
                log += "🤖 AI has no cards to play\n";

            return log;
        }

        // Execute attacks with AI's cards
        private async Task<string> ExecuteAttacks()
        {
            var gameState = _gameEngine.GetGameState();
            var ai = gameState.Player2;
            var player = gameState.Player1;
            string log = "";

            var aiAttackers = ai.Field.Where(c => c.CardType == "CHARACTER").ToList();

            if (aiAttackers.Count == 0)
            {
                log += "🤖 AI has no cards to attack with\n";
                return log;
            }

            foreach (var attacker in aiAttackers)
            {
                // Check if player has defenders
                var playerDefenders = player.Field.Where(c => c.CardType == "CHARACTER").ToList();

                if (playerDefenders.Count == 0)
                {
                    // Direct attack!
                    var result = _gameEngine.ExecuteDirectAttack(attacker, ai, player);
                    log += $"🤖 {attacker.Name} attacks directly! {result.Damage} damage!\n";
                }
                else
                {
                    // Attack weakest defender
                    var weakest = playerDefenders.OrderBy(c => c.CurrentDEF).First();
                    var result = _gameEngine.ExecuteBattle(attacker, weakest, ai, player);
                    log += $"🤖 {attacker.Name} vs {weakest.Name}: {result.Message}\n";
                }

                await Task.Delay(500);

                // Check if player lost
                gameState.CheckVictoryCondition();
                if (gameState.IsGameOver)
                    break;
            }

            return log;
        }
    }
}