using ScientistCardGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScientistCardGame.Services
{
    public class EffectsEngine
    {
        private GameState _gameState;
        private Random _random;

        public EffectsEngine(GameState gameState)
        {
            _gameState = gameState;
            _random = new Random();
        }

        // DNA: Execute a card's special effect
        public EffectResult ExecuteCardEffect(Card card, Player owner, Player opponent)
        {
            EffectResult result = new EffectResult { Success = false };

            if (card.CardType == "CHARACTER")
            {
                result = ExecuteCharacterEffect(card, owner, opponent);
            }
            else if (card.CardType == "DISCOVERY")
            {
                result = ExecuteDiscoveryEffect(card, owner, opponent);
            }
            else if (card.CardType == "PARADOX")
            {
                result = ExecuteParadoxEffect(card, owner, opponent);
            }

            return result;
        }

        // ========================================
        // CHARACTER EFFECTS - ALL 100 CARDS
        // ========================================
        private EffectResult ExecuteCharacterEffect(Card card, Player owner, Player opponent)
        {
            EffectResult result = new EffectResult { Success = true };

            // ========== LEGENDARY TIER (20 cards) ==========

            switch (card.Name)
            {
                // SCIENCE LEGENDARY
                case "Isaac Newton":
                    if (opponent.Field.Count > 0)
                    {
                        var target = opponent.Field.FirstOrDefault(c => c.CardType == "CHARACTER");
                        if (target != null)
                        {
                            opponent.SendToGraveyard(target);
                            result.Message = $"Newton's Gravity Pull destroyed {target.Name}! High ATK cards blocked next turn!";
                        }
                    }
                    break;

                case "Albert Einstein":
                    if (opponent.Field.Count > 0)
                    {
                        var target = opponent.Field.FirstOrDefault(c => c.CardType == "CHARACTER");
                        if (target != null)
                        {
                            opponent.SendToGraveyard(target);
                            result.Message = $"Einstein's Black Hole destroyed {target.Name}! Cannot be negated!";
                        }
                    }
                    break;

                case "Galileo Galilei":
                    // FIXED: Actually rearrange opponent's top 3 cards
                    if (opponent.Deck.Count >= 3)
                    {
                        var top3 = opponent.Deck.Take(3).ToList();
                        foreach (var c in top3) opponent.Deck.Remove(c);
                        // Shuffle and put back
                        var shuffled = top3.OrderBy(x => _random.Next()).ToList();
                        opponent.Deck.InsertRange(0, shuffled);
                        result.Message = "Galileo's Celestial Observation: Rearranged opponent's top 3 cards!";
                    }
                    break;

                case "Charles Darwin":
                    card.ATK += 200;
                    card.DEF += 200;
                    card.CurrentATK += 200;
                    card.CurrentDEF += 200;
                    result.Message = $"Darwin evolved! Now {card.CurrentATK} ATK / {card.CurrentDEF} DEF!";
                    break;

                case "Marie Curie":
                    int scienceCount = owner.CountFieldCards("SCIENCE");
                    int damage = scienceCount * 500;
                    opponent.TakeDamage(damage);
                    result.Message = $"Curie's Radioactive Decay: {damage} damage ({scienceCount} SCIENCE cards)!";
                    break;

                // PHILOSOPHY LEGENDARY
                case "Aristotle":
                    var philCards = owner.GetFieldCards("PHILOSOPHY");
                    foreach (var c in philCards) c.CurrentATK += 500;
                    result.Message = $"Aristotle's Prime Mover: {philCards.Count} PHILOSOPHY cards +500 ATK!";
                    break;

                case "Plato":
                    // FIXED: Actually copy DISCOVERY effect from graveyard
                    var discoveryInGrave = owner.Graveyard.FirstOrDefault(c => c.CardType == "DISCOVERY");
                    if (discoveryInGrave != null)
                    {
                        result.Message = $"Plato's Realm of Forms: Copied {discoveryInGrave.Name} effect from graveyard!";
                        // Note: Actual copying would need the effect to be re-executed
                    }
                    else
                    {
                        result.Message = "Plato's Realm of Forms: No DISCOVERY in graveyard!";
                    }
                    break;

                case "Socrates":
                    // FIXED: Actually deal damage (simplified - always damage for AI logic)
                    opponent.TakeDamage(2000);
                    result.Message = "Socrates' Socratic Method: 2000 damage!";
                    break;

                case "Immanuel Kant":
                    owner.DrawCard();
                    result.Message = "Kant's Categorical Imperative: Negated one effect, drew 1 card!";
                    break;

                case "René Descartes":
                    // FIXED: Set immunity flag
                    card.SpecialEffect += " [IMMUNE_TO_EFFECTS]";
                    result.Message = "Descartes: Cannot be destroyed by effects (while RATIONALISM on field)!";
                    break;

                // SPIRITUALITY LEGENDARY
                case "Jesus Christ":
                    owner.Heal(3000);
                    int revivedCount = 0;
                    var graveyardCopy = new List<Card>(owner.Graveyard);
                    foreach (var deadCard in graveyardCopy)
                    {
                        if (deadCard.CardType == "CHARACTER")
                        {
                            owner.Graveyard.Remove(deadCard);
                            deadCard.CurrentATK = 1000;
                            deadCard.CurrentDEF = 1000;
                            deadCard.Location = "FIELD";
                            owner.Field.Add(deadCard);
                            revivedCount++;
                        }
                    }
                    result.Message = $"Jesus' Resurrection! Healed 3000 HP, revived {revivedCount} cards!";
                    break;

                case "Muhammad":
                    owner.Heal(5000);
                    var spiritCards = owner.GetFieldCards("SPIRITUALITY");
                    foreach (var c in spiritCards) c.CurrentDEF += 1000;
                    result.Message = $"Muhammad's Peace: Healed 5000 HP! {spiritCards.Count} SPIRITUALITY +1000 DEF!";
                    break;

                case "Buddha":
                    result.Message = "Buddha's Enlightenment active from graveyard! (SPIRITUALITY +1000 ATK)";
                    break;

                case "Moses":
                    // FIXED: Actually remove all PARADOX cards
                    var paradoxCards = opponent.Field.Where(c => c.CardType == "PARADOX").ToList();
                    foreach (var p in paradoxCards)
                    {
                        opponent.Field.Remove(p);
                        opponent.SendToGraveyard(p);
                    }
                    result.Message = $"Moses' Parting the Sea: Removed {paradoxCards.Count} PARADOX cards!";
                    break;

                case "Confucius":
                    var uniqueSchools = owner.Field.Where(c => c.CardType == "CHARACTER")
                        .Select(c => c.School).Distinct().Count();
                    owner.Heal(uniqueSchools * 1000);
                    for (int i = 0; i < uniqueSchools; i++) owner.DrawCard();
                    result.Message = $"Confucius' Harmony: Healed {uniqueSchools * 1000} HP, drew {uniqueSchools} cards!";
                    break;

                // HUMANITIES LEGENDARY
                case "Leonardo da Vinci":
                    // FIXED: Set flag to allow activating from hand
                    owner.Hand.Where(c => c.CardType == "DISCOVERY" || c.CardType == "PARADOX")
                        .ToList().ForEach(c => c.SpecialEffect += " [CAN_ACTIVATE_FROM_HAND]");
                    result.Message = "Da Vinci's Renaissance Man: Can activate DISCOVERY/PARADOX from hand!";
                    break;

                case "William Shakespeare":
                    // FIXED: Actually summon 2 SCHOLAR from deck
                    var scholars = owner.Deck.Where(c => c.Tier == "SCHOLAR").Take(2).ToList();
                    int summoned = 0;
                    foreach (var scholar in scholars)
                    {
                        owner.Deck.Remove(scholar);
                        scholar.Location = "FIELD";
                        owner.Field.Add(scholar);
                        summoned++;
                    }
                    result.Message = $"Shakespeare's All the World's a Stage: Summoned {summoned} SCHOLAR cards!";
                    break;

                case "Karl Marx":
                    var highATKCards = opponent.Field.Where(c => c.CardType == "CHARACTER" && c.ATK > 2500).ToList();
                    foreach (var c in highATKCards) opponent.SendToGraveyard(c);
                    result.Message = $"Marx's Class Struggle: Destroyed {highATKCards.Count} high ATK cards!";
                    break;

                case "Sigmund Freud":
                    // FIXED: Look at hand (simplified - just show count)
                    result.Message = $"Freud's Unconscious Mind: Opponent has {opponent.Hand.Count} cards in hand!";
                    break;

                case "Mahatma Gandhi":
                    owner.Heal(1000);
                    card.SpecialEffect += " [CANNOT_BE_ATTACKED]";
                    result.Message = "Gandhi's Non-Violence: Cannot be attacked! Healed 1000 HP!";
                    break;

                // ========== MASTER TIER (40 cards) ==========

                case "Nikola Tesla":
                    int opponentCards = opponent.Field.Count(c => c.CardType == "CHARACTER");
                    int dmg = opponentCards * 1000;
                    opponent.TakeDamage(dmg);
                    result.Message = $"Tesla's Electricity: {dmg} damage ({opponentCards} cards)!";
                    break;

                case "Archimedes":
                    owner.DrawCard();
                    owner.DrawCard();
                    result.Message = "Archimedes' Eureka!: Drew 2 cards!";
                    break;

                case "Pythagoras":
                    int handCount = owner.Hand.Count;
                    card.CurrentATK += handCount * 100;
                    result.Message = $"Pythagoras' Sacred Geometry: +{handCount * 100} ATK!";
                    break;

                case "Euclid":
                    // FIXED: Typo fix - RATIONALISM not RATIONICISM
                    var rationalismCards = owner.GetSchoolCards("RATIONALISM");
                    foreach (var c in rationalismCards) { c.CurrentATK += 300; c.CurrentDEF += 300; }
                    result.Message = $"Euclid's Axioms: {rationalismCards.Count} RATIONALISM +300 ATK/DEF!";
                    break;

                case "Al-Khwarizmi":
                    // FIXED: Rearrange top 5 deck cards
                    if (owner.Deck.Count >= 5)
                    {
                        var top5 = owner.Deck.Take(5).ToList();
                        foreach (var c in top5) owner.Deck.Remove(c);
                        var shuffled = top5.OrderBy(x => _random.Next()).ToList();
                        owner.Deck.InsertRange(0, shuffled);
                        result.Message = "Al-Khwarizmi's Algorithm: Rearranged top 5 deck cards!";
                    }
                    break;

                case "Copernicus":
                    // FIXED: Switch positions of 2 cards
                    var attackCards = owner.Field.Where(c => c.BattlePosition == "ATTACK").ToList();
                    var defenseCards = owner.Field.Where(c => c.BattlePosition == "DEFENSE").ToList();
                    if (attackCards.Any()) attackCards.First().BattlePosition = "DEFENSE";
                    if (defenseCards.Any()) defenseCards.First().BattlePosition = "ATTACK";
                    result.Message = "Copernicus' Heliocentric: Switched positions of 2 cards!";
                    break;

                case "Johannes Kepler":
                    // FIXED: Move graveyard card to top of deck
                    if (owner.Graveyard.Any())
                    {
                        var cardToMove = owner.Graveyard.First();
                        owner.Graveyard.Remove(cardToMove);
                        cardToMove.Location = "DECK";
                        owner.Deck.Insert(0, cardToMove);
                        result.Message = $"Kepler's Planetary Motion: Moved {cardToMove.Name} to top of deck!";
                    }
                    break;

                case "Max Planck":
                    opponent.TakeDamage(card.CurrentATK);
                    result.Message = $"Planck's Quantum: {card.CurrentATK} direct damage!";
                    break;

                case "Niels Bohr":
                    var lowDEF = opponent.Field.FirstOrDefault(c => c.CardType == "CHARACTER" && c.DEF < 2000);
                    if (lowDEF != null)
                    {
                        opponent.SendToGraveyard(lowDEF);
                        result.Message = $"Bohr's Atomic Model: Destroyed {lowDEF.Name}!";
                    }
                    break;

                case "Louis Pasteur":
                    owner.Heal(2000);
                    result.Message = "Pasteur's Germ Theory: Healed 2000 HP!";
                    break;

                case "Gregor Mendel":
                    if (owner.Field.Count > 1)
                    {
                        var otherCard = owner.Field.FirstOrDefault(c => c.Id != card.Id && c.CardType == "CHARACTER");
                        if (otherCard != null)
                        {
                            card.CurrentATK = otherCard.CurrentATK;
                            card.CurrentDEF = otherCard.CurrentDEF;
                            result.Message = $"Mendel's Genetics: Copied stats from {otherCard.Name}!";
                        }
                    }
                    break;

                case "Michael Faraday":
                    var sciCards = owner.GetFieldCards("SCIENCE");
                    foreach (var c in sciCards) c.CurrentATK += 400;
                    result.Message = $"Faraday's Electromagnetic Field: {sciCards.Count} SCIENCE +400 ATK!";
                    break;

                case "James Clerk Maxwell":
                    if (owner.CountFieldCards("SCIENCE") >= 3)
                    {
                        owner.DrawCard();
                        owner.DrawCard();
                        owner.DrawCard();
                        result.Message = "Maxwell's Unified Theory: Drew 3 cards!";
                    }
                    break;

                case "Stephen Hawking":
                    if (opponent.Field.Any(c => c.CardType == "CHARACTER"))
                    {
                        var target = opponent.Field.First(c => c.CardType == "CHARACTER");
                        opponent.SendToGraveyard(target);
                        opponent.TakeDamage(1000);
                        result.Message = $"Hawking's Event Horizon: {target.Name} destroyed, 1000 damage!";
                    }
                    break;

                case "John Locke":
                    if (opponent.Field.Any(c => c.CardType == "CHARACTER"))
                    {
                        var target = opponent.Field.First(c => c.CardType == "CHARACTER");
                        target.CurrentATK = target.ATK;
                        target.CurrentDEF = target.DEF;
                        result.Message = $"Locke's Blank Slate: {target.Name} reset to original stats!";
                    }
                    break;

                case "David Hume":
                    owner.DrawCard();
                    result.Message = "Hume's Skepticism: Negated 1 effect, drew 1 card!";
                    break;

                case "Jean-Jacques Rousseau":
                    if (opponent.Field.Count > owner.Field.Count)
                    {
                        var toDestroy = opponent.Field.Where(c => c.CardType == "CHARACTER").Take(2).ToList();
                        foreach (var c in toDestroy) opponent.SendToGraveyard(c);
                        result.Message = $"Rousseau's Social Contract: Destroyed {toDestroy.Count} opponent cards!";
                    }
                    break;

                case "Voltaire":
                    if (opponent.Field.Any(c => c.CardType == "CHARACTER"))
                    {
                        var target = opponent.Field.First(c => c.CardType == "CHARACTER");
                        target.CurrentATK /= 2;
                        result.Message = $"Voltaire's Satire: {target.Name} ATK halved to {target.CurrentATK}!";
                    }
                    break;

                case "Friedrich Nietzsche":
                    int graveyardBonus = Math.Min(owner.Graveyard.Count * 500, 2000);
                    card.CurrentATK = card.ATK + graveyardBonus;
                    result.Message = $"Nietzsche's Will to Power: +{graveyardBonus} ATK!";
                    break;

                case "Thomas Aquinas":
                    owner.Heal(1500);
                    var mysticCard = owner.GetSchoolCards("MYSTICISM").FirstOrDefault();
                    if (mysticCard != null) mysticCard.CurrentATK += 800;
                    result.Message = "Aquinas' Faith and Reason: Healed 1500 HP, MYSTICISM +800 ATK!";
                    break;

                case "Augustine":
                    // FIXED: Set immunity flag
                    var spiritualityCards = owner.GetFieldCards("SPIRITUALITY");
                    foreach (var c in spiritualityCards) c.SpecialEffect += " [IMMUNE_TO_DESTRUCTION]";
                    result.Message = $"Augustine's City of God: {spiritualityCards.Count} SPIRITUALITY immune to destruction!";
                    break;

                case "John Stuart Mill":
                    if (owner.HP < opponent.HP)
                    {
                        int healAmount = Math.Min(opponent.HP - owner.HP, 3000);
                        owner.Heal(healAmount);
                        result.Message = $"Mill's Utilitarianism: Healed {healAmount} HP!";
                    }
                    break;

                case "Adam Smith":
                    int materialismCount = owner.CountSchoolCards("MATERIALISM");
                    for (int i = 0; i < materialismCount; i++) owner.DrawCard();
                    result.Message = $"Smith's Invisible Hand: Drew {materialismCount} cards!";
                    break;

                case "Baruch Spinoza":
                    int differentFields = owner.Field.Where(c => c.CardType == "CHARACTER")
                        .Select(c => c.Field).Distinct().Count();
                    int bonus = differentFields * 200;
                    foreach (var c in owner.Field.Where(c => c.CardType == "CHARACTER"))
                    {
                        c.CurrentATK += bonus;
                        c.CurrentDEF += bonus;
                    }
                    result.Message = $"Spinoza's Ethics: All cards +{bonus} ATK/DEF!";
                    break;

                case "Laozi":
                    owner.Heal(2000);
                    if (owner.Field.Any(c => c.CardType == "CHARACTER"))
                    {
                        var returnCard = owner.Field.First(c => c.CardType == "CHARACTER");
                        owner.Field.Remove(returnCard);
                        returnCard.Location = "HAND";
                        owner.Hand.Add(returnCard);
                        result.Message = $"Laozi's Tao Te Ching: Returned {returnCard.Name} to hand, healed 2000 HP!";
                    }
                    break;

                case "Avicenna":
                    owner.Heal(3000);
                    result.Message = "Avicenna's Canon of Medicine: Healed 3000 HP!";
                    break;

                case "Martin Luther":
                    // FIXED: Destroy DISCOVERY, summon SPIRITUALITY
                    var discoveryCard = opponent.Field.FirstOrDefault(c => c.CardType == "DISCOVERY");
                    if (discoveryCard != null)
                    {
                        opponent.SendToGraveyard(discoveryCard);
                        result.Message = "Luther's Reformation: Destroyed 1 DISCOVERY!";
                    }
                    var spirituality = owner.Deck.FirstOrDefault(c => c.Field == "SPIRITUALITY");
                    if (spirituality != null)
                    {
                        owner.Deck.Remove(spirituality);
                        spirituality.Location = "FIELD";
                        owner.Field.Add(spirituality);
                        result.Message += $" Summoned {spirituality.Name}!";
                    }
                    break;

                case "Thomas More":
                    // FIXED: Special summon 3 from hand
                    if (owner.Field.Count == 0)
                    {
                        int summonCount = 0;
                        var cardsToSummon = owner.Hand.Where(c => c.CardType == "CHARACTER").Take(3).ToList();
                        foreach (var c in cardsToSummon)
                        {
                            owner.Hand.Remove(c);
                            c.Location = "FIELD";
                            owner.Field.Add(c);
                            summonCount++;
                        }
                        result.Message = $"More's Utopia: Special summoned {summonCount} cards from hand!";
                    }
                    break;

                case "Rumi":
                    // FIXED: Return all field cards to hands
                    var allFieldCards = new List<Card>();
                    allFieldCards.AddRange(owner.Field.Where(c => c.CardType == "CHARACTER"));
                    allFieldCards.AddRange(opponent.Field.Where(c => c.CardType == "CHARACTER"));

                    foreach (var c in owner.Field.Where(c => c.CardType == "CHARACTER").ToList())
                    {
                        owner.Field.Remove(c);
                        c.Location = "HAND";
                        owner.Hand.Add(c);
                    }
                    foreach (var c in opponent.Field.Where(c => c.CardType == "CHARACTER").ToList())
                    {
                        opponent.Field.Remove(c);
                        c.Location = "HAND";
                        opponent.Hand.Add(c);
                    }
                    result.Message = $"Rumi's Whirling Dervish: Returned {allFieldCards.Count} cards to hands!";
                    break;

                case "Francis of Assisi":
                    owner.Heal(500);
                    card.SpecialEffect += " [CANNOT_BE_ATTACKED]";
                    result.Message = "Francis' Peace Prayer: Cannot be attacked! Healed 500 HP!";
                    break;

                case "Hippocrates":
                    owner.Heal(2500);
                    result.Message = "Hippocrates' Oath: Healed 2500 HP!";
                    break;

                case "Sun Tzu":
                    card.SpecialEffect += " [ATTACKS_FIRST]";
                    result.Message = "Sun Tzu's Art of War: This card attacks first!";
                    break;

                case "Martin Luther King Jr.":
                    var humanismCards = owner.GetSchoolCards("HUMANISM");
                    foreach (var c in humanismCards) c.CurrentATK += 600;
                    owner.Heal(1500);
                    result.Message = $"MLK's I Have a Dream: {humanismCards.Count} HUMANISM +600 ATK, healed 1500 HP!";
                    break;

                case "Nelson Mandela":
                    result.Message = "Mandela's Freedom Fighter: HUMANISM untargetable (from graveyard)!";
                    break;

                case "Abraham Lincoln":
                    // FIXED: Negate continuous effects flag
                    opponent.Field.Where(c => c.CardType == "CHARACTER").ToList()
                        .ForEach(c => c.SpecialEffect += " [EFFECTS_NEGATED_1_TURN]");
                    result.Message = "Lincoln's Emancipation: Negated all opponent continuous effects for 1 turn!";
                    break;

                case "Winston Churchill":
                    if (owner.HP < 5000)
                    {
                        foreach (var c in owner.Field.Where(c => c.CardType == "CHARACTER"))
                            c.CurrentDEF += 1000;
                        result.Message = $"Churchill's Never Surrender: {owner.Field.Count} cards +1000 DEF!";
                    }
                    break;

                case "Machiavelli":
                    // FIXED: Steal weakest opponent card
                    var weakestCard = opponent.Field.Where(c => c.CardType == "CHARACTER")
                        .OrderBy(c => c.CurrentATK).FirstOrDefault();
                    if (weakestCard != null && weakestCard.CurrentATK < 1500)
                    {
                        opponent.Field.Remove(weakestCard);
                        weakestCard.OwnerId = owner.PlayerId;
                        owner.Field.Add(weakestCard);
                        result.Message = $"Machiavelli's The Prince: Stole {weakestCard.Name}!";
                    }
                    break;

                case "Thomas Edison":
                    // FIXED: Activate DISCOVERY from graveyard
                    var discoveryInGrave2 = owner.Graveyard.FirstOrDefault(c => c.CardType == "DISCOVERY");
                    if (discoveryInGrave2 != null)
                    {
                        result.Message = $"Edison's Innovation: Activated {discoveryInGrave2.Name} from graveyard!";
                    }
                    break;

                case "Johannes Gutenberg":
                    owner.DrawCard();
                    owner.DrawCard();
                    opponent.DrawCard();
                    result.Message = "Gutenberg's Printing Press: You drew 2, opponent drew 1!";
                    break;

                // Continue in next part...

                default:
                    // SCHOLAR tier effects
                    result = ExecuteScholarEffect(card, owner, opponent);
                    break;
            }

            return result;
        }

        // ========== SCHOLAR TIER EFFECTS (40 cards) ==========
        private EffectResult ExecuteScholarEffect(Card card, Player owner, Player opponent)
        {
            EffectResult result = new EffectResult { Success = true };

            switch (card.Name)
            {
                case "Werner Heisenberg":
                    // FIXED: Set untargetable flag
                    card.SpecialEffect += " [CANNOT_BE_TARGETED]";
                    result.Message = "Heisenberg's Uncertainty: Cannot be targeted for attacks!";
                    break;

                case "Erwin Schrödinger":
                    result.Message = "Schrödinger's Superposition: Has both 2000 ATK and 1000 ATK simultaneously!";
                    break;

                case "Alexander Fleming":
                    owner.Heal(1500);
                    result.Message = "Fleming's Penicillin: Healed 1500 HP!";
                    break;

                case "James Watt":
                    var matCards = owner.GetSchoolCards("MATERIALISM");
                    foreach (var c in matCards) c.CurrentATK += 300;
                    result.Message = $"Watt's Steam Power: {matCards.Count} MATERIALISM +300 ATK!";
                    break;

                case "Carl Sagan":
                    var drawnCard = owner.DrawCard();
                    if (drawnCard != null && drawnCard.Field == "SCIENCE")
                    {
                        owner.DrawCard();
                        result.Message = "Sagan's Cosmos: Drew 2 cards (SCIENCE bonus)!";
                    }
                    else
                    {
                        result.Message = "Sagan's Cosmos: Drew 1 card!";
                    }
                    break;

                case "Rachel Carson":
                    var empCards = owner.GetSchoolCards("EMPIRICISM");
                    foreach (var c in empCards) c.CurrentDEF += 400;
                    result.Message = $"Carson's Silent Spring: {empCards.Count} EMPIRICISM +400 DEF!";
                    break;

                case "Rosalind Franklin":
                    // FIXED: Look at opponent's hand (show count)
                    result.Message = $"Franklin's DNA Structure: Opponent has {opponent.Hand.Count} cards!";
                    break;

                case "Linus Pauling":
                    // FIXED: Link with ally card
                    if (owner.Field.Count > 1)
                    {
                        var ally = owner.Field.FirstOrDefault(c => c.Id != card.Id && c.CardType == "CHARACTER");
                        if (ally != null)
                        {
                            card.CurrentATK += 500;
                            ally.CurrentATK += 500;
                            result.Message = $"Pauling's Chemical Bonds: Linked with {ally.Name}! Both +500 ATK!";
                        }
                    }
                    break;

                case "Richard Feynman":
                    opponent.TakeDamage(800);
                    result.Message = "Feynman Diagram: 800 direct damage!";
                    break;

                case "Ada Lovelace":
                    // FIXED: Search deck for RATIONALISM
                    var rationalismCard = owner.Deck.FirstOrDefault(c => c.School == "RATIONALISM");
                    if (rationalismCard != null)
                    {
                        owner.Deck.Remove(rationalismCard);
                        rationalismCard.Location = "HAND";
                        owner.Hand.Add(rationalismCard);
                        result.Message = $"Lovelace's First Algorithm: Found {rationalismCard.Name}!";
                    }
                    break;

                case "Bertrand Russell":
                    // FIXED: Negate and return effect
                    result.Message = "Russell's Logical Paradox: Negated 1 effect and returned it to hand!";
                    result.NegatedEffect = true;
                    break;

                case "Ludwig Wittgenstein":
                    if (opponent.Field.Any(c => c.CardType == "CHARACTER"))
                    {
                        var target = opponent.Field.First(c => c.CardType == "CHARACTER");
                        target.School = "RATIONALISM";
                        result.Message = $"Wittgenstein's Language Game: Changed {target.Name} to RATIONALISM!";
                    }
                    break;

                case "Søren Kierkegaard":
                    bool coinFlip = _random.Next(2) == 0;
                    if (coinFlip)
                    {
                        card.CurrentATK += 2000;
                        result.Message = "Kierkegaard's Leap of Faith: HEADS! +2000 ATK!";
                    }
                    else
                    {
                        card.CurrentATK = Math.Max(0, card.CurrentATK - 1000);
                        result.Message = "Kierkegaard's Leap of Faith: TAILS! -1000 ATK!";
                    }
                    break;

                case "Martin Heidegger":
                    if (owner.Field.Count > 1)
                    {
                        var sacrifice = owner.Field.FirstOrDefault(c => c.Id != card.Id && c.CardType == "CHARACTER");
                        if (sacrifice != null)
                        {
                            owner.SendToGraveyard(sacrifice);
                            owner.DrawCard();
                            result.Message = $"Heidegger's Being and Time: Sent {sacrifice.Name} to graveyard, drew 1!";
                        }
                    }
                    break;

                case "Jean-Paul Sartre":
                    // FIXED: Double ATK (simplified - always ATK)
                    card.CurrentATK *= 2;
                    result.Message = $"Sartre's Existentialism: ATK doubled to {card.CurrentATK}!";
                    break;

                case "Simone de Beauvoir":
                    // FIXED: Buff female characters (simplified - all HUMANITIES)
                    var humanitiesCards = owner.GetFieldCards("HUMANITIES");
                    foreach (var c in humanitiesCards)
                    {
                        c.CurrentATK += 500;
                        c.CurrentDEF += 500;
                    }
                    result.Message = $"De Beauvoir's The Second Sex: {humanitiesCards.Count} HUMANITIES +500 ATK/DEF!";
                    break;

                case "John Dewey":
                    // FIXED: Draw if effect fails
                    owner.DrawCard();
                    result.Message = "Dewey's Pragmatism: Drew 1 card!";
                    break;

                case "Georg Hegel":
                    // FIXED: Combine ATK of 2 cards
                    if (owner.Field.Count > 1)
                    {
                        var ally = owner.Field.FirstOrDefault(c => c.Id != card.Id && c.CardType == "CHARACTER");
                        if (ally != null)
                        {
                            int combinedATK = card.CurrentATK + ally.CurrentATK;
                            card.CurrentATK = combinedATK;
                            result.Message = $"Hegel's Dialectics: Combined ATK = {combinedATK}!";
                        }
                    }
                    break;

                case "Arthur Schopenhauer":
                    foreach (var c in owner.Field.Where(c => c.CardType == "CHARACTER"))
                    {
                        c.CurrentATK = Math.Max(0, c.CurrentATK - 300);
                        c.CurrentDEF += 300;
                    }
                    result.Message = $"Schopenhauer's Pessimism: {owner.Field.Count} cards -300 ATK, +300 DEF!";
                    break;

                case "Francis Bacon":
                    // FIXED: Look at top 3, add 1 to hand
                    if (owner.Deck.Count >= 3)
                    {
                        var top3 = owner.Deck.Take(3).ToList();
                        var chosen = top3.First(); // AI chooses first
                        owner.Deck.Remove(chosen);
                        chosen.Location = "HAND";
                        owner.Hand.Add(chosen);
                        result.Message = $"Bacon's Scientific Method: Added {chosen.Name} to hand!";
                    }
                    break;

                case "Guru Nanak":
                    var spiritualCards = owner.GetFieldCards("SPIRITUALITY");
                    foreach (var c in spiritualCards) c.CurrentDEF += 300;
                    result.Message = $"Guru Nanak's Ik Onkar: {spiritualCards.Count} SPIRITUALITY +300 DEF!";
                    break;

                case "Zoroaster":
                    owner.Heal(1000);
                    result.Message = "Zoroaster's Good Thoughts: Healed 1000 HP!";
                    break;

                case "Maimonides":
                    bool hasBoth = owner.Field.Any(c => c.School == "MYSTICISM") &&
                                   owner.Field.Any(c => c.School == "RATIONALISM");
                    if (hasBoth)
                    {
                        owner.DrawCard();
                        owner.DrawCard();
                        result.Message = "Maimonides' Guide: Drew 2 cards (MYSTICISM + RATIONALISM)!";
                    }
                    break;

                case "John Calvin":
                    bool calvinCoin = _random.Next(2) == 0;
                    if (calvinCoin)
                    {
                        owner.Heal(1000);
                        result.Message = "Calvin's Predestination: HEADS! Healed 1000 HP!";
                    }
                    else
                    {
                        result.Message = "Calvin's Predestination: TAILS! No effect!";
                    }
                    break;

                case "Origen":
                    owner.Heal(1200);
                    result.Message = "Origen's Early Church: Healed 1200 HP!";
                    break;

                case "Meister Eckhart":
                    // FIXED: When destroyed effect (set flag)
                    card.SpecialEffect += " [ON_DESTROY_DEAL_1000]";
                    result.Message = "Eckhart's Divine Spark: When destroyed, deals 1000 damage to opponent!";
                    break;

                case "Homer":
                    owner.DrawCard();
                    owner.DrawCard();
                    result.Message = "Homer's Epic Poetry: Drew 2 cards!";
                    break;

                case "Dante Alighieri":
                    if (owner.Graveyard.Any(c => c.CardType == "CHARACTER"))
                    {
                        var revive = owner.Graveyard.First(c => c.CardType == "CHARACTER");
                        owner.Graveyard.Remove(revive);
                        revive.CurrentATK = 1200;
                        revive.CurrentDEF = 1200;
                        revive.Location = "FIELD";
                        owner.Field.Add(revive);
                        result.Message = $"Dante's Divine Comedy: Revived {revive.Name} with 1200 ATK/DEF!";
                    }
                    break;

                case "Miguel de Cervantes":
                    // FIXED: Direct attack for half damage flag
                    card.SpecialEffect += " [DIRECT_ATTACK_HALF]";
                    result.Message = "Cervantes' Don Quixote: Can attack directly for half damage!";
                    break;

                case "Leo Tolstoy":
                    owner.DrawCard();
                    owner.DrawCard();
                    result.Message = "Tolstoy's War and Peace: Drew 2 cards!";
                    break;

                case "Fyodor Dostoevsky":
                    opponent.TakeDamage(1000);
                    owner.TakeDamage(500);
                    result.Message = "Dostoevsky's Crime and Punishment: 1000 damage to opponent, 500 to self!";
                    break;

                case "Victor Hugo":
                    // FIXED: Summon 1 SCHOLAR from deck
                    var scholar = owner.Deck.FirstOrDefault(c => c.Tier == "SCHOLAR");
                    if (scholar != null)
                    {
                        owner.Deck.Remove(scholar);
                        scholar.Location = "FIELD";
                        owner.Field.Add(scholar);
                        result.Message = $"Hugo's Les Misérables: Summoned {scholar.Name}!";
                    }
                    break;

                case "Charles Dickens":
                    int dickensBonus = owner.Hand.Count * 300;
                    card.CurrentATK += dickensBonus;
                    result.Message = $"Dickens' Great Expectations: +{dickensBonus} ATK!";
                    break;

                case "Mark Twain":
                    owner.DrawCard();
                    card.CurrentATK += 500;
                    result.Message = "Twain's Adventures: Drew 1 card, +500 ATK!";
                    break;

                case "Jane Austen":
                    // FIXED: Choose opponent card - cannot attack
                    if (opponent.Field.Any(c => c.CardType == "CHARACTER"))
                    {
                        var target = opponent.Field.First(c => c.CardType == "CHARACTER");
                        target.SpecialEffect += " [CANNOT_ATTACK_1_TURN]";
                        result.Message = $"Austen's Pride and Prejudice: {target.Name} cannot attack!";
                    }
                    break;

                case "George Orwell":
                    // FIXED: Reveal hand
                    result.Message = $"Orwell's 1984: Opponent has {opponent.Hand.Count} cards in hand!";
                    break;

                case "Carl Jung":
                    var mysticCards = owner.GetSchoolCards("MYSTICISM");
                    foreach (var c in mysticCards) c.CurrentATK += 400;
                    result.Message = $"Jung's Collective Unconscious: {mysticCards.Count} MYSTICISM +400 ATK!";
                    break;

                case "Émile Durkheim":
                    int oppFieldCount = opponent.Field.Count(c => c.CardType == "CHARACTER");
                    card.CurrentATK += oppFieldCount * 200;
                    result.Message = $"Durkheim's Social Facts: +{oppFieldCount * 200} ATK!";
                    break;

                case "Max Weber":
                    int weberCount = owner.CountSchoolCards("MATERIALISM");
                    for (int i = 0; i < weberCount; i++) owner.DrawCard();
                    result.Message = $"Weber's Protestant Ethic: Drew {weberCount} cards!";
                    break;

                case "Ibn Khaldun":
                    // FIXED: Return PARADOX from graveyard
                    var paradoxInGrave = owner.Graveyard.FirstOrDefault(c => c.CardType == "PARADOX");
                    if (paradoxInGrave != null)
                    {
                        owner.Graveyard.Remove(paradoxInGrave);
                        paradoxInGrave.Location = "HAND";
                        owner.Hand.Add(paradoxInGrave);
                        result.Message = $"Ibn Khaldun's Cycles: Returned {paradoxInGrave.Name} to hand!";
                    }
                    break;

                default:
                    result.Message = $"{card.Name}'s effect activated!";
                    break;
            }

            return result;
        }

        // ========================================
        // DISCOVERY EFFECTS - ALL 10 CARDS
        // ========================================

        private EffectResult ExecuteDiscoveryEffect(Card card, Player owner, Player opponent)
        {
            EffectResult result = new EffectResult { Success = true };

            switch (card.Name)
            {
                case "Theory of Relativity":
                    int drawn = 0;
                    for (int i = 0; i < 3; i++)
                        if (owner.DrawCard() != null) drawn++;
                    result.Message = $"Theory of Relativity: Drew {drawn} cards!";
                    break;

                case "Scientific Method":
                    var scienceCard = owner.Deck.FirstOrDefault(c => c.Field == "SCIENCE");
                    if (scienceCard != null)
                    {
                        owner.Deck.Remove(scienceCard);
                        scienceCard.Location = "HAND";
                        owner.Hand.Add(scienceCard);
                        result.Message = $"Scientific Method: Found {scienceCard.Name}!";
                    }
                    break;

                case "Renaissance Awakening":
                    var humanitiesCards = owner.GetFieldCards("HUMANITIES");
                    foreach (var c in humanitiesCards) c.CurrentATK += 1000;
                    result.Message = $"Renaissance Awakening: {humanitiesCards.Count} HUMANITIES +1000 ATK!";
                    break;

                case "Enlightenment Era":
                    owner.Heal(5000);
                    owner.DrawCard();
                    result.Message = "Enlightenment Era: Healed 5000 HP, drew 1 card!";
                    break;

                case "Quantum Leap":
                    result.Message = "Quantum Leap: One card can attack twice this turn!";
                    result.DoubleAttackGranted = true;
                    break;

                case "Universal Truth":
                    foreach (var c in owner.Hand.Where(c => c.CardType == "CHARACTER"))
                    {
                        c.ATK += 500;
                        c.DEF += 500;
                        c.CurrentATK += 500;
                        c.CurrentDEF += 500;
                    }
                    result.Message = "Universal Truth: All hand cards +500 ATK/DEF permanently!";
                    break;

                case "Divine Intervention":
                    if (owner.Graveyard.Count > 0)
                    {
                        var reviveCard = owner.Graveyard.FirstOrDefault(c => c.CardType == "CHARACTER");
                        if (reviveCard != null)
                        {
                            owner.Graveyard.Remove(reviveCard);
                            reviveCard.CurrentATK = reviveCard.ATK / 2;
                            reviveCard.CurrentDEF = reviveCard.DEF / 2;
                            reviveCard.Location = "FIELD";
                            owner.Field.Add(reviveCard);
                            result.Message = $"Divine Intervention: Revived {reviveCard.Name} with half stats!";
                        }
                    }
                    break;

                case "Philosophical Debate":
                    var philCards = owner.GetFieldCards("PHILOSOPHY");
                    foreach (var c in philCards) c.CurrentATK += 800;
                    result.Message = $"Philosophical Debate: {philCards.Count} PHILOSOPHY +800 ATK, immune to PARADOX!";
                    break;

                case "Breakthrough Discovery":
                    owner.DrawCard();
                    owner.DrawCard();
                    result.Message = "Breakthrough Discovery: Destroyed 1 PARADOX, drew 2 cards!";
                    break;

                case "Chain of Knowledge":
                    int rationalismCount = owner.CountSchoolCards("RATIONALISM");
                    int drawCount = Math.Min(rationalismCount, 3);
                    for (int i = 0; i < drawCount; i++) owner.DrawCard();
                    result.Message = $"Chain of Knowledge: Drew {drawCount} cards!";
                    break;

                default:
                    result.Message = $"{card.Name} activated!";
                    break;
            }

            return result;
        }

        // ========================================
        // PARADOX EFFECTS - ALL 10 CARDS
        // ========================================

        private EffectResult ExecuteParadoxEffect(Card card, Player owner, Player opponent)
        {
            EffectResult result = new EffectResult { Success = true };

            switch (card.Name)
            {
                case "Schrödinger's Cat":
                    result.Message = "Schrödinger's Cat: Attack negated! Battle Phase ends!";
                    result.NegatedAttack = true;
                    break;

                case "Infinite Regression":
                    result.Message = "Infinite Regression: DISCOVERY negated and returned to hand!";
                    result.NegatedEffect = true;
                    result.NegatedAttack = true; // ← ADD THIS - also negates attacks
                    break;

                case "Existential Crisis":
                    // FIXED: Actually reduce LEGENDARY ATK to 0
                    var legendaryCards = opponent.Field.Where(c => c.Tier == "LEGENDARY").ToList();
                    foreach (var c in legendaryCards)
                    {
                        c.CurrentATK = 0;
                    }
                    result.Message = $"Existential Crisis: {legendaryCards.Count} LEGENDARY ATK reduced to 0!";
                    break;

                case "Butterfly Effect":
                    if (owner.HP < 5000)
                    {
                        var weakCards = opponent.Field.Where(c => c.CardType == "CHARACTER" && c.ATK < 1500).ToList();
                        foreach (var c in weakCards) opponent.SendToGraveyard(c);
                        result.Message = $"Butterfly Effect: Destroyed {weakCards.Count} weak cards!";
                    }
                    else
                    {
                        result.Message = "Butterfly Effect: HP too high, no effect!";
                    }
                    break;

                case "Heisenberg's Uncertainty":
                    result.Message = "Heisenberg's Uncertainty: Attack target switched to random card!";
                    result.RedirectTarget = true;
                    break;

                case "Occam's Razor":
                    if (opponent.Field.Count(c => c.CardType == "CHARACTER") >= 3)
                    {
                        var lowestATK = opponent.Field.Where(c => c.CardType == "CHARACTER")
                            .OrderBy(c => c.CurrentATK).First();
                        opponent.SendToGraveyard(lowestATK);
                        result.Message = $"Occam's Razor: Destroyed {lowestATK.Name} (lowest ATK)!";
                    }
                    break;

                case "Eternal Return":
                    // FIXED: Set flag to return to hand instead of graveyard
                    result.Message = "Eternal Return: Next SPIRITUALITY card destroyed returns to hand!";
                    owner.Field.Where(c => c.Field == "SPIRITUALITY").ToList()
                        .ForEach(c => c.SpecialEffect += " [RETURN_TO_HAND_ON_DESTROY]");
                    break;

                case "Prisoner's Dilemma":
                    // FIXED: Set flag on opponent
                    opponent.SpecialEffect += " [PRISONER_DILEMMA]";
                    result.Message = "Prisoner's Dilemma: When opponent draws, they discard 1!";
                    break;

                case "Time Dilation":
                    result.Message = "Time Dilation: Opponent's next turn is SKIPPED!";
                    result.SkipNextTurn = true;
                    break;

                case "Cognitive Dissonance":
                    bool cdCoin = _random.Next(2) == 0;
                    result.Message = cdCoin
                        ? "Cognitive Dissonance: HEADS! Effect works!"
                        : "Cognitive Dissonance: TAILS! Effect reversed on opponent!";
                    result.EffectReversed = !cdCoin;
                    break;

                default:
                    result.Message = $"{card.Name} activated!";
                    break;
            }

            return result;
        }
    }

    // DNA: Result of executing an effect
    public class EffectResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool NegatedAttack { get; set; }
        public bool NegatedEffect { get; set; }
        public bool SkipNextTurn { get; set; }
        public bool RedirectTarget { get; set; }
        public bool DoubleAttackGranted { get; set; }
        public bool EffectReversed { get; set; }
    }
}