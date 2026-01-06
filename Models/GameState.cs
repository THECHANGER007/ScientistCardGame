using System.Collections.Generic;
using System.Linq;

namespace ScientistCardGame.Models
{
    public class GameState
    {
        // The two players
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }

        // Turn management
        public int CurrentTurn { get; set; }  // Turn number (1, 2, 3...)
        public Player CurrentPlayer { get; set; }  // Whose turn is it?
        public Player OpponentPlayer { get; set; }  // The other player

        // Game phase (from DNA: Draw, Main, Battle, End)
        public string CurrentPhase { get; set; }  // "DRAW", "MAIN", "BATTLE", "END"

        // Game status
        public bool IsGameOver { get; set; }
        public Player Winner { get; set; }

        // Constructor - starts a new game
        public GameState(string player1Name, string player2Name)
        {
            // Create both players
            Player1 = new Player(1, player1Name);
            Player2 = new Player(2, player2Name);

            // Player 1 goes first
            CurrentTurn = 1;
            CurrentPlayer = Player1;
            OpponentPlayer = Player2;
            Player1.IsMyTurn = true;
            Player2.IsMyTurn = false;

            // Start at draw phase
            CurrentPhase = "DRAW";

            // Game is not over
            IsGameOver = false;
            Winner = null;
        }

        // DNA: Switch turns between players
        public void SwitchTurn()
        {
            // Swap current and opponent
            Player temp = CurrentPlayer;
            CurrentPlayer = OpponentPlayer;
            OpponentPlayer = temp;

            // Update turn flags
            CurrentPlayer.IsMyTurn = true;
            OpponentPlayer.IsMyTurn = false;

            // DNA NEW: Reset summon counter for new turn
            CurrentPlayer.SummonsThisTurn = 0;  // ← ADD THIS

            // DNA: Reset effect usage for all cards
            foreach (var card in CurrentPlayer.Field)
                card.ResetTurnEffects();
            foreach (var card in OpponentPlayer.Field)
                card.ResetTurnEffects();
            // DNA: Reset effect usage for all cards
            foreach (var card in CurrentPlayer.Field)
            {
                card.ResetTurnEffects();
                card.HasAttackedThisTurn = false;
            }

            CurrentTurn++;
            CurrentPhase = "DRAW";
        }

        // DNA: Move to next phase in turn
        public void NextPhase()
        {
            switch (CurrentPhase)
            {
                case "DRAW":
                    CurrentPhase = "MAIN";
                    break;
                case "MAIN":
                    CurrentPhase = "BATTLE";
                    break;
                case "BATTLE":
                    CurrentPhase = "END";
                    break;
                case "END":
                    // End phase complete, switch turns
                    SwitchTurn();
                    break;
            }
        }

        // DNA: Check for victory condition (0 HP = lose)
        public void CheckVictoryCondition()
        {
            if (Player1.HasLost())
            {
                IsGameOver = true;
                Winner = Player2;
            }
            else if (Player2.HasLost())
            {
                IsGameOver = true;
                Winner = Player1;
            }
        }

        // DNA: Draw phase - draw 1 card
        public Card ExecuteDrawPhase()
        {
            Card drawnCard = CurrentPlayer.DrawCard();
            return drawnCard;
        }

        // Get all cards on field (both players)
        public List<Card> GetAllFieldCards()
        {
            List<Card> allCards = new List<Card>();
            allCards.AddRange(Player1.Field);
            allCards.AddRange(Player2.Field);
            return allCards;
        }

        // Find a specific card by ID
        public Card FindCardById(int cardId)
        {
            // Search in all locations for both players
            var allCards = new List<Card>();
            allCards.AddRange(Player1.Deck);
            allCards.AddRange(Player1.Hand);
            allCards.AddRange(Player1.Field);
            allCards.AddRange(Player1.Graveyard);
            allCards.AddRange(Player2.Deck);
            allCards.AddRange(Player2.Hand);
            allCards.AddRange(Player2.Field);
            allCards.AddRange(Player2.Graveyard);

            return allCards.FirstOrDefault(c => c.Id == cardId);
        }

        // Get player by ID
        public Player GetPlayerById(int playerId)
        {
            if (playerId == 1)
                return Player1;
            else if (playerId == 2)
                return Player2;
            else
                return null;
        }
    }
}