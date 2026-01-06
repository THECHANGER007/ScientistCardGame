using ScientistCardGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScientistCardGame.Services
{
    public class GameEngine
    {
        private GameState _gameState;

        // DNA: Initialize a new game
        public void StartNewGame(string player1Name, string player2Name, Deck player1Deck, Deck player2Deck)
        {
            _gameState = new GameState(player1Name, player2Name);

            // Load decks
            LoadDeck(_gameState.Player1, player1Deck);
            LoadDeck(_gameState.Player2, player2Deck);

            // DNA: Shuffle both decks
            _gameState.Player1.Deck = ShuffleCards(_gameState.Player1.Deck);
            _gameState.Player2.Deck = ShuffleCards(_gameState.Player2.Deck);

            // DNA: Draw starting hand (5 cards each)
            for (int i = 0; i < 5; i++)
            {
                _gameState.Player1.DrawCard();
                _gameState.Player2.DrawCard();
            }
        }

        // Load deck into player
        private void LoadDeck(Player player, Deck deck)
        {
            foreach (var card in deck.Cards)
            {
                card.OwnerId = player.PlayerId;
                card.Location = "DECK";
                card.ResetStats(); // Reset to original ATK/DEF
                player.Deck.Add(card);
            }
        }

        // DNA: Shuffle cards
        private List<Card> ShuffleCards(List<Card> cards)
        {
            Random rng = new Random();
            return cards.OrderBy(c => rng.Next()).ToList();
        }

        // DNA: Execute a full turn
        public TurnResult ExecuteTurn()
        {
            TurnResult result = new TurnResult();

            // Draw Phase
            if (_gameState.CurrentPhase == "DRAW")
            {
                var drawnCard = _gameState.ExecuteDrawPhase();
                result.DrawnCard = drawnCard;
                result.Message = drawnCard != null
                    ? $"{_gameState.CurrentPlayer.PlayerName} drew {drawnCard.Name}"
                    : "Deck is empty!";
            }

            return result;
        }

        // DNA: Calculate ATK with all bonuses (Field synergy, School clash)
        public int CalculateFinalATK(Card attackingCard, Card defendingCard, Player attacker)
        {
            int finalATK = attackingCard.CurrentATK;

            // DNA: Field Synergy - 2+ same FIELD cards = +200 ATK each
            if (attackingCard.CardType == "CHARACTER")
            {
                int sameFieldCount = attacker.CountFieldCards(attackingCard.Field);
                if (sameFieldCount >= 2)
                {
                    finalATK += 200; // Field bonus
                }
            }

            // DNA: School Clash - advantage matchups
            if (attackingCard.CardType == "CHARACTER" && defendingCard != null && defendingCard.CardType == "CHARACTER")
            {
                int schoolBonus = CalculateSchoolBonus(attackingCard.School, defendingCard.School);
                finalATK += schoolBonus;
            }

            return finalATK;
        }

        // DNA: School clash system
        private int CalculateSchoolBonus(string attackerSchool, string defenderSchool)
        {
            // DNA: Advantage matchups (+500 ATK)
            if (attackerSchool == "RATIONALISM" && defenderSchool == "MYSTICISM") return 500;
            if (attackerSchool == "EMPIRICISM" && defenderSchool == "IDEALISM") return 500;
            if (attackerSchool == "MATERIALISM" && defenderSchool == "HUMANISM") return 500;

            // DNA: Reverse matchups (defender gets +500, so attacker gets -500 effectively)
            if (attackerSchool == "MYSTICISM" && defenderSchool == "RATIONALISM") return -500;
            if (attackerSchool == "IDEALISM" && defenderSchool == "EMPIRICISM") return -500;
            if (attackerSchool == "HUMANISM" && defenderSchool == "MATERIALISM") return -500;

            return 0; // No bonus
        }

        // DNA: Calculate DEF with all bonuses
        public int CalculateFinalDEF(Card defendingCard, Player defender)
        {
            int finalDEF = defendingCard.CurrentDEF;

            // DNA: Field Synergy for SPIRITUALITY = +200 DEF
            if (defendingCard.Field == "SPIRITUALITY")
            {
                int spiritualityCount = defender.CountFieldCards("SPIRITUALITY");
                if (spiritualityCount >= 2)
                {
                    finalDEF += 200;
                }
            }

            return finalDEF;
        }

      
        // DNA: Execute battle between two cards (OPTION C - Position-based)
        public BattleResult ExecuteBattle(Card attackingCard, Card defendingCard, Player attacker, Player defender)
        {
            BattleResult result = new BattleResult();

            // Calculate final stats with all bonuses
            int attackerFinalATK = CalculateFinalATK(attackingCard, defendingCard, attacker);

            result.FinalATK = attackerFinalATK;

            // DNA OPTION C: Check defender's position
            if (defendingCard.BattlePosition == "ATTACK")
            {
                // ATK vs ATK battle
                int defenderFinalATK = CalculateFinalATK(defendingCard, attackingCard, defender);
                result.FinalDEF = defenderFinalATK;

                if (attackerFinalATK > defenderFinalATK)
                {
                    // Attacker wins
                    int damage = attackerFinalATK - defenderFinalATK;
                    defender.TakeDamage(damage);
                    defender.SendToGraveyard(defendingCard);

                    result.Winner = attacker;
                    result.Damage = damage;
                    result.DestroyedCard = defendingCard;
                    result.Message = $"{attackingCard.Name} ({attackerFinalATK} ATK) destroyed {defendingCard.Name} ({defenderFinalATK} ATK)! {damage} damage!";
                }
                else if (attackerFinalATK < defenderFinalATK)
                {
                    // Defender wins
                    int damage = defenderFinalATK - attackerFinalATK;
                    attacker.TakeDamage(damage);
                    attacker.SendToGraveyard(attackingCard);

                    result.Winner = defender;
                    result.Damage = damage;
                    result.DestroyedCard = attackingCard;
                    result.Message = $"{defendingCard.Name} ({defenderFinalATK} ATK) destroyed {attackingCard.Name} ({attackerFinalATK} ATK)! {damage} damage!";
                }
                else
                {
                    // Equal ATK - BOTH DESTROYED
                    attacker.SendToGraveyard(attackingCard);
                    defender.SendToGraveyard(defendingCard);

                    result.Winner = null;
                    result.Damage = 0;
                    result.Message = $"MUTUAL DESTRUCTION! {attackingCard.Name} and {defendingCard.Name} destroyed! (Both {attackerFinalATK} ATK)";
                }
            }
            else // defendingCard.BattlePosition == "DEFENSE"
            {
                // ATK vs DEF battle
                int defenderFinalDEF = CalculateFinalDEF(defendingCard, defender);
                result.FinalDEF = defenderFinalDEF;

                if (attackerFinalATK > defenderFinalDEF)
                {
                    // Attacker destroys defender - NO DAMAGE to player
                    defender.SendToGraveyard(defendingCard);

                    result.Winner = attacker;
                    result.Damage = 0;
                    result.DestroyedCard = defendingCard;
                    result.Message = $"{attackingCard.Name} ({attackerFinalATK} ATK) destroyed {defendingCard.Name} ({defenderFinalDEF} DEF)! [Defense Position]";
                }
                else if (attackerFinalATK < defenderFinalDEF)
                {
                    // Defender's DEF holds - attacker takes damage
                    int damage = defenderFinalDEF - attackerFinalATK;
                    attacker.TakeDamage(damage);

                    result.Winner = defender;
                    result.Damage = damage;
                    result.Message = $"{defendingCard.Name} ({defenderFinalDEF} DEF) blocked {attackingCard.Name} ({attackerFinalATK} ATK)! {damage} damage reflected!";
                }
                else
                {
                    // ATK = DEF - Defender destroyed, NO damage
                    defender.SendToGraveyard(defendingCard);

                    result.Winner = null;
                    result.Damage = 0;
                    result.DestroyedCard = defendingCard;
                    result.Message = $"{attackingCard.Name} ({attackerFinalATK} ATK) barely destroyed {defendingCard.Name} ({defenderFinalDEF} DEF)!";
                }
            }

            // DNA: Check victory condition
            _gameState.CheckVictoryCondition();

            return result;
        }

        // DNA: Direct attack (when opponent has no cards on field)
        public BattleResult ExecuteDirectAttack(Card attackingCard, Player attacker, Player defender)
        {
            BattleResult result = new BattleResult();

            int finalATK = CalculateFinalATK(attackingCard, null, attacker);

            // DNA: Direct attack deals full ATK as damage
            defender.TakeDamage(finalATK);

            result.FinalATK = finalATK;
            result.Damage = finalATK;
            result.Winner = attacker;
            result.Message = $"{attackingCard.Name} attacks directly! {finalATK} damage!";

            // DNA: Check victory condition
            _gameState.CheckVictoryCondition();

            return result;
        }

        // Play a CHARACTER card to field
        public bool PlayCharacterCard(Card card, Player player)
        {
            if (!player.Hand.Contains(card))
                return false;

            if (card.CardType != "CHARACTER")
                return false;

            player.PlayCardToField(card);
            return true;
        }

        // DNA: Activate DISCOVERY card (instant effect)
        public bool ActivateDiscoveryCard(Card card, Player player)
        {
            if (!player.Hand.Contains(card))
                return false;

            if (card.CardType != "DISCOVERY")
                return false;

            // Execute effect (we'll implement specific effects next)
            // For now, just send to graveyard
            player.Hand.Remove(card);
            player.SendToGraveyard(card);

            return true;
        }

        // DNA: Set PARADOX card face-down
        public bool SetParadoxCard(Card card, Player player)
        {
            if (!player.Hand.Contains(card))
                return false;

            if (card.CardType != "PARADOX")
                return false;

            player.Hand.Remove(card);
            card.IsFaceDown = true;
            card.Location = "FIELD";
            player.Field.Add(card);

            return true;
        }

        // Get current game state
        public GameState GetGameState()
        {
            return _gameState;
        }

        // DNA: End turn and switch to next player
        public void EndTurn()
        {
            _gameState.SwitchTurn();
        }

        // DNA: Get all graveyard effects currently active
        public List<string> GetActiveGraveyardEffects(Player player)
        {
            List<string> effects = new List<string>();

            // DNA: Buddha effect - while in graveyard, SPIRITUALITY cards get +1000 ATK
            if (player.Graveyard.Any(c => c.Name == "Buddha"))
            {
                effects.Add("Buddha: All SPIRITUALITY cards gain +1000 ATK");
            }

            // DNA: Nelson Mandela - while in graveyard, HUMANISM cards can't be targeted
            if (player.Graveyard.Any(c => c.Name == "Nelson Mandela"))
            {
                effects.Add("Nelson Mandela: HUMANISM cards cannot be targeted");
            }

            return effects;
        }

        // DNA: Apply graveyard effects to field cards
        // DNA: Apply graveyard effects to field cards
        public void ApplyGraveyardEffects(Player player)
        {
            // Buddha effect - All SPIRITUALITY cards gain +1000 ATK
            if (player.Graveyard.Any(c => c.Name == "Buddha"))
            {
                var spiritualityCards = player.GetFieldCards("SPIRITUALITY");
                foreach (var card in spiritualityCards)
                {
                    // DNA: Permanent buff while Buddha is in graveyard
                    if (!card.SpecialEffect.Contains("[Buddha Buff Applied]"))
                    {
                        card.CurrentATK = card.ATK + 1000;
                        card.SpecialEffect += " [Buddha Buff Applied]";
                    }
                }
            }

            // Nelson Mandela effect - HUMANISM cards cannot be targeted
            // (This is handled in target selection - see next step)

            // Dante effect - Can activate from graveyard
            // (This is a special manual activation - handled separately)
        }

        // DNA: Remove graveyard buffs when card leaves field
        public void RemoveGraveyardEffects(Card card)
        {
            if (card.SpecialEffect.Contains("[Buddha Buff Applied]"))
            {
                card.CurrentATK = card.ATK; // Reset to base
                card.SpecialEffect = card.SpecialEffect.Replace(" [Buddha Buff Applied]", "");
            }
        }
    }

    // DNA: Result of a turn action
    public class TurnResult
    {
        public Card DrawnCard { get; set; }
        public string Message { get; set; }
    }

    // DNA: Result of a battle
    public class BattleResult
    {
        public Player Winner { get; set; }
        public int Damage { get; set; }
        public Card DestroyedCard { get; set; }
        public int FinalATK { get; set; }
        public int FinalDEF { get; set; }
        public string Message { get; set; }
    }
}